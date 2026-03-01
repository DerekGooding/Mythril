# Mythril Agent Configuration

**Documentation Status:** Last reviewed March 1, 2026 (Logging & Dev Bridge). Consistent with recent record struct and Blazor fixes.

## 🧭 Project Ethos
Mythril is an **agentically developed** project. Agents are not just contributors but primary architects. This document provides the necessary context for AI agents (Gemini CLI) to operate effectively within this workspace.

## 📜 Agentic Mandates
All development must strictly adhere to [GEMINI.md](GEMINI.md). Key mandates include:
- **Architecture First**: Prioritize clean separation of concerns.
- **Validation**: No change is complete without passing unit tests and maintaining >70% coverage. Use `Mythril.Headless` for complex logic verification.
- **Source Control**: Use descriptive commit messages and follow git best practices.
- **Health Checks**: Always run `python scripts/check_health.py` before finalizing any significant task.
- **Status Report**: Run `python scripts/generate_report.py` at the end of a session to leave a clean state record (`STATUS.md`).
- **PowerShell Mastery**: Use PowerShell syntax (e.g., `;` for command chaining) for all CLI operations on this Windows environment.

## 🛠️ Technical Context
- **Framework**: .NET 9 (Blazor WebAssembly frontend, C# 13 backend).
- **Core Design**: Partial record structs and source-generated dependency injection (`SimpleInjection`).
- **Data Management**: Centralized via `ContentHost` and various `IContent<T>`/`ISubContent<K, V>` implementations. Logic resides in `Mythril.Data` (merged Core).
- **Testing**: MSTest with `Parallelize` disabled. `Mythril.Headless` for integration scenarios.
- **Persistence**: `PersistenceService` manages `LocalStorage` saves.

## ⚙️ Operational Workflow
1. **Research**: Map the codebase using `grep_search` and `glob`.
2. **Strategy**: Formulate a plan and share it concisely.
3. **Execution**: Implement changes surgicaly, avoiding unrelated refactoring.
4. **Validation**: Run the health check script to ensure zero monoliths, >70% coverage, and fresh documentation. Run `.\run_ai_test.ps1` for logic changes.

## 🧬 Project Health Metrics
- **Max Lines**: 250 per file (excluding `wwwroot/lib`).
- **Min Coverage**: 70% overall.
- **Docs Staleness**: 8 source file changes threshold.
- **Health Resolution**: Fix root causes (refactor monoliths, add tests, or fully update stale docs). Documentation is considered "stale" if it lags behind source changes; resolving this requires meaningful content updates that reflect the current project state.

---
*Operationalize the above for maximum technical integrity.*
