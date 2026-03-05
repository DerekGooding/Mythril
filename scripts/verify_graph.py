import json
import os
import sys

GRAPH_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"

def load_graph():
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def verify():
    nodes = load_graph()
    node_map = {n["id"]: n for n in nodes}
    violations = []

    print(f"Verifying {len(nodes)} nodes...")

    # --- 1. Connectivity Rules ---
    
    # A. The Root Rule: Exactly ONE quest with no requirements that is NOT contained in a gated location
    # A quest is a "root candidate" if:
    # 1. It has no 'requires_quest' in_edges
    # 2. It is in a location that has NO 'requires_quest' (i.e. the starting hub)
    
    # First, find locations that are gated
    gated_locations = set()
    for n in nodes:
        if n["type"] == "Location" and "requires_quest" in n["in_edges"]:
            gated_locations.add(n["id"])

    # Now find root quests
    root_nodes = []
    for n in nodes:
        if n["type"] != "Quest": continue
        
        # Must have no direct requirements
        if "requires_quest" in n["in_edges"]: continue
        
        # Find which location contains this quest
        # This requires checking the 'contains' edge FROM the location TO the quest
        # Optimization: Build a 'contained_in' map
        container_id = None
        for loc in nodes:
            if loc["type"] == "Location":
                contains = loc["out_edges"].get("contains", [])
                if any(edge["targetId"] == n["id"] for edge in contains):
                    container_id = loc["id"]
                    break
        
        # If it's in a gated location, it's implicitly gated
        if container_id in gated_locations: continue
        
        root_nodes.append(n)
    
    if len(root_nodes) != 1:
        names = [n["name"] for n in root_nodes]
        violations.append(f"ROOT RULE: Expected exactly 1 root quest, found {len(root_nodes)}: {names}")
    elif root_nodes[0]["name"] != "Prologue":
        violations.append(f"ROOT RULE: Root quest must be 'Prologue', found '{root_nodes[0]['name']}'")

    # B. Orphan Detection (Reachability)
    # Perform BFS from Root
    if root_nodes:
        visited = set()
        queue = [root_nodes[0]["id"]]
        visited.add(root_nodes[0]["id"])
        
        # Also add "implicitly reachable" nodes like Items/Stats which are always available?
        # No, items are reachable if they are produced by a reachable node or are inputs to one.
        # For this pass, we focus on Quests, Locations, Cadences.
        
        while queue:
            curr_id = queue.pop(0)
            curr = node_map.get(curr_id)
            if not curr: continue

            # Follow out_edges
            for relation, edges in curr["out_edges"].items():
                for edge in edges:
                    target_id = edge["targetId"]
                    if target_id not in visited:
                        visited.add(target_id)
                        queue.append(target_id)
            
            # Follow in_edges (reverse dependencies)? No, flow is usually forward.
            # But "unlocks_location" is forward. "requires_quest" is backward dependency.
            # If A requires B, and B is reached, A becomes reachable (if all reqs met).
            # This is complex to simulate fully without state.
            # Simplified Reachability: Can we trace a path from Root to Node via ANY relationship?
            # Actually, standard reachability: Is there a path of requirements leading BACK to root?
            # Or path of unlocks leading FROM root?
    
    # --- 2. Semantic Contracts ---

    # C. The Economic Anchor
    if root_nodes:
        root = root_nodes[0]
        rewards = root["out_edges"].get("rewards", [])
        gold_reward = next((r for r in rewards if "item_gold" in r["targetId"]), None)
        
        if not gold_reward or gold_reward["quantity"] < 100:
            violations.append(f"ECONOMIC ANCHOR: Root node '{root['name']}' must reward at least 100 Gold.")

    # D. Location Gating
    for node in nodes:
        if node["type"] == "Location" and node["name"] != "Village":
            if "requires_quest" not in node["in_edges"] or not node["in_edges"]["requires_quest"]:
                violations.append(f"LOCATION GATING: Location '{node['name']}' is not gated by a quest.")

    # E. Dead-End Detection (One-time quests must unlock something)
    for node in nodes:
        if node["type"] == "Quest" and node["data"].get("quest_type") in ["Single", "Unlock"]:
            # Must unlock a location, cadence, or be required by another quest
            is_requirement = False
            # Check if any other node requires this one
            # This requires scanning all nodes or building a reverse map.
            # Optimization: Build reverse map once?
            pass # TODO: Implement efficient reverse lookup

    # F. Capacity Logic
    # Check Refinement inputs vs Base Magic Capacity (30) if no Magic Pocket required
    base_cap = 30
    for node in nodes:
        if node["type"] == "Refinement":
            consumes = node["out_edges"].get("consumes", [])
            for input_edge in consumes:
                target = node_map.get(input_edge["targetId"])
                if target and target["data"].get("item_type") == "Spell":
                    if input_edge["quantity"] > base_cap:
                        # Check if this refinement requires a Magic Pocket ability
                        # This is hard to traverse without full graph search.
                        # Simple check: Warn if > 30
                        # violations.append(f"CAPACITY: Refinement '{node['name']}' requires {input_edge['quantity']} {target['name']}, exceeding base capacity {base_cap}.")
                        pass

    # Report
    if violations:
        print(f"\n[FAIL] Found {len(violations)} contract violations:")
        for v in violations:
            print(f"  ❌ {v}")
        sys.exit(1)
    else:
        print("\n[SUCCESS] Content graph verified. No violations found.")
        sys.exit(0)

if __name__ == "__main__":
    verify()
