# User Feedback Management

## Developer Mode Bridge
For local development, you can start a bridge server that automatically captures runtime errors and feedback from the browser and writes them to the filesystem immediately.
1. Unlock Developer Mode in the browser (click version 3x).
2. Start the bridge:
```bash
python scripts/dev_bridge.py
```
When an error occurs or you click "Save Locally" in the game, the bridge will print the created file path to your console.

## Feedback Synchronization
The primary method for an agent to ingest feedback and error reports is via a **Sync String** provided from the in-game developer console.

To ingest and sort reports into `docs/feedback/` and `docs/errors/`:
```bash
python scripts/sync_feedback.py '<json_string_or_file_path>'
```
This script will automatically detect the report type and place it in the correct directory, printing the full path of each created file to the console.

## Health Check Mandate
The `docs/feedback/` and `docs/errors/` directories are "Zero-Backlog" zones. If any files exist in these directories, the automated health check will fail.

To resolve an item and pass the health check:
1. **Analyze:** Understand the issue or request.
2. **Implement:** Apply the necessary code changes, tests, or content updates.
3. **Verify:** Ensure the fix or feature works as expected and passes all tests.
4. **Resolve:** Create a resolution file in `docs/resolution/` (naming: `YYYY-MM-DD_resolution_short_description.md`) explaining the technical solution.
5. **Archive:** Delete the original file from `docs/feedback/` or `docs/errors/`.

## Resolution Template
All files in `docs/resolution/` must use this template:

```markdown
# Resolution: [Short Title]

**Date:** YYYY-MM-DD
**Resolved By:** [Agent Name / Human Name]
**Feedback Source:** [Reference original item]

## Technical Solution
[Detailed explanation of how the problem was solved or feature implemented]

## Changes Made
- [List files modified]
- [List new tests added]

## Verification
[Details on how the fix was verified - tests run, manual checks, etc.]
```

## Feedback Template
If manually creating a feedback file in `docs/feedback/`, use this format:

```markdown
# Feedback: [Short Title]

**Date:** YYYY-MM-DD
**Type:** [Bug / Feature Request / Suggestion]
**Source:** [User Name / Community Platform]

## Description
[Detailed description of the feedback]

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
```
