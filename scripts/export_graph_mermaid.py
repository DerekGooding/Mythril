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
    lines.append("classDef quest fill:#f9f,stroke:#333,stroke-width:2px;")
    lines.append("classDef location fill:#ccf,stroke:#333,stroke-width:2px;")
    lines.append("classDef cadence fill:#cfc,stroke:#333,stroke-width:2px;")
    lines.append("classDef root fill:#ff9,stroke:#333,stroke-width:4px;")

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

        # Edges
        # 1. Requirements (Quest -> Quest)
        if "requires_quest" in node["in_edges"]:
            for req_id in node["in_edges"]["requires_quest"]:
                if req_id in sig_nodes:
                    lines.append(f"{req_id} -->|Requires| {nid}")

        # 2. Location Unlocks (Quest -> Location) - Actually Location requires Quest in gating
        # In our graph, Location has in_edge requires_quest. 
        # Mermaid: Quest --> Location
        # Wait, Location -> Quest means Location requires Quest to enter. 
        # So Quest -> Location flow is correct for progression.
        
        # 3. Quest containment (Location -> Quest)
        # Location contains Quest
        if "contains" in node["out_edges"]:
            for edge in node["out_edges"]["contains"]:
                target_id = edge["targetId"]
                if target_id in sig_nodes:
                    # lines.append(f"{nid} -.->|Contains| {target_id}") # Optional: Can be noisy
                    pass

        # 4. Cadence Unlocks (Quest -> Cadence)
        if "unlocks_cadence" in node["out_edges"]:
            for edge in node["out_edges"]["unlocks_cadence"]:
                target_id = edge["targetId"]
                if target_id in sig_nodes:
                    lines.append(f"{nid} ==>|Unlocks| {target_id}")

    # Output
    print("\n".join(lines))

if __name__ == "__main__":
    generate_mermaid()
