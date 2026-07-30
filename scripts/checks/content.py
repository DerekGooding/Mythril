"""
Content graph and visualization health checks.
"""
import sys
import subprocess

from .config import record_failure


def check_content_graph() -> bool:
    print("--- Verifying Content Graph Integrity ---")
    try:
        result = subprocess.run(
            [sys.executable, "scripts/verify_graph.py"],
            capture_output=True, text=True,
        )
        if result.returncode == 0:
            print(result.stdout.strip())
            return True
        else:
            record_failure(
                "content_graph",
                "Content contract violations found",
                {"output": result.stdout.strip()},
            )
            print(result.stdout)
            return False
    except Exception as e:
        record_failure("content_graph", f"Error running verification script: {e}")
        return False


def check_visualization() -> bool:
    print("--- Verifying Visualization Module ---")
    try:
        print("Running Python tests...")
        result = subprocess.run(
            [sys.executable, "-m", "unittest", "discover", "modules/visualization/tests/"],
            capture_output=True, text=True,
        )
        if result.returncode != 0:
            record_failure("visualization", "Python tests failed", {"output": result.stderr})
            return False

        print("Generating Dashboard...")
        subprocess.check_call([sys.executable, "scripts/visualize.py", "--no-serve"])

        print("Running UI tests...")
        result = subprocess.run(
            ["node", "modules/visualization/tests/test_ui.js"],
            capture_output=True, text=True,
        )
        if result.returncode != 0:
            record_failure("visualization", "UI tests failed", {"output": result.stdout + result.stderr})
            return False

        print("[SUCCESS] Visualization health verified.")
        return True
    except Exception as e:
        record_failure("visualization", f"Error during visualization check: {e}")
        return False
