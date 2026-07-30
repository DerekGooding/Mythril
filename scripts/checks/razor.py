"""
Razor enforcement: bUnit test presence, @key usage, and data-testid coverage.
"""
import re
from pathlib import Path

from .config import SOURCE_DIR, record_failure


def razor_files() -> list[Path]:
    return list(SOURCE_DIR.rglob("*.razor"))


def check_razor_has_test() -> int:
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
            except Exception:
                continue

        if not found:
            missing_count += 1
            record_failure("razor_test", f"No bUnit test found for {component_name}")
    return missing_count


def check_key_usage() -> int:
    print("--- Checking @key Usage ---")
    loop_pattern = re.compile(r"@foreach\s*\(")
    count = 0
    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if loop_pattern.search(content) and "@key" not in content:
            count += 1
            record_failure("razor_key", f"@foreach without @key in {razor}")
    return count


def check_data_testid() -> int:
    print("--- Checking data-testid Usage ---")
    interactive_pattern = re.compile(r"<(button|input|form|select)")
    count = 0
    for razor in razor_files():
        content = razor.read_text(encoding="utf-8")
        if interactive_pattern.search(content) and "data-testid" not in content:
            count += 1
            record_failure("razor_testid", f"Interactive elements missing data-testid in {razor}")
    return count
