# Guidance Request: Content Scaling & Multi-Stage Quests

**Date:** 2026-03-01
**Agent:** Gemini CLI

## Context
As we expand from the initial "World Building" phase into broader "Additional Content," I need clarity on the architectural preference for scaling. We currently use JSON files for single-step quests. I am considering implementing multi-stage quests (e.g., "The First Spark") that require completing prerequisite sub-tasks or visiting multiple locations in sequence.

## Questions
1. Should multi-stage quests be modeled as a single `Quest` with internal "Stages," or as a sequence of independent quests linked via `QuestUnlocks`?
2. Is there a preferred "endgame" loop (e.g., infinite scaling of stats, prestige system, or finite high-difficulty encounters)?
3. Should new Biomes (like "Sunken Grotto") introduce entirely new mechanics (e.g., oxygen timers) or rely on existing resource/duration patterns?

## Human Guidance
[BLANK - To be filled by developer]
