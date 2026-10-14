const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
const { chromium } = require('playwright');

// Exercise the real HTML event handlers, including errors caught by the prototype's guard.
(async () => {
  const design = path.resolve(__dirname, '..');
  const catalog = JSON.parse(fs.readFileSync(path.join(design, 'catalog.json'), 'utf8'));
  const html = fs.readdirSync(design).find(name => /^CodeWF\..*Demo原型\.html$/.test(name));
  assert.ok(html, 'Prototype HTML is missing');
  const output = path.join(__dirname, 'artifacts');
  fs.mkdirSync(output, { recursive: true });
  const browser = await chromium.launch({ headless: true });
  const results = [];
  try {
    for (const width of [1440, 1120]) {
      const context = await browser.newContext({ viewport: { width, height: 860 } });
      const page = await context.newPage();
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
      try {
        await page.goto(pathToFileURL(path.join(design, html)).href);
        await page.waitForFunction(() => window.CodeWFDemoCatalog && typeof openDoc === 'function' && document.querySelector('#dBody[data-page]'));
        await page.evaluate(() => {
          window.auditGuardErrors = [];
          guard = function (action, tag) {
            try { return action(); }
            catch (error) { window.auditGuardErrors.push(tag + ': ' + error.message); }
          };
        });
        for (const culture of ['zh-CN', 'en-US']) {
          await page.evaluate(language => setLang(language), culture);
          assert.equal(await page.locator('html').getAttribute('lang'), culture);
          assert.equal(await page.locator('#rTabs [data-t]').count(), catalog.length);
          for (const group of catalog) {
            await page.locator(`#rTabs [data-t="${group.id}"]`).click();
            assert.equal(await page.locator('#rBody .cbtn').count(), group.items.length);
            for (const entry of group.items) {
              const route = entry.prototypeId;
              const button = page.locator(`#rBody .cbtn[data-p="${route}"]`);
              await button.click();
              await page.waitForFunction(id => document.querySelector('#dBody')?.dataset.page === id, route);
              const segment = page.locator(`#seg-${route}`);
              await page.waitForFunction(id => document.getElementById('seg-' + id)?.innerText.trim().length > 0, route);
              assert.ok(await segment.isVisible(), `${culture}: hidden page ${route}`);
              assert.ok((await segment.innerText()).trim(), `${culture}: empty page ${route}`);
              const expectedTitle = culture === 'zh-CN' ? entry.titleZh : entry.titleEn;
              if (route !== 'welcome') assert.equal(await page.locator('.catalog-header h1').innerText(), expectedTitle);
              assert.equal(await button.getAttribute('aria-pressed'), 'true', `Ribbon selection: ${route}`);
              await page.locator(`#lTree .lf[data-p="${route}"]`).click();
              assert.deepEqual(await page.evaluate(() => [ACTIVE_DOC, curTab]), [route, group.id]);
              assert.equal(await page.locator(`#lTree .lf.on[data-p="${route}"]`).count(), 1);
              const source = page.locator('#catalog-source-toggle');
              if (await source.count()) {
                await source.click();
                assert.ok(await page.locator('.catalog-source').isVisible());
                assert.ok((await page.locator('.catalog-source pre').innerText()).trim());
                await source.click();
                assert.equal(await page.locator('.catalog-source').isVisible(), false);
              }
              results.push({ width, culture, route });
            }
          }
        }
        assert.deepEqual(await page.evaluate(() => window.auditGuardErrors), [], 'Caught prototype exceptions');
        assert.doesNotMatch(await page.locator('#logP').innerText(), /JS 异常\[/, 'Startup guard exception');
        assert.deepEqual(errors, [], 'Browser errors');
      } catch (error) {
        await page.screenshot({ path: path.join(output, `failure-${width}.png`), fullPage: true });
        throw error;
      } finally { await context.close(); }
    }
    fs.writeFileSync(path.join(output, 'routes.json'), JSON.stringify(results, null, 2));
    console.log(`Passed: ${catalog.length} Ribbon groups, ${catalog.flatMap(group => group.items).length} routes, ${results.length} route/language/viewport checks.`);
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
