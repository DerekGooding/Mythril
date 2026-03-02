using Microsoft.JSInterop;
using Newtonsoft.Json;
using Mythril.Data;

namespace Mythril.Blazor.Services;

public class PersistenceService(
    IJSRuntime js, 
    ResourceManager resourceManager, 
    JunctionManager junctionManager,
    Items items, 
    Cadences cadences, 
    CadenceAbilities abilities,
    Quests quests,
    Stats stats)
{
    private const string STORAGE_KEY = "mythril_save_v1";

    public async Task SaveAsync()
    {
        var saveData = new SaveData
        {
            LastSaveTime = DateTime.Now,
            Inventory = resourceManager.Inventory.GetItems()
                .Concat(resourceManager.Inventory.GetSpells())
                .Select(x => new KeyValuePair<string, int>(x.Item.Name, x.Quantity))
                .ToList(),
            UnlockedCadences = resourceManager.UnlockedCadences.Select(c => c.Name).ToList(),
            UnlockedAbilities = resourceManager.UnlockedAbilities.Select(a => a.Name).ToList(),
            CompletedQuests = resourceManager.GetCompletedQuests().Select(q => q.Name).ToList(),
            Junctions = junctionManager.Junctions.Select(j => new JunctionDTO
            {
                CharacterName = j.Character.Name,
                StatName = j.Stat.Name,
                MagicName = j.Magic.Name
            }).ToList(),
            AssignedCadences = resourceManager.Characters.Select(c => new { Char = c, Cad = junctionManager.CurrentlyAssigned(c).FirstOrDefault() })
                .Where(x => x.Cad.Name != null)
                .Select(x => new AssignedCadenceDTO { CharacterName = x.Char.Name, CadenceName = x.Cad.Name })
                .ToList(),
            ActiveQuests = resourceManager.ActiveQuests.Select(q => new QuestProgressDTO
            {
                ItemName = q.Item is QuestData qd ? qd.Name : (q.Item is CadenceUnlock cu ? cu.Ability.Name : ""),
                ItemType = q.Item is QuestData ? "Quest" : "CadenceUnlock",
                CharacterName = q.Character.Name,
                SecondsElapsed = q.SecondsElapsed,
                StartTime = q.StartTime
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
        // Ensure Test is always unlocked
        var testCadence = cadences.All.FirstOrDefault(c => c.Name == "Test");
        if (testCadence.Name != null) resourceManager.UnlockCadence(testCadence);

        // Restore Abilities
        resourceManager.UnlockedAbilities.Clear();
        foreach (var name in saveData.UnlockedAbilities)
        {
            var ability = abilities.All.FirstOrDefault(x => x.Name == name);
            if (ability.Name != null) resourceManager.UnlockedAbilities.Add(ability);
        }
        resourceManager.UpdateMagicCapacity();

        // Restore Completed Quests
        resourceManager.ClearCompletedQuests();
        foreach(var name in saveData.CompletedQuests)
        {
            var quest = quests.All.FirstOrDefault(q => q.Name == name);
            if (quest.Name != null) resourceManager.RestoreCompletedQuest(quest);
        }

        // Restore Assignments
        junctionManager.Initialize(); 
        foreach(var dto in saveData.AssignedCadences)
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == dto.CharacterName);
            var cadence = cadences.All.FirstOrDefault(c => c.Name == dto.CadenceName);
            if (character.Name != null && cadence.Name != null)
            {
                junctionManager.RestoreAssignment(cadence, character);
            }
        }

        // Restore Junctions
        junctionManager.Junctions.Clear();
        foreach(var dto in saveData.Junctions)
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == dto.CharacterName);
            var stat = stats.All.FirstOrDefault(s => s.Name == dto.StatName);
            var magic = items.All.FirstOrDefault(i => i.Name == dto.MagicName);
            if (character.Name != null && stat.Name != null && magic.Name != null)
            {
                junctionManager.Junctions.Add(new Junction(character, stat, magic));
            }
        }

        // Restore Active Quests
        resourceManager.ActiveQuests.Clear();
        double bonusSeconds = (DateTime.Now - saveData.LastSaveTime).TotalSeconds;
        foreach (var dto in saveData.ActiveQuests)
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == dto.CharacterName);
            if (character.Name == null) continue;

            QuestProgress? qp = null;
            if (dto.ItemType == "Quest")
            {
                var quest = quests.All.FirstOrDefault(q => q.Name == dto.ItemName);
                if (quest.Name != null)
                {
                    var detail = resourceManager.GetQuestDetails(quest);
                    qp = new QuestProgress(new QuestData(quest, detail), quest.Description, detail.DurationSeconds, character);
                }
            }
            else if (dto.ItemType == "CadenceUnlock")
            {
                var cadence = cadences.All.FirstOrDefault(c => c.Abilities.Any(a => a.Ability.Name == dto.ItemName));
                var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == dto.ItemName);
                if (unlock.Ability.Name != null)
                {
                    qp = new QuestProgress(unlock, unlock.Ability.Description, 10, character);
                }
            }

            if (qp != null)
            {
                qp.SecondsElapsed = dto.SecondsElapsed + bonusSeconds;
                qp.StartTime = dto.StartTime;
                resourceManager.ActiveQuests.Add(qp);
            }
        }
    }
    
    public async Task<bool> HasSave()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        return !string.IsNullOrEmpty(json);
    }
}
