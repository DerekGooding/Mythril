function calculateLatticeFlow(nodesData, currentView) {
    // 1. Prepare Global Data
    const nodeMap = new Map(nodesData.map(n => [n.id, n]));
    
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

    return calculateEnhancedFlow(nodesData, currentView, baseEntities, entityMap, entityTiers, adj, revAdj);
}
