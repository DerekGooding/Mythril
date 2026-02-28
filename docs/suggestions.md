# AI Suggestions

## Instructions
New AI-generated suggestions for the project should be placed in this file if it is currently empty or has been fully processed. 

A user will periodically review these suggestions and greenlight specific goals by moving them to the Roadmap or issuing new directives.

**Mandate:** At least 30% of suggestions must focus on **Additional Content** (e.g., new locations, items, quests, or cadences) to ensure the game world continues to grow alongside technical features.

**Tip:** When adding suggestions, ensure they are actionable, scoped, and aligned with the current architectural phase.

---

### New Suggestions
1.  **Comprehensive Persistence (Active Quests)**: 
    - **Goal**: Fully restore the game state, including mid-progress tasks.
    - **Description**: Refactor `QuestProgress` to support serialization via DTOs. Store quest/cadence IDs and character names in `SaveData` so the `PersistenceService` can reconstruct the `ActiveQuests` list on load.
2.  **Item Refinement Workshop UI**: 
    - **Goal**: Expose the `ItemRefinements` logic to the player.
    - **Description**: Create a "Workshop" tab in the Blazor frontend. Utilize existing `ItemRefinements` data to show valid recipes and allow users to drag-and-drop materials to produce refined items.
3.  **Data-Driven Content Migration**: 
    - **Goal**: Decouple game content from the compiled binary.
    - **Description**: Migrate static data definitions (`Items`, `Quests`, `Cadences`) from C# classes to JSON files in `wwwroot/data`. Update `ContentHost` to deserialize these files at startup.
4.  **Agentic "Status Report" Utility**: 
    - **Goal**: Improve project visibility for AI agents.
    - **Description**: Create a specialized script/prompt that runs `check_health.py` and `run_ai_test.ps1`, then aggregates the results into a concise `STATUS.md`.
5.  **Character Specialization & Stats UI**: 
    - **Goal**: Make character differences meaningful.
    - **Description**: Expand `CharacterDisplay` to show stats. Implement logic where character stats affect `QuestProgress` (e.g., high Strength reduces "Farm Golems" duration).
6.  **Additional Content: The Whispering Woods Biome**:
    - **Goal**: Expand the world map with a nature-themed zone.
    - **Description**: Add "Whispering Woods" to `Locations.cs`. Include quests like "Gather Moonberries" and "Defeat Treant Guardian". Add new items: "Moonberry" (Consumable) and "Ancient Bark" (Material).
7.  **Additional Content: Legendary Questline "The First Spark"**:
    - **Goal**: Implement multi-stage progression via sequential quests.
    - **Description**: Create a chain of 3 linked quests in the Ancient Ruins that culminate in unlocking a unique "Mythril Spark" item. Requires explicit quest-dependency logic in `QuestUnlocks`.
8.  **Additional Content: Master Cadence "Mythril Weaver"**:
    - **Goal**: Provide high-level endgame progression goals.
    - **Description**: Add a high-tier Cadence that requires materials from all existing locations to unlock. Provides powerful abilities like "Mass Refine" and "Global Efficiency Boost".
