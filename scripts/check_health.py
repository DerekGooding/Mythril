"""
check_health.py - Health check orchestrator for Mythril.

All individual checks live in scripts/checks/. This file is intentionally
kept thin so it stays within the project's own 250-line monolith limit.

Usage:
    python scripts/check_health.py                # standard run
    python scripts/check_health.py --skip-tests   # skip dotnet test
    python scripts/check_health.py --mutation     # include mutation testing
"""
import sys

from checks.config         import FAILURES
from checks.tests_runner   import run_tests, parse_trx_results
from checks.mutation       import check_mutation, check_mutation_staleness
from checks.monolith       import check_monoliths
from checks.coverage       import parse_coverage
from checks.razor          import check_razor_has_test, check_key_usage, check_data_testid
from checks.docs_staleness import check_docs_staleness
from checks.reachability   import check_reachability
from checks.content        import check_content_graph, check_visualization
from checks.feedback       import check_feedback
from checks.responsive     import check_responsive
from checks.self_integrity import check_self_integrity
from checks.reporting      import generate_shields, export_results


def main() -> None:
    skip_tests   = "--skip-tests" in sys.argv
    run_mutation = "--mutation"   in sys.argv

    # --- Tests ---
    test_passed = True
    if not skip_tests:
        test_passed = run_tests()
    test_counts = parse_trx_results()

    # --- Mutation ---
    mutation_score = 0.0
    if run_mutation:
        mutation_score = check_mutation()

    # --- Core checks ---
    content_graph_passed = check_content_graph()
    monolith_count       = check_monoliths()
    coverage_pct         = parse_coverage()
    missing_tests        = check_razor_has_test()
    key_violations       = check_key_usage()
    testid_violations    = check_data_testid()
    stale_docs           = check_docs_staleness()
    stale_mutation       = check_mutation_staleness()
    reachability_result  = check_reachability()
    visualization_passed = check_visualization()
    pending_feedback     = check_feedback()
    responsive_result    = check_responsive()

    # --- Self-referential: health scripts must obey their own rules ---
    self_integrity_violations = check_self_integrity()

    metrics = {
        "monoliths":          monolith_count,
        "coverage":           coverage_pct,
        "mutation_score":     mutation_score,
        "missing_tests":      missing_tests,
        "key_violations":     key_violations,
        "testid_violations":  testid_violations,
        "stale_docs":         stale_docs,
        "stale_mutation":     stale_mutation,
        "reachability_passed": reachability_result,
        "visualization_passed": visualization_passed,
        "pending_feedback":   pending_feedback,
        "responsive_passed":  responsive_result,
        "test_passed":        test_passed,
        "test_counts":        test_counts,
        "self_integrity":     self_integrity_violations,
    }

    export_results(metrics)
    generate_shields(metrics)

    if FAILURES:
        print(f"\n[FAIL] Health checks failed with {len(FAILURES)} issues.")
        sys.exit(1)

    print("\n[SUCCESS] All health checks passed.")
    sys.exit(0)


if __name__ == "__main__":
    main()
