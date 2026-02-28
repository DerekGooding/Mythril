# Mythril Agent Configuration

## Project Overview
This project is built with **.NET 9 (Blazor)** and is primarily developed by **AI agents**.  
All code is written in C# and unit tests use **MSTest** with **Moq** and **Coverlet**.

## Agentic Mandates
Agents must adhere to the [GEMINI.md](GEMINI.md) mandates, which include:
- **Architecture First**: Clean separation of logic and presentation.
- **Validation**: All logic changes must have corresponding unit tests.
- **Health Checks**: Run `python scripts/check_health.py` before completing any significant feature.

## Technical Standards
- **Framework**: .NET 9 (Blazor WebAssembly)
- **OS**: Windows
- **Shell**: PowerShell (use `;` for command chaining)
- **Coverage**: Maintain overall coverage above 70%.

## Environment Setup
Ensure you have the .NET 9 SDK and Python 3.x installed.

```powershell
dotnet build
dotnet test
python scripts/check_health.py
```
