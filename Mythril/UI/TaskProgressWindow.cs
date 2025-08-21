using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class TaskProgressWindow : Window
{
    private readonly VerticalStackPanel _stackPanel;

    public TaskProgressWindow()
    {
        Title = "Task Progress";
        Width = 400;
        Height = 500;

        _stackPanel = new VerticalStackPanel
        {
            Spacing = 5
        };

        Content = new ScrollViewer
        {
            Content = _stackPanel
        };
    }

    public void AddTask(GameTaskProgress task)
    {
        var widget = new TaskProgressWidget(task);
        _stackPanel.Widgets.Add(widget);
    }

    public void RemoveTask(GameTaskProgress task)
    {
        var widgetToRemove = _stackPanel.Widgets.OfType<TaskProgressWidget>()
                            .FirstOrDefault(w => w.TaskProgress == task);
        if (widgetToRemove != null)
        {
            _stackPanel.Widgets.Remove(widgetToRemove);
        }
    }

    public void UpdateTasks()
    {
        foreach (var widget in _stackPanel.Widgets)
        {
            if (widget is TaskProgressWidget taskProgressWidget)
            {
                taskProgressWidget.UpdateProgress();
            }
        }
    }
}
