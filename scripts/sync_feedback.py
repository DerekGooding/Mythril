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
        print("Usage: python scripts/sync_feedback.py '<json_string>'")
        sys.exit(1)

    try:
        data = json.loads(sys.argv[1])
    except Exception as e:
        print(f"Error parsing JSON: {e}")
        sys.exit(1)

    if not isinstance(data, list):
        data = [data]

    feedback_dir = "docs/feedback"
    os.makedirs(feedback_dir, exist_ok=True)

    for entry in data:
        title = entry.get("Title", "Untitled")
        fb_type = entry.get("Type", "Suggestion")
        description = entry.get("Description", "")
        source = entry.get("Source", "In-Game UI")
        stack = entry.get("StackTrace", "")
        date_str = entry.get("Date", datetime.now().isoformat())[:10]

        template = f"""# Feedback: {title}

**Date:** {date_str}
**Type:** {fb_type}
**Source:** {source}

## Description
{description}

## Impact
Captured from In-Game UI.

## Proposed Solution (Optional)
N/A

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
"""
        if stack:
            template += f"
## Stack Trace
```
{stack}
```
"

        base_name = f"{date_str}_{slugify(title)}"
        filename = f"{base_name}.md"
        filepath = os.path.join(feedback_dir, filename)
        
        counter = 1
        while os.path.exists(filepath):
            filename = f"{base_name}_{counter}.md"
            filepath = os.path.join(feedback_dir, filename)
            counter += 1

        with open(filepath, "w", encoding="utf-8") as f:
            f.write(template)
        print(f"Synced: {filename}")

if __name__ == "__main__":
    main()
