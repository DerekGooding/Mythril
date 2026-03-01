# Agentic Status Report
**Generated:** 2026-02-28 18:20:15

## 1. System Integrity (check_health.py)
❌ **FAILED**
- **Monoliths:** 0
- **Coverage:** 28.24%
- **Docs Stale:** False

## 2. Functional Verification (run_ai_test.ps1)
❌ **FAILED**
```
Headless test failed.
```

## 3. Raw Logs
<details>
<summary>Health Check Output</summary>

```
Test run for C:\Users\Derek\source\repos\Mythril\Mythril.Tests\bin\Debug\net9.0\Mythril.Tests.dll (.NETCoreApp,Version=v9.0)
VSTest version 18.0.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Failed!  - Failed:    19, Passed:     0, Skipped:     0, Total:    19, Duration: 231 ms - Mythril.Tests.dll (net9.0)

Attachments:
  C:\Users\Derek\source\repos\Mythril\TestResults\b91b7049-6f2c-4933-9c3e-e45fe9d2dc6b\coverage.cobertura.xml
--- Generating Fresh Test Results ---
[ERROR] Tests failed during health check.
--- Checking for Monoliths (> 250 lines) ---

--- Checking Test Coverage ---
Using coverage report: TestResults\b91b7049-6f2c-4933-9c3e-e45fe9d2dc6b\coverage.cobertura.xml
Overall Coverage: 28.24%

--- Checking Documentation Staleness ---
README.md: 4 source files changed.
GEMINI.md: 3 source files changed.
AGENTS.md: 4 source files changed.
docs/roadmap.md: 0 source files changed.
docs/suggestions.md: 0 source files changed.
Results and shields exported to scripts/data/

[FAIL] Health checks failed.

```
</details>

<details>
<summary>Headless Test Output</summary>

```
Headless test failed.

```
</details>
