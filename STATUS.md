# Agentic Status Report
**Generated:** 2026-03-01 17:26:35

## 1. System Integrity (check_health.py)
❌ **FAILED**
- **Monoliths:** 0
- **Coverage:** 90.78%
- **Docs Stale:** False

## 2. Functional Verification (run_ai_test.ps1)
✅ **PASSED**
```
    {
      "Key": "Mana Leaf",
      "Value": 1
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
Test run for C:\Users\Derek\source\repos\Mythril\Mythril.Tests\bin\Debug\net10.0\Mythril.Tests.dll (.NETCoreApp,Version=v10.0)
VSTest version 18.0.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    56, Skipped:     0, Total:    56, Duration: 666 ms - Mythril.Tests.dll (net10.0)

Attachments:
  C:\Users\Derek\source\repos\Mythril\TestResults\2c3d114b-531b-45dc-96a0-8af162e50424\coverage.cobertura.xml
--- Generating Fresh Test Results ---
[SUCCESS] Tests completed.
--- Checking for Monoliths (> 250 lines) ---

--- Checking Test Coverage ---
Using coverage report: TestResults\2c3d114b-531b-45dc-96a0-8af162e50424\coverage.cobertura.xml
Overall Coverage: 90.78%

--- Checking Documentation Staleness ---
README.md: 0 source files changed since its last update.
GEMINI.md: 0 source files changed since its last update.
AGENTS.md: 0 source files changed since its last update.

--- Checking User Feedback & Error Backlog ---
[FAIL] 4 unresolved items found in docs/feedback!
  - 2026-03-01_game_too_easy.md
  - 2026-03-01_toggle_theme.md
  - 2026-03-01_tutorial_section.md
  - 2026-03-01_window_scrolling.md
[FAIL] 2 unresolved items found in docs/errors!
  - 2026-03-01_automated_error_report.md
  - 2026-03-01_automated_error_report_1.md

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
    },
    {
      "Key": "Mana Leaf",
      "Value": 1
    }
  ],
  "UnlockedCadences": [
    "Mythril Weaver"
  ]
}

```
</details>
