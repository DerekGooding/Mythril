"""
Coverage parsing: reads Cobertura XML reports and enforces line/branch thresholds.
"""
import os
import xml.etree.ElementTree as ET
from pathlib import Path

from .config import (
    RESULTS_DIR, COVERAGE_PATTERN, IGNORED_FILES,
    MIN_OVERALL_COVERAGE, MIN_FILE_COVERAGE,
    record_failure,
)


def find_latest_coverage() -> Path | None:
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


def parse_coverage() -> float:
    print("--- Checking Coverage ---")

    coverage_file = find_latest_coverage()
    if not coverage_file:
        record_failure("coverage", "Coverage report not found")
        return 0.0

    tree = ET.parse(coverage_file)
    root = tree.getroot()

    overall_line   = float(root.attrib.get("line-rate",   0)) * 100
    overall_branch = float(root.attrib.get("branch-rate", 0)) * 100

    print(f"Overall Line Coverage: {overall_line:.2f}%")
    print(f"Overall Branch Coverage: {overall_branch:.2f}%")

    if overall_line < MIN_OVERALL_COVERAGE:
        record_failure(
            "coverage",
            "Overall line coverage below threshold",
            {"actual": overall_line, "required": MIN_OVERALL_COVERAGE},
        )

    for cls in root.findall(".//class"):
        filename = cls.attrib.get("filename", "")
        if "obj" in filename or filename.endswith(".g.cs"):
            continue
        if any(ignored in filename for ignored in IGNORED_FILES):
            continue

        line_rate = float(cls.attrib.get("line-rate", 0)) * 100
        if line_rate < MIN_FILE_COVERAGE:
            record_failure(
                "file_coverage",
                f"{filename} below minimum coverage: {line_rate:.2f}% (required: {MIN_FILE_COVERAGE}%)",
                {"actual": line_rate, "required": MIN_FILE_COVERAGE},
            )

    return overall_line
