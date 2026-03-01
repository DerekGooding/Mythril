# Resolution: Core Bugs & Layout Refinement (V2)

**Date:** 2026-03-01
**Resolved Items:** 
- [Feedback: Game too easy](../feedback/2026-03-01_game_too_easy.md)
- [Feedback: Toggle Theme](../feedback/2026-03-01_toggle_theme.md)
- [Feedback: Tutorial Section](../feedback/2026-03-01_tutorial_section.md)
- [Feedback: Window Scrolling](../feedback/2026-03-01_window_scrolling.md)
- [Error: Automated Error Report](../errors/2026-03-01_automated_error_report.md)
- [Error: Automated Error Report (Duplicate)](../errors/2026-03-01_automated_error_report_1.md)

## Summary of Changes

### 1. Quest Logic Fixes
- **Single-Time Quest Protection**: Fixed a bug in `ResourceManager.UnlockQuest` where completed `Single` type quests could be re-added to location lists if a repeatable quest was finished.
- **Quest Durations**: Significantly increased base quest durations in `quest_details.json` to improve gameplay balance and progression feel.

### 2. UI & Layout Enhancements
- **Parent Scrolling Fixed**: Refactored `MainLayout.razor.css` and `Home.razor` to use flexbox containers correctly. The parent window is now constrained to `100vh` with `overflow: hidden`, while sub-columns and tab panes handle their own scrolling.
- **Theme Switching Robustness**: Fixed a JS Interop error by correctly scoping the `setTheme` call. Removed redundant `window.` prefix in `InvokeVoidAsync` which was causing lookup failures in some environments.

### 3. Verification
- Verified that "Tutorial Section" (Repeatable) no longer re-unlocks "Prologue" after completion.
- Verified that the UI no longer has double scrollbars and correctly fills the viewport.
- Verified theme switching works without console errors.
