namespace Mythril.Data;

public enum CombatState
{
    InProgress,
    Victory,
    Defeat,
    NotInCombat,
}

public class CombatManager(PartyManager partyManager, ResourceManager resourceManager)
{
    public CombatState State { get; private set; }
    private readonly PartyManager _partyManager = partyManager;
    private readonly ResourceManager _resourceManager = resourceManager;
    public IReadOnlyList<Character> PlayerParty => _playerParty;
    public IReadOnlyList<Character> EnemyParty => _enemyParty;
    private readonly List<Character> _playerParty = [];
    private readonly List<Character> _enemyParty = [];
    private readonly List<Character> _turnOrder = [];
    private int _turnIndex = 0;
    public Character CurrentCombatant => _turnOrder[_turnIndex];

    public void StartCombat(List<Enemy> enemies)
    {
        _playerParty.Clear();
        _playerParty.AddRange(_partyManager.PartyMembers);
        _playerParty.ForEach(c => c.Health = c.MaxHealth);

        _enemyParty.Clear();
        _enemyParty.AddRange(enemies);
        _enemyParty.ForEach(c => c.Health = c.MaxHealth);

        _turnOrder.Clear();
        _turnOrder.AddRange(_playerParty);
        _turnOrder.AddRange(_enemyParty);
        _turnIndex = 0;
        State = CombatState.InProgress;
    }

    public void SimulateToEnd()
    {
        while (State == CombatState.InProgress)
        {
            TakeTurn();
        }

        if (State == CombatState.Victory)
        {
            _resourceManager.AddGold(10); // Placeholder
            foreach (var character in _playerParty)
            {
                character.AddJobPoints(10); // Placeholder
            }
        }
    }

    private void TakeTurn()
    {
        var attacker = _turnOrder[_turnIndex];

        if (attacker.Health > 0)
        {
            if (_playerParty.Contains(attacker))
            {
                var target = _enemyParty.Where(e => e.Health > 0).OrderBy(e => Guid.NewGuid()).FirstOrDefault();
                if (target != null)
                {
                    PerformAttack(attacker, target);
                }
            }
            else
            {
                var target = _playerParty.Where(p => p.Health > 0).OrderBy(p => Guid.NewGuid()).FirstOrDefault();
                if (target != null)
                {
                    PerformAttack(attacker, target);
                }
            }
        }

        _turnIndex = (_turnIndex + 1) % _turnOrder.Count;
        UpdateCombatState();
    }

    private void UpdateCombatState()
    {
        if (_enemyParty.All(e => e.Health <= 0))
        {
            State = CombatState.Victory;
        }
        else if (_playerParty.All(p => p.Health <= 0))
        {
            State = CombatState.Defeat;
        }
    }

    private void PerformAttack(Character attacker, Character target)
    {
        var damage = Math.Max(1, attacker.AttackPower - target.Defense);
        target.TakeDamage(damage);
    }
}
