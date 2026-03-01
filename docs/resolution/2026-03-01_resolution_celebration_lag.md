# Resolution: Celebration animation lags

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Derek

## Technical Solution
The lag was caused by an excessive number of confetti particles (100) being generated on every task completion. By reducing the particle count and spread, the performance impact is minimized while maintaining the visual reward. Additionally, the CSS animation class was being incorrectly targeted, which has been corrected.

## Changes Made
- Modified `Mythril.Blazor\wwwroot\index.html` to reduce `particleCount` from 100 to 40 and `spread` from 70 to 50 in `triggerConfettiAt`.
- Updated `Mythril.Blazor\wwwroot\css\completion-effects.css` to correctly target `.task-progress-card.completed` instead of the non-existent `.quest-progress-card`.

## Verification
- Manual verification of multiple simultaneous task completions shows significantly reduced CPU spikes and smoother UI response.
- CSS animation verified to trigger correctly on completion.
