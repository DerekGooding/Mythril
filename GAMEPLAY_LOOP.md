# Mythril: Gameplay Loop

## Game Overview

Mythril is an incremental game with RPG elements, inspired by classic Final Fantasy titles. The core of the game revolves around managing a party of adventurers, sending them on tasks to gather resources, and using those resources to grow stronger and take on more challenging encounters.

## The Core Loop

The fundamental gameplay loop in Mythril consists of the following steps:

1.  **Task Generation:** The game continuously generates a list of available tasks. These tasks represent various activities your characters can undertake, such as "Gathering Herbs," "Mining Ore," or "Patrolling the Area." Each task has a specific duration and a reward value.

2.  **Assigning Tasks:** You, the player, assign these tasks to the characters in your party. To do this, you simply drag a task from the "Hand" panel and drop it onto a character in the "Party" panel.

3.  **Task Completion:** Once a task is assigned, the character begins working on it. A progress bar will appear, showing the task's completion status. The time it takes to complete a task is determined by its duration.

4.  **Reaping the Rewards:** Upon task completion, you will automatically receive a reward, which is currently a set amount of Gold. The completed task is then removed, and a new, random task is generated, ensuring a constant flow of activities.

## Character Progression

While not fully implemented yet, the long-term goal for character progression is as follows:

*   **Experience Points (XP):** Characters will gain XP for completing tasks and winning battles.
*   **Leveling Up:** Accumulating enough XP will cause a character to level up, improving their base stats.
*   **Jobs:** Characters can be assigned different jobs (e.g., Knight, Archer, Black Mage), each with unique abilities and equipment affinities. Characters will gain Job Points (JP) to unlock new abilities within their current job.
*   **Materia:** The game will feature a Materia system, allowing you to customize your characters' abilities by equipping magical orbs to their weapons and armor.

## Combat

The game will feature a turn-based combat system, similar to classic JRPGs.

*   **Random Encounters:** As you explore different zones, you will randomly encounter enemies.
*   **Turn-Based-Combat:** You will issue commands to your party members, such as "Attack," "Defend," "Magic," or "Item."
*   **Rewards:** Defeating enemies will yield Gold, XP, JP, and occasionally, rare items or Materia.

This document provides a high-level overview of the intended gameplay experience for Mythril. As the game is in active development, these mechanics are subject to change and refinement.

## Proposed New Features

To further enhance the gameplay experience, we propose the following new features:

*   **Crafting System:** A system that allows players to use resources gathered from tasks and combat to craft new equipment, consumable items, and even Materia. This would add another layer of depth to the resource management aspect of the game.

*   **Quest System:** A simple quest system that provides players with short-term goals, such as "Defeat 10 Goblins" or "Craft a Potion." Completing quests would yield unique rewards, such as rare items or large sums of Gold.

*   **Dungeons:** Instanced areas with a series of combat encounters culminating in a challenging boss fight. Dungeons would be a great source of XP, JP, and rare loot.

*   **Skill Trees:** Each job could have its own skill tree, allowing for greater character customization. Players could spend JP to unlock new active and passive abilities, tailoring each character to their preferred playstyle.
