function renderQuestFlow() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';

    // 1. Calculate Flow using modular logic
    const { flowNodes, flowEdges } = calculateLatticeFlow(nodesData, currentView);
    window.nodeMap = new Map(nodesData.map(n => [n.id, n])); // For sidebar/tooltips

    // 2. Draw Edges
    const flowNodeIdMap = new Map(flowNodes.map(n => [n.id, n]));
    flowEdges.forEach(edge => {
        const s = flowNodeIdMap.get(edge.source);
        const t = flowNodeIdMap.get(edge.target);
        if (s && t) {
            const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            const isMilestoneEdge = s.type === 'Milestone' || t.type === 'Milestone';
            path.setAttribute('class', `edge ${edge.category} ${isMilestoneEdge ? 'milestone-link' : ''}`);
            path.setAttribute('id', edge.id);
            const midX = (s.fx + t.fx) / 2;
            path.setAttribute('d', `M ${s.fx} ${s.fy} Q ${midX} ${s.fy + (t.fy - s.fy) * 0.1} ${t.fx} ${t.fy}`);
            if (isMilestoneEdge) path.setAttribute('style', 'stroke: var(--accent-color); stroke-width: 3px; opacity: 0.5;');
            edgesLayer.appendChild(path);
        }
    });

    // 3. Draw Nodes
    flowNodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        const isLocked = currentView === 'progressive' && node.isSustainablyActive === false;
        g.setAttribute('class', `node ${node.type === 'Milestone' ? 'milestone-node' : ''} ${isLocked ? 'locked' : ''}`);
        g.setAttribute('transform', `translate(${node.fx}, ${node.fy})`);
        g.setAttribute('id', `node-${node.id}`);
        
        const shape = createNodeShape(node, isLocked);
        g.appendChild(shape);

        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute('class', 'label'); text.setAttribute('y', node.type === 'Milestone' ? '80' : '32');
        text.setAttribute('text-anchor', 'middle'); text.style.fontSize = '12px';
        text.textContent = node.name;
        if (node.type === 'Milestone') text.style.fontWeight = 'bold';
        g.appendChild(text);

        g.addEventListener('mouseenter', (e) => showTooltip(e, node));
        g.addEventListener('mouseleave', hideTooltip);
        g.addEventListener('click', () => {
            const selectData = node.isProduction ? window.nodeMap.get(node.baseId) : node;
            selectNode(selectData);
        });
        nodesLayer.appendChild(g);
    });
}

function createNodeShape(node, isLocked) {
    let shape;
    const color = isLocked ? '#333' : getCategoryColor(node.type);
    if (node.type === 'Milestone') {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        shape.setAttribute('x', '-20'); shape.setAttribute('y', '-60');
        shape.setAttribute('width', '40'); shape.setAttribute('height', '120');
        shape.setAttribute('rx', '10');
        shape.setAttribute('fill', 'var(--accent-color)');
        shape.setAttribute('style', 'filter: drop-shadow(0 0 10px var(--accent-color));');
    } else if (node.type === 'Item') {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        shape.setAttribute('r', '14'); shape.setAttribute('fill', color);
    } else if (node.type === 'Cadence') {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        shape.setAttribute('x', '-14'); shape.setAttribute('y', '-14');
        shape.setAttribute('width', '28'); shape.setAttribute('height', '28');
        shape.setAttribute('rx', '4'); shape.setAttribute('fill', color);
    } else if (node.type === 'Ability') {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
        shape.setAttribute('points', '0,-14 14,0 0,14 -14,0'); shape.setAttribute('fill', color);
    } else if (node.type === 'Refinement') {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        shape.setAttribute('x', '-12'); shape.setAttribute('y', '-12');
        shape.setAttribute('width', '24'); shape.setAttribute('height', '24');
        shape.setAttribute('rx', '8'); shape.setAttribute('fill', color);
    } else {
        shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
        shape.setAttribute('points', '-14,-7 -14,7 0,14 14,7 14,-7 0,-14'); shape.setAttribute('fill', color);
    }
    if (isLocked) { shape.setAttribute('stroke', '#666'); shape.setAttribute('stroke-dasharray', '2,2'); }
    return shape;
}
