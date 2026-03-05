# Mythril Project Roadmap

## 🎯 Current Goal
Implement the comprehensive UI and Asset Overhaul as greenlit in the Suggestions document, transitioning the project from a text-heavy prototype to a high-density, visually rich, and tactile management dashboard.

## 🛤️ Active Tasks

### Phase 1: Global Inventory Overhaul (Section 1)
- [x] **Grid-Based Layout**: Replace the horizontal row with a 40x40 icon grid.
- [x] **Categorization Tabs**: Split inventory into "Materials", "Magic", and "Key Items".
- [x] **Search & Sort**: Add a search bar and sort options (Alphabetical, Quantity, Recent).
- [x] **Pinning System**: Allow pinning up to 5 items for quick monitoring.

### Phase 2: Location Management (Section 2)
- [x] **Compact Mode**: Automatically collapse locations where all one-time quests are complete.
- [x] **Visual Distinction**: Apply environment-specific CSS shapes/colors to location headers.
- [x] **Active Task Indicator**: Show a pulsing dot on location headers for active character tasks.

### Phase 3: Cadence Intelligence (Section 3)
- [x] **Equipped Status**: Render character portraits on Cadence expanders when assigned.
- [x] **Ability Search**: Highlight Cadences containing specific searched abilities (e.g., "J-Str").
- [x] **Tree Progress**: Display a percentage bar for ability unlock completion on headers.

### Phase 4: Workshop Optimization (Section 4)
- [x] **Affordability Toggle**: Option to hide recipes with missing requirements.
- [x] **Output Filtering**: Buttons to filter by Spells, Materials, or Consumables.
- [x] **Favorites (Starring)**: Implement a "Star" system to pin frequent recipes.
- [x] **Consolidated Groups**: Use sub-expanders for abilities with numerous recipes.

### Phase 5: Party & Tactile Interaction (Section 5)
- [x] **Junction "Quick Swap"**: Enable dragging junctioned stats between character cards.
- [x] **Character Mini-Log**: Add a small activity history (last 3 items) per character card.

### Phase 6: Primary Asset Generation (Section 6)
- [x] **Procedural Sprite Generator**: Python script (Pillow) for symmetrical 32x32 pixel-art icons.
- [x] **Emoji "Baking"**: Utility to process OS emojis into pixel-art sprites.
- [x] **Layered SVG Tinting**: System for color-swapping base SVG shapes (potions, scrolls).

### Phase 7: Secondary Asset Techniques (Section 7)
- [x] **CSS Primitives**: Implement clip-path and gradient-based magic items (e.g., fire diamonds).
- [x] **SVG Sourcing**: Integrate Game-icons.net SVG library (via CSS masks).
- [x] **CSS-to-Canvas Baking**: Runtime utility to convert complex CSS objects into PNG sprites. (Replaced with unified ItemIcon component)

### Phase 8: General UX & Accessibility
- [x] **Keyboard Shortcuts**: Map numeric keys `1-4` to main navigation tabs.
- [x] **Contextual Help Overlay**: Implement a global `?` help system for mechanics.
- [x] **Theme Persistence**: Polish high-contrast variants for all new UI components.
- [x] **Standardized Tooltips**: Implement advanced descriptions across all grid systems.
