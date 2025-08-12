## Troubleshooting Persistent UI Issues

1.  **Investigate Myra UI NuGet Package Compatibility:**
    *   **Status:** Investigated. Myra UI 1.5.9 targets .NET Standard 2.0 and is compatible with .NET 9.0. **(COMPLETED)**
2.  **Attempt Myra UI NuGet Package Upgrade:**
    *   **Status:** Attempted. The latest stable version of Myra UI is still 1.5.9, so no upgrade is possible. **(COMPLETED)**
3.  **Re-attempt `Thickness` Implementation (Post-Upgrade):**
    *   **Status:** Failed due to persistent build errors. Reverted changes. **(FAILED/BLOCKED)**
4.  **Re-attempt `Background` Implementation (Post-Upgrade):**
    *   **Status:** Failed due to persistent build errors. Reverted changes. **(FAILED/BLOCKED)**
5.  **Re-attempt Input Event Handling (Post-Upgrade):**
    *   **Status:** Failed due to persistent build errors. Reverted changes. **(FAILED/BLOCKED)**

# UI Improvements and Enhancements

This document outlines potential improvements and enhancements for the game's user interface, suitable for implementation by the Gemini CLI agent.

## General UI Enhancements

1.  **Implement `Thickness` and `Background` properties:** Re-attempt to correctly implement `Padding` and `Background` for `CardWidget` and `DropZoneWidget` by finding the correct Myra UI usage or alternative approaches. This is a recurring issue that needs a robust solution. **(BLOCKED by Troubleshooting Issues)**
2.  **Add `Border` to `CardWidget`:** Implement a visual border around `CardWidget`s to better define their boundaries.
3.  **Implement `CardWidget` drag visual feedback:** Change `CardWidget` appearance (e.g., color, opacity, scale) when it is being dragged to provide clear visual feedback to the player.
4.  **Implement `DropZoneWidget` hover highlighting:** Change `DropZoneWidget` appearance (e.g., background color, border, glow effect) when a draggable `CardWidget` is hovering over it, indicating it's a valid drop target.
5.  **Implement `CardWidget` drag-and-drop functionality:** Make `CardWidget`s actually move with the mouse cursor during a drag operation, providing a more intuitive drag-and-drop experience.
6.  **Center `DropZoneWidget` in its cell:** Ensure `DropZoneWidget` is visually centered within its grid cell in `MainLayout` for better aesthetics.
7.  **Add `TaskProgressWidget` title display:** Re-implement displaying the task title within the `TaskProgressWidget` to clearly identify the task being tracked.
8.  **Add `TaskProgressWidget` completion visual feedback:** Change `TaskProgressWidget` appearance (e.g., color, text, animation) when a task is completed, providing a satisfying visual cue.
9.  **Add `TaskProgressWidget` removal animation:** Implement a fade-out or slide-out animation when a `TaskProgressWidget` is removed from the UI upon task completion.
10. **Improve resource display in `MainLayout`:** Refactor the resource display (e.g., Gold, Mana) in the top row of `MainLayout` to use separate `Label`s for the resource name and its value, and ensure the value updates dynamically.
11. **Add Mana display to `MainLayout`:** Display the Mana resource next to Gold in the top resource panel of `MainLayout`.
12. **Add a "Settings" button to `MainLayout`:** Add a new button to the bottom row of `MainLayout` for accessing game settings.
13. **Implement a basic "Settings" dialog:** Create a simple Myra UI dialog that appears when the "Settings" button is clicked, allowing for future configuration options.
14. **Add a "Pause" button to `MainLayout`:** Add a button to the UI to pause/unpause the game's progression.
15. **Implement game pause/unpause logic:** Hook up the "Pause" button to pause/unpause the `TaskManager` updates, effectively pausing the game.
16. **Add a "Game Over" screen:** Create a simple Myra UI screen that can be displayed when the game ends (e.g., player runs out of resources, reaches a specific game state).
17. **Implement basic sound effects for UI interactions:** Add placeholder sound effects for common UI interactions such as button clicks, card drops, and task completions. (This may require integrating a sound management library).
18. **Refactor `MainLayout.cs` into smaller methods:** Break down the large `MainLayout` constructor into smaller, more manageable and readable methods (e.g., `InitializeResourcesPanel`, `InitializeHandPanel`, `InitializeButtonsPanel`).
19. **Add tooltips to UI elements:** Implement tooltips for interactive UI elements like buttons and cards to provide additional information on hover.
20. **Implement a simple "Inventory" panel:** Add a new panel to the `MainLayout` to display collected items or resources beyond Gold and Mana, providing a visual inventory for the player.