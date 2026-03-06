import json
import os
import hashlib

DATA_DIR = "Mythril.Blazor/wwwroot/data"
OUTPUT_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"

def load_json(filename):
    with open(os.path.join(DATA_DIR, filename), 'r', encoding='utf-8') as f:
        return json.load(f)

def generate_id(type, name):
    # Create a consistent ID: type_clean_name
    clean_name = name.lower().replace(' ', '_').replace('-', '_').replace("'", "")
    return f"{type.lower()}_{clean_name}"

def migrate():
    nodes = []
    
    # Load all source data
    items = load_json("items.json")
    quests = load_json("quests.json")
    locations = load_json("locations.json")
    cadences = load_json("cadences.json")
    quest_details = load_json("quest_details.json")
    quest_unlocks = load_json("quest_unlocks.json")
    quest_cadence = load_json("quest_cadence_unlocks.json")
    refinements = load_json("refinements.json")
    stats = load_json("stats.json")

    print(f"Loaded {len(items)} items, {len(quests)} quests, {len(locations)} locations...")

    # 1. Items
    for item in items:
        node = {
            "id": generate_id("item", item["Name"]),
            "type": "Item",
            "name": item["Name"],
            "data": { "description": item.get("Description", ""), "item_type": item.get("ItemType", "Material") },
            "in_edges": {},
            "out_edges": {}
        }
        nodes.append(node)

    # 2. Stats
    for stat in stats:
        node = {
            "id": generate_id("stat", stat["Name"]),
            "type": "Stat",
            "name": stat["Name"],
            "data": { "description": stat["Description"] },
            "in_edges": {},
            "out_edges": {}
        }
        nodes.append(node)

    # 3. Quests
    # Convert lists to dicts for easier lookup
    details_map = {d["Quest"]: d for d in quest_details}
    unlocks_map = {u["Quest"]: u["Requires"] for u in quest_unlocks}
    cadence_unlock_map = {q["Quest"]: q["Cadences"] for q in quest_cadence}

    for quest in quests:
        q_name = quest["Name"]
        q_id = generate_id("quest", q_name)
        detail = details_map.get(q_name, {})
        
        node = {
            "id": q_id,
            "type": "Quest",
            "name": q_name,
            "data": {
                "description": quest["Description"],
                "duration": detail.get("DurationSeconds", 10),
                "quest_type": detail.get("Type", "Single"),
                "primary_stat": detail.get("PrimaryStat", "Vitality")
            },
            "in_edges": {},
            "out_edges": {}
        }

        # Requirements (In-Edges)
        reqs = []
        if q_name in unlocks_map:
            for req_quest in unlocks_map[q_name]:
                reqs.append(generate_id("quest", req_quest))
        if reqs:
            node["in_edges"]["requires_quest"] = reqs

        item_reqs = []
        if "Requirements" in detail:
            for req in detail["Requirements"]:
                item_reqs.append(generate_id("item", req["Item"])) # Simplified for now, quantity needs edge property
                # Ideally edge logic: out_edges: { "consumes": [ { target: "item_id", qty: 5 } ] }
                # But for migration, we are just mapping IDs first.
        
        # Item Costs (Consumes)
        consumes = []
        if "Requirements" in detail:
            for req in detail["Requirements"]:
                consumes.append({ "targetId": generate_id("item", req["Item"]), "quantity": req["Quantity"] })
        if consumes:
            node["out_edges"]["consumes"] = consumes

        # Rewards (Produces)
        rewards = []
        if "Rewards" in detail:
            for rew in detail["Rewards"]:
                rewards.append({ "targetId": generate_id("item", rew["Item"]), "quantity": rew["Quantity"] })
        if rewards:
            node["out_edges"]["rewards"] = rewards

        # Cadence Unlocks (Out-Edges)
        if q_name in cadence_unlock_map:
            unlocks = []
            for cad_name in cadence_unlock_map[q_name]:
                unlocks.append({ "targetId": generate_id("cadence", cad_name), "quantity": 1 })
            if unlocks:
                node["out_edges"]["unlocks_cadence"] = unlocks

        nodes.append(node)

    # 4. Locations
    for loc in locations:
        l_id = generate_id("location", loc["Name"])
        node = {
            "id": l_id,
            "type": "Location",
            "name": loc["Name"],
            "data": { "region_type": loc.get("Type", "Plains") },
            "in_edges": {},
            "out_edges": {}
        }

        # Location Gating
        if "RequiredQuest" in loc and loc["RequiredQuest"]:
            node["in_edges"]["requires_quest"] = [generate_id("quest", loc["RequiredQuest"])]

        # Contains Quests
        contains = []
        for q_name in loc.get("Quests", []):
            contains.append({ "targetId": generate_id("quest", q_name), "quantity": 1 })
        if contains:
            node["out_edges"]["contains"] = contains

        nodes.append(node)

    # 5. Cadences
    for cad in cadences:
        c_id = generate_id("cadence", cad["Name"])
        node = {
            "id": c_id,
            "type": "Cadence",
            "name": cad["Name"],
            "data": { "description": cad["Description"] },
            "in_edges": {},
            "out_edges": { "provides_ability": [] }
        }

        for abil in cad.get("Abilities", []):
            # Ability Node
            ab_id = generate_id("ability", abil["Ability"])
            ab_node = {
                "id": ab_id,
                "type": "Ability",
                "name": abil["Ability"],
                "data": { "primary_stat": abil.get("PrimaryStat", "Magic"), "metadata": abil.get("Metadata", {}) },
                "in_edges": {}, # Requirements handled on edge or separate? 
                "out_edges": {}
            }
            
            # Ability Requirements (Unlock Cost)
            consumes = []
            for req in abil.get("Requirements", []):
                consumes.append({ "targetId": generate_id("item", req["Item"]), "quantity": req["Quantity"] })
            if consumes:
                ab_node["out_edges"]["consumes"] = consumes

            # Check if duplicate ability node already exists (shared abilities?)
            # Usually abilities are unique per cadence in Mythril or shared by name.
            # We'll treat them as unique nodes for now, or check existence.
            if not any(n["id"] == ab_id for n in nodes):
                nodes.append(ab_node)
            
            node["out_edges"]["provides_ability"].append({ "targetId": ab_id, "quantity": 1 })

        nodes.append(node)

    # 6. Refinements (Workshop)
    for ref in refinements:
        # Refinement is usually bound to an ability
        ab_id = generate_id("ability", ref["Ability"])
        
        # Find the ability node to attach refinement data or create refinement nodes?
        # Let's create Recipe Nodes linked to the Ability.
        
        for recipe in ref["Recipes"]:
            r_name = f"{ref['Ability']}: {recipe['OutputItem']}"
            # Unique ID including input item to avoid collisions for multiple recipes for same ability/output
            r_id = generate_id("recipe", f"{ref['Ability']}_{recipe['InputItem']}_{recipe['OutputItem']}")

            node = {
                "id": r_id,
                "type": "Refinement",
                "name": r_name,                "data": { "primary_stat": ref.get("PrimaryStat", "Strength") },
                "in_edges": {
                    "requires_ability": [ab_id]
                },
                "out_edges": {
                    "consumes": [{ "targetId": generate_id("item", recipe["InputItem"]), "quantity": recipe["InputQuantity"] }],
                    "produces": [{ "targetId": generate_id("item", recipe["OutputItem"]), "quantity": recipe["OutputQuantity"] }]
                }
            }
            nodes.append(node)

    # Write Output
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        json.dump(nodes, f, indent=2)

    print(f"Migration complete. Graph saved to {OUTPUT_FILE} with {len(nodes)} nodes.")

if __name__ == "__main__":
    migrate()
