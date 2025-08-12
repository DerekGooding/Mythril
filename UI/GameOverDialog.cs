using Myra.Graphics2D.UI;
using System;

namespace Mythril.UI;

public class GameOverDialog : Dialog
{
    public event Action? OnRestartGame;

    public GameOverDialog()
    {
        Title = "Game Over!";
        Content = new VerticalStackPanel
        {
            Widgets =
            {
                new Label { Text = "You have run out of resources!" },
                new Button { Content = new Label { Text = "Restart" }, HorizontalAlignment = HorizontalAlignment.Center }
            }
        };

        // Handle Restart button click
        ((Button)((VerticalStackPanel)Content).Widgets[1]).Click += (s, a) =>
        {
            OnRestartGame?.Invoke();
            Close();
        };
    }
}