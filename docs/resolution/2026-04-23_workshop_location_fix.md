# Resolution: Workshop Sorting & Location Management

## Problem
1.  **Workshop Sorting**: The workshop's sorting logic previously only considered the output item's type. This meant that refinements with non-material/non-spell outputs (like Currency/Gold) didn't appear in the "Materials" or "Spells" tabs, even if their input items were materials or spells.
2.  **CMS Location Management**: The Content Manager lacked a way to view or modify which quests belonged to a specific location.
3.  **Health Check Failure**: The project health check was failing due to missing quest data ("Buy Potion") and graph root violations caused by out-of-sync content files.

## Technical Resolution
1.  **Workshop Logic Update**: 
    *   Modified `Mythril.Blazor/Components/Workshop.razor` to include a fallback check.
    *   If a refinement's output item type is neither `Material` nor `Spell`, the filter now checks the `InputItem`'s type against the active filter.
2.  **CMS Enhancements**:
    *   Updated `modules/contentManager/app.py` and `ui_components.py` to support `Locations` editing.
    *   Added a `Required Quest` selector and a `Quests in Location` string list editor to the CMS GUI.
3.  **Data Integrity Fixes**:
    *   Re-added missing `Buy Potion` and `Sell Gem` quests to `quests.json` and `quest_details.json`.
    *   Updated `quest_unlocks.json` to make `Visit Starting Town` a prerequisite for these quests, restoring a single root quest ("Prologue") and satisfying graph integrity rules.
    *   Updated `Mythril.Tests/QuestRewardTests.cs` to align with current quest reward data and prerequisite structures.

## Verification
*   Ran `scripts/migrate_to_graph.py` and `scripts/verify_graph.py`; confirmed 0 contract violations.
*   Ran 205 unit tests; all passing.
*   Ran `scripts/check_health.py`; reachability simulation and economic sustainability passed.
