import json
import os
import webbrowser
import http.server
import socketserver
import threading
import time
import sys
from collections import deque

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

def enrich_data(nodes):
    """Calculate tiers and clusters in Python for better performance."""
    node_map = {n["id"]: n for n in nodes}
    
    # 1. Build Adjacency
    adj = {n["id"]: [] for n in nodes}
    for n in nodes:
        # Out edges
        if "out_edges" in n:
            for target_list in n["out_edges"].values():
                for target in target_list:
                    target_id = target if isinstance(target, str) else target.get("targetId")
                    if target_id in adj:
                        adj[n["id"]].append(target_id)
        # In edges
        if "in_edges" in n:
            for source_list in n["in_edges"].values():
                for source_id in source_list:
                    if source_id in adj:
                        adj[source_id].append(n["id"])

    # 2. BFS for Tiers
    tiers = {n["id"]: 0 for n in nodes}
    
    roots = [n["id"] for n in nodes if n["id"] == "quest_prologue"]
    if not roots:
        roots = [n["id"] for n in nodes if not n.get("in_edges")]
    
    queue = deque([(root, 0) for root in roots])
    visited_depth = {r: 0 for r in roots}
    
    while queue:
        curr_id, d = queue.popleft()
        
        for neighbor in adj[curr_id]:
            if neighbor not in visited_depth or visited_depth[neighbor] < d + 1:
                visited_depth[neighbor] = d + 1
                tiers[neighbor] = d + 1
                queue.append((neighbor, d + 1))

    # Special handling for nodes with hardcoded requirements not in the graph
    max_bfs_tier = max(tiers.values()) if tiers else 0
    for n in nodes:
        if n["id"] == "cadence_slayer" or "slayer" in n["id"].lower():
            tiers[n["id"]] = max_bfs_tier + 1

    # 3. Cluster Identification
    clusters = {} # NodeID -> ClusterID
    cluster_names = {} # ClusterID -> DisplayName
    
    # Locations -> Quests
    for n in nodes:
        if n["type"] == "Location":
            c_id = f"cluster_loc_{n['id']}"
            cluster_names[c_id] = n["name"]
            if "out_edges" in n and "contains" in n["out_edges"]:
                for target in n["out_edges"]["contains"]:
                    clusters[target["targetId"]] = c_id
            clusters[n["id"]] = c_id # Location node itself is in its cluster
            
        if n["type"] == "Cadence":
            c_id = f"cluster_cad_{n['id']}"
            cluster_names[c_id] = n["name"]
            if "out_edges" in n and "provides_ability" in n["out_edges"]:
                for target in n["out_edges"]["provides_ability"]:
                    clusters[target["targetId"]] = c_id
            clusters[n["id"]] = c_id

        if n["type"] == "Refinement":
            # Group by category (Magic vs Material)
            # Usually based on output item or primary stat
            is_magic = n.get("data", {}).get("primary_stat") in ["Magic", "Speed"]
            c_id = "cluster_ref_magic" if is_magic else "cluster_ref_material"
            cluster_names[c_id] = "Workshop: Magic" if is_magic else "Workshop: Materials"
            clusters[n["id"]] = c_id

    # Attach to nodes
    for n in nodes:
        n["tier"] = tiers.get(n["id"], 0)
        n["cluster_id"] = clusters.get(n["id"], "cluster_none")
    
    return nodes, cluster_names

def generate_html(nodes, cluster_names):
    html_template = f"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mythril Visual Dashboard V2</title>
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
            width: 320px;
            background-color: var(--panel-bg);
            border-left: 1px solid var(--border-color);
            padding: 20px;
            overflow-y: auto;
            display: none;
            box-shadow: -4px 0 10px rgba(0,0,0,0.3);
            z-index: 50;
        }}

        #graph-svg {{
            width: 100%;
            height: 100%;
            cursor: grab;
        }}

        #graph-svg:active {{
            cursor: grabbing;
        }}

        /* Cluster Boxes */
        .cluster-box {{
            fill: rgba(88, 166, 255, 0.03);
            stroke: rgba(88, 166, 255, 0.15);
            stroke-width: 1px;
            stroke-dasharray: 5,5;
            rx: 15;
            ry: 15;
        }}

        .cluster-label {{
            font-size: 14px;
            font-weight: bold;
            fill: rgba(255, 255, 255, 0.3);
            text-transform: uppercase;
            letter-spacing: 1px;
            pointer-events: none;
        }}

        /* Nodes */
        .node circle, .node rect, .node polygon {{
            stroke-width: 2px;
            stroke: rgba(0,0,0,0.5);
        }}

        .node.dimmed {{ opacity: 0.2; }}
        .node.highlighted {{ opacity: 1; stroke: #fff; stroke-width: 3px; }}

        /* Edges */
        .edge {{
            fill: none;
            stroke: #30363d;
            stroke-width: 1.5px;
            marker-end: url(#arrowhead);
        }}

        .edge.dimmed {{ opacity: 0.1; }}
        .edge.highlighted-up {{ stroke: #ff7b72; stroke-width: 3px; opacity: 1; }}
        .edge.highlighted-down {{ stroke: #7ee787; stroke-width: 3px; opacity: 1; }}

        .label {{
            font-size: 11px;
            font-weight: 500;
            fill: var(--text-color);
            pointer-events: none;
            text-shadow: 0 1px 2px rgba(0,0,0,0.8);
        }}

        /* Tier Indicators */
        .tier-line {{
            stroke: rgba(255,255,255,0.05);
            stroke-width: 1px;
            stroke-dasharray: 10,10;
        }}

        .tier-label {{
            font-size: 10px;
            fill: rgba(255,255,255,0.2);
            text-anchor: middle;
        }}

        /* Hierarchy View */
        #hierarchy-view {{
            width: 100%;
            height: 100%;
            display: none;
            overflow: auto;
            padding: 40px;
            box-sizing: border-box;
            background: #0d1117;
        }}

        .tier-column {{
            display: inline-flex;
            flex-direction: column;
            gap: 15px;
            vertical-align: top;
            min-width: 280px;
            padding: 0 40px;
            border-right: 1px solid var(--border-color);
        }}

        .tier-header {{
            font-weight: bold;
            font-size: 18px;
            margin-bottom: 20px;
            color: var(--accent-color);
            position: sticky;
            top: 0;
            background: var(--bg-color);
            padding: 10px 0;
            z-index: 10;
        }}

        .card {{
            background-color: var(--panel-bg);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            padding: 12px;
            font-size: 13px;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }}

        .card:hover {{
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.4);
            border-color: var(--accent-color);
        }}

        .card-type {{
            font-size: 10px;
            text-transform: uppercase;
            font-weight: bold;
            margin-bottom: 6px;
            opacity: 0.8;
        }}

        .card-name {{
            font-weight: bold;
            margin-bottom: 6px;
            font-size: 14px;
        }}

        .tooltip {{
            position: absolute;
            background: rgba(22, 27, 34, 0.95);
            border: 1px solid var(--border-color);
            padding: 12px;
            border-radius: 8px;
            pointer-events: none;
            display: none;
            z-index: 1000;
            max-width: 280px;
            box-shadow: 0 8px 24px rgba(0,0,0,0.6);
            font-size: 13px;
        }}
    </style>
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
                    <button onclick="sidebar.style.display='none'" style="padding:2px 8px;">×</button>
                </div>
                <p id="side-type" style="margin-top:5px; margin-bottom: 20px;"></p>
                <div id="side-content"></div>
            </div>
        </div>
    </div>
    <div id="tooltip" class="tooltip"></div>

    <script>
        const nodesData = {json.dumps(nodes)};
        const clusterNames = {json.dumps(cluster_names)};
        
        // --- Configuration ---
        const TIER_WIDTH = 450;
        const NODE_RADIUS = 15;
        
        // --- State ---
        let currentView = 'lattice';
        const viewport = document.getElementById('viewport');
        const svg = document.getElementById('graph-svg');
        const tooltip = document.getElementById('tooltip');
        const sidebar = document.getElementById('sidebar');

        let nodes = [];
        let edges = [];
        let transform = {{ x: 50, y: 50, k: 0.6 }};
        let nodeMap = new Map();
        let simulationFrame = 0;
        const MAX_SIM_FRAMES = 300;
        let isSimulating = true;
        let draggedNode = null;

        
        // --- Initialization ---
        function init() {{
            processData();
            setupInteractions();
            renderTiers();
            renderLattice();
            updateStats();
            updateTransform();
        }}

        function processData() {{
            nodes = nodesData.map(d => ({{
                ...d,
                // Initial position based on tier
                x: d.tier * TIER_WIDTH + (Math.random() - 0.5) * 200,
                y: (window.innerHeight / 2) + (Math.random() - 0.5) * 600,
                vx: 0,
                vy: 0
            }}));

            nodeMap = new Map(nodes.map(n => [n.id, n]));

            nodes.forEach(node => {{
                if (node.out_edges) {{
                    Object.entries(node.out_edges).forEach(([type, targetList]) => {{
                        targetList.forEach(target => {{
                            const targetId = typeof target === 'string' ? target : target.targetId;
                            if (nodeMap.has(targetId)) {{
                                edges.push({{
                                    id: `edge-${{node.id}}-${{targetId}}`,
                                    source: node.id,
                                    target: targetId,
                                    type: type
                                }});
                            }}
                        }});
                    }});
                }}
                if (node.in_edges) {{
                    Object.entries(node.in_edges).forEach(([type, sourceList]) => {{
                        sourceList.forEach(sourceId => {{
                            if (nodeMap.has(sourceId)) {{
                                edges.push({{
                                    id: `edge-${{sourceId}}-${{node.id}}`,
                                    source: sourceId,
                                    target: node.id,
                                    type: type
                                }});
                            }}
                        }});
                    }});
                }}
            }});
            
            // Deduplicate edges
            const seenEdges = new Set();
            edges = edges.filter(e => {{
                const key = `${{e.source}}-${{e.target}}`;
                if (seenEdges.has(key)) return false;
                seenEdges.add(key);
                return true;
            }});
        }}

        function updateStats() {{
            document.getElementById('stats').innerText = `NODES: ${{nodes.length}} | EDGES: ${{edges.length}} | TIERS: ${{Math.max(...nodes.map(n=>n.tier))+1}}`;
        }}

        // --- Lattice View (Custom Simulation) ---
        function renderTiers() {{
            const layer = document.getElementById('tiers-layer');
            const maxTier = Math.max(...nodes.map(n => n.tier));
            for(let i=0; i<=maxTier; i++) {{
                const x = i * TIER_WIDTH;
                const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute('class', 'tier-line');
                line.setAttribute('x1', x);
                line.setAttribute('y1', -5000);
                line.setAttribute('x2', x);
                line.setAttribute('y2', 5000);
                layer.appendChild(line);

                const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
                text.setAttribute('class', 'tier-label');
                text.setAttribute('x', x);
                text.setAttribute('y', 20);
                text.textContent = `PROGRESSION TIER ${{i}}`;
                layer.appendChild(text);
            }}
        }}

        function renderLattice() {{
            const nodesLayer = document.getElementById('nodes-layer');
            const edgesLayer = document.getElementById('edges-layer');
            nodesLayer.innerHTML = '';
            edgesLayer.innerHTML = '';

            // Render Edges
            edges.forEach(edge => {{
                const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
                path.setAttribute('class', 'edge');
                path.setAttribute('id', edge.id);
                edgesLayer.appendChild(path);
                edge.el = path;
            }});

            // Render Nodes
            nodes.forEach(node => {{
                const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
                g.setAttribute('class', 'node');
                g.setAttribute('id', `node-${{node.id}}`);
                
                let shape;
                const color = getCategoryColor(node.type);
                
                if (node.type === 'Quest') {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                    shape.setAttribute('points', '-16,-8 -16,8 0,16 16,8 16,-8 0,-16');
                }} else if (node.type === 'Ability') {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                    shape.setAttribute('points', '0,-16 16,0 0,16 -16,0');
                }} else if (node.type === 'Item') {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                    shape.setAttribute('r', '14');
                }} else if (node.type === 'Cadence') {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                    shape.setAttribute('x', '-14');
                    shape.setAttribute('y', '-14');
                    shape.setAttribute('width', '28');
                    shape.setAttribute('height', '28');
                    shape.setAttribute('rx', '4');
                }} else {{
                    shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                    shape.setAttribute('x', '-12');
                    shape.setAttribute('y', '-12');
                    shape.setAttribute('width', '24');
                    shape.setAttribute('height', '24');
                }}
                
                shape.setAttribute('fill', color);
                g.appendChild(shape);

                const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
                text.setAttribute('class', 'label');
                text.setAttribute('y', '32');
                text.setAttribute('text-anchor', 'middle');
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
            if (!isSimulating && !draggedNode) return;

            // Increment frame only if not dragging
            if (isSimulating && !draggedNode) {{
                simulationFrame++;
                if (simulationFrame > MAX_SIM_FRAMES) {{
                    isSimulating = false;
                    console.log("Simulation settled.");
                }}
            }}

            // 1. Calculate Forces
            const clusters = new Map();

            nodes.forEach(n => {{
                if (n === draggedNode) return;

                // PROGRESSION FORCE (Snap to X tier)
                const targetX = n.tier * TIER_WIDTH;
                n.vx += (targetX - n.x) * 0.05;

                // CENTER Y FORCE
                n.vy += (window.innerHeight / 2 - n.y) * 0.005;

                // REPULSION
                nodes.forEach(m => {{
                    if (n === m) return;
                    const dx = n.x - m.x;
                    const dy = n.y - m.y;
                    const distSq = dx * dx + dy * dy || 1;
                    if (distSq < 2500) {{ // Very close repulsion
                        const force = 50 / distSq;
                        n.vx += dx * force;
                        n.vy += dy * force;
                    }} else if (distSq < 40000) {{ // Distant repulsion
                        const force = 10 / distSq;
                        n.vx += dx * force;
                        n.vy += dy * force;
                    }}
                }});

                // Collect cluster centroids
                if (n.cluster_id !== 'cluster_none') {{
                    if (!clusters.has(n.cluster_id)) clusters.set(n.cluster_id, {{ x: 0, y: 0, count: 0 }});
                    const c = clusters.get(n.cluster_id);
                    c.x += n.x;
                    c.y += n.y;
                    c.count++;
                }}
            }});

            // CLUSTER FORCE
            for (const [id, c] of clusters.entries()) {{
                const avgX = c.x / c.count;
                const avgY = c.y / c.count;
                nodes.filter(n => n.cluster_id === id).forEach(n => {{
                    n.vx += (avgX - n.x) * 0.02;
                    n.vy += (avgY - n.y) * 0.02;
                }});
            }}

            // EDGE ATTRACTION
            edges.forEach(e => {{
                const s = nodeMap.get(e.source);
                const t = nodeMap.get(e.target);
                const dx = t.x - s.x;
                const dy = t.y - s.y;
                const dist = Math.sqrt(dx * dx + dy * dy) || 1;
                // Stronger attraction if they are in different tiers to pull them together
                const strength = Math.abs(s.tier - t.tier) > 0 ? 0.01 : 0.05;
                const force = (dist - 150) * strength;
                const fx = (dx / dist) * force;
                const fy = (dy / dist) * force;
                s.vx += fx;
                s.vy += fy;
                t.vx -= fx;
                t.vy -= fy;
            }});

            // 2. Apply Velocity & Render
            nodes.forEach(n => {{
                n.x += n.vx;
                n.y += n.vy;
                n.vx *= 0.7; // Friction
                n.vy *= 0.7;
                n.el.setAttribute('transform', `translate(${{n.x}}, ${{n.y}})`);
            }});

            edges.forEach(e => {{
                const s = nodeMap.get(e.source);
                const t = nodeMap.get(e.target);
                // Curved edge
                const midX = (s.x + t.x) / 2;
                const path = `M ${{s.x}} ${{s.y}} Q ${{midX}} ${{s.y + (t.y - s.y) * 0.1}} ${{t.x}} ${{t.y}}`;
                e.el.setAttribute('d', path);
            }});

            // 3. Render Clusters
            renderClusterBoxes(clusters);

            requestAnimationFrame(simulationStep);
        }}

        function renderClusterBoxes(clusters) {{
            const layer = document.getElementById('clusters-layer');
            layer.innerHTML = '';
            
            for (const [id, c] of clusters.entries()) {{
                if (id === 'cluster_none') continue;
                
                const clusterNodes = nodes.filter(n => n.cluster_id === id);
                if (clusterNodes.length < 2) continue;

                let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
                clusterNodes.forEach(n => {{
                    minX = Math.min(minX, n.x);
                    minY = Math.min(minY, n.y);
                    maxX = Math.max(maxX, n.x);
                    maxY = Math.max(maxY, n.y);
                }});

                const padding = 50;
                const rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                rect.setAttribute('class', 'cluster-box');
                rect.setAttribute('x', minX - padding);
                rect.setAttribute('y', minY - padding);
                rect.setAttribute('width', maxX - minX + padding * 2);
                rect.setAttribute('height', maxY - minY + padding * 2);
                layer.appendChild(rect);

                const label = document.createElementNS("http://www.w3.org/2000/svg", "text");
                label.setAttribute('class', 'cluster-label');
                label.setAttribute('x', minX - padding + 10);
                label.setAttribute('y', minY - padding - 10);
                label.textContent = clusterNames[id] || id;
                layer.appendChild(label);
            }}
        }}

        // --- Hierarchy View ---
        function renderHierarchy() {{
            const container = document.getElementById('hierarchy-view');
            container.innerHTML = '';
            
            const maxTier = Math.max(...nodes.map(n => n.tier));
            const tiers = Array.from({{ length: maxTier + 1 }}, () => []);
            
            nodes.forEach(n => tiers[n.tier].push(n));

            tiers.forEach((tierNodes, i) => {{
                const col = document.createElement('div');
                col.className = 'tier-column';
                col.innerHTML = `<div class="tier-header">Tier ${{i}}</div>`;
                
                tierNodes.sort((a,b) => a.type.localeCompare(b.type));
                
                tierNodes.forEach(node => {{
                    const card = document.createElement('div');
                    card.className = 'card';
                    card.style.borderLeft = `5px solid ${{getCategoryColor(node.type)}}`;
                    card.innerHTML = `
                        <div class="card-type" style="color: ${{getCategoryColor(node.type)}}">${{node.type}}</div>
                        <div class="card-name">${{node.name}}</div>
                        <div style="font-size: 11px; opacity: 0.6; height: 32px; overflow: hidden; text-overflow: ellipsis;">${{node.data.description || ''}}</div>
                    `;
                    card.addEventListener('click', () => selectNode(node));
                    col.appendChild(card);
                }});
                
                container.appendChild(col);
            }});
        }}

        // --- Interactions & Path Highlighting ---
        function selectNode(node) {{
            sidebar.style.display = 'block';
            document.getElementById('side-name').innerText = node.name;
            document.getElementById('side-type').innerText = node.type;
            document.getElementById('side-type').style.color = getCategoryColor(node.type);
            
            let content = `<p style="line-height:1.6; font-size: 14px;">${{node.data.description || 'No description available.'}}</p>`;
            
            if (node.data.quest_type) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Quest Type:</strong> ${{node.data.quest_type}}</div>`;
            if (node.data.primary_stat) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Primary Stat:</strong> ${{node.data.primary_stat}}</div>`;

            // Upstream Requirements
            const upstream = edges.filter(e => e.target === node.id);
            if (upstream.length > 0) {{
                content += '<h4 style="border-bottom: 1px solid #30363d; padding-bottom: 5px;">Requirements</h4><ul style="padding-left: 20px;">';
                upstream.forEach(r => {{
                    const src = nodeMap.get(r.source);
                    content += `<li style="margin-bottom: 5px;">${{src ? src.name : r.source}} <span style="font-size: 11px; color: #8b949e;">(${{r.type}})</span></li>`;
                }});
                content += '</ul>';
            }}

            // Downstream Unlocks
            const downstream = edges.filter(e => e.source === node.id);
            if (downstream.length > 0) {{
                content += '<h4 style="border-bottom: 1px solid #30363d; padding-bottom: 5px;">Unlocks / Produces</h4><ul style="padding-left: 20px;">';
                downstream.forEach(u => {{
                    const tgt = nodeMap.get(u.target);
                    content += `<li style="margin-bottom: 5px;">${{tgt ? tgt.name : u.target}} <span style="font-size: 11px; color: #8b949e;">(${{u.type}})</span></li>`;
                }});
                content += '</ul>';
            }}

            document.getElementById('side-content').innerHTML = content;

            highlightPaths(node.id);
        }}

        function highlightPaths(targetId) {{
            const upstreamNodes = new Set();
            const downstreamNodes = new Set();
            const upstreamEdges = new Set();
            const downstreamEdges = new Set();

            // Trace Up
            const traceUp = (id) => {{
                edges.forEach(e => {{
                    if (e.target === id && !upstreamEdges.has(e.id)) {{
                        upstreamEdges.add(e.id);
                        upstreamNodes.add(e.source);
                        traceUp(e.source);
                    }}
                }});
            }};

            // Trace Down
            const traceDown = (id) => {{
                edges.forEach(e => {{
                    if (e.source === id && !downstreamEdges.has(e.id)) {{
                        downstreamEdges.add(e.id);
                        downstreamNodes.add(e.target);
                        traceDown(e.target);
                    }}
                }});
            }};

            traceUp(targetId);
            traceDown(targetId);

            // Apply Classes
            nodes.forEach(n => {{
                const el = n.el;
                el.classList.remove('dimmed', 'highlighted');
                if (n.id === targetId) {{
                    el.classList.add('highlighted');
                }} else if (upstreamNodes.has(n.id) || downstreamNodes.has(n.id)) {{
                    el.classList.add('highlighted');
                }} else {{
                    el.classList.add('dimmed');
                }}
            }});

            edges.forEach(e => {{
                e.el.classList.remove('dimmed', 'highlighted-up', 'highlighted-down');
                if (upstreamEdges.has(e.id)) {{
                    e.el.classList.add('highlighted-up');
                }} else if (downstreamEdges.has(e.id)) {{
                    e.el.classList.add('highlighted-down');
                }} else {{
                    e.el.classList.add('dimmed');
                }}
            }});
        }}

        function setupInteractions() {{
            document.querySelectorAll('.node').forEach(nodeEl => {{
                nodeEl.addEventListener('mousedown', e => {{
                    if (currentView !== 'lattice') return;
                    e.stopPropagation();
                    const nodeId = nodeEl.id.replace('node-', '');
                    draggedNode = nodeMap.get(nodeId);
                    isSimulating = true; // Wake up simulation
                    svg.style.cursor = 'grabbing';
                }});
            }});

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

            document.getElementById('btn-reset').addEventListener('click', () => {{
                nodes.forEach(n => {{
                    n.x = n.tier * TIER_WIDTH + (Math.random() - 0.5) * 100;
                    n.y = window.innerHeight / 2 + (Math.random() - 0.5) * 400;
                    n.vx = 0; n.vy = 0;
                }});
                transform = {{ x: 50, y: 50, k: 0.6 }};
                updateTransform();
            }});

            // Zoom and Pan
            let isDragging = false;
            let startPos = {{ x: 0, y: 0 }};

            svg.addEventListener('mousedown', e => {{
                if (e.target === svg || e.target.closest('#clusters-layer') || e.target.closest('#tiers-layer')) {{
                    isDragging = true;
                    startPos = {{ x: e.clientX - transform.x, y: e.clientY - transform.y }};
                    svg.style.cursor = 'grabbing';
                }}
            }});

            window.addEventListener('mousemove', e => {{
                if (draggedNode) {{
                    const rect = svg.getBoundingClientRect();
                    draggedNode.x = (e.clientX - rect.left - transform.x) / transform.k;
                    draggedNode.y = (e.clientY - rect.top - transform.y) / transform.k;
                }} else if (isDragging) {{
                    transform.x = e.clientX - startPos.x;
                    transform.y = e.clientY - startPos.y;
                    updateTransform();
                }}
                if (tooltip.style.display === 'block') {{
                    tooltip.style.left = (e.pageX + 15) + 'px';
                    tooltip.style.top = (e.pageY + 15) + 'px';
                }}
            }});

            window.addEventListener('mouseup', () => {{
                isDragging = false;
                draggedNode = null;
                svg.style.cursor = 'grab';
            }});

            svg.addEventListener('wheel', e => {{
                e.preventDefault();
                const delta = e.deltaY > 0 ? 0.9 : 1.1;
                const mouseX = e.clientX;
                const mouseY = e.clientY;
                
                // Zoom relative to mouse
                const beforeX = (mouseX - transform.x) / transform.k;
                const beforeY = (mouseY - transform.y) / transform.k;
                
                transform.k *= delta;
                transform.k = Math.max(0.1, Math.min(transform.k, 3));
                
                transform.x = mouseX - beforeX * transform.k;
                transform.y = mouseY - beforeY * transform.k;
                
                updateTransform();
            }}, {{ passive: false }});
        }}

        function updateTransform() {{
            viewport.setAttribute('transform', `translate(${{transform.x}}, ${{transform.y}}) scale(${{transform.k}})`);
        }}

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
            tooltip.innerHTML = `
                <div style="color: ${{getCategoryColor(node.type)}}; font-size: 10px; font-weight: bold; text-transform: uppercase;">${{node.type}}</div>
                <div style="font-weight: bold; font-size: 15px; margin: 4px 0;">${{node.name}}</div>
                <div style="opacity: 0.7; font-size: 12px;">${{node.data.description || ''}}</div>
                <div style="margin-top: 8px; font-size: 11px; color: var(--accent-color);">Tier ${{node.tier}}</div>
            `;
        }}

        function hideTooltip() {{
            tooltip.style.display = 'none';
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
    print("Generating Clustered & Chronological Visual Dashboard...")
    try:
        graph_data = load_graph()
        nodes, cluster_names = enrich_data(graph_data)
        
        # Debug cluster info
        cluster_counts = {}
        for n in nodes:
            cid = n["cluster_id"]
            cluster_counts[cid] = cluster_counts.get(cid, 0) + 1
        print(f"Clusters identified: {len(cluster_counts)} groups")
        for cid, count in cluster_counts.items():
            if cid != "cluster_none":
                print(f" - {cluster_names.get(cid, cid)}: {count} nodes")

        html = generate_html(nodes, cluster_names)
        
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

        port = 8000
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
