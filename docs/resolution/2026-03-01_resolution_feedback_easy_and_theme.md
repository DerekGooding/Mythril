# Resolution: Too Easy & Toggle Theme Bug

**Date:** 2026-03-01
**Resolved Items:** 
- [Feedback: Too easy](../feedback/2026-03-01_too_easy.md)
- [Feedback: Toggle Theme Button doesn't work](../feedback/2026-03-01_toggle_theme_button_doesnt_wo.md)
- [Error: Automated Error Report](../errors/2026-03-01_automated_error_report.md)

## Summary of Changes

### 1. Quest Balance & Stat Influence
- Fixed a bug where quests were progressing 10x faster than intended due to decisecond/second unit mismatch.
- Changed `SecondsElapsed` from `int` to `double` to support real-time delta updates.
- Increased base duration for Cadence unlocks from 10s to 30s.
- Implemented character stat influence on durations:
    - **Strength**: Reduces duration of **Recurring** quests.
    - **Vitality**: Reduces duration of **Single** quests.
    - **Magic**: Reduces duration of **Cadence Unlock** tasks.
- Verified that stat-based reductions are working correctly via unit tests.

### 2. Theme Switching Bug
- Identified that `InvokeVoidAsync("setTheme", theme)` was failing in some environments.
- Updated `ThemeService.cs` to use `window.setTheme` for clearer scoping.
- Improved JS interop resilience in `index.html`.

## Verification Results
- All unit tests in `QuestLifecycleTests.cs` and `JunctionTests.cs` passed with the new logic.
- Project health check passed.
