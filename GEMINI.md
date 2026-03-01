# Environment Knowledge
- **Last Reviewed:** Feb 28, 2026.
- **Shell:** PowerShell is used. Standard Unix utilities like `grep` and `cat` are not available. Use `grep_search` tool or PowerShell equivalents (e.g., `Select-String`, `Get-Content`).
- **Formatting:** `cat -n` equivalent in PowerShell is `Get-Content <file> | ForEach-Object { "$($_.ReadCount) $_" }`.

# AI Mandates

This document contains foundational mandates for the AI assistant (Gemini) during development.

## 1. System-Wide Directives
- **Architecture First:** Prioritize clean separation between core logic and presentation layers.
- **Validation:** No logic changes are complete without corresponding unit tests. For complex system interactions (e.g. quests, cadence, persistence), utilize the `Mythril.Headless` project to run scenario-based tests via `test_scenario.json` and verify the output `state.json`.
- **Documentation:** Keep documentation synchronized with code changes.
- **Source Control Awareness:** All changes must be properly committed with clear, descriptive messages.
- **Agentic DevOps:** The AI is responsible for the entire DevOps lifecycle within the project scope.
- **Source Control Submission:** Always commit changes to the git repository upon completing a task. Use clear, descriptive commit messages.

## 2. Technical Standards
- **Framework/Stack:** .NET 9 (Blazor)
- **Operating System:** Windows
- **Shell:** Use PowerShell for all CLI operations (e.g., use `;` instead of `&&` for command chaining).
- **Style:** Adhere to project-specific coding conventions and file-scoped structures.
- **State Management:** Ensure state is manageable and, where applicable, serializable.
- **Dependencies:** Avoid adding external packages unless essential or explicitly requested.

## 3. Project Health & Quality
- **Test Coverage:** Maintain overall coverage above **70%**.
- **Monolith Prevention:** No single source file should exceed **250** lines.
- **Documentation Staleness:** Documentation is considered "stale" if more than **8** source files have changed since its last update.
- **Health Integrity:** Resolve staleness by providing actual content improvements. Documentation must be fully reviewed and updated to reflect the current state of the project; simple line edits or "touching" files to reset staleness counters is strictly prohibited.
- **Automated Checks:** Run `python scripts/check_health.py` before completing any significant feature.

### Resolving Health Failures
1. **Monoliths:** Refactor into specialized systems or components.
2. **Coverage:** Add unit tests targeting uncovered branches.
3. **Staleness:** Review source changes and update the corresponding documentation.
4. **Build/Test Errors:** Fix root causes immediately.

---
*Follow these mandates strictly. If a request conflicts with these mandates, clarify with the user.*
