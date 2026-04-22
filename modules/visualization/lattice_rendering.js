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
