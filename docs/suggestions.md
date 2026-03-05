# UI Scaling & UX Suggestions

As the game progresses, several lists and panels will become unwieldy due to the increasing volume of items, locations, and abilities. Below are suggestions for human review to improve the long-term playability and aesthetic of the Mythril dashboard.

## 1. Global Inventory
**Current State:** A single horizontal row that grows indefinitely.
**Problem:** Hard to find specific materials or spells once the variety exceeds 10+ items.
**Suggestions:**
- **Categorization Tabs/Icons:** Split the inventory into "Materials", "Magic (Spells)", and "Key Items".
- **Grid Layout with Tooltips:** Instead of text-heavy items, use 40x40px icons in a wrapping grid. Show names and descriptions only on hover or selection.
- **Search & Sort:** Add a small search bar or sort by "Recent", "Quantity", or "Alphabetical".
- **"Pinning" system:** Allow users to pin 3-5 resources to the top/start of the list for quick monitoring.

## 2. Locations (HandPanel)
**Current State:** Vertical stack of Expanders.
**Problem:** Excessive vertical scrolling as more areas unlock.
**Suggestions:**
- **"Compact Mode" for Completed Areas:** If all one-time quests in a location are done, default that Expander to a collapsed state or move it to a "Legacy" group.
- **Visual Distinction by Region:** Use color-coded headers or small icons (e.g., a mountain for Peaks, a drop for Caverns) to make scanning faster.
- **Active Task Indicator:** Show a small pulsing dot on the Location header if a character is currently performing a task there.

## 3. Cadences (Jobs/GFs)
**Current State:** Vertical list of all unlocked Cadences.
**Problem:** High mental load to find which character has which cadence or where a specific ability is located.
**Suggestions:**
- **Equipped Status Overview:** Add a small portrait of the character currently holding the Cadence directly to the Expander header.
- **Ability Search/Highlight:** A global search that highlights which Cadence contains a specific ability (e.g., searching "J-Str" highlights Recruit and Warrior).
- **Tree Preview:** Show a tiny progress bar on the Cadence header representing % of abilities unlocked.

## 4. Workshop (Refinements)
**Current State:** Flat list of all discovered recipes grouped by ability.
**Problem:** Becomes extremely cluttered as more refined spells and materials are unlocked.
**Suggestions:**
- **Filtering by Output:** Add filter buttons for "Magic", "Materials", or "Consumables".
- **"Affordability" Toggle:** A switch to "Show only recipes I have materials for".
- **Favorites:** Allow users to "Star" recipes they use frequently (e.g., Iron Ore -> Fire I) to keep them at the top.
- **Consolidated Groups:** If an ability has 10 recipes, use a sub-expander or a grid within that ability group.

## 5. Party Panel (Right Column)
**Current State:** Vertical cards for each character.
**Problem:** Efficient, but could use more density for "pro" players.
**Suggestions:**
- **Junction "Quick Swap":** Allow dragging a junction from one character's stat directly to another's to swap the magic and the underlying cadence assignment in one move.
- **Mini-Log:** A tiny scrollable "Recent Results" list per character (e.g., "Hero: +5 Iron Ore", "Wifu: Refined Fire I").

## 7. Asset Strategy: "The High-Quality Placeholder"
To avoid the "developer art" look without a dedicated artist, we can use the following methods:
- **Game-icons.net (SVG)**: A massive, free library of white-on-black SVG icons that fit the "Fantasy/Dashboard" aesthetic perfectly. They are easily stylable via CSS `filter: drop-shadow()` or `mask-image`.
- **Generative AI (DALL-E 3 / Midjourney)**: Generate a consistent set of "UI Sprite Sheets" for materials. 
    - *Prompt Tip*: "Pixel art icon of a [Item Name], fantasy RPG style, white background, consistent 32x32 framing."
- **CSS-Only "Primitives"**: Use CSS `clip-path` and gradients to create items. For example, a "Fire I" spell could be a simple CSS diamond with a red/orange radial gradient and a `pulse` animation.

## 8. Asset-Light Alternatives (No Art Required)
If we choose not to add external images, we can achieve similar results using the tools already in the project:

### A. Material Icon Overlays
The project already uses Material Icons. We can combine them with CSS backgrounds to create unique "composite" icons:
- **Materials**: Use a circle background with a specific color + a central icon.
    - *Iron Ore*: Grey Circle + `settings` icon.
    - *Mana Leaf*: Green Circle + `eco` icon.
    - *Gold*: Gold Circle + `payments` icon.

### B. Typography & Color Coding
- **Inventory Groups**: Instead of icons, use consistent color borders for item types.
    - **Spells**: Neon Blue border with a subtle "inner glow" box-shadow.
    - **Materials**: Solid Grey/Brown borders.
    - **Key Items**: Pulsing Gold border.

### C. Unicode "Glyphs"
Many RPG stats and types can be represented via standard Unicode characters which inherit the project's font styling:
- **Strength**: `✦` or `⚔`
- **Magic**: `❃` or `☄`
- **Vitality**: `❤` or `🛡`
- **Speed**: `⚡` or `➳`

### D. CSS Grid "Shape" Recognition
- **Location Differentiation**: Instead of region icons, use distinct **header shapes** via `border-radius`. 
    - *Forests*: Top-left and top-right rounded.
    - *Mines/Mountains*: No rounding (sharp edges).
    - *Water/Caverns*: Fully pill-shaped headers.
- **Recipe Rarity**: Use CSS `repeating-linear-gradient` as a background for "Rare" recipes to make them instantly recognizable in a long list without needing a new icon.
