import os
import sys
import shutil
import subprocess
from datetime import datetime
from pathlib import Path

REPORTS_DIR = Path("docs/mutation_reports")

def archive():
    # 1. Get current git hash
    try:
        commit_hash = subprocess.check_output(["git", "rev-parse", "--short", "HEAD"], text=True).strip()
    except:
        commit_hash = "unknown"

    timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
    folder_name = f"{timestamp}_v{commit_hash}"
    target_path = REPORTS_DIR / folder_name

    print(f"Archiving mutation results to: {target_path}")

    # Create target directory
    os.makedirs(target_path, exist_ok=True)

    # 2. Run mutation test and capture output
    print("Running mutation pipeline (this may take a minute)...")
    # We call run_mutation.py directly to get the raw report
    result = subprocess.run([sys.executable, "scripts/run_mutation.py"], capture_output=True, text=True)
    
    # 3. Save report summary
    report_file = target_path / "mutation_report.txt"
    with open(report_file, "w", encoding="utf-8") as f:
        f.write(result.stdout)
        if result.stderr:
            f.write("\n\nERRORS:\n")
            f.write(result.stderr)
    
    print(f"Archive complete. Report saved to {report_file}")

if __name__ == "__main__":
    archive()
