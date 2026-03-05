## 🎯 Current Goal
Implement a robust mutation testing pipeline to quantify test effectiveness and ensure the integrity of core RPG logic. Note: Transitioned from Stryker.NET to a custom pipeline (`scripts/run_mutation.py`) due to .NET 10 source generator conflicts.

## 🛤️ Active Tasks

### Phase 1: Environment & Tooling
- [x] **Custom Mutation Pipeline**: Implemented `scripts/run_mutation.py` to overcome Stryker.NET source generator conflicts.
- [x] **Baseline Configuration**: Established robust mutation patterns for RPG logic.
- [x] **Project Targeting**: Focused on `Mythril.Data` core logic.

### Phase 2: Core Logic Mutation (Mythril.Data)
- [x] **Exclusion Rules**: Heuristics added to script to avoid mutating declarations and boilerplate.
- [x] **Baseline Run**: Successful execution against `JunctionManager`, `InventoryManager`, etc.
- [x] **Survivor Analysis**: Identified critical survivors in `JunctionMagic` logic.

### Phase 3: Test Suite Hardening
- [x] **Junction Logic Fixes**: Added `JunctionManager_JunctionMagic_StrictlyRequiresCorrectAbility` to `Mythril.Tests`.
- [ ] **Resource Flow Fixes**: Add edge-case tests for `InventoryManager` and `ResourceManager_Logistics`.
- [ ] **Simulation Verification**: Ensure `ReachabilitySimulator` logic is fully protected against logic mutations.

### Phase 4: Pipeline & Health Integration
- [x] **Health Check Integration**: Updated `scripts/check_health.py` to support `--mutation` flag and enforce minimum score.
- [ ] **Reporting Utility**: Create a script to archive mutation reports into a `docs/mutation_reports/` directory.
- [ ] **CI/CD Configuration**: (Future) Add mutation testing steps to the GitHub Actions workflow.

### Phase 5: Enforcement
- [x] **Threshold Setting**: Set a project-wide mandatory Mutation Score of 60% for Core Logic.
- [ ] **Final Validation**: Pass health checks with mutation testing enabled.
