function selectNode(node) {
    sidebar.style.display = 'block';
    document.getElementById('side-name').innerText = node.name;
    document.getElementById('side-type').innerText = node.type;
    document.getElementById('side-type').style.color = getCategoryColor(node.type);
    let content = `<p style="line-height:1.6; font-size: 14px;">${node.data.description || 'No description available.'}</p>`;
    
    if (node.simulation) {
        if (node.simulation.sustainable) content += `<div style="margin: 10px 0; background: rgba(126, 231, 135, 0.1); border-left: 3px solid var(--ability-color); padding: 8px; border-radius: 4px;"><strong>Simulation:</strong> Sustainable</div>`;
        if (node.simulation.unsustainable) content += `<div style="margin: 10px 0; background: rgba(255, 123, 114, 0.1); border-left: 3px solid var(--location-color); padding: 8px; border-radius: 4px;"><strong>Simulation:</strong> Unsustainable</div>`;
        if (node.simulation.net_rate > 0) content += `<div style="margin: 10px 0; background: rgba(210, 153, 34, 0.1); border-left: 3px solid var(--stat-color); padding: 8px; border-radius: 4px;"><strong>Net Rate:</strong> ${node.simulation.net_rate.toFixed(4)}/s</div>`;
    }

    if (node.data.quest_type) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Quest Type:</strong> ${node.data.quest_type}</div>`;
    if (node.data.primary_stat) content += `<div style="margin: 10px 0; background: rgba(255,255,255,0.05); padding: 8px; border-radius: 4px;"><strong>Primary Stat:</strong> ${node.data.primary_stat}</div>`;
    
    const upstream = allEdges.filter(e => e.target === node.id);
    if (upstream.length > 0) {
        content += '<h4 style="border-bottom: 1px solid #30363d;">Requirements</h4><ul>';
        upstream.forEach(r => { const src = nodeMap.get(r.source); content += `<li>${src ? src.name : r.source} <span style="font-size:11px;">(${r.type})</span></li>`; });
        content += '</ul>';
    }
    const downstream = allEdges.filter(e => e.source === node.id);
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
    const traceUp = (id) => { allEdges.forEach(e => { if (e.target === id && !upE.has(e.id)) { upE.add(e.id); upN.add(e.source); traceUp(e.source); } }); };
    const traceDown = (id) => { allEdges.forEach(e => { if (e.source === id && !downE.has(e.id)) { downE.add(e.id); downN.add(e.target); traceDown(e.target); } }); };
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
    document.getElementById('toggle-progression').addEventListener('click', (e) => {
        showProgressionOnly = !showProgressionOnly;
        e.target.classList.toggle('on', showProgressionOnly);
        filterEdges(); renderLattice();
    });

    document.getElementById('toggle-hubs').addEventListener('click', (e) => {
        showHubs = !showHubs;
        e.target.classList.toggle('on', showHubs);
        filterEdges(); renderLattice();
    });

    document.getElementById('toggle-sim').addEventListener('click', (e) => {
        showSimOverlay = !showSimOverlay;
        e.target.classList.toggle('on', showSimOverlay);
        renderLattice();
    });

    document.getElementById('btn-lattice').addEventListener('click', () => {
        currentView = 'lattice';
        document.getElementById('btn-lattice').classList.add('active');
        document.getElementById('btn-hierarchy').classList.remove('active');
        document.getElementById('graph-svg').style.display = 'block';
        document.getElementById('hierarchy-view').style.display = 'none';
        updateLayout();
    });

    document.getElementById('btn-quest-flow').addEventListener('click', () => {
        currentView = 'quest-flow';
        document.getElementById('btn-lattice').classList.remove('active');
        document.getElementById('btn-quest-flow').classList.add('active');
        document.getElementById('btn-hierarchy').classList.remove('active');
        document.getElementById('graph-svg').style.display = 'block';
        document.getElementById('hierarchy-view').style.display = 'none';
        renderQuestFlow();
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
        // Reload original coordinates from data
        processData();
        renderLattice();
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
            updateLayout();
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
    let html = `<div style="color: ${getCategoryColor(node.type)}; font-size: 10px; font-weight: bold; text-transform: uppercase;">${node.type}</div>
        <div style="font-weight: bold; font-size: 15px; margin: 4px 0;">${node.name}</div>
        <div style="opacity: 0.7; font-size: 12px;">${node.data.description || ''}</div>`;
    
    if (node.simulation && (node.simulation.sustainable || node.simulation.unsustainable || node.simulation.net_rate > 0)) {
        html += `<div style="margin-top: 8px; padding-top: 8px; border-top: 1px solid rgba(255,255,255,0.1);">`;
        if (node.simulation.sustainable) html += `<div style="color: var(--ability-color); font-size: 11px;">● Sustainable</div>`;
        if (node.simulation.unsustainable) html += `<div style="color: var(--location-color); font-size: 11px;">● Unsustainable</div>`;
        if (node.simulation.net_rate > 0) html += `<div style="color: var(--stat-color); font-size: 11px;">Net Rate: ${node.simulation.net_rate.toFixed(4)}/s</div>`;
        html += `</div>`;
    }

    html += `<div style="margin-top: 8px; font-size: 11px; color: var(--accent-color);">Tier ${node.tier}</div>`;
    tooltip.innerHTML = html;
}

function hideTooltip() { tooltip.style.display = 'none'; }
