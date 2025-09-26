using System.Collections.Concurrent;

namespace Mythril.Blazor.Services;

public class SnackbarService
{
    private readonly ConcurrentQueue<(string Message, string Severity)> _queue = new();
    public event Func<Task>? OnChange;

    public void Show(string message, string severity = "info")
    {
        _queue.Enqueue((message, severity));
        OnChange?.Invoke();
    }

    public bool TryDequeue(out (string Message, string Severity) item) => _queue.TryDequeue(out item);
}
