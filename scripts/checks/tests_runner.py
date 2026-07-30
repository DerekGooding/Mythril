"""
Tests module: runs dotnet test and parses TRX results.
"""
import subprocess
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path

from .config import TEST_COMMAND, RESULTS_DIR, record_failure


def run_tests() -> bool:
    print("--- Running Tests ---")

    if RESULTS_DIR.exists():
        shutil.rmtree(RESULTS_DIR, ignore_errors=True)

    cmd = list(TEST_COMMAND)
    if not any("trx" in arg for arg in cmd):
        cmd.extend(["--logger", "trx;LogFileName=test_results.trx"])

    try:
        subprocess.check_call(cmd)
        return True
    except subprocess.CalledProcessError:
        record_failure("tests", "dotnet test failed")
        return False


def parse_trx_results() -> dict:
    total, passed, failed = 0, 0, 0
    for trx_path in Path(".").rglob("*.trx"):
        try:
            tree = ET.parse(trx_path)
            root = tree.getroot()
            counters = root.find(".//{*}Counters")
            if counters is not None:
                total  = int(counters.attrib.get("total",  0))
                passed = int(counters.attrib.get("passed", 0))
                failed = int(counters.attrib.get("failed", 0))
                return {"total": total, "passed": passed, "failed": failed}
        except Exception as e:
            print(f"Error parsing TRX file {trx_path}: {e}")
    return {"total": total, "passed": passed, "failed": failed}
