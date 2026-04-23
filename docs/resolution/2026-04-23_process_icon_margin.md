# Resolution: Process Icon Margin

## Problem
When quests or unlocks were being worked on, the "in process" icon (active-dot) was either before the title or too close to the title, lacking proper visual separation.

## Technical Resolution
1.  **Global Styles**: Moved `active-dot` and `pulse` animations from component-specific CSS to `app.css` to allow consistent usage across the application.
2.  **Expander Component**:
    *   Moved the `active-dot` icon to follow the `@Header` text.
    *   Removed `flex-grow: 1` from `.header-text` to ensure the icon stays adjacent to the title.
    *   Added `ms-2` (margin-start) to the icon for consistent spacing.
3.  **QuestCard Component**:
    *   Converted `card-header` to a flexbox (`d-flex align-items-center`).
    *   Added the `active-dot` icon after the quest name when the quest is in progress.
    *   Ensured the "In Progress" badge remains right-aligned using `ms-auto`.
4.  **AbilityUnlockCard Component**:
    *   Converted `card-header` to a flexbox.
    *   Added the `active-dot` icon after the ability name when in progress.
5.  **CadenceDragExpander Component**:
    *   Added `IsActive` parameter.
    *   Added the `active-dot` icon after the cadence name when any of its abilities are being unlocked.
    *   Updated `CadencePanel.razor` to calculate and pass this state.

## Verification
*   Verified that all projects build successfully.
*   Ran 205 unit tests, all passing.
*   Ran project health check, which passed reachability and economic sustainability simulations.
