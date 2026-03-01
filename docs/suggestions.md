# AI Suggestions

**Last Generation:** March 1, 2026. Aligned with FF8 Junction / FFT Job system vision.

## Instructions
New AI-generated suggestions for the project should be placed in this file if it is currently empty or has been fully processed. 

A user will periodically review these suggestions and greenlight specific goals by moving them to the Roadmap or issuing new directives.

**Mandate:** At least 30% of suggestions must focus on **Additional Content** (e.g., new locations, items, quests, or cadences) to ensure the game world continues to grow alongside technical features.

**Tip:** When adding suggestions, ensure they are actionable, scoped, and aligned with the current architectural phase.

---

### New Suggestions

#### Technical Features & Improvements
- [ ] **Junction UI: Stat Mapping**: Implement a dedicated UI where players can select a Stat (e.g., Strength) and assign a Magic item (e.g., Fire I) to it, provided the equipped Cadence has the corresponding "J-Str" or "J-Magic" ability.
- [ ] **Global Magic Capacity Enforcement**: Update `InventoryManager` or a new `MagicManager` to enforce the global limit of 30 per spell, and implement the logic for "Capacity Expansion" abilities within the Cadence tree.
- [ ] **Cadence Assignment UI: Exclusivity Logic**: Create a drag-and-drop or selection UI for assigning Cadences to characters that enforces the "one character per Cadence" rule and automatically clears Junctions when a Cadence is unequipped.
- [ ] **Linear Stat Augment Calculator**: Implement the linear scaling formula for Junctioned magic, ensuring that stat bonuses are recalculated in real-time as magic quantities change.
- [ ] **Tutorial Questline: "The First Refinement"**: Create a scripted sequence of quests that rewards "Basic Gems," instructs the player to unlock the "Refine Fire" ability, and then use the Workshop to create their first spell.

#### Additional Content (30% Mandate)
- [ ] **Capacity Expansion Abilities**: Add new high-tier abilities to existing Cadences (e.g., "Magic Pocket I") that increase the global magic limit from 30 to 60.
- [ ] **Starter Biome: The Whispering Woods**: A low-level area focused on gathering "Mana Leaves" and "Magic Sap" for early-tier support magic refinements.
- [ ] **New Items: Elemental Shards**: Tier 1 materials (Fire Shard, Ice Shard) found in early quests to be used as base ingredients for elemental magic.
- [ ] **New Cadence: "The Arcanist"**: A starter magic-focused job providing abilities for "Refine Ice" and "J-Magic."
- [ ] **Quest: "Recover the Ancient Tome"**: A single-completion quest in the Starting Town that rewards the player with their first Cadence-unlocking item.
