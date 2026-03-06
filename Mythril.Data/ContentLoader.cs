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
    AbilityAugments abilityAugments,
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

            var nodeMap = nodes.ToDictionary(n => n.Id);

            LoadBaseEntities(nodes);
            
            var (locationList, cadenceList, questDetailDict, questUnlockDict, refinementDict, abilityAugmentsDict) = ProcessRelationships(nodes, nodeMap);

            locations.Load(locationList);
            cadences.Load(cadenceList);
            questDetails.Load(questDetailDict);
            questUnlocks.Load(questUnlockDict);
            refinements.Load(refinementDict);
            abilityAugments.Load(abilityAugmentsDict);

            LoadQuestCadenceUnlocks(nodes, nodeMap);
            await LoadLegacyStatAugments();

        } catch (Exception ex) { Console.WriteLine($"CRITICAL CONTENT LOAD ERROR: {ex.Message}"); throw; }
        Console.WriteLine("Content Graph Load Complete.");
    }

    private void LoadBaseEntities(List<ContentNode> nodes)
    {
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
                    var metaDict = new Dictionary<string, string>();
                    if (data.TryGetValue("metadata", out var metaObj) && metaObj is JsonElement metaElem)
                        foreach (var prop in metaElem.EnumerateObject()) metaDict[prop.Name] = prop.Value.ToString();
                    loadedAbilities.Add(new CadenceAbility(node.Name, "") { Metadata = metaDict });
                    break;
                case "Quest":
                    loadedQuests.Add(new Quest(node.Name, data["description"].ToString() ?? ""));
                    break;
            }
        }
        items.Load(loadedItems);
        stats.Load(loadedStats);
        abilities.Load(loadedAbilities);
        quests.Load(loadedQuests);
    }

    private (List<Location>, List<Cadence>, Dictionary<Quest, QuestDetail>, Dictionary<Quest, Quest[]>, Dictionary<CadenceAbility, (string, Dictionary<Item, Recipe>)>, Dictionary<CadenceAbility, Stat>) 
    ProcessRelationships(List<ContentNode> nodes, Dictionary<string, ContentNode> nodeMap)
    {
        var locationList = new List<Location>();
        var cadenceList = new List<Cadence>();
        var questDetailDict = new Dictionary<Quest, QuestDetail>();
        var questUnlockDict = new Dictionary<Quest, Quest[]>();
        var refinementDict = new Dictionary<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)>();
        var abilityAugmentsDict = new Dictionary<CadenceAbility, Stat>();

        foreach (var node in nodes)
        {
            if (node.Type == "Ability")
            {
                var ability = abilities.All.First(a => a.Name == node.Name);
                var statName = node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Magic" : "Magic";
                abilityAugmentsDict[ability] = stats.All.First(s => s.Name == statName);
            }
            else if (node.Type == "Location")
            {
                var questsInLoc = node.OutEdges.TryGetValue("contains", out var edges) 
                    ? edges.Select(e => quests.All.First(q => q.Name == nodeMap[e.TargetId].Name)).ToList() 
                    : [];
                string? reqQuest = node.InEdges.TryGetValue("requires_quest", out var inEdges) && inEdges.Any() ? nodeMap[inEdges.First()].Name : null;
                locationList.Add(new Location(node.Name, questsInLoc, reqQuest, node.Data["region_type"].ToString()));
            }
            else if (node.Type == "Cadence")
            {
                var unlocks = new List<CadenceUnlock>();
                if (node.OutEdges.TryGetValue("provides_ability", out var edges))
                {
                    foreach (var edge in edges)
                    {
                        var abNode = nodeMap[edge.TargetId];
                        var ability = abilities.All.First(a => a.Name == abNode.Name);
                        var reqs = abNode.OutEdges.TryGetValue("consumes", out var costs) 
                            ? costs.Select(c => new ItemQuantity(items.All.First(i => i.Name == nodeMap[c.TargetId].Name), c.Quantity)).ToArray() 
                            : [];
                        unlocks.Add(new CadenceUnlock(node.Name, ability, reqs, abNode.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Magic" : "Magic"));
                    }
                }
                cadenceList.Add(new Cadence(node.Name, node.Data["description"].ToString() ?? "", unlocks.ToArray()));
            }
            else if (node.Type == "Quest")
            {
                var quest = quests.All.First(q => q.Name == node.Name);
                if (node.InEdges.TryGetValue("requires_quest", out var inEdges))
                    questUnlockDict[quest] = inEdges.Select(id => quests.All.First(q => q.Name == nodeMap[id].Name)).ToArray();

                var reqs = node.OutEdges.TryGetValue("consumes", out var costs) ? costs.Select(c => new ItemQuantity(items.All.First(i => i.Name == nodeMap[c.TargetId].Name), c.Quantity)).ToArray() : [];
                var rews = node.OutEdges.TryGetValue("rewards", out var rewards) ? rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == nodeMap[r.TargetId].Name), r.Quantity)).ToArray() : [];
                
                var reqStats = node.Data.TryGetValue("required_stats", out var rs) && rs is JsonElement rsElem ? rsElem.EnumerateObject().ToDictionary(p => propName(p), p => p.Value.GetInt32()) : null;
                var statRews = node.Data.TryGetValue("stat_rewards", out var sr) && sr is JsonElement srElem ? srElem.EnumerateObject().ToDictionary(p => propName(p), p => p.Value.GetInt32()) : null;

                questDetailDict[quest] = new QuestDetail(int.Parse(node.Data["duration"].ToString() ?? "10"), reqs, rews, 
                    Enum.Parse<QuestType>(node.Data.TryGetValue("quest_type", out var qt) ? qt.ToString() ?? "Single" : "Single"),
                    node.Data["primary_stat"].ToString() ?? "Vitality", reqStats, statRews);
            }
            else if (node.Type == "Refinement" && node.InEdges.TryGetValue("requires_ability", out var abIds))
            {
                var abNode = nodeMap[abIds.First()];
                var ability = abilities.All.First(a => a.Name == abNode.Name);
                if (!refinementDict.ContainsKey(ability)) refinementDict[ability] = (node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Strength" : "Strength", []);
                if (node.OutEdges.TryGetValue("consumes", out var ins) && node.OutEdges.TryGetValue("produces", out var outs))
                    refinementDict[ability].Recipes[items.All.First(i => i.Name == nodeMap[ins.First().TargetId].Name)] = new Recipe(ins.First().Quantity, items.All.First(i => i.Name == nodeMap[outs.First().TargetId].Name), outs.First().Quantity);
            }
        }
        return (locationList, cadenceList, questDetailDict, questUnlockDict, refinementDict, abilityAugmentsDict);
    }

    private string propName(JsonProperty p) => p.Name;

    private void LoadQuestCadenceUnlocks(List<ContentNode> nodes, Dictionary<string, ContentNode> nodeMap)
    {
        var dict = new Dictionary<Quest, Cadence[]>();
        foreach (var node in nodes.Where(n => n.Type == "Quest" && n.OutEdges.ContainsKey("unlocks_cadence")))
        {
            var quest = quests.All.First(q => q.Name == node.Name);
            dict[quest] = node.OutEdges["unlocks_cadence"].Select(e => cadences.All.First(c => c.Name == nodeMap[e.TargetId].Name)).ToArray();
        }
        questToCadenceUnlocks.Load(dict);
    }

    private async Task LoadLegacyStatAugments()
    {
        try {
            var dtos = await http.GetFromJsonAsync<List<StatAugmentItemDTO>>("data/stat_augments.json", _options) ?? [];
            var dict = new Dictionary<Item, StatAugment[]>();
            foreach (var d in dtos) {
                var i = items.All.FirstOrDefault(x => x.Name == d.Item);
                if (i.Name == null) continue;
                dict[i] = d.Augments.Select(a => new StatAugment(stats.All.First(s => s.Name == a.Stat), a.ModifierAtFull)).ToArray();
            }
            statAugments.Load(dict);
        } catch (Exception ex) { Console.WriteLine($"Error loading legacy stat augments: {ex.Message}"); }
    }
}
