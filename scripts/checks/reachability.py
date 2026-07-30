"""
Reachability simulation: runs Mythril.Headless and parses the simulation report.
"""
import re
import json
import subprocess
from pathlib import Path

from .config import record_failure


def _format_game_time(minutes_str: str) -> str:
    try:
        minutes = float(minutes_str.replace("m", ""))
        total_seconds = minutes * 60
        if minutes >= 50 * 60:
            return f"{minutes / (60 * 24):.1f}d"
        elif minutes >= 120:
            return f"{minutes / 60:.1f}h"
        elif total_seconds >= 300:
            return f"{minutes:.1f}m"
        else:
            return f"{total_seconds:.0f}s"
    except Exception:
        return minutes_str


def check_reachability() -> dict:
    print("--- Running Reachability Simulation ---")
    try:
        subprocess.check_call(["dotnet", "run", "--project", "Mythril.Headless", "--", "--run-sim"])

        report_path = Path("simulation_report.md")
        game_time = "Unknown"
        raw_minutes = 0.0
        sustainable_count = 0
        unsustainable_count = 0
        reachable_count = 0
        total_count = 0

        if report_path.exists():
            content = report_path.read_text(encoding="utf-8")

            routed_match  = re.search(r"Routed Completion Time: ([\d.]+)m",   content)
            lattice_match = re.search(r"Estimated End-Game Time: ([\d.]+)m", content)

            raw_time = "0"
            if routed_match:
                raw_time = routed_match.group(1)
                raw_minutes = float(raw_time)
            elif lattice_match:
                raw_time = lattice_match.group(1)
                raw_minutes = float(raw_time)

            game_time = _format_game_time(raw_time)

            sust_match = re.search(
                r"### Sustainable Recurring Activities\n(.*?)\n\n", content, re.DOTALL
            )
            if sust_match:
                sustainable_count = len(
                    [l for l in sust_match.group(1).split("\n") if l.strip().startswith("-")]
                )

            unsust_match = re.search(
                r"### ⚠️ Unsustainable Activities.*?\n(.*?)\n\n", content, re.DOTALL
            )
            if unsust_match:
                unsustainable_count = len(
                    [l for l in unsust_match.group(1).split("\n") if l.strip().startswith("-")]
                )

            unreachable_match = re.search(
                r"### Unreachable Quests\n(.*?)\n\n", content, re.DOTALL
            )
            unreachable_count = 0
            if unreachable_match:
                unreachable_count = len(
                    [l for l in unreachable_match.group(1).split("\n") if l.strip().startswith("-")]
                )

            quest_total_match = re.search(r"Total Quests Completed: (\d+)", content)
            if quest_total_match:
                reachable_count = int(quest_total_match.group(1))
                total_count = reachable_count + unreachable_count

        # Pacing regression check
        baseline_path = Path("docs/pacing_baseline.json")
        if baseline_path.exists():
            with open(baseline_path, "r") as f:
                baseline = json.load(f)

            base_time = baseline.get("routed_completion_time_minutes", 0)
            if base_time > 0:
                threshold = base_time * 1.15
                if raw_minutes > threshold:
                    record_failure(
                        "pacing",
                        f"Pacing regression detected: {raw_minutes:.1f}m "
                        f"(baseline: {base_time:.1f}m, max: {threshold:.1f}m)",
                        {"actual": raw_minutes, "baseline": base_time},
                    )

            base_reachable = baseline.get("reachable_quests", 0)
            if reachable_count < base_reachable:
                record_failure(
                    "reachability",
                    f"Content regression: Reachable quests dropped from {base_reachable} to {reachable_count}",
                    {"actual": reachable_count, "baseline": base_reachable},
                )

        return {
            "passed":       True,
            "time":         game_time,
            "sustainable":  sustainable_count,
            "unsustainable": unsustainable_count,
            "completed":    reachable_count,
            "total":        total_count,
        }
    except subprocess.CalledProcessError:
        record_failure("reachability", "Simulation failed: One or more quests are mathematically unreachable.")
        return {"passed": False, "time": "N/A"}
