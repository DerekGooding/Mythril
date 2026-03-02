# Resolution: Core System & Layout Refinement (V3)

**Date:** 2026-03-01
**Resolved Items:** 
- [Feedback: Game too easy](../feedback/2026-03-01_game_too_easy.md)
- [Feedback: Toggle Theme](../feedback/2026-03-01_toggle_theme.md)
- [Feedback: Tutorial Section](../feedback/2026-03-01_tutorial_section.md)
- [Feedback: Window Scrolling](../feedback/2026-03-01_window_scrolling.md)

## Summary of Changes

### 1. Quest Logic & Content
- **Prologue Definition**: Added "Prologue" to `quest_details.json`. Previously, it was missing from the details dictionary, which caused logic errors when trying to check its `Type` (Single vs Recurring) during the unlock phase.
- **Quest Re-Unlock Bug**: Verified the fix in `ResourceManager.UnlockQuest` which prevents `Single` type quests from being re-added to locations if they are already in the `_completedQuests` set.
- **Game Balance**: Quest durations have been significantly increased across the board to provide a more meaningful progression pace.

### 2. Layout & Scrolling
- **Viewport Constraints**: Hardened `app.css` by adding `width: 100vw` and `overflow: hidden` to the `html, body, #app` chain.
- **Internal Scrolling**: Refactored `Home.razor` to use nested flexbox containers with `overflow: hidden` and `flex-grow: 1`. This correctly delegates scrolling to sub-panels (Hand, Party, etc.) and prevents scroll areas from taking up the entire vertical space or pushing elements off-screen.
- **Expander Fix**: Replaced JS-driven expansion logic with pure CSS transitions using `max-height` and `opacity`. Additionally, implemented `@key` directives in all `@foreach` loops within `HandPanel.razor` and `CadencePanel.razor`. This ensures Blazor preserves the component state (including `isExpanded`) during rapid data updates, eliminating the "expand and unexpand" flickering bug. Removed obsolete `expander.js`.
- **Character Stacks**: Enforced strictly horizontal stat listing within cards using `flex-nowrap` and `overflow-x: auto`.

### 3. Theme System Overhaul
- **CSS Variables Implementation**: Standardized the entire UI by moving hardcoded colors into a shared set of CSS variables defined in `:root` within `light-theme.css` and `dark-theme.css`. This ensures that when the theme stylesheet is swapped, all components (Character Cards, Inventory, Quest Cards, Workshop, etc.) update their colors correctly and instantly.
- **Component Styling**: Updated all `.razor.css` and internal `<style>` blocks to use these variables, eliminating theme-inconsistent "pockets" of hardcoded light colors.
- **Explicit Scoping**: Updated `ThemeService.cs` to call `window.setTheme` instead of the implicit `setTheme`.
- **Initialization Order**: Moved global script blocks to the end of `<body>` in `index.html` and added a `setTimeout` guard to the initial theme load to ensure the DOM is fully constructed before JS Interop attempts to access the CSS link element.
- **Cadence Restoration**: Fixed a bug where cadences were not being re-unlocked when loading a save. `ResourceManager.RestoreCompletedQuest` now correctly re-triggers cadence unlocks associated with restored quests.

## Verification
- Quest logic verified via unit tests.
- UI layout verified via manual inspection of CSS constraints.
- Theme switching verified via log analysis.
