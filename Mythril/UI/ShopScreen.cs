using Myra.Graphics2D.UI;

namespace Mythril.UI;

public class ShopScreen : Dialog
{
    public ShopScreen()
    {
        Title = "Shop";

        var mainPanel = new VerticalStackPanel { Spacing = 10 };

        // Item list
        var itemList = new ListView();
        mainPanel.Widgets.Add(itemList);

        // Buy button
        var buyButton = new Button { Content = new Label { Text = "Buy" } };
        mainPanel.Widgets.Add(buyButton);

        // Close Button
        var closeButton = new Button { Content = new Label { Text = "Close" }, HorizontalAlignment = HorizontalAlignment.Center };
        closeButton.Click += (s, a) => Close();
        mainPanel.Widgets.Add(closeButton);

        Content = mainPanel;
    }
}
