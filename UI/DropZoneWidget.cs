using Microsoft.Xna.Framework; // For Color
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes; // For SolidBrush
using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class DropZoneWidget : Panel
{
    private readonly TaskManager _taskManager;
    private readonly Color _defaultBackgroundColor = Color.DarkGray;
    private readonly Color _hoverBackgroundColor = Color.Gray;

    public DropZoneWidget(TaskManager taskManager) // Constructor now takes TaskManager
    {
        _taskManager = taskManager; // Assign TaskManager

        Width = 200;
        Height = 150;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        Padding = new Thickness(10);
        Background = new SolidBrush(_defaultBackgroundColor);

        // Subscribe to mouse events
        MouseEntered += DropZoneWidget_MouseEntered;
        MouseLeft += DropZoneWidget_MouseExited;
    }

    private void DropZoneWidget_MouseEntered(object? sender, EventArgs e)
    {
        Background = new SolidBrush(_hoverBackgroundColor); // Change color on hover
        Border = new SolidBrush(Color.White);
        BorderThickness = new Thickness(2);
    }

    private void DropZoneWidget_MouseExited(object? sender, EventArgs e)
    {
        Background = new SolidBrush(_defaultBackgroundColor); // Revert color on leave
        Border = null;
        BorderThickness = new Thickness(0);
    }

    public void HandleDrop(CardWidget cardWidget)
    {
        _taskManager.StartTask(cardWidget.CardData);
        Console.WriteLine($"Card {cardWidget.CardData.Title} dropped on DropZone.");
    }
}
