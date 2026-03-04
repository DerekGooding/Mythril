namespace Mythril.Blazor.Services;

public class InventoryService
{
    public event Action<string>? OnItemOverflow;

    public void NotifyOverflow(string itemName)
    {
        OnItemOverflow?.Invoke(itemName);
    }
}
