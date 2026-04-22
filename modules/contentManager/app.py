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

def edit_list(data_list, key_prefix):
    to_delete = None
    for i, entry in enumerate(data_list):
        col1, col2, col3 = st.columns([3, 2, 1])
        with col1:
            # Safely find index of current item
            item_names = [it["Name"] for i in [manager.unified_data["items"]] for it in i]
            current_index = 0
            if entry["Item"] in item_names:
                current_index = item_names.index(entry["Item"])
            
            new_name = st.selectbox(f"Item {i}", item_names, index=current_index, key=f"{key_prefix}_name_{i}")
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
        data_list.append({"Item": manager.unified_data["items"][0]["Name"], "Quantity": 1})
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
        manager.unified_data[page.lower()].append(new_item)
        st.rerun()

else:
    item = next(i for i in data if i["Name"] == selected_name)
    
    with st.form(f"edit_{page}_{selected_name}"):
        st.subheader(f"Editing: {selected_name}")
        
        name = st.text_input("Name", item["Name"])
        description = st.text_area("Description", item.get("Description", ""))
        
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
            
            # Complex nested data (Requirements/Rewards) are hard in standard Streamlit forms 
            # so we'll just show them as JSON for now or implement outside form
            st.info("Requirements and Rewards are managed below the form for technical reasons.")

        if page == "Items":
            i_type = st.selectbox("Type", ["Material", "Currency", "Consumable", "Spell", "KeyItem"], index=["Material", "Currency", "Consumable", "Spell", "KeyItem"].index(item["ItemType"]))
            
        if st.form_submit_button("Update Basic Info"):
            item["Name"] = name
            item["Description"] = description
            if page == "Quests":
                item["DurationSeconds"] = duration
                item["Type"] = q_type
                item["PrimaryStat"] = stat
                item["Requires"] = requires
                item["UnlocksCadences"] = unlocks_c
            if page == "Items":
                item["ItemType"] = i_type
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

    if st.button("🔥 Delete Entity", type="secondary"):
        manager.unified_data[page.lower()].remove(item)
        st.warning(f"Deleted {selected_name} from memory.")
        st.rerun()
