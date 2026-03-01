# Mythril

[![Deploy Blazor to GitHub Pages](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml/badge.svg)](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml)
![Tests](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_tests.json)
![Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_coverage.json)
![Monoliths](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_monoliths.json)
![Docs](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_docs.json)

[**Live Website**](https://DerekGooding.Github.io/Mythril)

**Documentation Status:** Last updated March 1, 2026. Added In-Game Feedback system, global error reporting, and live update notifications.

## Project Structure
**This project is heavily and primarily developed by AI agents.**  
From architecture and core logic to the Blazor frontend and DevOps pipelines, Mythril is a testament to agentic software engineering. All changes, including this documentation, are managed and validated by agents to ensure technical integrity and adherence to the project's [AI Mandates](GEMINI.md).

## ⚔️ Project Overview
Mythril is an RPG-inspired web application built with **.NET 9** and **Blazor WebAssembly**. It serves as a sandbox for exploring agentic development patterns and modern C# architectures.

### Key Systems
- **Character Core**: Implements a `partial record struct` for characters, allowing for distributed definition of properties and behaviors.
- **Quest & Progression**: A real-time, asynchronous tick system managing quests, durations, and rewards.
- **Inventory & Items**: A robust `InventoryManager` supporting various item types (Currency, Consumable, Material, Spell) and quantity tracking.
- **Cadence System**: A unique progression mechanic where `Cadences` provide `CadenceAbilities` and `AbilityAugments`. Visualized via an interactive tree.
- **Persistence**: Game state (Inventory, Unlocked Cadences) is automatically saved to browser `LocalStorage`.
- **Headless Testing**: An automated scenario runner (`Mythril.Headless`) verifies complex game states and logic without a UI.

## 🛠️ Technical Stack
- **Frontend**: Blazor WebAssembly (.NET 9)
- **Core Logic**: C# 13 / .NET 9 Class Libraries (Mythril.Data)
- **Dependency Injection**: `SimpleInjection` (Source-generated)
- **Testing**: MSTest, Moq, and Coverlet for coverage reporting
- **Automation**: Python 3.x for custom health checks and CI integration
- **CI/CD**: GitHub Actions for automated deployment and health monitoring

## ⚖️ Quality Assurance & Health
We maintain project health through a custom automated suite (`scripts/check_health.py`) which runs on every commit:
- **Monolith Prevention**: Strict 250-line limit for source files to ensure modularity.
- **Coverage**: Mandatory 70% unit test coverage for all core logic.
- **Documentation Integrity**: Automated staleness tracking.
- **Mandate Adherence**: All development must comply with the foundational directives in [GEMINI.md](GEMINI.md).

## 🚀 Getting Started
1. **Prerequisites**: .NET 9 SDK, Python 3.x, and a git-compatible shell (PowerShell recommended on Windows).
2. **Build**: `dotnet build`
3. **Test**: `dotnet test`
4. **Health Check**: `python scripts/check_health.py`
5. **Headless Test**: `.\run_ai_test.ps1`

---
*Developed with 💖 by Gemini CLI.*
