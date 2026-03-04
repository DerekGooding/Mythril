# AI Guidance Knowledge Base

This file contains distilled architectural and project guidance provided by human developers. All agents must consult this base to ensure alignment with long-term project goals.

---
## [2026-03-03] Task Scaling, Junction UX & Content Expansion

### **Task Scaling Mechanics**
- **Stat Dependencies**: Every task (Quests, Refinement, Research) must have unique stat dependencies that influence completion time.
- **Formula**: Stat effects must be **multiplicative**, not linear, to prevent tasks from reaching zero or negative durations.
- **Minimum Duration**: All tasks must require a minimum of **0.5 seconds** to complete, regardless of character stats.
- **Examples**: 
    - Physical farming (e.g., Bats) should be improved by **Speed**.
    - Heavy labor (e.g., Treants) should be improved by **Strength**.

### **Junction System & UX**
- **Interaction Model**: Junctioning is performed by **dragging magic items** from the inventory directly onto character stats (requires the corresponding Cadence ability to be unlocked).
- **Junction Removal**: The "Junction" button on the character display is dedicated to **removing** junctions.
- **Visual Feedback**: During a drag operation, the UI must provide a "Predicted Effect" preview.
    - Show stat changes in **green** for increases.
    - Show stat changes in **red** for decreases (relative to the current junction).
    - Junctions never have a negative absolute effect, only a comparative decrease.
- **Efficiency**: No confirmation dialogs are required for junctioning/unjunctioning.

### **Auto-Quest Visuals**
- **Delay Implementation**: The Auto-Quest delay is **purely visual**. It adds no mechanical time cost to the task loop.

### **Content Strategy**
- **Expansion Model**: Both horizontal (new areas/jobs) and vertical (deeper quests/items) expansion are acceptable.
- **Progression Integrity**: New content must not invalidate existing progression (e.g., early-game jobs should not get abilities that make late-game jobs redundant).
- **Junction Depth**: Priority is to expand the Junction system.
    - Add more magic spells.
    - Create quests that require expending magic resources.
    - Ensure all game tasks are mechanically influenced by junctioned stats.

---
## [2026-03-03] Character Growth, Inventory Scaling & Longevity

### **Stat Progression**
- **Permanent Upgrades:** There are no permanent stat upgrades or "Character Levels". Stats are strictly derived from assigned Cadences and Junctioned magic.
- **Character Impact:** Tasks are natively affected by character stats (e.g., higher Strength speeds up Strength-dependent tasks).
- **Future Potential:** One-time permanent buffs from specific late-game quests may be considered eventually, but are not part of the current architecture.

### **Inventory Management**
- **Capacity Limits:** Non-magic items (Materials, Gold) have no capacity limit.
- **UI Scaling:** Large inventories should be managed via minimal UI solutions: filtering and horizontal scrolling.

### **Longevity & Prestige**
- **Prestige Mechanics:** There are no prestige or "hard reset" mechanics.
- **Expansion Model:** The game advances strictly through horizontal and vertical content expansion: new Cadences, new resources, and new magic.

---
## [2026-03-01] Junctioning Specifics & Cadence Sharing Rules

### **Resource Limits**
- **Magic:** Shared as a global inventory resource.
- **Capacity:** Initial limit is **30** of any single spell. This limit can be expanded via future Cadence ability unlocks.

### **Junction Scaling**
- **Formula:** Linear scaling (e.g., +1 Stat per fixed amount of Magic), modeled after the FF8 system.

### **Equipping & Persistence**
- **Exclusivity:** Each Cadence is a unique entity. A single Cadence can only be equipped by **one character** at a time (FF8 GF model).
- **Dependency:** Junctions are tied to the equipped Cadence. If a Cadence is unequipped, the character **loses all associated Junctions**. Junction knowledge does not persist across Cadence swaps unless the new Cadence also provides the required junction ability (e.g., "J-Str").


### **Vision**
The game is inspired by the **FF8 Junction System** and the **FF Tactics Job System**.
- **Cadences** function as both **Jobs** and **Guardian Forces (GFs)**.

### **The Core Game Loop**
1. **Unlock & Equip:** Unlock a Cadence and equip it to a character.
2. **Ability Progression:** Spend items to unlock new abilities within that Cadence.
3. **Refinement**: Use Cadence abilities to refine rewarded items into **Magic**.
4. **Junctioning**: Combine Magic with Cadence **Junction Abilities** to increase character stats.
5. **Scaling**: Higher stats allow characters to perform related quests faster or unlock higher-tier item quests.

---
## [2026-03-04] Automation, Balancing, and Aesthetics

### **Automation & Pacing**
- **Gameplay Ratio**: Aim for an optimal gameplay ratio of **2/3 idle** (automated recurring tasks) and **1/3 active** (one-time objectives and long-running quests).
- **Multi-Tasking**: As players unlock more task slots, they should be encouraged to keep a balance between automated resource generation and manual progression.
- **The "Wall"**: The first significant "grind" where junction optimization becomes necessary should occur around **10 minutes** into a fresh game.

### **Character & Stat Limits**
- **Character Identity**: Characters remain **Blank Slates**. No unique starting passives or narrative roles that influence builds.
- **Stat Ceiling**: Maximum stat values are capped at **255** (Final Fantasy standard).

### **Equipment & Economy**
- **No Equipment**: There will be no dedicated equipment slots (Weapons/Armor). **Junctioning remains the primary and only system** for stat modification.
- **Resource Scarcity**: Rare progression-gating resources (e.g., Mythril Spark) should initially be one-time rewards. They may become recurring sources much later in the progression tree.

### **Visual Aesthetic**
- **Minimalism**: Maintain the clean, minimalist dashboard look.
- **Visual Cues**: Expand the use of icons for quest requirements. In addition to the "shield" for stats, introduce a "bullet point" or similar icon for item requirements.
