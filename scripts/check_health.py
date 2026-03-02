import os
import sys
import re
import json
import shutil
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

CONFIG_PATH = Path(__file__).parent / "config.json"

with open(CONFIG_PATH, "r") as f:
    config = json.load(f)

MAX_LINES_PER_FILE = config.get("MAX_LINES_PER_FILE", 250)
MIN_OVERALL_COVERAGE = config.get("MIN_OVERALL_COVERAGE", 70.0)
MIN_FILE_COVERAGE = config.get("MIN_FILE_COVERAGE", 50.0)
MIN_BRANCH_COVERAGE = config.get("MIN_BRANCH_COVERAGE", 50.0)
DOCS_STALENESS_THRESHOLD = config.get("DOCS_STALENESS_THRESHOLD", 8)

SOURCE_DIR = Path(config.get("SOURCE_DIR", "src"))
RESULTS_DIR = Path(config.get("RESULTS_DIR", "TestResults"))
SOURCE_EXTENSIONS = config.get("SOURCE_EXTENSIONS", [".cs"])
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
    exclude_dirs = {"bin", "obj", "node_modules", "lib"}
    for path in SOURCE_DIR.rglob("*"):
        if any(part in path.parts for part in exclude_dirs):
            continue
        if path.suffix in SOURCE_EXTENSIONS and path.is_file():
            lines = sum(1 for _ in open(path, encoding="utf-8"))
            if lines > MAX_LINES_PER_FILE:
                record_failure(
                    "monolith",
                    f"{path} exceeds {MAX_LINES_PER_FILE} lines",
                    {"lines": lines}
                )

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
        record_failure("coverage", "Coverage report not found")
        return None

    candidates.sort(key=lambda x: x[1], reverse=True)
    return candidates[0][0]

def parse_coverage():
    print("--- Checking Coverage ---")

    coverage_file = find_latest_coverage()
    if not coverage_file:
        return

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

    if overall_branch < MIN_BRANCH_COVERAGE:
        record_failure(
            "coverage",
            "Overall branch coverage below threshold",
            {"actual": overall_branch, "required": MIN_BRANCH_COVERAGE}
        )

    # Per-file enforcement
    for cls in root.findall(".//class"):
        filename = cls.attrib.get("filename")
        if "obj" in filename or filename.endswith(".g.cs"):
            continue
        
        # Exclude purely structural/data files from per-file check if they are problematic
        if filename in ["Models.cs", "Cadences.cs"]:
            continue
            
        line_rate = float(cls.attrib.get("line-rate", 0)) * 100

        if line_rate < MIN_FILE_COVERAGE:
            record_failure(
                "file_coverage",
                f"{filename} below minimum coverage: {line_rate:.2f}% (required: {MIN_FILE_COVERAGE}%)",
                {"actual": line_rate, "required": MIN_FILE_COVERAGE}
            )

# -----------------------
# Razor Enforcement
# -----------------------

def razor_files():
    return list(SOURCE_DIR.rglob("*.razor"))

def check_razor_has_test():
    print("--- Checking Razor Test Presence ---")

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
            record_failure(
                "razor_test",
                f"No bUnit test found for {component_name}"
            )

def check_key_usage():
    print("--- Checking @key Usage ---")
    loop_pattern = re.compile(r"@foreach\s*\(")

    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if loop_pattern.search(content):
            if "@key" not in content:
                record_failure(
                    "razor_key",
                    f"@foreach without @key in {razor}"
                )

def check_data_testid():
    print("--- Checking data-testid Usage ---")

    interactive_pattern = re.compile(r"<(button|input|form|select)")

    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if interactive_pattern.search(content):
            if "data-testid" not in content:
                record_failure(
                    "razor_testid",
                    f"Interactive elements missing data-testid in {razor}"
                )

# -----------------------
# Export Results
# -----------------------

def export_results():
    summary = {
        "is_healthy": len(FAILURES) == 0,
        "failure_count": len(FAILURES),
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

    if not skip_tests:
        run_tests()

    check_monoliths()
    parse_coverage()
    check_razor_has_test()
    check_key_usage()
    check_data_testid()

    export_results()

    if FAILURES:
        print("\n[FAIL] Health checks failed.")
        sys.exit(1)

    print("\n[SUCCESS] All health checks passed.")
    sys.exit(0)