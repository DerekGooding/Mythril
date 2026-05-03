const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

(async () => {
    const browser = await chromium.launch();
    const context = await browser.newContext();
    const page = await context.newPage();
    
    page.on('console', msg => {
        if (msg.type() === 'error') console.log(`PAGE LOG [${msg.type()}]: ${msg.text()}`);
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
        console.log('[SUCCESS] Nodes detected.');
        
        const edgeCount = await page.locator('.edge').count();
        if (edgeCount > 0) {
            console.log(`[SUCCESS] ${edgeCount} Edges detected.`);
        } else {
            throw new Error('No edges found');
        }

        // Check buttons
        await page.waitForSelector('#btn-standard');
        await page.waitForSelector('#btn-advanced');
        await page.waitForSelector('#btn-progressive');
        console.log('[SUCCESS] View buttons detected.');

        // Click Advanced
        console.log('Testing Advanced View...');
        await page.click('#btn-advanced');
        await page.waitForTimeout(500);
        const nodeCountAdv = await page.locator('.node').count();
        console.log(`Advanced node count: ${nodeCountAdv}`);

        // Click Progressive
        console.log('Testing Progressive View...');
        await page.click('#btn-progressive');
        await page.waitForTimeout(500);
        const nodeCountProg = await page.locator('.node').count();
        console.log(`Progressive node count: ${nodeCountProg}`);
        
        const milestoneCount = await page.locator('.milestone-node').count();
        console.log(`Milestone nodes detected: ${milestoneCount}`);

    } catch (e) {
        console.error('[FAIL] UI Verification failed:', e.message);
        await browser.close();
        process.exit(1);
    }
    
    await browser.close();
    console.log('[PASS] All UI regression checks completed.');
})();
