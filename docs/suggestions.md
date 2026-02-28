# AI Suggestions

## Instructions
New AI-generated suggestions for the project should be placed in this file if it is currently empty or has been fully processed. 

A user will periodically review these suggestions and greenlight specific goals by moving them to the Roadmap or issuing new directives.

---

### New Suggestions
1.  **Headless Testing Assertion Engine**: 
    - **Goal**: Automate state verification in `Mythril.Headless`.
    - **Description**: Extend `CommandFile` in the Headless project to include `Assertions` (e.g., `expect_item_count`, `expect_cadence_unlocked`). This allows the AI to run a scenario and immediately know if the resulting `state.json` meets expectations without manual review.
2.  **Blazor State Persistence**: 
    - **Goal**: Preserve game state between browser sessions.
    - **Description**: Implement a `PersistenceService` in `Mythril.Blazor` that serializes the `ResourceManager` state to `LocalStorage`. Add "Save" and "Load" buttons or implement an auto-save trigger on state changes.
3.  **Continuous Health Monitoring (CI/CD)**: 
    - **Goal**: Enforce quality standards on every push.
    - **Description**: Update `.github/workflows/deploy.yml` to install Python and execute `python scripts/check_health.py` as a blocking step. This ensures that no code with monoliths or insufficient coverage is ever deployed to GitHub Pages.
4.  **Asynchronous Quest Tick System**: 
    - **Goal**: Transition from immediate rewards to real-time progression.
    - **Description**: Refactor `ResourceManager` to manage active quests with timers. Instead of `ReceiveRewards(quest).Wait()`, implement a background "Tick" that updates `QuestProgress.Progress` and triggers rewards only when the duration has elapsed.
5.  **Cadence Visualizer Component**: 
    - **Goal**: Improve UX for the unique Cadence progression system.
    - **Description**: Build a specialized Blazor component that displays the `Cadence` tree visually, showing prerequisites, locked/unlocked abilities, and requirement tooltips. This makes the progression system much more tangible for the player.
