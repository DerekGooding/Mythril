# Specialized Agents for Mythril

This project uses agentic workflows to maintain its health and quality.

## Available Sub-Agents
- **codebase_investigator**: Architectural mapping and root-cause analysis.
- **cli_help**: Support for Gemini CLI features.

## Health Mandates
- **Monolith Prevention**: Files must be under 250 lines. (Enforced by `check_health.py`)
- **Test Coverage**: Minimum 70%.
- **Documentation**: Must be updated every 8 file changes.
- **Feedback Integrity**: Every cleared item in `docs/feedback/` or `docs/errors/` must have a corresponding technical resolution file in `docs/resolution/` explaining the fix.

## Architecture & State
- **ResourceManager**: Core game engine managing quests, locations, and global state. Uses thread-safe locking for active tasks.
- **JunctionManager**: Handles character-cadence assignments and stat calculations (Junction system).
- **InventoryManager**: Manages items, spells, and capacity enforcement.
- **Data-Driven**: Content is located in `Mythril.Blazor/wwwroot/data/*.json`.

## Recent Improvements (March 1, 2026)
- **Stability**: Implemented thread-safe quest management in `ResourceManager`.
- **Expander Logic**: Robust JS-driven height transitions with `display: none` support for full container collapse.
- **Quest Progression**: Fixed 10x speed bug and implemented stat-influenced durations.
- **Layout Refinement**: Restored 2-column sidebar layout. Optimized character cards with horizontal, scrolling stats.
- **Theme Resilience**: Extracted JS logic to `theme.js`. Implemented `Mythril.ThemeTest` for isolated Interop validation.
