using Myra.Graphics2D.UI;
using Mythril.API;
using Mythril.GameLogic.AI;
using Mythril.GameLogic.Combat;
using System.Threading.Tasks;

namespace Mythril.UI;

public class CombatScreen : Dialog, ICommandExecutor
{
    private readonly CombatManager _combatManager;
    private readonly VerticalStackPanel _logPanel;

    public CombatScreen(CombatManager combatManager)
    {
        _combatManager = combatManager;
        Title = "Combat";

        var mainPanel = new HorizontalStackPanel { Spacing = 20 };

        // Player Party Panel
        var playerPanel = new VerticalStackPanel { Spacing = 10 };
        playerPanel.Widgets.Add(new Label { Text = "Player Party" });
        foreach (var character in _combatManager.PlayerParty)
        {
            playerPanel.Widgets.Add(new Label { Text = character.Name });
        }
        mainPanel.Widgets.Add(playerPanel);

        // Enemy Party Panel
        var enemyPanel = new VerticalStackPanel { Spacing = 10 };
        enemyPanel.Widgets.Add(new Label { Text = "Enemy Party" });
        foreach (var character in _combatManager.EnemyParty)
        {
            enemyPanel.Widgets.Add(new Label { Text = character.Name });
        }
        mainPanel.Widgets.Add(enemyPanel);

        // Log Panel
        _logPanel = new VerticalStackPanel { Spacing = 5 };
        var logScrollViewer = new ScrollViewer
        {
            Content = _logPanel
        };
        mainPanel.Widgets.Add(logScrollViewer);

        // Action Panel
        var actionPanel = new VerticalStackPanel { Spacing = 10 };
        actionPanel.Widgets.Add(new Label { Text = "Actions" });

        var attackButton = new Button { Id = "Attack", Content = new Label { Text = "Attack" } };
        attackButton.Click += (s, a) =>
        {
            if (_combatManager.EnemyParty.Count > 0)
                _combatManager.PlayerTurn_Attack(_combatManager.EnemyParty[0]);
        };
        actionPanel.Widgets.Add(attackButton);

        var defendButton = new Button { Id = "Defend", Content = new Label { Text = "Defend" } };
        defendButton.Click += (s, a) =>
        {
            _combatManager.PlayerTurn_Defend();
        };
        actionPanel.Widgets.Add(defendButton);

        mainPanel.Widgets.Add(actionPanel);

        Content = mainPanel;
    }

    public void AddLogMessage(string message) => _logPanel.Widgets.Add(new Label { Text = message });

    public async Task ExecuteCommand(Command command)
    {
        switch (command.Action.ToUpperInvariant())
        {
            case "CLICK_BUTTON":
                HandleClickButton(command);
                break;
        }
    }

    private void HandleClickButton(Command command)
    {
        if (command.Target == "Attack")
        {
            if (_combatManager.EnemyParty.Count > 0)
                _combatManager.PlayerTurn_Attack(_combatManager.EnemyParty[0]);
        }
        else if (command.Target == "Defend")
        {
            _combatManager.PlayerTurn_Defend();
        }
    }
}
