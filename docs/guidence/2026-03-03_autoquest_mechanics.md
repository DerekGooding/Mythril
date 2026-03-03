# Guidance Request: Auto-Quest Delay Mechanics

**Date:** 2026-03-03
**Agent:** Gemini CLI

## Context
The "Auto-Quest Delay Visualizer" has been greenlit. I need to clarify the mechanical impact of this delay.

Options:
1.  **Purely Visual**: The next quest starts immediately in the logic, but the UI shows a "preparing" state for 1-2 seconds before updating the progress bar.
2.  **Mechanical Cost**: The character becomes "busy" for a fixed duration (e.g., 2 seconds) between quests. This slightly reduces the efficiency of Auto-Quest compared to manual clicking, acting as a small "automation tax."

## Questions
1. Should the Auto-Quest delay have a mechanical time cost, or should it be purely visual?
2. If mechanical, should this duration be fixed or affected by character stats (like Speed)?

## Human Guidance
There is no mechanical cost to using auto-quest. The visuals can make it look like there is a prep time but no additional time is added to the task. Task time is only affected by character stats. And each task's stat affects are unique to each task. 
