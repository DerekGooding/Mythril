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

    const filePath = path.resolve('output/visual_dashboard.html');
    if (!fs.existsSync(filePath)) {
        console.error(`[FAIL] File not found: ${filePath}`);
        process.exit(1);
    }

    const url = 'file://' + filePath;
    console.log('Loading ' + url);
    
    await page.goto(url);
    
    console.log('Waiting for graph to render...');
    try {
        await page.waitForSelector('.node', { timeout: 10000 });
        console.log('[SUCCESS] Nodes detected in the lattice view.');
        
        // Wait a bit for clusters to calculate and render
        console.log('Waiting for cluster boxes...');
        await page.waitForSelector('.cluster-box', { timeout: 10000 }).catch(() => null);

        const clusterCount = await page.locator('.cluster-box').count();
        console.log(`Cluster boxes detected: ${clusterCount}`);
        if (clusterCount > 0) {
            console.log('[SUCCESS] Clustered layout elements verified.');
        } else {
            console.warn('[WARN] No cluster boxes detected. This might be normal if nodes aren\'t grouped yet.');
        }

        const tierLabelCount = await page.locator('.tier-label').count();
        console.log(`Tier labels detected: ${tierLabelCount}`);
        if (tierLabelCount > 0) {
            console.log('[SUCCESS] Chronological tier elements verified.');
        } else {
            console.error('[FAIL] No tier labels detected.');
        }

    } catch (e) {
        console.error('[FAIL] Simulation/Rendering failed.');
        await browser.close();
        process.exit(1);
    }
    
    console.log('Testing Hierarchy View...');
    await page.click('#btn-hierarchy');
    try {
        await page.waitForSelector('.tier-column', { timeout: 5000 });
        console.log('[SUCCESS] Tier columns detected in hierarchy view.');
    } catch (e) {
        console.error('[FAIL] Hierarchy view failed.');
    }

    const screenshotPath = 'output/visual_debug_v2.png';
    await page.screenshot({ path: screenshotPath, fullPage: true });
    console.log(`V2 Screenshot saved to ${screenshotPath}`);
    
    await browser.close();
})();
