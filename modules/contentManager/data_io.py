import json
import os
import shutil
from datetime import datetime
from copy import deepcopy
from validator import ContentValidator

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
        self.unified_data["abilities"] = [deepcopy(a) for a in self.raw_data["cadence_abilities.json"]]

    def _unify_items(self):
        augments = { a["Item"]: a["Augments"] for a in self.raw_data["stat_augments.json"] }
        self.unified_data["items"] = []
        for item in self.raw_data["items.json"]:
            item_copy = deepcopy(item)
            item_copy["Augments"] = augments.get(item["Name"], [])
            self.unified_data["items"].append(item_copy)

    def _unify_quests(self):
        details = { d["Quest"]: d for d in self.raw_data["quest_details.json"] }
        unlocks = { u["Quest"]: u["Requires"] for u in self.raw_data["quest_unlocks.json"] }
        cadence_unlocks = { q["Quest"]: q["Cadences"] for q in self.raw_data["quest_cadence_unlocks.json"] }
        
        # Build map of quest name -> location name
        quest_to_loc = {}
        for loc in self.raw_data.get("locations.json", []):
            for q_name in loc.get("Quests", []):
                quest_to_loc[q_name] = loc["Name"]

        self.unified_data["quests"] = []
        for quest in self.raw_data["quests.json"]:
            q_name = quest["Name"]
            q_detail = details.get(q_name, {})
            
            unified_q = {
                "Name": q_name,
                "Description": quest.get("Description", ""),
                "DurationSeconds": q_detail.get("DurationSeconds", 10),
                "Type": q_detail.get("Type", "Single"),
                "Requirements": deepcopy(q_detail.get("Requirements", [])),
                "Rewards": deepcopy(q_detail.get("Rewards", [])),
                "PrimaryStat": q_detail.get("PrimaryStat", "Vitality"),
                "RequiredStats": deepcopy(q_detail.get("RequiredStats", {})),
                "StatRewards": deepcopy(q_detail.get("StatRewards", {})),
                "Requires": deepcopy(unlocks.get(q_name, [])),
                "UnlocksCadences": deepcopy(cadence_unlocks.get(q_name, [])),
                "Effects": deepcopy(q_detail.get("Effects", [])),
                "Location": quest_to_loc.get(q_name, "None")
            }
            self.unified_data["quests"].append(unified_q)

    def _unify_cadences(self):
        self.unified_data["cadences"] = [deepcopy(c) for c in self.raw_data["cadences.json"]]

    def _unify_locations(self):
        self.unified_data["locations"] = [deepcopy(l) for l in self.raw_data["locations.json"]]

    def _unify_refinements(self):
        self.unified_data["refinements"] = []
        for r in self.raw_data["refinements.json"]:
            rc = deepcopy(r)
            rc["Name"] = r["Ability"]
            self.unified_data["refinements"].append(rc)

    def _unify_stats(self):
        self.unified_data["stats"] = [deepcopy(s) for s in self.raw_data["stats.json"]]


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
        
        # Build quest groups by location for locations.json
        loc_to_quests = {}

        for q in self.unified_data["quests"]:
            new_quests.append({ "Name": q["Name"], "Description": q["Description"] })
            detail = {
                "Quest": q["Name"],
                "DurationSeconds": q["DurationSeconds"],
                "Type": q["Type"],
                "Requirements": q["Requirements"],
                "Rewards": q["Rewards"],
                "PrimaryStat": q["PrimaryStat"],
                "RequiredStats": q["RequiredStats"],
                "StatRewards": q["StatRewards"]
            }
            if q.get("Effects"):
                detail["Effects"] = q["Effects"]
            new_details.append(detail)
            
            if q.get("Requires"):
                new_unlocks.append({ "Quest": q["Name"], "Requires": q["Requires"] })
            if q.get("UnlocksCadences"):
                new_cadence_unlocks.append({ "Quest": q["Name"], "Cadences": q["UnlocksCadences"] })

            # Track location membership
            loc_name = q.get("Location", "None")
            if loc_name != "None":
                if loc_name not in loc_to_quests: loc_to_quests[loc_name] = []
                loc_to_quests[loc_name].append(q["Name"])

        new_refinements = []
        for r in self.unified_data["refinements"]:
            rc = { k: v for k, v in r.items() if k != "Name" }
            rc["Ability"] = r["Name"] # Ensure Ability is updated if Name was changed
            new_refinements.append(rc)

        # Update locations based on quest assignments
        updated_locations = []
        for loc in self.unified_data["locations"]:
            lc = deepcopy(loc)
            lc["Quests"] = loc_to_quests.get(loc["Name"], [])
            updated_locations.append(lc)

        self._save_json("items.json", new_items)
        self._save_json("stat_augments.json", new_augments)
        self._save_json("quests.json", new_quests)
        self._save_json("quest_details.json", new_details)
        self._save_json("quest_unlocks.json", new_unlocks)
        self._save_json("quest_cadence_unlocks.json", new_cadence_unlocks)
        self._save_json("cadences.json", self.unified_data["cadences"])
        self._save_json("locations.json", updated_locations)
        self._save_json("refinements.json", new_refinements)
        self._save_json("stats.json", self.unified_data["stats"])
        self._save_json("cadence_abilities.json", self.unified_data["abilities"])

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
        return ContentValidator.get_warnings(self.unified_data)
