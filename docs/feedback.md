# User Feedback Management

## Closing Feedback
To resolve a feedback item and pass the health check:
1. **Analyze:** Understand the issue or request.
2. **Implement:** Apply the necessary code changes, tests, or content updates.
3. **Verify:** Ensure the fix or feature works as expected and passes all existing tests.
4. **Close:** Delete the corresponding feedback file from `docs/feedback/`.
5. **Document:** If applicable, update the `README.md` or `ROADMAP.md` to reflect the changes.

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
