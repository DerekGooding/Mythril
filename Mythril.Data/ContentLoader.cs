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
        Console.WriteLine("Starting Content Load...");
        try {
            var itemsList = await http.GetFromJsonAsync<List<Item>>("data/items.json", _options) ?? [];
            Console.WriteLine($"Loaded {itemsList.Count} items.");
            items.Load(itemsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading items: {ex.Message}"); throw; }

        try {
            var statsList = await http.GetFromJsonAsync<List<Stat>>("data/stats.json", _options) ?? [];
            Console.WriteLine($"Loaded {statsList.Count} stats.");
            stats.Load(statsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading stats: {ex.Message}"); throw; }

        try {
            var abilitiesList = await http.GetFromJsonAsync<List<CadenceAbility>>("data/cadence_abilities.json", _options) ?? [];
            Console.WriteLine($"Loaded {abilitiesList.Count} abilities.");
            abilities.Load(abilitiesList);
        } catch (Exception ex) { Console.WriteLine($"Error loading abilities: {ex.Message}"); throw; }

        try {
            var questsList = await http.GetFromJsonAsync<List<Quest>>("data/quests.json", _options) ?? [];
            Console.WriteLine($"Loaded {questsList.Count} quests.");
            quests.Load(questsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading quests: {ex.Message}"); throw; }

        try {
            var locationDTOs = await http.GetFromJsonAsync<List<LocationDTO>>("data/locations.json", _options) ?? [];
            Console.WriteLine($"Loaded {locationDTOs.Count} locations.");
            var locationsList = locationDTOs.Select(d => new Location(d.Name, d.Quests.Select(qn => {
                var q = quests.All.FirstOrDefault(x => x.Name == qn);
                if (q.Name == null) Console.WriteLine($"WARNING: Quest '{qn}' not found for location '{d.Name}'");
                return q;
            }).Where(x => x.Name != null))).ToList();
            locations.Load(locationsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading locations: {ex.Message}"); throw; }

        try {
            var cadenceDTOs = await http.GetFromJsonAsync<List<CadenceDTO>>("data/cadences.json", _options) ?? [];
            Console.WriteLine($"Loaded {cadenceDTOs.Count} cadences.");
            var cadencesList = cadenceDTOs.Select(d => new Cadence(d.Name, d.Description, d.Abilities.Select(a => {
                var ab = abilities.All.FirstOrDefault(x => x.Name == a.Ability);
                if (ab.Name == null) Console.WriteLine($"WARNING: Ability '{a.Ability}' not found for cadence '{d.Name}'");
                return new CadenceUnlock(
                    ab,
                    a.Requirements.Select(r => {
                        var i = items.All.FirstOrDefault(x => x.Name == r.Item);
                        if (i.Name == null) Console.WriteLine($"WARNING: Item '{r.Item}' not found for ability '{a.Ability}' in cadence '{d.Name}'");
                        return new ItemQuantity(i, r.Quantity);
                    }).Where(x => x.Item.Name != null).ToArray()
                );
            }).Where(x => x.Ability.Name != null).ToArray())).ToList();
            cadences.Load(cadencesList);
        } catch (Exception ex) { Console.WriteLine($"Error loading cadences: {ex.Message}"); throw; }

        try {
            var detailDTOs = await http.GetFromJsonAsync<List<QuestDetailDTO>>("data/quest_details.json", _options) ?? [];
            Console.WriteLine($"Loaded {detailDTOs.Count} quest details.");
            var detailsDict = new Dictionary<Quest, QuestDetail>();
            foreach (var d in detailDTOs)
            {
                var q = quests.All.FirstOrDefault(x => x.Name == d.Quest);
                if (q.Name == null) { Console.WriteLine($"WARNING: Quest '{d.Quest}' not found for details"); continue; }
                detailsDict[q] = new QuestDetail(d.DurationSeconds, 
                    d.Requirements.Select(r => {
                        var i = items.All.FirstOrDefault(x => x.Name == r.Item);
                        if (i.Name == null) Console.WriteLine($"WARNING: Item '{r.Item}' not found for quest requirements '{d.Quest}'");
                        return new ItemQuantity(i, r.Quantity);
                    }).Where(x => x.Item.Name != null).ToArray(),
                    d.Rewards.Select(r => {
                        var i = items.All.FirstOrDefault(x => x.Name == r.Item);
                        if (i.Name == null) Console.WriteLine($"WARNING: Item '{r.Item}' not found for quest rewards '{d.Quest}'");
                        return new ItemQuantity(i, r.Quantity);
                    }).Where(x => x.Item.Name != null).ToArray(),
                    Enum.Parse<QuestType>(d.Type)
                );
            }
            questDetails.Load(detailsDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading quest details: {ex.Message}"); throw; }

        try {
            var unlockDTOs = await http.GetFromJsonAsync<List<QuestUnlockDTO>>("data/quest_unlocks.json", _options) ?? [];
            Console.WriteLine($"Loaded {unlockDTOs.Count} quest unlocks.");
            var unlocksDict = new Dictionary<Quest, Quest[]>();
            foreach (var d in unlockDTOs)
            {
                var q = quests.All.FirstOrDefault(x => x.Name == d.Quest);
                if (q.Name == null) { Console.WriteLine($"WARNING: Quest '{d.Quest}' not found for unlocks"); continue; }
                unlocksDict[q] = d.Requires.Select(rn => {
                    var rq = quests.All.FirstOrDefault(x => x.Name == rn);
                    if (rq.Name == null) Console.WriteLine($"WARNING: Required quest '{rn}' not found for quest '{d.Quest}'");
                    return rq;
                }).Where(x => x.Name != null).ToArray();
            }
            questUnlocks.Load(unlocksDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading quest unlocks: {ex.Message}"); throw; }

        try {
            var refinementDTOs = await http.GetFromJsonAsync<List<RefinementDTO>>("data/refinements.json", _options) ?? [];
            Console.WriteLine($"Loaded {refinementDTOs.Count} refinements.");
            var refinementsDict = new Dictionary<CadenceAbility, Dictionary<Item, Recipe>>();
            foreach (var d in refinementDTOs)
            {
                var ab = abilities.All.FirstOrDefault(x => x.Name == d.Ability);
                if (ab.Name == null) { Console.WriteLine($"WARNING: Ability '{d.Ability}' not found for refinements"); continue; }
                refinementsDict[ab] = d.Recipes.Select(r => {
                    var ii = items.All.FirstOrDefault(x => x.Name == r.InputItem);
                    var oi = items.All.FirstOrDefault(x => x.Name == r.OutputItem);
                    if (ii.Name == null) Console.WriteLine($"WARNING: Input item '{r.InputItem}' not found for refinement '{d.Ability}'");
                    if (oi.Name == null) Console.WriteLine($"WARNING: Output item '{r.OutputItem}' not found for refinement '{d.Ability}'");
                    return new { Input = ii, Output = oi, r.InputQuantity, r.OutputQuantity };
                }).Where(x => x.Input.Name != null && x.Output.Name != null)
                .ToDictionary(x => x.Input, x => new Recipe(x.InputQuantity, x.Output, x.OutputQuantity));
            }
            refinements.Load(refinementsDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading refinements: {ex.Message}"); throw; }

        try {
            var questCadenceDTOs = await http.GetFromJsonAsync<List<QuestCadenceUnlockDTO>>("data/quest_cadence_unlocks.json", _options) ?? [];
            Console.WriteLine($"Loaded {questCadenceDTOs.Count} quest-to-cadence unlocks.");
            var questCadenceDict = new Dictionary<Quest, Cadence[]>();
            foreach (var d in questCadenceDTOs)
            {
                var q = quests.All.FirstOrDefault(x => x.Name == d.Quest);
                if (q.Name == null) { Console.WriteLine($"WARNING: Quest '{d.Quest}' not found for cadence unlocks"); continue; }
                questCadenceDict[q] = d.Cadences.Select(cn => {
                    var c = cadences.All.FirstOrDefault(x => x.Name == cn);
                    if (c.Name == null) Console.WriteLine($"WARNING: Cadence '{cn}' not found for quest '{d.Quest}'");
                    return c;
                }).Where(x => x.Name != null).ToArray();
            }
            questToCadenceUnlocks.Load(questCadenceDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading quest-to-cadence unlocks: {ex.Message}"); throw; }

        try {
            var statAugmentDTOs = await http.GetFromJsonAsync<List<StatAugmentItemDTO>>("data/stat_augments.json", _options) ?? [];
            Console.WriteLine($"Loaded {statAugmentDTOs.Count} stat augments.");
            var statAugmentsDict = new Dictionary<Item, StatAugment[]>();
            foreach (var d in statAugmentDTOs)
            {
                var i = items.All.FirstOrDefault(x => x.Name == d.Item);
                if (i.Name == null) { Console.WriteLine($"WARNING: Item '{d.Item}' not found for stat augments"); continue; }
                statAugmentsDict[i] = d.Augments.Select(a => {
                    var s = stats.All.FirstOrDefault(x => x.Name == a.Stat);
                    if (s.Name == null) Console.WriteLine($"WARNING: Stat '{a.Stat}' not found for item '{d.Item}' augments");
                    return new StatAugment(s, a.ModifierAtFull);
                }).Where(x => x.Stat.Name != null).ToArray();
            }
            statAugments.Load(statAugmentsDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading stat augments: {ex.Message}"); }
        
        Console.WriteLine("Content Load Complete.");
    }
}
