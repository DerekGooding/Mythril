function processData() {
    nodes = nodesData.map(d => ({
        ...d,
        x: d.tier * TIER_WIDTH + (Math.random() - 0.5) * 400,
        y: (window.innerHeight / 2) + (Math.random() - 0.5) * 2000,
        vx: 0, vy: 0
    }));

    nodeMap = new Map(nodes.map(n => [n.id, n]));

    nodes.forEach(node => {
        if (node.out_edges) {
            Object.entries(node.out_edges).forEach(([type, targetList]) => {
                targetList.forEach(target => {
                    const targetId = typeof target === 'string' ? target : target.targetId;
                    if (nodeMap.has(targetId)) {
                        edges.push({
                            id: `edge-${node.id}-${targetId}`,
                            source: node.id, target: targetId, type: type
                        });
                    }
                });
            });
        }
        if (node.in_edges) {
            Object.entries(node.in_edges).forEach(([type, sourceList]) => {
                sourceList.forEach(sourceId => {
                    if (nodeMap.has(sourceId)) {
                        edges.push({
                            id: `edge-${sourceId}-${node.id}`,
                            source: sourceId, target: node.id, type: type
                        });
                    }
                });
            });
        }
    });
    
    const seenEdges = new Set();
    edges = edges.filter(e => {
        const key = `${e.source}-${e.target}`;
        if (seenEdges.has(key)) return false;
        seenEdges.add(key);
        return true;
    });
}

function updateStats() {
    document.getElementById('stats').innerText = `NODES: ${nodes.length} | EDGES: ${edges.length} | TIERS: ${Math.max(...nodes.map(n=>n.tier))+1}`;
}
