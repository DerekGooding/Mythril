namespace Mythril.Data;

public class JunctionManager(
    InventoryManager inventory,
    StatAugments statAugments,
    Cadences cadences)
{
    private readonly InventoryManager _inventory = inventory;
    private readonly StatAugments _statAugments = statAugments;
    private readonly Cadences _cadences = cadences;

    private Dictionary<Cadence, Character?> _assignedCadences = [];
    public List<Junction> Junctions { get; } = [];
    public Dictionary<string, Dictionary<string, int>> CharacterStatBoosts { get; } = [];
    private Dictionary<string, Dictionary<string, int>> _passiveAbilitiyBoosts = [];

    public event Action<Character>? OnCadenceUnassigned;

    public void Initialize()
    {
        _assignedCadences = _cadences.All.ToNamedDictionary(_ => (Character?)null);
        Junctions.Clear();
        CharacterStatBoosts.Clear();
        _passiveAbilitiyBoosts.Clear();
    }

    public void UpdatePassiveBoosts(Character character, HashSet<string> unlockedAbilities)
    {
        var boosts = new Dictionary<string, int>();
        var assigned = CurrentlyAssigned(character);
        
        foreach (var cadence in assigned)
        {
            foreach (var unlock in cadence.Abilities)
            {
                if (unlockedAbilities.Contains($"{cadence.Name}:{unlock.Ability.Name}") && unlock.Ability.Effects != null)
                {
                    foreach (var effect in unlock.Ability.Effects)
                    {
                        if (effect.Type == EffectType.StatBoost && !string.IsNullOrEmpty(effect.Target))
                        {
                            boosts[effect.Target] = boosts.GetValueOrDefault(effect.Target, 0) + effect.Value;
                        }
                    }
                }
            }
        }
        _passiveAbilitiyBoosts[character.Name] = boosts;
    }

    public void AssignCadence(Cadence cadence, Character character, HashSet<string> unlockedAbilities)
    {
        var existingOwner = _assignedCadences.GetValueOrDefault(cadence);
        if (existingOwner != null)
        {
            if (existingOwner.Value.Name == character.Name) return; // Already assigned to this character
            Unassign(cadence, unlockedAbilities);
        }

        _assignedCadences[cadence] = character;
        UpdatePassiveBoosts(character, unlockedAbilities);
    }

    public void Unassign(Cadence cadence, HashSet<string> unlockedAbilities)
    {
        if (_assignedCadences.TryGetValue(cadence, out var owner) && owner != null)
        {
            _assignedCadences[cadence] = null;
            UpdatePassiveBoosts(owner.Value, unlockedAbilities);
            
            // Check if any junctions are now invalid because the ability is gone OR locked for remaining cadences
            foreach (var junction in Junctions.Where(j => j.Character.Name == owner.Value.Name).ToList())
            {
                string abilityName = GetJunctionAbilityName(junction.Stat.Name);
                bool hasAbility = CurrentlyAssigned(owner.Value).Any(c => c.Abilities.Any(a => a.Ability.Name == abilityName && unlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}")));
                if (!hasAbility)
                {
                    Junctions.Remove(junction);
                }
            }

            OnCadenceUnassigned?.Invoke(owner.Value);
        }
    }

    public Character? GetAssignedCharacter(Cadence cadence) => _assignedCadences.GetValueOrDefault(cadence);

    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => _assignedCadences.Where(x => x.Value?.Name == character.Name).Select(x => x.Key);

    public void JunctionMagic(Character character, Stat stat, Item magic, HashSet<string> unlockedAbilities)
    {
        string abilityName = GetJunctionAbilityName(stat.Name);
        var cadencesWithAbility = CurrentlyAssigned(character)
            .Where(c => c.Abilities.Any(a => a.Ability.Name == abilityName && unlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}")));

        if (!cadencesWithAbility.Any()) return;

        Junctions.RemoveAll(j => j.Character.Name == character.Name && j.Stat.Name == stat.Name);
        if (magic.Name != null)
        {
            Junctions.Add(new Junction(character, stat, magic));
        }
    }

    private string GetJunctionAbilityName(string statName)
    {
        return statName switch
        {
            "Strength" => "J-Str",
            "Magic" => "J-Magic",
            "Vitality" => "J-Vit",
            "Speed" => "J-Speed",
            _ => "J-" + statName
        };
    }

    public void AddStatBoost(Character character, string statName, int amount)
    {
        if (!CharacterStatBoosts.ContainsKey(character.Name))
            CharacterStatBoosts[character.Name] = [];
        
        var boosts = CharacterStatBoosts[character.Name];
        boosts[statName] = boosts.GetValueOrDefault(statName, 0) + amount;
    }

    public int GetStatValue(Character character, string statName)
    {
        int baseValue = 10;

        // Apply permanent boosts (from quests, etc.)
        if (CharacterStatBoosts.TryGetValue(character.Name, out var boosts))
        {
            baseValue += boosts.GetValueOrDefault(statName, 0);
        }

        // Apply passive ability boosts (from assigned cadences)
        if (_passiveAbilitiyBoosts.TryGetValue(character.Name, out var pBoosts))
        {
            baseValue += pBoosts.GetValueOrDefault(statName, 0);
        }

        var junction = Junctions.FirstOrDefault(j => j.Character.Name == character.Name && j.Stat.Name == statName);
        if (junction != null)
        {
            int qty = _inventory.GetQuantity(junction.Magic);
            var augments = _statAugments[junction.Magic];
            var augment = augments.FirstOrDefault(a => a.Stat.Name == statName);
            if (augment.Stat.Name != null)
            {
                baseValue += (int)(qty * (augment.ModifierAtFull / 100.0));
            }
            else
            {
                baseValue += qty / 10;
            }
        }

        return Math.Min(255, baseValue);
    }
    
    public void RestoreAssignment(Cadence cadence, Character character)
    {
        _assignedCadences[cadence] = character;
    }
}
