using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class CharacterDisplayWidget : Button
{
    public CharacterDisplayWidget(Character character)
    {
        var panel = new HorizontalStackPanel
        {
            Spacing = 10
        };

        var nameLabel = new Label
        {
            Text = character.Name,
        };

        var jobLabel = new Label
        {
            Text = $"({character.Job?.Name})",
        };

        panel.Widgets.Add(nameLabel);
        panel.Widgets.Add(jobLabel);

        Content = panel;

        Click += (s, a) =>
        {
            var statusScreen = new CharacterStatusScreen(character);
            statusScreen.ShowModal(Desktop);
        };
    }
}
