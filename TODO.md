# Mythril Project TODO List

## Core Gameplay (15 points)
- [ ] 1. Implement a more robust game loop tick system.
- [ ] 2. Implement a system for gaining experience points for characters.
- [ ] 3. Implement a character leveling system.
- [ ] 4. Implement a system for earning Gil (money).
- [ ] 5. Implement a basic inventory system for items.
- [ ] 6. Implement a system for using consumable items from the inventory.
- [ ] 7. Design and implement a basic crafting system.
- [ ] 8. Implement a system for random encounters.
- [ ] 9. Implement a turn-based combat system.
- [ ] 10. Implement basic attack and defend commands in combat.
- [ ] 11. Implement a system for fleeing from combat.
- [ ] 12. Implement a system for game over and continuing/loading a saved game.
- [ ] 13. Implement a basic quest or mission system.
- [ ] 14. Implement a system for saving and loading game progress.
- [ ] 15. Implement a day/night cycle that affects gameplay.

## UI (20 points)
- [ ] 16. Redesign the main game screen to be more visually appealing.
- [ ] 17. Create a dedicated inventory screen.
- [x] 18. Create a dedicated character status screen.
- [x] 19. Create a dedicated job screen for changing and viewing jobs.
- [x] 20. Create a dedicated materia screen for equipping and managing materia.
- [ ] 21. Create a dedicated shop screen for buying and selling items.
- [ ] 22. Create a dedicated save/load screen.
- [ ] 23. Create a dedicated quest log screen.
- [ ] 24. Implement tooltips for UI elements.
- [ ] 25. Add sound effects to UI interactions.
- [ ] 26. Add background music to the game.
- [ ] 27. Implement a settings screen with volume controls.
- [ ] 28. Ensure the UI is fully navigable with a gamepad.
- [ ] 29. Add visual feedback for button presses and other interactions.
- [ ] 30. Implement a notification system for important events.
- [ ] 31. Add a world map screen.
- [ ] 32. Improve the visual design of the card widgets.
- [ ] 33. Improve the visual design of the drop zone.
- [ ] 34. Add animations for cards being drawn and played.
- [ ] 35. Add a combat log to the UI.

## Materia System (25 points)
- [x] 36. Implement the base `Materia` class.
- [x] 37. Implement `MagicMateria` (Green).
- [x] 38. Implement `SummonMateria` (Red).
- [x] 39. Implement `CommandMateria` (Yellow).
- [x] 40. Implement `IndependentMateria` (Purple).
- [x] 41. Implement `SupportMateria` (Blue).
- [x] 42. Implement a system for materia leveling up with AP (Ability Points).
- [x] 43. Implement the "Barrier" magic materia.
- [x] 44. Implement the "Fire" magic materia.
- [x] 45. Implement the "Ice" magic materia.
- [ ] 46. Implement the "Lightning" magic materia.
- [ ] 47. Implement the "Restore" magic materia.
- [ ] 48. Implement the "Revive" magic materia.
- [x] 49. Implement the "Ifrit" summon materia.
- [ ] 50. Implement the "Shiva" summon materia.
- [ ] 51. Implement the "Ramuh" summon materia.
- [ ] 52. Implement the "Steal" command materia.
- [ ] 53. Implement the "Deathblow" command materia.
- [ ] 54. Implement the "HP Plus" independent materia.
- [ ] 55. Implement the "MP Plus" independent materia.
- [ ] 56. Implement the "All" support materia.
- [ ] 57. Implement materia slots on weapons and armor.
- [ ] 58. Implement linked materia slots.
- [ ] 59. Implement a system for finding new materia in the game world.
- [ ] 60. Implement a system for buying and selling materia.

## Job System (20 points)
- [ ] 61. Implement the base `Job` class.
- [ ] 62. Implement the "Squire" job.
- [ ] 63. Implement the "Chemist" job.
- [ ] 64. Implement the "Knight" job.
- [ ] 65. Implement the "Archer" job.
- [ ] 66. Implement the "Monk" job.
- [ ] 67. Implement the "Priest" (White Mage) job.
- [ ] 68. Implement the "Wizard" (Black Mage) job.
- [ ] 69. Implement the "Time Mage" job.
- [ ] 70. Implement the "Summoner" job.
- [ ] 71. Implement a system for gaining Job Points (JP).
- [ ] 72. Implement a system for unlocking new jobs.
- [ ] 73. Implement job-specific abilities.
- [ ] 74. Implement job-specific equipment restrictions.
- [ ] 75. Implement a system for characters to change jobs.
- [ ] 76. Implement a system for mastering jobs.
- [ ] 77. Implement the "Thief" job.
- [ ] 78. Implement the "Geomancer" job.
- [ ] 79. Implement the "Lancer" (Dragoon) job.
- [ ] 80. Implement the "Samurai" job.

## Content (10 points)
- [ ] 81. Create a starting town area.
- [ ] 82. Create a world map with multiple locations to visit.
- [ ] 83. Create at least 3 dungeons with unique enemies and a boss.
- [ ] 84. Create a variety of enemy types with different stats and abilities.
- [ ] 85. Create a variety of weapons and armor.
- [ ] 86. Create a variety of consumable items.
- [ ] 87. Write a compelling story and dialogue for the main quest.
- [ ] 88. Create a cast of interesting characters to join the player's party.
- [ ] 89. Create side quests for the player to complete.
- [ ] 90. Add lore and background information to the game world.

## Technical Debt & Refactoring (10 points)
- [ ] 91. Refactor the `GameManager` to handle more complex game states.
- [ ] 92. Refactor the `ResourceManager` to be more robust.
- [ ] 93. Refactor the `TaskManager` to better integrate with the combat and quest systems.
- [x] 94. Add unit tests for the core game logic.
- [ ] 95. Add integration tests for the UI and game logic.
- [ ] 96. Set up a CI/CD pipeline for automated builds and testing.
- [ ] 97. Profile the game for performance and optimize where necessary.
- [ ] 98. Review and improve code documentation.
- [x] 99. Create a more flexible system for managing game data (e.g., using JSON files).
- [x] 100. Address all items from the original `TODO.md`.
