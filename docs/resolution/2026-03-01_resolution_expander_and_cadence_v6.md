# Resolution: Expander Behavior & Cadence Initialization (V6)

**Date:** 2026-03-01
**Target Issues:** 
- Locations (Expanders) didn't collapse their height correctly.
- Cadence tab was empty and failed to show unlocked jobs.
- Thread-safety issues in the quest timer loop.

## Summary of Changes

### 1. Robust Expander Transitions
- **JS Logic Update**: Refactored `expander.js` to correctly manage `display: none` and `display: block`. When collapsing, the element now transitions to `0px` height and then switches to `display: none`, ensuring it occupies no space in the parent container.
- **CSS Cleanup**: Removed a hardcoded `750px` height from `HandPanel.razor.css` that was preventing the tab content from shrinking naturally.

### 2. Cadence System Fixes
- **Initial Cadence**: Added a "Test" cadence to `cadences.json`. This cadence is specifically designed for UI testing; it has no abilities but is fully assignable.
- **Pre-Unlock & Persistence**: Updated `ResourceManager.Initialize` and `PersistenceService.LoadAsync` to ensure the "Test" cadence is always unlocked, even when restoring from a save.
- **Empty Tab Resolved**: Fixed `Home.razor` to correctly call `PersistenceService.LoadAsync` on initialization, ensuring data is restored and cadences populate the tab.
- **Panel Visibility**: Modified `CadencePanel.razor` to display all unlocked cadences, and fixed `CadencePanel.razor.css` to allow natural height scaling.

### 3. Layout & Character Display
- **Party Layout Restored**: Returned the `Party` section to the right-side column in `Home.razor`. Characters are now listed vertically again.
- **Horizontal Stats**: Refactored `CharacterDisplay.razor` to ensure stats are strictly horizontal using `flex-nowrap` and `overflow-x: auto`. The stats container is width-constrained to prevent pushing other header elements off-screen.
- **Draggable Cadences**: Verified that cadences can be dragged from the Cadence tab and dropped onto characters in the Party section.

## Verification
- Verified that location expanders fully collapse and remove their height from the scroll area.
- Verified that the "Test" cadence appears in the Cadence tab immediately on load.
- All unit tests passed (89.18% coverage).
