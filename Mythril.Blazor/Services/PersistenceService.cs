using Microsoft.JSInterop;
using Newtonsoft.Json;
using Mythril.Data;

namespace Mythril.Blazor.Services;

public class PersistenceService(
    IJSRuntime js, 
    ResourceManager resourceManager, 
    Items items, 
    Cadences cadences, 
    CadenceAbilities abilities,
    Quests quests)
{
    private const string STORAGE_KEY = "mythril_save_v1";

    public async Task SaveAsync()
    {
        var saveData = new SaveData
        {
            Inventory = resourceManager.Inventory.GetItems()
                .Concat(resourceManager.Inventory.GetSpells())
                .Select(x => new KeyValuePair<string, int>(x.Item.Name, x.Quantity))
                .ToList(),
            UnlockedCadences = resourceManager.UnlockedCadences.Select(c => c.Name).ToList(),
            UnlockedAbilities = resourceManager.UnlockedAbilities.Select(a => a.Name).ToList(),
            ActiveQuests = resourceManager.ActiveQuests.Select(q => new QuestProgressDTO
            {
                ItemName = q.Item is QuestData qd ? qd.Name : (q.Item is CadenceUnlock cu ? cu.Ability.Name : ""),
                ItemType = q.Item is QuestData ? "Quest" : "CadenceUnlock",
                CharacterName = q.Character.Name,
                SecondsElapsed = q.SecondsElapsed
            }).ToList()
        };

        var json = JsonConvert.SerializeObject(saveData);
        await js.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
    }

    public async Task LoadAsync()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        if (string.IsNullOrEmpty(json)) return;

        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        if (saveData == null) return;

        // Restore Inventory
        resourceManager.Inventory.Clear();
        foreach (var kvp in saveData.Inventory)
        {
            var item = items.All.FirstOrDefault(x => x.Name == kvp.Key);
            if (item.Name != null) resourceManager.Inventory.Add(item, kvp.Value);
        }

        // Restore Cadences
        resourceManager.UnlockedCadences.Clear();
        foreach (var name in saveData.UnlockedCadences)
        {
            var cadence = cadences.All.FirstOrDefault(x => x.Name == name);
            if (cadence.Name != null) resourceManager.UnlockCadence(cadence);
        }

        // Restore Abilities
        resourceManager.UnlockedAbilities.Clear();
        foreach (var name in saveData.UnlockedAbilities)
        {
            var ability = abilities.All.FirstOrDefault(x => x.Name == name);
            if (ability.Name != null) resourceManager.UnlockedAbilities.Add(ability);
        }

        // Restore Active Quests
        resourceManager.ActiveQuests.Clear();
        foreach (var dto in saveData.ActiveQuests)
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == dto.CharacterName);
            if (character.Name == null) continue;

            if (dto.ItemType == "Quest")
            {
                var quest = quests.All.FirstOrDefault(q => q.Name == dto.ItemName);
                if (quest.Name != null)
                {
                    var detail = resourceManager.GetQuestDetails(quest);
                    var qp = new QuestProgress(new QuestData(quest, detail), quest.Description, detail.DurationSeconds, character)
                    {
                        SecondsElapsed = dto.SecondsElapsed
                    };
                    resourceManager.ActiveQuests.Add(qp);
                }
            }
            else if (dto.ItemType == "CadenceUnlock")
            {
                var cadence = cadences.All.FirstOrDefault(c => c.Abilities.Any(a => a.Ability.Name == dto.ItemName));
                var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == dto.ItemName);
                if (unlock.Ability.Name != null)
                {
                    var qp = new QuestProgress(unlock, unlock.Ability.Description, 3, character)
                    {
                        SecondsElapsed = dto.SecondsElapsed
                    };
                    resourceManager.ActiveQuests.Add(qp);
                }
            }
        }
    }
    
    public async Task<bool> HasSave()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        return !string.IsNullOrEmpty(json);
    }
}
