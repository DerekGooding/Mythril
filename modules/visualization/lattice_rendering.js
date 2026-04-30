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
        let cls = `edge ${edge.category}`;
        path.setAttribute('class', cls);
        path.setAttribute('id', edge.id);
        edgesLayer.appendChild(path);
        edge.el = path;
    });

    nodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        let cls = 'node';
        if (!node.visible) cls += ' dimmed';
        if (node.is_milestone) cls += ' milestone';
        if (showSimOverlay) {
            if (node.simulation.sustainable) cls += ' sustainable';
            if (node.simulation.unsustainable) cls += ' unsustainable';
        }
        
        g.setAttribute('class', cls);
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

        if (node.visible) {
            g.addEventListener('mouseenter', (e) => showTooltip(e, node));
            g.addEventListener('mouseleave', hideTooltip);
            g.addEventListener('click', () => selectNode(node));
        }

        nodesLayer.appendChild(g);
        node.el = g;
    });

    updateLayout();
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

function renderQuestFlow() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';
    document.getElementById('clusters-layer').innerHTML = '';

    // 1. Filter to Quests
    const quests = nodesData.filter(n => n.type === 'Quest');
    const questMap = new Map(quests.map(n => [n.id, n]));
    
    // 2. Build Quest-Only Dependency Graph
    const adj = new Map();
    const revAdj = new Map();
    quests.forEach(q => {
        adj.set(q.id, []);
        revAdj.set(q.id, []);
    });

    quests.forEach(q => {
        if (q.in_edges && q.in_edges.requires_quest) {
            q.in_edges.requires_quest.forEach(reqId => {
                if (questMap.has(reqId)) {
                    adj.get(reqId).push(q.id);
                    revAdj.get(q.id).push(reqId);
                }
            });
        }
    });

    // 3. Calculate Quest-Only Tiers (BFS/Topological)
    const questTiers = new Map();
    const roots = quests.filter(q => revAdj.get(q.id).length === 0);
    const queue = roots.map(r => ({ id: r.id, depth: 0 }));
    
    roots.forEach(r => questTiers.set(r.id, 0));

    while (queue.length > 0) {
        const { id, depth } = queue.shift();
        adj.get(id).forEach(neighborId => {
            const currentTier = questTiers.get(neighborId) || 0;
            if (depth + 1 > currentTier) {
                questTiers.set(neighborId, depth + 1);
                queue.push({ id: neighborId, depth: depth + 1 });
            }
        });
    }

    // 4. Group by Tiers and Order for Crossing Minimization
    const FLOW_TIER_WIDTH = 500;
    const FLOW_VERTICAL_SPACING = 140;
    const tierGroups = [];
    
    // Fill tier groups
    quests.forEach(q => {
        const t = questTiers.get(q.id) || 0;
        if (!tierGroups[t]) tierGroups[t] = [];
        tierGroups[t].push(q);
    });

    const flowNodes = [];
    const flowEdges = [];
    const nodeYPositions = new Map(); // Store assigned Y index for barycenter calculation

    // Process tiers sequentially
    tierGroups.forEach((tierQuests, t) => {
        if (t > 0) {
            // Sort by Barycenter (average Y position of parents)
            tierQuests.sort((a, b) => {
                const parentsA = revAdj.get(a.id);
                const parentsB = revAdj.get(b.id);
                
                const getAvgY = (parents) => {
                    if (parents.length === 0) return 0;
                    let sum = 0;
                    parents.forEach(pId => {
                        sum += nodeYPositions.get(pId) || 0;
                    });
                    return sum / parents.length;
                };

                return getAvgY(parentsA) - getAvgY(parentsB);
            });
        } else {
            // Initial sort for roots
            tierQuests.sort((a, b) => a.name.localeCompare(b.name));
        }

        // Assign positions
        tierQuests.forEach((q, idx) => {
            const fy = idx * FLOW_VERTICAL_SPACING;
            nodeYPositions.set(q.id, idx);
            
            const qNode = {
                ...q,
                fx: t * FLOW_TIER_WIDTH,
                fy: fy,
                isTerminal: false
            };
            flowNodes.push(qNode);
        });
    });

    // 5. Add Terminal Items & Edges
    const flowNodeMap = new Map(flowNodes.map(n => [n.id, n]));
    
    flowNodes.forEach(qNode => {
        if (qNode.isTerminal) return;

        // Quest -> Quest Edges
        adj.get(qNode.id).forEach(targetId => {
            flowEdges.push({
                id: `flow-${qNode.id}-${targetId}`,
                source: qNode.id,
                target: targetId,
                category: 'progression'
            });
        });

        // Terminal Resources
        if (qNode.data.quest_type === 'Repeatable' && qNode.out_edges && qNode.out_edges.rewards) {
            const rewardCount = qNode.out_edges.rewards.length;
            qNode.out_edges.rewards.forEach((rew, idx) => {
                const itemData = nodesData.find(n => n.id === rew.targetId);
                if (itemData) {
                    const rate = (rew.quantity * 60) / (qNode.data.duration || 10);
                    const terminalId = `terminal-${qNode.id}-${itemData.id}`;
                    
                    // Offset rewards vertically so they don't overlap, centering them on the quest
                    const verticalOffset = (idx - (rewardCount - 1) / 2) * 35;
                    
                    const tNode = {
                        ...itemData,
                        id: terminalId,
                        name: `${itemData.name} (${rate.toFixed(1)}/m)`,
                        fx: qNode.fx + 250,
                        fy: qNode.fy + verticalOffset,
                        isTerminal: true
                    };
                    flowNodes.push(tNode);
                    flowEdges.push({
                        id: `flow-reward-${qNode.id}-${terminalId}`,
                        source: qNode.id,
                        target: terminalId,
                        category: 'economy'
                    });
                }
            });
        }
    });

    // 6. Render
    flowEdges.forEach(edge => {
        const s = flowNodes.find(n => n.id === edge.source);
        const t = flowNodes.find(n => n.id === edge.target);
        if (s && t) {
            const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            path.setAttribute('class', `edge ${edge.category}`);
            const midX = (s.fx + t.fx) / 2;
            const d = `M ${s.fx} ${s.fy} Q ${midX} ${s.fy + (t.fy - s.fy) * 0.1} ${t.fx} ${t.fy}`;
            path.setAttribute('d', d);
            edgesLayer.appendChild(path);
        }
    });

    flowNodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        g.setAttribute('class', `node ${node.is_milestone ? 'milestone' : ''}`);
        g.setAttribute('transform', `translate(${node.fx}, ${node.fy})`);
        
        let shape;
        if (node.isTerminal) {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            shape.setAttribute('r', '8');
            shape.setAttribute('fill', 'var(--item-color)');
        } else {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
            shape.setAttribute('points', '-14,-7 -14,7 0,14 14,7 14,-7 0,-14');
            shape.setAttribute('fill', 'var(--quest-color)');
        }
        g.appendChild(shape);

        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute('class', 'label');
        text.setAttribute('y', node.isTerminal ? '5' : '28');
        text.setAttribute('x', node.isTerminal ? '15' : '0');
        text.setAttribute('text-anchor', node.isTerminal ? 'start' : 'middle');
        text.style.fontSize = node.isTerminal ? '10px' : '12px';
        text.textContent = node.name;
        g.appendChild(text);

        g.addEventListener('mouseenter', (e) => {
            const tempNode = {...node, x: node.fx, y: node.fy}; // Tooltip expects x/y
            showTooltip(e, tempNode);
        });
        g.addEventListener('mouseleave', hideTooltip);
        g.addEventListener('click', () => selectNode(node));
        nodesLayer.appendChild(g);
    });
}
