using Microsoft.JSInterop;
using Newtonsoft.Json;
using Mythril.Data;
using System.Collections.Immutable;

namespace Mythril.Blazor.Services;

public class PersistenceService(
    IJSRuntime js, 
    ResourceManager resourceManager, 
    JunctionManager junctionManager,
    Items items, 
    Cadences cadences, 
    Quests quests,
    Stats stats,
    ItemRefinements refinements,
    GameStore gameStore)
{
    private const string STORAGE_KEY = "mythril_save_v2";

    public async Task SaveAsync()
    {
        var saveData = new SaveData
        {
            State = gameStore.State,
            LastSaveTime = DateTime.Now,
            // Keep legacy fields for a while or if needed for specific UI views
            Journal = resourceManager.Journal.Select(j => new JournalEntryDTO
            {
                TaskName = j.TaskName,
                CharacterName = j.CharacterName,
                Details = j.Details,
                CompletedAt = j.CompletedAt,
                IsFirstTime = j.IsFirstTime
            }).ToList(),
            HasUnseenCadence = resourceManager.HasUnseenCadence,
            HasUnseenWorkshop = resourceManager.HasUnseenWorkshop
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

        if (saveData.State != null)
        {
            // Calculate bonus time since last save
            double bonusSeconds = (DateTime.Now - saveData.LastSaveTime).TotalSeconds;
            
            var stateWithBonusTime = saveData.State with {
                ActiveQuests = saveData.State.ActiveQuests
                    .Select(q => q with { SecondsElapsed = q.SecondsElapsed + bonusSeconds })
                    .ToImmutableList()
            };

            gameStore.Dispatch(new SetStateAction(stateWithBonusTime));
            
            // Restore non-state fields
            resourceManager.HasUnseenCadence = saveData.HasUnseenCadence;
            resourceManager.HasUnseenWorkshop = saveData.HasUnseenWorkshop;
            
            resourceManager.Journal.Clear();
            if (saveData.Journal != null)
            {
                foreach (var dto in saveData.Journal)
                {
                    resourceManager.Journal.Add(new ResourceManager.JournalEntry(dto.TaskName, dto.CharacterName, dto.Details, dto.CompletedAt, dto.IsFirstTime));
                }
            }

            resourceManager.UpdateUsableLocations();
        }
    }
    
    public async Task<bool> HasSave()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        return !string.IsNullOrEmpty(json);
    }

    public async Task ClearSaveAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
    }
}
