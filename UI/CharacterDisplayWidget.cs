using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI
{
    public class CharacterDisplayWidget : HorizontalStackPanel
    {
        public CharacterDisplayWidget(Character character)
        {
            Spacing = 10;

            var nameLabel = new Label
            {
                Text = character.Name,
            };

            var jobLabel = new Label
            {
                Text = $"({character.Job?.Name})",
            };

            Widgets.Add(nameLabel);
            Widgets.Add(jobLabel);
        }
    }
}
