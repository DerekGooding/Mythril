import streamlit as st
import pandas as pd
from data_io import ContentManager, PROJECT_ROOT
import subprocess
import os

st.set_page_config(page_title="Mythril Content Manager", layout="wide")

if 'manager' not in st.session_state:
    st.session_state.manager = ContentManager()

manager = st.session_state.manager

st.sidebar.title("💎 Mythril CMS")
page = st.sidebar.selectbox("Navigate", ["Quests", "Items", "Cadences", "Locations", "Refinements", "Stats"])

if st.sidebar.button("💾 Save All Changes"):
    manager.save_all()
    st.sidebar.success("Saved to JSON files (Backup created)")

if st.sidebar.button("🔨 Compile & Verify"):
    with st.spinner("Compiling content graph..."):
        # Run from project root so scripts can find their own paths
        migrate_script = os.path.join(PROJECT_ROOT, "scripts", "migrate_to_graph.py")
        verify_script = os.path.join(PROJECT_ROOT, "scripts", "verify_graph.py")
        
        res1 = subprocess.run(["python", migrate_script], capture_output=True, text=True, cwd=PROJECT_ROOT)
        res2 = subprocess.run(["python", verify_script], capture_output=True, text=True, cwd=PROJECT_ROOT)
        
        if res1.returncode == 0:
            st.sidebar.success("Compiled successfully")
        else:
            st.sidebar.error(f"Migration Error: {res1.stderr}")
            
        if res2.returncode == 0:
            st.sidebar.success("Graph verified")
        else:
            st.sidebar.error(f"Verification Failed:\n{res2.stdout}")

st.sidebar.write("---")
st.sidebar.subheader("⚠️ Content Warnings")
warnings = manager.get_warnings()
if not warnings:
    st.sidebar.success("No issues detected!")
else:
    for w in warnings:
        st.sidebar.warning(f"**{w['type']} [{w['name']}]**: {w['msg']}")


def edit_list(data_list, key_prefix, item_pool=None):
    if item_pool is None:
        item_pool = [it["Name"] for i in [manager.unified_data["items"]] for it in i]
    
    to_delete = None
    for i, entry in enumerate(data_list):
        col1, col2, col3 = st.columns([3, 2, 1])
        with col1:
            current_index = 0
            if entry["Item"] in item_pool:
                current_index = item_pool.index(entry["Item"])
            
            new_name = st.selectbox(f"Item {i}", item_pool, index=current_index, key=f"{key_prefix}_name_{i}")
            entry["Item"] = new_name
            
        with col2:
            entry["Quantity"] = st.number_input(f"Qty {i}", value=entry["Quantity"], min_value=1, key=f"{key_prefix}_qty_{i}")
            
        with col3:
            if st.button("🗑️", key=f"{key_prefix}_del_{i}"):
                to_delete = i
                
    if to_delete is not None:
        data_list.pop(to_delete)
        st.rerun()
    
    if st.button("➕ Add Entry", key=f"{key_prefix}_add"):
        data_list.append({"Item": item_pool[0], "Quantity": 1})
        st.rerun()

def edit_dict_list(data_dict, key_prefix, label_name="Stat"):
    to_delete = None
    keys = list(data_dict.keys())
    for i, key in enumerate(keys):
        col1, col2, col3 = st.columns([3, 2, 1])
        with col1:
            stat_names = [s["Name"] for s in manager.unified_data["stats"]]
            current_index = 0
            if key in stat_names:
                current_index = stat_names.index(key)
            
            new_key = st.selectbox(f"{label_name} {i}", stat_names, index=current_index, key=f"{key_prefix}_key_{i}")
            if new_key != key:
                data_dict[new_key] = data_dict.pop(key)
                st.rerun()
                
        with col2:
            data_dict[new_key] = st.number_input(f"Value {i}", value=data_dict[new_key], key=f"{key_prefix}_val_{i}")
            
        with col3:
            if st.button("🗑️", key=f"{key_prefix}_del_{i}"):
                to_delete = key
                
    if to_delete is not None:
        del data_dict[to_delete]
        st.rerun()
    
    if st.button(f"➕ Add {label_name}", key=f"{key_prefix}_add"):
        available = [s["Name"] for s in manager.unified_data["stats"] if s["Name"] not in data_dict]
        if available:
            data_dict[available[0]] = 1
            st.rerun()

def edit_recipes(recipes, key_prefix):
    to_delete = None
    item_names = [i["Name"] for i in manager.unified_data["items"]]
    for i, recipe in enumerate(recipes):
        with st.container(border=True):
            col1, col2, col3, col4, col5 = st.columns([2, 1, 2, 1, 0.5])
            with col1:
                recipe["InputItem"] = st.selectbox("In", item_names, index=item_names.index(recipe["InputItem"]) if recipe["InputItem"] in item_names else 0, key=f"{key_prefix}_in_{i}")
            with col2:
                recipe["InputQuantity"] = st.number_input("Qty", value=recipe["InputQuantity"], min_value=1, key=f"{key_prefix}_in_q_{i}")
            with col3:
                recipe["OutputItem"] = st.selectbox("Out", item_names, index=item_names.index(recipe["OutputItem"]) if recipe["OutputItem"] in item_names else 0, key=f"{key_prefix}_out_{i}")
            with col4:
                recipe["OutputQuantity"] = st.number_input("Qty", value=recipe["OutputQuantity"], min_value=1, key=f"{key_prefix}_out_q_{i}")
            with col5:
                if st.button("🗑️", key=f"{key_prefix}_del_{i}"):
                    to_delete = i
    
    if to_delete is not None:
        recipes.pop(to_delete)
        st.rerun()
    
    if st.button("➕ Add Recipe", key=f"{key_prefix}_add"):
        recipes.append({"InputItem": item_names[0], "InputQuantity": 1, "OutputItem": item_names[0], "OutputQuantity": 1})
        st.rerun()

def edit_cadence_abilities(abilities_list, key_prefix):
    to_delete = None
    all_ability_names = [a["Name"] for a in manager.unified_data["abilities"]]
    stat_names = [s["Name"] for s in manager.unified_data["stats"]]

    for i, ab_entry in enumerate(abilities_list):
        with st.container(border=True):
            col1, col2, col3 = st.columns([3, 2, 1])
            with col1:
                # The structure in JSON is {"Ability": "Name", "Requirements": [...], "PrimaryStat": "..."}
                current_ab_name = ab_entry["Ability"]
                new_ab_name = st.selectbox(f"Ability {i}", all_ability_names, index=all_ability_names.index(current_ab_name) if current_ab_name in all_ability_names else 0, key=f"{key_prefix}_ab_{i}")
                
                if new_ab_name != current_ab_name:
                    ab_entry["Ability"] = new_ab_name
            
            with col2:
                ab_entry["PrimaryStat"] = st.selectbox(f"Stat {i}", stat_names, index=stat_names.index(ab_entry["PrimaryStat"]) if ab_entry.get("PrimaryStat") in stat_names else 0, key=f"{key_prefix}_stat_{i}")
            
            with col3:
                if st.button("🗑️ Remove Ability", key=f"{key_prefix}_del_{i}"):
                    to_delete = i
            
            st.write("**Requirements**")
            edit_list(ab_entry["Requirements"], f"{key_prefix}_req_{i}")

    if to_delete is not None:
        abilities_list.pop(to_delete)
        st.rerun()

    if st.button("➕ Add Ability to Cadence", key=f"{key_prefix}_add"):
        source_ab = manager.unified_data["abilities"][0]
        abilities_list.append({
            "Ability": source_ab["Name"],
            "Requirements": [],
            "PrimaryStat": "Magic"
        })
        st.rerun()

# --- Page Rendering ---

st.title(f"Manage {page}")

data = manager.unified_data[page.lower()]
df = pd.DataFrame(data)

if not df.empty and "Name" in df.columns:
    search = st.text_input(f"Search {page}...", "")
    if search:
        df = df[df['Name'].str.contains(search, case=False)]
    
    selected_name = st.selectbox(f"Select {page} to Edit", ["-- New --"] + df['Name'].tolist())
else:
    selected_name = st.selectbox(f"Select {page} to Edit", ["-- New --"])


if selected_name == "-- New --":
    if st.button(f"Create New {page}"):
        new_item = {"Name": "New " + page, "Description": ""}
        if page == "Quests":
            new_item.update({"DurationSeconds": 10, "Type": "Single", "Requirements": [], "Rewards": [], "PrimaryStat": "Vitality", "RequiredStats": {}, "StatRewards": {}, "Requires": [], "UnlocksCadences": []})
        elif page == "Items":
            new_item.update({"ItemType": "Material", "Augments": []})
        elif page == "Cadences":
            new_item.update({"Abilities": []})
        elif page == "Refinements":
            new_item = {"Name": "New Refinement", "PrimaryStat": "Strength", "Recipes": []}
        
        manager.unified_data[page.lower()].append(new_item)
        st.rerun()

else:
    item = next(i for i in data if i["Name"] == selected_name)
    
    with st.form(f"edit_{page}_{selected_name}"):
        st.subheader(f"Editing: {selected_name}")
        
        if page != "Refinements": # Refinements use 'Ability' as name, handled in data_io
            name = st.text_input("Name", item["Name"])
            description = st.text_area("Description", item.get("Description", ""))
        else:
            name = st.selectbox("Ability Base", [a["Name"] for a in manager.unified_data["abilities"]], index=[a["Name"] for a in manager.unified_data["abilities"]].index(item["Name"]) if item["Name"] in [a["Name"] for a in manager.unified_data["abilities"]] else 0)
            description = "" # Not used for refinements
            
        if page == "Quests":
            col1, col2, col3 = st.columns(3)
            with col1:
                duration = st.number_input("Duration (s)", value=item["DurationSeconds"])
            with col2:
                q_type = st.selectbox("Type", ["Single", "Recurring", "Unlock"], index=["Single", "Recurring", "Unlock"].index(item["Type"]))
            with col3:
                stat = st.selectbox("Primary Stat", [s["Name"] for s in manager.unified_data["stats"]], index=[s["Name"] for s in manager.unified_data["stats"]].index(item["PrimaryStat"]))
            
            st.write("---")
            st.subheader("Unlocks & Dependencies")
            requires = st.multiselect("Prerequisite Quests", [q["Name"] for q in manager.unified_data["quests"] if q["Name"] != name], default=item["Requires"])
            unlocks_c = st.multiselect("Unlocks Cadences", [c["Name"] for c in manager.unified_data["cadences"]], default=item["UnlocksCadences"])

        elif page == "Items":
            i_type = st.selectbox("Type", ["Material", "Currency", "Consumable", "Spell", "KeyItem"], index=["Material", "Currency", "Consumable", "Spell", "KeyItem"].index(item["ItemType"]))
        
        elif page == "Refinements":
             stat = st.selectbox("Primary Stat", [s["Name"] for s in manager.unified_data["stats"]], index=[s["Name"] for s in manager.unified_data["stats"]].index(item["PrimaryStat"]))
            
        if st.form_submit_button("Update Basic Info"):
            item["Name"] = name
            if "Description" in item: item["Description"] = description
            if page == "Quests":
                item["DurationSeconds"] = duration
                item["Type"] = q_type
                item["PrimaryStat"] = stat
                item["Requires"] = requires
                item["UnlocksCadences"] = unlocks_c
            elif page == "Items":
                item["ItemType"] = i_type
            elif page == "Refinements":
                item["PrimaryStat"] = stat
            st.success("Updated basic info in memory.")
            st.rerun()

    # Nested data management outside the form
    if page == "Quests":
        st.subheader("📦 Requirements")
        edit_list(item["Requirements"], "req")
        
        st.subheader("💎 Rewards")
        edit_list(item["Rewards"], "rew")

        st.subheader("🛡️ Required Stats")
        edit_dict_list(item["RequiredStats"], "req_stat")

        st.subheader("📈 Stat Rewards")
        edit_dict_list(item["StatRewards"], "rew_stat")
    
    elif page == "Cadences":
        st.subheader("⚡ Abilities")
        edit_cadence_abilities(item["Abilities"], "cad_ab")

    elif page == "Refinements":
        st.subheader("🧪 Recipes")
        edit_recipes(item["Recipes"], "ref_rec")

    elif page == "Abilities":
        st.info("Abilities themselves are simple Name/Description. Use the Cadence page to assign them and define requirements.")

    if st.button("🔥 Delete Entity", type="secondary"):
        manager.unified_data[page.lower()].remove(item)
        st.warning(f"Deleted {selected_name} from memory.")
        st.rerun()

