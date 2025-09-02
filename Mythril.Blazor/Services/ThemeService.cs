using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Mythril.Blazor.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string ThemeKey = "theme";

        public event Action OnThemeChanged;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetTheme()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ThemeKey) ?? "light-theme";
        }

        public async Task SetTheme(string theme)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemeKey, theme);
            OnThemeChanged?.Invoke();
        }

        public async Task ToggleTheme()
        {
            var currentTheme = await GetTheme();
            var newTheme = currentTheme == "light-theme" ? "dark-theme" : "light-theme";
            await SetTheme(newTheme);
        }
    }
}
