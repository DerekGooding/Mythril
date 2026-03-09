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
    
    # Define styles
    lines.append("classDef quest fill:#4b0082,stroke:#f9f,stroke-width:2px,color:#fff;")
    lines.append("classDef location fill:#00008b,stroke:#ccf,stroke-width:2px,color:#fff;")
    lines.append("classDef cadence fill:#006400,stroke:#cfc,stroke-width:2px,color:#fff;")
    lines.append("classDef root fill:#8b8b00,stroke:#ff9,stroke-width:4px,color:#fff;")

    # Filter for significant nodes to avoid clutter
    # Focus on Quests, Locations, Cadences
    significant_types = ["Quest", "Location", "Cadence"]
    sig_nodes = {n["id"]: n for n in nodes if n["type"] in significant_types}

    for nid, node in sig_nodes.items():
        # Node Label
        name = node["name"].replace('"', "'")
        style = ""
        
        if node["name"] == "Prologue":
            style = ":::root"
        elif node["type"] == "Quest":
            style = ":::quest"
        elif node["type"] == "Location":
            style = ":::location"
        elif node["type"] == "Cadence":
            style = ":::cadence"

        lines.append(f'{nid}["{name}"]{style}')

        # 2. Location Unlocks (Quest -> Location)
        if "unlocks_location" in node["out_edges"]:
            for edge in node["out_edges"]["unlocks_location"]:
                target_id = edge["targetId"]
                if target_id in sig_nodes:
                    lines.append(f"{nid} ==>|Unlocks| {target_id}")

        # 3. Requirements (Location -> Quest)
        if "requires_quest" in node["in_edges"]:
            for req_id in node["in_edges"]["requires_quest"]:
                if req_id in sig_nodes:
                    # Avoid duplicate edges with 'requires_quest' which is also on Quest nodes.
                    if node["type"] == "Location":
                        lines.append(f"{req_id} -->|Enables| {nid}")
                    elif node["type"] == "Quest":
                        lines.append(f"{req_id} -->|Requires| {nid}")

        # 4. Cadence Unlocks (Quest -> Cadence)
        if "unlocks_cadence" in node["out_edges"]:
            for edge in node["out_edges"]["unlocks_cadence"]:
                target_id = edge["targetId"]
                if target_id in sig_nodes:
                    lines.append(f"{nid} ==>|Unlocks| {target_id}")

    # Output
    return "\n".join(lines)

if __name__ == "__main__":
    print(generate_mermaid())
