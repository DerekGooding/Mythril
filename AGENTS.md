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

## Recent Improvements (March 1, 2026)
- **Quest Progression**: Fixed 10x speed bug and implemented stat-influenced durations.
- **Single-Quest Protection**: Prevented repeatable quests from re-unlocking completed single-time quests.
- **Layout Refinement**: Fixed parent window scrolling issues and character card overflow.
- **Theme Resilience**: Optimized JS Interop for theme switching.
- **Cadence Unlocks**: Fixed character-to-cadence drag logic and restored cadence unlock persistence.
