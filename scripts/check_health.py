import os
import sys
import re
import json
import shutil
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path
from datetime import datetime

CONFIG_PATH = Path(__file__).parent / "config.json"

if CONFIG_PATH.exists():
    with open(CONFIG_PATH, "r") as f:
        config = json.load(f)
else:
    config = {}

MAX_LINES_PER_FILE = config.get("MAX_LINES_PER_FILE", 250)
MIN_OVERALL_COVERAGE = config.get("MIN_OVERALL_COVERAGE", 70.0)
MIN_FILE_COVERAGE = config.get("MIN_FILE_COVERAGE", 25.0)
MIN_BRANCH_COVERAGE = config.get("MIN_BRANCH_COVERAGE", 50.0)
DOCS_STALENESS_THRESHOLD = config.get("DOCS_STALENESS_THRESHOLD", 8)

SOURCE_DIR = Path(config.get("SOURCE_DIR", "."))
RESULTS_DIR = Path(config.get("RESULTS_DIR", "TestResults"))
SOURCE_EXTENSIONS = config.get("SOURCE_EXTENSIONS", [".cs", ".py", ".js", ".ts", ".razor"])
DOC_FILES = config.get("DOC_FILES", ["README.md", "GEMINI.md", "AGENTS.md"])
FEEDBACK_DIR = Path(config.get("FEEDBACK_DIR", "docs/feedback"))
ERRORS_DIR = Path(config.get("ERRORS_DIR", "docs/errors"))
TEST_COMMAND = config.get("TEST_COMMAND", ["dotnet", "test"])
COVERAGE_PATTERN = re.compile(config.get("COVERAGE_REPORT_PATTERN", r"coverage\.cobertura\.xml"))

FAILURES = []

# -----------------------
# Utilities
# -----------------------

def record_failure(category, message, metadata=None):
    FAILURES.append({
        "category": category,
        "message": message,
        "metadata": metadata or {}
    })
    print(f"[FAIL] {category}: {message}")

def run_tests():
    print("--- Running Tests ---")

    if RESULTS_DIR.exists():
        shutil.rmtree(RESULTS_DIR, ignore_errors=True)

    try:
        subprocess.check_call(TEST_COMMAND)
        return True
    except subprocess.CalledProcessError:
        record_failure("tests", "dotnet test failed")
        return False

# -----------------------
# Monolith Check
# -----------------------

def check_monoliths():
    print("--- Checking Monoliths ---")
    count = 0
    exclude_dirs = {"bin", "obj", "node_modules", "lib", "scripts"}
    for path in SOURCE_DIR.rglob("*"):
        if any(part in path.parts for part in exclude_dirs):
            continue
        if path.suffix in SOURCE_EXTENSIONS and path.is_file():
            try:
                with open(path, encoding="utf-8") as f:
                    lines = sum(1 for _ in f)
                if lines > MAX_LINES_PER_FILE:
                    count += 1
                    record_failure(
                        "monolith",
                        f"{path} exceeds {MAX_LINES_PER_FILE} lines",
                        {"lines": lines}
                    )
            except Exception as e:
                print(f"Error reading {path}: {e}")
    return count

# -----------------------
# Coverage Parsing
# -----------------------

def find_latest_coverage():
    candidates = []
    search_root = RESULTS_DIR if RESULTS_DIR.exists() else Path(".")
    for root, _, files in os.walk(search_root):
        for file in files:
            if COVERAGE_PATTERN.match(file):
                full = Path(root) / file
                candidates.append((full, full.stat().st_mtime))

    if not candidates:
        return None

    candidates.sort(key=lambda x: x[1], reverse=True)
    return candidates[0][0]

def parse_coverage():
    print("--- Checking Coverage ---")

    coverage_file = find_latest_coverage()
    if not coverage_file:
        record_failure("coverage", "Coverage report not found")
        return 0.0

    tree = ET.parse(coverage_file)
    root = tree.getroot()

    overall_line = float(root.attrib.get("line-rate", 0)) * 100
    overall_branch = float(root.attrib.get("branch-rate", 0)) * 100

    print(f"Overall Line Coverage: {overall_line:.2f}%")
    print(f"Overall Branch Coverage: {overall_branch:.2f}%")

    if overall_line < MIN_OVERALL_COVERAGE:
        record_failure(
            "coverage",
            "Overall line coverage below threshold",
            {"actual": overall_line, "required": MIN_OVERALL_COVERAGE}
        )

    # Per-file enforcement
    for cls in root.findall(".//class"):
        filename = cls.attrib.get("filename")
        if "obj" in filename or filename.endswith(".g.cs"):
            continue
        
        if filename in ["Models.cs", "Cadences.cs"]:
            continue
            
        line_rate = float(cls.attrib.get("line-rate", 0)) * 100

        if line_rate < MIN_FILE_COVERAGE:
            record_failure(
                "file_coverage",
                f"{filename} below minimum coverage: {line_rate:.2f}% (required: {MIN_FILE_COVERAGE}%)",
                {"actual": line_rate, "required": MIN_FILE_COVERAGE}
            )
    
    return overall_line

# -----------------------
# Razor Enforcement
# -----------------------

def razor_files():
    return list(SOURCE_DIR.rglob("*.razor"))

def check_razor_has_test():
    print("--- Checking Razor Test Presence ---")
    missing_count = 0
    test_files = list(Path(".").rglob("*.cs"))

    for razor in razor_files():
        component_name = razor.stem
        if component_name == "_Imports":
            continue
        found = False
        for test in test_files:
            try:
                content = test.read_text(encoding="utf-8")
                if f"RenderComponent<{component_name}>" in content:
                    found = True
                    break
            except:
                continue

        if not found:
            missing_count += 1
            record_failure(
                "razor_test",
                f"No bUnit test found for {component_name}"
            )
    return missing_count

def check_key_usage():
    print("--- Checking @key Usage ---")
    loop_pattern = re.compile(r"@foreach\s*\(")
    count = 0
    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if loop_pattern.search(content):
            if "@key" not in content:
                count += 1
                record_failure(
                    "razor_key",
                    f"@foreach without @key in {razor}"
                )
    return count

def check_data_testid():
    print("--- Checking data-testid Usage ---")
    interactive_pattern = re.compile(r"<(button|input|form|select)")
    count = 0
    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if interactive_pattern.search(content):
            if "data-testid" not in content:
                count += 1
                record_failure(
                    "razor_testid",
                    f"Interactive elements missing data-testid in {razor}"
                )
    return count

# -----------------------
# Documentation Staleness
# -----------------------

def check_docs_staleness():
    print("--- Checking Docs Staleness ---")
    stale_count = 0
    
    # Get the latest commit time for any source file
    try:
        # Check if we're in a git repo
        subprocess.check_call(["git", "rev-parse", "--is-inside-work-tree"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except:
        print("Not a git repository, skipping staleness check.")
        return 0

    for doc in DOC_FILES:
        doc_path = Path(doc)
        if not doc_path.exists():
            continue
            
        try:
            # Files changed since doc was last updated
            # Use git log to find the last commit that touched the doc
            last_doc_commit = subprocess.check_output(["git", "log", "-1", "--format=%ct", str(doc)], text=True).strip()
            if not last_doc_commit:
                continue
                
            # Count commits since then that touched source files
            # We'll simplify: count files changed in commits since last_doc_commit
            changed_files = subprocess.check_output(["git", "diff", "--name-only", f"HEAD@{{{last_doc_commit}}}"], text=True).splitlines()
            source_changes = [f for f in changed_files if Path(f).suffix in SOURCE_EXTENSIONS]
            
            if len(source_changes) > DOCS_STALENESS_THRESHOLD:
                stale_count += 1
                record_failure("docs_stale", f"{doc} is stale ({len(source_changes)} source changes since last update)")
        except Exception as e:
            print(f"Error checking staleness for {doc}: {e}")
            
    return stale_count

# -----------------------
# Feedback Check
# -----------------------

def check_feedback():
    print("--- Checking Feedback ---")
    pending = 0
    for d in [FEEDBACK_DIR, ERRORS_DIR]:
        if d.exists():
            files = [f for f in d.iterdir() if f.is_file() and not f.name.startswith(".")]
            pending += len(files)
    
    if pending > 0:
        # We don't record a failure here because feedback isn't a "hard" health requirement
        # that should block CI, but we track the metric for the shield.
        print(f"Pending feedback items: {pending}")
    return pending

# -----------------------
# Shield Generation
# -----------------------

def write_shield(name, label, message, color):
    os.makedirs("scripts/data", exist_ok=True)
    shield = {
        "schemaVersion": 1,
        "label": label,
        "message": message,
        "color": color
    }
    with open(f"scripts/data/shield_{name}.json", "w") as f:
        json.dump(shield, f, indent=4)

def generate_shields(metrics):
    # Tests Shield
    test_fail_count = len([f for f in FAILURES if f['category'] == 'tests'])
    if test_fail_count == 0:
        write_shield("tests", "tests", "passed", "brightgreen")
    else:
        write_shield("tests", "tests", "failed", "red")
        
    # Coverage Shield
    cov = metrics.get('coverage', 0.0)
    color = "brightgreen" if cov >= 90 else "green" if cov >= MIN_OVERALL_COVERAGE else "orange" if cov >= 50 else "red"
    write_shield("coverage", "coverage", f"{cov:.1f}%", color)
    
    # Monoliths Shield
    monos = metrics.get('monoliths', 0)
    color = "brightgreen" if monos == 0 else "orange" if monos < 3 else "red"
    write_shield("monoliths", "monoliths", str(monos), color)
    
    # Docs Shield
    stale = metrics.get('stale_docs', 0)
    color = "brightgreen" if stale == 0 else "orange"
    write_shield("docs", "docs", "stale" if stale > 0 else "up-to-date", color)

    # Feedback Shield
    pending = metrics.get('pending_feedback', 0)
    color = "brightgreen" if pending == 0 else "orange"
    write_shield("feedback", "feedback", "clear" if pending == 0 else str(pending), color)

# -----------------------
# Export Results
# -----------------------

def export_results(metrics):
    summary = {
        "is_healthy": len(FAILURES) == 0,
        "failure_count": len(FAILURES),
        "metrics": metrics,
        "failures": FAILURES
    }

    os.makedirs("scripts/data", exist_ok=True)
    with open("scripts/data/health_summary.json", "w") as f:
        json.dump(summary, f, indent=4)

# -----------------------
# Entry
# -----------------------

if __name__ == "__main__":
    skip_tests = "--skip-tests" in sys.argv

    test_passed = True
    if not skip_tests:
        test_passed = run_tests()

    monolith_count = check_monoliths()
    coverage_pct = parse_coverage()
    missing_tests = check_razor_has_test()
    key_violations = check_key_usage()
    testid_violations = check_data_testid()
    stale_docs = check_docs_staleness()
    pending_feedback = check_feedback()

    metrics = {
        "monoliths": monolith_count,
        "coverage": coverage_pct,
        "missing_tests": missing_tests,
        "key_violations": key_violations,
        "testid_violations": testid_violations,
        "stale_docs": stale_docs,
        "pending_feedback": pending_feedback,
        "test_passed": test_passed
    }

    export_results(metrics)
    generate_shields(metrics)

    if FAILURES:
        print(f"\n[FAIL] Health checks failed with {len(FAILURES)} issues.")
        sys.exit(1)

    print("\n[SUCCESS] All health checks passed.")
    sys.exit(0)
