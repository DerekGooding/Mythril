using Newtonsoft.Json;
using Mythril.Data;

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

            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string? rootDir = currentDir;
            while (rootDir != null && !File.Exists(Path.Combine(rootDir, "Mythril.sln")))
            {
                rootDir = Path.GetDirectoryName(rootDir);
            }
            
            if (rootDir == null) throw new Exception("Could not find solution root.");

            string dataDir = Path.Combine(rootDir, "Mythril.Blazor/wwwroot/data");
            
            var itemsList = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(Path.Combine(dataDir, "items.json"))) ?? [];
            items.Load(itemsList);
            var questsList = JsonConvert.DeserializeObject<List<Quest>>(File.ReadAllText(Path.Combine(dataDir, "quests.json"))) ?? [];
            quests.Load(questsList);
            stats.Load(JsonConvert.DeserializeObject<List<Stat>>(File.ReadAllText(Path.Combine(dataDir, "stats.json"))) ?? []);
            abilities.Load(JsonConvert.DeserializeObject<List<CadenceAbility>>(File.ReadAllText(Path.Combine(dataDir, "cadence_abilities.json"))) ?? []);

            var locDTOs = JsonConvert.DeserializeObject<List<LocationDTO>>(File.ReadAllText(Path.Combine(dataDir, "locations.json"))) ?? [];
            locations.Load(locDTOs.Select(d => new Location(d.Name, d.Quests.Select(qn => quests.All.First(q => q.Name == qn)))).ToList());

            var cadDTOs = JsonConvert.DeserializeObject<List<CadenceDTO>>(File.ReadAllText(Path.Combine(dataDir, "cadences.json"))) ?? [];
            cadences.Load(cadDTOs.Select(d => new Cadence(d.Name, d.Description, d.Abilities.Select(a => new CadenceUnlock(
                abilities.All.First(ab => ab.Name == a.Ability),
                a.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray()
            )).ToArray())).ToList());

            var detailDTOs = JsonConvert.DeserializeObject<List<QuestDetailDTO>>(File.ReadAllText(Path.Combine(dataDir, "quest_details.json"))) ?? [];
            var detailsDict = detailDTOs.ToDictionary(
                d => quests.All.First(q => q.Name == d.Quest),
                d => new QuestDetail(d.DurationSeconds, 
                    d.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                    d.Rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                    Enum.Parse<QuestType>(d.Type)
                )
            );
            questDetails.Load(detailsDict);

            var unlockDTOs = JsonConvert.DeserializeObject<List<QuestUnlockDTO>>(File.ReadAllText(Path.Combine(dataDir, "quest_unlocks.json"))) ?? [];
            var unlocksDict = unlockDTOs.ToDictionary(
                d => quests.All.First(q => q.Name == d.Quest),
                d => d.Requires.Select(rn => quests.All.First(q => q.Name == rn)).ToArray()
            );
            questUnlocks.Load(unlocksDict);

            var refinementsDict = new Dictionary<CadenceAbility, Dictionary<Item, Recipe>>();
            // Add some dummy refinements if needed, but for coverage let's just Load an empty one or a small one
            ContentHost.GetContent<ItemRefinements>().Load(refinementsDict);

            var statAugments = ContentHost.GetContent<StatAugments>();
            var health = stats.All.First(s => s.Name == "Health");
            var potion = items.All.First(i => i.Name == "Potion");
            statAugments.Load(new Dictionary<Item, StatAugment[]> {
                { potion, [new StatAugment(health, 10)] }
            });

            var abilityAugments = ContentHost.GetContent<AbilityAugments>();
            var autoQuest = abilities.All.First(a => a.Name == "AutoQuest I");
            var magic = stats.All.First(s => s.Name == "Magic");
            abilityAugments.Load(new Dictionary<CadenceAbility, Stat> {
                { autoQuest, magic }
            });

            _loaded = true;
        }
    }
}
