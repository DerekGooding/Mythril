using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class PartyPanel : VerticalStackPanel
{
    public PartyPanel(PartyManager partyManager)
    {
        Spacing = 5;

        foreach (var character in partyManager.PartyMembers)
        {
            var charWidget = new CharacterDisplayWidget(character);
            Widgets.Add(charWidget);
        }
    }
}
