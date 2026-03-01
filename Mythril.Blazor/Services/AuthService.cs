using Microsoft.JSInterop;

namespace Mythril.Blazor.Services;

public class AuthService(IJSRuntime js)
{
    private const string AUTH_KEY = "mythril_dev_mode";
    private bool? _isAuthenticated;

    public bool IsAuthenticated => _isAuthenticated ?? false;

    public async Task InitializeAsync()
    {
        var stored = await js.InvokeAsync<string>("localStorage.getItem", AUTH_KEY);
        _isAuthenticated = stored == "true";
    }

    public async Task SetDevMode(bool enabled)
    {
        await js.InvokeVoidAsync("localStorage.setItem", AUTH_KEY, enabled.ToString().ToLower());
        _isAuthenticated = enabled;
    }

    public async Task Logout()
    {
        await SetDevMode(false);
    }
}
