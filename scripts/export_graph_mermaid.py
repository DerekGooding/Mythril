import json
import os
import sys

GRAPH_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"

def load_graph():
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def generate_mermaid():
    nodes = load_graph()
    
    lines = ["graph TD"]
    
    # Define styles with better contrast
    lines.append("classDef quest fill:#4b0082,stroke:#f9f,stroke-width:2px,color:#fff;")
    lines.append("classDef location fill:#00008b,stroke:#ccf,stroke-width:2px,color:#fff;")
    lines.append("classDef cadence fill:#006400,stroke:#cfc,stroke-width:2px,color:#fff;")
    lines.append("classDef item fill:#444,stroke:#fff,stroke-width:1px,color:#fff;")
    lines.append("classDef stat fill:#8b4513,stroke:#ff8c00,stroke-width:1px,color:#fff;")
    lines.append("classDef ability fill:#2f4f4f,stroke:#00ced1,stroke-width:1px,color:#fff;")
    lines.append("classDef root fill:#8b8b00,stroke:#ff9,stroke-width:4px,color:#fff;")

    # We want to show almost everything now
    # Filter out some very common ones if they make it too noisy, 
    # but for now let's include all for a "Complete Map"
    
    for node in nodes:
        nid = node["id"]
        ntype = node["type"]
        name = node["name"]
        
        # 1. Define Node
        style = ntype.lower()
        if nid == "quest_prologue":
            style = "root"
            
        # Shorten some names for the graph
        display_name = name.replace('"', "'")
        lines.append(f'{nid}["{display_name}"]:::{style}')

        # 2. In-Edges (Requirements)
        in_edges = node.get("in_edges", {})
        
        # Quest Requirements
        if "requires_quest" in in_edges:
            for req_id in in_edges["requires_quest"]:
                # If Location requires Quest, it's an "Enables" link
                if ntype == "Location":
                    lines.append(f"{req_id} -->|Enables| {nid}")
                else:
                    lines.append(f"{req_id} -->|Requires| {nid}")
                    
        # Ability Requirements (for Refinements usually)
        if "requires_ability" in in_edges:
            for req_id in in_edges["requires_ability"]:
                lines.append(f"{req_id} -.->|Allows| {nid}")
                
        # Stat Requirements
        if "requires_stat" in in_edges:
            for stat_name, val in in_edges["requires_stat"].items():
                stat_id = f"stat_{stat_name.lower()}"
                lines.append(f"{stat_id} -.->|Requires {val}| {nid}")

        # 3. Out-Edges (Unlocks / Rewards / Productions)
        out_edges = node.get("out_edges", {})
        
        # Cadence/Ability Unlocks
        for unlock_type in ["unlocks_cadence", "unlocks_ability", "provides_ability"]:
            if unlock_type in out_edges:
                for edge in out_edges[unlock_type]:
                    target_id = edge["targetId"]
                    lines.append(f"{nid} ==>|Unlocks| {target_id}")
                    
        # Location Unlocks (explicit)
        if "unlocks_location" in out_edges:
            for edge in out_edges["unlocks_location"]:
                target_id = edge["targetId"]
                lines.append(f"{nid} ==>|Unlocks| {target_id}")

        # Item Rewards / Production
        for reward_type in ["rewards", "produces", "rewards_item"]:
            if reward_type in out_edges:
                for edge in out_edges[reward_type]:
                    target_id = edge["targetId"]
                    qty = edge.get("quantity", 1)
                    lines.append(f"{nid} --o|Gives {qty}| {target_id}")

        # Item Consumption
        if "consumes" in out_edges:
            for edge in out_edges["consumes"]:
                target_id = edge["targetId"]
                qty = edge.get("quantity", 1)
                # Link Item to the consumer
                lines.append(f"{target_id} --x|Consumes {qty}| {nid}")

        # Containment (Optional, but useful for locations)
        if ntype == "Location" and "contains" in out_edges:
            for edge in out_edges["contains"]:
                target_id = edge["targetId"]
                # lines.append(f"{nid} -.->|Home of| {target_id}") # Usually too noisy

    return "\n".join(lines)

if __name__ == "__main__":
    print(generate_mermaid())
