# Resolution: Content Manager Data Integrity & Refinement Loading

## Problem
1.  **Refinement Loading**: When switching between refinements in the CMS, nested data (like Recipes) appeared to load from previous or "topmost" refinements. This was caused by Streamlit's state persistence when using non-unique keys for nested UI components.
2.  **Data Loss (Effects)**: Saving changes in the CMS resulted in "Effects" (like AutoQuest or Logistics) being stripped from the JSON files because they were not handled in the unified data model.
3.  **Reference Sharing**: The `copy()` method used in `data_io.py` created shallow copies, potentially allowing nested lists to be shared between different entities in memory.

## Technical Resolution
1.  **Deep Copying**: Updated `modules/contentManager/data_io.py` to use `copy.deepcopy()` instead of `copy()` for all unification logic, ensuring complete isolation of nested data structures.
2.  **Unique Streamlit Keys**: Updated `modules/contentManager/app.py` to generate unique keys for all nested UI components by including the `selected_name` of the entity being edited.
3.  **Preserving Metadata**: Modified `data_io.py` to preserve extra fields (like `Effects` and `Metadata`) when loading and saving, ensuring the CMS does not strip unhandled data.

## Verification
*   Verified that `refinements.json` preserves `Ability` keys correctly.
*   Confirmed that switching between entities in the CMS reloads fresh data for each nested list.
