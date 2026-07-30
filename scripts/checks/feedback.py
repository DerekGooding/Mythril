"""
Feedback integrity check: counts pending items in docs/feedback and docs/errors.
"""
from .config import FEEDBACK_DIR, ERRORS_DIR


def check_feedback() -> int:
    print("--- Checking Feedback ---")
    pending = 0
    for d in [FEEDBACK_DIR, ERRORS_DIR]:
        if d.exists():
            files = [f for f in d.iterdir() if f.is_file() and not f.name.startswith(".")]
            pending += len(files)

    if pending > 0:
        print(f"Pending feedback items: {pending}")
    return pending
