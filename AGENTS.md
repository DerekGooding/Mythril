# Specialized Agents for Mythril

This project uses agentic workflows to maintain its health and quality.

## Available Sub-Agents
- **codebase_investigator**: Architectural mapping and root-cause analysis.
- **cli_help**: Support for Gemini CLI features.

## Health Mandates
- **Monolith Prevention**: Files must be under 250 lines. `output/` published artifacts are excluded.
- **Test Coverage**: Minimum 70%. Currently maintained >89%.
- **Documentation**: Staleness is tracked locally. Must be updated every 8 file changes.
- **Feedback Integrity**: Every cleared item in `docs/feedback/` or `docs/errors/` must have a corresponding technical resolution file in `docs/resolution/`.

## Architecture & State
- **ResourceManager**: Core game engine managing quests, locations, and global state.
- **JunctionManager**: Handles character-cadence assignments and stat calculations.
- **InventoryManager**: Manages items, spells, and capacity enforcement.
- **Persistence**: Managed via `PersistenceService` using JSON serialization to `LocalStorage`.

## Testing Infrastructure
- **Headless Testing**: `Mythril.Headless` verifies game logic and data integrity without a UI.
- **Theme Testing**: `Mythril.ThemeTest` isolates and validates theme switching interop.
- **Live Diagnostics**: `TestRunner.razor` provides runtime state snapshots and JS interop validation.

## Recent Improvements (March 1, 2026)
- **Stability**: Fixed CadencePanel and TestRunner compilation issues.
- **Interop**: Hardened theme switching with re-injection fallback logic.
- **Layout**: Restored 2-column layout and optimized character card horizontal stat listing.
