using Microsoft.JSInterop;
using Mythril.Data;
using System.Collections.Immutable;
using System.Text.Json;

namespace Mythril.Blazor.Services;

public class PersistenceService(
    IJSRuntime js,
    ResourceManager resourceManager,
    GameStore gameStore)
{
    private const string STORAGE_KEY = "mythril_save_v3"; // Bump version since format changed

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task SaveAsync()
    {
        var state = gameStore.State;
        var saveData = new SaveData
        {
            Inventory = state.Inventory.ToDictionary(k => k.Key, v => v.Value),
            MagicCapacity = state.MagicCapacity,
            PinnedItems = [.. state.PinnedItems],
            UnlockedCadences = [.. state.UnlockedCadenceNames],
            UnlockedAbilities = [.. state.UnlockedAbilities],
            CompletedQuests = [.. state.CompletedQuests],
            ActiveQuests = [.. state.ActiveQuests.Select(q => new QuestProgressDTO {
                ItemName = q.Name,
                AbilityName = q.Item is CadenceUnlock cu ? cu.CadenceName : "",
                ItemType = q.Item is QuestData ? "Quest" : (q.Item is CadenceUnlock ? "CadenceUnlock" : "Refinement"),
                CharacterName = q.Character.Name,
                SecondsElapsed = q.SecondsElapsed,
                StartTime = q.StartTime,
                SlotIndex = q.SlotIndex,
                Description = q.Description,
                DurationSeconds = q.DurationSeconds
            })],
            Junctions = [.. state.Junctions.Select(j => new JunctionDTO {
                CharacterName = j.Character.Name,
                StatName = j.Stat.Name,
                MagicName = j.Magic.Name
            })],
            AssignedCadences = [.. state.AssignedCadences.Where(kvp => kvp.Value != null).Select(kvp => new AssignedCadenceDTO {
                CadenceName = kvp.Key,
                CharacterName = kvp.Value!
            })],
            AutoQuestEnabled = state.AutoQuestEnabled.ToDictionary(k => k.Key, v => v.Value),
            UnlockedLocations = [.. state.UnlockedLocationNames],
            StarredRecipes = [.. state.StarredRecipes],
            SeenContent = [.. state.SeenContent],
            HasUnseenCadence = state.HasUnseenCadence,
            HasUnseenWorkshop = state.HasUnseenWorkshop,
            CurrentTime = state.CurrentTime,
            IsTestMode = state.IsTestMode,
            ActiveTab = state.ActiveTab,
            LastSaveTime = DateTime.Now,
            CharacterStatBoosts = state.CharacterPermanentStatBoosts.ToDictionary(k => k.Key, v => v.Value.ToDictionary(ik => ik.Key, iv => iv.Value))
        };

        var json = JsonSerializer.Serialize(saveData, _options);
        await js.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
    }

    public async Task LoadAsync()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        if (string.IsNullOrEmpty(json)) return;

        SaveData? saveData = null;
        try
        {
            saveData = JsonSerializer.Deserialize<SaveData>(json, _options);
        }
        catch
        {
            return;
        }

        if (saveData == null) return;

        // Reconstruct GameState from DTOs
        var quests = ContentHost.GetContent<Quests>();
        var details = ContentHost.GetContent<QuestDetails>();
        var cadences = ContentHost.GetContent<Cadences>();
        var items = ContentHost.GetContent<Items>();
        var stats = ContentHost.GetContent<Stats>();
        var refinements = ContentHost.GetContent<ItemRefinements>();

        var inventory = saveData.Inventory.ToImmutableDictionary();
        var pinnedItems = saveData.PinnedItems.ToImmutableHashSet();

        var junctions = saveData.Junctions.Select(j =>
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == j.CharacterName);
            var stat = stats.All.FirstOrDefault(s => s.Name == j.StatName);
            var magic = items.All.FirstOrDefault(i => i.Name == j.MagicName);
            if (character.Name == null || stat.Name == null || magic.Name == null) return null;
            return new Junction(character, stat, magic);
        }).Where(j => j != null).Select(j => j!).ToImmutableList();

        var activeQuests = saveData.ActiveQuests.Select(dto =>
        {
            var character = resourceManager.Characters.FirstOrDefault(c => c.Name == dto.CharacterName);
            if (character.Name == null) return null;

            object? item = dto.ItemType switch
            {
                "Quest" => quests.All.FirstOrDefault(q => q.Name == dto.ItemName) is var q && q.Name != null ? new QuestData(q, details[q]) : null,
                "CadenceUnlock" => cadences.All.SelectMany(c => c.Abilities).FirstOrDefault(u => u.Ability.Name == dto.ItemName && u.CadenceName == dto.AbilityName),
                "Refinement" => refinements.ByKey.SelectMany(kvp => kvp.Value.Recipes.Select(r => new RefinementData(kvp.Key, r.Key, r.Value, kvp.Value.PrimaryStat))).FirstOrDefault(r => r.Name == dto.ItemName),
                _ => null
            };
            if (item == null) return null;
            return new QuestProgress(item, dto.Description, dto.DurationSeconds, character, dto.SlotIndex)
            {
                StartTime = dto.StartTime,
                SecondsElapsed = dto.SecondsElapsed
            };
        }).Where(q => q != null).Select(q => q!).ToImmutableList();

        // Calculate bonus time since last save
        var bonusSeconds = (DateTime.Now - saveData.LastSaveTime).TotalSeconds;
        var activeQuestsWithBonus = activeQuests.Select(q => q with { SecondsElapsed = q.SecondsElapsed + Math.Max(0, bonusSeconds) }).ToImmutableList();

        var state = new GameState(
            Inventory: inventory,
            MagicCapacity: saveData.MagicCapacity,
            PinnedItems: pinnedItems,
            AssignedCadences: saveData.AssignedCadences.ToImmutableDictionary(x => x.CadenceName, x => (string?)x.CharacterName),
            Junctions: junctions,
            CharacterPermanentStatBoosts: saveData.CharacterStatBoosts.ToImmutableDictionary(k => k.Key, v => v.Value.ToImmutableDictionary()),
            CompletedQuests: [.. saveData.CompletedQuests],
            UnlockedAbilities: [.. saveData.UnlockedAbilities],
            ActiveQuests: activeQuestsWithBonus,
            AutoQuestEnabled: saveData.AutoQuestEnabled.ToImmutableDictionary(),
            StarredRecipes: [.. saveData.StarredRecipes],
            UnlockedLocationNames: [.. saveData.UnlockedLocations],
            UnlockedCadenceNames: [.. saveData.UnlockedCadences],
            LastFinishedActivity: saveData.LastFinishedActivities.ToImmutableDictionary(),
            HighlightedPath: [],
            EverPerformedActivities: [.. saveData.EverPerformedActivities],
            SeenContent: [.. saveData.SeenContent],
            CurrentTime: saveData.CurrentTime,
            IsTestMode: saveData.IsTestMode,
            HasUnseenCadence: saveData.HasUnseenCadence,
            HasUnseenWorkshop: saveData.HasUnseenWorkshop,
            ActiveTab: saveData.ActiveTab
        );

        gameStore.Dispatch(new SetStateAction(state));
        resourceManager.UpdateUsableLocations();
    }

    public async Task<bool> HasSave()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        return !string.IsNullOrEmpty(json);
    }

    public async Task ClearSaveAsync() => await js.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
}