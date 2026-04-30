// No longer used, replaced by inline processing in renderQuestFlow to maintain distillation
function updateStats() {
    const totalNodes = nodesData.length;
    document.getElementById('stats').innerText = `TOTAL NODES: ${totalNodes}`;
}
