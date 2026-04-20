import json
import os
import sys
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

def enrich_data(nodes):
    """Calculate tiers and clusters in Python for better performance."""
    node_map = {n["id"]: n for n in nodes}
    
    # 1. Build Adjacency
    adj = {n["id"]: [] for n in nodes}
    for n in nodes:
        if "out_edges" in n:
            for target_list in n["out_edges"].values():
                for target in target_list:
                    target_id = target if isinstance(target, str) else target.get("targetId")
                    if target_id in adj:
                        adj[n["id"]].append(target_id)
        if "in_edges" in n:
            for source_list in n["in_edges"].values():
                for source_id in source_list:
                    if source_id in adj:
                        adj[source_id].append(n["id"])

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

    for n in nodes:
        n["tier"] = tiers.get(n["id"], 0)
        n["cluster_id"] = clusters.get(n["id"], "cluster_none")
    
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
