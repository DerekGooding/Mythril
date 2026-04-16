# Mythril Project Roadmap: Simulation & Architecture Maturity

This roadmap focuses on refining the game's economic balance, optimizing the core simulation engines, and enhancing the UI with predictive data.

## Phase 1: Economic & Simulation Hardening
- [x] **Economic Equilibrium Refinement**: Achieve 100% sustainability for all reachable recurring activities.
- [x] **Dependency-Tracked Fixpoint Solver**: Implement a worklist-based `LatticeSimulator` for $O(I \cdot \text{avg\_out\_degree})$ performance.
- [x] **Simulation-Driven Regression Assertions**: Integrate end-game pacing checks into `check_health.py`.

## Phase 2: UI & UX Predictive Intelligence
- [x] **Eliminate "Magic String" Metadata**: Refactor ability/quest effects into a typed `EffectDefinition` schema.
- [x] **Predictive Resource Highlighting**: Visual indicators for unaffordable quest requirements in real-time.
- [x] **Effect-Driven UI Logic**: Passive stat boosts and magic capacity now driven by typed effects.
- [x] **Predictive Dependency Overlay**: Implement "Prerequisite Path" highlighting in the `CadenceTree` and `QuestPanel`.

## Phase 3: State & Runtime Consolidation
- [x] **Unified Deterministic State Store**: Implement a central Reducer-based `GameStateStore` for full simulation/runtime parity.

---
*Last Updated: 2026-04-08*
