import json
import os
import shutil
from datetime import datetime

# Resolve DATA_DIR relative to this file's location (modules/contentManager/data_io.py)
# Root is two levels up from this file
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
DATA_DIR = os.path.join(PROJECT_ROOT, "Mythril.Blazor", "wwwroot", "data")

class ContentManager:
    def __init__(self, data_dir=DATA_DIR):
        self.data_dir = data_dir
        self.raw_data = {}
        self.unified_data = {
            "items": [],
            "quests": [],
            "cadences": [],
            "locations": [],
            "refinements": [],
            "stats": [],
            "abilities": []
        }
        self.load_all()

    def _load_json(self, filename):
        path = os.path.join(self.data_dir, filename)
        if not os.path.exists(path):
            return []
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)

    def load_all(self):
        # Load raw files
        files = [
            "items.json", "stat_augments.json", "quests.json", 
            "quest_details.json", "quest_unlocks.json", 
            "quest_cadence_unlocks.json", "cadences.json", 
            "locations.json", "refinements.json", "stats.json",
            "cadence_abilities.json"
        ]
        for f in files:
            self.raw_data[f] = self._load_json(f)

        self._unify_items()
        self._unify_quests()
        self._unify_cadences()
        self._unify_locations()
        self._unify_refinements()
        self._unify_stats()
        self._unify_abilities()

    def _unify_abilities(self):
        self.unified_data["abilities"] = [a.copy() for a in self.raw_data["cadence_abilities.json"]]

    def _unify_items(self):
        augments = { a["Item"]: a["Augments"] for a in self.raw_data["stat_augments.json"] }
        self.unified_data["items"] = []
        for item in self.raw_data["items.json"]:
            item_copy = item.copy()
            item_copy["Augments"] = augments.get(item["Name"], [])
            self.unified_data["items"].append(item_copy)

    def _unify_quests(self):
        details = { d["Quest"]: d for d in self.raw_data["quest_details.json"] }
        unlocks = { u["Quest"]: u["Requires"] for u in self.raw_data["quest_unlocks.json"] }
        cadence_unlocks = { q["Quest"]: q["Cadences"] for q in self.raw_data["quest_cadence_unlocks.json"] }
        
        self.unified_data["quests"] = []
        for quest in self.raw_data["quests.json"]:
            q_name = quest["Name"]
            q_detail = details.get(q_name, {})
            
            unified_q = {
                "Name": q_name,
                "Description": quest.get("Description", ""),
                "DurationSeconds": q_detail.get("DurationSeconds", 10),
                "Type": q_detail.get("Type", "Single"),
                "Requirements": q_detail.get("Requirements", []),
                "Rewards": q_detail.get("Rewards", []),
                "PrimaryStat": q_detail.get("PrimaryStat", "Vitality"),
                "RequiredStats": q_detail.get("RequiredStats", {}),
                "StatRewards": q_detail.get("StatRewards", {}),
                "Requires": unlocks.get(q_name, []),
                "UnlocksCadences": cadence_unlocks.get(q_name, [])
            }
            self.unified_data["quests"].append(unified_q)

    def _unify_cadences(self):
        self.unified_data["cadences"] = [c.copy() for c in self.raw_data["cadences.json"]]

    def _unify_locations(self):
        self.unified_data["locations"] = [l.copy() for l in self.raw_data["locations.json"]]

    def _unify_refinements(self):
        self.unified_data["refinements"] = []
        for r in self.raw_data["refinements.json"]:
            rc = r.copy()
            rc["Name"] = r["Ability"]
            self.unified_data["refinements"].append(rc)

    def _unify_stats(self):
        self.unified_data["stats"] = [s.copy() for s in self.raw_data["stats.json"]]

    def save_all(self):
        # Create backup
        backup_dir = os.path.join(self.data_dir, "backups", datetime.now().strftime("%Y%m%d_%H%M%S"))
        os.makedirs(backup_dir, exist_ok=True)
        for f in self.raw_data.keys():
            src = os.path.join(self.data_dir, f)
            if os.path.exists(src):
                shutil.copy(src, os.path.join(backup_dir, f))

        # Split unified data back into files
        new_items = []
        new_augments = []
        for item in self.unified_data["items"]:
            new_items.append({ "Name": item["Name"], "Description": item["Description"], "ItemType": item["ItemType"] })
            if item.get("Augments"):
                new_augments.append({ "Item": item["Name"], "Augments": item["Augments"] })

        new_quests = []
        new_details = []
        new_unlocks = []
        new_cadence_unlocks = []
        for q in self.unified_data["quests"]:
            new_quests.append({ "Name": q["Name"], "Description": q["Description"] })
            new_details.append({
                "Quest": q["Name"],
                "DurationSeconds": q["DurationSeconds"],
                "Type": q["Type"],
                "Requirements": q["Requirements"],
                "Rewards": q["Rewards"],
                "PrimaryStat": q["PrimaryStat"],
                "RequiredStats": q["RequiredStats"],
                "StatRewards": q["StatRewards"]
            })
            if q.get("Requires"):
                new_unlocks.append({ "Quest": q["Name"], "Requires": q["Requires"] })
            if q.get("UnlocksCadences"):
                new_cadence_unlocks.append({ "Quest": q["Name"], "Cadences": q["UnlocksCadences"] })

        new_refinements = []
        for r in self.unified_data["refinements"]:
            rc = { k: v for k, v in r.items() if k != "Name" }
            rc["Ability"] = r["Name"] # Ensure Ability is updated if Name was changed
            new_refinements.append(rc)

        self._save_json("items.json", new_items)
        self._save_json("stat_augments.json", new_augments)
        self._save_json("quests.json", new_quests)
        self._save_json("quest_details.json", new_details)
        self._save_json("quest_unlocks.json", new_unlocks)
        self._save_json("quest_cadence_unlocks.json", new_cadence_unlocks)
        self._save_json("cadences.json", self.unified_data["cadences"])
        self._save_json("locations.json", self.unified_data["locations"])
        self._save_json("refinements.json", new_refinements)
        self._save_json("stats.json", self.unified_data["stats"])

        # Automatically recompile the content graph
        print("Recompiling content graph...")
        migrate_script = os.path.join(PROJECT_ROOT, "scripts", "migrate_to_graph.py")
        import subprocess
        try:
            subprocess.run(["python", migrate_script], check=True, cwd=PROJECT_ROOT)
            print("Content graph recompiled successfully.")
        except Exception as e:
            print(f"Error recompiling content graph: {e}")

    def _save_json(self, filename, data):
        path = os.path.join(self.data_dir, filename)
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)

    def get_warnings(self):
        warnings = []
        items = { i["Name"]: i for i in self.unified_data["items"] }
        quests = self.unified_data["quests"]
        refinements = self.unified_data["refinements"]
        locations = self.unified_data["locations"]
        cadences = self.unified_data["cadences"]

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
                # Check if it's a starting item in any quest requirement (might be provided by prologue)
                # But generally if it's not produced anywhere, it's a warning
                warnings.append({ "type": "Item", "name": item_name, "msg": "No generation source (never rewarded or refined)." })

        # 2. Items with no sink
        for item_name in produced_items:
            # An item has a sink if:
            # - It's consumed by something (quest, refinement, cadence)
            # - It's a special type (Currency, KeyItem, Consumable)
            # - It can be JUNCTIONED (has augments)
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
                
                # Check if it unlocks anything
                is_prereq = any(q["Name"] in other.get("Requires", []) for other in quests)
                unlocks_cadence = len(q.get("UnlocksCadences", [])) > 0
                unlocks_loc = any(l.get("RequiredQuest") == q["Name"] for l in locations)
                
                if not has_output and not is_prereq and not unlocks_cadence and not unlocks_loc:
                    warnings.append({ "type": "Quest", "name": q["Name"], "msg": "Recurring quest that neither produces resources nor unlocks content." })

        return warnings
