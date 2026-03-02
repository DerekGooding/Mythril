import os
import sys
import xml.etree.ElementTree as ET
import re
import subprocess
import json
import shutil

# Load configuration
CONFIG_PATH = os.path.join(os.path.dirname(__file__), "config.json")
with open(CONFIG_PATH, "r") as f:
    config = json.load(f)

MAX_LINES_PER_FILE = config.get("MAX_LINES_PER_FILE", 250)
MIN_OVERALL_COVERAGE = config.get("MIN_OVERALL_COVERAGE", 70.0)
MIN_FILE_COVERAGE = config.get("MIN_FILE_COVERAGE", 50.0)
DOCS_STALENESS_THRESHOLD = config.get("DOCS_STALENESS_THRESHOLD", 8)
SOURCE_DIR = config.get("SOURCE_DIR", "src")
RESULTS_DIR = config.get("RESULTS_DIR", "TestResults")
SOURCE_EXTENSIONS = config.get("SOURCE_EXTENSIONS", [".cs"])
DOC_FILES = config.get("DOC_FILES", [])
FEEDBACK_DIR = config.get("FEEDBACK_DIR", "docs/feedback")
ERRORS_DIR = "docs/errors"
TEST_COMMAND = config.get("TEST_COMMAND", ["dotnet", "test"])
COVERAGE_REPORT_PATTERN = config.get("COVERAGE_REPORT_PATTERN", r"coverage\.cobertura\.xml")

def run_tests():
    print("--- Generating Fresh Test Results ---")
    if os.path.exists(RESULTS_DIR):
        try:
            shutil.rmtree(RESULTS_DIR)
        except Exception as e:
            print(f"[WARNING] Could not remove {RESULTS_DIR}: {e}")
    
    try:
        subprocess.check_call(TEST_COMMAND)
        print("[SUCCESS] Tests completed.")
        return True
    except subprocess.CalledProcessError:
        print("[ERROR] Tests failed during health check.")
        return False

def check_feedback_backlog():
    print("\n--- Checking User Feedback & Error Backlog ---")
    total_count = 0
    for d in [FEEDBACK_DIR, ERRORS_DIR]:
        if not os.path.exists(d):
            continue
        items = [f for f in os.listdir(d) if os.path.isfile(os.path.join(d, f)) and not f.startswith(".")]
        total_count += len(items)
        if len(items) > 0:
            print(f"[FAIL] {len(items)} unresolved items found in {d}!")
            for item in items:
                print(f"  - {item}")
    
    if total_count == 0:
        print("[SUCCESS] Feedback and error backlogs are empty.")
    return total_count

def get_local_changes_since_file(doc_file_path):
    if not os.path.exists(doc_file_path):
        return 999 
    
    doc_mtime = os.path.getmtime(doc_file_path)
    changed_count = 0
    
    EXCLUDE_DIRS = {"obj", "bin", ".git", "lib", "node_modules", "wwwroot/lib", "TestResults", ".gemini", ".vs", "output"}
    for root, dirs, files in os.walk(SOURCE_DIR):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        for file in files:
            if any(file.endswith(ext) for ext in SOURCE_EXTENSIONS):
                file_path = os.path.join(root, file)
                if os.path.abspath(file_path) == os.path.abspath(doc_file_path):
                    continue
                try:
                    if os.path.getmtime(file_path) > doc_mtime:
                        changed_count += 1
                except Exception:
                    continue
    return changed_count

def check_docs_staleness():
    print("\n--- Checking Documentation Staleness ---")
    all_up_to_date = True
    for doc in DOC_FILES:
        if os.path.exists(doc):
            changes = get_local_changes_since_file(doc)
            print(f"{doc}: {changes} source files changed since its last update.")
            if changes > DOCS_STALENESS_THRESHOLD:
                print(f"[FAIL] {doc} is stale!")
                all_up_to_date = False
        else:
            print(f"[WARNING] Documentation file {doc} not found.")
    return all_up_to_date

def check_monoliths():
    print(f"--- Checking for Monoliths (> {MAX_LINES_PER_FILE} lines) ---")
    monolith_count = 0
    EXCLUDE_DIRS = {"obj", "bin", ".git", "lib", "node_modules", "wwwroot/lib", "TestResults", "output"}
    for root, dirs, files in os.walk(SOURCE_DIR):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        if any(ex in root.replace("\\", "/") for ex in ["/obj/", "/bin/", "/.git/", "/lib/", "/node_modules/", "/wwwroot/lib/", "/TestResults/", "/output/"]):
            continue
        
        for file in files:
            if any(file.endswith(ext) for ext in SOURCE_EXTENSIONS):
                path = os.path.join(root, file)
                try:
                    with open(path, "r", encoding="utf-8") as f:
                        lines = len(f.readlines())
                        if lines > MAX_LINES_PER_FILE:
                            print(f"[FAIL] {path} has {lines} lines.")
                            monolith_count += 1
                except Exception as e:
                    print(f"[WARNING] Could not read {path}: {e}")
    return monolith_count

def parse_coverage():
    print("\n--- Checking Test Coverage ---")
    coverage_files = []
    search_dir = RESULTS_DIR if os.path.exists(RESULTS_DIR) else "."
    for root, _, files in os.walk(search_dir):
        for file in files:
            if re.match(COVERAGE_REPORT_PATTERN, file):
                path = os.path.join(root, file)
                coverage_files.append((path, os.path.getmtime(path)))
    
    if not coverage_files:
        print("[ERROR] Coverage report not found.")
        return 0.0, False

    coverage_files.sort(key=lambda x: x[1], reverse=True)
    coverage_file = coverage_files[0][0]
    print(f"Using coverage report: {coverage_file}")

    try:
        tree = ET.parse(coverage_file)
        root = tree.getroot()
        overall_line_rate = float(root.attrib["line-rate"]) * 100
        print(f"Overall Coverage: {overall_line_rate:.2f}%")
        return overall_line_rate, overall_line_rate >= MIN_OVERALL_COVERAGE
    except Exception as e:
        print(f"[ERROR] Could not parse coverage: {e}")
        return 0.0, False

def export_results(monolith_count, coverage, docs_pass, tests_pass, feedback_count):
    summary = {
        "is_healthy": (monolith_count == 0 and docs_pass and tests_pass and feedback_count == 0),
        "monoliths": monolith_count,
        "coverage": coverage,
        "docs_up_to_date": docs_pass,
        "tests_passing": tests_pass,
        "feedback_backlog": feedback_count
    }
    
    os.makedirs("scripts/data", exist_ok=True)
    with open("scripts/data/health_summary.json", "w") as f:
        json.dump(summary, f, indent=4)
    
    def save_shield(name, label, message, color):
        with open(f"scripts/data/shield_{name}.json", "w") as f:
            json.dump({
                "schemaVersion": 1,
                "label": label,
                "message": str(message),
                "color": color
            }, f, indent=4)

    save_shield("monoliths", "Monoliths", monolith_count, "brightgreen" if monolith_count == 0 else "red")
    save_shield("coverage", "Coverage", f"{coverage:.2f}%", "brightgreen" if coverage >= MIN_OVERALL_COVERAGE else "yellow")
    save_shield("docs", "Docs", "Up-to-date" if docs_pass else "Stale", "brightgreen" if docs_pass else "red")
    save_shield("tests", "Tests", "Passing" if tests_pass else "Failing", "brightgreen" if tests_pass else "red")
    save_shield("feedback", "Feedback", "Clear" if feedback_count == 0 else f"{feedback_count} Pending", "brightgreen" if feedback_count == 0 else "orange")

if __name__ == "__main__":
    skip_tests = "--skip-tests" in sys.argv
    tests_passed = True if skip_tests else run_tests()
    m_count = check_monoliths()
    cov_val, cov_pass = parse_coverage()
    d_pass = check_docs_staleness()
    f_count = check_feedback_backlog()
    all_pass = m_count == 0 and cov_pass and d_pass and tests_passed and f_count == 0
    export_results(m_count, cov_val, d_pass, tests_passed, f_count)
    if not all_pass:
        print("\n[FAIL] Health checks failed.")
        sys.exit(1)
    print("\n[SUCCESS] All health checks passed!")
    sys.exit(0)
