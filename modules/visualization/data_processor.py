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
    
    # 1. Build Adjacency for Tiering
    adj = {n["id"]: [] for n in nodes}
    edge_counts = {n["id"]: 0 for n in nodes}
    for n in nodes:
        # Forward edges
        if "out_edges" in n:
            for rel_type, targets in n["out_edges"].items():
                for target in targets:
                    target_id = target if isinstance(target, str) else target.get("targetId")
                    if target_id in node_map:
                        edge_counts[n["id"]] += 1
                        edge_counts[target_id] += 1
                        
                        if rel_type in ["consumes", "requires_quest", "requires_ability", "requires_stat"]:
                            adj[target_id].append(n["id"])
                        else:
                            adj[n["id"]].append(target_id)
        
        if "in_edges" in n:
            for rel_type, sources in n["in_edges"].items():
                for source in sources:
                    source_id = source if isinstance(source, str) else source.get("targetId")
                    if source_id in node_map:
                        edge_counts[n["id"]] += 1
                        edge_counts[source_id] += 1
                        adj[source_id].append(n["id"])

    # 2. BFS for Tiers
    tiers = {n["id"]: 999 for n in nodes}
    prologue_node = next((n for n in nodes if n["id"] == "quest_prologue"), None)
    roots = [prologue_node["id"]] if prologue_node else [n["id"] for n in nodes if n["type"] == "Quest" and not n.get("in_edges", {}).get("requires_quest")]
    if not roots: roots = [n["id"] for n in nodes if not n.get("in_edges")]

    queue = deque([(root, 0) for root in roots])
    for r in roots: tiers[r] = 0
    
    while queue:
        curr_id, d = queue.popleft()
        for neighbor in adj[curr_id]:
            if tiers[neighbor] > d + 1:
                tiers[neighbor] = d + 1
                queue.append((neighbor, d + 1))

    for n in nodes:
        if tiers[n["id"]] == 999: tiers[n["id"]] = 0

    # 3. Node Splitting for Sustainable Resources
    sust_names = sim_data.get("sustainable", set())
    new_nodes = []
    for n in nodes:
        if n["type"] == "Item" and n["name"] in sust_names:
            # Find producers (refinements/recurring quests) that are sustainable
            producers = [m for m in nodes if m["type"] in ["Refinement", "Quest"] and _matches_activity(m["name"], sust_names)]
            sust_producers = [p for p in producers if any(e.get("targetId") == n["id"] for e in p.get("out_edges", {}).get("produces", []) + p.get("out_edges", {}).get("rewards", []))]
            
            if sust_producers:
                min_sust_tier = min(tiers[p["id"]] for p in sust_producers)
                if min_sust_tier > tiers[n["id"]] + 1:
                    # Create a "Sustainable" variant
                    sust_node = n.copy()
                    sust_node["id"] = f"{n['id']}_sustainable"
                    sust_node["name"] = f"{n['name']} (Sustainable)"
                    sust_node["tier"] = min_sust_tier + 1
                    sust_node["is_sustainable_instance"] = True
                    sust_node["original_id"] = n["id"]
                    # Redirect edges from sustainable producers to this new node
                    for p in sust_producers:
                        if "out_edges" in p:
                            for rel in ["produces", "rewards"]:
                                if rel in p["out_edges"]:
                                    for edge in p["out_edges"][rel]:
                                        if edge.get("targetId") == n["id"]:
                                            edge["targetId"] = sust_node["id"]
                    new_nodes.append(sust_node)
        
    nodes.extend(new_nodes)
    node_map = {n["id"]: n for n in nodes}

    # Force Slayer to end
    max_bfs_tier = max(t for t in tiers.values() if t < 999) if any(t < 999 for t in tiers.values()) else 0
    for n in nodes:
        if n["id"] == "cadence_slayer" or "slayer" in n["id"].lower():
            n["tier"] = max_bfs_tier + 1
        elif n["id"] not in tiers and "tier" in n:
            # Preserve tier for newly created nodes (like sustainable variants)
            pass
        else:
            n["tier"] = tiers.get(n["id"], 0)

    # 4. Final Enrichment & Stable Ordering
    clusters, cluster_names = _identify_clusters(nodes)
    nodes.sort(key=lambda x: (x.get("tier", 0), x["type"], x["name"]))
    
    # Constants for static layout
    TIER_WIDTH = 800
    VERTICAL_SPACING = 100
    
    type_counts = {}
    HUB_THRESHOLD = 10
    hubs = {n["id"] for n in nodes if edge_counts.get(n.get("original_id", n["id"]), 0) > HUB_THRESHOLD}

    # Track how many nodes are in each tier to center them
    tier_counts = {}
    for n in nodes:
        t = n.get("tier", 0)
        tier_counts[t] = tier_counts.get(t, 0) + 1

    current_tier_indices = {}

    for n in nodes:
        t = n.get("tier", 0)
        n["cluster_id"] = clusters.get(n["id"], "cluster_none")
        n["is_hub"] = n["id"] in hubs or n.get("original_id") in hubs
        
        # Deterministic Grid Position
        idx = current_tier_indices.get(t, 0)
        current_tier_indices[t] = idx + 1
        
        # Calculate Y to center the tier column
        total_in_tier = tier_counts[t]
        start_y = - (total_in_tier * VERTICAL_SPACING) / 2
        
        n["x"] = t * TIER_WIDTH
        n["y"] = start_y + (idx * VERTICAL_SPACING)
        
        n["simulation"] = {
            "sustainable": _matches_activity(n["name"], sust_names) or n.get("is_sustainable_instance"),
            "unsustainable": _matches_activity(n["name"], sim_data.get("unsustainable", set())),
            "net_rate": sim_data.get("rates", {}).get(n["name"], 0)
        }
        
        n["is_milestone"] = (n["type"] == "Quest" and (n["id"] == "quest_prologue" or "unlocks_cadence" in n.get("out_edges", {}) or "unlocks_location" in n.get("out_edges", {})))

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
        elif n["type"] == "Refinement":
            is_magic = n.get("data", {}).get("primary_stat") in ["Magic", "Speed"]
            c_id = "cluster_ref_magic" if is_magic else "cluster_ref_material"
            cluster_names[c_id] = "Workshop: Magic" if is_magic else "Workshop: Materials"
            clusters[n["id"]] = c_id
    return clusters, cluster_names
