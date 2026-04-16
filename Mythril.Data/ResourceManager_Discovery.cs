namespace Mythril.Data;

public partial class ResourceManager
{
    public void UpdateUsableLocations()
    {
        var usable = UsableLocations;
        
        foreach (var loc in usable)
        {
            if (!UnlockedLocationNames.Contains(loc.Name))
            {
                _gameStore.Dispatch(new UnlockLocationAction(loc.Name));
            }
        }
    }

    public void CheckHiddenCadences()
    {
        // Recruit is always unlocked by Prologue completion
        
        // Potential Cadence: Arcanist (Strength + Speed > 50) - Logic from original
        // Let's use the actual thresholds from simulation logic if they exist
        // Currently they are hardcoded in RoutedSimulator.UpdateStats
        
        // I'll keep them here for runtime discovery
        foreach (var character in Characters)
        {
            if (JunctionManager.GetStatValue(character, "Strength") >= 60) UnlockCadenceByName("Geologist");
            if (JunctionManager.GetStatValue(character, "Speed") >= 60) UnlockCadenceByName("Tide-Caller");
            if (JunctionManager.GetStatValue(character, "Vitality") >= 60) UnlockCadenceByName("The Sentinel");
            if (JunctionManager.GetStatValue(character, "Magic") >= 100) UnlockCadenceByName("Scholar");
            if (JunctionManager.GetStatValue(character, "Strength") >= 100 && JunctionManager.GetStatValue(character, "Speed") >= 100) UnlockCadenceByName("Slayer");
        }
    }

    private void UnlockCadenceByName(string name)
    {
        var cadence = _cadences.All.FirstOrDefault(c => c.Name == name);
        if (cadence.Name != null) UnlockCadence(cadence);
    }

    public void UnlockAbility(string cadenceName, string abilityName)
    {
        string key = $"{cadenceName}:{abilityName}";
        if (!UnlockedAbilities.Contains(key))
        {
            _gameStore.Dispatch(new UnlockAbilityAction(key));
        }
    }

    public bool HasAbility(Character character, CadenceAbility ability)
    {
        return JunctionManager.CurrentlyAssigned(character).Any(cad => 
            cad.Abilities.Any(a => a.Ability.Name == ability.Name && UnlockedAbilities.Contains($"{cad.Name}:{a.Ability.Name}"))
        );
    }
}
