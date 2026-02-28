# AI Suggestions

## Instructions
New AI-generated suggestions for the project should be placed in this file if it is currently empty or has been fully processed. 

A user will periodically review these suggestions and greenlight specific goals by moving them to the Roadmap or issuing new directives.

**Tip:** When adding suggestions, ensure they are actionable, scoped, and aligned with the current architectural phase.

---

### New Suggestions
1.  **Comprehensive Persistence (Active Quests)**:
    - **Goal**: Fully restore the game state, including mid-progress tasks.
    - **Description**: Refactor `QuestProgress` to support serialization via DTOs (Data Transfer Objects). Store quest/cadence IDs and character names in `SaveData` so the `PersistenceService` can reconstruct the `ActiveQuests` list on load, ensuring players don't lose time on long-running tasks.
2.  **Item Refinement Workshop UI**:
    - **Goal**: Expose the `ItemRefinements` logic to the player.
    - **Description**: Create a "Workshop" tab in the Blazor frontend. Utilize the existing `ItemRefinements` data to show valid recipes. Allow users to drag-and-drop materials to produce refined items (e.g., turning Logs into Herbs or Gems into Magic Spells).
3.  **Data-Driven Content Migration**:
    - **Goal**: Decouple game content from the compiled binary.
    - **Description**: Migrate static data definitions (`Items`, `Quests`, `Cadences`) from C# singleton classes to JSON files in `wwwroot/data`. Update `ContentHost` to deserialize these files at startup. This enables faster content balancing and allows agents to modify the game world without touching core engine code.
4.  **Agentic "Status Report" Utility**:
    - **Goal**: Improve project visibility for AI agents.
    - **Description**: Create a specialized script/prompt that runs `check_health.py` and `run_ai_test.ps1`, then aggregates the results into a concise `STATUS.md`. This gives any new agent entering the workspace an immediate, accurate snapshot of system integrity and functional state.
5.  **Character Specialization & Stats UI**:
    - **Goal**: Make character differences meaningful.
    - **Description**: Expand `CharacterDisplay` to show the stats defined in `Mythril.Data.Stats`. Implement logic where character stats affect `QuestProgress` (e.g., high Strength reduces "Farm Golems" duration). This adds a layer of strategy to character assignment.
