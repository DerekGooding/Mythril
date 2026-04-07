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
            var dataDir = GetTestDataDir();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
            var nodes = JsonSerializer.Deserialize<List<ContentNode>>(File.ReadAllText(Path.Combine(dataDir, "content_graph.json")), options) ?? [];
            var nodeMap = nodes.ToDictionary(n => n.Id);

            LoadBaseEntities(nodes);
            var (locs, cads, details, unlocks, refinements, abAugs) = ProcessRelationships(nodes, nodeMap);

            ContentHost.GetContent<Locations>().Load(locs);
            ContentHost.GetContent<Cadences>().Load(cads);
            ContentHost.GetContent<QuestDetails>().Load(details);
            ContentHost.GetContent<QuestUnlocks>().Load(unlocks);
            ContentHost.GetContent<ItemRefinements>().Load(refinements);
            ContentHost.GetContent<AbilityAugments>().Load(abAugs);

            LoadQuestCadenceUnlocks(nodes, nodeMap);
            LoadLegacyStatAugments(dataDir, options);
            _loaded = true;
        }
    }

    private static string GetTestDataDir()
    {
        var root = AppDomain.CurrentDomain.BaseDirectory;
        while (root != null && !File.Exists(Path.Combine(root, "Mythril.sln"))) root = Path.GetDirectoryName(root);
        return Path.Combine(root ?? throw new Exception("No solution root"), "Mythril.Blazor/wwwroot/data");
    }

    private static void LoadBaseEntities(List<ContentNode> nodes)
    {
        var items = new List<Item>(); var stats = new List<Stat>(); var abils = new List<CadenceAbility>(); var qs = new List<Quest>();
        foreach (var node in nodes) {
            var data = node.Data;
            switch (node.Type) {
                case "Item": items.Add(new Item(node.Name, data.TryGetValue("description", out var d) ? d.ToString() ?? "" : "", Enum.Parse<ItemType>(data.TryGetValue("item_type", out var t) ? t.ToString() ?? "Material" : "Material"))); break;
                case "Stat": stats.Add(new Stat(node.Name, data.TryGetValue("description", out var sd) ? sd.ToString() ?? "" : "")); break;
                case "Ability":
                    var meta = new Dictionary<string, string>();
                    if (data.TryGetValue("metadata", out var mo) && mo is JsonElement me) foreach (var p in me.EnumerateObject()) meta[p.Name] = p.Value.ToString();
                    var effs = new List<EffectDefinition>();
                    if (data.TryGetValue("effects", out var eo) && eo is JsonElement ee && ee.ValueKind == JsonValueKind.Array)
                        effs = JsonSerializer.Deserialize<List<EffectDefinition>>(ee.GetRawText(), options) ?? [];
                    abils.Add(new CadenceAbility(node.Name, "") { Metadata = meta, Effects = effs.ToArray() }); break;
                case "Quest": qs.Add(new Quest(node.Name, data.TryGetValue("description", out var qd) ? qd.ToString() ?? "" : "")); break;
            }
        }
        ContentHost.GetContent<Items>().Load(items); ContentHost.GetContent<Stats>().Load(stats);
        ContentHost.GetContent<CadenceAbilities>().Load(abils); ContentHost.GetContent<Quests>().Load(qs);
    }

    private static (List<Location>, List<Cadence>, Dictionary<Quest, QuestDetail>, Dictionary<Quest, Quest[]>, Dictionary<CadenceAbility, (string, Dictionary<Item, Recipe>)>, Dictionary<CadenceAbility, Stat>)
    ProcessRelationships(List<ContentNode> nodes, Dictionary<string, ContentNode> nodeMap, JsonSerializerOptions options)
    {
        var items = ContentHost.GetContent<Items>(); var stats = ContentHost.GetContent<Stats>(); var abils = ContentHost.GetContent<CadenceAbilities>(); var qs = ContentHost.GetContent<Quests>();
        var locList = new List<Location>(); var cadList = new List<Cadence>(); var detailDict = new Dictionary<Quest, QuestDetail>();
        var unlockDict = new Dictionary<Quest, Quest[]>(); var refDict = new Dictionary<CadenceAbility, (string, Dictionary<Item, Recipe>)>(); var abAugDict = new Dictionary<CadenceAbility, Stat>();

        foreach (var node in nodes) {
            if (node.Type == "Ability") {
                var ab = abils.All.First(a => a.Name == node.Name);
                abAugDict[ab] = stats.All.First(s => s.Name == (node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Magic" : "Magic"));
            } else if (node.Type == "Location") {
                locList.Add(new Location(node.Name, node.OutEdges.TryGetValue("contains", out var ces) ? ces.Select(e => qs.All.First(q => q.Name == nodeMap[e.TargetId].Name)) : [], node.InEdges.TryGetValue("requires_quest", out var res) && res.Any() ? nodeMap[res.First()].Name : null, node.Data.TryGetValue("region_type", out var rt) ? rt.ToString() : "Plains"));
            } else if (node.Type == "Cadence") {
                var unls = new List<CadenceUnlock>();
                if (node.OutEdges.TryGetValue("provides_ability", out var aes)) foreach (var e in aes) {
                    var an = nodeMap[e.TargetId]; var ab = abils.All.First(a => a.Name == an.Name);
                    var reqs = an.OutEdges.TryGetValue("consumes", out var costs) ? costs.Select(c => new ItemQuantity(items.All.First(i => i.Name == nodeMap[c.TargetId].Name), c.Quantity)).ToArray() : [];
                    unls.Add(new CadenceUnlock(node.Name, ab, reqs, an.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Magic" : "Magic"));
                }
                cadList.Add(new Cadence(node.Name, node.Data.TryGetValue("description", out var cd) ? cd.ToString() ?? "" : "", unls.ToArray()));
            } else if (node.Type == "Quest") {
                var q = qs.All.First(x => x.Name == node.Name);
                if (node.InEdges.TryGetValue("requires_quest", out var res)) unlockDict[q] = res.Select(id => qs.All.First(x => x.Name == nodeMap[id].Name)).ToArray();
                var reqs = node.OutEdges.TryGetValue("consumes", out var costs) ? costs.Select(c => new ItemQuantity(items.All.First(i => i.Name == nodeMap[c.TargetId].Name), c.Quantity)).ToArray() : [];
                var rews = node.OutEdges.TryGetValue("rewards", out var rewards) ? rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == nodeMap[r.TargetId].Name), r.Quantity)).ToArray() : [];
                var rs = node.Data.TryGetValue("required_stats", out var rso) && rso is JsonElement rse ? rse.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32()) : null;
                var sr = node.Data.TryGetValue("stat_rewards", out var sro) && sro is JsonElement sre ? sre.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32()) : null;
                var qeffs = new List<EffectDefinition>();
                if (node.Data.TryGetValue("effects", out var eo) && eo is JsonElement ee && ee.ValueKind == JsonValueKind.Array)
                    qeffs = JsonSerializer.Deserialize<List<EffectDefinition>>(ee.GetRawText(), options) ?? [];
                detailDict[q] = new QuestDetail(int.Parse(node.Data.TryGetValue("duration", out var dur) ? dur.ToString() ?? "10" : "10"), reqs, rews, Enum.Parse<QuestType>(node.Data.TryGetValue("quest_type", out var qt) ? qt.ToString() ?? "Single" : "Single"), node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Vitality" : "Vitality", rs, sr, qeffs.ToArray());
            } else if (node.Type == "Refinement" && node.InEdges.TryGetValue("requires_ability", out var abIds)) {
                var an = nodeMap[abIds.First()]; var ab = abils.All.First(a => a.Name == an.Name);
                if (!refDict.ContainsKey(ab)) refDict[ab] = (node.Data.TryGetValue("primary_stat", out var ps) ? ps.ToString() ?? "Strength" : "Strength", []);
                if (node.OutEdges.TryGetValue("consumes", out var ins) && node.OutEdges.TryGetValue("produces", out var outs))
                    refDict[ab].Item2[items.All.First(i => i.Name == nodeMap[ins.First().TargetId].Name)] = new Recipe(ins.First().Quantity, items.All.First(i => i.Name == nodeMap[outs.First().TargetId].Name), outs.First().Quantity);
            }
        }
        return (locList, cadList, detailDict, unlockDict, refDict, abAugDict);
    }

    private static void LoadQuestCadenceUnlocks(List<ContentNode> nodes, Dictionary<string, ContentNode> nodeMap)
    {
        var dict = new Dictionary<Quest, Cadence[]>(); var qs = ContentHost.GetContent<Quests>(); var cads = ContentHost.GetContent<Cadences>();
        foreach (var node in nodes.Where(n => n.Type == "Quest" && n.OutEdges.ContainsKey("unlocks_cadence")))
            dict[qs.All.First(q => q.Name == node.Name)] = node.OutEdges["unlocks_cadence"].Select(e => cads.All.First(c => c.Name == nodeMap[e.TargetId].Name)).ToArray();
        ContentHost.GetContent<QuestToCadenceUnlocks>().Load(dict);
    }

    private static void LoadLegacyStatAugments(string dataDir, JsonSerializerOptions options)
    {
        try {
            var dtos = JsonSerializer.Deserialize<List<StatAugmentItemDTO>>(File.ReadAllText(Path.Combine(dataDir, "stat_augments.json")), options) ?? [];
            var items = ContentHost.GetContent<Items>(); var stats = ContentHost.GetContent<Stats>(); var dict = new Dictionary<Item, StatAugment[]>();
            foreach (var d in dtos) {
                var i = items.All.FirstOrDefault(x => x.Name == d.Item); if (i.Name == null) continue;
                dict[i] = d.Augments.Select(a => new StatAugment(stats.All.First(s => s.Name == a.Stat), a.ModifierAtFull)).ToArray();
            }
            ContentHost.GetContent<StatAugments>().Load(dict);
        } catch (Exception ex) { Console.WriteLine($"Error loading legacy stat augments: {ex.Message}"); }
    }
}
