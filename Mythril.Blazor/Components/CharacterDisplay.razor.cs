using Microsoft.AspNetCore.Components;
using Mythril.Data;
using Mythril.Blazor.Services;

namespace Mythril.Blazor.Components;

public partial class CharacterDisplay : IDisposable
{
    [Inject]
    public Stats statsContent { get; set; } = null!;

    [Inject]
    public ResourceManager resourceManager { get; set; } = null!;

    [Inject]
    public JunctionManager JunctionManager { get; set; } = null!;

    [Inject]
    public DragDropService DragDropService { get; set; } = null!;

    [Inject]
    public SnackbarService SnackbarService { get; set; } = null!;

    [Parameter]
    public Character Character { get; set; }

    [Parameter]
    public IEnumerable<QuestProgress> QuestProgresses { get; set; } = [];

    [Parameter]
    public EventCallback<object> OnQuestDrop { get; set; }

    [Parameter]
    public Func<object, bool>? Accepts { get; set; }

    [Parameter]
    public EventCallback<QuestProgress> OnCompletionAnimationEnd { get; set; }

    [Parameter]
    public EventCallback<Cadence> OnUnequip { get; set; }

    private bool _showJunctionMenu = false;

    protected override void OnInitialized()
    {
        DragDropService.OnHoverChanged += HandleHoverChanged;
    }

    public void Dispose()
    {
        DragDropService.OnHoverChanged -= HandleHoverChanged;
    }

    private void HandleHoverChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private int GetPredictionDelta(Stat stat)
    {
        if (DragDropService.HoveredTarget?.character.Name != Character.Name || DragDropService.HoveredTarget?.stat.Name != stat.Name)
            return 0;

        if (DragDropService.Data is not ItemQuantity spell || spell.Item.ItemType != ItemType.Spell)
            return 0;

        if (!CanJunction(stat))
            return 0;

        int currentVal = JunctionManager.GetStatValue(Character, stat.Name);
        
        // Calculate what it WOULD be
        int newVal = 10;
        int qty = resourceManager.Inventory.GetQuantity(spell.Item);
        var augments = ContentHost.GetContent<StatAugments>()[spell.Item];
        var augment = augments.FirstOrDefault(a => a.Stat.Name == stat.Name);
        if (augment.Stat.Name != null)
        {
            newVal += (int)(qty * (augment.ModifierAtFull / 100.0));
        }
        else
        {
            newVal += qty / 10;
        }

        return newVal - currentVal;
    }

    private void HandleStatDragEnter(Stat stat)
    {
        if (DragDropService.Data is ItemQuantity spell && spell.Item.ItemType == ItemType.Spell && CanJunction(stat))
        {
            DragDropService.SetHoveredTarget(Character, stat);
        }
    }

    private void HandleStatDragLeave()
    {
        DragDropService.ClearHoveredTarget();
    }

    private void HandleStatDrop(Stat stat)
    {
        if (DragDropService.Data is ItemQuantity spell && spell.Item.ItemType == ItemType.Spell && CanJunction(stat))
        {
            JunctionManager.JunctionMagic(Character, stat, spell.Item, resourceManager.UnlockedAbilities);
            DragDropService.ClearHoveredTarget();
            DragDropService.Data = null;
        }
    }

    private void ToggleJunctionMenu() => _showJunctionMenu = !_showJunctionMenu;

    private void ToggleAutoQuest()
    {
        resourceManager.SetAutoQuestEnabled(Character, !resourceManager.IsAutoQuestEnabled(Character));
    }

    private void CancelQuest(QuestProgress progress)
    {
        resourceManager.CancelQuest(progress);
    }

    private bool CanJunction(Stat stat)
    {
        var assigned = JunctionManager.CurrentlyAssigned(Character);
        
        string abilityName = "J-" + stat.Name;
        if (stat.Name == "Vitality") abilityName = "J-Vit";
        if (stat.Name == "Strength") abilityName = "J-Str";
        if (stat.Name == "Speed") abilityName = "J-Speed";
        if (stat.Name == "Magic") abilityName = "J-Magic";

        return assigned.Any(c => c.Abilities.Any(a => a.Ability.Name == abilityName && resourceManager.UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}")));
    }

    private void HandleJunctionChange(Stat stat, ChangeEventArgs e)
    {
        string magicName = e.Value?.ToString() ?? "";
        var magic = resourceManager.Inventory.GetSpells().FirstOrDefault(s => s.Item.Name == magicName).Item;
        JunctionManager.JunctionMagic(Character, stat, magic, resourceManager.UnlockedAbilities);
        StateHasChanged();
    }

    private void HandleTaskDrop(object task)
    {
        OnQuestDrop.InvokeAsync(task);
    }

    private void HandleDropFailed(object data)
    {
        if (data is RefinementData refinement)
        {
            if (!resourceManager.HasAbility(Character, refinement.Ability))
            {
                SnackbarService.Show($"{Character.Name} lacks the '{refinement.Ability.Name}' ability.", "warning");
            }
            else if (!resourceManager.Inventory.Has(refinement.InputItem, refinement.Recipe.InputQuantity))
            {
                SnackbarService.Show($"Insufficient materials: {refinement.Recipe.InputQuantity}x {refinement.InputItem.Name} required.", "warning");
            }
        }
        else if (data is QuestData quest)
        {
            foreach (var req in quest.Requirements)
            {
                if (!resourceManager.Inventory.Has(req.Item, req.Quantity))
                {
                    SnackbarService.Show($"Insufficient materials: {req.Quantity}x {req.Item.Name} required.", "warning");
                    return;
                }
            }

            if (quest.RequiredStats != null)
            {
                foreach (var req in quest.RequiredStats)
                {
                    if (JunctionManager.GetStatValue(Character, req.Key) < req.Value)
                    {
                        SnackbarService.Show($"{Character.Name} needs {req.Value} {req.Key} (Current: {JunctionManager.GetStatValue(Character, req.Key)}).", "warning");
                        return;
                    }
                }
            }
        }
        else if (data is CadenceUnlock unlock)
        {
            foreach (var req in unlock.Requirements)
            {
                if (!resourceManager.Inventory.Has(req.Item, req.Quantity))
                {
                    SnackbarService.Show($"Insufficient materials: {req.Quantity}x {req.Item.Name} required.", "warning");
                    return;
                }
            }
        }
    }
}
