# Mythril

[![Deploy Blazor to GitHub Pages](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml/badge.svg)](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml)
![Tests](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_tests.json)
![Coverage](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_coverage.json)
![Monoliths](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_monoliths.json)
![Docs](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_docs.json)
![UI Integrity](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_ui.json)
![Reachability](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_simulation.json)
![Economy](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_sustainability.json)
![Optimal Completion](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_game_time.json)

[**Live Website**](https://DerekGooding.Github.io/Mythril) | [**How to Play**](docs/instructions.md)

A .NET 10 Blazor-based incremental RPG with a focus on job-based progression (Cadences) and tactical stat management (Junctioning).

## Project Structure
**This project is heavily and primarily developed by AI agents.**
From architecture and core logic to the Blazor frontend and DevOps pipelines, Mythril is a testament to agentic software engineering. All changes, including this documentation, are managed and validated by agents to ensure technical integrity and adherence to the project's [AI Mandates](GEMINI.md).

## ⚔️ Project Overview
Mythril is an RPG-inspired web application built with **.NET 10** and **Blazor WebAssembly**. It serves as a sandbox for exploring agentic development patterns and modern C# architectures.

### Key Systems
- **Character Core**: Modular system where characters share baseline stats (**Strength, Vitality, Magic, Speed**), differentiated by assigned Cadences and junctioned magic. Features immersive visual feedback for task completion and per-character equipment management.
- **Junctioning**: Assign magic items to character stats to gain powerful bonuses. Features a tactile drag-and-drop system from the inventory directly onto character stats, complete with predictive effect previews and a specialized removal tool.
- **Cadence System**: Progression mechanic where `Cadences` provide `CadenceAbilities`. Unlocking is performed by dragging ability nodes directly onto characters.
- **Journal System**: Persistent historical log tracking the last 50 completed tasks, providing transparency into character activities and resource gains.
- **Quest & Progression**: Real-time asynchronous tick system managing quests, durations, and rewards, with offline progress continuity and interconnected world unlocks.
- **UI Stability**: Advanced flexbox layouts and pure CSS Grid transitions ensure a responsive, flicker-free experience that fills the browser viewport, featuring an intuitive "Drag-to-Character" interaction model.

## 🛠️ Technical Stack
- **Frontend**: Blazor WebAssembly (.NET 10)
- **Core Logic**: C# 13 / .NET 10 Class Libraries
- **Architecture**: Separated into core managers: `ResourceManager` (Logic), `JunctionManager` (Stats), and `InventoryManager` (Items).
- **Testing**: MSTest, Moq, and bUnit for component testing. Coverage is maintained at >90% overall.
- **CI/CD**: GitHub Actions for automated deployment and health monitoring.

## 🚀 Recent Updates (March 4, 2026)
- **Tactile Junction Overhaul**: Transitioned to a pure drag-from-inventory model for Junctioning. Added color-coded stat delta previews (↑/↓) and a dedicated "Link Off" removal tool.
- **Stat Ceiling Enforcement**: Implemented a global **255** maximum cap for all character stats to ensure long-term game balance.
- **Tier II Multi-Tasking & Automation**: Expanded character capacity to a 3rd task slot via **Logistics II** and enabled automation for the 2nd slot with **AutoQuest II** (Scholar cadence).
- **Historical Journal**: Integrated a new Journal tab that tracks task completion history across sessions, including character names and specific rewards.
- **Task Sorting**: Added a duration-based sorting toggle to the Locations panel, allowing players to easily prioritize tasks based on their playstyle.
- **Requirement Iconography**: Implemented standardized icons (🛡️ for stats, 📦 for items, 🔑 for prerequisites) across all quest and ability cards for improved readability.
- **Monolith Prevention Refactor**: Decomposed the `ResourceManager` into specialized partial classes (`State`, `Discovery`, `Inventory`, `Quests`, `Journal`, `Rewards`, `Logistics`) to maintain a lean, maintainable architecture.

## ⚖️ Quality Assurance & Health
We maintain project health through a custom automated suite (`scripts/check_health.py`) which runs on every commit:
- **Monolith Prevention**: Strict 250-line limit for source files (excluding tools).
- **Game Graph Simulation**: 
    - **Lattice Reachability**: Mathematically verifies every quest, cadence, and resource is attainable from a fresh start using a monotonic fixpoint solver.
    - **Quantitative Flow Analysis**: Models the steady-state economy. It identifies "starving" activities where consumption exceeds production and detects infinite feedback loops.
- **Automated Balancing**: The simulation suite ensures no content change makes a quest mathematically impossible or economically unsustainable.
- **Coverage**: Mandatory 70% overall line coverage; 25% per-file minimum.
- **Razor Integrity**: All interactive components must have bUnit tests, `@key` usage in loops, and `data-testid` anchors.
- **Documentation Integrity**: Automated staleness tracking via local file modification times.

### 🛡️ Health Shield Guide
All badges are automatically updated by `scripts/check_health.py` on every commit:
- **Tests**: Status of the entire MSTest suite. Fails if any unit test is broken.
- **Coverage**: Total line coverage percentage. Must remain above 70% overall.
- **Monoliths**: A count of source files exceeding 250 lines. Aiming for 0 for maximum maintainability.
- **Docs**: Staleness indicator. Flags documentation if more than 10 source changes occur without a doc update.
- **UI Integrity**: Validates bUnit test presence, `@key` usage in loops, and stable `data-testid` anchors.
- **Reachability**: Result of the Lattice Simulation. Verifies all content is mathematically attainable.
- **Economic Stability**: Percentage of recurring activities that are sustainable under current production rates.
- **Optimal Completion**: The minimum real-world time required to finish the current endgame content.


## 🚀 Getting Started
1. **Build**: `dotnet build`
2. **Test**: `dotnet test`
3. **Health Check**: `python scripts/check_health.py`
4. **Run**: `dotnet run --project Mythril.Blazor`

---
*Developed with 💖 by Gemini CLI.*
