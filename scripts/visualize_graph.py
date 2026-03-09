import os
import webbrowser
import tempfile
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
                justify-content: space-between;
                align-items: center;
                position: sticky;
                top: 0;
                z-index: 100;
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
            
            #graph-container {{
                width: 100%;
                overflow: auto;
                padding: 40px;
                box-sizing: border-box;
            }}
            
            .mermaid {{
                background: #0d1117;
                display: block;
                margin: 0 auto;
            }}
        </style>
    </head>
    <body>
        <div class="header">
            <div>
                <h2 style="margin:0; font-size: 1.2em;">Mythril Content Graph</h2>
            </div>
            <div class="legend">
                <div class="legend-item"><div class="legend-box quest-box"></div> Quest</div>
                <div class="legend-item"><div class="legend-box location-box"></div> Location</div>
                <div class="legend-item"><div class="legend-box cadence-box"></div> Cadence</div>
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
        </script>
    </body>
    </html>
    """
    return html_template

def main():
    print("Generating Mermaid graph content...")
    try:
        mermaid_code = generate_mermaid()
        html = generate_html(mermaid_code)
        
        # Ensure output directory exists
        output_dir = "output"
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
            
        file_path = os.path.join(output_dir, "graph_visualizer.html")
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(html)
            
        abs_path = os.path.abspath(file_path)
        print(f"Graph generated: {abs_path}")
        
        # Open in browser
        print("Opening browser...")
        webbrowser.open(f"file://{abs_path}")
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
