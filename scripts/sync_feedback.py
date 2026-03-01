import os
import sys
import json
import re
from datetime import datetime

def slugify(text):
    text = text.lower()
    text = re.sub(r'[^\w\s-]', '', text)
    text = re.sub(r'[\s_-]+', '_', text).strip('_')
    return text

def main():
    if len(sys.argv) < 2:
        print("Usage: python scripts/sync_feedback.py '<json_string_or_file_path>'")
        sys.exit(1)

    input_data = sys.argv[1]
    data = []

    # Check if input is a file path
    if os.path.exists(input_data):
        try:
            with open(input_data, 'r', encoding='utf-8') as f:
                data = json.load(f)
        except Exception as e:
            print(f"Error reading file: {e}")
            sys.exit(1)
    else:
        # Assume it's a JSON string
        try:
            data = json.loads(input_data)
        except Exception as e:
            print(f"Error parsing JSON string: {e}")
            sys.exit(1)

    if not isinstance(data, list):
        data = [data]

    feedback_dir = "docs/feedback"
    errors_dir = "docs/errors"
    os.makedirs(feedback_dir, exist_ok=True)
    os.makedirs(errors_dir, exist_ok=True)

    synced_count = 0
    for entry in data:
        title = entry.get("Title", "Untitled")
        fb_type = entry.get("Type", "Suggestion")
        description = entry.get("Description", "")
        source = entry.get("Source", "In-Game UI")
        stack = entry.get("StackTrace", "")
        date_str = entry.get("Date", datetime.now().isoformat())[:10]

        # Determine target directory
        # Type is 3 for Error in the enum (Error=3) or the string "Error"
        is_error = fb_type == "Error" or fb_type == 3
        target_dir = errors_dir if is_error else feedback_dir

        template = f"""# {'Error' if is_error else 'Feedback'}: {title}

**Date:** {date_str}
**Type:** {fb_type}
**Source:** {source}

## Description
{description}

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
"""
        if stack:
            template += f"\n## Stack Trace\n```\n{stack}\n```\n"

        base_name = f"{date_str}_{slugify(title[:30])}"
        filename = f"{base_name}.md"
        filepath = os.path.join(target_dir, filename)
        
        counter = 1
        while os.path.exists(filepath):
            filename = f"{base_name}_{counter}.md"
            filepath = os.path.join(target_dir, filename)
            counter += 1

        with open(filepath, "w", encoding="utf-8") as f:
            f.write(template)
        
        abs_path = os.path.abspath(filepath)
        print(f"[CREATED] {abs_path}")
        synced_count += 1
    
    print(f"\n[SUCCESS] Synced {synced_count} items.")

if __name__ == "__main__":
    main()
