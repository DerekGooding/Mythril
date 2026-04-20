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
        
        # 2. Load JS Engine (from file)
        js_path = os.path.join(os.path.dirname(__file__), "lattice_engine.js")
        with open(js_path, "r", encoding="utf-8") as f:
            js_code = f.read()

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
