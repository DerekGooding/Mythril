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

Mythril is a .NET 10 Blazor-based incremental RPG designed as a **sandbox for Agentic Software Engineering**. 

## 🤖 Developed by Agents
**This project is primarily built, managed, and maintained by AI agents.**
From architectural design and core logic implementation to the Blazor frontend and custom DevOps pipelines, Mythril demonstrates the capabilities of agent-driven development. 

### Agentic Principles
- **Mandate-Driven**: All development adheres to the foundational rules in [GEMINI.md](GEMINI.md).
- **Self-Validating**: Agents are responsible for the entire lifecycle: implementation, test generation, and health validation.
- **Architectural Integrity**: Heavy use of modular partial classes and specialized managers to prevent code rot and monoliths.
- **Agentic DevOps**: A custom health suite (`scripts/check_health.py`) enforces strict technical standards on every commit.

## 🛠️ Technical Stack & Architecture
- **Frontend**: Blazor WebAssembly (.NET 10) utilizing advanced Flexbox/Grid layouts for full-viewport stability.
- **Core Logic**: C# 13 / .NET 10 Libraries with a clean separation of concerns:
    - `ResourceManager`: The orchestrator of game state and asynchronous progression.
    - `JunctionManager`: Handles the complex stat-calculation logic and magic assignments.
    - `InventoryManager`: Manages resource collection, pinning, and capacity limits.
- **State Management**: Serialized state preservation via `PersistenceService` in `LocalStorage`.
- **Content Management**: A Python-based **Content Manager Module** (`modules/contentManager`) provides a GUI for rapidly editing game data (Quests, Items, Cadences, Refinements) with built-in validation and automated backups.
- **Testing**: Comprehensive suite using MSTest, Moq, and bUnit. Line coverage is maintained at **>75%**.

## ⚖️ Quality Assurance & Automated Balancing
To ensure the game remains both technically sound and balanced, we use a custom headless simulation suite:
- **Lattice Reachability Analysis**: A monotonic fixpoint solver that mathematically verifies every quest, cadence, and resource is attainable from a fresh start.
- **Path-Routed Simulation**: A character-aware simulator that models optimal gameplay paths to calculate realistic "real-world" completion times.
- **Quantitative Flow Analysis**: Models the steady-state economy to identify "starving" activities and prevent infinite resource feedback loops.
- **Monolith Prevention**: Automated enforcement of a 250-line limit for all source files.

## ⚔️ Game Overview (Brief)
Mythril features a job-based progression system where you manage a party of characters.
- **Cadences**: Specialized jobs that grant unique abilities through research.
- **Junctioning**: A tactile drag-and-drop system to assign refined magic to character stats for massive bonuses.
- **World Progression**: Asynchronous quests that unlock new locations, refined through a workshop system.
- **Journaling**: A persistent historical log of all character activities and achievements.

*For detailed gameplay mechanics, see the [How to Play](docs/instructions.md) guide.*

## 🚀 Recent Updates (April 23, 2026)
- **Content Manager Overhaul**: Implemented `deepcopy` persistence and unique Streamlit keys to fix data leaking between refinements and ensure all fields (including Effects) are preserved.
- **UI Polish & Accessibility**: Added animated "in-process" icons (active-dot) for quests and unlocks with standardized margins for better readability.
- **Visual Determinism**: Refactored `Expander` and `QuestCard` headers to flexbox-based layouts for consistent icon positioning.
- **Agentic Health Verification**: Integrated real-time content manager integrity checks directly into the DevOps pipeline.

## 🚀 Getting Started
1. **Build**: `dotnet build`
2. **Test**: `dotnet test`
3. **Health Check**: `python scripts/check_health.py`
4. **Run CMS**: `python modules/contentManager/app.py` (Requires Streamlit)
5. **Run Game**: `dotnet run --project Mythril.Blazor`

---
*Developed with 💖 by Gemini CLI.*

---  

## Words from the human developer

I get it. AI fatigue. I'd gloss over the majority of this as slop too, and honestly, it probably is. The reson I'm doing this project isn't for money or clout. It's to push the limits of what can be done entirely agentically. When I dip my hand into the project, it's not to build the game or even the tools around the game. It's to create the agentic DevOps that maintain a project as it scales. When something starts getting AI-sloppy, that's why I want to try and solve that mess. I see agentic development as that dirt line you can never actually sweep up. It just gets thinner and thinner. So as the project grows, so does the dirty line and additional metrics and checks and balances are put in place to reduce the slop as much as possible. Layering guard rail on guard rail. Safety net on safety, just to keep regressions low and results shiny. 

Will this be any good? Hard to say. That's the fun of it. Are there better tools out there already doing the things I'm doing? Of course. But that's not going to stop me from enjoying the journey. If anyone human is actually reading these words, thank you. That means a lot to me in this day and age. 

--- Derek Gooding
