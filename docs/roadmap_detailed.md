# Detailed Technical Roadmap: Programmatic Assets & UI Overhaul

## ⚙️ Mechanical Specifications (Section-by-Section Mapping)

### Phase 1: Global Inventory Grid (Section 1)
- **Structure**: 40x40px icon grid with tooltips.
- **Categorization**: Multi-tab interface (Materials, Magic, Key Items).
- **UX Features**: 
    - Real-time search by name.
    - Multi-select for mass refinement (future-proofing).
    - Top-bar pinning for 5 tracked resources.

### Phase 2: Location Environment Logic (Section 2)
- **Visual Mapping**: 
    - Forests: Rounded corners (`border-radius: 12px 12px 0 0`).
    - Mines/Mountains: Sharp edges (`border-radius: 0`).
    - Water/Caverns: Pill-shaped (`border-radius: 20px`).
- **Compact Logic**: If `location.AllQuests.All(q => q.IsComplete)` then `Expander.IsDefaultCollapsed = true`.
- **Status Indicators**: Pulse animation on Location headers if `location.Quests.Any(q => q.IsActive)`.

### Phase 3: Cadence Management UI (Section 3)
- **Assignments**: Character avatar icons displayed on the Cadence Expander if assigned.
- **Search**: "Find Ability" input that highlights Cadence cards.
- **Progress**: `(UnlockedAbilities / TotalAbilities) * 100` progress bar on headers.

### Phase 4: Workshop Management (Section 4)
- **Filtering**: Hide recipes where requirements are not met.
- **Favorites**: `IsStarred` Boolean on recipe data to pin to the top of the group.
- **Consolidation**: Group multiple recipes from one ability into sub-menus or scrollable grids.

### Phase 5: Party Interaction (Section 5)
- **Junction Quick Swap**: Inter-card drag event between stat badges.
- **Character Mini-History**: Last 3 activities (e.g., "Mined Ore", "Unlocked Logistics") rendered in small text on character cards.

### Phase 6: Asset Generator Script (Section 6)
- **Core Script**: `scripts/generate_sprites.py` (Pillow).
- **Procedural Pixel Art**: 7x7 core patterns mirrored to 32x32 using fixed **DB8 palette**.
- **Emoji Baking**: Downscale (No-interpolation) processing of OS emojis into PNGs.
- **Layered SVG Logic**: Programmatic tinting of SVG base files (Potions, Crystals, Scrolls).

### Phase 7: Secondary Visual Asset Techniques (Section 7)
- **Game-icons.net**: Integration of standardized SVGs via `mask-image` for high-quality silhouettes.
- **CSS-to-Canvas**: Hidden utility to "bake" complex CSS objects into LocalStorage cached images.

### Phase 8: General UX & Polishing
- **Shortcuts**: `numeric 1-4` for navigation tabs.
- **Help System**: `?` hotkey triggers a detailed mechanic overlay.
- **Accessibility**: Theme-consistent glows for magic items and high-density grid layouts.
