# Agentic Status Report
**Generated:** 2026-03-01 11:18:11

## 1. System Integrity (check_health.py)
❌ **FAILED**
- **Monoliths:** 0
- **Coverage:** 70.45%
- **Docs Stale:** True

## 2. Functional Verification (run_ai_test.ps1)
✅ **PASSED**
```
    {
      "Key": "Moonberry",
      "Value": 2
    }
  ],
  "UnlockedCadences": [
    "Mythril Weaver"
  ]
}
```

## 3. Raw Logs
<details>
<summary>Health Check Output</summary>

```
Test run for C:\Users\Derek\source\repos\Mythril\Mythril.Tests\bin\Debug\net9.0\Mythril.Tests.dll (.NETCoreApp,Version=v9.0)
VSTest version 18.0.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    41, Skipped:     0, Total:    41, Duration: 296 ms - Mythril.Tests.dll (net9.0)

Attachments:
  C:\Users\Derek\source\repos\Mythril\TestResults\a25c9762-331b-4315-ab63-c670f2fb0382\coverage.cobertura.xml
--- Generating Fresh Test Results ---
[SUCCESS] Tests completed.
--- Checking for Monoliths (> 250 lines) ---

--- Checking Test Coverage ---
Using coverage report: TestResults\a25c9762-331b-4315-ab63-c670f2fb0382\coverage.cobertura.xml
Overall Coverage: 70.45%

--- Checking Documentation Staleness ---
README.md: 0 source files changed.
GEMINI.md: 17 source files changed.
[FAIL] GEMINI.md is stale!
AGENTS.md: 11 source files changed.
[FAIL] AGENTS.md is stale!

--- Checking User Feedback Backlog ---
[SUCCESS] Feedback backlog is empty.
Results and shields exported to scripts/data/

[FAIL] Health checks failed.

```
</details>

<details>
<summary>Headless Test Output</summary>

```
ResourceManager initializing...
Initializing Cadences...
Initializing Locations...
ResourceManager initialized.
Executing: add_item Ancient Bark
Executing: complete_quest Gather Moonberries
Executing: unlock_cadence Mythril Weaver
Final state saved to state.json
Assertion: InventoryCount Ancient Bark expected 10, got 10 - PASS
Assertion: CadenceUnlocked Mythril Weaver expected True, got True - PASS
Headless test completed successfully.
{
  "Inventory": [
    {
      "Key": "Gold",
      "Value": 100
    },
    {
      "Key": "Ancient Bark",
      "Value": 10
    },
    {
      "Key": "Moonberry",
      "Value": 2
    }
  ],
  "UnlockedCadences": [
    "Mythril Weaver"
  ]
}

```
</details>
