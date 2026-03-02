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

## Testing Infrastructure
- **Headless Testing**: `Mythril.Headless` verifies game logic and data integrity without a UI.
- **Theme Testing**: `Mythril.ThemeTest` isolates and validates theme switching interop.
- **Live Diagnostics**: `TestRunner.razor` provides runtime state snapshots and JS eval capabilities for debugging.

## Recent Improvements (March 1, 2026)
- **Data Integrity**: Verified "Test" cadence presence via headless test.
- **Theme Resilience**: Inlined theme switching logic in `index.html` to eliminate race conditions.
- **UI Diagnostics**: Added `TestRunner` page and logging to `CadencePanel`.
