using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes; // For SolidBrush
using Microsoft.Xna.Framework; // For Color
using Mythril.GameLogic;

namespace Mythril.UI;

public class DropZoneWidget : Panel
{
    private readonly TaskManager _taskManager;
    private Color _defaultBackgroundColor = Color.LightBlue;
    private Color _hoverBackgroundColor = Color.CornflowerBlue;

    public DropZoneWidget(TaskManager taskManager) // Constructor now takes TaskManager
    {
        _taskManager = taskManager; // Assign TaskManager

        Width = 200;
        Height = 150;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        // Subscribe to mouse events
        MouseEntered += DropZoneWidget_MouseEntered;
        MouseLeft += DropZoneWidget_MouseExited;
    }

    private void DropZoneWidget_MouseEntered(object? sender, EventArgs e) => Background = new SolidBrush(Color.CornflowerBlue); // Change color on hover

    private void DropZoneWidget_MouseExited(object? sender, EventArgs e) => Background = new SolidBrush(Color.LightBlue); // Revert color on leave

    public void HandleDrop(CardWidget cardWidget)
    {
        _taskManager.StartTask(cardWidget.CardData);
        Console.WriteLine($"Card {cardWidget.CardData.Title} dropped on DropZone.");
    }
}
