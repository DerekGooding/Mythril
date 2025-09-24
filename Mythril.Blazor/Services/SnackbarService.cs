namespace Mythril.Blazor.Services;

public class SnackbarService
{
    public event Action<string, string>? OnShow;
    // string message, string severity maybe ("info", "error", etc.)

    public void Show(string message, string severity = "info")
    {
        OnShow?.Invoke(message, severity);
    }
}
