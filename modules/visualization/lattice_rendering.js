function renderQuestFlow() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';

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
