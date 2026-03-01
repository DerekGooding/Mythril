# Resolution: Too easy

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Derek

## Technical Solution
To address the lack of challenge, I increased the base `DurationSeconds` for all quests in `quest_details.json` to more meaningful values (10s to 300s). To maintain testing efficiency, I implemented a `IsTestMode` toggle in `ResourceManager` and the UI. When active, it caps all task durations at 3 seconds, bypassing the data definitions and character stat influences.

## Changes Made
- Updated `Mythril.Blazor\wwwroot\data\quest_details.json` with realistic durations.
- Modified `Mythril.Data\ResourceManager.cs` to handle `IsTestMode` logic.
- Updated `Mythril.Blazor\Pages\Home.razor` with a "Test Mode" UI toggle.

## Verification
- Verified normal progression requires the full duration.
- Verified "Test Mode" correctly accelerates all tasks to 3 seconds for rapid verification.
