# AI Suggestions

## Instructions
New AI-generated suggestions for the project should be placed in this file if it is currently empty or has been fully processed. 

A user will periodically review these suggestions and greenlight specific goals by moving them to the Roadmap or issuing new directives.

**Mandate:** At least 30% of suggestions must focus on **Additional Content** (e.g., new locations, items, quests, or cadences) to ensure the game world continues to grow alongside technical features.

**Tip:** When adding suggestions, ensure they are actionable, scoped, and aligned with the current architectural phase.

---

### New Suggestions

#### Technical Features & Improvements
- [ ] **Workshop Progression: Batch Crafting & Efficiency**: Implement batch processing (craft 5x, 10x) and a 'Mastery' system where repeated crafting reduces material costs or increases output for a specific recipe.
- [ ] **Character Stats: Influence on Quest Rewards**: Expand the character stat system so that 'Luck' increases rare item drop rates and 'Spirit' increases gold rewards from successful quests.
- [ ] **Visual Overhaul: Animation System**: Integrate CSS-based animations for quest progress (shaking, glowing) and success effects (particle bursts) within the Blazor UI components.
- [ ] **Data-Driven Cadence Unlocks**: Move the hardcoded Cadence unlock logic to a `cadence_unlocks.json` file, allowing for more flexible agentic updates to the progression tree.
- [ ] **State Persistence: Delta-Based Syncing**: Optimize the persistence layer to only sync changes (deltas) instead of the entire state to improve performance on larger save files.
- [ ] **Advanced Assertions: Condition-Based Validation**: Extend the `Mythril.Headless` testing framework to support complex assertions based on multiple state conditions (e.g., 'If Quest A is complete AND Inventory has Item B').

#### Additional Content (30% Mandate)
- [ ] **New Biome: The Sunken Grotto**: An underwater cavern system featuring aquatic materials (Coral, Pearl) and unique 'Diving' quests that require specific gear.
- [ ] **Legendary Questline: "The Echoes of Time"**: A narrative-heavy questline that takes players through different temporal versions of the same location, unlocking powerful artifacts.
- [ ] **New Cadence: "Nature's Guardian"**: Focuses on passive resource generation (Herbs, Wood) and unlocking 'Alchemical' refinements.
- [ ] **New Items: Elemental Infusions**: Consumables that provide temporary stat boosts or reduce the duration of specific quest types for a set number of completions.
- [ ] **New Cadence Ability: "Chrono-Manipulation"**: A high-tier ability that allows players to instantly complete a percentage of an active quest's remaining duration.

