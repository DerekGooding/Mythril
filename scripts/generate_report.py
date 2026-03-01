import os
import subprocess
import json
import datetime

def run_command(command):
    print(f"Running: {command}")
    result = subprocess.run(command, shell=True, capture_output=True, text=True)
    return result

def main():
    print("--- Generating Agentic Status Report ---")
    
    # 1. Run Health Check
    health_res = run_command("python scripts/check_health.py")
    
    # 2. Run Headless Test
    test_res = run_command("powershell -ExecutionPolicy Bypass -File ./run_ai_test.ps1")
    
    # 3. Load Health Data
    health_data = {}
    try:
        with open("scripts/data/health_summary.json", "r") as f:
            health_data = json.load(f)
    except:
        pass

    # 4. Generate STATUS.md
    with open("STATUS.md", "w", encoding="utf-8") as f:
        f.write(f"# Agentic Status Report\n")
        f.write(f"**Generated:** {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n\n")
        
        f.write("## 1. System Integrity (check_health.py)\n")
        if health_res.returncode == 0:
            f.write("✅ **PASSED**\n")
        else:
            f.write("❌ **FAILED**\n")
        
        f.write(f"- **Monoliths:** {health_data.get('monoliths', 'N/A')}\n")
        f.write(f"- **Coverage:** {health_data.get('coverage', 'N/A')}%\n")
        f.write(f"- **Docs Stale:** {not health_data.get('docs_up_to_date', True)}\n")
        
        f.write("\n## 2. Functional Verification (run_ai_test.ps1)\n")
        if test_res.returncode == 0:
            f.write("✅ **PASSED**\n")
        else:
            f.write("❌ **FAILED**\n")
        f.write("```\n")
        lines = test_res.stdout.split('\n')
        for line in lines[-10:]:
            if line.strip():
                f.write(line + "\n")
        f.write("```\n")

        f.write("\n## 3. Raw Logs\n")
        f.write("<details>\n<summary>Health Check Output</summary>\n\n")
        f.write("```\n" + health_res.stdout + "\n```\n")
        f.write("</details>\n\n")
        
        f.write("<details>\n<summary>Headless Test Output</summary>\n\n")
        f.write("```\n" + test_res.stdout + "\n```\n")
        f.write("</details>\n")

    print("STATUS.md generated.")

if __name__ == "__main__":
    main()
