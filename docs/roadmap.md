# Mythril Project Roadmap

## 🎯 Current Goal
Implement the third tier of multi-tasking and the second tier of automation, alongside a massive content expansion across new locations and specialized cadences, while overhauling the Junction UX for a more tactile experience.

## 🛤️ Active Tasks

### Phase 1: UI & Visual Foundation
- [ ] **Requirement Iconography**: Implement 🛡️ (Stats), 📦 (Items), and 🔑 (Prerequisites) on all Quest and Ability cards.
- [ ] **Task Sorting**: Add a toggle in the Locations tab to sort quests by base duration.
- [ ] **Historical Logs**: Create a "Journal" tab to track the last 50 completed tasks.
- [ ] **Inventory UI Scaling**: Implement horizontal scrolling and category filtering for large inventories.

### Phase 2: Logistics & Automation (Tier II)
- [ ] **Logistics II**: Expand character capacity to a **3rd task slot**.
- [ ] **AutoQuest II**: Enable automation for the **2nd task slot** (Logistics I slot).
- [ ] **Logic Enforcement**: Ensure Slot 2 (Logistics II) remains manual and single-use quests never loop.
- [ ] **Auto-Quest Visuals**: Implement a non-mechanical visual delay for auto-restarting tasks to improve game "feel."

### Phase 3: Junction System Overhaul (Tactile Interaction)
- [ ] **Drag-and-Drop Enhancement**: Finalize the transition to a pure drag-from-inventory model.
- [ ] **Predicted Effect Preview**: Implement color-coded stat changes (Green/Red) during drag operations.
- [ ] **Junction Removal**: Repurpose the "Junction" button on the character display as a dedicated removal tool.
- [ ] **Stat Ceiling**: Enforce the **255** maximum stat value across all systems.

### Phase 4: Advanced Progression & Gates
- [ ] **Hidden Cadences**: Implement stat-threshold detection for auto-unlocking Cadences (e.g., Geologist at 100 STR, Tide-Caller at 100 SPD).
- [ ] **Stat Scaling Polish**: Audit the multiplicative duration formula to ensure it aligns with the "not linear" mandate.

### Phase 5: World Content Expansion (Tier II)
- [ ] **New Regions**: Crystal Peaks, Tidal Caverns, Ancient Library.
- [ ] **New Cadences**: Slayer, Geologist, Tide-Caller.
- [ ] **Quest Lines**: 6 new recurring quest chains for advanced materials (Crystal Shards, Blue Coral, Lost Parchment).
- [ ] **Refinement Tier II**: Earth, Water, and Haste magic; Geode/Seed processing.

## ✅ Completed Tasks

### Core Engine & Systems
- [x] **Logistics I**: Character capacity expanded to 2 task slots.
- [x] **AutoQuest I**: Automated task restarting for the primary slot.
- [x] **Location Gating**: World map biomes gated behind prerequisite story quests.
- [x] **Completion Tracking**: Progress counters and checkmarks for Locations and Cadences.
- [x] **Monolith Prevention**: Refactored `ResourceManager` into specialized partial classes.
- [x] **Persistence Layer**: Cross-session saving for task slots, inventory, and world state.
- [x] **Junctioning MVP**: Initial implementation of magic-to-stat bonuses and drag-and-drop basics.

### Initial Content
- [x] **Whispering Woods**: Initial quests and materials (Gather Moonberries, Gather Bark).
- [x] **Mythril Weaver**: Core cadence for initial magic refinement.
- [x] **Workshop Reactivity**: Immediate UI updates when new abilities are learned.
