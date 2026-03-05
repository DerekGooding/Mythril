# Mythril Project Roadmap: Mutation Testing Overhaul

## 🎯 Current Goal
Implement a robust mutation testing pipeline using **Stryker.NET** to quantify test effectiveness, identify "survivors" (weak test coverage), and ensure the integrity of core RPG logic (Junctions, Resource Scaling, and Simulation).

## 🛤️ Active Tasks

### Phase 1: Environment & Tooling
- [ ] **Stryker Installation**: Verify and document the installation of `dotnet-stryker` as a global or local tool.
- [ ] **Baseline Configuration**: Create `stryker-config.json` in the project root with optimal defaults for .NET 10.
- [ ] **Project Targeting**: Configure Stryker to target `Mythril.Data.csproj` using `Mythril.Tests.csproj` as the test runner.

### Phase 2: Core Logic Mutation (Mythril.Data)
- [ ] **Exclusion Rules**: Exclude DTOs, `Models.cs` (simple records), and generated code from mutation to reduce noise.
- [ ] **Baseline Run**: Execute an initial Stryker run on the `ResourceManager` and `JunctionManager` to establish a current Mutation Score.
- [ ] **Survivor Analysis**: Identify critical survivors in the Junction math and Resource scaling logic.

### Phase 3: Test Suite Hardening
- [ ] **Junction Logic Fixes**: Add targeted MSTests to "kill" survivors in `JunctionManager.cs`.
- [ ] **Resource Flow Fixes**: Add edge-case tests for `InventoryManager` and `ResourceManager_Logistics`.
- [ ] **Simulation Verification**: Ensure `ReachabilitySimulator` logic is fully protected against logic mutations.

### Phase 4: UI & Component Mutation (Mythril.Blazor)
- [ ] **bUnit Integration**: Configure Stryker to run against `Mythril.Blazor` using the bUnit test suite.
- [ ] **Conditional Rendering Protection**: Mutate conditional UI logic (e.g., `IsActive` dots, `IsCompact` expanders) to ensure tests fail if visual logic breaks.

### Phase 5: Pipeline & Health Integration
- [ ] **Health Check Integration**: Update `scripts/check_health.py` to optionally run Stryker and check for a minimum Mutation Score threshold.
- [ ] **Reporting Utility**: Create a script to archive Stryker HTML reports into a `docs/mutation_reports/` directory.
- [ ] **CI/CD Configuration**: (Future) Add Stryker steps to the GitHub Actions workflow.

### Phase 6: Enforcement
- [ ] **Threshold Setting**: Set a project-wide mandatory Mutation Score (e.g., 60% for Core Logic).
- [ ] **Final Validation**: Pass health checks with mutation testing enabled.
