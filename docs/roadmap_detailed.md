# Detailed Project Roadmap

This document provides explicit implementation details for each active goal in the [Roadmap](roadmap.md).

---

## 1. Auto-Quest Visualizer (Prep Time)
- **Objective**: Improve the "aliveness" of the auto-quest loop without changing balance.
- **Key Deliverables**:
    - [ ] Update `QuestProgressCard.razor` to handle a "Preparing" state.
    - [ ] When a recurring quest finishes and auto-quest is ON, trigger a 1.5s visual delay before starting the next loop.
    - [ ] Display a "Preparing next cycle..." label or animation during this delay.
    - [ ] Ensure `resourceManager.Tick` does not advance progress during this visual delay.

## 2. Junction Prediction UI
- **Objective**: Provide clear feedback on the power of magic items before they are junctioned.
- **Key Deliverables**:
    - [ ] Update `DragDropService` to support a "Hovered Target" or similar state to communicate which stat slot is being targeted.
    - [ ] Modify `CharacterDisplay.razor` to calculate the potential delta between the currently junctioned spell and the dragged spell in real-time.
    - [ ] Display the delta (e.g. "+5" in green or "-2" in red) next to the stat value during the drag operation.
    - [ ] Ensure the preview updates if the quantity of the dragged spell in inventory changes during the drag (edge case).

## 3. Universal Stat Multipliers
- **Objective**: Ensure character progression through Junctioning is mechanically relevant for every task.
- **Key Deliverables**:
    - [ ] Update `ResourceManager_Quests.cs` to apply multiplicative stat scaling to all task types (Quests, Cadence Unlocks, Refinements).
    - [ ] Formula: `EffectiveDuration = BaseDuration / (1.0 + (RelevantStat / 100.0))`.
    - [ ] Enforce a `Math.Max(0.5, ...)` floor for all task durations in the `StartQuest` logic.
    - [ ] Audit `quest_details.json` and `cadences.json` to ensure every task has at least one associated "primary stat" for scaling.

## 4. Progression Stat Gates
- **Objective**: Create content "walls" that require the player to engage with Junctioning.
- **Key Deliverables**:
    - [ ] Update `QuestDetail` model and JSON schema to include an optional `RequiredStats` dictionary (Stat Name -> Minimum Value).
    - [ ] Modify `ResourceManager.CanAfford` to also check `RequiredStats` against the character's junctioned totals.
    - [ ] Update `QuestCard.razor` to display requirement icons for stats (e.g. a small sword icon for 15 Strength).
    - [ ] Apply gates to mid-tier quests in Iron Mines (Strength) and Whispering Woods (Speed).

## 5. Content: The Sentinel Cadence
- **Objective**: Add a defensive job focused on survival and resource management.
- **Key Deliverables**:
    - [ ] Add `The Sentinel` entry to `cadences.json`.
    - [ ] Ability 1: `J-Vit` (Allows junctioning to Vitality).
    - [ ] Ability 2: `Magic Pocket II` (Increases global magic capacity to 100).
    - [ ] Define unlock costs: `Iron Ore x50`, `Ancient Bark x20`, `Gold x2000`.

## 6. Content: Sun-Drenched Desert Biome
- **Objective**: Expand the map with a mid-game desert environment.
- **Key Deliverables**:
    - [ ] Add `Sun-Drenched Desert` to `locations.json`.
    - [ ] Add new items to `items.json`: `Sun-baked Scale`, `Solar Essence`.
    - [ ] Define quests in `quest_details.json`: 
        - `Scavenge Scrap` (Recurring, rewards Iron Ore and Gold).
        - `Hunt Sand-Sharks` (Recurring, rewards Sun-baked Scale).
        - `Locate the Hidden Oasis` (Single, rewards Solar Essence).

## 7. Content: Magic Expenditure Quests
- **Objective**: Deepen the link between refinement and questing.
- **Key Deliverables**:
    - [ ] Update `ResourceManager.CanAfford` and `PayCosts` to handle Spell items as requirements.
    - [ ] Add quests to `quest_details.json` that consume magic:
        - Example: "Purify the Grove" requires `Cure I x20`.
        - Example: "Power the Forge" requires `Fire I x50`.

---

## Future Exploration
- [ ] **Legendary Questline "The First Spark"**: Multi-stage ruins questline.
- [ ] **Master Cadence "Mythril Weaver"**: Endgame progression path.
- [ ] **Refinement Mastery**: Efficiency bonuses for repeated crafting.
- [ ] **Dynamic Pricing**: Shop costs that fluctuate based on global quest completions.
