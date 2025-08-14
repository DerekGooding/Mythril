using Myra.Graphics2D.UI;
using Mythril.GameLogic.Combat;
using System.Collections.Generic;

namespace Mythril.UI
{
    public class CombatScreen : Dialog
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

            Content = mainPanel;
        }

        public void AddLogMessage(string message)
        {
            _logPanel.Widgets.Add(new Label { Text = message });
        }
    }
}
