"""
Mutation testing: score check and staleness check.
"""
import re
import sys
import subprocess
from pathlib import Path

from .config import MIN_MUTATION_SCORE, MUTATION_STALENESS_THRESHOLD, record_failure


def check_mutation() -> float:
    print("--- Running Mutation Testing ---")
    try:
        result = subprocess.run(
            [sys.executable, "scripts/run_mutation.py"],
            capture_output=True, text=True
        )
        output = result.stdout
        score_match = re.search(r"Mutation Score: ([\d.]+)%", output)
        if score_match:
            score = float(score_match.group(1))
            print(f"Mutation Score: {score:.2f}%")
            if score < MIN_MUTATION_SCORE:
                record_failure(
                    "mutation",
                    f"Mutation score below threshold: {score:.2f}% (required: {MIN_MUTATION_SCORE}%)",
                    {"actual": score, "required": MIN_MUTATION_SCORE},
                )
            return score
        else:
            record_failure("mutation", "Could not parse mutation score from output")
            return 0.0
    except Exception as e:
        record_failure("mutation", f"Error running mutation script: {e}")
        return 0.0


def check_mutation_staleness() -> int:
    print("--- Checking Mutation Staleness ---")
    reports_dir = Path("docs/mutation_reports")
    if not reports_dir.exists():
        return 0

    report_dirs = [d for d in reports_dir.iterdir() if d.is_dir()]
    if not report_dirs:
        return 0

    latest_report = max(report_dirs, key=lambda p: p.stat().st_mtime)

    try:
        subprocess.check_call(
            ["git", "rev-parse", "--is-inside-work-tree"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
        )
        last_commit = subprocess.check_output(
            ["git", "log", "-1", "--format=%ct", str(latest_report)], text=True
        ).strip()
        if not last_commit:
            return 0

        changed_files = subprocess.check_output(
            ["git", "diff", "--name-only", f"HEAD@{{{last_commit}}}"], text=True
        ).splitlines()
        test_changes = [
            f for f in changed_files
            if ("Mythril.Tests" in f or "Tests.cs" in f) and f.endswith(".cs")
        ]

        if len(test_changes) >= MUTATION_STALENESS_THRESHOLD:
            record_failure(
                "mutation_stale",
                f"Mutation test report is stale ({len(test_changes)} test files modified since last "
                f"mutation run). Run 'python scripts/archive_mutation.py' to update.",
                {"stale_test_files": len(test_changes), "threshold": MUTATION_STALENESS_THRESHOLD},
            )
            return len(test_changes)
    except Exception as e:
        print(f"Notice: Could not check mutation staleness: {e}")

    return 0
