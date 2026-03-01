# Resolution: Too easy

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Derek

## Technical Solution
The feedback indicated that 3-second quest durations were too short for normal gameplay. I implemented a `IsTestMode` property in `ResourceManager` that, when enabled, caps all quest and cadence unlock durations at 3 seconds. When disabled (default), the system uses the full `DurationSeconds` defined in the data files and applies character stat influences. I also increased the default durations in `quest_details.json` to range from 10 to 300 seconds to provide a more meaningful progression pace.

## Changes Made
- Modified `Mythril.Data\ResourceManager.cs` to add `IsTestMode` and respect it in `StartQuest`.
- Updated `Mythril.Blazor\wwwroot\data\quest_details.json` with increased durations for all quests.
- Updated `Mythril.Blazor\Pages\Home.razor` to add a UI toggle for Test Mode.

## Verification
- Verified that with Test Mode OFF, quests use their full duration (e.g., 10s for Tutorial).
- Verified that with Test Mode ON, quests complete in 3 seconds regardless of data definition.
- Verified that character stats (Strength/Magic) only influence duration when Test Mode is OFF.
