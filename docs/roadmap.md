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
- [ ] **Compact Mode**: Automatically collapse locations where all one-time quests are complete.
- [ ] **Visual Distinction**: Apply environment-specific CSS shapes/colors to location headers.
- [ ] **Active Task Indicator**: Show a pulsing dot on location headers for active character tasks.

### Phase 3: Cadence Intelligence (Section 3)
- [ ] **Equipped Status**: Render character portraits on Cadence expanders when assigned.
- [ ] **Ability Search**: Highlight Cadences containing specific searched abilities (e.g., "J-Str").
- [ ] **Tree Progress**: Display a percentage bar for ability unlock completion on headers.

### Phase 4: Workshop Optimization (Section 4)
- [ ] **Affordability Toggle**: Option to hide recipes with missing requirements.
- [ ] **Output Filtering**: Buttons to filter by Spells, Materials, or Consumables.
- [ ] **Favorites (Starring)**: Implement a "Star" system to pin frequent recipes.
- [ ] **Consolidated Groups**: Use sub-expanders for abilities with numerous recipes.

### Phase 5: Party & Tactile Interaction (Section 5)
- [ ] **Junction "Quick Swap"**: Enable dragging junctioned stats between character cards.
- [ ] **Character Mini-Log**: Add a small activity history (last 3 items) per character card.

### Phase 6: Primary Asset Generation (Section 6)
- [ ] **Procedural Sprite Generator**: Python script (Pillow) for symmetrical 32x32 pixel-art icons.
- [ ] **Emoji "Baking"**: Utility to process OS emojis into pixel-art sprites.
- [ ] **Layered SVG Tinting**: System for color-swapping base SVG shapes (potions, scrolls).

### Phase 7: Secondary Asset Techniques (Section 7)
- [ ] **CSS Primitives**: Implement clip-path and gradient-based magic items (e.g., fire diamonds).
- [ ] **SVG Sourcing**: Integrate Game-icons.net SVG library.
- [ ] **CSS-to-Canvas Baking**: Runtime utility to convert complex CSS objects into PNG sprites.

### Phase 8: General UX & Accessibility
- [ ] **Keyboard Shortcuts**: Map numeric keys `1-4` to main navigation tabs.
- [ ] **Contextual Help Overlay**: Implement a global `?` help system for mechanics.
- [ ] **Theme Persistence**: Polish high-contrast variants for all new UI components.
- [ ] **Standardized Tooltips**: Implement advanced descriptions across all grid systems.
