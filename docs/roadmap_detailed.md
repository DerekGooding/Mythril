# Detailed Project Roadmap

This document provides explicit implementation details for each active goal in the [Roadmap](roadmap.md).

---

## 1. Auto-Quest Visualizer (Prep Time)
- **Objective**: Improve the "aliveness" of the auto-quest loop without changing balance.
- **Key Deliverables**:
    - [ ] Update `QuestProgressCard.razor` to handle a "Preparing" state.
    - [ ] When a recurring quest finishes and auto-quest is ON, trigger a 1.5s visual delay before starting the next loop.
    - [ ] Display a "Preparing next cycle..." label or animation during this delay.

## 2. Junction Prediction UI
- **Objective**: Provide clear feedback on the power of magic items before they are junctioned.
- **Key Deliverables**:
    - [ ] Update `DragDropService` to support a "Hovered Target" or similar state.
    - [ ] Modify `CharacterDisplay.razor` to detect when a Magic item is being dragged over a stat.
    - [ ] Calculate the delta between the currently junctioned spell and the dragged spell.
    - [ ] Display the delta (e.g. "+5" in green or "-2" in red) next to the stat value during the drag operation.

## 3. Universal Stat Multipliers
- **Objective**: Ensure character progression through Junctioning is mechanically relevant for every task.
- **Key Deliverables**:
    - [ ] Update `ResourceManager_Quests.cs` to apply multiplicative stat scaling to all task types.
    - [ ] Formula: `EffectiveDuration = BaseDuration / (1.0 + (RelevantStat / 100.0))`.
    - [ ] Enforce a `Math.Max(0.5, ...)` floor for all task durations.
    - [ ] Define stat mappings for all existing quests (e.g. Speed for gathering, Strength for combat).

## 4. Progression Stat Gates
- **Objective**: Create content "walls" that require the player to engage with Junctioning.
- **Key Deliverables**:
    - [ ] Update `QuestData` and JSON schema to include an optional `RequiredStats` dictionary.
    - [ ] Modify `ResourceManager.CanAfford` or a new `CanAttempt` method to check character stats against requirements.
    - [ ] Update UI to display required stats on Quest Cards (e.g. "Requires 15 Strength").
    - [ ] Implementation: Apply these gates to mid-tier quests in Iron Mines and Dark Forest.

## 5. Content: The Sentinel Cadence
- **Objective**: Add a defensive job focused on survival and resource management.
- **Key Deliverables**:
    - [ ] Define `The Sentinel` in `cadences.json`.
    - [ ] Abilities: `J-Vit` (Junction Vitality), `Magic Pocket II` (Expand capacity to 100).
    - [ ] Requirements: `Iron Ore x50`, `Ancient Bark x20`, `Gold x2000`.

## 6. Content: Sun-Drenched Desert Biome
- **Objective**: Expand the map with a mid-game desert environment.
- **Key Deliverables**:
    - [ ] Add `Sun-Drenched Desert` to `locations.json`.
    - [ ] Add new items: `Sun-baked Scale`, `Solar Essence`.
    - [ ] Define quests: `Scavenge Scrap` (Recurring), `Hunt Sand-Sharks` (Combat), `Locate the Hidden Oasis` (Unlock).

## 7. Content: Magic Expenditure Quests
- **Objective**: Deepen the link between refinement and questing.
- **Key Deliverables**:
    - [ ] Add quests to existing locations that require expending specific spells.
    - [ ] Example: "Purify the Grove" requires `Cure I x20` and `Gold x500`.
    - [ ] Ensure `ResourceManager.PayCosts` correctly removes magic items from inventory.

---

## Future Exploration
- [ ] **Legendary Questline "The First Spark"**: Multi-stage ruins questline.
- [ ] **Master Cadence "Mythril Weaver"**: Endgame progression path.
- [ ] **Refinement Mastery**: Efficiency bonuses for repeated crafting.

---

## Completed Foundational Goals
- [x] **Comprehensive Persistence**: State restoration works across sessions.
- [x] **Item Refinement Workshop**: Time-based refinement tasks are functional.
- [x] **Data-Driven Content Migration**: Content loaded from JSON.
- [x] **Linear Stat Augment Calculator**: Junction bonuses are correctly applied.
- [x] **Tutorial Questline**: "The First Refinement" sequence.
- [x] **Cadence Visualizer & Assignment UI**: Drag-and-drop job management.
