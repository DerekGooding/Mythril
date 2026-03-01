# Resolution: Celebration lag

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Admin (via new feedback collection)

## Technical Solution
The lag was primarily caused by `QuestProgressCard.razor` triggering the `TriggerConfetti` and `OnCompletionAnimationEnd` logic multiple times during the 1-second delay period because `OnParametersSetAsync` would fire again before the animation finished. I added a `_hasTriggeredCompletion` boolean flag to ensure the completion logic only runs exactly once per quest.

## Changes Made
- Modified `Mythril.Blazor\Components\QuestProgressCard.razor` to include a completion gate flag.

## Verification
- Verified that completing multiple quests simultaneously no longer stacks multiple confetti calls per card, resulting in smooth performance.
