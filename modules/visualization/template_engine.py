import json

def get_css():
    return """
        :root {
            --bg-color: #0d1117;
            --panel-bg: #161b22;
            --border-color: #30363d;
            --text-color: #c9d1d9;
            --accent-color: #58a6ff;
            --quest-color: #bc8cff;
            --item-color: #79c0ff;
            --ability-color: #7ee787;
            --cadence-color: #ffa657;
            --location-color: #ff7b72;
            --stat-color: #d29922;
            --refinement-color: #f2cc60;
        }

        body, html {
            margin: 0; padding: 0; width: 100%; height: 100%;
            background-color: var(--bg-color); color: var(--text-color);
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
            overflow: hidden;
        }

        .dashboard { display: flex; flex-direction: column; width: 100%; height: 100%; }

        header {
            background-color: var(--panel-bg); border-bottom: 1px solid var(--border-color);
            padding: 10px 20px; display: flex; justify-content: space-between; align-items: center;
            z-index: 100;
        }

        .controls { display: flex; gap: 10px; }

        button {
            background-color: #21262d; color: var(--text-color); border: 1px solid var(--border-color);
            padding: 6px 12px; border-radius: 6px; cursor: pointer; font-size: 14px;
        }

        button.active { background-color: var(--accent-color); color: white; border-color: var(--accent-color); }
        button:hover { background-color: #30363d; }
        
        button.toggle-btn.on { border-color: var(--ability-color); box-shadow: 0 0 5px rgba(126, 231, 135, 0.3); }

        .main-container { flex: 1; position: relative; display: flex; }
        #view-canvas { flex: 1; position: relative; overflow: hidden; }

        .sidebar {
            width: 320px; background-color: var(--panel-bg); border-left: 1px solid var(--border-color);
            padding: 20px; overflow-y: auto; display: none; box-shadow: -4px 0 10px rgba(0,0,0,0.3); z-index: 50;
        }

        #graph-svg { width: 100%; height: 100%; cursor: grab; }
        #graph-svg:active { cursor: grabbing; }

        .cluster-box { fill: rgba(88, 166, 255, 0.03); stroke: rgba(88, 166, 255, 0.15); stroke-width: 1px; stroke-dasharray: 5,5; rx: 15; ry: 15; }
        .cluster-label { font-size: 14px; font-weight: bold; fill: rgba(255, 255, 255, 0.3); text-transform: uppercase; pointer-events: none; }

        .node circle, .node rect, .node polygon { stroke-width: 2px; stroke: rgba(0,0,0,0.5); }
        .node.dimmed { opacity: 0.15; pointer-events: none; }
        .node.highlighted { opacity: 1; stroke: #fff; stroke-width: 3px; }
        .node.milestone { stroke: gold; stroke-width: 3px; filter: drop-shadow(0 0 8px rgba(210, 153, 34, 0.6)); }
        .node.sustainable { filter: drop-shadow(0 0 5px var(--ability-color)); }
        .node.unsustainable { filter: drop-shadow(0 0 5px var(--location-color)); }

        .edge { fill: none; stroke: #30363d; stroke-width: 1.5px; marker-end: url(#arrowhead); opacity: 0.6; }
        .edge.progression { stroke: var(--accent-color); stroke-width: 2px; opacity: 0.8; }
        .edge.economy { stroke: #484f58; stroke-dasharray: 2,2; }
        .edge.dimmed { opacity: 0.05; }
        .edge.hidden { display: none; }
        .edge.highlighted-up { stroke: #ff7b72; stroke-width: 3px; opacity: 1; }
        .edge.highlighted-down { stroke: #7ee787; stroke-width: 3px; opacity: 1; }

        .label { font-size: 11px; font-weight: 500; fill: var(--text-color); pointer-events: none; text-shadow: 0 1px 2px rgba(0,0,0,0.8); }
        .tier-line { stroke: rgba(255,255,255,0.05); stroke-width: 1px; stroke-dasharray: 10,10; }
        .tier-label { font-size: 10px; fill: rgba(255,255,255,0.2); text-anchor: middle; }

        #hierarchy-view { width: 100%; height: 100%; display: none; overflow: auto; padding: 40px; box-sizing: border-box; background: #0d1117; }
        .tier-column { display: inline-flex; flex-direction: column; gap: 15px; vertical-align: top; min-width: 280px; padding: 0 40px; border-right: 1px solid var(--border-color); }
        .tier-header { font-weight: bold; font-size: 18px; margin-bottom: 20px; color: var(--accent-color); position: sticky; top: 0; background: var(--bg-color); padding: 10px 0; z-index: 10; }

        .card { background-color: var(--panel-bg); border: 1px solid var(--border-color); border-radius: 8px; padding: 12px; font-size: 13px; cursor: pointer; transition: transform 0.2s, box-shadow 0.2s; }
        .card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.4); border-color: var(--accent-color); }
        .card-type { font-size: 10px; text-transform: uppercase; font-weight: bold; margin-bottom: 6px; opacity: 0.8; }
        .card-name { font-weight: bold; margin-bottom: 6px; font-size: 14px; }

        .tooltip { position: absolute; background: rgba(22, 27, 34, 0.95); border: 1px solid var(--border-color); padding: 12px; border-radius: 8px; pointer-events: none; display: none; z-index: 1000; max-width: 280px; box-shadow: 0 8px 24px rgba(0,0,0,0.6); font-size: 13px; }
    """

def get_html_skeleton(nodes_json, clusters_json, css_content, js_content):
    return f"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mythril Visual Dashboard V2</title>
    <style>{css_content}</style>
</head>
<body>
    <div class="dashboard">
        <header>
            <div style="display: flex; align-items: center; gap: 20px;">
                <h2 style="margin:0; letter-spacing: -0.5px;">Mythril <span style="color:var(--accent-color)">Lattice</span></h2>
                <div class="controls">
                    <button id="btn-lattice" class="active">Lattice View</button>
                    <button id="btn-hierarchy">Hierarchy View</button>
                    <button id="btn-reset">Reset Layout</button>
                    <div style="width: 1px; background: var(--border-color); margin: 0 5px;"></div>
                    <button id="toggle-progression" class="toggle-btn on">Progression Only</button>
                    <button id="toggle-hubs" class="toggle-btn">Show Hubs</button>
                    <button id="toggle-sim" class="toggle-btn">Sim Overlay</button>
                </div>
            </div>
            <div id="stats" style="font-size: 12px; color: #8b949e; font-family: monospace;"></div>
        </header>
        <div class="main-container">
            <div id="view-canvas">
                <svg id="graph-svg">
                    <defs>
                        <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="22" refY="3.5" orient="auto">
                            <polygon points="0 0, 10 3.5, 0 7" fill="currentColor" />
                        </marker>
                    </defs>
                    <g id="viewport">
                        <g id="tiers-layer"></g>
                        <g id="clusters-layer"></g>
                        <g id="edges-layer"></g>
                        <g id="nodes-layer"></g>
                    </g>
                </svg>
                <div id="hierarchy-view"></div>
            </div>
            <div id="sidebar" class="sidebar">
                <div style="display:flex; justify-content: space-between; align-items: flex-start;">
                    <h2 id="side-name" style="margin:0; font-size: 1.5em; color: white;"></h2>
                    <button onclick="document.getElementById('sidebar').style.display='none'" style="padding:2px 8px;">×</button>
                </div>
                <p id="side-type" style="margin-top:5px; margin-bottom: 20px;"></p>
                <div id="side-content"></div>
            </div>
        </div>
    </div>
    <div id="tooltip" class="tooltip"></div>
    <script>
        const nodesData = {nodes_json};
        const clusterNames = {clusters_json};
        {js_content}
    </script>
</body>
</html>
"""

def generate_full_html(nodes, cluster_names, js_code):
    css = get_css()
    nodes_json = json.dumps(nodes)
    clusters_json = json.dumps(cluster_names)
    return get_html_skeleton(nodes_json, clusters_json, css, js_code)
