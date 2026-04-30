function renderQuestFlow() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';

    // 1. Prepare Global Data for Sidebar
    window.nodeMap = new Map(nodesData.map(n => [n.id, n]));
    window.allEdges = [];
    
    // Populate allEdges from the entire graph for the sidebar to function correctly
    nodesData.forEach(node => {
        if (node.out_edges) {
            Object.entries(node.out_edges).forEach(([type, targets]) => {
                targets.forEach(t => {
                    const targetId = typeof t === 'string' ? t : t.targetId;
                    window.allEdges.push({
                        id: `edge-${node.id}-${targetId}-${type}`,
                        source: node.id,
                        target: targetId,
                        type: type,
                        quantity: t.quantity || 1
                    });
                });
            });
        }
    });

    // 2. Filter to Quests and (in Advanced) quest-unlocked Cadences + their Abilities
    const quests = nodesData.filter(n => n.type === 'Quest');
    
    // Find cadences that are unlocked by quests
    const questUnlockedCadenceIds = new Set();
    quests.forEach(q => {
        if (q.out_edges && q.out_edges.unlocks_cadence) {
            q.out_edges.unlocks_cadence.forEach(t => questUnlockedCadenceIds.add(t.targetId));
        }
    });

    const cadences = currentView === 'advanced' 
        ? nodesData.filter(n => n.type === 'Cadence' && questUnlockedCadenceIds.has(n.id))
        : [];

    // Find abilities provided by those cadences
    const cadenceProvidedAbilityIds = new Set();
    cadences.forEach(c => {
        if (c.out_edges && c.out_edges.provides_ability) {
            c.out_edges.provides_ability.forEach(t => cadenceProvidedAbilityIds.add(t.targetId));
        }
    });

    const abilities = currentView === 'advanced'
        ? nodesData.filter(n => n.type === 'Ability' && cadenceProvidedAbilityIds.has(n.id))
        : [];

    const flowEntities = [...quests, ...cadences, ...abilities];
    const entityMap = new Map(flowEntities.map(n => [n.id, n]));

    // 3. Build Dependency Graph for Layout
    const adj = new Map();
    const revAdj = new Map();
    flowEntities.forEach(q => {
        adj.set(q.id, []);
        revAdj.set(q.id, []);
    });

    flowEntities.forEach(q => {
        // Quest Dependencies
        if (q.in_edges && q.in_edges.requires_quest) {
            q.in_edges.requires_quest.forEach(reqId => {
                if (entityMap.has(reqId)) {
                    adj.get(reqId).push(q.id);
                    revAdj.get(q.id).push(reqId);
                }
            });
        }
        // Cadence Unlocks (Quest -> Cadence)
        if (q.type === 'Cadence') {
            // Find which quests unlock this cadence
            quests.forEach(otherQ => {
                if (otherQ.out_edges && otherQ.out_edges.unlocks_cadence) {
                    if (otherQ.out_edges.unlocks_cadence.some(t => t.targetId === q.id)) {
                        adj.get(otherQ.id).push(q.id);
                        revAdj.get(q.id).push(otherQ.id);
                    }
                }
            });
        }
        // Ability Unlocks (Cadence -> Ability)
        if (q.type === 'Ability') {
            // Find which cadences provide this ability
            cadences.forEach(otherC => {
                if (otherC.out_edges && otherC.out_edges.provides_ability) {
                    if (otherC.out_edges.provides_ability.some(t => t.targetId === q.id)) {
                        adj.get(otherC.id).push(q.id);
                        revAdj.get(q.id).push(otherC.id);
                    }
                }
            });
        }
    });

    // 3. Calculate Tiers
    const entityTiers = new Map();
    const roots = flowEntities.filter(q => revAdj.get(q.id).length === 0);
    const queue = roots.map(r => ({ id: r.id, depth: 0 }));
    
    roots.forEach(r => entityTiers.set(r.id, 0));

    while (queue.length > 0) {
        const { id, depth } = queue.shift();
        adj.get(id).forEach(neighborId => {
            const currentTier = entityTiers.get(neighborId) || 0;
            if (depth + 1 > currentTier) {
                entityTiers.set(neighborId, depth + 1);
                queue.push({ id: neighborId, depth: depth + 1 });
            }
        });
    }

    // 4. Group by Tiers and Order
    const FLOW_TIER_WIDTH = 550;
    const FLOW_VERTICAL_SPACING = 150;
    const tierGroups = [];
    
    flowEntities.forEach(q => {
        const t = entityTiers.get(q.id) || 0;
        if (!tierGroups[t]) tierGroups[t] = [];
        tierGroups[t].push(q);
    });

    const flowNodes = [];
    const flowEdges = [];
    const nodeYPositions = new Map();

    tierGroups.forEach((tierEntities, t) => {
        if (t > 0) {
            tierEntities.sort((a, b) => {
                const parentsA = revAdj.get(a.id);
                const parentsB = revAdj.get(b.id);
                const getAvgY = (parents) => {
                    if (parents.length === 0) return 0;
                    let sum = 0;
                    parents.forEach(pId => sum += nodeYPositions.get(pId) || 0);
                    return sum / parents.length;
                };
                return getAvgY(parentsA) - getAvgY(parentsB);
            });
        } else {
            tierEntities.sort((a, b) => a.name.localeCompare(b.name));
        }

        tierEntities.forEach((q, idx) => {
            const fy = idx * FLOW_VERTICAL_SPACING;
            nodeYPositions.set(q.id, idx);
            flowNodes.push({ ...q, fx: t * FLOW_TIER_WIDTH, fy: fy, isTerminal: false });
        });
    });

    // 5. Edges and Terminals
    flowNodes.forEach(qNode => {
        adj.get(qNode.id).forEach(targetId => {
            flowEdges.push({ id: `flow-${qNode.id}-${targetId}`, source: qNode.id, target: targetId, category: 'progression' });
        });

        if (qNode.type === 'Quest' && qNode.data.quest_type === 'Repeatable' && qNode.out_edges && qNode.out_edges.rewards) {
            const rewardCount = qNode.out_edges.rewards.length;
            qNode.out_edges.rewards.forEach((rew, idx) => {
                const itemData = nodesData.find(n => n.id === rew.targetId);
                if (itemData) {
                    const rate = (rew.quantity * 60) / (qNode.data.duration || 10);
                    const terminalId = `terminal-${qNode.id}-${itemData.id}`;
                    const verticalOffset = (idx - (rewardCount - 1) / 2) * 35;
                    flowNodes.push({ ...itemData, id: terminalId, name: `${itemData.name} (${rate.toFixed(1)}/m)`, fx: qNode.fx + 250, fy: qNode.fy + verticalOffset, isTerminal: true });
                    flowEdges.push({ id: `flow-reward-${qNode.id}-${terminalId}`, source: qNode.id, target: terminalId, category: 'economy' });
                }
            });
        }
    });

    // 6. Final Render
    const flowNodeIdMap = new Map(flowNodes.map(n => [n.id, n]));
    flowEdges.forEach(edge => {
        const s = flowNodeIdMap.get(edge.source);
        const t = flowNodeIdMap.get(edge.target);
        if (s && t) {
            const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            path.setAttribute('class', `edge ${edge.category}`);
            path.setAttribute('id', edge.id);
            const midX = (s.fx + t.fx) / 2;
            path.setAttribute('d', `M ${s.fx} ${s.fy} Q ${midX} ${s.fy + (t.fy - s.fy) * 0.1} ${t.fx} ${t.fy}`);
            edgesLayer.appendChild(path);
        }
    });

    flowNodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        g.setAttribute('class', `node ${node.is_milestone ? 'milestone' : ''}`);
        g.setAttribute('transform', `translate(${node.fx}, ${node.fy})`);
        g.setAttribute('id', `node-${node.id}`);
        
        let shape;
        if (node.isTerminal) {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            shape.setAttribute('r', '8');
            shape.setAttribute('fill', 'var(--item-color)');
        } else {
            if (node.type === 'Cadence') {
                shape = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                shape.setAttribute('x', '-14'); shape.setAttribute('y', '-14');
                shape.setAttribute('width', '28'); shape.setAttribute('height', '28');
                shape.setAttribute('rx', '4');
                shape.setAttribute('fill', 'var(--cadence-color)');
            } else if (node.type === 'Ability') {
                shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                shape.setAttribute('points', '0,-14 14,0 0,14 -14,0');
                shape.setAttribute('fill', 'var(--ability-color)');
            } else {
                shape = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                shape.setAttribute('points', '-14,-7 -14,7 0,14 14,7 14,-7 0,-14');
                shape.setAttribute('fill', 'var(--quest-color)');
            }
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

        g.addEventListener('mouseenter', (e) => showTooltip(e, node));
        g.addEventListener('mouseleave', hideTooltip);
        g.addEventListener('click', () => selectNode(node));
        nodesLayer.appendChild(g);
    });
}
