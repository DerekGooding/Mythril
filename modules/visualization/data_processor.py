import json
import os
import sys
import re
from collections import deque
from .constants import GRAPH_FILE

def load_graph():
    if not os.path.exists(GRAPH_FILE):
        # Allow running from root or scripts dir
        alt_path = os.path.join("..", "..", GRAPH_FILE)
        if os.path.exists(alt_path):
            with open(alt_path, 'r', encoding='utf-8') as f:
                return json.load(f)
        print(f"Error: {GRAPH_FILE} not found. Run scripts/migrate_to_graph.py first.")
        sys.exit(1)
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

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

def _matches_activity(node_name, activity_names):
    if node_name in activity_names:
        return True
    
    # Try normalization for refinements
    # node: "Refine Wood - Herb"
    # activity: "Refine Wood:Log->Herb"
    norm_node = node_name.lower().replace(" ", "").replace("-", "")
    for act in activity_names:
        norm_act = act.lower().replace(" ", "").replace(":", "").replace("->", "")
        if norm_node in norm_act or norm_act in norm_node:
            return True
    return False

def enrich_data(nodes):
    """Calculate tiers and clusters in Python for better performance."""
    node_map = {n["id"]: n for n in nodes}
    sim_data = parse_simulation_report()
    
    # 1. Build Adjacency and Edge Counts
    adj = {n["id"]: [] for n in nodes}
    edge_counts = {n["id"]: 0 for n in nodes}
    
    for n in nodes:
        for direction in ["out_edges", "in_edges"]:
            if direction in n:
                for rel_type, targets in n[direction].items():
                    for target in targets:
                        target_id = target if isinstance(target, str) else target.get("targetId")
                        if target_id in node_map:
                            edge_counts[n["id"]] += 1
                            edge_counts[target_id] += 1
                            if direction == "out_edges":
                                adj[n["id"]].append(target_id)

    # 2. BFS for Tiers
    tiers = {n["id"]: 0 for n in nodes}
    roots = [n["id"] for n in nodes if n["id"] == "quest_prologue"]
    if not roots:
        roots = [n["id"] for n in nodes if not n.get("in_edges")]
    
    queue = deque([(root, 0) for root in roots])
    visited_depth = {r: 0 for r in roots}
    
    while queue:
        curr_id, d = queue.popleft()
        for neighbor in adj[curr_id]:
            if neighbor not in visited_depth or visited_depth[neighbor] < d + 1:
                visited_depth[neighbor] = d + 1
                tiers[neighbor] = d + 1
                queue.append((neighbor, d + 1))

    # Force Slayer to end
    max_bfs_tier = max(tiers.values()) if tiers else 0
    for n in nodes:
        if n["id"] == "cadence_slayer" or "slayer" in n["id"].lower():
            tiers[n["id"]] = max_bfs_tier + 1

    # 3. Clusters
    clusters, cluster_names = _identify_clusters(nodes)

    # Hub Detection Threshold
    HUB_THRESHOLD = 10

    sust_names = sim_data.get("sustainable", set())
    unsust_names = sim_data.get("unsustainable", set())

    for n in nodes:
        n["tier"] = tiers.get(n["id"], 0)
        n["cluster_id"] = clusters.get(n["id"], "cluster_none")
        n["is_hub"] = edge_counts[n["id"]] > HUB_THRESHOLD
        
        # Milestone Detection
        is_milestone = False
        if n["type"] == "Quest":
            # Unlocks something important
            out = n.get("out_edges", {})
            if "unlocks_cadence" in out or "unlocks_location" in out:
                is_milestone = True
            if n["id"] == "quest_prologue":
                is_milestone = True
        n["is_milestone"] = is_milestone

        # Simulation Integration
        n["simulation"] = {
            "sustainable": _matches_activity(n["name"], sust_names),
            "unsustainable": _matches_activity(n["name"], unsust_names),
            "net_rate": sim_data.get("rates", {}).get(n["name"], 0)
        }
    
    return nodes, cluster_names

def _identify_clusters(nodes):
    clusters = {}
    cluster_names = {}
    for n in nodes:
        if n["type"] == "Location":
            c_id = f"cluster_loc_{n['id']}"
            cluster_names[c_id] = n["name"]
            if "out_edges" in n and "contains" in n["out_edges"]:
                for target in n["out_edges"]["contains"]:
                    clusters[target["targetId"]] = c_id
            clusters[n["id"]] = c_id
        elif n["type"] == "Cadence":
            c_id = f"cluster_cad_{n['id']}"
            cluster_names[c_id] = n["name"]
            if "out_edges" in n and "provides_ability" in n["out_edges"]:
                for target in n["out_edges"]["provides_ability"]:
                    clusters[target["targetId"]] = c_id
            clusters[n["id"]] = c_id
        elif n["type"] == "Refinement":
            is_magic = n.get("data", {}).get("primary_stat") in ["Magic", "Speed"]
            c_id = "cluster_ref_magic" if is_magic else "cluster_ref_material"
            cluster_names[c_id] = "Workshop: Magic" if is_magic else "Workshop: Materials"
            clusters[n["id"]] = c_id
    return clusters, cluster_names
