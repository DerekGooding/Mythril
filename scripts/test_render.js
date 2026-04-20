const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

(async () => {
    const browser = await chromium.launch();
    const context = await browser.newContext();
    const page = await context.newPage();
    
    page.on('console', msg => {
        console.log(`PAGE LOG [${msg.type()}]: ${msg.text()}`);
    });

    // We expect visual_dashboard.html to be generated in output/
    const filePath = path.resolve('output/visual_dashboard.html');
    if (!fs.existsSync(filePath)) {
        console.error(`[FAIL] File not found: ${filePath}`);
        process.exit(1);
    }

    const url = 'file://' + filePath;
    console.log('Loading ' + url);
    
    await page.goto(url);
    
    console.log('Waiting for graph to render...');
    // The new visualizer uses requestAnimationFrame for simulation
    // We'll wait for nodes to appear
    try {
        await page.waitForSelector('.node', { timeout: 10000 });
        console.log('[SUCCESS] Nodes detected in the lattice view.');
    } catch (e) {
        console.error('[FAIL] Timeout waiting for .node elements.');
        await browser.close();
        process.exit(1);
    }
    
    // Switch to Hierarchy View
    console.log('Testing Hierarchy View...');
    await page.click('#btn-hierarchy');
    try {
        await page.waitForSelector('.tier-column', { timeout: 5000 });
        console.log('[SUCCESS] Tier columns detected in hierarchy view.');
    } catch (e) {
        console.error('[FAIL] Timeout waiting for .tier-column elements.');
    }

    // Take a screenshot for manual verification if needed
    const screenshotPath = 'output/visual_debug.png';
    await page.screenshot({ path: screenshotPath, fullPage: true });
    console.log(`Screenshot saved to ${screenshotPath}`);
    
    await browser.close();
})();
