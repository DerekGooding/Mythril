using System.Net.Http.Json;
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
    QuestToCadenceUnlocks questToCadenceUnlocks)
{
    public async Task LoadAllAsync()
    {
        Console.WriteLine("Starting Content Load...");
        try {
            var itemsList = await http.GetFromJsonAsync<List<Item>>("data/items.json") ?? [];
            Console.WriteLine($"Loaded {itemsList.Count} items.");
            items.Load(itemsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading items: {ex.Message}"); throw; }

        try {
            var statsList = await http.GetFromJsonAsync<List<Stat>>("data/stats.json") ?? [];
            Console.WriteLine($"Loaded {statsList.Count} stats.");
            stats.Load(statsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading stats: {ex.Message}"); throw; }

        try {
            var abilitiesList = await http.GetFromJsonAsync<List<CadenceAbility>>("data/cadence_abilities.json") ?? [];
            Console.WriteLine($"Loaded {abilitiesList.Count} abilities.");
            abilities.Load(abilitiesList);
        } catch (Exception ex) { Console.WriteLine($"Error loading abilities: {ex.Message}"); throw; }

        try {
            var questsList = await http.GetFromJsonAsync<List<Quest>>("data/quests.json") ?? [];
            Console.WriteLine($"Loaded {questsList.Count} quests.");
            quests.Load(questsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading quests: {ex.Message}"); throw; }

        try {
            var locationDTOs = await http.GetFromJsonAsync<List<LocationDTO>>("data/locations.json") ?? [];
            Console.WriteLine($"Loaded {locationDTOs.Count} locations.");
            var locationsList = locationDTOs.Select(d => new Location(d.Name, d.Quests.Select(qn => quests.All.First(q => q.Name == qn)))).ToList();
            locations.Load(locationsList);
        } catch (Exception ex) { Console.WriteLine($"Error loading locations: {ex.Message}"); throw; }

        try {
            var cadenceDTOs = await http.GetFromJsonAsync<List<CadenceDTO>>("data/cadences.json") ?? [];
            Console.WriteLine($"Loaded {cadenceDTOs.Count} cadences.");
            var cadencesList = cadenceDTOs.Select(d => new Cadence(d.Name, d.Description, d.Abilities.Select(a => new CadenceUnlock(
                abilities.All.First(ab => ab.Name == a.Ability),
                a.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray()
            )).ToArray())).ToList();
            cadences.Load(cadencesList);
        } catch (Exception ex) { Console.WriteLine($"Error loading cadences: {ex.Message}"); throw; }

        try {
            var detailDTOs = await http.GetFromJsonAsync<List<QuestDetailDTO>>("data/quest_details.json") ?? [];
            Console.WriteLine($"Loaded {detailDTOs.Count} quest details.");
            var detailsDict = detailDTOs.ToDictionary(
                d => quests.All.First(q => q.Name == d.Quest),
                d => new QuestDetail(d.DurationSeconds, 
                    d.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                    d.Rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                    Enum.Parse<QuestType>(d.Type)
                )
            );
            questDetails.Load(detailsDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading quest details: {ex.Message}"); throw; }

        try {
            var unlockDTOs = await http.GetFromJsonAsync<List<QuestUnlockDTO>>("data/quest_unlocks.json") ?? [];
            Console.WriteLine($"Loaded {unlockDTOs.Count} quest unlocks.");
            var unlocksDict = unlockDTOs.ToDictionary(
                d => quests.All.First(q => q.Name == d.Quest),
                d => d.Requires.Select(rn => quests.All.First(q => q.Name == rn)).ToArray()
            );
            questUnlocks.Load(unlocksDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading quest unlocks: {ex.Message}"); throw; }

        try {
            var refinementDTOs = await http.GetFromJsonAsync<List<RefinementDTO>>("data/refinements.json") ?? [];
            Console.WriteLine($"Loaded {refinementDTOs.Count} refinements.");
            var refinementsDict = refinementDTOs.ToDictionary(
                d => abilities.All.First(a => a.Name == d.Ability),
                d => d.Recipes.ToDictionary(
                    r => items.All.First(i => i.Name == r.InputItem),
                    r => new Recipe(r.InputQuantity, items.All.First(i => i.Name == r.OutputItem), r.OutputQuantity)
                )
            );
            refinements.Load(refinementsDict);
        } catch (Exception ex) { Console.WriteLine($"Error loading refinements: {ex.Message}"); throw; }
        
        Console.WriteLine("Content Load Complete.");
    }
}
