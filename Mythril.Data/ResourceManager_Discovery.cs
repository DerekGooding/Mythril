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

    public Location GetLocation(string name) => _locations.All.FirstOrDefault(l => l.Name == name);

    public bool IsQuestUnlocked(Quest quest)
    {
        if ((_questDetails[quest].Type == QuestType.Single || _questDetails[quest].Type == QuestType.Unlock) && _completedQuests.Contains(quest))
            return false;
            
        return IsComplete(_questUnlocks[quest]);
    }

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

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        if (ActiveTab != "cadence")
        {
            HasUnseenCadence = true;
        }
        UpdateAvaiableCadences();
    }

    public void CheckHiddenCadences()
    {
        foreach (var character in Characters)
        {
            // Geologist: 60 STR
            if (JunctionManager.GetStatValue(character, "Strength") >= 60)
                UnlockByName("Geologist");
            
            // Tide-Caller: 60 SPD
            if (JunctionManager.GetStatValue(character, "Speed") >= 60)
                UnlockByName("Tide-Caller");

            // The Sentinel: 60 VIT
            if (JunctionManager.GetStatValue(character, "Vitality") >= 60)
                UnlockByName("The Sentinel");

            // Scholar: 100 MAG
            if (JunctionManager.GetStatValue(character, "Magic") >= 100)
                UnlockByName("Scholar");
            
            // Slayer: 100 STR AND 100 SPD
            if (JunctionManager.GetStatValue(character, "Strength") >= 100 && JunctionManager.GetStatValue(character, "Speed") >= 100)
                UnlockByName("Slayer");
        }
    }

    private void UnlockByName(string name)
    {
        var cadence = _cadences.All.FirstOrDefault(c => c.Name == name);
        if (cadence.Name != null && _lockedCadences.TryGetValue(cadence, out var isLocked) && isLocked)
        {
            UnlockCadence(cadence);
            Console.WriteLine($"Stat-based unlock: {name}");
        }
    }

    public bool HasAbility(Character character, CadenceAbility ability)
    {
        return JunctionManager.CurrentlyAssigned(character).Any(cad => 
            cad.Abilities.Any(a => a.Ability.Name == ability.Name && UnlockedAbilities.Contains($"{cad.Name}:{a.Ability.Name}"))
        );
    }
}
