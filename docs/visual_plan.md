# Homegrown Game Data Visualization Plan - V2 (Clustered & Chronological)

## 1. Objective
Enhance the homegrown visualization to move beyond a "floating soup" of nodes. The goal is to provide a structured, grouped, and chronologically flowing representation of game data.

## 2. Structural Improvements

### A. Clustered Layout Logic
Nodes will no longer float freely. They will be bound to "Clusters" based on their data relationships:
- **Quest Nodes**: Grouped by the **Location** they belong to.
- **Ability Nodes**: Grouped by the **Cadence** they are associated with.
- **Refinement Nodes**: Grouped by their **Category** (Magic vs. Material).
- **Item Nodes**: Floating near the quests/refinements that produce/consume them.

### B. Chronological Left-to-Right Flow
The visualization will enforce a progression-based X-axis:
- **Depth Calculation**: Using Breadth-First Search (BFS) starting from the `quest_prologue`, every node will be assigned a `tier` (depth).
- **X-Constraint**: A node's X-position will be strictly or softly constrained by its `tier`. 
    - Tier 0 nodes (Start) on the far left.
    - End-game nodes on the far right.
- **Progressive Force**: The force-directed engine will apply a "Wind" or "Flow" force that gently pushes nodes toward their tier-appropriate X-coordinate.

## 3. Visual Enhancements

### A. Visual Grouping (Containers)
The SVG will render background "Region Boxes" for major clusters:
- **Locations**: Large, semi-transparent titled boxes containing all quests for that region.
- **Cadences**: Titled boxes containing the cadence node and its abilities.
- **Refinement Labs**: A dedicated area for workshop recipes.

### B. "Webbed" Connection Styling
- **Edge Routing**: Lines will be rendered with slight curves to avoid overlapping node labels and to emphasize the left-to-right flow.
- **Progress Markers**: Directional arrows will be more prominent.
- **Path Highlighting**: Clicking a node will highlight the entire "upstream" path (requirements) and "downstream" path (unlocks) to show the progression chain.

## 4. Technical Strategy (Scripts/visualize_v2.py)

### 1. Data Enrichment
The Python generator will pre-process the graph to identify:
- `cluster_id`: Mapping nodes to their parent Location/Cadence.
- `progression_tier`: The BFS depth.
- `refinement_type`: Categorizing refinements as 'Magic' or 'Material' based on their inputs.

### 2. Updated JS Force Engine
The embedded JavaScript will be updated with:
- `ClusterForce`: A force that pulls nodes toward the centroid of their cluster.
- `ProgressionForce`: A force that snaps nodes to X-lanes based on their tier.
- `CollisionForce`: Improved radius-based collision to prevent overlap within clusters.

## 5. View Modes Refinement
- **Lattice View (Default)**: The primary organized, webbed graph.
- **Hierarchy View**: Remains as a clean, tiered list for quick reference.

## 6. Verification
- Validate that Quests for "Dark Forest" are visually grouped together.
- Confirm that "Arcanist" abilities are clustered near the Arcanist Cadence node.
- Ensure that the progression from "Tutorial" to "End Game" moves consistently from left to right.
- Verify that the force-directed simulation settles into a readable state without manual intervention.
