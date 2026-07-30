import subprocess
import os
import webbrowser
import sys
import time

def run_command(command, description):
    print(f"--- {description} ---")
    start_time = time.time()
    try:
        # Using shell=True for compatibility with Windows environment aliases
        process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding="utf-8")
        
        for line in process.stdout:
            print(line, end="")
            
        process.wait()
        elapsed = time.time() - start_time
        
        if process.returncode == 0:
            print(f"\n[SUCCESS] {description} completed in {elapsed:.1f}s\n")
            return True
        else:
            print(f"\n[ERROR] {description} failed with exit code {process.returncode}\n")
            return False
    except Exception as e:
        print(f"\n[EXCEPTION] Failed to run {description}: {e}\n")
        return False

def main():
    print("====================================================")
    print("   Mythril Content Visualization & Review Tool")
    print("====================================================\n")

    # 1. Run Simulation
    # This generates the simulation_report.json needed for Chrono and Quantitative views
    sim_cmd = "dotnet run --project Mythril.Headless -- --run-sim"
    if not run_command(sim_cmd, "Running C# Simulation & Lattice Solver"):
        sys.exit(1)

    # 2. Generate Dashboard
    # This processes the graph and simulation data into an HTML dashboard
    viz_cmd = f'"{sys.executable}" scripts/visualize.py --no-serve'
    if not run_command(viz_cmd, "Generating Visual Dashboard"):
        sys.exit(1)

    # 3. Launch Dashboard
    output_path = os.path.abspath("output/visual_dashboard.html")
    if os.path.exists(output_path):
        print(f"--- Launching Dashboard ---")
        print(f"File: {output_path}")
        # Use file:// prefix for better browser compatibility on Windows
        webbrowser.open(f"file:///{output_path.replace('\\', '/')}")
        print("\nDone. The dashboard should now be open in your default browser.")
    else:
        print(f"\n[ERROR] Dashboard file not found at: {output_path}")
        sys.exit(1)

if __name__ == "__main__":
    main()
