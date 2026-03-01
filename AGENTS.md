# Specialized Agents for Mythril

This project uses agentic workflows to maintain its health and quality.

## Available Sub-Agents
- **codebase_investigator**: Architectural mapping and root-cause analysis.
- **cli_help**: Support for Gemini CLI features.

## Health Mandates
- **Monolith Prevention**: Files must be under 250 lines. (Enforced by `check_health.py`)
- **Test Coverage**: Minimum 70%.
- **Documentation**: Must be updated every 8 file changes.

## Persistence & State
Agents should be aware of the `JunctionManager` and `ResourceManager` separation when modifying core logic or UI.
Data-driven content is located in `Mythril.Blazor/wwwroot/data/*.json`.
