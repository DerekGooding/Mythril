using Microsoft.AspNetCore.Components;
using Mythril.Data;
using Mythril.Blazor.Services;

namespace Mythril.Blazor.Components;

public partial class CharacterDisplay
{
    private void ToggleJunctionMenu() 
    {
        _showJunctionMenu = !_showJunctionMenu;
        if (_showJunctionMenu) _isRemovalMode = false;
    }

    private void ToggleRemovalMode()
    {
        _isRemovalMode = !_isRemovalMode;
        if (_isRemovalMode) _showJunctionMenu = false;
    }

    private void ClearJunction(Stat stat)
    {
        JunctionManager.JunctionMagic(Character, stat, new Item(), resourceManager.UnlockedAbilities);
        StateHasChanged();
    }

    private void ToggleAutoQuest() => resourceManager.ToggleAutoQuest(Character);

    private void CancelQuest(QuestProgress progress) => resourceManager.CancelQuest(progress);

    private bool CanJunction(Stat stat)
    {
        var assigned = JunctionManager.CurrentlyAssigned(Character);
        string abilityName = stat.Name switch {
            "Vitality" => "J-Vit", "Strength" => "J-Str", "Speed" => "J-Speed", "Magic" => "J-Magic", _ => "J-" + stat.Name
        };
        return assigned.Any(c => c.Abilities.Any(a => a.Ability.Name == abilityName && resourceManager.UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}")));
    }

    private void HandleJunctionChange(Stat stat, ChangeEventArgs e)
    {
        string magicName = e.Value?.ToString() ?? "";
        var magic = resourceManager.Inventory.GetSpells().FirstOrDefault(s => s.Item.Name == magicName).Item;
        JunctionManager.JunctionMagic(Character, stat, magic, resourceManager.UnlockedAbilities);
        StateHasChanged();
    }

    private void HandleDropFailed(object data)
    {
        if (data is RefinementData refinement)
        {
            if (!resourceManager.HasAbility(Character, refinement.Ability))
                SnackbarService.Show($"{Character.Name} lacks the '{refinement.Ability.Name}' ability.", "warning");
            else if (!resourceManager.Inventory.Has(refinement.InputItem, refinement.Recipe.InputQuantity))
                SnackbarService.Show($"Insufficient materials: {refinement.Recipe.InputQuantity}x {refinement.InputItem.Name} required.", "warning");
        }
        else if (data is QuestData quest)
        {
            foreach (var req in quest.Requirements)
                if (!resourceManager.Inventory.Has(req.Item, req.Quantity))
                { SnackbarService.Show($"Insufficient materials: {req.Quantity}x {req.Item.Name} required.", "warning"); return; }

            if (quest.RequiredStats != null)
                foreach (var req in quest.RequiredStats)
                    if (JunctionManager.GetStatValue(Character, req.Key) < req.Value)
                    { SnackbarService.Show($"{Character.Name} needs {req.Value} {req.Key} (Current: {JunctionManager.GetStatValue(Character, req.Key)}).", "warning"); return; }
        }
        else if (data is CadenceUnlock unlock)
        {
            foreach (var req in unlock.Requirements)
                if (!resourceManager.Inventory.Has(req.Item, req.Quantity))
                { SnackbarService.Show($"Insufficient materials: {req.Quantity}x {req.Item.Name} required.", "warning"); return; }
        }
    }
}
