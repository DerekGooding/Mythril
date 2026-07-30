"""
Monolith check: enforces max lines-per-file limit across source files.
"""
from pathlib import Path

from .config import (
    SOURCE_DIR, SOURCE_EXTENSIONS,
    MAX_LINES_PER_FILE, IGNORED_FILES,
    record_failure,
)


def check_monoliths() -> int:
    print("--- Checking Monoliths ---")
    count = 0
    exclude_dirs = {"bin", "obj", "node_modules", "lib", "scripts", "output"}

    for path in SOURCE_DIR.rglob("*"):
        if any(part in path.parts for part in exclude_dirs):
            continue
        if any(ignored in path.name for ignored in IGNORED_FILES):
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
                        {"lines": lines},
                    )
            except Exception as e:
                print(f"Error reading {path}: {e}")
    return count
