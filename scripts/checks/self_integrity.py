"""
Self-referential integrity check: verifies the health check system itself
does not violate the monolith limit it enforces on the rest of the codebase.

This check prevents check_health.py and its sub-modules from silently growing
into unmaintainable monoliths that evade their own rules.
"""
from pathlib import Path

from .config import MAX_LINES_PER_FILE, record_failure

# The scripts/ directory is excluded from the main monolith check so it can't
# self-report. This module closes that loophole by explicitly auditing it.
_SCRIPTS_DIR = Path(__file__).parent.parent  # scripts/
_PYTHON_EXTENSIONS = {".py"}


def check_self_integrity() -> int:
    """
    Counts Python script files under scripts/ (including scripts/checks/)
    that exceed MAX_LINES_PER_FILE. Records a failure for each violation.

    Returns the number of violating files.
    """
    print("--- Checking Health Script Self-Integrity ---")
    violations = 0

    for path in _SCRIPTS_DIR.rglob("*.py"):
        # Skip compiled cache
        if "__pycache__" in path.parts:
            continue
        try:
            with open(path, encoding="utf-8") as f:
                line_count = sum(1 for _ in f)
            if line_count > MAX_LINES_PER_FILE:
                violations += 1
                record_failure(
                    "self_integrity",
                    f"Health script {path.relative_to(_SCRIPTS_DIR.parent)} "
                    f"exceeds {MAX_LINES_PER_FILE} lines ({line_count} lines). "
                    "Refactor into a sub-module under scripts/checks/.",
                    {"file": str(path), "lines": line_count, "limit": MAX_LINES_PER_FILE},
                )
        except Exception as e:
            print(f"Warning: Could not read {path}: {e}")

    if violations == 0:
        print(f"Self-integrity OK: all health scripts are under {MAX_LINES_PER_FILE} lines.")
    return violations
