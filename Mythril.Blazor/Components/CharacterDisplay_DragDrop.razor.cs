using Microsoft.AspNetCore.Components;
using Mythril.Data;
using Mythril.Blazor.Services;

namespace Mythril.Blazor.Components;

public partial class CharacterDisplay
{
    private int GetPredictionDelta(Stat stat)
    {
        if (DragDropService.HoveredTarget?.character.Name != Character.Name || DragDropService.HoveredTarget?.stat.Name != stat.Name)
            return 0;

        Item? draggedMagic = null;
        if (DragDropService.Data is ItemQuantity iq && iq.Item.ItemType == ItemType.Spell) draggedMagic = iq.Item;
        if (DragDropService.Data is Item i && i.ItemType == ItemType.Spell) draggedMagic = i;

        if (draggedMagic == null || !CanJunction(stat))
            return 0;

        Item activeMagic = (Item)draggedMagic;
        int currentVal = JunctionManager.GetStatValue(Character, stat.Name);
        
        int newVal = 10;
        int qty = resourceManager.Inventory.GetQuantity(activeMagic);
        var augments = ContentHost.GetContent<StatAugments>()[activeMagic];
        var augment = augments.FirstOrDefault(a => a.Stat.Name == stat.Name);
        if (augment.Stat.Name != null)
        {
            newVal += (int)(qty * (augment.ModifierAtFull / 100.0));
        }
        else
        {
            newVal += qty / 10;
        }

        newVal = Math.Min(255, newVal);
        return newVal - currentVal;
    }

    private void HandleStatDragStart(Stat stat, Junction? junction)
    {
        if (junction != null) DragDropService.Data = junction;
    }

    private void HandleStatDragEnter(Stat stat)
    {
        bool isValid = false;
        if (DragDropService.Data is ItemQuantity iq && iq.Item.ItemType == ItemType.Spell) isValid = true;
        if (DragDropService.Data is Item i && i.ItemType == ItemType.Spell) isValid = true;
        if (DragDropService.Data is Junction) isValid = true;

        if (isValid && CanJunction(stat))
        {
            DragDropService.SetHoveredTarget(Character, stat);
        }
    }

    private void HandleStatDragLeave() => DragDropService.ClearHoveredTarget();

    private void HandleStatDrop(Stat stat)
    {
        if (!CanJunction(stat)) return;

        if (DragDropService.Data is Junction sourceJunction)
        {
            var sourceChar = sourceJunction.Character;
            var sourceStat = sourceJunction.Stat;
            var sourceMagic = sourceJunction.Magic;

            var targetJunction = JunctionManager.Junctions.FirstOrDefault(j => j.Character.Name == Character.Name && j.Stat.Name == stat.Name);
            var targetMagic = targetJunction?.Magic ?? new Item();

            JunctionManager.JunctionMagic(Character, stat, sourceMagic, resourceManager.UnlockedAbilities);
            JunctionManager.JunctionMagic(sourceChar, sourceStat, targetMagic, resourceManager.UnlockedAbilities);

            SnackbarService.Show($"Swapped {sourceMagic.Name} and {targetMagic.Name ?? "None"}", "info");
        }
        else
        {
            Item? droppedMagic = null;
            if (DragDropService.Data is ItemQuantity iq && iq.Item.ItemType == ItemType.Spell) droppedMagic = iq.Item;
            if (DragDropService.Data is Item i && i.ItemType == ItemType.Spell) droppedMagic = i;

            if (droppedMagic != null)
            {
                JunctionManager.JunctionMagic(Character, stat, (Item)droppedMagic, resourceManager.UnlockedAbilities);
            }
        }

        DragDropService.ClearHoveredTarget();
        DragDropService.Data = null;
    }

    private void HandleTaskDrop(object task) => OnQuestDrop.InvokeAsync(task);
}
