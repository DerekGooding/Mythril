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

    public void Initialize()
    {
        _assignedCadences = _cadences.All.ToNamedDictionary(_ => (Character?)null);
    }

    public void AssignCadence(Cadence cadence, Character character, List<CadenceAbility> unlockedAbilities)
    {
        var existingOwner = _assignedCadences.GetValueOrDefault(cadence);
        if (existingOwner != null && existingOwner.Value.Name != character.Name)
        {
            Unassign(cadence);
        }
        
        foreach (var c in _assignedCadences.Where(x => x.Value?.Name == character.Name).ToList())
        {
            Unassign(c.Key);
        }

        _assignedCadences[cadence] = character;
    }

    public void Unassign(Cadence cadence)
    {
        if (_assignedCadences.TryGetValue(cadence, out var owner) && owner != null)
        {
            Junctions.RemoveAll(j => j.Character.Name == owner.Value.Name);
        }
        _assignedCadences[cadence] = null;
    }

    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => _assignedCadences.Where(x => x.Value?.Name == character.Name).Select(x => x.Key);

    public void JunctionMagic(Character character, Stat stat, Item magic, List<CadenceAbility> unlockedAbilities)
    {
        var cadence = CurrentlyAssigned(character).FirstOrDefault();
        if (cadence.Name == null) return;

        string abilityName = GetJunctionAbilityName(stat.Name);
        if (!cadence.Abilities.Any(a => unlockedAbilities.Contains(a.Ability) && a.Ability.Name == abilityName)) return;

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
            "Health" => "J-HP",
            _ => "J-" + statName
        };
    }

    public int GetStatValue(Character character, string statName)
    {
        int baseValue = 10;

        var junction = Junctions.FirstOrDefault(j => j.Character.Name == character.Name && (j.Stat.Name == statName || (statName == "Health" && j.Stat.Name == "HP")));
        if (junction != null)
        {
            int qty = _inventory.GetQuantity(junction.Magic);
            var augments = _statAugments[junction.Magic];
            var augment = augments.FirstOrDefault(a => a.Stat.Name == statName || (statName == "Health" && a.Stat.Name == "HP"));
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
