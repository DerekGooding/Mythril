# User Feedback Management

## Closing Feedback
To resolve a feedback item and pass the health check:
1. **Analyze:** Understand the issue or request.
2. **Implement:** Apply the necessary code changes, tests, or content updates.
3. **Verify:** Ensure the fix or feature works as expected and passes all existing tests.
4. **Resolve:** Create a corresponding resolution file in `docs/resolution/` (naming: `YYYY-MM-DD_resolution_short_description.md`) explaining the technical solution and changes made.
5. **Archive:** Move or delete the original feedback file from `docs/feedback/` once the resolution is documented.
6. **Document:** If applicable, update the `README.md` or `ROADMAP.md` to reflect the changes.

## Resolution Template
All files in `docs/resolution/` must use this template:

```markdown
# Resolution: [Short Title]

**Date:** YYYY-MM-DD
**Resolved By:** [Agent Name / Human Name]
**Feedback Source:** [Reference original source]

## Technical Solution
[Detailed explanation of how the problem was solved or feature implemented]

## Changes Made
- [List files modified]
- [List new tests added]

## Verification
[Details on how the fix was verified - tests run, manual checks, etc.]
```

## Remote Synchronization
The game UI automatically submits feedback to a remote Google Sheet if configured. To sync these items to this repository, provide the Web App URL at runtime:
```bash
python scripts/sync_feedback.py --remote --url "<WEB_APP_URL>"
```
*Note: The URL is never stored in the repository for security.*

## Interactive Collector
You can use the automated script to add new feedback entries:
```bash
python scripts/add_feedback.py
```

## Feedback Template
All files in `docs/feedback/` must adhere to the following naming convention: `YYYY-MM-DD_short_description.md` and use this template:

```markdown
# Feedback: [Short Title]

**Date:** YYYY-MM-DD
**Type:** [Bug / Feature Request / Suggestion]
**Source:** [User Name / Community Platform]

## Description
[Detailed description of the feedback]

## Impact
[How this affects the game or user experience]

## Proposed Solution (Optional)
[Steps to resolve or implement]

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
```
