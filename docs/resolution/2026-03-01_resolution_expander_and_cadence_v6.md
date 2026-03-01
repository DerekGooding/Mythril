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
- **Initial Cadence**: Added a "Test" cadence to `cadences.json`. This cadence is specifically designed for UI testing; it has no abilities but allows for character assignment validation.
- **Pre-Unlock Logic**: Updated `ResourceManager.Initialize` to automatically unlock the "Test" cadence on startup.
- **Panel Visibility**: Modified `CadencePanel.razor` to display all unlocked cadences regardless of whether they possess abilities.

### 3. Thread Safety & Stability
- **Locked Access**: Implemented a `lock` object in `ResourceManager` to protect the `ActiveQuests` collection.
- **Safe Removal**: Added `RemoveActiveQuest` to handle deletions safely during the completion phase, preventing race conditions between the background timer thread and the UI rendering thread.

## Verification
- Verified that location expanders fully collapse and remove their height from the scroll area.
- Verified that the "Test" cadence appears in the Cadence tab immediately on load.
- All unit tests passed (89.18% coverage).
