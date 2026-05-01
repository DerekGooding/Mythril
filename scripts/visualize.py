import sys
import os

# Add project root to path so we can import modules
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from modules.visualization.orchestrator import run_visualization
except ImportError:
    # If running from a context where modules is not in path
    print("Error: Could not import modules.visualization. Ensure you are running from project root.")
    sys.exit(1)

if __name__ == "__main__":
    no_serve = "--no-serve" in sys.argv
    run_visualization(no_serve=no_serve)
