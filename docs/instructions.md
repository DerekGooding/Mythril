# How to Play Mythril

Mythril is a job-based incremental RPG where you manage a party of characters, unlock powerful abilities (Cadences), and tactically manage stats through Junctioning.

## 🕹️ Core Gameplay Loop

1.  **Complete Quests**: Assign characters to quests in various locations to earn resources like Gold, Materials, and Magic.
2.  **Unlock Cadences**: Use resources to research and unlock new jobs (Cadences) for your characters.
3.  **Junction Magic**: Assign magic items to character stats to gain massive bonuses.
4.  **Refine Items**: Use the Workshop to transform base materials into advanced resources and powerful spells.

## 🖱️ Drag and Drop Functionality

Mythril uses an intuitive "Drag-to-Character" interaction model for most assignments:

### Assigning Quests
- Navigate to the **Locations** tab.
- Click and drag a **Quest Card** onto a character in the **Party** panel on the right.
- The quest will begin immediately, and a progress bar will appear at the bottom of the character's card.

### Assigning Cadences
- Navigate to the **Cadence** tab.
- Click and drag a **Cadence Node** (the job name header) onto a character in the **Party** panel.
- The character will now be equipped with that Cadence, giving them access to its specific abilities and stat growth bonuses.

### Researching Abilities
- Open a Cadence's tree by clicking the expander.
- Click and drag an **Ability Node** onto a character to begin researching/unlocking that ability.
- Once unlocked, the ability provides its permanent bonus or unlocks new workshop recipes.

## ⚔️ Key Systems

### Character Stats
Characters have four primary stats that determine their effectiveness. Every task has a **Primary Stat** that reduces its duration according to the formula: `EffectiveDuration = BaseDuration / (1.0 + (Stat / 100.0))`.
- **Strength (STR)**: Often reduces duration for physical farming and manual labor.
- **Vitality (VIT)**: Often reduces duration for exploration and survival tasks.
- **Magic (MAG)**: Often reduces duration for arcane research and magical purification.
- **Speed (SPD)**: Often reduces duration for fast-paced gathering and commerce.

### Junctioning
Inspired by classic RPGs, Junctioning allows you to "equip" magic to your stats:
1.  Equip a Cadence that has a **Junction Ability** unlocked (e.g., "J-Str", "J-Magic").
2.  **Drag and Drop Preview**: Drag a magic item from your inventory over a character's stat badge to see a real-time preview of the stat change (e.g. +5 or -2). Drop it to junction!
3.  **Manual Selection**: Alternatively, click the **Junction** button on the character card to select a spell.
4.  The stat bonus scales with the **Quantity** of the spell you have in your inventory.

### The Workshop
The Workshop is where you refine raw materials into advanced items:
- New recipes are discovered as you unlock corresponding abilities across your Cadences.
- **Time-Based Refinement**: Drag a refinement task onto a character to begin the process. Efficiency is determined by the character's stats.
- **Assignment**: Refinement tasks can only be assigned to characters with the relevant Cadence equipped.

### Progression Gates
Advanced quests in regions like the **Iron Mines** or **Sun-Drenched Desert** require minimum junctioned stat values. If a character doesn't meet the requirement, the card will shake and return to your hand. Check the quest card for the 🛡️ icon!

### Location Completion
You can track your progress in each region by checking the counter on the location expanders. It shows how many **one-time quests** (Single or Unlock types) you have completed out of the total available in that area. Once you've cleared everything unique in a region, a green checkmark will appear!

### Cadence Completion
Similarly, you can track your research progress for each job in the **Cadence** tab. The counter shows how many abilities you have unlocked out of the total available for that Cadence. A green checkmark indicates you have mastered that job!

## 💡 Tips for Success
- **Specialization**: Differentiate your characters! Give one high Magic for fast research and another high Strength for resource farming.

### Logistics and Multi-Tasking
As you progress, you may unlock the **Logistics I** ability on certain advanced Cadences.
- **Dual Slots**: Equipping a Cadence with Logistics I gives that character a second task slot, allowing them to perform two actions at once.
- **Management**: You can drag and drop different tasks into each slot independently.
- **Cancellation**: If you unequip your Logistics Cadence while two tasks are active, the second task will be automatically cancelled and its costs refunded.

### Auto-Quest
Once you unlock the "AutoQuest I" ability on a Cadence, you can toggle "Auto" on for that character to automatically restart recurring quests.
- **Slot Restriction**: Note that Auto-Quest only affects the **primary (first) task slot**. Quests in additional slots must be restarted manually.

### Magic Capacity
- **Capacity**: Watch your magic limits. Research "Magic Pocket" abilities to increase the amount of magic you can carry.
