# Detailed Project Roadmap

This document provides explicit implementation details for each active goal in the [Roadmap](roadmap.md).

---

## 1. Headless Testing Assertion Engine
- **Objective**: Transition from manual state review to automated validation for AI-generated scenarios.
- **Key Deliverables**:
    - [ ] Update `Mythril.Headless.CommandFile` to include an `Assertions` list.
    - [ ] Implement `Assertion` model with `Type` (e.g., `InventoryCount`, `CadenceUnlocked`), `Target`, and `ExpectedValue`.
    - [ ] Add an assertion runner in `Program.cs` that validates the `ResourceManager` state after commands execute.
    - [ ] Return non-zero exit code if any assertion fails, allowing for CI/CD integration of AI tests.

## 2. Blazor State Persistence
- **Objective**: Ensure players can resume their progress between sessions.
- **Key Deliverables**:
    - [ ] Create `PersistenceService` in `Mythril.Blazor.Services`.
    - [ ] Implement JSON serialization for `ResourceManager` (Inventory, Completed Quests, Unlocked Cadences, Assigned Cadences).
    - [ ] Use `IJSRuntime` to interface with browser `LocalStorage`.
    - [ ] Add "Export State" and "Import State" functionality to the `MainLayout` or `Home` page.
    - [ ] Implement auto-save trigger after significant events (Quest completion, Cadence unlock).

## 3. Continuous Health Monitoring (CI/CD)
- **Objective**: Protect the repository from regressions in code quality and coverage.
- **Key Deliverables**:
    - [ ] Update `.github/workflows/deploy.yml` to include a `health-check` job.
    - [ ] Ensure `python 3.x` is installed in the runner environment.
    - [ ] Run `dotnet test` with coverage collection.
    - [ ] Execute `python scripts/check_health.py` and fail the build if any check (Monolith, Coverage, Staleness) returns non-zero.
    - [ ] Ensure deployment only occurs if health checks pass.

## 4. Asynchronous Quest Tick System
- **Objective**: Move away from instant quest completion toward a timer-based mechanic.
- **Key Deliverables**:
    - [ ] Add an `ActiveQuests` list to `ResourceManager`.
    - [ ] Implement a `Tick(int deltaSeconds)` method in `ResourceManager`.
    - [ ] Update `QuestProgress` to calculate progress based on elapsed time relative to `DurationSeconds`.
    - [ ] Refactor `ReceiveRewards` to be triggered by the `Tick` system when progress reaches 100%.
    - [ ] In `Mythril.Blazor`, use a `System.Timers.Timer` to call `Tick` and refresh the UI.

## 5. Cadence Visualizer Component
- **Objective**: Provide a clear, intuitive view of the progression system.
- **Key Deliverables**:
    - [ ] Create a new Blazor component: `CadenceTree.razor`.
    - [ ] Map the `Cadences` and their `Abilities` into a visual hierarchy.
    - [ ] Use CSS to represent locked (greyed out) and unlocked (colored/glowing) states.
    - [ ] Add requirement tooltips that show what items/quests are needed to unlock a specific node.
    - [ ] Integrate the visualizer into the main `CadencePanel`.
