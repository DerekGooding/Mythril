"""
Docs staleness check: counts source file changes since last doc update via git.
"""
import subprocess
from pathlib import Path

from .config import DOC_FILES, SOURCE_EXTENSIONS, DOCS_STALENESS_THRESHOLD


def check_docs_staleness() -> int:
    print("--- Checking Docs Staleness ---")
    stale_count = 0

    try:
        subprocess.check_call(
            ["git", "rev-parse", "--is-inside-work-tree"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
        )
    except Exception:
        print("Not a git repository, skipping staleness check.")
        return 0

    for doc in DOC_FILES:
        doc_path = Path(doc)
        if not doc_path.exists():
            continue

        try:
            last_doc_commit = subprocess.check_output(
                ["git", "log", "-1", "--format=%ct", str(doc)], text=True
            ).strip()
            if not last_doc_commit:
                continue

            changed_files = subprocess.check_output(
                ["git", "diff", "--name-only", f"HEAD@{{{last_doc_commit}}}"], text=True
            ).splitlines()
            source_changes = [f for f in changed_files if Path(f).suffix in SOURCE_EXTENSIONS]

            if len(source_changes) > DOCS_STALENESS_THRESHOLD:
                stale_count += 1
        except Exception as e:
            print(f"Error checking staleness for {doc}: {e}")

    return stale_count
