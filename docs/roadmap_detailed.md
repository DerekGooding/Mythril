# Detailed Technical Roadmap: Mutation Testing Implementation

Mutation testing ensures that tests are actually checking logic, not just passing over lines of code. By introducing small bugs (mutants) into the source, we verify that the test suite is sensitive enough to fail.

## ⚙️ Mechanical Specifications

### 1. Tooling: Stryker.NET
- **Choice**: [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction) is the industry standard for .NET mutation testing.
- **Support**: Native support for .NET 10 and C# 13 features.
- **Reporting**: Generates interactive HTML reports showing exactly which line mutation survived and why.

### 2. Stryker Configuration (`stryker-config.json`)
```json
{
  "stryker-config": {
    "project-info": {
      "name": "Mythril",
      "module": "Mythril.Data"
    },
    "mutate": [
      "**/!(*DTO|*Models|*ContentHost|*Loader).cs"
    ],
    "test-projects": [
      "../Mythril.Tests/Mythril.Tests.csproj"
    ],
    "reporters": [
      "html",
      "progress",
      "cleartext"
    ],
    "concurrency": 4,
    "thresholds": {
      "high": 80,
      "low": 60,
      "break": 50
    }
  }
}
```

### 3. Mutator Targeting Strategy
- **Arithmetic Mutators**: Swapping `+` for `-` in `JunctionManager.GetStatValue`.
- **Equality Mutators**: Swapping `==` for `!=` in `ResourceManager.CanAfford`.
- **Logical Mutators**: Swapping `&&` for `||` in `LatticeSimulator`.
- **Update Mutators**: Swapping `++` for `--` in `InventoryManager.Add`.

### 4. Integration with Health Checks
Update `scripts/check_health.py` to support a `--mutation` flag:
- **Command**: `dotnet stryker --project Mythril.Data.csproj`
- **Validation**: Parse the `mutation-report.html` or JSON output.
- **Failure Condition**: If `MutationScore < Threshold`, health check fails even if regular tests pass.

## 🧪 Phase-Specific Implementation Details

### Phase 2: Core Logic Focus
- **Priority**: Files like `JunctionManager.cs` and `ResourceManager_Quests.cs`.
- **Goal**: Kill all mutants related to the Junction formula:
  - `StatValue = 10 + (Qty * Modifier / 100)`
  - If a mutator changes `100` to `101` and tests pass, the test is insufficient.

### Phase 4: UI Logic Focus
- **bUnit Mutators**: Targeting C# logic inside `.razor` files or code-behind.
- **Example**: If `IsActive` is mutated to `!IsActive`, the bUnit test must fail to find the `.active-dot.pulse` element.

### Phase 5: Report Archiving
- Reports will be stored in `docs/mutation_reports/YYYY-MM-DD_v[hash]/`.
- This allows historical tracking of "Test suite hardening" over time.

## 🛡️ Stability & Performance
- **Concurrency**: Set `concurrency` to `CPU_COUNT - 1` to prevent system hang during massive mutation runs.
- **Baseline**: Regular `dotnet test` must pass before Stryker will even begin.
