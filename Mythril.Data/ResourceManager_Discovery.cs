namespace Mythril.Data;

public partial class ResourceManager
{
    public void UpdateUsableLocations()
    {
        var usable = _locations.All
            .Where(l => string.IsNullOrEmpty(l.RequiredQuest) || _gameStore.State.CompletedQuests.Contains(l.RequiredQuest))
            .Select(l => new LocationData(
                l.Name,
                l.Quests.Where(q => _questUnlocks[q].All(req => _gameStore.State.CompletedQuests.Contains(req.Name))),
                l.Quests.Where(q => !_questUnlocks[q].All(req => _gameStore.State.CompletedQuests.Contains(req.Name))),
                l.Type ?? "Adventure"
            )).ToList();

        UsableLocations = usable;
        
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

    public void UpdateMagicCapacity()
    {
        int capacity = 30;
        foreach (var abilityKey in UnlockedAbilities)
        {
            var parts = abilityKey.Split(':');
            if (parts.Length < 2) continue;
            var cadenceName = parts[0];
            var abilityName = parts[1];
            
            var cadence = _cadences.All.FirstOrDefault(c => c.Name == cadenceName);
            if (cadence.Name == null) continue;
            
            var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == abilityName);
            if (unlock.Ability.Name != null && unlock.Ability.Effects != null)
            {
                foreach (var effect in unlock.Ability.Effects)
                {
                    if (effect.Type == EffectType.MagicCapacity)
                    {
                        capacity = Math.Max(capacity, effect.Value);
                    }
                }
            }
        }
        _gameStore.Dispatch(new SetMagicCapacityAction(capacity));
    }

    public bool HasAbility(Character character, CadenceAbility ability)
    {
        return JunctionManager.CurrentlyAssigned(character).Any(cad => 
            cad.Abilities.Any(a => a.Ability.Name == ability.Name && UnlockedAbilities.Contains($"{cad.Name}:{a.Ability.Name}"))
        );
    }
}
