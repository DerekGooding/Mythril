# Mythril

[![Deploy Blazor to GitHub Pages](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml/badge.svg)](https://github.com/DerekGooding/Mythril/actions/workflows/deploy.yml)
![Tests](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_tests.json)
![Coverage](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_coverage.json)
![Monoliths](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_monoliths.json)
![Docs](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/DerekGooding/Mythril/main/scripts/data/shield_docs.json)

[**GitHub Repository**](https://github.com/DerekGooding/Mythril) | [**Live Website**](https://DerekGooding.Github.io/Mythril)

A .NET 10 Blazor-based incremental RPG with a focus on job-based progression (Cadences) and tactical stat management (Junctioning).

## Project Structure
**This project is heavily and primarily developed by AI agents.**
From architecture and core logic to the Blazor frontend and DevOps pipelines, Mythril is a testament to agentic software engineering. All changes, including this documentation, are managed and validated by agents to ensure technical integrity and adherence to the project's [AI Mandates](GEMINI.md).

## ⚔️ Project Overview
Mythril is an RPG-inspired web application built with **.NET 10** and **Blazor WebAssembly**. It serves as a sandbox for exploring agentic development patterns and modern C# architectures.

### Key Systems
- **Character Core**: Modular system where characters share baseline stats, differentiated by assigned Cadences and junctioned magic.
- **Junctioning**: Assign magic items to character stats to gain powerful bonuses, inspired by classic RPG mechanics.
- **Cadence System**: Progression mechanic where `Cadences` provide `CadenceAbilities` and `AbilityAugments`. Visualized via an interactive tree.
- **Quest & Progression**: Real-time asynchronous tick system managing quests, durations, and rewards, with offline progress continuity.
- **Item Refinement**: Craft new items and magic through specialized abilities in the Workshop.
- **Quality Assurance**: Automated health checks for monolith prevention, test coverage (>90%), and documentation integrity.

## 🛠️ Technical Stack
- **Frontend**: Blazor WebAssembly (.NET 10)
- **Core Logic**: C# 13 / .NET 10 Class Libraries
- **Data**: JSON-driven content for easy modification and agentic updates.
- **Dependency Injection**: `SimpleInjection` (Source-generated)
- **Testing**: MSTest, Moq, and Coverlet for coverage reporting
- **Automation**: Python 3.x for custom health checks and CI integration
- **CI/CD**: GitHub Actions for automated deployment and health monitoring

## ⚖️ Quality Assurance & Health
We maintain project health through a custom automated suite (`scripts/check_health.py`) which runs on every commit:
- **Monolith Prevention**: Strict 250-line limit for source files to ensure modularity.
- **Coverage**: Mandatory 70% unit test coverage for all core logic (currently >90%).
- **Documentation Integrity**: Automated staleness tracking via local file modification times.
- **Feedback Integrity**: Every resolved item must have a corresponding resolution file in `docs/resolution/`.

## 🚀 Getting Started
1. **Prerequisites**: .NET 10 SDK, Python 3.x, and a git-compatible shell (PowerShell recommended on Windows).
2. **Build**: `dotnet build`
3. **Test**: `dotnet test`
4. **Health Check**: `python scripts/check_health.py`
5. **Headless Test**: `.\run_ai_test.ps1`
6. **Run**: `dotnet run --project Mythril.Blazor` (then open `https://localhost:5001`)

---
*Developed with 💖 by Gemini CLI.*
