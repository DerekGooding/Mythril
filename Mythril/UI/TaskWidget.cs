using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Mythril.Data;

namespace Mythril.UI;

public class TaskWidget : VerticalStackPanel
{
    public event Action<TaskWidget>? OnDragEnd;

    private bool _isDragging;
    private Point _dragOffset;
    public Container? OriginalParent { get; private set; }
    public int OriginalIndex { get; private set; }

    public bool IsDraggable { get; set; } = true;

    public TaskData TaskData { get; }

    private readonly DropZoneWidget _dropZone;

    public TaskWidget(TaskData taskData, DropZoneWidget dropZone)
    {
        TaskData = taskData;
        _dropZone = dropZone;

        // Appearance
        Padding = new Thickness(10);
        Background = new SolidBrush("#333333");
        Border = new SolidBrush(Color.Yellow);
        BorderThickness = new Thickness(4);

        // Title
        Widgets.Add(new Label { Text = taskData.Title, HorizontalAlignment = HorizontalAlignment.Center });

        // Image Placeholder
        var imagePlaceholder = new Panel
        {
            Width = 100,
            Height = 100,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Widgets.Add(imagePlaceholder);

        // Description
        Widgets.Add(new Label { Text = taskData.Description, Wrap = true });

        // Subscribe to touch events
        TouchDown += TaskWidget_MouseDown;
        TouchUp += TaskWidget_MouseUp;
        TouchMoved += TaskWidget_MouseMove;
    }

    private void TaskWidget_MouseDown(object? sender, EventArgs e)
    {
        if (Desktop == null || Desktop.TouchPosition == null || Parent == null) return;

        _isDragging = true;
        _dragOffset = new Point(Desktop.TouchPosition.Value.X - Bounds.X, Desktop.TouchPosition.Value.Y - Bounds.Y);
        Opacity = 0.8f;

        // Store original parent and index
        OriginalParent = (Container)Parent;
        OriginalIndex = OriginalParent.Widgets.IndexOf(this);

        // Reparent to desktop
        OriginalParent.Widgets.Remove(this);
        Desktop.Widgets.Add(this);
    }

    private void TaskWidget_MouseUp(object? sender, EventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            Opacity = 1.0f;

            // The Game1 class will handle re-parenting or destroying the card
            OnDragEnd?.Invoke(this);
        }
    }

    private void TaskWidget_MouseMove(object? sender, EventArgs e)
    {
        if (Desktop == null || Desktop.TouchPosition == null) return;
        if (_isDragging)
        {
            Left = Desktop.TouchPosition.Value.X - _dragOffset.X;
            Top = Desktop.TouchPosition.Value.Y - _dragOffset.Y;
            //e.Handled = true;
        }
    }
}