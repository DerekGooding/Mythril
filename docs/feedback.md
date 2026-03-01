# User Feedback Management

## GitHub Synchronization
The primary method for collecting feedback, bug reports, and feature requests is via **GitHub Issues**. 

To pull open issues from the remote repository into this workspace for processing:
```bash
python scripts/sync_github_issues.py
```
This script converts all open GitHub issues (excluding PRs) into Markdown files in `docs/feedback/`.

## Manual Synchronization
If a user provides a "Sync String" (JSON array) directly from the in-game UI, use:
```bash
python scripts/sync_feedback.py '<json_string>'
```

## Closing Feedback
To resolve a feedback item and pass the health check:
1. **Sync:** Run `python scripts/sync_github_issues.py` to ensure all remote items are tracked.
2. **Analyze:** Understand the issue or request.
3. **Implement:** Apply the necessary code changes, tests, or content updates.
4. **Verify:** Ensure the fix or feature works as expected and passes all tests.
5. **Resolve:** Create a resolution file in `docs/resolution/` (naming: `YYYY-MM-DD_resolution_short_description.md`) explaining the technical solution.
6. **Archive:** Delete the feedback file from `docs/feedback/` and close the GitHub Issue manually.

## Resolution Template
All files in `docs/resolution/` must use this template:

```markdown
# Resolution: [Short Title]

**Date:** YYYY-MM-DD
**Resolved By:** [Agent Name / Human Name]
**Feedback Source:** [Reference original source, e.g., GitHub Issue #123]

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
