function simulationStep() {
    if (currentView !== 'lattice') return;
    if (!isSimulating && !draggedNode) return;

    if (isSimulating && !draggedNode) {
        simulationFrame++;
        if (simulationFrame > MAX_SIM_FRAMES) {
            isSimulating = false;
            console.log("Simulation settled.");
        }
    }

    const clusters = new Map();

    nodes.forEach(n => {
        if (n === draggedNode) return;
        n.vx += (n.tier * TIER_WIDTH - n.x) * 0.05;
        n.vy += (window.innerHeight / 2 - n.y) * 0.01;

        nodes.forEach(m => {
            if (n === m) return;
            const dx = n.x - m.x, dy = n.y - m.y;
            const distSq = dx * dx + dy * dy || 1;
            if (distSq < 3600) { 
                const force = 100 / Math.sqrt(distSq);
                n.vx += dx * force * 0.5; n.vy += dy * force * 0.5;
            } else if (distSq < 100000) {
                const force = 20 / distSq;
                n.vx += dx * force; n.vy += dy * force;
            }
        });

        if (n.cluster_id !== 'cluster_none') {
            if (!clusters.has(n.cluster_id)) clusters.set(n.cluster_id, { x: 0, y: 0, count: 0 });
            const c = clusters.get(n.cluster_id);
            c.x += n.x; c.y += n.y; c.count++;
        }
    });

    for (const [id, c] of clusters.entries()) {
        const avgX = c.x / c.count, avgY = c.y / c.count;
        nodes.filter(n => n.cluster_id === id).forEach(n => {
            n.vx += (avgX - n.x) * 0.02; n.vy += (avgY - n.y) * 0.02;
        });
    }

    edges.forEach(e => {
        const s = nodeMap.get(e.source), t = nodeMap.get(e.target);
        const dx = t.x - s.x, dy = t.y - s.y;
        const dist = Math.sqrt(dx * dx + dy * dy) || 1;
        const strength = Math.abs(s.tier - t.tier) > 0 ? 0.01 : 0.05;
        const force = (dist - 150) * strength;
        const fx = (dx / dist) * force, fy = (dy / dist) * force;
        s.vx += fx; s.vy += fy; t.vx -= fx; t.vy -= fy;
    });

    nodes.forEach(n => {
        n.x += n.vx; n.y += n.vy; n.vx *= 0.7; n.vy *= 0.7;
        n.el.setAttribute('transform', `translate(${n.x}, ${n.y})`);
    });

    edges.forEach(e => {
        const s = nodeMap.get(e.source), t = nodeMap.get(e.target);
        const midX = (s.x + t.x) / 2;
        const path = `M ${s.x} ${s.y} Q ${midX} ${s.y + (t.y - s.y) * 0.1} ${t.x} ${t.y}`;
        e.el.setAttribute('d', path);
    });

    renderClusterBoxes(clusters);
    requestAnimationFrame(simulationStep);
}
