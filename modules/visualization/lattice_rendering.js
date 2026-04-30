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

    // 2. Filter to Quests and (in Advanced/Progressive) quest-unlocked Cadences + their Abilities
    const quests = nodesData.filter(n => n.type === 'Quest');
    
    let cadences = [];
    let abilities = [];
    if (currentView === 'advanced' || currentView === 'progressive') {
        const questUnlockedCadenceIds = new Set();
        quests.forEach(q => {
            if (q.out_edges && q.out_edges.unlocks_cadence) {
                q.out_edges.unlocks_cadence.forEach(t => questUnlockedCadenceIds.add(t.targetId));
            }
        });
        cadences = nodesData.filter(n => n.type === 'Cadence' && questUnlockedCadenceIds.has(n.id));
        
        const cadenceProvidedAbilityIds = new Set();
        cadences.forEach(c => {
            if (c.out_edges && c.out_edges.provides_ability) {
                c.out_edges.provides_ability.forEach(t => cadenceProvidedAbilityIds.add(t.targetId));
            }
        });
        abilities = nodesData.filter(n => n.type === 'Ability' && cadenceProvidedAbilityIds.has(n.id));
    }

    const flowEntities = [...quests, ...cadences, ...abilities];
    const entityMap = new Map(flowEntities.map(n => [n.id, n]));

    // 3. Build Dependency Graph for Layout (Base Progression)
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

    // 4. Calculate Tiers for Core Entities
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

    // 5. Add Non-Shared Item Nodes (Local to producing quest)
    const productionNodes = [];
    const productionEdges = [];
    
    if (currentView === 'advanced' || currentView === 'progressive') {
        quests.forEach(q => {
            if (q.data.quest_type === 'Recurring' && q.out_edges && q.out_edges.rewards) {
                const questTier = entityTiers.get(q.id) || 0;
                q.out_edges.rewards.forEach(rew => {
                    const itemTemplate = nodesData.find(n => n.id === rew.targetId);
                    if (itemTemplate) {
                        const uniqueId = `prod-${q.id}-${itemTemplate.id}`;
                        const rate = (rew.quantity * 60) / (q.data.duration || 10);
                        
                        const itemNode = {
                            ...itemTemplate,
                            id: uniqueId,
                            baseId: itemTemplate.id, // For selection/sidebar
                            name: `${itemTemplate.name} (${rate.toFixed(1)}/m)`,
                            tier: questTier + 1,
                            isProduction: true
                        };
                        productionNodes.push(itemNode);
                        productionEdges.push({
                            id: `edge-${q.id}-${uniqueId}`,
                            source: q.id,
                            target: uniqueId,
                            category: 'economy'
                        });
                    }
                });
            }
        });
    }

    const finalFlowEntities = [...flowEntities, ...productionNodes];
    const tierGroups = [];
    
    finalFlowEntities.forEach(q => {
        const t = q.isProduction ? q.tier : (entityTiers.get(q.id) || 0);
        if (!tierGroups[t]) tierGroups[t] = [];
        tierGroups[t].push(q);
    });

    const flowNodes = [];
    const flowEdges = [];
    const nodeYPositions = new Map();

    const FLOW_TIER_WIDTH = 700;
    const FLOW_VERTICAL_SPACING = 160;

    tierGroups.forEach((tierEntities, t) => {
        if (t > 0) {
            tierEntities.sort((a, b) => {
                const getParents = (id) => {
                    const p = revAdj.get(id) || [];
                    // Check production edges too
                    productionEdges.forEach(pe => { if (pe.target === id) p.push(pe.source); });
                    return p;
                };
                const parentsA = getParents(a.id);
                const parentsB = getParents(b.id);
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
            flowNodes.push({ ...q, fx: t * FLOW_TIER_WIDTH, fy: fy });
        });
    });

    // 6. Build Final Edges for Rendering
    flowNodes.forEach(qNode => {
        if (adj.has(qNode.id)) {
            adj.get(qNode.id).forEach(targetId => {
                flowEdges.push({ id: `flow-${qNode.id}-${targetId}`, source: qNode.id, target: targetId, category: 'progression' });
            });
        }
    });
    productionEdges.forEach(pe => {
        flowEdges.push({ ...pe, category: 'economy' });
    });

    // 7. Final Render
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
        if (node.type === 'Item') {
            shape = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            shape.setAttribute('r', '14');
            shape.setAttribute('fill', 'var(--item-color)');
        } else if (node.type === 'Cadence') {
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
        g.appendChild(shape);

        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute('class', 'label');
        text.setAttribute('y', '32');
        text.setAttribute('text-anchor', 'middle');
        text.style.fontSize = '12px';
        text.textContent = node.name;
        g.appendChild(text);

        g.addEventListener('mouseenter', (e) => {
            const tempNode = {...node, x: node.fx, y: node.fy};
            showTooltip(e, tempNode);
        });
        g.addEventListener('mouseleave', hideTooltip);
        g.addEventListener('click', () => {
            // Use baseId for sidebar consistency if it's a production node
            const selectData = node.isProduction ? window.nodeMap.get(node.baseId) : node;
            selectNode(selectData);
        });
        nodesLayer.appendChild(g);
    });
}
