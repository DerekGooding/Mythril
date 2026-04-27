namespace Mythril.Data;

public class JunctionManager(
    GameStore gameStore,
    InventoryManager inventory,
    StatAugments statAugments,
    Cadences cadences)
{
    private readonly GameStore _gameStore = gameStore;
    private readonly InventoryManager _inventory = inventory;
    private readonly StatAugments _statAugments = statAugments;
    private readonly Cadences _cadences = cadences;

    public List<Junction> Junctions => [.. _gameStore.State.Junctions];

    public Dictionary<string, Dictionary<string, int>> CharacterStatBoosts
        => _gameStore.State.CharacterPermanentStatBoosts.ToDictionary(k => k.Key, v => v.Value.ToDictionary(ik => ik.Key, iv => iv.Value));

    public event Action<Character>? OnCadenceUnassigned;

    public event Action<Character>? OnJunctionChanged;

    public void Initialize()
    { }

    public void UpdatePassiveBoosts(Character character, HashSet<string> unlockedAbilities)
    { }

    public void AssignCadence(Cadence cadence, Character character, HashSet<string> unlockedAbilities)
    {
        var prevOwner = GetAssignedCharacter(cadence);
        _gameStore.Dispatch(new AssignCadenceAction(cadence.Name, character.Name));

        if (prevOwner.HasValue && prevOwner.Value.Name != character.Name)
        {
            OnCadenceUnassigned?.Invoke(prevOwner.Value);
        }
        OnJunctionChanged?.Invoke(character);
    }

    public void Unassign(Cadence cadence, HashSet<string> unlockedAbilities)
    {
        if (_gameStore.State.AssignedCadences.TryGetValue(cadence.Name, out var owner) && owner != null)
        {
            _gameStore.Dispatch(new UnassignCadenceAction(cadence.Name));
            OnCadenceUnassigned?.Invoke(new Character(owner));
        }
    }

    public Character? GetAssignedCharacter(Cadence cadence)
    {
        var name = _gameStore.State.AssignedCadences.GetValueOrDefault(cadence.Name);
        return name != null ? new Character(name) : null;
    }

    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => _gameStore.State.AssignedCadences.Where(x => x.Value == character.Name)
            .Select(x => _cadences.All.FirstOrDefault(c => c.Name == x.Key))
            .Where(c => c.Name != null);

    public void JunctionMagic(Character character, Stat stat, Item magic, HashSet<string> unlockedAbilities)
    {
        var abilityName = GetJunctionAbilityName(stat.Name);
        var cadencesWithAbility = CurrentlyAssigned(character)
            .Where(c => c.Abilities.Any(a => a.Ability.Name == abilityName && unlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}")));

        if (!cadencesWithAbility.Any()) return;

        if (magic.Name != null)
        {
            _gameStore.Dispatch(new JunctionMagicAction(character, stat, magic));
        }
        else
        {
            _gameStore.Dispatch(new UnjunctionAction(character, stat));
        }
        OnJunctionChanged?.Invoke(character);
    }

    private string GetJunctionAbilityName(string statName) => statName switch
    {
        "Strength" => "J-Str",
        "Magic" => "J-Magic",
        "Vitality" => "J-Vit",
        "Speed" => "J-Speed",
        _ => "J-" + statName
    };

    public void AddStatBoost(Character character, string statName, int amount) => _gameStore.Dispatch(new AddStatBoostAction(character.Name, statName, amount));

    public int GetStatValue(Character character, string statName)
    {
        var baseValue = 10;

        // Apply permanent boosts
        if (_gameStore.State.CharacterPermanentStatBoosts.TryGetValue(character.Name, out var boosts))
        {
            baseValue += boosts.GetValueOrDefault(statName, 0);
        }

        // Apply passive ability boosts (calculated on demand for now)
        var assigned = CurrentlyAssigned(character);
        foreach (var cadence in assigned)
        {
            foreach (var unlock in cadence.Abilities)
            {
                if (_gameStore.State.UnlockedAbilities.Contains($"{cadence.Name}:{unlock.Ability.Name}") && unlock.Ability.Effects != null)
                {
                    foreach (var effect in unlock.Ability.Effects)
                    {
                        if (effect.Type == EffectType.StatBoost && effect.Target == statName)
                        {
                            baseValue += effect.Value;
                        }
                    }
                }
            }
        }

        var junction = _gameStore.State.Junctions.FirstOrDefault(j => j.Character.Name == character.Name && j.Stat.Name == statName);
        if (junction?.Magic.Name != null)
        {
            var qty = _inventory.GetQuantity(junction.Magic);
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

    public void RestoreAssignment(Cadence cadence, Character character) => _gameStore.Dispatch(new AssignCadenceAction(cadence.Name, character.Name));
}