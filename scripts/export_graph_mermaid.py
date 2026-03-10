import json
import os
import sys
import re

GRAPH_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"

def load_graph():
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def clean_label(label):
    if not label: return "Untitled"
    # Rigorous cleaning
    label = label.replace('"', "'").replace(":", "-").replace("[", "(").replace("]", ")")
    label = "".join(c for c in label if c.isprintable() or c == " ")
    label = " ".join(label.split()).strip()
    return label if label else "Untitled"

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
    by_location = {} # LocName -> [NodeIDs]
    by_cadence = {}  # CadName -> [NodeIDs]
    
    lines = ["flowchart TD"]
    
    # Styles
    lines.append("classDef quest fill:#4b0082,stroke:#f9f,stroke-width:2px,color:#fff")
    lines.append("classDef location fill:#00008b,stroke:#ccf,stroke-width:2px,color:#fff")
    lines.append("classDef cadence fill:#006400,stroke:#cfc,stroke-width:2px,color:#fff")
    lines.append("classDef item fill:#444,stroke:#fff,stroke-width:1px,color:#fff")
    lines.append("classDef stat fill:#8b4513,stroke:#ff8c00,stroke-width:1px,color:#fff")
    lines.append("classDef ability fill:#2f4f4f,stroke:#00ced1,stroke-width:1px,color:#fff")
    lines.append("classDef refinement fill:#ff4500,stroke:#fff,stroke-width:1px,color:#fff")
    lines.append("classDef root fill:#8b8b00,stroke:#ff9,stroke-width:4px,color:#fff")

    edge_lines = []
    processed_ids = set()
    node_defs = []
    
    # Pre-define stat nodes
    for stat in ["Strength", "Vitality", "Magic", "Speed"]:
        stat_id = f"stat_{stat.lower()}"
        node_defs.append(f'{stat_id}["{stat}"]:::stat')
        processed_ids.add(stat_id)
    
    for node in nodes:
        nid = node["id"].strip()
        ntype = node["type"]
        
        # IGNORE GOLD
        if nid == "item_gold":
            continue
            
        name = clean_label(node["name"])
        style = ntype.lower()
        if nid == "quest_prologue": style = "root"
        
        node_def = f'{nid}["{name}"]:::{style}'
        node_defs.append(node_def)
        processed_ids.add(nid)
        
        # Assign to group
        if ntype == "Quest" and nid in location_map:
            loc = clean_label(location_map[nid])
            if loc not in by_location: by_location[loc] = []
            by_location[loc].append(nid)
        elif ntype == "Ability" and nid in cadence_map:
            cad = clean_label(cadence_map[nid])
            if cad not in by_cadence: by_cadence[cad] = []
            by_cadence[cad].append(nid)

        # Edges (same as before)
        in_edges = node.get("in_edges", {})
        out_edges = node.get("out_edges", {})
        
        if "requires_quest" in in_edges:
            for req_id in in_edges["requires_quest"]:
                label = "Enables" if ntype == "Location" else "Requires"
                edge_lines.append(f'{req_id} -->|"{label}"| {nid}')
                    
        if "requires_ability" in in_edges:
            for req_id in in_edges["requires_ability"]:
                edge_lines.append(f'{req_id} -->|"Allows"| {nid}')
                
        if "requires_stat" in in_edges:
            for stat_name, val in in_edges["requires_stat"].items():
                stat_id = f"stat_{stat_name.lower()}"
                edge_lines.append(f'{stat_id} -->|"Req {val}"| {nid}')

        for unlock_type in ["unlocks_cadence", "unlocks_ability", "provides_ability", "unlocks_location"]:
            if unlock_type in out_edges:
                for edge in out_edges[unlock_type]:
                    edge_lines.append(f'{nid} -->|"Unlocks"| {edge["targetId"]}')

        for reward_type in ["rewards", "produces", "rewards_item"]:
            if reward_type in out_edges:
                for edge in out_edges[reward_type]:
                    target_id = edge["targetId"]
                    if target_id == "item_gold": continue
                    qty = edge.get("quantity", 1)
                    edge_lines.append(f'{nid} --o|"Gives {qty}"| {target_id}')

        if "consumes" in out_edges:
            for edge in out_edges["consumes"]:
                target_id = edge["targetId"]
                if target_id == "item_gold": continue
                qty = edge.get("quantity", 1)
                edge_lines.append(f'{target_id} --x|"Consumes {qty}"| {nid}')

    # Build final output
    lines.extend(node_defs)
        
    for loc, nodes_in_loc in by_location.items():
        # Sanitize loc for ID
        loc_id = "loc_" + re.sub(r'\W+', '_', loc.lower())
        lines.append(f'subgraph {loc_id} ["Location - {loc}"]')
        lines.extend(nodes_in_loc)
        lines.append("end")
        
    for cad, nodes_in_cad in by_cadence.items():
        # Sanitize cad for ID
        cad_id = "cad_" + re.sub(r'\W+', '_', cad.lower())
        lines.append(f'subgraph {cad_id} ["Cadence - {cad}"]')
        lines.extend(nodes_in_cad)
        lines.append("end")
        
    # Final filter to ensure no edges to missing nodes and no self-edges
    valid_edges = []
    for edge in edge_lines:
        # Simple extraction of IDs from "id1 --o|...| id2" or "id1 --x|...| id2" or "id1 -->|...| id2"
        match = re.search(r'^(\w+)\s*--[->ox]\s*\|".*?"\|\s*(\w+)$', edge)
        if match:
            source_id = match.group(1)
            target_id = match.group(2)
            if source_id in processed_ids and target_id in processed_ids:
                if source_id != target_id:
                    valid_edges.append(edge)
                else:
                    print(f"Filtered out self-edge: {source_id} --> {target_id}")
    
    lines.extend(valid_edges)
    
    return "\n".join(lines)

if __name__ == "__main__":
    mermaid = generate_mermaid()
    if len(sys.argv) > 1:
        with open(sys.argv[1], 'w', encoding='utf-8') as f:
            f.write(mermaid)
    else:
        print(mermaid)
