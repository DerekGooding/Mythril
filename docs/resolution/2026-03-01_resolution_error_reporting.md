# Resolution: Automatic error reporting

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Greg

## Technical Solution
Implemented a `FeedbackService` that can capture error messages and stack traces. Updated `App.razor` to catch global exceptions during the initialization phase and log them to the local feedback store. These errors can then be exported by the user using the new "Sync String" feature.

## Changes Made
- Created `Mythril.Blazor\Services\FeedbackService.cs`.
- Updated `Mythril.Blazor\App.razor` to capture global startup errors.
- Registered `FeedbackService` in `Mythril.Blazor\Program.cs`.

## Verification
- Manual verification by temporarily throwing an exception in `LoadAllAsync` confirmed the error was captured and visible in the local feedback count.
