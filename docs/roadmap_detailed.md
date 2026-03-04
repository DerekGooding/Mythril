# Resolved Implementation Details: Phase 2 Content & Systems

## ⚙️ Mechanical Specifications

### 1. Tiered Multi-Tasking
- **Logistics II**: A high-tier ability. When active on a character, the `CharacterDisplay` UI will render 3 task slots.
- **AutoQuest II**: Extends the auto-restart logic.
    - If a character has AutoQuest II, recurring tasks in **Slot 0 AND Slot 1** will restart.
    - Slot 2 remains manual-only.

### 2. Requirement Iconography
- **Icons**:
    - `StatRequirement`: 🛡️
    - `ItemRequirement`: 📦
    - `Prerequisite`: 🔑
- **Implementation**: Cards will display these icons inline with text. Tooltips will explain the requirement on hover.

### 3. Hidden Cadences (Stat Gates)
- `ResourceManager` will check `MaxStats` during each `Tick`.
- **Geologist**: Unlocks when any character reaches **100 Strength**.
- **Tide-Caller**: Unlocks when any character reaches **100 Speed**.
- **Scholar**: (Reserved for future) Unlocks at **100 Magic**.

### 4. Content Mapping
- **Crystal Peaks**:
    - Quests: *Shatter the Crystals* (Rewards: Crystal Shards).
    - Refinement: *Refine Earth* (Shards -> Earth I).
- **Tidal Caverns**:
    - Quests: *Deep Sea Scavenge* (Rewards: Blue Coral).
    - Refinement: *Refine Water* (Coral -> Water I).
- **Ancient Library**:
    - Quests: *Archive Sifting* (Rewards: Lost Parchment).
    - Refinement: *Refine Parchment* (Parchment -> Haste I).

## 🧪 Simulation Constraints
- The **Reachability Simulator** must be updated to include these new nodes.
- Total Stat Ceiling: **255**.
- "No combining" rule: All refinement recipes must follow the `1 Input -> 1 Output` pattern.
