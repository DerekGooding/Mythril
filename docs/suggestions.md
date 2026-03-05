# Content Integrity & Graph-Based Architecture Suggestions

The current content system is "Relational but Decoupled" across 11 different JSON files. To improve long-term scalability and enable automated integrity checks, I suggest transitioning to a **Formal Content Graph**.

## 1. The Concept: Content Graph Validation
This approach treats every quest, location, and cadence as a **Node** in a Directed Graph. The relationships (requirements, rewards, unlocks) are the **Edges**.

### Why Restructure?
- **Standardized Format**: Moving from many files to a single `content_graph.json` (or a directory of node-schema files).
- **Inherent Integrity**: A node cannot point to a non-existent requirement if the schema requires a valid Node ID.
- **Automated Visualization**: We could programmatically generate "Flow Maps" (using Mermaid or Graphviz) to see the entire player journey visually.
- **Cycle Detection**: Using standard graph algorithms to ensure we haven't created a circular dependency (e.g., Quest A requires B, B requires A).

## 2. Proposed Unified Schema (The "Node" Format)
Instead of separate detail/unlock/location files, each content item becomes a self-contained node:

```json
{
  "id": "quest_tutorial",
  "type": "Quest",
  "name": "Tutorial Section",
  "container": "Village",
  "properties": {
    "duration": 15,
    "repeatable": false
  },
  "in_edges": {
    "requires_quest": ["quest_prologue"],
    "requires_stat": { "Vitality": 10 }
  },
  "out_edges": {
    "unlocks_location": ["loc_greenwood"],
    "rewards_item": [{ "item": "item_gold", "qty": 50 }]
  }
}
```

## 3. Mandatory Integrity Rules (The Semantic Linter)
Regardless of the file format, the following rules should be enforced programmatically:

### A. Connectivity Rules
- **The Root Rule**: Exactly ONE node must have `in_edges: []` (The Prologue).
- **The Dead-End Rule**: Every `Single` type node must have at least one `out_edge` that points to another quest, location, or cadence.
- **Orphan Rule**: Every node must be reachable from the Root.

### B. Progression Rules
- **The Economic Anchor**: The Root node must reward `item_gold: 100` to prevent initial soft-locks.
- **Location Gating**: Every Location node (except Village) must have a `RequiredQuest` edge pointing to a Quest node.
- **Capacity Constraint**: No `Refinement` node can require an `InputQuantity` that exceeds the `base_magic_capacity` unless it is reachable only after a `MagicPocket` ability node.

## 4. Implementation Path
1.  **Refactor**: Create a migration script to combine `quests.json`, `quest_details.json`, `quest_unlocks.json`, etc., into a unified `content_graph.json`.
2.  **Validate**: Create `scripts/verify_graph.py` to check the DAG for cycles, orphans, and semantic violations.
3.  **Graphing**: Add a tool to export the graph to a Markdown-compatible Mermaid diagram for documentation.
