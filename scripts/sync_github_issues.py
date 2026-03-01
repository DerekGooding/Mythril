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

def fetch_from_github(repo_owner, repo_name):
    url = f"https://api.github.com/repos/{repo_owner}/{repo_name}/issues?state=open"
    headers = {"User-Agent": "Mythril-Feedback-Sync"}
    
    try:
        print(f"Fetching issues from {repo_owner}/{repo_name}...")
        req = urllib.request.Request(url, headers=headers)
        with urllib.request.urlopen(req) as response:
            issues = json.loads(response.read().decode('utf-8'))
            
            # GitHub issues include PRs, filter them out
            return [i for i in issues if "pull_request" not in i]
    except Exception as e:
        print(f"Error fetching GitHub issues: {e}")
        return []

def main():
    repo_owner = "DerekGooding"
    repo_name = "Mythril"
    
    issues = fetch_from_github(repo_owner, repo_name)
    if not issues:
        print("No new issues found.")
        return

    feedback_dir = "docs/feedback"
    os.makedirs(feedback_dir, exist_ok=True)

    synced_count = 0
    for issue in issues:
        title = issue.get("title", "Untitled")
        body = issue.get("body", "")
        source = f"GitHub Issue #{issue.get('number')}"
        user = issue.get("user", {}).get("login", "unknown")
        date_str = issue.get("created_at", datetime.now().isoformat())[:10]
        
        # Determine Type based on labels
        labels = [l.get("name", "").lower() for l in issue.get("labels", [])]
        fb_type = "Bug" if "bug" in labels else "Feature Request" if "feature" in labels else "Suggestion"

        template = f"""# Feedback: {title}

**Date:** {date_str}
**Type:** {fb_type}
**Source:** {source} (User: {user})

## Description
{body}

## Status
- [ ] Investigated
- [ ] Implemented
- [ ] Verified
"""
        base_name = f"{date_str}_github_{issue.get('number')}_{slugify(title[:30])}"
        filename = f"{base_name}.md"
        filepath = os.path.join(feedback_dir, filename)
        
        # Avoid overwriting if it already exists locally
        if os.path.exists(filepath):
            continue

        with open(filepath, "w", encoding="utf-8") as f:
            f.write(template)
        print(f"Synced: {filename}")
        synced_count += 1
    
    print(f"
[SUCCESS] Synced {synced_count} new items from GitHub.")

if __name__ == "__main__":
    main()
