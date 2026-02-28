# Detailed Project Roadmap

This document provides explicit implementation details for each active goal in the [Roadmap](roadmap.md).

---

## 1. Comprehensive Persistence (Active Quests)
- **Objective**: Fully restore the game state, including mid-progress tasks.
- **Key Deliverables**:
    - [ ] Create `QuestProgressDTO` model to handle serialization of active tasks.
    - [ ] Update `SaveData` to include a list of `QuestProgressDTO`.
    - [ ] Modify `PersistenceService` to map DTOs back to `QuestProgress` objects using `Items`, `Quests`, and `Character` lookups.
    - [ ] Ensure `StartTime` and `SecondsElapsed` are preserved to maintain progress continuity.

## 2. Item Refinement Workshop UI
- **Objective**: Expose the `ItemRefinements` logic to the player.
- **Key Deliverables**:
    - [ ] Create `Workshop.razor` component and add it as a new tab in `Home.razor`.
    - [ ] Inject `ItemRefinements` and display available recipes based on the character's unlocked abilities.
    - [ ] Implement a "Craft" interaction that validates requirements and triggers refinement rewards.
    - [ ] Add visual feedback for successful refinements and missing materials.

## 3. Data-Driven Content Migration
- **Objective**: Decouple game content from the compiled binary for easier balancing.
- **Key Deliverables**:
    - [ ] Create a JSON schema for `Items`, `Quests`, and `Cadences`.
    - [ ] Export current static data to `wwwroot/data/*.json`.
    - [ ] Refactor content classes (e.g., `Items.cs`) to load data from `HttpClient` instead of hardcoded arrays.
    - [ ] Update `ContentHost` to manage the asynchronous loading of JSON content at startup.

## 4. Agentic "Status Report" Utility
- **Objective**: Improve project visibility and onboarding for AI agents.
- **Key Deliverables**:
    - [ ] Create `scripts/generate_report.py`.
    - [ ] Script should:
        - [ ] Run `python scripts/check_health.py`.
        - [ ] Run `.\run_ai_test.ps1`.
        - [ ] Aggregate coverage, monolith counts, and headless test results.
    - [ ] Output a consolidated `STATUS.md` in the project root.
    - [ ] Update mandates to require running this script before ending a session.

## 5. Character Specialization & Stats UI
- **Objective**: Make character differences meaningful through stat-based efficiency.
- **Key Deliverables**:
    - [ ] Update `CharacterDisplay.razor` to show a breakdown of the character's current stats.
    - [ ] Implement a "Stat Influence" formula in `ResourceManager.StartQuest`.
    - [ ] Example: `EffectiveDuration = BaseDuration / (1 + (RelevantStat / 100))`.
    - [ ] Add tooltips explaining which stat influences which quest types.

## 6. Content: The Whispering Woods Biome
- **Objective**: Expand the world map with a nature-themed zone.
- **Key Deliverables**:
    - [ ] Add `Whispering Woods` to the `Locations` data.
    - [ ] Define new quests: "Gather Moonberries" (Recurring), "Defeat Treant Guardian" (Single).
    - [ ] Add new items: `Moonberry` (Consumable), `Ancient Bark` (Material).
    - [ ] Update `QuestDetails` and `QuestUnlocks` to integrate the woods into the early-to-mid game progression.

## 7. Content: Legendary Questline "The First Spark"
- **Objective**: Implement multi-stage progression via sequential quests.
- **Key Deliverables**:
    - [ ] Add a sequence of 3 quests: "Ancient Inscriptions", "Finding the Hearth", "Rekindling the Spark".
    - [ ] Configure `QuestUnlocks` so each quest requires the completion of the previous one.
    - [ ] Add `Mythril Spark` as a unique quest reward.
    - [ ] Implement an unlock trigger that grants a specialized Cadence upon completion of the full chain.

## 8. Content: Master Cadence "Mythril Weaver"
- **Objective**: Provide high-level endgame progression goals.
- **Key Deliverables**:
    - [ ] Define the `Mythril Weaver` Cadence with high-tier requirements (e.g., 1000 Gold, 50 Iron Ore, 10 Ancient Bark).
    - [ ] Add unique abilities: `Mass Refine` (reduces refinement costs), `Essence Harvest` (increases drop rates).
    - [ ] Update `CadenceTree` visualizer to support these high-tier nodes.

---

## Completed Goals
- [x] **AI Mandates & Health Check System**: Established foundation for code quality.
- [x] **Headless AI Testing Framework**: Created `Mythril.Headless` for scenario testing.
- [x] **Core Logic Consolidation**: Merged logic into `Mythril.Data`.
- [x] **Headless Testing Assertion Engine**: Automated state verification.
- [x] **Blazor State Persistence**: Implemented `LocalStorage` save/load.
- [x] **Continuous Health Monitoring (CI/CD)**: Integrated health checks into GitHub Actions.
- [x] **Asynchronous Quest Tick System**: Refactored to real-time timer system.
- [x] **Cadence Visualizer Component**: Built interactive progression tree.
