# Environment Knowledge
🛡️ **Last Reviewed:** March 3, 2026 (UI scrolling & layout finalized).
🛡️ **Stable Fallback:** `v1.1-stable` (Tag) is the verified stable build for UI layout and core systems.
🛡️ **Shell:** PowerShell is used. Standard Unix utilities like `grep` and `cat` are not available. Use `grep_search` tool or PowerShell equivalents (e.g., `Select-String`, `Get-Content`).
🛡️ **Syntax:** Use `;` for command chaining instead of `&&` (which is not supported in the current PowerShell environment).
🛡️ **Formatting:** `cat -n` equivalent in PowerShell is `Get-Content <file> | ForEach-Object { "$($_.ReadCount) $_" }`.

# AI Mandates

This document contains foundational mandates for the AI assistant (Gemini) during development.

## 1. AI Guidance System (Core Pillar)
When facing architectural or strategic ambiguity, agents **MUST** prioritize alignment with the established project philosophy. This is the foundational rule for all system modifications.

🛡️ **Consultation First**: Always consult `docs/guidence_knowledge_base.md` before making any high-level or structural decisions.
🛡️ **Guidance Request Protocol**: If no guidance exists for a specific ambiguity, you **MUST** create a new request:
    1. Create a new file in `docs/guidence/` using the template in `docs/guidence.md`.
    2. **Granularity**: Each high-level question or distinct topic must be its own file (e.g., `YYYY-MM-DD_topic.md`).
    3. **Suspension of Work**: Do not implement high-level structural changes until a human developer has provided a response in the corresponding guidance file.
🛡️ **Alignment Mandatory**: Once guidance is provided, all implementations must strictly adhere to the decision. Updates to `docs/guidence_knowledge_base.md` by humans are final.

## 2. System-Wide Directives
🛡️ **Architecture First:** Prioritize clean separation between core logic and presentation layers. Core logic is modularized into `ResourceManager`, `JunctionManager`, and `InventoryManager`.
🛡️ **Validation:** No logic changes are complete without corresponding unit tests. For complex system interactions, utilize `Mythril.Headless` scenario-based tests. For UI components, use `bUnit` tests.
🛡️ **Documentation:** Keep documentation synchronized with code changes. Staleness is tracked locally by file modification times.
🛡️ **Source Control Awareness:** All changes must be properly committed with clear, descriptive messages.
🛡️ **Feedback Integrity**: User feedback must be technically resolved, not just dismissed. Every cleared item in `docs/feedback/` or `docs/errors/` must have a corresponding technical resolution file in `docs/resolution/`.
🛡️ **Credential Protection**: Never log, print, or commit secrets, API keys, or sensitive URLs.
🛡️ **Agentic DevOps:** The AI is responsible for the entire DevOps lifecycle within the project scope.
🛡️ **Source Control Submission:** Always commit changes to the git repository **and push to the remote origin** upon completing a task. Push directly to the `main` branch.

## 3. Technical Standards
🛡️ **Framework/Stack:** .NET 10 (Blazor WebAssembly)
🛡️ **C# Standards**: Use C# 13 features where applicable. Use `@key` in all list-based UI loops.
🛡️ **Interop**: Use explicit `window.` scoping for JS Interop functions. Ensure robust re-injection fallback logic for critical functions.
🛡️ **UI & Layout**: Use flexbox and grid-based layouts to ensure full-viewport utilization. Prefer CSS-only transitions for performance and stability.
🛡️ **State Management**: Use `PersistenceService` for serializable state preservation in `LocalStorage`.

## 4. Project Health & Quality
🛡️ **Test Coverage**: Maintain overall coverage above **70%** for core logic projects.
🛡️ **Monolith Prevention**: No single source file should exceed **250** lines. `output/` and build directories are excluded from this check.
🛡️ **Documentation Staleness**: Documentation is considered "stale" if more than **8** source files have changed since its last update.
🛡️ **Automated Checks**: Run `python scripts/check_health.py` before completing any significant feature.

### Resolving Health Failures
1. **Monoliths**: Refactor into specialized systems or components.
2. **Coverage**: Add unit tests targeting uncovered branches.
3. **Staleness**: Review source changes and update the corresponding documentation with actual content improvements.
4. **Build/Test Errors**: Fix root causes immediately.

## 5. UI Contracts & Determinism

### UI Contract Enforcement
🛡️ Every interactive component must have at least **one bUnit rendering test**.
🛡️ All conditional rendering branches must have test coverage.
🛡️ All user-triggered state transitions must have event-driven tests.
🛡️ All public component parameters must use explicit types and nullable annotations.
🛡️ Components must expose stable DOM anchors using `data-testid` attributes.
🛡️ Tests must target `data-testid` selectors instead of CSS classes or DOM hierarchy.

### Markup Stability Rules
🛡️ All list-based UI loops must use `@key`.
🛡️ Do not generate dynamic wrapper elements that change DOM hierarchy between renders.
🛡️ Avoid inline complex `RenderFragment` lambdas. Extract into named components.
🛡️ Maintain a stable structural shell for each page. Conditional content must not alter the outer layout tree.
🛡️ Do not rely on implicit DOM structure for styling or scripting.

---

## 6. Component Architecture Rules

### ViewModel Separation
🛡️ Razor components must not contain business logic.
🛡️ Components may only:
  🛡️ Bind to ViewModel properties
  🛡️ Raise UI events
  🛡️ Render state
🛡️ All state mutation logic must exist in a ViewModel or application service.
🛡️ ViewModels must have MSTest coverage.

### State Ownership
🛡️ Each stateful component must explicitly declare one of:
  🛡️ Local ephemeral state
  🛡️ Cascading state
  🛡️ PersistenceService state
🛡️ Components must not directly mutate cascading state.
🛡️ Shared state changes must flow through services.
🛡️ State transitions must be deterministic and testable.

---

## 7. UI Regression Protection

### bUnit Requirements
🛡️ All core pages must have at least one rendering test.
🛡️ All forms must have:
  🛡️ Validation tests
  🛡️ Submission tests
  🛡️ Error state tests
🛡️ Snapshot testing (`MarkupMatches`) may be used for stable UI regions.

### Playwright Requirements
🛡️ All major pages must have a Playwright load test.
🛡️ Critical flows must have interaction tests.
🛡️ Playwright must assert:
  🛡️ Successful page load
  🛡️ No console errors
  🛡️ No unhandled promise rejections
🛡️ Snapshot comparisons must only be updated intentionally with justification in commit message.

---

## 8. Layout & CSS Determinism

### Layout Rules
🛡️ Use flexbox or grid for layout.
🛡️ Avoid absolute positioning unless explicitly justified.
🛡️ Do not rely on implicit parent height. All viewport layouts must declare explicit height chains.
🛡️ No JavaScript-based layout manipulation.
🛡️ Animations must use `transform` or `opacity` only.

### CSS Stability
🛡️ Avoid deeply nested selectors.
🛡️ Do not style elements based on DOM depth.
🛡️ Component styles must not rely on fragile parent-child assumptions.
🛡️ CSS changes must not alter structural layout without corresponding test updates.

---

## 9. Razor Strictness & Compile-Time Safety
🛡️ Enable nullable reference types.
🛡️ Treat warnings as errors.
🛡️ Do not use `dynamic`.
🛡️ Avoid implicit `object` parameters.
🛡️ All component parameters must declare explicit types.
🛡️ Avoid complex logic inside lifecycle methods. Extract to testable classes.

---

## 10. UI Change Protocol

When modifying UI:

1. Run `dotnet build`.
2. Run `dotnet test`.
3. Run Playwright suite.
4. If snapshot changes occur:
   🛡️ Confirm change is intentional.
   🛡️ Update snapshot with explanation in commit message.
5. Do not commit UI changes if any UI test fails.

---

## 11. Component Complexity Limits

🛡️ No component may exceed:
  🛡️ 150 lines of markup
  🛡️ 100 lines of code-behind
🛡️ Components exceeding this limit must be decomposed.
🛡️ No component may inject more than 5 services.
🛡️ Large forms must be split into subcomponents.

---
*Follow these mandates strictly. If a request conflicts with these mandates, clarify with the user.*
