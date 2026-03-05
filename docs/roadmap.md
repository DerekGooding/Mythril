# Mythril Project Roadmap: Content Graph Transition

## 🎯 Current Goal
Transition the project from a relational multi-file data structure to a formal **Content Graph Architecture**, enabling advanced integrity validation, automated flow visualization, and robust semantic linting.

## 🛤️ Active Tasks

### Phase 1: Unified Graph Schema & Migration
- [ ] **Schema Definition**: Define the formal `ContentNode` and `ContentEdge` structures in `Mythril.Data/Models.cs`.
- [ ] **Migration Utility**: Create `scripts/migrate_to_graph.py` to consolidate 11 separate JSON files into `Mythril.Blazor/wwwroot/data/content_graph.json`.
- [ ] **Data Verification**: Ensure no content is lost or corrupted during the migration process.

### Phase 2: Semantic Linter (Content Contract)
- [ ] **Verification Engine**: Create `scripts/verify_graph.py` to perform Directed Acyclic Graph (DAG) analysis.
- [ ] **The Root Rule**: Enforce that exactly ONE node (Prologue) has zero requirements.
- [ ] **The Economic Anchor**: Assert that the Root node rewards 100 Gold.
- [ ] **Progression Enforcement**: Implement Location Gating, Dead-End Detection, and Orphan Node detection.
- [ ] **Capacity Logic**: Verify that refinement requirements never exceed reachable `MagicCapacity`.

### Phase 3: Graph Visualization
- [ ] **Mermaid Exporter**: Implement a utility to convert `content_graph.json` into a Mermaid Markdown diagram.
- [ ] **Standalone Tool**: Create `scripts/export_graph_mermaid.py` to print the entire content web as a Mermaid graph.
- [ ] **Auto-Documentation**: Automatically update `docs/content_web.md` with the latest generated graph visualization.

### Phase 4: Engine Refactor
- [ ] **ContentLoader Update**: Refactor `ContentLoader.cs` to load the unified graph instead of separate files.
- [ ] **ResourceManager Integration**: Update `ResourceManager` and its partial classes to query the graph structure.
- [ ] **JunctionManager Update**: Align Junction logic with the new node-based data model.

### Phase 5: Health & Integrity Enforcement
- [ ] **Health Check Integration**: Add `verify_graph.py` as a mandatory step in `python scripts/check_health.py`.
- [ ] **Validation Pass**: Pass all health checks and simulation runs using the new graph-driven engine.
