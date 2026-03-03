# Detailed Project Roadmap

This document provides explicit implementation details for each active goal in the [Roadmap](roadmap.md).

---

## 1. Auto-Quest Visualizer (Prep Time)
- [x] **Objective**: Improve the "aliveness" of the auto-quest loop without changing balance.
- [x] **Key Deliverables**:
    - [x] Update `QuestProgressCard.razor` to handle a "Preparing" state.
    - [x] When a recurring quest finishes and auto-quest is ON, trigger a 1.5s visual delay before starting the next loop.
    - [x] Display a "Preparing next cycle..." label or animation during this delay.
    - [x] Ensure `resourceManager.Tick` does not advance progress during this visual delay.

## 2. Junction Prediction UI
- [x] **Objective**: Provide clear feedback on the power of magic items before they are junctioned.
- [x] **Key Deliverables**:
    - [x] Update `DragDropService` to support a "Hovered Target" or similar state to communicate which stat slot is being targeted.
    - [x] Modify `CharacterDisplay.razor` to calculate the potential delta between the currently junctioned spell and the dragged spell in real-time.
    - [x] Display the delta (e.g. "+5" in green or "-2" in red) next to the stat value during the drag operation.
    - [x] Ensure the preview updates if the quantity of the dragged spell in inventory changes during the drag.

## 3. Universal Stat Multipliers
- [x] **Objective**: Ensure character progression through Junctioning is mechanically relevant for every task.
- [x] **Key Deliverables**:
    - [x] Update `ResourceManager_Quests.cs` to apply multiplicative stat scaling to all task types (Quests, Cadence Unlocks, Refinements).
    - [x] Formula: `EffectiveDuration = BaseDuration / (1.0 + (RelevantStat / 100.0))`.
    - [x] Enforce a `Math.Max(0.5, ...)` floor for all task durations in the `StartQuest` logic.
    - [x] Audit `quest_details.json` and `cadences.json` to ensure every task has at least one associated "primary stat" for scaling.

## 4. Progression Stat Gates
- [x] **Objective**: Create content "walls" that require the player to engage with Junctioning.
- [x] **Key Deliverables**:
    - [x] Update `QuestDetail` model and JSON schema to include an optional `RequiredStats` dictionary (Stat Name -> Minimum Value).
    - [x] Modify `ResourceManager.CanAfford` to also check `RequiredStats` against the character's junctioned totals.
    - [x] Update `QuestCard.razor` to display requirement icons for stats (e.g. a small sword icon for 15 Strength).
    - [x] Apply gates to mid-tier quests in Iron Mines (Strength) and Whispering Woods (Speed).

## 5. Content: The Sentinel Cadence
- [x] **Objective**: Add a defensive job focused on survival and resource management.
- [x] **Key Deliverables**:
    - [x] Add `The Sentinel` entry to `cadences.json`.
    - [x] Ability 1: `J-Vit` (Allows junctioning to Vitality).
    - [x] Ability 2: `Magic Pocket II` (Increases global magic capacity to 100).
    - [x] Define unlock costs: `Iron Ore x50`, `Ancient Bark x20`, `Gold x2000`.

## 6. Content: Sun-Drenched Desert Biome
- [x] **Objective**: Expand the map with a mid-game desert environment.
- [x] **Key Deliverables**:
    - [x] Add `Sun-Drenched Desert` to `locations.json`.
    - [x] Add new items to `items.json`: `Sun-baked Scale`, `Solar Essence`.
    - [x] Define quests in `quest_details.json`: 
        - `Scavenge Scrap` (Recurring, rewards Iron Ore and Gold).
        - `Hunt Sand-Sharks` (Recurring, rewards Sun-baked Scale).
        - `Locate the Hidden Oasis` (Single, rewards Solar Essence).

## 7. Content: Magic Expenditure Quests
- [x] **Objective**: Deepen the link between refinement and questing.
- [x] **Key Deliverables**:
    - [x] Update `ResourceManager.CanAfford` and `PayCosts` to handle Spell items as requirements.
    - [x] Add quests to `quest_details.json` that consume magic (e.g. "Purify the Grove" requires Cure I x20).
