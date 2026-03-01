import os
import re
from datetime import datetime

def slugify(text):
    text = text.lower()
    text = re.sub(r'[^\w\s-]', '', text)
    text = re.sub(r'[\s_-]+', '_', text).strip('_')
    return text

def get_input(prompt, default=""):
    val = input(f"{prompt} [{default}]: " if default else f"{prompt}: ").strip()
    return val if val else default

def main():
    print("--- Mythril User Feedback Collector ---")
    
    title = get_input("Short Title (e.g., Inventory UI lag)")
    date_str = datetime.now().strftime("%Y-%m-%d")
    
    print("
Select Type:")
    print("1. Bug")
    print("2. Feature Request")
    print("3. Suggestion")
    type_choice = get_input("Choice (1-3)", "1")
    type_map = {"1": "Bug", "2": "Feature Request", "3": "Suggestion"}
    fb_type = type_map.get(type_choice, "Bug")
    
    source = get_input("Source (e.g., Discord / User Name)")
    description = get_input("Detailed Description")
    impact = get_input("Impact (How it affects experience)")
    solution = get_input("Proposed Solution (Optional)")
    
    template = f"""# Feedback: {title}

**Date:** {date_str}
**Type:** {fb_type}
**Source:** {source}

## Description
{description}

## Impact
{impact}

## Proposed Solution (Optional)
{solution if solution else "N/A"}

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
"""

    # Generate filename
    base_name = f"{date_str}_{slugify(title)}"
    feedback_dir = "docs/feedback"
    os.makedirs(feedback_dir, exist_ok=True)
    
    filename = f"{base_name}.md"
    filepath = os.path.join(feedback_dir, filename)
    
    counter = 1
    while os.path.exists(filepath):
        filename = f"{base_name}_{counter}.md"
        filepath = os.path.join(feedback_dir, filename)
        counter += 1
        
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(template)
        
    print(f"
[SUCCESS] Feedback saved to: {filepath}")
    print("Note: The health check will now fail until this feedback is resolved/deleted.")

if __name__ == "__main__":
    main()
