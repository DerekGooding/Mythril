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

    public event Action<Character>? OnCadenceUnassigned;

    public void Initialize()
    {
        _assignedCadences = _cadences.All.ToNamedDictionary(_ => (Character?)null);
        Junctions.Clear();
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
    }

    public void Unassign(Cadence cadence, HashSet<string> unlockedAbilities)
    {
        if (_assignedCadences.TryGetValue(cadence, out var owner) && owner != null)
        {
            _assignedCadences[cadence] = null;
            
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

    public int GetStatValue(Character character, string statName)
    {
        int baseValue = 10;

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

        return baseValue;
    }
    
    public void RestoreAssignment(Cadence cadence, Character character)
    {
        _assignedCadences[cadence] = character;
    }
}
