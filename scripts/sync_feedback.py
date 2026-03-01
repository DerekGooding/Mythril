import os
import sys
import json
import re
import urllib.request
from datetime import datetime

def slugify(text):
    text = text.lower()
    text = re.sub(r'[^\w\s-]', '', text)
    text = re.sub(r'[\s_-]+', '_', text).strip('_')
    return text

def fetch_from_remote(url):
    try:
        print(f"Fetching from remote endpoint...")
        with urllib.request.urlopen(url) as response:
            csv_data = response.read().decode('utf-8')
            lines = csv_data.strip().split('\n')
            if not lines: return []
            
            # Google Apps Script doGet returns CSV: Timestamp, Type, Title, Description, Source, StackTrace
            entries = []
            for line in lines[1:]: # Skip header
                # Simple comma split
                parts = line.split(',')
                if len(parts) >= 6:
                    entries.append({
                        "Date": parts[0],
                        "Type": parts[1],
                        "Title": parts[2],
                        "Description": parts[3],
                        "Source": parts[4],
                        "StackTrace": parts[5]
                    })
            return entries
    except Exception as e:
        print(f"Error fetching remote feedback: {e}")
        return []

def main():
    data = []
    if "--remote" in sys.argv:
        # Try to find --url argument
        url = ""
        if "--url" in sys.argv:
            idx = sys.argv.index("--url")
            if idx + 1 < len(sys.argv):
                url = sys.argv[idx + 1]
        
        if not url:
            print("Error: --remote requires --url <WEB_APP_URL>")
            sys.exit(1)
            
        data = fetch_from_remote(url)
    elif len(sys.argv) >= 2:
        try:
            data = json.loads(sys.argv[1])
        except Exception as e:
            print(f"Error parsing JSON: {e}")
            sys.exit(1)
    else:
        print("Usage: python scripts/sync_feedback.py '<json_string>' OR python scripts/sync_feedback.py --remote")
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
            template += f"\n## Stack Trace\n```\n{stack}\n```\n"

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
