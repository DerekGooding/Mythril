# Resolution: CMS Widget Identity & Workshop Refinement

## Problems
1.  **CMS Deletion Bug**: When deleting an item from a list (Requirements, Quests in Location, etc.) in the Content Manager, Streamlit would often remove the last item in the UI regardless of which "trashcan" button was clicked. This was caused by using simple indices for widget keys, leading to state-shifting issues when the list size changed.
2.  **Workshop Visibility**: Some workshop tasks (like "Sell Gem") were not appearing in the expected tabs (e.g., Materials) because the filter only checked the output item type (Currency/Gold), ignoring the material nature of the input.

## Technical Resolution
1.  **Robust Key Generation**:
    *   Updated all editor components in `modules/contentManager/ui_components.py` (`edit_list`, `edit_dict_list`, `edit_recipes`, `edit_cadence_abilities`, `edit_string_list`) to use a `row_id`.
    *   `row_id` combines the loop index with the content of the row (e.g., item name or key), ensuring that Streamlit can uniquely identify each widget even when other rows are added or removed.
    *   Applied `safe_key` (derived from the entity name) to the `Locations` quest editor in `app.py` to prevent state leakage between different locations.
2.  **Workshop Filter Fallback**:
    *   Refined the filtering logic in `Mythril.Blazor/Components/Workshop.razor`.
    *   If a refinement's output type does not match the active filter (Materials or Spells), the filter now falls back to checking the **Input Item's type**.
    *   This ensures that "Sell Gem" (Material -> Gold) is correctly listed under "Materials".

## Verification
*   Verified through health check that all 205 unit tests pass.
*   Verified that reachability and economic sustainability simulations pass.
*   The `content_graph.json` was successfully re-synchronized and verified with 0 violations.
