using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mythril.Data;

public class ContentLoader(
    HttpClient http,
    Items items,
    Stats stats,
    CadenceAbilities abilities,
    Quests quests,
    Locations locations,
    Cadences cadences,
    QuestDetails questDetails,
    QuestUnlocks questUnlocks,
    ItemRefinements refinements,
    StatAugments statAugments,
    QuestToCadenceUnlocks questToCadenceUnlocks)
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public async Task LoadAllAsync()
    {
        Console.WriteLine("Starting Content Graph Load...");
        try {
            var nodes = await http.GetFromJsonAsync<List<ContentNode>>("data/content_graph.json", _options) ?? [];
            Console.WriteLine($"Loaded {nodes.Count} nodes from graph.");

            // 1. First Pass: Create base entities
            var loadedItems = new List<Item>();
            var loadedStats = new List<Stat>();
            var loadedAbilities = new List<CadenceAbility>();
            var loadedQuests = new List<Quest>();
            var loadedLocations = new List<Location>(); // Temp list
            var loadedCadences = new List<Cadence>();   // Temp list

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
                        // Metadata handling
                        var metaDict = new Dictionary<string, string>();
                        if (data.TryGetValue("metadata", out var metaObj) && metaObj is JsonElement metaElem)
                        {
                            foreach (var prop in metaElem.EnumerateObject())
                            {
                                metaDict[prop.Name] = prop.Value.ToString();
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
            
            // Temporary storage for Cadence Unlocks to be built after we have Cadence objects? 
            // No, Cadence objects need CadenceUnlock structs which need Items/Abilities.
            // Cadence objects are defined in the graph.

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

                    // Cadence Unlocks (Out-Edge)
                    if (node.OutEdges.TryGetValue("unlocks_cadence", out var cadEdges))
                    {
                        // We can't map to Cadence objects yet as we are building them.
                        // We store the names and map later? 
                        // Actually, we iterate nodes. Cadence list is local.
                        // We need a post-pass or just store names. 
                        // QuestToCadenceUnlocks needs Cadence objects.
                        // Let's defer QuestToCadence population until after this loop.
                    }

                    var qType = Enum.Parse<QuestType>(node.Data.TryGetValue("quest_type", out var qt) ? qt.ToString() ?? "Single" : "Single");
                    
                    // Stat Rewards / Requirements (Not fully in graph yet? Using defaults/legacy logic if missing)
                    // Graph migration script didn't explicitly map stat reqs/rewards to edges, 
                    // but they might be in 'data' or separate edges.
                    // For now, assume simple migration where complex stat logic might be missing 
                    // OR we need to update migration script if we lost data.
                    // Checking migration script... it only mapped 'quest_type', 'duration', 'primary_stat'.
                    // It missed 'RequiredStats' and 'StatRewards'. 
                    // CRITICAL: We need to update migration script or load legacy logic for those.
                    // BUT: We are replacing the loader. 
                    // FIX: I will update the migration script to include 'required_stats' and 'stat_rewards' in data.
                    
                    var reqStats = new Dictionary<string, int>();
                    if (node.Data.TryGetValue("required_stats", out var rsObj) && rsObj is JsonElement rsElem)
                    {
                       foreach (var prop in rsElem.EnumerateObject()) reqStats[prop.Name] = prop.Value.GetInt32();
                    }

                    var statRewards = new Dictionary<string, int>();
                    if (node.Data.TryGetValue("stat_rewards", out var srObj) && srObj is JsonElement srElem)
                    {
                       foreach (var prop in srElem.EnumerateObject()) statRewards[prop.Name] = prop.Value.GetInt32();
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
                    // Refinement node name is "Ability: Output"
                    // In-edge: requires_ability -> Ability Node
                    // Out-edges: consumes -> Input, produces -> Output
                    
                    if (node.InEdges.TryGetValue("requires_ability", out var abIds) && abIds.Any())
                    {
                        if (nodeMap.TryGetValue(abIds.First(), out var abNode))
                        {
                            var ability = abilities.All.First(a => a.Name == abNode.Name);
                            if (!refinementDict.ContainsKey(ability))
                            {
                                refinementDict[ability] = (node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Strength" : "Strength", new Dictionary<Item, Recipe>());
                            }

                            // Input/Output
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

            // 4. Stat Augments (Not in graph yet? Need to check migration script)
            // Migration script did NOT include stat_augments.json. 
            // We need to load that legacy file or update migration.
            // For completeness, I will load legacy stat_augments.json here as it wasn't graph-migrated.
            try {
                var statAugmentDTOs = await http.GetFromJsonAsync<List<StatAugmentItemDTO>>("data/stat_augments.json", _options) ?? [];
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

        } catch (Exception ex) { Console.WriteLine($"CRITICAL CONTENT LOAD ERROR: {ex.Message}"); throw; }
        
        Console.WriteLine("Content Graph Load Complete.");
    }
}
