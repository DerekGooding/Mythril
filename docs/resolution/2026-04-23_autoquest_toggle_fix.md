# Resolution: Missing AutoQuest Toggle

## Problem
The `AutoQuest` toggle button was missing from the UI for characters who should have had it unlocked. This was caused by the Content Manager (CMS) stripping the `Effects` field from `cadence_abilities.json` during a previous save operation, as `Effects` were not part of the CMS's internal data model.

## Technical Resolution
1.  **Effects Editing**: Added a new `edit_effects` UI component to `modules/contentManager/ui_components.py` to allow manual management of `AutoQuest`, `Logistics`, and `MagicCapacity` effects.
2.  **CMS Integration**: Integrated `edit_effects` into the `Quests` and `Cadences` editing pages in `app.py`.
3.  **Data Persistence**: Updated `data_io.py` to correctly unify and save `Effects` for both Quests and Cadences, preventing future data loss.
4.  **Deep Copying**: Implemented `deepcopy` to ensure effects aren't accidentally shared between abilities.

## Verification
*   Ran health check simulation; confirmed `AutoQuest` abilities are correctly detected (`[DEBUG] Unlocked Ability: Recruit:AutoQuest I`).
*   Verified that saving in the CMS now preserves the `Effects` array in `quest_details.json` and `cadences.json`.
