# Resolution: Easier way to submit feedback

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Jeff

## Technical Solution
Built a new `FeedbackPanel` component directly into the Blazor UI as a new tab. This panel allows users to enter a title, type, and description for their feedback. Since the app is hosted statically, the feedback is stored in `LocalStorage`. To allow for permanent recording, I added a "Copy Sync String" feature that exports the pending feedback as JSON, which can then be processed by the agent using the new `scripts/sync_feedback.py` tool.

## Changes Made
- Created `Mythril.Blazor\Components\FeedbackPanel.razor`.
- Added "Feedback" tab to `Mythril.Blazor\Pages\Home.razor`.
- Updated `Mythril.Blazor\_Imports.razor` to include the `Services` namespace.
- Created `scripts/sync_feedback.py` for agentic ingestion.

## Verification
- Verified the form correctly saves multiple items to `localStorage`.
- Verified "Copy Sync String" correctly exports the JSON array.
- Verified `sync_feedback.py` correctly parses the JSON and generates Markdown files in `docs/feedback/`.
