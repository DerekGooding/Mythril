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
    - [x] Create `scripts/generate_report.py`.
    - [x] Script should:
        - [x] Run `python scripts/check_health.py`.
        - [x] Run `.\run_ai_test.ps1`.
        - [x] Aggregate coverage, monolith counts, and headless test results.
    - [x] Output a consolidated `STATUS.md` in the project root.
    - [x] Update mandates to require running this script before ending a session.

## 5. Character Stats & Junctioning UI
- **Objective**: Implement the core FF8-inspired stat-boosting mechanic.
- **Key Deliverables**:
    - [ ] Update `CharacterDisplay.razor` to show a breakdown of the character's current stats and active junctions.
    - [ ] Create a "Junctioning" sub-menu:
        - [ ] List available Stats (Strength, Magic, Vitality, etc.).
        - [ ] Check if the current Cadence has the matching "J-Stat" ability (e.g., "J-Str").
        - [ ] Allow assigning a Magic item (e.g., "Fire I") to an enabled Stat slot.
    - [ ] Implement the "Junction" data model to track these character-specific mappings.

## 6. Linear Stat Augment Calculator
- **Objective**: Calculate dynamic stat bonuses based on magic quantities.
- **Key Deliverables**:
    - [ ] Implement a `StatCalculator` that aggregates base stats + junction bonuses.
    - [ ] Formula: `TotalStat = BaseStat + (MagicQuantity * StatMultiplierPerUnit)`.
    - [ ] Ensure `ResourceManager` recalculates effective durations immediately when magic quantities change.

## 7. Global Magic Capacity Enforcement
- **Objective**: Enforce the strategic resource limitation of 30 per spell.
- **Key Deliverables**:
    - [ ] Update `InventoryManager.Add` to cap Magic items at the current `GlobalLimit` (default 30).
    - [ ] Add "Capacity Expansion" abilities to the Cadence tree (e.g., "Magic Pocket I").
    - [ ] Implement logic to update the `GlobalLimit` when these abilities are unlocked.

## 8. Cadence Assignment UI: Exclusivity Logic
- **Objective**: Enforce the rule that each Cadence is a unique entity.
- **Key Deliverables**:
    - [ ] Create a dedicated "Party Management" UI.
    - [ ] Implement "Equip" logic:
        - [ ] Check if the Cadence is already equipped by another character.
        - [ ] If yes, prompt to "Swap" or deny.
    - [ ] Implement "Unequip" logic:
        - [ ] **Crucial**: Automatically clear all Junctions for the character when their Cadence is removed.

## 9. Tutorial Questline: "The First Refinement"
- **Objective**: A scripted sequence to onboard players to the core loop.
- **Key Deliverables**:
    - [ ] Phase 1: Quest "Tutorial Section" rewards "Basic Gem".
    - [ ] Phase 2: Instruction to unlock "Refine Fire" in the Cadence tree.
    - [ ] Phase 3: Instruction to use the Workshop to refine "Basic Gem" into "Fire I".
    - [ ] Phase 4: Instruction to Junction "Fire I" to "Strength" (requires "J-Str" on starter cadence).

## 10. Content: The Whispering Woods Biome
- **Objective**: Expand the map with early-game nature materials.
- **Key Deliverables**:
    - [ ] Add `Whispering Woods` to the `Locations` data.
    - [ ] Add new items: `Mana Leaf` (Material), `Fire Shard` (Material).
    - [ ] Define "Gathering" quests with related rewards.

## 11. Content: New Cadence "The Arcanist"
- **Objective**: Introduce the first magic-specialized job.
- **Key Deliverables**:
    - [ ] Define `The Arcanist` Cadence.
    - [ ] Abilities: `Refine Ice`, `J-Magic`, `Magic Pocket I`.
    - [ ] Requirements: `Gold x500`, `Mana Leaf x10`.

---

## Future Exploration
- [ ] **Legendary Questline "The First Spark"**: Multi-stage ruins questline.
- [ ] **Master Cadence "Mythril Weaver"**: Endgame progression path.
- [ ] **Refinement Mastery**: Efficiency bonuses for repeated crafting.
- [ ] **Infinite Scaling & Prestige**: Concepts for extreme long-term play.

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
- [x] **Agentic "Status Report" Utility**: Implemented automated aggregation.
