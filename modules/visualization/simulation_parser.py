import os
import re
import json

def parse_simulation_report():
    report_path = "simulation_report.md"
    if not os.path.exists(report_path):
        return {}
    
    data = {
        "sustainable": set(),
        "unsustainable": set(),
        "rates": {}
    }
    
    try:
        with open(report_path, "r", encoding="utf-8") as f:
            content = f.read()
            
            # Parse sustainable
            sust_match = re.search(r"### Sustainable Recurring Activities\n(.*?)\n\n", content, re.DOTALL)
            if sust_match:
                for line in sust_match.group(1).split("\n"):
                    if line.strip().startswith("- "):
                        data["sustainable"].add(line.strip()[2:])
            
            # Parse unsustainable
            unsust_match = re.search(r"### ⚠️ Unsustainable Activities.*?\n(.*?)\n\n", content, re.DOTALL)
            if unsust_match:
                for line in unsust_match.group(1).split("\n"):
                    if line.strip().startswith("- "):
                        data["unsustainable"].add(line.strip()[2:])
            
            # Parse rates
            rates_match = re.search(r"### Net Resource Rates \(per second\)\n(.*?)\n\n", content, re.DOTALL)
            if rates_match:
                for line in rates_match.group(1).split("\n"):
                    m = re.match(r"- \*\*(.*?)\*\*: ([\d.]+)/s", line.strip())
                    if m:
                        data["rates"][m.group(1)] = float(m.group(2))
    except Exception as e:
        print(f"Warning: Failed to parse simulation report: {e}")
        
    return data

def load_simulation_data():
    sim_path = "simulation_report.json"
    if not os.path.exists(sim_path):
        return {}
    try:
        with open(sim_path, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception as e:
        print(f"Warning: Failed to load simulation JSON: {e}")
        return {}

def matches_activity(node_name, activity_names):
    if node_name in activity_names:
        return True
    
    # Try normalization for refinements
    norm_node = node_name.lower().replace(" ", "").replace("-", "")
    for act in activity_names:
        norm_act = act.lower().replace(" ", "").replace(":", "").replace("->", "")
        if norm_node in norm_act or norm_act in norm_node:
            return True
    return False
