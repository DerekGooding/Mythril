# Resolution: Location lists don't automatically update

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Admin (via new feedback collection)

## Technical Solution
While the previous fix added dynamic `maxHeight` logic to `expander.js`, the `Expander.razor` component was not triggering an update when its `ChildContent` changed (e.g., when a new quest was unlocked). I restored and enabled the `OnAfterRenderAsync` override in `Expander.razor` to re-trigger the JavaScript `expand` logic whenever the component re-renders while in an expanded state.

## Changes Made
- Enabled `OnAfterRenderAsync` in `Mythril.Blazor\Components\Expander.razor`.

## Verification
- Verified that unlocking new quests or removing one-time quests now immediately updates the visible list within an open location expander without requiring a toggle.
