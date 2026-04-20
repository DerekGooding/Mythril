import json
import os
import webbrowser
import http.server
import socketserver
import threading
import time
import sys

# Paths
GRAPH_FILE = "Mythril.Blazor/wwwroot/data/content_graph.json"
OUTPUT_DIR = "output"
OUTPUT_FILE = "visual_dashboard.html"

def load_graph():
    if not os.path.exists(GRAPH_FILE):
        print(f"Error: {GRAPH_FILE} not found. Run scripts/migrate_to_graph.py first.")
        sys.exit(1)
    with open(GRAPH_FILE, 'r', encoding='utf-8') as f:
        return json.load(f)

def generate_html(graph_data):
    html_template = f"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mythril Visual Dashboard</title>
    <style>
        :root {{
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
        }}

        body, html {{
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
            background-color: var(--bg-color);
            color: var(--text-color);
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
            overflow: hidden;
        }}

        .dashboard {{
            display: flex;
            flex-direction: column;
            width: 100%;
            height: 100%;
        }}

        header {{
            background-color: var(--panel-bg);
            border-bottom: 1px solid var(--border-color);
            padding: 10px 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            z-index: 100;
        }}

        .controls {{
            display: flex;
            gap: 10px;
        }}

        button {{
            background-color: #21262d;
            color: var(--text-color);
            border: 1px solid var(--border-color);
            padding: 6px 12px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
        }}

        button.active {{
            background-color: var(--accent-color);
            color: white;
            border-color: var(--accent-color);
        }}

        button:hover {{
            background-color: #30363d;
        }}

        .main-container {{
            flex: 1;
            position: relative;
            display: flex;
        }}

        #view-canvas {{
            flex: 1;
            position: relative;
            overflow: hidden;
        }}

        .sidebar {{
            width: 300px;
            background-color: var(--panel-bg);
            border-left: 1px solid var(--border-color);
            padding: 20px;
            overflow-y: auto;
            display: none;
        }}

        #graph-svg {{
            width: 100%;
            height: 100%;
            cursor: grab;
        }}

        #graph-svg:active {{
            cursor: grabbing;
        }}

        .node circle, .node rect, .node polygon {{
            stroke-width: 2px;
            transition: stroke-width 0.2s;
        }}

        .node:hover circle, .node:hover rect, .node:hover polygon {{
            stroke-width: 4px;
        }}

        .edge line {{
            stroke: #444;
            stroke-width: 1.5px;
            marker-end: url(#arrowhead);
        }}

        .edge.highlight {{
            stroke: var(--accent-color);
            stroke-width: 2.5px;
        }}

        .label {{
            font-size: 10px;
            fill: var(--text-color);
            pointer-events: none;
            text-anchor: middle;
        }}

        /* Hierarchy View Styling */
        #hierarchy-view {{
            width: 100%;
            height: 100%;
            display: none;
            overflow: auto;
            padding: 40px;
            box-sizing: border-box;
        }}

        .tier-column {{
            display: inline-flex;
            flex-direction: column;
            gap: 20px;
            vertical-align: top;
            min-width: 250px;
            padding-right: 50px;
            border-right: 1px dashed var(--border-color);
        }}

        .tier-header {{
            font-weight: bold;
            font-size: 18px;
            margin-bottom: 10px;
            color: var(--accent-color);
            position: sticky;
            top: 0;
            background: var(--bg-color);
            padding: 5px 0;
        }}

        .card {{
            background-color: var(--panel-bg);
            border: 1px solid var(--border-color);
            border-radius: 6px;
            padding: 10px;
            font-size: 13px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.3);
            position: relative;
        }}

        .card-type {{
            font-size: 10px;
            text-transform: uppercase;
            font-weight: bold;
            margin-bottom: 4px;
        }}

        .card-name {{
            font-weight: bold;
            margin-bottom: 4px;
        }}

        /* Tooltip */
        .tooltip {{
            position: absolute;
            background: rgba(22, 27, 34, 0.95);
            border: 1px solid var(--border-color);
            padding: 10px;
            border-radius: 6px;
            pointer-events: none;
            display: none;
            z-index: 1000;
            max-width: 250px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.5);
        }}
    </style>
</head>
<body>
    <div class="dashboard">
        <header>
            <div style="display: flex; align-items: center; gap: 15px;">
                <h2 style="margin:0;">Mythril Visual Dashboard</h2>
                <div class="controls">
                    <button id="btn-lattice" class="active">Lattice View</button>
                    <button id="btn-hierarchy">Hierarchy View</button>
                </div>
            </div>
            <div id="stats" style="font-size: 12px; color: #8b949e;"></div>
        </header>
        <div class="main-container">
            <div id="view-canvas">
                <svg id="graph-svg">
                    <defs>
                        <marker id="arrowhead" markerWidth="10" markerHeight="7" refX="19" refY="3.5" orient="auto">
                            <polygon points="0 0, 10 3.5, 0 7" fill="#444" />
                        </marker>
                    </defs>
                    <g id="viewport">
                        <g id="edges-layer"></g>
                        <g id="nodes-layer"></g>
                    </g>
                </svg>
                <div id="hierarchy-view"></div>
            </div>
            <div id="sidebar" class="sidebar">
                <h3 id="side-name">Select a node</h3>
                <p id="side-type" style="color: var(--accent-color); font-weight: bold;"></p>
                <hr style="border: 0; border-top: 1px solid var(--border-color); margin: 15px 0;">
                <div id="side-content"></div>
            </div>
        </div>
    </div>
    <div id="tooltip" class="tooltip"></div>

    <script>
        const graphData = {json.dumps(graph_data)};
        
        // --- State Management ---
        let currentView = 'lattice';
        const nodesLayer = document.getElementById('nodes-layer');
        const edgesLayer = document.getElementById('edges-layer');
        const viewport = document.getElementById('viewport');
        const svg = document.getElementById('graph-svg');
        const tooltip = document.getElementById('tooltip');
        const sidebar = document.getElementById('sidebar');
        
        let nodes = [];
        let edges = [];
        let transform = {{ x: 0, y: 0, k: 1 }};
        
        // --- Initialization ---
        function init() {{
            processData();
            setupInteractions();
            renderLattice();
            updateStats();
        }}

        function processData() {{
            nodes = graphData.map(d => ({{
                ...d,
                x: Math.random() * window.innerWidth,
                y: Math.random() * window.innerHeight,
                vx: 0,
                vy: 0,
                radius: 12
            }}));

            // Create flat edge list
            nodes.forEach(node => {{
                // Out edges
                if (node.out_edges) {{
                    Object.entries(node.out_edges).forEach(([type, targetList]) => {{
                        targetList.forEach(target => {{
                            const targetId = typeof target === 'string' ? target : target.targetId;
                            edges.push({{
                                source: node.id,
                                target: targetId,
                                type: type
                            }});
                        }});
                    }});
                }}
                // Special handling for in_edges (requires_quest etc)
                if (node.in_edges) {{
                    Object.entries(node.in_edges).forEach(([type, sourceList]) => {{
                        sourceList.forEach(sourceId => {{
                            edges.push({{
                                source: sourceId,
                                target: node.id,
                                type: type
                            }});
                        }});
                    }});
                }}
            }});
            
            // Filter broken edges
            const nodeMap = new Map(nodes.map(n => [n.id, n]));
            edges = edges.filter(e => nodeMap.has(e.source) && nodeMap.has(e.target));
        }}

        function updateStats() {{
            document.getElementById('stats').innerText = `Nodes: ${{nodes.length}} | Edges: ${{edges.length}}`;
        }}

        // --- Lattice View (Force Directed) ---
        function renderLattice() {{
            nodesLayer.innerHTML = '';
            edgesLayer.innerHTML = '';

            // Render Edges
            edges.forEach(edge => {{
                const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute('class', 'edge');
                line.dataset.source = edge.source;
                line.dataset.target = edge.target;
                edgesLayer.appendChild(line);
                edge.el = line;
            }});

            // Render Nodes
            nodes.forEach(node => {{
                const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
                g.setAttribute('class', 'node');
                
                let shape;
                const color = getCategoryColor(node.type);
                
                if (node.type === 'Quest') {{
                    // Hexagon
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                    shape.setAttribute('points', '-14,-7 -14,7 0,14 14,7 14,-7 0,-14');
                }} else if (node.type === 'Ability') {{
                    // Rhombus
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                    shape.setAttribute('points', '0,-14 14,0 0,14 -14,0');
                }} else if (node.type === 'Item') {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                    shape.setAttribute('r', '12');
                }} else {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                    shape.setAttribute('x', '-10');
                    shape.setAttribute('y', '-10');
                    shape.setAttribute('width', '20');
                    shape.setAttribute('height', '20');
                }}
                
                shape.setAttribute('fill', color);
                shape.setAttribute('stroke', 'rgba(0,0,0,0.5)');
                g.appendChild(shape);

                const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
                text.setAttribute('class', 'label');
                text.setAttribute('y', '25');
                text.textContent = node.name;
                g.appendChild(text);

                g.addEventListener('mouseenter', (e) => showTooltip(e, node));
                g.addEventListener('mouseleave', hideTooltip);
                g.addEventListener('click', () => selectNode(node));

                nodesLayer.appendChild(g);
                node.el = g;
            }});

            requestAnimationFrame(simulationStep);
        }}

        function simulationStep() {{
            if (currentView !== 'lattice') return;

            const alpha = 0.5;
            const nodeMap = new Map(nodes.map(n => [n.id, n]));

            // Forces
            nodes.forEach(n => {{
                // Center gravity
                n.vx += (window.innerWidth / 2 - n.x) * 0.001;
                n.vy += (window.innerHeight / 2 - n.y) * 0.001;

                // Repulsion
                nodes.forEach(m => {{
                    if (n === m) return;
                    const dx = n.x - m.x;
                    const dy = n.y - m.y;
                    const distSq = dx * dx + dy * dy || 1;
                    if (distSq < 40000) {{
                        const force = 100 / distSq;
                        n.vx += dx * force;
                        n.vy += dy * force;
                    }}
                }});
            }});

            edges.forEach(e => {{
                const s = nodeMap.get(e.source);
                const t = nodeMap.get(e.target);
                const dx = t.x - s.x;
                const dy = t.y - s.y;
                const dist = Math.sqrt(dx * dx + dy * dy) || 1;
                const force = (dist - 100) * 0.02;
                const fx = (dx / dist) * force;
                const fy = (dy / dist) * force;
                s.vx += fx;
                s.vy += fy;
                t.vx -= fx;
                t.vy -= fy;
            }});

            // Apply velocity
            nodes.forEach(n => {{
                n.x += n.vx;
                n.y += n.vy;
                n.vx *= 0.6;
                n.vy *= 0.6;

                n.el.setAttribute('transform', `translate(${{n.x}}, ${{n.y}})`);
            }});

            edges.forEach(e => {{
                const s = nodeMap.get(e.source);
                const t = nodeMap.get(e.target);
                e.el.setAttribute('x1', s.x);
                e.el.setAttribute('y1', s.y);
                e.el.setAttribute('x2', t.x);
                e.el.setAttribute('y2', t.y);
            }});

            requestAnimationFrame(simulationStep);
        }}

        // --- Hierarchy View ---
        function renderHierarchy() {{
            const container = document.getElementById('hierarchy-view');
            container.innerHTML = '';
            
            // Calculate Depth using BFS
            const depths = new Map();
            const roots = nodes.filter(n => {{
                const inEdges = edges.filter(e => e.target === n.id);
                return inEdges.length === 0 || n.id === 'quest_prologue';
            }});

            const queue = roots.map(r => ({{ id: r.id, depth: 0 }}));
            queue.forEach(q => depths.set(q.id, 0));

            let head = 0;
            while(head < queue.length) {{
                const current = queue[head++];
                const nextEdges = edges.filter(e => e.source === current.id);
                nextEdges.forEach(e => {{
                    if (!depths.has(e.target) || depths.get(e.target) < current.depth + 1) {{
                        depths.set(e.target, current.depth + 1);
                        queue.push({{ id: e.target, depth: current.depth + 1 }});
                    }}
                }});
            }}

            // Max Depth
            const maxDepth = Math.max(0, ...Array.from(depths.values()));
            const tiers = Array.from({{ length: maxDepth + 1 }}, () => []);
            
            nodes.forEach(n => {{
                const d = depths.has(n.id) ? depths.get(n.id) : 0;
                tiers[d].push(n);
            }});

            // Render Columns
            tiers.forEach((tierNodes, i) => {{
                const col = document.createElement('div');
                col.className = 'tier-column';
                col.innerHTML = `<div class="tier-header">Tier ${{i}}</div>`;
                
                // Group by type
                tierNodes.sort((a,b) => a.type.localeCompare(b.type));
                
                tierNodes.forEach(node => {{
                    const card = document.createElement('div');
                    card.className = 'card';
                    card.style.borderLeft = `4px solid ${{getCategoryColor(node.type)}}`;
                    card.innerHTML = `
                        <div class="card-type" style="color: ${{getCategoryColor(node.type)}}">${{node.type}}</div>
                        <div class="card-name">${{node.name}}</div>
                    `;
                    card.addEventListener('click', () => selectNode(node));
                    col.appendChild(card);
                }});
                
                container.appendChild(col);
            }});
        }}

        // --- UI Utilities ---
        function getCategoryColor(type) {{
            const colors = {{
                'Quest': 'var(--quest-color)',
                'Item': 'var(--item-color)',
                'Ability': 'var(--ability-color)',
                'Cadence': 'var(--cadence-color)',
                'Location': 'var(--location-color)',
                'Stat': 'var(--stat-color)',
                'Refinement': 'var(--refinement-color)'
            }};
            return colors[type] || '#ccc';
        }}

        function showTooltip(e, node) {{
            tooltip.style.display = 'block';
            tooltip.innerHTML = `<strong>${{node.name}}</strong><br><small>${{node.type}}</small>`;
            moveTooltip(e);
        }}

        function moveTooltip(e) {{
            tooltip.style.left = (e.pageX + 10) + 'px';
            tooltip.style.top = (e.pageY + 10) + 'px';
        }}

        function hideTooltip() {{
            tooltip.style.display = 'none';
        }}

        function selectNode(node) {{
            sidebar.style.display = 'block';
            document.getElementById('side-name').innerText = node.name;
            document.getElementById('side-type').innerText = node.type;
            
            let content = `<p>${{node.data.description || 'No description available.'}}</p>`;
            
            if (node.data.quest_type) content += `<p><strong>Type:</strong> ${{node.data.quest_type}}</p>`;
            if (node.data.primary_stat) content += `<p><strong>Stat:</strong> ${{node.data.primary_stat}}</p>`;

            // Requirements
            const reqs = edges.filter(e => e.target === node.id);
            if (reqs.length > 0) {{
                content += '<h4>Requirements</h4><ul>';
                reqs.forEach(r => {{
                    const src = nodes.find(n => n.id === r.source);
                    content += `<li>${{src ? src.name : r.source}} <small>(${{r.type}})</small></li>`;
                }});
                content += '</ul>';
            }}

            // Unlocks
            const unlocks = edges.filter(e => e.source === node.id);
            if (unlocks.length > 0) {{
                content += '<h4>Unlocks / Produces</h4><ul>';
                unlocks.forEach(u => {{
                    const tgt = nodes.find(n => n.id === u.target);
                    content += `<li>${{tgt ? tgt.name : u.target}} <small>(${{u.type}})</small></li>`;
                }});
                content += '</ul>';
            }}

            document.getElementById('side-content').innerHTML = content;

            // Highlight in Lattice
            if (currentView === 'lattice') {{
                document.querySelectorAll('.edge').forEach(el => el.classList.remove('highlight'));
                edges.forEach(e => {{
                    if (e.source === node.id || e.target === node.id) {{
                        e.el.classList.add('highlight');
                    }}
                }});
            }}
        }}

        function setupInteractions() {{
            document.getElementById('btn-lattice').addEventListener('click', () => {{
                currentView = 'lattice';
                document.getElementById('btn-lattice').classList.add('active');
                document.getElementById('btn-hierarchy').classList.remove('active');
                document.getElementById('graph-svg').style.display = 'block';
                document.getElementById('hierarchy-view').style.display = 'none';
                requestAnimationFrame(simulationStep);
            }});

            document.getElementById('btn-hierarchy').addEventListener('click', () => {{
                currentView = 'hierarchy';
                document.getElementById('btn-hierarchy').classList.add('active');
                document.getElementById('btn-lattice').classList.remove('active');
                document.getElementById('graph-svg').style.display = 'none';
                document.getElementById('hierarchy-view').style.display = 'flex';
                renderHierarchy();
            }});

            // Zoom and Pan
            let isDragging = false;
            let startPos = {{ x: 0, y: 0 }};

            svg.addEventListener('mousedown', e => {{
                if (e.target === svg || e.target === viewport) {{
                    isDragging = true;
                    startPos = {{ x: e.clientX - transform.x, y: e.clientY - transform.y }};
                }}
            }});

            window.addEventListener('mousemove', e => {{
                if (isDragging) {{
                    transform.x = e.clientX - startPos.x;
                    transform.y = e.clientY - startPos.y;
                    updateTransform();
                }}
                if (tooltip.style.display === 'block') moveTooltip(e);
            }});

            window.addEventListener('mouseup', () => isDragging = false);

            svg.addEventListener('wheel', e => {{
                e.preventDefault();
                const delta = e.deltaY > 0 ? 0.9 : 1.1;
                transform.k *= delta;
                updateTransform();
            }});
        }}

        function updateTransform() {{
            viewport.setAttribute('transform', `translate(${{transform.x}}, ${{transform.y}}) scale(${{transform.k}})`);
        }}

        init();
    </script>
</body>
</html>
    """
    return html_template

def start_server(port):
    handler = http.server.SimpleHTTPRequestHandler
    handler.log_message = lambda *args: None
    try:
        with socketserver.TCPServer(("", port), handler) as httpd:
            print(f"Server started at http://localhost:{port}")
            httpd.serve_forever()
    except Exception as e:
        print(f"Server error: {e}")

def main():
    no_serve = "--no-serve" in sys.argv
    print("Generating Homegrown Visual Dashboard...")
    try:
        graph_data = load_graph()
        html = generate_html(graph_data)
        
        if not os.path.exists(OUTPUT_DIR):
            os.makedirs(OUTPUT_DIR)
            
        file_path = os.path.join(OUTPUT_DIR, OUTPUT_FILE)
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(html)
            
        abs_path = os.path.abspath(file_path)
        print(f"Dashboard generated: {abs_path}")
        
        if no_serve:
            print("Skipping server as requested.")
            return

        # Start server
        port = 8000
        # Change dir to output
        original_dir = os.getcwd()
        os.chdir(OUTPUT_DIR)
        
        server_thread = threading.Thread(target=start_server, args=(port,), daemon=True)
        server_thread.start()
        
        time.sleep(1)
        url = f"http://localhost:{port}/{OUTPUT_FILE}"
        print(f"Opening {url}...")
        webbrowser.open(url)
        
        print("\nVisualizer is running. Press Ctrl+C to stop the server.")
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            print("\nStopping server...")
            os.chdir(original_dir)
            sys.exit(0)
            
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
