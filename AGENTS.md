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
- **ResourceManager**: Core game engine managing quests, locations, and global state.
- **JunctionManager**: Handles character-cadence assignments and stat calculations (Junction system).
- **InventoryManager**: Manages items, spells, and capacity enforcement.
- **Data-Driven**: Content is located in `Mythril.Blazor/wwwroot/data/*.json`.

## Diagnostics & Resilience
- **Theme Diagnostics**: Proactive C# eval-based status checks for `setTheme` in global scope.
- **JS Resilience**: Script-level `try-catch` blocks in `index.html` with console logging for interop setup.
