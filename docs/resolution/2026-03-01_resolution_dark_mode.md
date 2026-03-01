# Resolution: Dark mode broken

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Derek

## Technical Solution
The dark mode "lag" was actually a full page reload (`forceLoad: true`) triggered by the Blazor `NavigationManager` on every toggle. This has been replaced with a dynamic JavaScript-based stylesheet swap. Furthermore, the `dark-theme.css` was missing overrides for many Bootstrap and custom components, which led to poor readability and a "messy" UI.

## Changes Made
- Modified `Mythril.Blazor\wwwroot\index.html` to add a `window.setTheme` function.
- Updated `Mythril.Blazor\Services\ThemeService.cs` to call the JS `setTheme` function instead of just setting `localStorage`.
- Updated `Mythril.Blazor\Pages\Home.razor` to remove `navigationManager.NavigateTo(..., forceLoad: true)`.
- Significantly expanded `Mythril.Blazor\wwwroot\css\dark-theme.css` with overrides for tabs, cards, expanders, inventory, and character panels.

## Verification
- Theme toggle is now instantaneous.
- All UI elements (tabs, cards, inventory) now correctly adhere to the dark theme color palette.
