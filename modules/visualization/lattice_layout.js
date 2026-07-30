function calculatePhysicalLayout(currentView, layoutNodes, productionEdges, finalRevAdj, finalAdj, entityTiers) {
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
    const FLOW_TIER_WIDTH = currentView === VIEW_CHRONO ? 120 : 450;
    const FLOW_VERTICAL_SPACING = 160;

    // Calculate Max Time for Chrono Scaling
    let maxTime = 1;
    if (currentView === VIEW_CHRONO) {
        layoutNodes.forEach(n => {
            const t = n.simulation?.unlock_time;
            if (t !== undefined && t !== null && t < 1000000) maxTime = Math.max(maxTime, t);
        });
    }

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
            
            let fx = t * FLOW_TIER_WIDTH;
            if (currentView === VIEW_CHRONO) {
                const simTime = q.simulation?.unlock_time;
                if (simTime !== undefined && simTime !== null && simTime < 1000000) {
                    fx = (simTime / maxTime) * 10000;
                }
            }
            
            flowNodes.push({ ...q, fx: fx, fy: fy });
        });
    });

    flowNodes.forEach(qNode => {
        if (finalAdj.has(qNode.id)) {
            finalAdj.get(qNode.id).forEach(targetId => {
                let edgeData = { id: `flow-${qNode.id}-${targetId}`, source: qNode.id, target: targetId, category: 'progression' };
                
                if (currentView === VIEW_QUANTITATIVE) {
                    const rate = qNode.simulation?.net_rate || 0;
                    edgeData.magnitude = Math.min(10, Math.log1p(Math.abs(rate)));
                }
                
                flowEdges.push(edgeData);
            });
        }
    });
    productionEdges.forEach(pe => flowEdges.push({ ...pe, category: 'economy' }));

    return { flowNodes, flowEdges };
}
