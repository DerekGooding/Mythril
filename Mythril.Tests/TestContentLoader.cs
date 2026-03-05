using Newtonsoft.Json;
using Mythril.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mythril.Tests;

public static class TestContentLoader
{
    private static bool _loaded = false;
    private static readonly object _lock = new object();

    public static void Load()
    {
        if (_loaded) return;

        lock (_lock)
        {
            if (_loaded) return;

            var items = ContentHost.GetContent<Items>();
            var quests = ContentHost.GetContent<Quests>();
            var stats = ContentHost.GetContent<Stats>();
            var abilities = ContentHost.GetContent<CadenceAbilities>();
            var locations = ContentHost.GetContent<Locations>();
            var questDetails = ContentHost.GetContent<QuestDetails>();
            var questUnlocks = ContentHost.GetContent<QuestUnlocks>();
            var cadences = ContentHost.GetContent<Cadences>();
            var refinements = ContentHost.GetContent<ItemRefinements>();
            var questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();
            var statAugments = ContentHost.GetContent<StatAugments>();

            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string? rootDir = currentDir;
            while (rootDir != null && !File.Exists(Path.Combine(rootDir, "Mythril.sln")))
            {
                rootDir = Path.GetDirectoryName(rootDir);
            }
            
            if (rootDir == null) throw new Exception("Could not find solution root.");

            string dataDir = Path.Combine(rootDir, "Mythril.Blazor/wwwroot/data");
            
            // Load Graph
            var nodes = JsonConvert.DeserializeObject<List<ContentNode>>(File.ReadAllText(Path.Combine(dataDir, "content_graph.json"))) ?? [];

            // 1. First Pass: Create base entities
            var loadedItems = new List<Item>();
            var loadedStats = new List<Stat>();
            var loadedAbilities = new List<CadenceAbility>();
            var loadedQuests = new List<Quest>();

            foreach (var node in nodes)
            {
                var data = node.Data;
                switch (node.Type)
                {
                    case "Item":
                        var itemType = Enum.Parse<ItemType>(data["item_type"].ToString() ?? "Material");
                        loadedItems.Add(new Item(node.Name, data["description"].ToString() ?? "", itemType));
                        break;
                    case "Stat":
                        loadedStats.Add(new Stat(node.Name, data["description"].ToString() ?? ""));
                        break;
                    case "Ability":
                        // Metadata handling via Newtonsoft (JObject)
                        var metaDict = new Dictionary<string, string>();
                        if (data.TryGetValue("metadata", out var metaObj) && metaObj is Newtonsoft.Json.Linq.JObject metaElem)
                        {
                            foreach (var prop in metaElem)
                            {
                                metaDict[prop.Key] = prop.Value?.ToString() ?? "";
                            }
                        }
                        loadedAbilities.Add(new CadenceAbility(node.Name, "") { Metadata = metaDict });
                        break;
                    case "Quest":
                        loadedQuests.Add(new Quest(node.Name, data["description"].ToString() ?? ""));
                        break;
                }
            }

            // Load base lists into singletons
            items.Load(loadedItems);
            stats.Load(loadedStats);
            abilities.Load(loadedAbilities);
            quests.Load(loadedQuests);

            // 2. Second Pass: Build relationships and complex objects
            var nodeMap = nodes.ToDictionary(n => n.Id);
            
            var locationList = new List<Location>();
            var cadenceList = new List<Cadence>();
            
            var questDetailDict = new Dictionary<Quest, QuestDetail>();
            var questUnlockDict = new Dictionary<Quest, Quest[]>();
            var questCadenceDict = new Dictionary<Quest, Cadence[]>();
            var refinementDict = new Dictionary<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)>();

            foreach (var node in nodes)
            {
                if (node.Type == "Location")
                {
                    var questsInLoc = new List<Quest>();
                    if (node.OutEdges.TryGetValue("contains", out var containsEdges))
                    {
                        foreach (var edge in containsEdges)
                        {
                            if (nodeMap.TryGetValue(edge.TargetId, out var targetNode))
                                questsInLoc.Add(quests.All.First(q => q.Name == targetNode.Name));
                        }
                    }

                    string? reqQuestName = null;
                    if (node.InEdges.TryGetValue("requires_quest", out var reqEdges) && reqEdges.Any())
                    {
                        if (nodeMap.TryGetValue(reqEdges.First(), out var targetNode))
                            reqQuestName = targetNode.Name;
                    }

                    locationList.Add(new Location(node.Name, questsInLoc, reqQuestName, node.Data["region_type"].ToString()));
                }
                else if (node.Type == "Cadence")
                {
                    var unlocks = new List<CadenceUnlock>();
                    if (node.OutEdges.TryGetValue("provides_ability", out var abilEdges))
                    {
                        foreach (var edge in abilEdges)
                        {
                            if (nodeMap.TryGetValue(edge.TargetId, out var abNode))
                            {
                                var ability = abilities.All.First(a => a.Name == abNode.Name);
                                
                                var requirements = new List<ItemQuantity>();
                                if (abNode.OutEdges.TryGetValue("consumes", out var costEdges))
                                {
                                    foreach (var cost in costEdges)
                                    {
                                        if (nodeMap.TryGetValue(cost.TargetId, out var itemNode))
                                            requirements.Add(new ItemQuantity(items.All.First(i => i.Name == itemNode.Name), cost.Quantity));
                                    }
                                }

                                unlocks.Add(new CadenceUnlock(
                                    node.Name, 
                                    ability, 
                                    requirements.ToArray(), 
                                    abNode.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Magic" : "Magic"
                                ));
                            }
                        }
                    }
                    cadenceList.Add(new Cadence(node.Name, node.Data["description"].ToString() ?? "", unlocks.ToArray()));
                }
                else if (node.Type == "Quest")
                {
                    var quest = quests.All.First(q => q.Name == node.Name);
                    
                    // Unlocks (Requirements)
                    if (node.InEdges.TryGetValue("requires_quest", out var reqEdges))
                    {
                        var reqs = new List<Quest>();
                        foreach (var id in reqEdges)
                        {
                            if (nodeMap.TryGetValue(id, out var targetNode))
                                reqs.Add(quests.All.First(q => q.Name == targetNode.Name));
                        }
                        questUnlockDict[quest] = reqs.ToArray();
                    }

                    // Details
                    var reqItems = new List<ItemQuantity>();
                    if (node.OutEdges.TryGetValue("consumes", out var costEdges))
                    {
                        foreach (var cost in costEdges)
                        {
                            if (nodeMap.TryGetValue(cost.TargetId, out var itemNode))
                                reqItems.Add(new ItemQuantity(items.All.First(i => i.Name == itemNode.Name), cost.Quantity));
                        }
                    }

                    var rewItems = new List<ItemQuantity>();
                    if (node.OutEdges.TryGetValue("rewards", out var rewEdges))
                    {
                        foreach (var rew in rewEdges)
                        {
                            if (nodeMap.TryGetValue(rew.TargetId, out var itemNode))
                                rewItems.Add(new ItemQuantity(items.All.First(i => i.Name == itemNode.Name), rew.Quantity));
                        }
                    }

                    var qType = Enum.Parse<QuestType>(node.Data.TryGetValue("quest_type", out var qt) ? qt.ToString() ?? "Single" : "Single");
                    
                    var reqStats = new Dictionary<string, int>();
                    if (node.Data.TryGetValue("required_stats", out var rsObj) && rsObj is Newtonsoft.Json.Linq.JObject rsElem)
                    {
                       foreach (var prop in rsElem) reqStats[prop.Key] = (int)prop.Value!;
                    }

                    var statRewards = new Dictionary<string, int>();
                    if (node.Data.TryGetValue("stat_rewards", out var srObj) && srObj is Newtonsoft.Json.Linq.JObject srElem)
                    {
                       foreach (var prop in srElem) statRewards[prop.Key] = (int)prop.Value!;
                    }

                    questDetailDict[quest] = new QuestDetail(
                        int.Parse(node.Data["duration"].ToString() ?? "10"),
                        reqItems.ToArray(),
                        rewItems.ToArray(),
                        qType,
                        node.Data["primary_stat"].ToString() ?? "Vitality",
                        reqStats.Any() ? reqStats : null,
                        statRewards.Any() ? statRewards : null
                    );
                }
                else if (node.Type == "Refinement")
                {
                    if (node.InEdges.TryGetValue("requires_ability", out var abIds) && abIds.Any())
                    {
                        if (nodeMap.TryGetValue(abIds.First(), out var abNode))
                        {
                            var ability = abilities.All.First(a => a.Name == abNode.Name);
                            if (!refinementDict.ContainsKey(ability))
                            {
                                refinementDict[ability] = (node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Strength" : "Strength", new Dictionary<Item, Recipe>());
                            }

                            if (node.OutEdges.TryGetValue("consumes", out var inEdges) && node.OutEdges.TryGetValue("produces", out var outEdges))
                            {
                                var inEdge = inEdges.First();
                                var outEdge = outEdges.First();
                                if (nodeMap.TryGetValue(inEdge.TargetId, out var inItemNode) && nodeMap.TryGetValue(outEdge.TargetId, out var outItemNode))
                                {
                                    var inputItem = items.All.First(i => i.Name == inItemNode.Name);
                                    var outputItem = items.All.First(i => i.Name == outItemNode.Name);
                                    refinementDict[ability].Recipes[inputItem] = new Recipe(inEdge.Quantity, outputItem, outEdge.Quantity);
                                }
                            }
                        }
                    }
                }
            }

            // Load Secondary Singletons
            locations.Load(locationList);
            cadences.Load(cadenceList);
            questDetails.Load(questDetailDict);
            questUnlocks.Load(questUnlockDict);
            refinements.Load(refinementDict);

            // 3. Third Pass: Quest -> Cadence (Now that Cadences are loaded)
            foreach (var node in nodes.Where(n => n.Type == "Quest"))
            {
                if (node.OutEdges.TryGetValue("unlocks_cadence", out var cadEdges))
                {
                    var quest = quests.All.First(q => q.Name == node.Name);
                    var cads = new List<Cadence>();
                    foreach (var edge in cadEdges)
                    {
                        if (nodeMap.TryGetValue(edge.TargetId, out var targetNode))
                            cads.Add(cadences.All.First(c => c.Name == targetNode.Name));
                    }
                    if (cads.Any()) questCadenceDict[quest] = cads.ToArray();
                }
            }
            questToCadenceUnlocks.Load(questCadenceDict);

            // 4. Stat Augments (Legacy Load)
            try {
                var statAugmentDTOs = JsonConvert.DeserializeObject<List<StatAugmentItemDTO>>(File.ReadAllText(Path.Combine(dataDir, "stat_augments.json"))) ?? [];
                var statAugmentsDict = new Dictionary<Item, StatAugment[]>();
                foreach (var d in statAugmentDTOs)
                {
                    var i = items.All.FirstOrDefault(x => x.Name == d.Item);
                    if (i.Name == null) continue;
                    statAugmentsDict[i] = d.Augments.Select(a => {
                        var s = stats.All.FirstOrDefault(x => x.Name == a.Stat);
                        return new StatAugment(s, a.ModifierAtFull);
                    }).Where(x => x.Stat.Name != null).ToArray();
                }
                statAugments.Load(statAugmentsDict);
            } catch (Exception ex) { Console.WriteLine($"Error loading legacy stat augments: {ex.Message}"); }

            _loaded = true;
        }
    }
}
