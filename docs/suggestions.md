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

## 9. Zero-Cost Programmatic Sprite Generation
Since there is no budget for AI tokens, we can use a **Python-based Procedural Sprite Generator**. This allows us to generate a consistent library of 32x32 pixel-art icons locally and for free.

### A. The "Layered SVG" Approach (Highest Quality)
We can write a Python script using the `svglib` or `Cairo` libraries to programmatically layer shapes.
- **Base Shapes**: Define a set of "Item Bases" (Potion Bottle, Ore Chunk, Gemstone, Scroll).
- **Dynamic Tinting**: Use Python to swap hex colors based on the item type (e.g., Red for Fire, Blue for Ice).
- **Glow Effects**: Programmatically add SVG `<filter>` tags for magic items.

### B. Procedural Pixel Art Script
We can create a `scripts/generate_sprites.py` utility that uses the **Pillow (PIL)** library:
1. **Symmetry Generation**: Generate a 7x7 "Core" pattern and mirror it to create 16x16 or 32x32 icons (great for gems, crystals, and artifacts).
2. **Palette Mapping**: Use a fixed 8-color fantasy palette (e.g., [DawnBringer 8](https://lospec.com/palette-list/dawnbringer-8)) to ensure all generated sprites look like they belong to the same game.
3. **Noise & Dithering**: Programmatically add "shading" by darkening pixels furthest from a defined "light source" (top-left).

### C. CSS-to-Canvas Export
An even lighter solution:
- Create a hidden `SpriteGenerator.razor` component.
- Use CSS to style a `<div>` into a complex shape (using multiple box-shadows for "pixels").
- Use `html2canvas` or a native Canvas `drawImage` call to "bake" these CSS objects into PNG sprites at runtime, which are then cached in LocalStorage.

### D. Emoji "Baking"
- Use standard Emojis (which are high-quality vector assets provided by the OS) as a base.
- Render the Emoji to a 32x32 Canvas.
- Apply a "Pixelate" filter (downscale then upscale without interpolation).
- **Result**: High-quality, recognizable silhouettes with a consistent pixel-art aesthetic, generated 100% locally and programmatically.
