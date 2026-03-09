import json
import os
import sys

GRAPH_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"

def load_graph():
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def generate_mermaid():
    nodes = load_graph()
    
    # 1. Map associations
    location_map = {} # QuestID -> LocationName
    cadence_map = {}  # AbilityID -> CadenceName
    
    for node in nodes:
        nid = node["id"]
        ntype = node["type"]
        out_edges = node.get("out_edges", {})
        
        if ntype == "Location":
            if "contains" in out_edges:
                for edge in out_edges["contains"]:
                    location_map[edge["targetId"]] = node["name"]
        
        if ntype == "Cadence":
            for field in ["provides_ability", "unlocks_ability"]:
                if field in out_edges:
                    for edge in out_edges[field]:
                        cadence_map[edge["targetId"]] = node["name"]

    # 2. Group nodes
    by_location = {} # LocName -> [NodeLines]
    by_cadence = {}  # CadName -> [NodeLines]
    globals = []
    
    lines = ["graph TD"]
    
    # Styles
    lines.append("classDef quest fill:#4b0082,stroke:#f9f,stroke-width:2px,color:#fff;")
    lines.append("classDef location fill:#00008b,stroke:#ccf,stroke-width:2px,color:#fff;")
    lines.append("classDef cadence fill:#006400,stroke:#cfc,stroke-width:2px,color:#fff;")
    lines.append("classDef item fill:#444,stroke:#fff,stroke-width:1px,color:#fff;")
    lines.append("classDef stat fill:#8b4513,stroke:#ff8c00,stroke-width:1px,color:#fff;")
    lines.append("classDef ability fill:#2f4f4f,stroke:#00ced1,stroke-width:1px,color:#fff;")
    lines.append("classDef root fill:#8b8b00,stroke:#ff9,stroke-width:4px,color:#fff;")

    edge_lines = []
    
    for node in nodes:
        nid = node["id"]
        ntype = node["type"]
        name = node["name"]
        
        # IGNORE GOLD
        if nid == "item_gold":
            continue
            
        style = ntype.lower()
        if nid == "quest_prologue": style = "root"
        
        node_def = f'{nid}["{name.replace('"', "'")}"]:::{style}'
        
        # Assign to group
        if ntype == "Quest" and nid in location_map:
            loc = location_map[nid]
            if loc not in by_location: by_location[loc] = []
            by_location[loc].append(node_def)
        elif ntype == "Ability" and nid in cadence_map:
            cad = cadence_map[nid]
            if cad not in by_cadence: by_cadence[cad] = []
            by_cadence[cad].append(node_def)
        else:
            globals.append(node_def)

        # Edges
        in_edges = node.get("in_edges", {})
        out_edges = node.get("out_edges", {})
        
        if "requires_quest" in in_edges:
            for req_id in in_edges["requires_quest"]:
                label = "Enables" if ntype == "Location" else "Requires"
                edge_lines.append(f"{req_id} -->|{label}| {nid}")
                    
        if "requires_ability" in in_edges:
            for req_id in in_edges["requires_ability"]:
                edge_lines.append(f"{req_id} -.->|Allows| {nid}")
                
        if "requires_stat" in in_edges:
            for stat_name, val in in_edges["requires_stat"].items():
                stat_id = f"stat_{stat_name.lower()}"
                edge_lines.append(f"{stat_id} -.->|Req {val}| {nid}")

        for unlock_type in ["unlocks_cadence", "unlocks_ability", "provides_ability", "unlocks_location"]:
            if unlock_type in out_edges:
                for edge in out_edges[unlock_type]:
                    edge_lines.append(f"{nid} ==>|Unlocks| {edge['targetId']}")

        for reward_type in ["rewards", "produces", "rewards_item"]:
            if reward_type in out_edges:
                for edge in out_edges[reward_type]:
                    target_id = edge["targetId"]
                    # IGNORE GOLD EDGES
                    if target_id == "item_gold": continue
                    
                    qty = edge.get("quantity", 1)
                    edge_lines.append(f"{nid} --o|Gives {qty}| {target_id}")

        if "consumes" in out_edges:
            for edge in out_edges["consumes"]:
                target_id = edge["targetId"]
                # IGNORE GOLD EDGES
                if target_id == "item_gold": continue
                
                qty = edge.get("quantity", 1)
                edge_lines.append(f"{target_id} --x|Consumes {qty}| {nid}")

    # Build final output
    for node_def in globals:
        lines.append(node_def)
        
    for loc, nodes_in_loc in by_location.items():
        lines.append(f'subgraph "Location: {loc}"')
        lines.extend(nodes_in_loc)
        lines.append("end")
        
    for cad, nodes_in_cad in by_cadence.items():
        lines.append(f'subgraph "Cadence: {cad}"')
        lines.extend(nodes_in_cad)
        lines.append("end")
        
    lines.extend(edge_lines)
    
    return "\n".join(lines)

if __name__ == "__main__":
    print(generate_mermaid())
