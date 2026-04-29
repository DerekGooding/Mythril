function processData() {
    nodes = nodesData.map(d => ({
        ...d,
        x: d.tier * TIER_WIDTH + (Math.random() - 0.5) * 50,
        y: (window.innerHeight / 2) + (Math.random() - 0.5) * 100,
        vx: 0, vy: 0
    }));

    nodeMap = new Map(nodes.map(n => [n.id, n]));
    allEdges = [];

    const progressionTypes = ['unlocks_cadence', 'unlocks_location', 'requires_quest', 'contains', 'provides_ability', 'requires_ability'];

    nodes.forEach(node => {
        if (node.out_edges) {
            Object.entries(node.out_edges).forEach(([type, targetList]) => {
                targetList.forEach(target => {
                    const targetId = typeof target === 'string' ? target : target.targetId;
                    if (nodeMap.has(targetId)) {
                        allEdges.push({
                            id: `edge-${node.id}-${targetId}`,
                            source: node.id, target: targetId, 
                            type: type,
                            category: progressionTypes.includes(type) ? 'progression' : 'economy'
                        });
                    }
                });
            });
        }
        if (node.in_edges) {
            Object.entries(node.in_edges).forEach(([type, sourceList]) => {
                sourceList.forEach(sourceId => {
                    if (nodeMap.has(sourceId)) {
                        allEdges.push({
                            id: `edge-${sourceId}-${node.id}`,
                            source: sourceId, target: node.id, 
                            type: type,
                            category: progressionTypes.includes(type) ? 'progression' : 'economy'
                        });
                    }
                });
            });
        }
    });
    
    const seenEdges = new Set();
    allEdges = allEdges.filter(e => {
        const key = `${e.source}-${e.target}-${e.type}`;
        if (seenEdges.has(key)) return false;
        seenEdges.add(key);
        return true;
    });

    filterEdges();
}

function filterEdges() {
    edges = allEdges.filter(e => {
        const sourceNode = nodeMap.get(e.source);
        const targetNode = nodeMap.get(e.target);

        // Hub filtering
        if (!showHubs) {
            if (sourceNode.is_hub || targetNode.is_hub) return false;
        }

        // Progression Only filtering
        if (showProgressionOnly) {
            if (e.category !== 'progression') return false;
        }

        return true;
    });

    // Handle node visibility (dimming)
    nodes.forEach(n => {
        n.visible = true;
        if (!showHubs && n.is_hub) n.visible = false;
    });
}

function updateStats() {
    document.getElementById('stats').innerText = `NODES: ${nodes.filter(n=>n.visible).length}/${nodes.length} | EDGES: ${edges.length} | TIERS: ${Math.max(...nodes.map(n=>n.tier))+1}`;
}
