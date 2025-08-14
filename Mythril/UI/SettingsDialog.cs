using Myra.Graphics2D.UI;

namespace Mythril.UI;

public class SettingsDialog : Dialog
{
    public SettingsDialog()
    {
        Title = "Settings";

        var mainPanel = new VerticalStackPanel { Spacing = 10 };

        // Music Volume
        var musicPanel = new HorizontalStackPanel { Spacing = 10 };
        musicPanel.Widgets.Add(new Label { Text = "Music Volume:" });
        var musicSlider = new HorizontalSlider { Width = 200 };
        musicPanel.Widgets.Add(musicSlider);
        mainPanel.Widgets.Add(musicPanel);

        // Sound Effects Volume
        var sfxPanel = new HorizontalStackPanel { Spacing = 10 };
        sfxPanel.Widgets.Add(new Label { Text = "Sound Effects Volume:" });
        var sfxSlider = new HorizontalSlider { Width = 200 };
        sfxPanel.Widgets.Add(sfxSlider);
        mainPanel.Widgets.Add(sfxPanel);

        // Close Button
        var closeButton = new Button { Content = new Label { Text = "Close" }, HorizontalAlignment = HorizontalAlignment.Center };
        closeButton.Click += (s, a) => Close();
        mainPanel.Widgets.Add(closeButton);

        Content = mainPanel;
    }
}