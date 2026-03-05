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

---

# Visual Asset Strategy (Selected Approach)

To ensure high-quality visual richness without a budget for artists or AI tokens, we will adopt a **Zero-Cost Programmatic Sprite Generation** strategy. All assets will be generated locally and for free using procedural logic.

## 6. Primary Decision: Procedural Pixel Art
We will create a `scripts/generate_sprites.py` utility that uses the **Pillow (PIL)** library to generate a consistent library of 32x32 pixel-art icons.

### A. Procedural Pixel Art Script
1. **Symmetry Generation**: Generate a 7x7 "Core" pattern and mirror it to create 16x16 or 32x32 icons (ideal for gems, crystals, and artifacts).
2. **Palette Mapping**: Use a fixed 8-color fantasy palette (e.g., [DawnBringer 8](https://lospec.com/palette-list/dawnbringer-8)) to ensure all generated sprites look cohesive.
3. **Noise & Dithering**: Programmatically add "shading" by darkening pixels furthest from a defined "light source" (top-left).

### B. Emoji "Baking"
- Use standard Emojis (vector assets provided by the OS) as a base.
- Render the Emoji to a 32x32 Canvas.
- Apply a "Pixelate" filter (downscale then upscale without interpolation).
- **Result**: High-quality, recognizable silhouettes with a consistent pixel-art aesthetic.

### C. The "Layered SVG" Approach
We can write a script using `svglib` or `Cairo` to programmatically layer shapes.
- **Base Shapes**: Define "Item Bases" (Potion Bottle, Ore Chunk, Gemstone, Scroll).
- **Dynamic Tinting**: Swap hex colors based on the item type (e.g., Red for Fire, Blue for Ice).
- **Glow Effects**: Programmatically add SVG `<filter>` tags for magic items.

## 7. Secondary Techniques (CSS-Only)
- **CSS-Only "Primitives"**: Use CSS `clip-path` and gradients to create items. A "Fire I" spell could be a CSS diamond with a red/orange radial gradient and a `pulse` animation.
- **Game-icons.net (SVG)**: Utilize the free library of white-on-black SVG icons, styled via CSS `filter: drop-shadow()` or `mask-image`.
- **CSS-to-Canvas Export**: Use a hidden `SpriteGenerator.razor` component to style a `<div>` with multiple box-shadows and then "bake" it into a PNG sprite at runtime.
