# Mythril

[![Deploy Blazor to GitHub Pages](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml/badge.svg)](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml)
![Tests](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_tests.json)
![Coverage](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_coverage.json)
![Monoliths](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_monoliths.json)
![Docs](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_docs.json)
![UI Integrity](https://img.shields.io/endpoint?url=https://DerekGooding.github.io/Mythril/health/shield_ui.json)

[**GitHub Repository**](https://github.com/DerekGooding/Mythril) | [**Live Website**](https://DerekGooding.Github.io/Mythril)

A .NET 10 Blazor-based incremental RPG with a focus on job-based progression (Cadences) and tactical stat management (Junctioning).

## Project Structure
**This project is heavily and primarily developed by AI agents.**
From architecture and core logic to the Blazor frontend and DevOps pipelines, Mythril is a testament to agentic software engineering. All changes, including this documentation, are managed and validated by agents to ensure technical integrity and adherence to the project's [AI Mandates](GEMINI.md).

## ⚔️ Project Overview
Mythril is an RPG-inspired web application built with **.NET 10** and **Blazor WebAssembly**. It serves as a sandbox for exploring agentic development patterns and modern C# architectures.

### Key Systems
- **Character Core**: Modular system where characters share baseline stats (**Strength, Vitality, Magic, Speed**), differentiated by assigned Cadences and junctioned magic. Features horizontal stat layout, vertical party stacking, and immersive visual feedback for task completion.
- **Junctioning**: Assign magic items to character stats to gain powerful bonuses, inspired by classic RPG mechanics.
- **Cadence System**: Progression mechanic where `Cadences` provide `CadenceAbilities`. Unlocking is performed by assigning characters to specific ability nodes.
- **Quest & Progression**: Real-time asynchronous tick system managing quests, durations, and rewards, with offline progress continuity and interconnected world unlocks.
- **UI Stability**: Advanced flexbox layouts and pure CSS Grid transitions ensure a responsive, flicker-free experience that fills the browser viewport, with live update notifications.

## 🛠️ Technical Stack
- **Frontend**: Blazor WebAssembly (.NET 10)
- **Core Logic**: C# 13 / .NET 10 Class Libraries
- **Architecture**: Separated into core managers: `ResourceManager` (Logic), `JunctionManager` (Stats), and `InventoryManager` (Items).
- **Testing**: MSTest, Moq, and bUnit for component testing. Coverage is maintained at >90% overall.
- **CI/CD**: GitHub Actions for automated deployment and health monitoring.

## ⚖️ Quality Assurance & Health
We maintain project health through a custom automated suite (`scripts/check_health.py`) which runs on every commit:
- **Monolith Prevention**: Strict 250-line limit for source files (excluding tools).
- **Coverage**: Mandatory 70% overall line coverage; 25% per-file minimum.
- **Razor Integrity**: All interactive components must have bUnit tests, `@key` usage in loops, and `data-testid` anchors.
- **Documentation Integrity**: Automated staleness tracking via local file modification times.
- **Feedback Integrity**: Monitoring of pending user feedback and runtime errors in `docs/feedback/`.

## 🚀 Getting Started
1. **Build**: `dotnet build`
2. **Test**: `dotnet test`
3. **Health Check**: `python scripts/check_health.py`
4. **Run**: `dotnet run --project Mythril.Blazor`

---
*Developed with 💖 by Gemini CLI.*
