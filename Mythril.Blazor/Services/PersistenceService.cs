using Microsoft.JSInterop;
using Newtonsoft.Json;
using Mythril.Data;

namespace Mythril.Blazor.Services;

public class PersistenceService(IJSRuntime js, ResourceManager resourceManager, Items items, Cadences cadences, CadenceAbilities abilities)
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
            UnlockedAbilities = resourceManager.UnlockedAbilities.Select(a => a.Name).ToList()
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
            if (item.Name != null)
            {
                resourceManager.Inventory.Add(item, kvp.Value);
            }
        }

        // Restore Cadences
        resourceManager.UnlockedCadences.Clear();
        foreach (var name in saveData.UnlockedCadences)
        {
            var cadence = cadences.All.FirstOrDefault(x => x.Name == name);
            if (cadence.Name != null)
            {
                resourceManager.UnlockCadence(cadence);
            }
        }

        // Restore Abilities
        resourceManager.UnlockedAbilities.Clear();
        foreach (var name in saveData.UnlockedAbilities)
        {
            var ability = abilities.All.FirstOrDefault(x => x.Name == name);
            if (ability.Name != null)
            {
                resourceManager.UnlockedAbilities.Add(ability);
            }
        }
    }
    
    public async Task<bool> HasSave()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        return !string.IsNullOrEmpty(json);
    }
}
