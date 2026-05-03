import os
import sys
from . import constants as c
from . import data_processor as dp
from . import template_engine as te
from . import web_server as ws

def run_visualization(no_serve=False):
    print("Generating Clustered & Chronological Visual Dashboard...")
    try:
        # 1. Load and Enrich Data
        graph_data = dp.load_graph()
        nodes, cluster_names = dp.enrich_data(graph_data)
        
        # 2. Load JS Engine (from split files)
        js_files = [
            "lattice_config.js",
            "lattice_state.js",
            "lattice_data.js",
            "lattice_rendering_logic.js",
            "lattice_rendering_milestones.js",
            "lattice_rendering.js",
            "lattice_interactions.js",
            "lattice_main.js"
        ]
        js_code = ""
        base_path = os.path.dirname(__file__)
        for js_file in js_files:
            file_path = os.path.join(base_path, js_file)
            with open(file_path, "r", encoding="utf-8") as f:
                js_code += f"\n// --- {js_file} ---\n"
                js_code += f.read() + "\n"

        # 3. Generate HTML
        html = te.generate_full_html(nodes, cluster_names, js_code)
        
        # 4. Save Output
        if not os.path.exists(c.OUTPUT_DIR):
            os.makedirs(c.OUTPUT_DIR)
            
        file_path = os.path.join(c.OUTPUT_DIR, c.OUTPUT_FILE)
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(html)
            
        print(f"Dashboard generated: {os.path.abspath(file_path)}")
        
        # 5. Optional Serve
        if not no_serve:
            ws.run_visualizer_server(c.OUTPUT_DIR, c.OUTPUT_FILE)
            
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
