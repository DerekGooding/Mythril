# Detailed Technical Roadmap: Simulation & Architecture Maturity

Detailed breakdown of current roadmap initiatives.

## 1. Economic Equilibrium Refinement
Achieving 100% sustainability for all reachable recurring content.

- [x] **Data Audit**: Identify all "Unsustainable Activities" using `FlowSimulator` report.
- [x] **Logistics/Resource Injection**: Update `content_graph.json` to ensure bottleneck resources like `Log`, `Mana Leaf`, and `Ancient Bark` have sustainable production loops.
- [x] **Cost-Benefit Balance**: Adjust refinement costs and rewards for late-game content to prevent resource depletion.
- [x] **Verification**: Run `check_health.py` and confirm `shield_sustainability` reaches 100%.

## 2. Dependency-Tracked Fixpoint Solver
Optimizing `LatticeSimulator` performance for large-scale content expansion.

- [x] **Worklist Architecture**: Implement a queue-based `Solve()` loop in `LatticeSimulator.cs`.
- [x] **Dependency Graph Build**: Create an internal map of `Node -> [DependentNodes]` during initialization.
- [x] **Delta Propagation**: Only add nodes to the worklist when their inputs (in-edges) change state.
- [x] **Validation**: Ensure results are bit-for-bit identical to the exhaustive solver.

## 3. Simulation-Driven Regression Assertions
Automated pacing and balance checks for CI.

- [x] **Baseline Establishment**: Record current `Routed Completion Time` as the performance baseline.
- [x] **Pacing Logic**: Add logic to `check_health.py` to compare current sim results with baseline.
- [x] **Regression Thresholds**: Define acceptable variance (e.g., <15% increase without new content).
- [x] **CI Integration**: Fail the health check if pacing regressions are detected.

## 4. UI: Predictive Dependency Overlay
Graph-based navigation assistance for the player.

- [ ] **Pathfinding Logic**: Implement BFS/DFS on the client-side content graph to find the shortest path to a locked node.
- [ ] **UI State Integration**: Update `CadenceTree` and `QuestPanel` to accept a `HighightedNodes` parameter.
- [ ] **Hover Interactions**: Implement hover-triggers that activate the pathfinding and highlight prerequisite nodes.

## 5. Eliminate "Magic String" Metadata
Type-safe ability and quest effects.

- [ ] **Schema Definition**: Create a JSON schema/C# record for `EffectDefinition` (e.g., `StatBoost`, `MagicCapacity`, `AutoQuest`).
- [ ] **Content Migration**: Update all `Ability` and `Quest` nodes in `content_graph.json` to use the new `effects` field.
- [ ] **Loader Refactor**: Update `ContentLoader.cs` to deserialize the typed effects.
- [ ] **Code Cleanup**: Remove `Metadata` dictionary lookups and replace with typed property checks.

## 6. Unified Deterministic State Store
Full parity between simulation and Blazor runtime.

- [ ] **State Record**: Create a single `GameStoreState` immutable record.
- [ ] **Actions & Reducers**: Define formal `Actions` (e.g., `CompleteQuest`, `SpendResource`) and a pure `Reducer` function.
- [ ] **Manager Refactor**: Convert `ResourceManager`, `JunctionManager`, and `InventoryManager` to be view-only subscribers to the `GameStateStore`.
- [ ] **Snapshot Support**: Implement a one-click snapshot/restore feature for testing and save-games.

---
*Last Updated: 2026-04-06*
