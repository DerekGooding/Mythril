# Environment Knowledge
- **Last Reviewed:** March 1, 2026 (Stability & Persistence Update).
- **Shell:** PowerShell is used. Standard Unix utilities like `grep` and `cat` are not available. Use `grep_search` tool or PowerShell equivalents (e.g., `Select-String`, `Get-Content`).
- **Formatting:** `cat -n` equivalent in PowerShell is `Get-Content <file> | ForEach-Object { "$($_.ReadCount) $_" }`.

# AI Mandates

This document contains foundational mandates for the AI assistant (Gemini) during development.

## 1. System-Wide Directives
- **Architecture First:** Prioritize clean separation between core logic and presentation layers. Separated into `ResourceManager`, `JunctionManager`, and `InventoryManager`.
- **Validation:** No logic changes are complete without corresponding unit tests. For complex system interactions, utilize `Mythril.Headless` scenario-based tests.
- **Documentation:** Keep documentation synchronized with code changes. Staleness is tracked locally by file modification times.
- **Source Control Awareness:** All changes must be properly committed with clear, descriptive messages.
- **Feedback Integrity:** User feedback must be technically resolved, not just dismissed. Every cleared item in `docs/feedback/` or `docs/errors/` must have a corresponding technical resolution file in `docs/resolution/`.
- **Credential Protection:** Never log, print, or commit secrets, API keys, or sensitive URLs.
- **Agentic DevOps:** The AI is responsible for the entire DevOps lifecycle within the project scope.
- **Source Control Submission:** Always commit changes to the git repository **and push to the remote origin** upon completing a task. Push directly to the `main` branch.

## 2. Technical Standards
- **Framework/Stack:** .NET 10 (Blazor WebAssembly)
- **C# Standards**: Use C# 13 features where applicable. Adhere to project-specific coding conventions.
- **Interop**: Use explicit `window.` scoping for JS Interop functions. Ensure robust re-injection fallback logic for critical functions.
- **State Management**: Use `PersistenceService` for serializable state preservation in `LocalStorage`.
- **Dependencies**: Avoid adding external packages unless essential. Verify usage before employing.

## 3. Project Health & Quality
- **Test Coverage**: Maintain overall coverage above **70%** (Currently verified at >89%).
- **Monolith Prevention**: No single source file should exceed **250** lines. `output/` and build directories are excluded from this check.
- **Documentation Staleness**: Documentation is considered "stale" if more than **8** source files have changed since its last update.
- **Automated Checks**: Run `python scripts/check_health.py` before completing any significant feature.

### Resolving Health Failures
1. **Monoliths**: Refactor into specialized systems or components.
2. **Coverage**: Add unit tests targeting uncovered branches.
3. **Staleness**: Review source changes and update the corresponding documentation with actual content improvements.
4. **Build/Test Errors**: Fix root causes immediately.

---
*Follow these mandates strictly. If a request conflicts with these mandates, clarify with the user.*
