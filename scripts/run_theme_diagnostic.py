import subprocess
import time
import os
import sys
import urllib.request
from selenium import webdriver
from selenium.webdriver.edge.service import Service as EdgeService
from selenium.webdriver.edge.options import Options as EdgeOptions
from webdriver_manager.microsoft import EdgeChromiumDriverManager
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

def run_diagnostic():
    print("--- Starting Final Theme System Confirmation ---")
    
    # 1. Start the main project in the background
    project_path = "Mythril.Blazor/Mythril.Blazor.csproj"
    print(f"Launching {project_path} on port 5006...")
    
    env = os.environ.copy()
    env["ASPNETCORE_URLS"] = "http://localhost:5006"
    
    # Using --no-build to speed up since we built recently
    process = subprocess.Popen(
        ["dotnet", "run", "--project", project_path, "--no-launch-profile"],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        env=env
    )
    
    url = "http://localhost:5006"
    
    # 2. Wait for server to respond
    max_retries = 12
    server_ready = False
    print("Waiting for server to respond...")
    for i in range(max_retries):
        try:
            with urllib.request.urlopen(url) as response:
                if response.getcode() == 200:
                    print("Server is UP.")
                    server_ready = True
                    break
        except Exception:
            print(f"Server not ready yet (Attempt {i+1}/{max_retries})...")
            time.sleep(5)
    
    if not server_ready:
        print("CRITICAL ERROR: Server failed to start in time.")
        process.terminate()
        return

    # 3. Setup Selenium (Headless Edge)
    print("Initializing Headless Browser Automation...")
    options = EdgeOptions()
    options.add_argument("--headless")
    options.add_argument("--disable-gpu")
    options.add_argument("--no-sandbox")
    options.set_capability('goog:loggingPrefs', {'browser': 'ALL'})
    
    try:
        driver = webdriver.Edge(service=EdgeService(EdgeChromiumDriverManager().install()), options=options)
        driver.get(url)
        
        wait = WebDriverWait(driver, 30)
        
        # Helper to get current theme href
        def get_theme_href():
            return driver.execute_script("return document.getElementById('theme').getAttribute('href')")

        # Initial state
        print("Verifying Initial State...")
        initial_href = get_theme_href()
        print(f"  Initial Theme Href: {initial_href}")

        # Find and click toggle button
        print("Locating Toggle Theme button...")
        # The button text is "Toggle Theme"
        button = wait.until(EC.element_to_be_clickable((By.XPATH, "//button[contains(text(), 'Toggle Theme')]")))
        
        print("Action: Clicking Toggle Theme (Switching to Dark)...")
        button.click()
        time.sleep(2) # Wait for transition
        
        dark_href = get_theme_href()
        print(f"  Current Theme Href: {dark_href}")
        
        if "dark-theme.css" in dark_href:
            print("SUCCESS: Theme correctly switched to dark-theme.css")
        else:
            print(f"FAILURE: Href did not update as expected. (Got: {dark_href})")

        print("Action: Clicking Toggle Theme (Switching back to Light)...")
        button.click()
        time.sleep(2)
        
        light_href = get_theme_href()
        print(f"  Current Theme Href: {light_href}")
        
        if "light-theme.css" in light_href:
            print("SUCCESS: Theme correctly switched back to light-theme.css")
        else:
            print(f"FAILURE: Href did not update as expected. (Got: {light_href})")

        driver.quit()
        
    except Exception as e:
        print(f"Diagnostic Error: {e}")
    finally:
        print("Cleaning up: Stopping server...")
        process.terminate()

if __name__ == "__main__":
    run_diagnostic()
