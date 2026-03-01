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
- **Internal Scrolling**: Ensured that the `Home.razor` container and its columns use `flex-grow: 1` and `min-height: 0` to correctly delegate scrolling to the sub-panels (Hand, Party, etc.) rather than the parent window.

### 3. Theme System Resilience
- **Explicit Scoping**: Updated `ThemeService.cs` to call `window.setTheme` instead of the implicit `setTheme`.
- **Initialization Order**: Moved global script blocks to the end of `<body>` in `index.html` and added a `setTimeout` guard to the initial theme load to ensure the DOM is fully constructed before JS Interop attempts to access the CSS link element.
- **Logging**: Added verbose console logging to the JS layer to track exact execution state during theme changes.

## Verification
- Quest logic verified via unit tests.
- UI layout verified via manual inspection of CSS constraints.
- Theme switching verified via log analysis.
