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
    abilities_meta = load_json("cadence_abilities.json")

    print(f"Loaded {len(items)} items, {len(quests)} quests, {len(locations)} locations...")

    # Ability info map
    ab_map = { a["Name"]: a for a in abilities_meta }

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
                "primary_stat": detail.get("PrimaryStat", "Vitality"),
                "required_stats": detail.get("RequiredStats"),
                "stat_rewards": detail.get("StatRewards")
            },
            "in_edges": {},
            "out_edges": {}
        }

        if "Effects" in detail:
            node["effects"] = detail["Effects"]

        # Requirements (In-Edges)
        reqs = []
        if q_name in unlocks_map:
            for req_quest in unlocks_map[q_name]:
                reqs.append(generate_id("quest", req_quest))
        if reqs:
            node["in_edges"]["requires_quest"] = reqs

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
            ab_info = ab_map.get(abil["Ability"], {})

            ab_node = {
                "id": ab_id,
                "type": "Ability",
                "name": abil["Ability"],
                "data": { 
                    "description": ab_info.get("Description", ""),
                    "primary_stat": abil.get("PrimaryStat", "Magic"), 
                    "metadata": abil.get("Metadata", {}) 
                },
                "in_edges": {}, 
                "out_edges": {}
            }

            if "Effects" in abil:
                ab_node["effects"] = abil["Effects"]
            
            # Ability Requirements (Unlock Cost)
            consumes = []
            for req in abil.get("Requirements", []):
                consumes.append({ "targetId": generate_id("item", req["Item"]), "quantity": req["Quantity"] })
            if consumes:
                ab_node["out_edges"]["consumes"] = consumes

            # Shared ability node check
            if not any(n["id"] == ab_id for n in nodes):
                nodes.append(ab_node)
            
            node["out_edges"]["provides_ability"].append({ "targetId": ab_id, "quantity": 1 })

        nodes.append(node)

    # 6. Refinements (Workshop)
    for ref in refinements:
        ab_id = generate_id("ability", ref["Ability"])
        
        for recipe in ref["Recipes"]:
            r_name = f"{ref['Ability']} - {recipe['OutputItem']}"
            r_id = generate_id("recipe", f"{ref['Ability']}_{recipe['InputItem']}_{recipe['OutputItem']}")

            node = {
                "id": r_id,
                "type": "Refinement",
                "name": r_name,
                "data": { "primary_stat": ref.get("PrimaryStat", "Strength") },
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
