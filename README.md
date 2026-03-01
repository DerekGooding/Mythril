# Mythril

A .NET 9 Blazor-based incremental RPG with a focus on job-based progression (Cadences) and tactical stat management (Junctioning).

## Key Features
- **Cadence System**: Unlock and equip different "Cadences" (jobs) to change your character's capabilities.
- **Junctioning**: Assign magic items to character stats to gain powerful bonuses, inspired by classic RPG mechanics.
- **Item Refinement**: Craft new items and magic through specialized abilities in the Workshop.
- **Quest System**: Real-time quest progression with offline progress continuity.
- **Persistence**: Save and load your journey across sessions.

## Tech Stack
- **Frontend**: Blazor WebAssembly (.NET 9)
- **Data**: JSON-driven content for easy modification.
- **Testing**: MSTest for unit tests and a Headless CLI for scenario verification.

## Development
To run the project:
1. `dotnet run --project Mythril.Blazor`
2. Open `https://localhost:5001`

To run tests:
`dotnet test`
