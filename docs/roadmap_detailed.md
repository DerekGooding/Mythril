# Detailed Technical Roadmap: Content Graph Transition

## ⚙️ Mechanical Specifications

### 1. Unified Schema (The Content Node)
Every piece of game data (Quest, Location, Cadence, Item, Refinement) will be represented as a `ContentNode`.

**Node Fields:**
🛡️ `id`: Unique string (e.g., `q_prologue`, `loc_village`, `i_gold`).
🛡️ `type`: String enum (`Quest`, `Location`, `Cadence`, `Item`, `Refinement`, `Stat`).
🛡️ `name`: Display name.
🛡️ `data`: Dynamic dictionary for type-specific properties (duration, repeatability, etc).
🛡️ `in_edges`: Requirements (other node IDs).
🛡️ `out_edges`: Unlocks and Rewards (other node IDs with quantities).

### 2. Migration Logic (`scripts/migrate_to_graph.py`)
- **Input**: The 11 JSON files in `wwwroot/data/`.
- **Mapping**:
    - `quests.json` + `quest_details.json` + `quest_unlocks.json` → `Quest` nodes.
    - `locations.json` → `Location` nodes + `Quest` containment edges.
    - `refinements.json` → `Refinement` nodes + `Item` input/output edges.
- **Output**: A single `content_graph.json` array.

### 3. Semantic Linter Rules (`scripts/verify_graph.py`)
This script will implement graph traversal algorithms to enforce the following:

🛡️ **Connectivity**:
- **Root Validation**: Only one node of type `Quest` has zero incoming edges of type `requires_quest`.
- **Reachability**: All nodes must have a valid path from the Root node.
- **Cycle Detection**: Ensure no cycles exist in the `Quest` or `Location` unlock chains.

🛡️ **Semantic Contracts**:
- **Initial Capital**: The Root node must reward `item_gold` >= 100.
- **Location Lockdown**: Every Location node (except Village) must have an incoming edge from a Quest node.
- **Useless Content**: Every node of type `Quest` (with `Type: Single` or `Unlock`) must either unlock a new node or be a requirement for another node.
- **Capacity Check**: Refinement requirements must be holdable within the `MagicCapacity` available at the time the refinement becomes reachable.

### 4. Visualization (`Mermaid`)
- Export logic will translate the JSON graph into Mermaid's `graph TD` syntax.
- **Nodes**: Styled by type (e.g., Quests as rectangles, Locations as rounded rectangles).
- **Edges**: Labeled by relationship (e.g., `requires`, `unlocks`, `rewards`).

## 🧪 Implementation Guidelines
- **Zero Data Loss**: The migration script must include a checksum or item count verification to ensure every existing quest and item is preserved.
- **Backwards Compatibility**: During refactoring, the `ContentLoader` will temporarily support both formats to allow incremental migration.
- **Performance**: Use a standard Graph library (like `networkx` in Python) for verification to ensure rapid health checks.
- **Graph Invariants**: The graph is a Directed Acyclic Graph (DAG) regarding unlocks, but can be cyclic regarding resource flows (which is handled by the Flow Simulator).
