import os
import webbrowser
import http.server
import socketserver
import threading
import time
import sys
from export_graph_mermaid import generate_mermaid

def generate_html(mermaid_code):
    html_template = f"""
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="UTF-8">
        <title>Mythril Content Graph Visualizer</title>
        <script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"></script>
        <style>
            body {{ 
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                background-color: #0d1117; 
                color: #c9d1d9; 
                margin: 0; 
                padding: 0;
            }}
            .header {{
                background-color: #161b22;
                padding: 10px 20px;
                border-bottom: 1px solid #30363d;
                display: flex;
                flex-direction: column;
                position: sticky;
                top: 0;
                z-index: 100;
            }}
            .header-top {{
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 10px;
            }}
            .controls {{
                display: flex;
                gap: 20px;
                background: #21262d;
                padding: 8px 15px;
                border-radius: 6px;
                font-size: 0.9em;
            }}
            .control-item {{
                display: flex;
                align-items: center;
                gap: 8px;
                cursor: pointer;
            }}
            .legend {{
                display: flex;
                gap: 15px;
                font-size: 0.8em;
            }}
            .legend-item {{ display: flex; align-items: center; gap: 5px; }}
            .legend-box {{ width: 12px; height: 12px; border-radius: 2px; }}
            .quest-box {{ background-color: #4b0082; border: 1px solid #f9f; }}
            .location-box {{ background-color: #00008b; border: 1px solid #ccf; }}
            .cadence-box {{ background-color: #006400; border: 1px solid #cfc; }}
            .item-box {{ background-color: #444; border: 1px solid #fff; }}
            .stat-box {{ background-color: #8b4513; border: 1px solid #ff8c00; }}
            .ability-box {{ background-color: #2f4f4f; border: 1px solid #00ced1; }}
            .refinement-box {{ background-color: #ff4500; border: 1px solid #fff; }}
            
            #graph-container {{
                width: 100vw;
                height: calc(100vh - 100px);
                overflow: auto;
                padding: 40px;
                box-sizing: border-box;
                background-color: #0d1117;
            }}
            
            .mermaid {{
                margin: 0;
                display: inline-block;
            }}

            /* Toggling Logic */
            body.hide-items .item, body.hide-items [class*="Gives"], body.hide-items [class*="Consumes"] {{ display: none !important; }}
            body.hide-stats .stat, body.hide-stats [class*="Req"] {{ display: none !important; }}
            body.hide-abilities .ability, body.hide-abilities .cadence, body.hide-abilities [class*="Allows"], body.hide-abilities .refinement {{ display: none !important; }}
            
            /* Mermaid Specific SVG targeting */
            body.hide-items g.node.item, body.hide-items g.edgePath path[stroke*="o"], body.hide-items g.edgePath path[stroke*="x"] {{ opacity: 0; pointer-events: none; }}
        </style>
    </head>
    <body>
        <div class="header">
            <div class="header-top">
                <h2 style="margin:0; font-size: 1.2em;">Mythril Content Graph</h2>
                <div class="legend">
                    <div class="legend-item"><div class="legend-box quest-box"></div> Quest</div>
                    <div class="legend-item"><div class="legend-box location-box"></div> Location</div>
                    <div class="legend-item"><div class="legend-box cadence-box"></div> Cadence</div>
                    <div class="legend-item"><div class="legend-box item-box"></div> Item</div>
                    <div class="legend-item"><div class="legend-box stat-box"></div> Stat</div>
                    <div class="legend-item"><div class="legend-box ability-box"></div> Ability</div>
                    <div class="legend-item"><div class="legend-box refinement-box"></div> Refinement</div>
                </div>
            </div>
            <div class="controls">
                <strong>Visibility Toggles:</strong>
                <label class="control-item">
                    <input type="checkbox" checked onchange="toggleCategory('hide-items', this.checked)"> Items & Economy
                </label>
                <label class="control-item">
                    <input type="checkbox" checked onchange="toggleCategory('hide-stats', this.checked)"> Stat Requirements
                </label>
                <label class="control-item">
                    <input type="checkbox" checked onchange="toggleCategory('hide-abilities', this.checked)"> Cadences & Abilities
                </label>
            </div>
        </div>
        <div id="graph-container">
            <div class="mermaid">
{mermaid_code}
            </div>
        </div>
        <script>
            mermaid.initialize({{ 
                startOnLoad: true,
                theme: 'dark',
                securityLevel: 'loose',
                flowchart: {{
                    useMaxWidth: false,
                    htmlLabels: true,
                    curve: 'basis',
                    padding: 50
                }}
            }});

            function toggleCategory(className, isVisible) {{
                if (isVisible) {{
                    document.body.classList.remove(className);
                }} else {{
                    document.body.classList.add(className);
                }}
            }}
        </script>
    </body>
    </html>
    """
    return html_template

def start_server(port):
    """Starts a simple HTTP server in the current directory."""
    handler = http.server.SimpleHTTPRequestHandler
    # Suppress server logs to keep terminal clean
    handler.log_message = lambda *args: None
    try:
        with socketserver.TCPServer(("", port), handler) as httpd:
            print(f"Server started at http://localhost:{port}")
            httpd.serve_forever()
    except Exception as e:
        print(f"Server error: {e}")

def main():
    print("Generating Clustered Mermaid graph...")
    try:
        mermaid_code = generate_mermaid()
        html = generate_html(mermaid_code)
        
        output_dir = "output"
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
            
        file_path = os.path.join(output_dir, "graph_visualizer.html")
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(html)
            
        abs_path = os.path.abspath(file_path)
        print(f"Graph generated: {abs_path}")
        
        # Start server in a background thread
        port = 8000
        # Change dir to output so we serve the file directly
        os.chdir(output_dir)
        
        server_thread = threading.Thread(target=start_server, args=(port,), daemon=True)
        server_thread.start()
        
        # Small delay to ensure server is up
        time.sleep(1)
        
        url = f"http://localhost:{port}/graph_visualizer.html"
        print(f"Opening {url}...")
        webbrowser.open(url)
        
        print("\nVisualizer is running. Press Ctrl+C to stop the server.")
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            print("\nStopping server...")
            sys.exit(0)
            
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
