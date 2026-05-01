function renderQuestFlow() {
    const nodesLayer = document.getElementById('nodes-layer');
    const edgesLayer = document.getElementById('edges-layer');
    nodesLayer.innerHTML = ''; edgesLayer.innerHTML = '';

    // 1. Prepare Global Data for Sidebar
    window.nodeMap = new Map(nodesData.map(n => [n.id, n]));
    window.allEdges = [];
    
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

    // 2. Filter Base Entities
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

    let refinements = [];
    if (currentView === 'progressive') {
        const abilityIds = new Set(abilities.map(a => a.id));
        refinements = nodesData.filter(n => 
            n.type === 'Refinement' && 
            n.in_edges && 
            n.in_edges.requires_ability && 
            n.in_edges.requires_ability.some(aId => abilityIds.has(aId))
        );
    }

    const baseEntities = [...quests, ...cadences, ...abilities, ...refinements];
    const entityMap = new Map(baseEntities.map(n => [n.id, n]));

    // 3. Dependency Graph
    const adj = new Map();
    const revAdj = new Map();
    baseEntities.forEach(q => { adj.set(q.id, []); revAdj.set(q.id, []); });

    baseEntities.forEach(q => {
        if (q.in_edges && q.in_edges.requires_quest) {
            q.in_edges.requires_quest.forEach(reqId => {
                if (entityMap.has(reqId)) { adj.get(reqId).push(q.id); revAdj.get(q.id).push(reqId); }
            });
        }
        if (q.type === 'Cadence') {
            quests.forEach(otherQ => {
                if (otherQ.out_edges && otherQ.out_edges.unlocks_cadence) {
                    if (otherQ.out_edges.unlocks_cadence.some(t => t.targetId === q.id)) {
                        adj.get(otherQ.id).push(q.id); revAdj.get(q.id).push(otherQ.id);
                    }
                }
            });
        }
        if (q.type === 'Ability') {
            cadences.forEach(otherC => {
                if (otherC.out_edges && otherC.out_edges.provides_ability) {
                    if (otherC.out_edges.provides_ability.some(t => t.targetId === q.id)) {
                        adj.get(otherC.id).push(q.id); revAdj.get(q.id).push(otherC.id);
                    }
                }
            });
        }
        if (q.type === 'Refinement') {
            if (q.in_edges && q.in_edges.requires_ability) {
                q.in_edges.requires_ability.forEach(reqId => {
                    if (entityMap.has(reqId)) { adj.get(reqId).push(q.id); revAdj.get(q.id).push(reqId); }
                });
            }
        }
    });

    // 4. Initial Tiers
    const entityTiers = new Map();
    const roots = baseEntities.filter(q => revAdj.get(q.id).length === 0);
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

    // 5. Intelligence & Milestone Bridge Nodes
    const nodeAncestry = new Map();
    const sortedBase = [...baseEntities].sort((a, b) => (entityTiers.get(a.id) || 0) - (entityTiers.get(b.id) || 0));

    sortedBase.forEach(node => {
        const ancestors = new Set();
        const parents = revAdj.get(node.id) || [];
        parents.forEach(pId => {
            ancestors.add(pId);
            const pAncestors = nodeAncestry.get(pId);
            if (pAncestors) pAncestors.forEach(aId => ancestors.add(aId));
        });
        nodeAncestry.set(node.id, ancestors);
    });

    const sustainableProducers = new Map();
    nodesData.forEach(n => {
        if (n.type === 'Quest' && n.data.quest_type === 'Recurring' && n.out_edges && n.out_edges.rewards) {
            n.out_edges.rewards.forEach(rew => {
                if (!sustainableProducers.has(rew.targetId)) sustainableProducers.set(rew.targetId, new Set());
                sustainableProducers.get(rew.targetId).add(n.id);
            });
        }
        if (n.type === 'Refinement' && n.out_edges && n.out_edges.produces) {
            n.out_edges.produces.forEach(prod => {
                if (!sustainableProducers.has(prod.targetId)) sustainableProducers.set(prod.targetId, new Set());
                sustainableProducers.get(prod.targetId).add(n.id);
            });
        }
    });

    const milestoneBridges = [];
    const milestoneMap = new Map(); // tier -> milestoneNode
    
    if (currentView === 'progressive') {
        sortedBase.forEach(node => {
            if (node.out_edges && node.out_edges.consumes) {
                const ancestors = nodeAncestry.get(node.id);
                const tier = entityTiers.get(node.id);
                node.out_edges.consumes.forEach(cons => {
                    const producers = sustainableProducers.get(cons.targetId);
                    if (producers) {
                        let isCrossBranch = true;
                        producers.forEach(pId => { if (pId === node.id || ancestors.has(pId)) isCrossBranch = false; });
                        if (isCrossBranch) {
                            if (!milestoneMap.has(tier)) {
                                const mId = `milestone-tier-${tier}`;
                                const mNode = {
                                    id: mId, type: 'Milestone', tier: tier, x: 0, y: 0,
                                    data: { description: 'Global resource synchronization point.', gateNodes: [], resources: new Set() },
                                    name: `Economic Integration (Tier ${tier})`,
                                    upstream: new Set(), downstream: new Set()
                                };
                                milestoneBridges.push(mNode);
                                milestoneMap.set(tier, mNode);
                            }
                            const m = milestoneMap.get(tier);
                            m.data.gateNodes.push(node.id);
                            m.data.resources.add(cons.targetId);
                            m.downstream.add(node.id);
                        }
                    }
                });
            }
        });
    }

    // Pass 4: Global Sustainability & Refined Layout
    const globalSustainablePool = new Set();
    const tierSustainableOutputs = new Map();
    const finalFlowEntities = [...baseEntities, ...milestoneBridges];
    const finalAdj = new Map();
    const finalRevAdj = new Map();
    finalFlowEntities.forEach(q => { finalAdj.set(q.id, []); finalRevAdj.set(q.id, []); });

    // Transfer base edges
    baseEntities.forEach(q => {
        adj.get(q.id).forEach(tId => {
            finalAdj.get(q.id).push(tId);
            finalRevAdj.get(tId).push(q.id);
        });
    });

    // Re-route Milestone edges
    milestoneBridges.forEach(m => {
        // Collect everything before this milestone as potential upstream
        const mTier = m.tier;
        baseEntities.forEach(q => {
            if (entityTiers.get(q.id) < mTier) {
                // If it produces something used at or after this milestone
                if (q.sustainableOutputs && q.sustainableOutputs.size > 0) {
                    finalAdj.get(q.id).push(m.id);
                    finalRevAdj.get(m.id).push(q.id);
                    m.upstream.add(q.id);
                }
            }
        });
        // Connect milestone to its gated nodes
        m.downstream.forEach(tId => {
            // Remove direct progression links if they exist to force bottleneck? 
            // No, keep them but add the milestone relationship.
            finalAdj.get(m.id).push(tId);
            finalRevAdj.get(tId).push(m.id);
        });
    });

    // Final Sustainability Pass
    const finalSorted = [...finalFlowEntities].sort((a, b) => {
        const ta = a.type === 'Milestone' ? a.tier - 0.5 : (entityTiers.get(a.id) || 0);
        const tb = b.type === 'Milestone' ? b.tier - 0.5 : (entityTiers.get(b.id) || 0);
        return ta - tb;
    });

    finalSorted.forEach(node => {
        const tier = node.type === 'Milestone' ? node.tier : (entityTiers.get(node.id) || 0);
        if (!tierSustainableOutputs.has(tier)) tierSustainableOutputs.set(tier, new Set());
        
        node.sustainableOutputs = new Set();
        node.availableSustainable = new Set(globalSustainablePool);
        
        const ancestors = nodeAncestry.get(node.id) || new Set();
        ancestors.forEach(aId => {
            const aNode = entityMap.get(aId);
            if (aNode && aNode.sustainableOutputs) aNode.sustainableOutputs.forEach(itemId => node.availableSustainable.add(itemId));
        });

        node.missingSustainable = [];
        let isReachable = true;
        if (node.out_edges && node.out_edges.consumes) {
            node.out_edges.consumes.forEach(cons => {
                if (!node.availableSustainable.has(cons.targetId)) { isReachable = false; node.missingSustainable.push(cons.targetId); }
            });
        }
        node.isSustainablyActive = isReachable;

        if (node.isSustainablyActive) {
            if (node.type === 'Quest' && node.data.quest_type === 'Recurring') {
                node.out_edges.rewards.forEach(rew => { node.sustainableOutputs.add(rew.targetId); tierSustainableOutputs.get(tier).add(rew.targetId); });
            } else if (node.type === 'Refinement') {
                node.out_edges.produces.forEach(prod => { node.sustainableOutputs.add(prod.targetId); tierSustainableOutputs.get(tier).add(prod.targetId); });
            }
        }

        if (node.type === 'Milestone') {
            for (let t = 0; t <= tier; t++) {
                if (tierSustainableOutputs.has(t)) tierSustainableOutputs.get(t).forEach(itemId => globalSustainablePool.add(itemId));
            }
            node.data.globalCoverage = (globalSustainablePool.size / Array.from(sustainableProducers.keys()).length * 100).toFixed(1);
        }
    });

    // 6. Production Nodes
    const productionNodes = [];
    const productionEdges = [];
    if (currentView === 'advanced' || currentView === 'progressive') {
        baseEntities.forEach(node => {
            if (node.sustainableOutputs.size > 0) {
                const nodeTier = entityTiers.get(node.id) || 0;
                node.sustainableOutputs.forEach(itemId => {
                    const itemTemplate = nodesData.find(n => n.id === itemId);
                    if (itemTemplate) {
                        const uniqueId = `prod-${node.id}-${itemTemplate.id}`;
                        let rateInfo = "";
                        if (node.type === 'Quest') {
                            const rew = node.out_edges.rewards.find(r => r.targetId === itemId);
                            const rate = (rew.quantity * 60) / (node.data.duration || 10);
                            rateInfo = ` (${rate.toFixed(1)}/m)`;
                        }
                        productionNodes.push({
                            ...itemTemplate, id: uniqueId, baseId: itemTemplate.id,
                            name: `${itemTemplate.name}${rateInfo}`,
                            tier: nodeTier + 1, isProduction: true, isSustainablyActive: node.isSustainablyActive
                        });
                        productionEdges.push({ id: `edge-${node.id}-${uniqueId}`, source: node.id, target: uniqueId, category: 'economy' });
                    }
                });
            }
        });
    }

    // 7. Physical Layout
    const layoutNodes = [...finalFlowEntities, ...productionNodes];
    const tierGroups = [];
    layoutNodes.forEach(q => {
        let t = 0;
        if (q.type === 'Milestone') t = q.tier * 2 - 1;
        else if (q.isProduction) t = q.tier * 2;
        else t = (entityTiers.get(q.id) || 0) * 2;
        if (!tierGroups[t]) tierGroups[t] = [];
        tierGroups[t].push(q);
    });

    const flowNodes = [];
    const flowEdges = [];
    const nodeYPositions = new Map();
    const FLOW_TIER_WIDTH = 450; // Narrower tiers since we have more of them
    const FLOW_VERTICAL_SPACING = 160;

    tierGroups.forEach((tierEntities, t) => {
        if (t > 0) {
            tierEntities.sort((a, b) => {
                const getAvgY = (id) => {
                    const parents = finalRevAdj.get(id) || [];
                    productionEdges.forEach(pe => { if (pe.target === id) parents.push(pe.source); });
                    if (parents.length === 0) return 0;
                    let sum = 0; parents.forEach(pId => sum += nodeYPositions.get(pId) || 0);
                    return sum / parents.length;
                };
                return getAvgY(a.id) - getAvgY(b.id);
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

    // 8. Edges
    flowNodes.forEach(qNode => {
        if (finalAdj.has(qNode.id)) {
            finalAdj.get(qNode.id).forEach(targetId => {
                flowEdges.push({ id: `flow-${qNode.id}-${targetId}`, source: qNode.id, target: targetId, category: 'progression' });
            });
        }
    });
    productionEdges.forEach(pe => flowEdges.push({ ...pe, category: 'economy' }));

    // 9. Final Render
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

    flowNodes.forEach(node => {
        const g = document.createElementNS("http://www.w3.org/2000/svg", "g");
        const isLocked = currentView === 'progressive' && node.isSustainablyActive === false;
        g.setAttribute('class', `node ${node.type === 'Milestone' ? 'milestone-node' : ''} ${isLocked ? 'locked' : ''}`);
        g.setAttribute('transform', `translate(${node.fx}, ${node.fy})`);
        g.setAttribute('id', `node-${node.id}`);
        
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
