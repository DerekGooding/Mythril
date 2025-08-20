namespace Mythril.GameLogic.Combat;

public enum CombatState
{
    PlayerTurn,
    EnemyTurn,
    CombatOver
}

public class CombatManager(PartyManager partyManager)
{
    public CombatState State { get; private set; }
    private readonly PartyManager _partyManager = partyManager;
    public IReadOnlyList<Character> PlayerParty => _playerParty;
    public IReadOnlyList<Character> EnemyParty => _enemyParty;
    private readonly List<Character> _playerParty = [];
    private readonly List<Character> _enemyParty = [];
    private readonly List<Character> _turnOrder = [];
    private int _turnIndex = 0;
    public Character CurrentCombatant => _turnOrder[_turnIndex];

    public void StartCombat(List<Character> enemies)
    {
        _playerParty.Clear();
        _playerParty.AddRange(_partyManager.PartyMembers);

        _enemyParty.Clear();
        _enemyParty.AddRange(enemies);

        _turnOrder.Clear();
        _turnOrder.AddRange(_playerParty);
        _turnOrder.AddRange(_enemyParty);
        _turnIndex = 0;
        State = CombatState.PlayerTurn;

        Game1.Log("Combat started!");
    }

    public void Update(GameTime gameTime)
    {
        if (State == CombatState.CombatOver) return;

        if (IsCombatOver())
        {
            State = CombatState.CombatOver;
            return;
        }

        if (State == CombatState.EnemyTurn)
        {
            var enemy = CurrentCombatant;
            if (_playerParty.Count > 0)
            {
                var target = _playerParty[Random.Shared.Next(_playerParty.Count)];
                PerformAttack(enemy, target);
            }
            TakeTurn();
            State = CombatState.PlayerTurn;
        }
    }

    private void TakeTurn()
    {
        if (IsCombatOver()) return;

        var currentCombatant = _turnOrder[_turnIndex];
        Game1.Log($"{currentCombatant.Name}'s turn.");

        _turnIndex = (_turnIndex + 1) % _turnOrder.Count;
    }

    private bool IsCombatOver()
    {
        if (_playerParty.Count == 0)
        {
            Game1.Log("You have been defeated!");
            return true;
        }

        if (_enemyParty.Count == 0)
        {
            Game1.Log("You are victorious!");
            return true;
        }

        return false;
    }

    private void PerformAttack(Character attacker, Character target)
    {
        Game1.Log($"{attacker.Name} attacks {target.Name}!");
        target.TakeDamage(attacker.AttackPower);
    }

    public void PlayerTurn_Attack(Character target)
    {
        PerformAttack(CurrentCombatant, target);
        TakeTurn();
        State = CombatState.EnemyTurn;
    }

    public void PlayerTurn_Defend()
    {
        Game1.Log($"{CurrentCombatant.Name} defends!");
        TakeTurn();
        State = CombatState.EnemyTurn;
    }
}
