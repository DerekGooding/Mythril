import subprocess
import sys
import os

def run():
    print("Checking dependencies...")
    try:
        import streamlit
        import pandas
    except ImportError:
        print("Missing dependencies. Installing streamlit and pandas...")
        subprocess.check_call([sys.executable, "-m", "pip", "install", "streamlit", "pandas"])

    print("Launching Mythril Content Manager...")
    # Change to the module directory to ensure relative imports in app.py work correctly
    os.chdir(os.path.join(os.getcwd(), "modules", "contentManager"))
    
    try:
        subprocess.run(["streamlit", "run", "app.py"])
    except KeyboardInterrupt:
        print("\nShutting down Content Manager.")

if __name__ == "__main__":
    run()
