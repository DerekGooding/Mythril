using Microsoft.JSInterop;

namespace Mythril.Blazor.Services;

public class ThemeService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private const string _themeKey = "theme";

    public event Action? OnThemeChanged;

    public async Task<string> GetTheme() => await _jsRuntime.InvokeAsync<string>("localStorage.getItem", _themeKey) ?? "light-theme";

    public async Task SetTheme(string theme)
    {
        await _jsRuntime.InvokeVoidAsync("setTheme", theme);
        OnThemeChanged?.Invoke();
    }

    public async Task ToggleTheme()
    {
        var currentTheme = await GetTheme();
        var newTheme = currentTheme == "light-theme" ? "dark-theme" : "light-theme";
        await SetTheme(newTheme);
    }
}
