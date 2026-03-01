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
    QuestToCadenceUnlocks questToCadenceUnlocks,
    StatAugments statAugments,
    AbilityAugments abilityAugments)
{
    public async Task LoadAllAsync()
    {
        // 1. Independent Content
        var itemsList = await http.GetFromJsonAsync<List<Item>>("data/items.json") ?? [];
        items.Load(itemsList);

        var statsList = await http.GetFromJsonAsync<List<Stat>>("data/stats.json") ?? [];
        stats.Load(statsList);

        var abilitiesList = await http.GetFromJsonAsync<List<CadenceAbility>>("data/cadence_abilities.json") ?? [];
        abilities.Load(abilitiesList);

        var questsList = await http.GetFromJsonAsync<List<Quest>>("data/quests.json") ?? [];
        quests.Load(questsList);

        // 2. Dependent Content (requires basic types)
        var locationDTOs = await http.GetFromJsonAsync<List<LocationDTO>>("data/locations.json") ?? [];
        var locationsList = locationDTOs.Select(d => new Location(d.Name, d.Quests.Select(qn => quests.All.First(q => q.Name == qn)))).ToList();
        locations.Load(locationsList);

        var cadenceDTOs = await http.GetFromJsonAsync<List<CadenceDTO>>("data/cadences.json") ?? [];
        var cadencesList = cadenceDTOs.Select(d => new Cadence(d.Name, d.Description, d.Abilities.Select(a => new CadenceUnlock(
            abilities.All.First(ab => ab.Name == a.Ability),
            a.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray()
        )).ToArray())).ToList();
        cadences.Load(cadencesList);

        var detailDTOs = await http.GetFromJsonAsync<List<QuestDetailDTO>>("data/quest_details.json") ?? [];
        var detailsDict = detailDTOs.ToDictionary(
            d => quests.All.First(q => q.Name == d.Quest),
            d => new QuestDetail(d.DurationSeconds, 
                d.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                d.Rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                Enum.Parse<QuestType>(d.Type)
            )
        );
        questDetails.Load(detailsDict);

        var unlockDTOs = await http.GetFromJsonAsync<List<QuestUnlockDTO>>("data/quest_unlocks.json") ?? [];
        var unlocksDict = unlockDTOs.ToDictionary(
            d => quests.All.First(q => q.Name == d.Quest),
            d => d.Requires.Select(rn => quests.All.First(q => q.Name == rn)).ToArray()
        );
        questUnlocks.Load(unlocksDict);

        var refinementDTOs = await http.GetFromJsonAsync<List<RefinementDTO>>("data/refinements.json") ?? [];
        var refinementsDict = refinementDTOs.ToDictionary(
            d => abilities.All.First(a => a.Name == d.Ability),
            d => d.Recipes.ToDictionary(
                r => items.All.First(i => i.Name == r.InputItem),
                r => new Recipe(r.InputQuantity, items.All.First(i => i.Name == r.OutputItem), r.OutputQuantity)
            )
        );
        refinements.Load(refinementsDict);
        
        // Populate default QuestToCadence mapping (this could also be a JSON if needed)
        // For now, let's just hardcode the one link we had or leave it empty if no JSON
        // Actually, let's keep it simple and just have the major ones loaded.
    }
}
