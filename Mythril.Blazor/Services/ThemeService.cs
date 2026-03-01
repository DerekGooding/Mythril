using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace Mythril.Blazor.Services;

public class ThemeService(IJSRuntime jsRuntime, ILogger<ThemeService> logger)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<ThemeService> _logger = logger;
    private const string _themeKey = "theme";

    public event Action? OnThemeChanged;

    public async Task SetTheme(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("setTheme", theme);
            OnThemeChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set theme: {Theme}", theme);
        }
    }

    public async Task<string> GetTheme()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", _themeKey);
            return theme ?? "light-theme";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get theme from localStorage");
            return "light-theme";
        }
    }

    public async Task ToggleTheme()
    {
        try
        {
            var currentTheme = await GetTheme();
            var newTheme = currentTheme == "light-theme" ? "dark-theme" : "light-theme";
            await SetTheme(newTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle theme");
        }
    }
}
