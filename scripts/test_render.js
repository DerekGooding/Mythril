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

    const filePath = path.resolve('output/graph_visualizer_test.html');
    const url = 'file://' + filePath;
    console.log('Loading ' + url);
    
    await page.goto(url);
    
    // Wait for either the SVG or the error message
    console.log('Waiting for mermaid output...');
    await page.waitForTimeout(5000); // Give it plenty of time
    
    // Take a full page screenshot
    const screenshotPath = 'output/graph_debug.png';
    await page.screenshot({ path: screenshotPath, fullPage: true });
    console.log(`Screenshot saved to ${screenshotPath}`);
    
    // Check for error text in the page
    const content = await page.content();
    if (content.includes('Syntax error') || content.includes('Parser error')) {
        console.log('[FAIL] Syntax error detected in page content.');
    } else {
        const svgExists = await page.locator('.mermaid svg').count() > 0;
        console.log(`SVG element exists: ${svgExists}`);
    }

    await browser.close();
})();
