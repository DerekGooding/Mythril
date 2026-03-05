import os
import subprocess
import shutil
import re
import sys

# Get absolute path to root
ROOT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))

TARGET_FILES = [
    os.path.join(ROOT_DIR, "Mythril.Data", "JunctionManager.cs"),
    os.path.join(ROOT_DIR, "Mythril.Data", "ResourceManager_Quests.cs"),
    os.path.join(ROOT_DIR, "Mythril.Data", "InventoryManager.cs")
]

TEST_PROJECT = os.path.join(ROOT_DIR, "Mythril.Tests/Mythril.Tests.csproj")

# (Regex pattern, replacement)
MUTATIONS = [
    (r'\s\+\s', ' - '),
    (r'\s-\s', ' + '),
    (r'\s\*\s', ' / '),
    (r'==', '!='),
    (r'!=', '=='),
    (r' > ', ' < '),
    (r' < ', ' > '),
    (r' >= ', ' < '),
    (r' <= ', ' > ')
]

def run_tests(show_output=False):
    try:
        # Run tests and capture output
        result = subprocess.run(["dotnet", "test", TEST_PROJECT, "-c", "Debug"], 
                                capture_output=True, text=True, cwd=ROOT_DIR)
        
        if show_output:
            print(result.stdout)
        
        # print(f"RC:{result.returncode}", end=" ", flush=True) # Too noisy for long runs, but good for debug

        # If return code is 0, it's a pass
        if result.returncode == 0:
            return True
        
        # If return code is non-zero, it's a failure
        return False
    except Exception:
        return False

def mutate_and_test():
    total_mutants = 0
    killed_count = 0
    survived_mutants = []

    print("Starting Custom Mutation Testing Pipeline...")
    
    # 1. Baseline Check
    if not run_tests(show_output=True):
        print("FAIL: Baseline tests failed. Fix project state first.")
        return

    for file_path in TARGET_FILES:
        if not os.path.exists(file_path): continue

        with open(file_path, 'r', encoding='utf-8') as f:
            original_content = f.read()

        lines = original_content.splitlines()
        
        for i, line in enumerate(lines):
            # Avoid mutating declarations, imports, comments
            if any(x in line for x in ["using ", "namespace ", "class ", "record ", "struct ", "//"]):
                continue

            for pattern, replacement in MUTATIONS:
                if re.search(pattern, line):
                    mutated_line = re.sub(pattern, replacement, line, count=1)
                    if mutated_line == line: continue

                    # Apply mutation
                    mutated_lines = list(lines)
                    mutated_lines[i] = mutated_line
                    mutated_content = "\n".join(mutated_lines)

                    try:
                        with open(file_path, 'w', encoding='utf-8') as f:
                            f.write(mutated_content)

                        total_mutants += 1
                        # Limit for PoC
                        if total_mutants > 15:
                            return # Exit gracefully

                        print(f"[{total_mutants:02d}] {file_path}:{i+1} | {replacement.strip()}", end=" ", flush=True)

                        is_pass = run_tests()
                        if is_pass:
                            print("-> [SURVIVED]")
                            survived_mutants.append(f"{file_path}:{i+1}: {line.strip()} -> {mutated_line.strip()}")
                        else:
                            print("-> [KILLED]")
                            killed_count += 1
                    finally:
                        # ALWAYS revert
                        with open(file_path, 'w', encoding='utf-8') as f:
                            f.write(original_content)

    # Report
    print("\n" + "="*40)
    print(f"Mutation Report Summary")
    print(f"Total Mutants:  {total_mutants}")
    print(f"Killed:         {killed_count}")
    print(f"Survived:       {len(survived_mutants)}")
    score = (killed_count / total_mutants * 100) if total_mutants > 0 else 0
    print(f"Mutation Score: {score:.2f}%")
    print("="*40)

if __name__ == "__main__":
    try:
        mutate_and_test()
    except KeyboardInterrupt:
        print("\nAborted by user.")
        # Ensure all files reverted (redundant but safe)
        for f_path in TARGET_FILES:
            subprocess.run(["git", "checkout", f_path], capture_output=True)
