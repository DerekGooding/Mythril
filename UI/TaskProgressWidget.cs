using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class TaskProgressWidget : VerticalStackPanel
{
    private readonly HorizontalProgressBar _progressBar;
    private readonly Label _titleLabel;
    private readonly Label _statusLabel;

    public TaskProgressWidget(TaskProgress taskProgress)
    {
        TaskProgress = taskProgress;

        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        Spacing = 5;

        // Title Label
        _titleLabel = new Label
        {
            Text = taskProgress.CardData.Title,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Widgets.Add(_titleLabel);

        // Progress Bar
        _progressBar = new HorizontalProgressBar
        {
            Width = 200,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = taskProgress.CardData.DurationSeconds,
            Value = 0
        };
        Widgets.Add(_progressBar);

        // Status Label (for completion feedback)
        _statusLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Widgets.Add(_statusLabel);
    }

    public TaskProgress TaskProgress { get; }

    public void UpdateProgress()
    {
        _progressBar.Value = TaskProgress.ElapsedTime;

        if (TaskProgress.IsCompleted)
        {
            _statusLabel.Text = "Completed!";
            _progressBar.StyleName = "ProgressBarCompleted"; // New style for completed progress bar
        }
    }
}
