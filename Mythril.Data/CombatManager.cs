namespace Mythril.Data;

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
    }

    private void TakeTurn()
    {
        if (IsCombatOver()) return;

        var currentCombatant = _turnOrder[_turnIndex];
        _turnIndex = (_turnIndex + 1) % _turnOrder.Count;
    }

    private bool IsCombatOver() => _playerParty.Count == 0 || _enemyParty.Count == 0;

    private void PerformAttack(Character attacker, Character target)
    {
        // target.TakeDamage(attacker.AttackPower);
    }

    public void PlayerTurn_Attack(Character target)
    {
        PerformAttack(CurrentCombatant, target);
        TakeTurn();
        State = CombatState.EnemyTurn;
    }

    public void PlayerTurn_Defend()
    {
        TakeTurn();
        State = CombatState.EnemyTurn;
    }
}
