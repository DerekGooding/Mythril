# AI Guidance Knowledge Base

This file contains distilled architectural and project guidance provided by human developers. All agents must consult this base to ensure alignment with long-term project goals.

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
3. **Refinement:** Use Cadence abilities to refine rewarded items into **Magic**.
4. **Junctioning:** Combine Magic with Cadence **Junction Abilities** to increase character stats.
5. **Scaling:** Higher stats allow characters to perform related quests faster or unlock higher-tier item quests.

### **Short-Term Priority**
- Do not focus on endgame yet.
- **Goal:** Implement a solid **20-30 minute onboarding experience** that teaches these mechanics (Equipping, Refining, Junctioning).
- Ensure the core mechanics (Items -> Magic -> Stats -> Efficiency) are fully functional and intuitive before scaling content.
