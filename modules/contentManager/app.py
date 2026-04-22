import streamlit as st
import pandas as pd
from data_io import ContentManager
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
        res1 = subprocess.run(["python", "scripts/migrate_to_graph.py"], capture_output=True, text=True)
        res2 = subprocess.run(["python", "scripts/verify_graph.py"], capture_output=True, text=True)
        
        if res1.returncode == 0:
            st.sidebar.success("Compiled successfully")
        else:
            st.sidebar.error(f"Migration Error: {res1.stderr}")
            
        if res2.returncode == 0:
            st.sidebar.success("Graph verified")
        else:
            st.sidebar.error(f"Verification Failed:\n{res2.stdout}")

def edit_list(data_list, key_prefix):
    new_list = []
    for i, item in enumerate(data_list):
        col1, col2, col3 = st.columns([3, 2, 1])
        with col1:
            name = st.selectbox(f"Item {i}", [i["Name"] for i in manager.unified_data["items"]], 
                               index=[i["Name"] for i in manager.unified_data["items"]].index(item["Item"]) if item["Item"] in [i["Name"] for i in manager.unified_data["items"]] else 0,
                               key=f"{key_prefix}_name_{i}")
        with col2:
            qty = st.number_input(f"Qty {i}", value=item["Quantity"], min_value=1, key=f"{key_prefix}_qty_{i}")
        with col3:
            if st.button("🗑️", key=f"{key_prefix}_del_{i}"):
                continue
        new_list.append({"Item": name, "Quantity": qty})
    
    if st.button("➕ Add Entry", key=f"{key_prefix}_add"):
        new_list.append({"Item": manager.unified_data["items"][0]["Name"], "Quantity": 1})
        st.rerun()
    return new_list

# --- Page Rendering ---

st.title(f"Manage {page}")

data = manager.unified_data[page.lower()]
df = pd.DataFrame(data)

search = st.text_input(f"Search {page}...", "")
if search:
    df = df[df['Name'].str.contains(search, case=False)]

selected_name = st.selectbox(f"Select {page} to Edit", ["-- New --"] + df['Name'].tolist())

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
        item["Requirements"] = edit_list(item["Requirements"], "req")
        st.subheader("💎 Rewards")
        item["Rewards"] = edit_list(item["Rewards"], "rew")

    if st.button("🔥 Delete Entity", type="secondary"):
        manager.unified_data[page.lower()].remove(item)
        st.warning(f"Deleted {selected_name} from memory.")
        st.rerun()
