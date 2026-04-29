function updateLayout() {
    if (currentView !== 'lattice') return;

    const clusters = new Map();

    nodes.forEach(n => {
        if (n.cluster_id !== 'cluster_none') {
            if (!clusters.has(n.cluster_id)) clusters.set(n.cluster_id, { x: 0, y: 0, count: 0 });
            const c = clusters.get(n.cluster_id);
            c.x += n.x; c.y += n.y; c.count++;
        }
        
        n.el.setAttribute('transform', `translate(${n.x}, ${n.y})`);
    });

    edges.forEach(e => {
        const s = nodeMap.get(e.source), t = nodeMap.get(e.target);
        const midX = (s.x + t.x) / 2;
        const path = `M ${s.x} ${s.y} Q ${midX} ${s.y + (t.y - s.y) * 0.1} ${t.x} ${t.y}`;
        e.el.setAttribute('d', path);
    });

    renderClusterBoxes(clusters);
}
