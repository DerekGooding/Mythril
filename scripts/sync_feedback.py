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
        if not isinstance(entry, dict):
            print(f"Skipping non-dict entry: {entry}")
            continue
            
        # Create a lowercase mapping of keys
        entry_low = {str(k).lower(): v for k, v in entry.items()}
        
        def get_val(key, default=""):
            return entry_low.get(key.lower(), default)

        title = get_val("Title", "Untitled")
        
        # Handle numeric or string Type
        raw_type = get_val("Type", "Suggestion")
        type_map = {0: "Bug", 1: "FeatureRequest", 2: "Suggestion", 3: "Error"}
        if isinstance(raw_type, int) and raw_type in type_map:
            fb_type = type_map[raw_type]
        else:
            fb_type = str(raw_type)

        description = get_val("Description", "")
        source = get_val("Source", "In-Game UI")
        stack = get_val("StackTrace", "")
        logs = get_val("ConsoleLog", "")
        date_str = get_val("Date", datetime.now().isoformat())[:10]

        is_error = fb_type == "Error"
        target_dir = errors_dir if is_error else feedback_dir

        # Combine info for description as requested
        final_description = description
        if is_error:
            if not final_description or final_description == "Automated Error Report":
                final_description = stack.split('\n')[0] if stack else "An unknown runtime error occurred."
            
            # Append stack and logs to description area
            if stack:
                final_description += f"\n\n### Stack Trace\n```\n{stack}\n```"
            if logs:
                final_description += f"\n\n### Console Logs\n```\n{logs}\n```"

        template = f"""# {'Error' if is_error else 'Feedback'}: {title}

**Date:** {date_str}
**Type:** {fb_type}
**Source:** {source}

## Description
{final_description}

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
"""
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
