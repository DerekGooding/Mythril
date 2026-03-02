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
            // Try standard invoke on the window-scoped function
            await _jsRuntime.InvokeVoidAsync("window.setTheme", theme);
            OnThemeChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("setTheme interop failed, attempting re-injection. Error: {Msg}", ex.Message);
            try
            {
                // If it's missing, re-inject the core logic directly via eval
                await _jsRuntime.InvokeVoidAsync("eval", $@"
                    window.setTheme = function(t) {{
                        var link = document.getElementById('theme');
                        if (link) link.setAttribute('href', 'css/' + t + '.css');
                        localStorage.setItem('theme', t);
                    }};
                    window.setTheme('{theme}');
                ");
                OnThemeChanged?.Invoke();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Critical theme failure: Re-injection failed for {Theme}", theme);
            }
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
