using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class TaskProgressWidget : VerticalStackPanel
{
    private readonly HorizontalProgressBar _progressBar;
    private readonly Label _titleLabel;
    private readonly Label _statusLabel;
    private readonly Panel _glowPanel;

    public TaskProgressWidget(TaskProgress taskProgress)
    {
        TaskProgress = taskProgress;

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        Spacing = 5;

        _glowPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var content = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
        };

        // Title Label
        _titleLabel = new Label
        {
            Text = taskProgress.CardData.Title,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.Widgets.Add(_titleLabel);

        // Progress Bar
        _progressBar = new HorizontalProgressBar
        {
            Width = 200,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = taskProgress.CardData.DurationSeconds,
            Value = 0,
            StyleName = "ThemedProgressBar"
        };
        content.Widgets.Add(_progressBar);

        // Status Label (for completion feedback)
        _statusLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.Widgets.Add(_statusLabel);

        _glowPanel.Widgets.Add(content);
        Widgets.Add(_glowPanel);
    }

    public TaskProgress TaskProgress { get; }

    public void UpdateProgress()
    {
        _progressBar.Value = TaskProgress.ElapsedTime;

        if (TaskProgress.IsCompleted)
        {
            _statusLabel.Text = "Completed!";
            _progressBar.StyleName = "ProgressBarCompleted"; // New style for completed progress bar
            SetGlow(false);
        }
        else
        {
            SetGlow(true);
        }
    }

    public void SetGlow(bool isGlowing)
    {
        if (isGlowing)
        {
            _glowPanel.Background = new SolidBrush("#8000A0FF");
        }
        else
        {
            _glowPanel.Background = null;
        }
    }
}
