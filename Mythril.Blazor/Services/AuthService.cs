using Microsoft.JSInterop;

namespace Mythril.Blazor.Services;

public class AuthService(IJSRuntime js)
{
    private const string AUTH_KEY = "mythril_dev_auth";
    private bool? _isAuthenticated;

    public bool IsAuthenticated => _isAuthenticated ?? false;

    public async Task InitializeAsync()
    {
        var stored = await js.InvokeAsync<string>("localStorage.getItem", AUTH_KEY);
        // In a real app, you'd verify this against a hash or backend.
        // For now, we check if the key matches a specific secret.
        _isAuthenticated = !string.IsNullOrEmpty(stored) && stored == "MYTHRIL_ADMIN_KEY";
    }

    public async Task<bool> Authenticate(string secret)
    {
        if (secret == "MYTHRIL_ADMIN_KEY")
        {
            await js.InvokeVoidAsync("localStorage.setItem", AUTH_KEY, secret);
            _isAuthenticated = true;
            return true;
        }
        return false;
    }

    public async Task Logout()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", AUTH_KEY);
        _isAuthenticated = false;
    }
}
