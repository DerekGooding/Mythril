function calculateEnhancedFlow(nodesData, currentView, baseEntities, entityMap, entityTiers, adj, revAdj) {
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
    const milestoneMap = new Map();
    
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

    // Pass 4: Global Sustainability
    const globalSustainablePool = new Set();
    const tierSustainableOutputs = new Map();
    const finalFlowEntities = [...baseEntities, ...milestoneBridges];
    const finalAdj = new Map();
    const finalRevAdj = new Map();
    finalFlowEntities.forEach(q => { finalAdj.set(q.id, []); finalRevAdj.set(q.id, []); });

    baseEntities.forEach(q => {
        adj.get(q.id).forEach(tId => {
            finalAdj.get(q.id).push(tId);
            finalRevAdj.get(tId).push(q.id);
        });
    });

    milestoneBridges.forEach(m => {
        const mTier = m.tier;
        baseEntities.forEach(q => {
            if (entityTiers.get(q.id) < mTier) {
                if (q.sustainableOutputs && q.sustainableOutputs.size > 0) {
                    finalAdj.get(q.id).push(m.id);
                    finalRevAdj.get(m.id).push(q.id);
                    m.upstream.add(q.id);
                }
            }
        });
        m.downstream.forEach(tId => {
            finalAdj.get(m.id).push(tId);
            finalRevAdj.get(tId).push(m.id);
        });
    });

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
    const FLOW_TIER_WIDTH = 450;
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
            nodeYPositions.set(q.id, fy);
            flowNodes.push({ ...q, fx: t * FLOW_TIER_WIDTH, fy: fy });
        });
    });

    flowNodes.forEach(qNode => {
        if (finalAdj.has(qNode.id)) {
            finalAdj.get(qNode.id).forEach(targetId => {
                flowEdges.push({ id: `flow-${qNode.id}-${targetId}`, source: qNode.id, target: targetId, category: 'progression' });
            });
        }
    });
    productionEdges.forEach(pe => flowEdges.push({ ...pe, category: 'economy' }));

    return { flowNodes, flowEdges };
}
