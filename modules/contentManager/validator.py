
class ContentValidator:
    @staticmethod
    def get_warnings(unified_data):
        warnings = []
        items = { i["Name"]: i for i in unified_data["items"] }
        quests = unified_data["quests"]
        refinements = unified_data["refinements"]
        locations = unified_data["locations"]
        cadences = unified_data["cadences"]

        produced_items = set()
        consumed_items = set()

        # Track production
        for q in quests:
            for r in q.get("Rewards", []): produced_items.add(r["Item"])
        for ref in refinements:
            for rec in ref.get("Recipes", []): produced_items.add(rec["OutputItem"])

        # Track consumption
        for q in quests:
            for r in q.get("Requirements", []): consumed_items.add(r["Item"])
        for ref in refinements:
            for rec in ref.get("Recipes", []): consumed_items.add(rec["InputItem"])
        for c in cadences:
            for ab in c.get("Abilities", []):
                for req in ab.get("Requirements", []): consumed_items.add(req["Item"])

        # 1. Items with no source
        for item_name in items:
            if item_name not in produced_items and items[item_name]["ItemType"] not in ["Currency", "KeyItem"]:
                warnings.append({ "type": "Item", "name": item_name, "msg": "No generation source (never rewarded or refined)." })

        # 2. Items with no sink
        for item_name in produced_items:
            has_sink = (
                item_name in consumed_items or 
                (item_name in items and items[item_name]["ItemType"] in ["Currency", "KeyItem", "Consumable"]) or
                (item_name in items and len(items[item_name].get("Augments", [])) > 0)
            )
            
            if not has_sink:
                warnings.append({ "type": "Item", "name": item_name, "msg": "Has source but no sink (nothing to spend it on and cannot be junctioned)." })

        # 3. Useless recurring quests
        for q in quests:
            if q["Type"] == "Recurring":
                has_output = len(q.get("Rewards", [])) > 0 or len(q.get("StatRewards", {})) > 0
                is_prereq = any(q["Name"] in other.get("Requires", []) for other in quests)
                unlocks_cadence = len(q.get("UnlocksCadences", [])) > 0
                unlocks_loc = any(l.get("RequiredQuest") == q["Name"] for l in locations)
                
                if not has_output and not is_prereq and not unlocks_cadence and not unlocks_loc:
                    warnings.append({ "type": "Quest", "name": q["Name"], "msg": "Recurring quest that neither produces resources nor unlocks content." })

        return warnings
