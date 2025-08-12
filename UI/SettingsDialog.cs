using Myra.Graphics2D.UI;

namespace Mythril.UI;

public class SettingsDialog : Dialog
{
    public SettingsDialog()
    {
        Title = "Settings";
        Content = new VerticalStackPanel
        {
            Widgets =
            {
                new Label { Text = "This is a basic settings dialog." },
                new Button { Content = new Label { Text = "Close" }, HorizontalAlignment = HorizontalAlignment.Center }
            }
        };

        // Handle Close button click
        ((Button)((VerticalStackPanel)Content).Widgets[1]).Click += (s, a) => Close();
    }
}