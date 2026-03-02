# Specialized Agents for Mythril

This project uses agentic workflows to maintain its health and quality.

## Available Sub-Agents
- **codebase_investigator**: Architectural mapping and root-cause analysis.
- **cli_help**: Support for Gemini CLI features.

## Health Mandates
- **Monolith Prevention**: Files must be under 250 lines.
- **Test Coverage**: Minimum 70%. Currently maintained >89%.
- **Documentation**: Staleness is tracked locally. Must be updated every 8 file changes.
- **Feedback Integrity**: Every cleared item in `docs/feedback/` or `docs/errors/` must have a corresponding technical resolution file in `docs/resolution/`.

## Architecture & State
- **ResourceManager**: Core game engine managing quests, locations, and global state.
- **JunctionManager**: Handles character-cadence assignments and stat calculations.
- **InventoryManager**: Manages items, spells, and capacity enforcement.
- **Persistence**: Automated via `PersistenceService`.
- **Theming**: UI styles are standardized using CSS variables. Components should always use variables (e.g., `var(--card-bg)`).

## Testing Infrastructure
- **Headless Testing**: `Mythril.Headless` verifies game logic and data integrity.
- **Live Diagnostics**: `TestRunner.razor` provides runtime state snapshots and automated visual theme audits.

## Recent Improvements (March 1, 2026)
- **UI Stability**: Migrated expanders to CSS Grid and stabilized progress bars by removing problematic transitions.
- **Layout Integrity**: Standardized the 2-column layout and enforced horizontal stats.
- **Theming**: Completed a universal theme variable audit, including the Party section and progress bars.
- **Drag-and-Drop**: Fixed Cadence equipping/unequipping logic and UI.
