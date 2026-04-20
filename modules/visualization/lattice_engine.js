
// --- Configuration ---
const TIER_WIDTH = 800;
const NODE_RADIUS = 15;

// --- State ---
let currentView = 'lattice';
const viewport = document.getElementById('viewport');
const svg = document.getElementById('graph-svg');
const tooltip = document.getElementById('tooltip');
const sidebar = document.getElementById('sidebar');

let nodes = [];
let edges = [];
let transform = { x: 50, y: 50, k: 0.6 };
let nodeMap = new Map();
let simulationFrame = 0;
const MAX_SIM_FRAMES = 300;
let isSimulating = true;
let draggedNode = null;

// --- Initialization ---
function init() {
    processData();
    setupInteractions();
    renderTiers();
    renderLattice();
    updateStats();
    updateTransform();
}

function processData() {
    nodes = nodesData.map(d => ({
        ...d,
        x: d.tier * TIER_WIDTH + (Math.random() - 0.5) * 400,
        y: (window.innerHeight / 2) + (Math.random() - 0.5) * 2000,
        vx: 0, vy: 0
    }));

    nodeMap = new Map(nodes.map(n => [n.id, n]));

    nodes.forEach(node => {
        if (node.out_edges) {
            Object.entries(node.out_edges).forEach(([type, targetList]) => {
                targetList.forEach(target => {
                    const targetId = typeof target === 'string' ? target : target.targetId;
                    if (nodeMap.has(targetId)) {
                        edges.push({
                            id: `edge-${node.id}-${targetId}`,
                            source: node.id, target: targetId, type: type
                        });
                    }
                });
            });
        }
        if (node.in_edges) {
            Object.entries(node.in_edges).forEach(([type, sourceList]) => {
                sourceList.forEach(sourceId => {
                    if (nodeMap.has(sourceId)) {
                        edges.push({
                            id: `edge-${sourceId}-${node.id}`,
                            source: sourceId, target: node.id, type: type
                        });
                    }
                });
            });
        }
    });
    
    const seenEdges = new Set();
    edges = edges.filter(e => {
        const key = `${e.source}-${e.target}`;
        if (seenEdges.has(key)) return false;
        seenEdges.add(key);
        return true;
    });
}

function updateStats() {
    document.getElementById('stats').innerText = `NODES: ${nodes.length} | EDGES: ${edges.length} | TIERS: ${Math.max(...nodes.map(n=>n.tier))+1}`;
}

function renderTiers() {
    const layer = document.getElementById('tiers-layer');
    const maxTier = Math.max(...nodes.map(n => n.tier));
    for(let i=0; i<=maxTier; i++) {
        const x = i * TIER_WIDTH;
        const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
        line.setAttribute('class', 'tier-line');
        line.setAttribute('x1', x); line.setAttribute('y1', -5000);
        line.setAttribute('x2', x); line.setAttribute('y2', 5000);
        layer.appendChild(line);

        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute('class', 'tier-label');
        text.setAttribute('x', x); text.setAttribute('y', 20);
        text.textContent = `PROGRESSION TIER ${i}`;
        layer.appendChild(text);
    }
}

function renderLattice() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';

    edges.forEach(edge => {
        const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path.setAttribute('class', 'edge');
        path.setAttribute('id', edge.id);
        edgesLayer.appendChild(path);
        edge.el = path;
    });

    nodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        g.setAttribute('class', 'node');
        g.setAttribute('id', `node-${node.id}`);
        
        let shape;
        const color = getCategoryColor(node.type);
        
        if (node.type === 'Quest') {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
            shape.setAttribute('points', '-16,-8 -16,8 0,16 16,8 16,-8 0,-16');
        } else if (node.type === 'Ability') {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
            shape.setAttribute('points', '0,-16 16,0 0,16 -16,0');
        } else if (node.type === 'Item') {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            shape.setAttribute('r', '14');
        } else if (node.type === 'Cadence') {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
            shape.setAttribute('x', '-14'); shape.setAttribute('y', '-14');
            shape.setAttribute('width', '28'); shape.setAttribute('height', '28');
            shape.setAttribute('rx', '4');
        } else {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
            shape.setAttribute('x', '-12'); shape.setAttribute('y', '-12');
            shape.setAttribute('width', '24'); shape.setAttribute('height', '24');
        }
        
        shape.setAttribute('fill', color);
        g.appendChild(shape);

        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute('class', 'label');
        text.setAttribute('y', '32'); text.setAttribute('text-anchor', 'middle');
        text.textContent = node.name;
        g.appendChild(text);

        g.addEventListener('mouseenter', (e) => showTooltip(e, node));
        g.addEventListener('mouseleave', hideTooltip);
        g.addEventListener('click', () => selectNode(node));

        nodesLayer.appendChild(g);
        node.el = g;
    });

    requestAnimationFrame(simulationStep);
}

function simulationStep() {
    if (currentView !== 'lattice') return;
    if (!isSimulating && !draggedNode) return;

    if (isSimulating && !draggedNode) {
        simulationFrame++;
        if (simulationFrame > MAX_SIM_FRAMES) {
            isSimulating = false;
            console.log("Simulation settled.");
        }
    }

    const clusters = new Map();

    nodes.forEach(n => {
        if (n === draggedNode) return;
        n.vx += (n.tier * TIER_WIDTH - n.x) * 0.05;
        n.vy += (window.innerHeight / 2 - n.y) * 0.01;

        nodes.forEach(m => {
            if (n === m) return;
            const dx = n.x - m.x, dy = n.y - m.y;
            const distSq = dx * dx + dy * dy || 1;
            if (distSq < 3600) { 
                const force = 100 / Math.sqrt(distSq);
                n.vx += dx * force * 0.5; n.vy += dy * force * 0.5;
            } else if (distSq < 100000) {
                const force = 20 / distSq;
                n.vx += dx * force; n.vy += dy * force;
            }
        });

        if (n.cluster_id !== 'cluster_none') {
            if (!clusters.has(n.cluster_id)) clusters.set(n.cluster_id, { x: 0, y: 0, count: 0 });
            const c = clusters.get(n.cluster_id);
            c.x += n.x; c.y += n.y; c.count++;
        }
    });

    for (const [id, c] of clusters.entries()) {
        const avgX = c.x / c.count, avgY = c.y / c.count;
        nodes.filter(n => n.cluster_id === id).forEach(n => {
            n.vx += (avgX - n.x) * 0.02; n.vy += (avgY - n.y) * 0.02;
        });
    }

    edges.forEach(e => {
        const s = nodeMap.get(e.source), t = nodeMap.get(e.target);
        const dx = t.x - s.x, dy = t.y - s.y;
        const dist = Math.sqrt(dx * dx + dy * dy) || 1;
        const strength = Math.abs(s.tier - t.tier) > 0 ? 0.01 : 0.05;
        const force = (dist - 150) * strength;
        const fx = (dx / dist) * force, fy = (dy / dist) * force;
        s.vx += fx; s.vy += fy; t.vx -= fx; t.vy -= fy;
    });

    nodes.forEach(n => {
        n.x += n.vx; n.y += n.vy; n.vx *= 0.7; n.vy *= 0.7;
        n.el.setAttribute('transform', `translate(${n.x}, ${n.y})`);
    });

    edges.forEach(e => {
        const s = nodeMap.get(e.source), t = nodeMap.get(e.target);
        const midX = (s.x + t.x) / 2;
        const path = `M ${s.x} ${s.y} Q ${midX} ${s.y + (t.y - s.y) * 0.1} ${t.x} ${t.y}`;
        e.el.setAttribute('d', path);
    });

    renderClusterBoxes(clusters);
    requestAnimationFrame(simulationStep);
}

function renderClusterBoxes(clusters) {
    const layer = document.getElementById('clusters-layer');
    layer.innerHTML = '';
    for (const [id, c] of clusters.entries()) {
        if (id === 'cluster_none') continue;
        const clusterNodes = nodes.filter(n => n.cluster_id === id);
        if (clusterNodes.length < 2) continue;
        let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
        clusterNodes.forEach(n => {
            minX = Math.min(minX, n.x); minY = Math.min(minY, n.y);
            maxX = Math.max(maxX, n.x); maxY = Math.max(maxY, n.y);
        });
        const padding = 50;
        const rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        rect.setAttribute('class', 'cluster-box');
        rect.setAttribute('x', minX - padding); rect.setAttribute('y', minY - padding);
        rect.setAttribute('width', maxX - minX + padding * 2);
        rect.setAttribute('height', maxY - minY + padding * 2);
        layer.appendChild(rect);
        const label = document.createElementNS("http://www.w3.org/2000/svg", "text");
        label.setAttribute('class', 'cluster-label');
        label.setAttribute('x', minX - padding + 10); label.setAttribute('y', minY - padding - 10);
        label.textContent = clusterNames[id] || id;
        layer.appendChild(label);
    }
}

function renderHierarchy() {
    const container = document.getElementById('hierarchy-view');
    container.innerHTML = '';
    const maxTier = Math.max(...nodes.map(n => n.tier));
    const tiers = Array.from({ length: maxTier + 1 }, () => []);
    nodes.forEach(n => tiers[n.tier].push(n));
    tiers.forEach((tierNodes, i) => {
        const col = document.createElement('div');
        col.className = 'tier-column';
        col.innerHTML = `<div class="tier-header">Tier ${i}</div>`;
        tierNodes.sort((a,b) => a.type.localeCompare(b.type));
        tierNodes.forEach(node => {
            const card = document.createElement('div');
            card.className = 'card';
            card.style.borderLeft = `5px solid ${getCategoryColor(node.type)}`;
            card.innerHTML = `<div class="card-type" style="color: ${getCategoryColor(node.type)}">${node.type}</div>
                <div class="card-name">${node.name}</div>
                <div style="font-size: 11px; opacity: 0.6; height: 32px; overflow: hidden;">${node.data.description || ''}</div>`;
            card.addEventListener('click', () => selectNode(node));
            col.appendChild(card);
        });
        container.appendChild(col);
    });
}

function selectNode(node) {
    sidebar.style.display = 'block';
    document.getElementById('side-name').innerText = node.name;
    document.getElementById('side-type').innerText = node.type;
    document.getElementById('side-type').style.color = getCategoryColor(node.type);
    let content = `<p style="line-height:1.6; font-size: 14px;">${node.data.description || 'No description available.'}</p>`;
    if (node.data.quest_type) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Quest Type:</strong> ${node.data.quest_type}</div>`;
    if (node.data.primary_stat) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Primary Stat:</strong> ${node.data.primary_stat}</div>`;
    const upstream = edges.filter(e => e.target === node.id);
    if (upstream.length > 0) {
        content += '<h4 style="border-bottom: 1px solid #30363d;">Requirements</h4><ul>';
        upstream.forEach(r => { const src = nodeMap.get(r.source); content += `<li>${src ? src.name : r.source} <span style="font-size:11px;">(${r.type})</span></li>`; });
        content += '</ul>';
    }
    const downstream = edges.filter(e => e.source === node.id);
    if (downstream.length > 0) {
        content += '<h4 style="border-bottom: 1px solid #30363d;">Unlocks</h4><ul>';
        downstream.forEach(u => { const tgt = nodeMap.get(u.target); content += `<li>${tgt ? tgt.name : u.target} <span style="font-size:11px;">(${u.type})</span></li>`; });
        content += '</ul>';
    }
    document.getElementById('side-content').innerHTML = content;
    highlightPaths(node.id);
}

function highlightPaths(targetId) {
    const upN = new Set(), downN = new Set(), upE = new Set(), downE = new Set();
    const traceUp = (id) => { edges.forEach(e => { if (e.target === id && !upE.has(e.id)) { upE.add(e.id); upN.add(e.source); traceUp(e.source); } }); };
    const traceDown = (id) => { edges.forEach(e => { if (e.source === id && !downE.has(e.id)) { downE.add(e.id); downN.add(e.target); traceDown(e.target); } }); };
    traceUp(targetId); traceDown(targetId);
    nodes.forEach(n => {
        n.el.classList.remove('dimmed', 'highlighted');
        if (n.id === targetId || upN.has(n.id) || downN.has(n.id)) n.el.classList.add('highlighted');
        else n.el.classList.add('dimmed');
    });
    edges.forEach(e => {
        e.el.classList.remove('dimmed', 'highlighted-up', 'highlighted-down');
        if (upE.has(e.id)) e.el.classList.add('highlighted-up');
        else if (downE.has(e.id)) e.el.classList.add('highlighted-down');
        else e.el.classList.add('dimmed');
    });
}

function setupInteractions() {
    document.querySelectorAll('.node').forEach(nodeEl => {
        nodeEl.addEventListener('mousedown', e => {
            if (currentView !== 'lattice') return;
            e.stopPropagation();
            draggedNode = nodeMap.get(nodeEl.id.replace('node-', ''));
            isSimulating = true; svg.style.cursor = 'grabbing';
        });
    });

    document.getElementById('btn-lattice').addEventListener('click', () => {
        currentView = 'lattice';
        document.getElementById('btn-lattice').classList.add('active');
        document.getElementById('btn-hierarchy').classList.remove('active');
        document.getElementById('graph-svg').style.display = 'block';
        document.getElementById('hierarchy-view').style.display = 'none';
        requestAnimationFrame(simulationStep);
    });

    document.getElementById('btn-hierarchy').addEventListener('click', () => {
        currentView = 'hierarchy';
        document.getElementById('btn-hierarchy').classList.add('active');
        document.getElementById('btn-lattice').classList.remove('active');
        document.getElementById('graph-svg').style.display = 'none';
        document.getElementById('hierarchy-view').style.display = 'flex';
        renderHierarchy();
    });

    document.getElementById('btn-reset').addEventListener('click', () => {
        nodes.forEach(n => {
            n.x = n.tier * TIER_WIDTH + (Math.random() - 0.5) * 100;
            n.y = window.innerHeight / 2 + (Math.random() - 0.5) * 400;
            n.vx = 0; n.vy = 0;
        });
        transform = { x: 50, y: 50, k: 0.6 }; updateTransform();
    });

    let isDragging = false, startPos = { x: 0, y: 0 };
    svg.addEventListener('mousedown', e => {
        if (e.target === svg || e.target.closest('#clusters-layer') || e.target.closest('#tiers-layer')) {
            isDragging = true; startPos = { x: e.clientX - transform.x, y: e.clientY - transform.y };
            svg.style.cursor = 'grabbing';
        }
    });

    window.addEventListener('mousemove', e => {
        if (draggedNode) {
            const r = svg.getBoundingClientRect();
            draggedNode.x = (e.clientX - r.left - transform.x) / transform.k;
            draggedNode.y = (e.clientY - r.top - transform.y) / transform.k;
        } else if (isDragging) {
            transform.x = e.clientX - startPos.x; transform.y = e.clientY - startPos.y;
            updateTransform();
        }
        if (tooltip.style.display === 'block') { tooltip.style.left = (e.pageX + 15) + 'px'; tooltip.style.top = (e.pageY + 15) + 'px'; }
    });

    window.addEventListener('mouseup', () => { isDragging = false; draggedNode = null; svg.style.cursor = 'grab'; });

    svg.addEventListener('wheel', e => {
        e.preventDefault();
        const delta = e.deltaY > 0 ? 0.9 : 1.1;
        const bX = (e.clientX - transform.x) / transform.k, bY = (e.clientY - transform.y) / transform.k;
        transform.k = Math.max(0.1, Math.min(transform.k * delta, 3));
        transform.x = e.clientX - bX * transform.k; transform.y = e.clientY - bY * transform.k;
        updateTransform();
    }, { passive: false });
}

function updateTransform() { viewport.setAttribute('transform', `translate(${transform.x}, ${transform.y}) scale(${transform.k})`); }

function getCategoryColor(type) {
    const c = { 'Quest': 'var(--quest-color)', 'Item': 'var(--item-color)', 'Ability': 'var(--ability-color)', 'Cadence': 'var(--cadence-color)', 'Location': 'var(--location-color)', 'Stat': 'var(--stat-color)', 'Refinement': 'var(--refinement-color)' };
    return c[type] || '#ccc';
}

function showTooltip(e, node) {
    tooltip.style.display = 'block';
    tooltip.innerHTML = `<div style="color: ${getCategoryColor(node.type)}; font-size: 10px; font-weight: bold; text-transform: uppercase;">${node.type}</div>
        <div style="font-weight: bold; font-size: 15px; margin: 4px 0;">${node.name}</div>
        <div style="opacity: 0.7; font-size: 12px;">${node.data.description || ''}</div>
        <div style="margin-top: 8px; font-size: 11px; color: var(--accent-color);">Tier ${node.tier}</div>`;
}

function hideTooltip() { tooltip.style.display = 'none'; }

init();
