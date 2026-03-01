import subprocess
import time
import os
import sys
from selenium import webdriver
from selenium.webdriver.edge.service import Service as EdgeService
from selenium.webdriver.edge.options import Options as EdgeOptions
from webdriver_manager.microsoft import EdgeChromiumDriverManager
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

def run_diagnostic():
    print("--- Starting Theme Diagnostic ---")
    
    # 1. Start the Blazor project in the background
    test_project_path = "Mythril.ThemeTest/Mythril.ThemeTest.csproj"
    print(f"Launching {test_project_path}...")
    
    # Use a specific port to be certain
    env = os.environ.copy()
    env["ASPNETCORE_URLS"] = "http://localhost:5005"
    
    process = subprocess.Popen(
        ["dotnet", "run", "--project", test_project_path],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        env=env
    )
    
    url = "http://localhost:5005"
    
    print("Waiting for server to start (30s)...")
    time.sleep(30) 
    
    # 2. Setup Selenium (Headless Edge)
    print("Setting up Headless Browser...")
    options = EdgeOptions()
    options.add_argument("--headless")
    options.add_argument("--disable-gpu")
    options.add_argument("--no-sandbox")
    options.set_capability('goog:loggingPrefs', {'browser': 'ALL'})
    
    try:
        # Check if we can reach the URL first with a simple check
        import urllib.request
        try:
            with urllib.request.urlopen(url) as response:
                print(f"Server response code: {response.getcode()}")
        except Exception as e:
            print(f"Server ping failed: {e}")
            # Continue anyway, might just be a slow startup

        driver = webdriver.Edge(service=EdgeService(EdgeChromiumDriverManager().install()), options=options)
        
        print(f"Navigating to {url}...")
        driver.get(url)
        
        # Wait for Blazor to load
        print("Waiting for app to load...")
        wait = WebDriverWait(driver, 30)
        button = wait.until(EC.element_to_be_clickable((By.TAG_NAME, "button")))
        
        print("Clicking Toggle Theme...")
        button.click()
        time.sleep(3) 
        
        button.click() # Toggle back
        time.sleep(3)
        
        # 3. Collect Logs
        print("\n--- Browser Console Logs ---")
        logs = driver.get_log('browser')
        for entry in logs:
            print(f"[{entry['level']}] {entry['message']}")
            
        print("\n--- On-Screen Diagnostics ---")
        diag_element = driver.find_element(By.TAG_NAME, "pre")
        print(diag_element.text)
        
        driver.quit()
        
    except Exception as e:
        print(f"Diagnostic Error: {e}")
    finally:
        print("Stopping server...")
        process.terminate()

if __name__ == "__main__":
    run_diagnostic()
