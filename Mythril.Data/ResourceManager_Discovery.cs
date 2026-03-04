namespace Mythril.Data;

public partial class ResourceManager
{
    public void UpdateUsableLocations()
    {
        UsableLocations = [.. _locations.All
            .Where(l => string.IsNullOrEmpty(l.RequiredQuest) || UnlockedLocationNames.Contains(l.Name) || _completedQuests.Any(q => q.Name == l.RequiredQuest))
            .Select(x => new LocationData(x, x.Quests.Where(IsQuestUnlocked)))];
        
        foreach(var location in UsableLocations)
        {
            if (!string.IsNullOrEmpty(location.Name))
                UnlockedLocationNames.Add(location.Name);
        }
    }

    public bool IsQuestUnlocked(Quest quest)
    {
        if ((_questDetails[quest].Type == QuestType.Single || _questDetails[quest].Type == QuestType.Unlock) && _completedQuests.Contains(quest))
            return false;
            
        return IsComplete(_questUnlocks[quest]);
    }

    public bool CanAutoQuest(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        return assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "AutoQuest I"));
    }

    public bool IsAutoQuestEnabled(Character character) => _autoQuestEnabled.TryGetValue(character.Name, out var enabled) && enabled;

    public void SetAutoQuestEnabled(Character character, bool enabled) => _autoQuestEnabled[character.Name] = enabled;

    public void UpdateAvaiableCadences()
    {
        UnlockedCadences = [.. _lockedCadences.Where(x => !x.Value).Select(x => x.Key)];
        UnlockedCadenceNames = [.. UnlockedCadences.Select(c => c.Name)];
        Console.WriteLine($"Unlocked Cadences Updated: {string.Join(", ", UnlockedCadenceNames)}");
    }

    public void UpdateMagicCapacity()
    {
        int capacity = 30;
        if (UnlockedAbilities.Any(a => a.EndsWith(":Magic Pocket I"))) capacity = 60;
        if (UnlockedAbilities.Any(a => a.EndsWith(":Magic Pocket II"))) capacity = 100;
        Inventory.MagicCapacity = capacity;
    }

    public int GetTaskLimit(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        if (assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "Logistics I")))
            return 2;
        return 1;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        if (ActiveTab != "cadence")
        {
            HasUnseenCadence = true;
        }
        UpdateAvaiableCadences();
    }

    public bool HasAbility(Character character, CadenceAbility ability)
    {
        return JunctionManager.CurrentlyAssigned(character).Any(cad => 
            cad.Abilities.Any(a => a.Ability.Name == ability.Name && UnlockedAbilities.Contains($"{cad.Name}:{a.Ability.Name}"))
        );
    }
}
