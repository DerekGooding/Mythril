namespace Mythril.GameLogic.Combat;

public class CombatManager(PartyManager partyManager)
{
    private readonly PartyManager _partyManager = partyManager;
    public IReadOnlyList<Character> PlayerParty => _playerParty;
    public IReadOnlyList<Character> EnemyParty => _enemyParty;
    private readonly List<Character> _playerParty = new List<Character>();
    private readonly List<Character> _enemyParty = new List<Character>();
    private readonly List<Character> _turnOrder = new List<Character>();
    private int _turnIndex = 0;

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

        Game1.Log("Combat started!");
    }

    public void Update(GameTime gameTime)
    {
        // This is where the main combat logic will go.
        // For now, we can just advance the turn on a timer.
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
        if (!_playerParty.Any())
        {
            Game1.Log("You have been defeated!");
            return true;
        }

        if (!_enemyParty.Any())
        {
            Game1.Log("You are victorious!");
            return true;
        }

        return false;
    }
}
