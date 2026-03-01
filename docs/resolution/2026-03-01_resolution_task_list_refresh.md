# Resolution: Task lists not refreshing live

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Derek

## Technical Solution
The issue was twofold: 1) The CSS `max-height` transition in `expander.js` was keeping the height fixed after expansion, preventing new quests from being visible until the expander was toggled. 2) One-time quests were not being correctly removed from the UI state on completion. 

The JavaScript expander logic has been updated to set `maxHeight = "none"` after expansion completes, allowing the container to grow or shrink dynamically as its content changes.

## Changes Made
- Modified `Mythril.Blazor\wwwroot\expander.js` to clear `maxHeight` after the transition ends.
- Updated `QuestLifecycleTests.cs` to verify that `PayCosts` correctly locks (removes) single-time quests from the usable locations list.

## Verification
- Verified that adding new items/unlocking new quests immediately updates the visible list within an already expanded location.
- Verified that single-time quests (like the Prologue) disappear immediately from the hand panel upon starting.
