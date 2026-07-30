"""
Shield generation and results export for the health check dashboard.
"""
import os
import json
from pathlib import Path

from .config import MIN_OVERALL_COVERAGE, FAILURES


def write_shield(name: str, label: str, message: str, color: str) -> None:
    os.makedirs("scripts/data", exist_ok=True)
    shield = {"schemaVersion": 1, "label": label, "message": message, "color": color}
    with open(f"scripts/data/shield_{name}.json", "w") as f:
        json.dump(shield, f, indent=4)


def generate_shields(metrics: dict) -> None:
    test_counts = metrics.get("test_counts", {})
    total   = test_counts.get("total",  0)
    passed  = test_counts.get("passed", 0)
    failed  = test_counts.get("failed", 0)
    test_ok = metrics.get("test_passed", False)

    if total > 0 and failed == 0 and test_ok:
        write_shield("tests", "tests", f"{passed}/{total} passed", "brightgreen")
    elif total > 0:
        write_shield("tests", "tests", f"{failed}/{total} failed", "red")
    else:
        write_shield("tests", "tests", "passed" if test_ok else "failed",
                     "brightgreen" if test_ok else "red")

    cov = metrics.get("coverage", 0.0)
    cov_color = ("brightgreen" if cov >= 90
                 else "green" if cov >= MIN_OVERALL_COVERAGE
                 else "orange" if cov >= 50
                 else "red")
    write_shield("coverage", "coverage", f"{cov:.1f}%", cov_color)

    monos = metrics.get("monoliths", 0)
    write_shield("monoliths", "monoliths", str(monos),
                 "brightgreen" if monos == 0 else "orange" if monos < 3 else "red")

    stale = metrics.get("stale_docs", 0)
    write_shield("docs", "docs", "stale" if stale > 0 else "up-to-date",
                 "orange" if stale > 0 else "brightgreen")

    missing  = metrics.get("missing_tests",       0)
    key_v    = metrics.get("key_violations",       0)
    testid_v = metrics.get("testid_violations",    0)
    ui_ok    = missing == 0 and key_v == 0 and testid_v == 0
    write_shield("ui", "UI integrity", "passed" if ui_ok else "failed",
                 "brightgreen" if ui_ok else "red")

    sim        = metrics.get("reachability_passed", {})
    sim_ok     = sim.get("passed", False)
    completed  = sim.get("completed", 0)
    total_q    = sim.get("total",     0)
    sim_msg    = (f"{completed}/{total_q} quests" if total_q > 0
                  else ("passed" if sim_ok else "failed"))
    write_shield("simulation", "reachability", sim_msg, "brightgreen" if sim_ok else "red")

    game_time = sim.get("time", "N/A")
    write_shield("game_time", "optimal completion", game_time, "blue")

    sust  = sim.get("sustainable",   0)
    unsust = sim.get("unsustainable", 0)
    total_act = sust + unsust
    if total_act > 0:
        pct = (sust / total_act) * 100
        s_color = ("brightgreen" if pct == 100
                   else "green" if pct >= 80
                   else "orange" if pct >= 50
                   else "red")
        write_shield("sustainability", "economy", f"{pct:.0f}% sustainable", s_color)
    else:
        write_shield("sustainability", "economy", "N/A", "inactive")

    # Self-integrity shield
    si_violations = metrics.get("self_integrity", 0)
    write_shield("self_integrity", "script health",
                 "ok" if si_violations == 0 else f"{si_violations} violations",
                 "brightgreen" if si_violations == 0 else "red")


def export_results(metrics: dict) -> None:
    summary = {
        "is_healthy":     len(FAILURES) == 0,
        "failure_count":  len(FAILURES),
        "metrics":        metrics,
        "failures":       FAILURES,
    }
    os.makedirs("scripts/data", exist_ok=True)
    with open("scripts/data/health_summary.json", "w") as f:
        json.dump(summary, f, indent=4)
