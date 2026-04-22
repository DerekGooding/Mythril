import streamlit as st

def edit_list(manager, data_list, key_prefix, item_pool=None):
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

def edit_dict_list(manager, data_dict, key_prefix, label_name="Stat"):
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

def edit_recipes(manager, recipes, key_prefix):
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

def edit_cadence_abilities(manager, abilities_list, key_prefix):
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
            edit_list(manager, ab_entry["Requirements"], f"{key_prefix}_req_{i}")

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
