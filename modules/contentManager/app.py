import streamlit as st
import pandas as pd
from data_io import ContentManager, PROJECT_ROOT
import ui_components as ui
import subprocess
import os

st.set_page_config(page_title="Mythril Content Manager", layout="wide")

if 'manager' not in st.session_state:
    st.session_state.manager = ContentManager()

manager = st.session_state.manager

st.sidebar.title("💎 Mythril CMS")
page = st.sidebar.selectbox("Navigate", ["Quests", "Items", "Cadences", "Abilities", "Locations", "Refinements", "Stats"])

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
        
        if page == "Refinements":
            name = st.selectbox("Linked Ability", [a["Name"] for a in manager.unified_data["abilities"]], index=[a["Name"] for a in manager.unified_data["abilities"]].index(item["Name"]) if item["Name"] in [a["Name"] for a in manager.unified_data["abilities"]] else 0)
        else:
            name = st.text_input("Name", item["Name"])
            
        description = st.text_area("Description", item.get("Description", "")) if "Description" in item else ""
            
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
             stat = st.selectbox("Primary Stat", [s["Name"] for s in manager.unified_data["stats"]], index=[s["Name"] for s in manager.unified_data["stats"]].index(item["PrimaryStat"]) if item["PrimaryStat"] in [s["Name"] for s in manager.unified_data["stats"]] else 0)
            
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
    # We use selected_name in keys to ensure Streamlit treats them as unique per-entity
    safe_key = selected_name.replace(" ", "_").lower()
    
    if page == "Quests":
        st.subheader("📦 Requirements")
        ui.edit_list(manager, item["Requirements"], f"req_{safe_key}")
        
        st.subheader("💎 Rewards")
        ui.edit_list(manager, item["Rewards"], f"rew_{safe_key}")

        st.subheader("🛡️ Required Stats")
        ui.edit_dict_list(manager, item["RequiredStats"], f"req_stat_{safe_key}")

        st.subheader("📈 Stat Rewards")
        ui.edit_dict_list(manager, item["StatRewards"], f"rew_stat_{safe_key}")

        st.subheader("✨ Effects")
        if "Effects" not in item: item["Effects"] = []
        ui.edit_effects(manager, item["Effects"], f"eff_{safe_key}")
    
    elif page == "Cadences":
        st.subheader("⚡ Abilities")
        ui.edit_cadence_abilities(manager, item["Abilities"], f"cad_ab_{safe_key}")

    elif page == "Refinements":
        st.subheader("🧪 Recipes")
        ui.edit_recipes(manager, item["Recipes"], f"ref_rec_{safe_key}")

    elif page == "Abilities":
        st.info("Abilities themselves are simple Name/Description. Use the Cadence page to assign them and define requirements.")

    if st.button("🔥 Delete Entity", type="secondary"):
        manager.unified_data[page.lower()].remove(item)
        st.warning(f"Deleted {selected_name} from memory.")
        st.rerun()
