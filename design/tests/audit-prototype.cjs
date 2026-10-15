const standaloneFixture = require('./standalone-fixture.cjs');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
const { chromium } = require('playwright');
const properties = require('./audit-properties.cjs');

// Exercise the real HTML event handlers, including errors caught by the prototype's guard.
(async () => {
  const design = path.resolve(__dirname, '..');
  const catalog = JSON.parse(fs.readFileSync(path.join(design, 'catalog.json'), 'utf8'));
  const html = fs.readdirSync(design).find(name => /^CodeWF\..*Demo原型\.html$/.test(name));
  assert.ok(html, 'Prototype HTML is missing');
  const output = process.env.CODEWF_AUDIT_OUTPUT || path.join(__dirname, 'artifacts');
  fs.mkdirSync(output, { recursive: true });
  const standalone = standaloneFixture(path.join(design, html));
  const browser = await chromium.launch({ headless: true });
  const results = [];
  try {
    for (const width of [1440, 1120]) {
      const context = await browser.newContext({ viewport: { width, height: 860 } });
      const page = await context.newPage();
      await standalone.attach(page);
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
      try {
        await page.goto(standalone.url);
        await page.waitForFunction(() => window.CodeWFDemoCatalog && typeof openDoc === 'function' && document.querySelector('#dBody[data-page]'));
        assert.ok(await page.locator('.win').evaluate(element => parseFloat(getComputedStyle(element).borderTopWidth) >= 1));
        assert.equal(await page.locator('[data-window-action]').count(), 3);
        await page.locator('[data-window-action="maximize"]').click();
        assert.equal(await page.locator('body').evaluate(element => element.classList.contains('prototype-maximized')), true);
        await page.locator('[data-window-action="maximize"]').click();
        for (const action of ['minimize', 'close']) {
          await page.locator('[data-window-action="'+action+'"]').click();
          assert.equal(await page.locator('.win').isVisible(), false);
          await page.locator('.prototype-window-restore').click();
          assert.equal(await page.locator('.win').isVisible(), true);
        }
        await page.evaluate(() => {
          window.auditGuardErrors = [];
          guard = function (action, tag) {
            try { return action(); }
            catch (error) { window.auditGuardErrors.push(tag + ': ' + error.message); }
          };
        });
        // 验证八套配色与三态模式独立，以及系统主题动态变化。
        assert.equal(await page.locator('#palSel, #themeModeSel').count(), 0);
        assert.equal(await page.locator('#palDots button').count(), 8);
        async function chooseMode(mode) { for(let count=0;count<3 && await page.locator('#darkBtn').getAttribute('data-mode') !== mode;count++) await page.locator('#darkBtn').click(); assert.equal(await page.locator('#darkBtn').getAttribute('data-mode'), mode); }
        for (const palette of await page.locator('#palDots button').evaluateAll(items => items.map(item => item.dataset.p))) {
          await page.locator('#palDots button[data-p="'+palette+'"]').click();
          for (const mode of ['light', 'dark']) {
            await chooseMode(mode);
            assert.equal(await page.locator('html').getAttribute('data-theme'), mode);
            assert.equal(await page.locator('html').getAttribute('data-palette'), palette);
            assert.equal(await page.locator('html').getAttribute('data-theme-mode'), mode);
          }
        }
        await chooseMode('system');
        for (const mode of ['light', 'dark']) {
          await page.emulateMedia({ colorScheme: mode });
          await page.waitForFunction(expected => document.documentElement.dataset.theme === expected, mode);
          assert.equal(await page.locator('#darkBtn').getAttribute('data-mode'), 'system');
        }
        await chooseMode('light');
        await page.emulateMedia({ colorScheme: 'dark' });
        assert.equal(await page.locator('html').getAttribute('data-theme'), 'light');
        await page.locator('#palDots button[data-p="dotnet"]').click();
        for (const culture of ['zh-CN', 'en-US']) {
          await page.locator('#langBtn').click();
          assert.equal(await page.locator('#languageMenu').isVisible(), true);
          await page.locator('#languageMenu [data-language="'+culture+'"]').click();
          assert.equal(await page.locator('#languageMenu').isVisible(), false);
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
              await page.waitForFunction(id => { const segment=document.getElementById('seg-'+id); return segment && (segment.innerText.trim().length>0 || segment.querySelector('svg')); }, route);
              assert.ok(await segment.isVisible(), `${culture}: hidden page ${route}`);
              assert.ok((await segment.innerText()).trim() || await segment.locator('svg').count(), `${culture}: empty page ${route}`);
              await properties.verifyContract(page, route);
              const expectedTitle = culture === 'zh-CN' ? entry.titleZh : entry.titleEn;
              if (route !== 'welcome') assert.equal(await page.locator('.catalog-header h1').innerText(), expectedTitle);
              assert.equal(await button.getAttribute('aria-pressed'), 'true', `Ribbon selection: ${route}`);
              await page.locator(`#lTree .lf[data-p="${route}"]`).click();
              assert.deepEqual(await page.evaluate(() => [ACTIVE_DOC, curTab]), [route, group.id]);
              assert.equal(await page.locator(`#lTree .lf.on[data-p="${route}"]`).count(), 1);
              assert.deepEqual(await page.locator('select').evaluateAll(selects => selects.flatMap(select => [select, ...select.options]).filter(element => {
                const style = getComputedStyle(element);
                return style.textAlign !== 'left' || style.textAlignLast !== 'left';
              }).map(element => element.outerHTML)), [], `Dropdown alignment: ${route}`);
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
          await properties.verifyEffects(page, culture);
          await page.evaluate(() => openDoc('welcome'));
          const firstRow = page.locator('#pGrid .rw').first();
          const column = firstRow.locator('.property-column-handle');
          const columnBox = await column.boundingBox();
          const originalNameWidth = await firstRow.locator('label,.nm').evaluate(element => element.getBoundingClientRect().width);
          await page.mouse.move(columnBox.x + 3, columnBox.y + 10);
          await page.mouse.down();
          await page.mouse.move(columnBox.x + 33, columnBox.y + 10);
          await page.mouse.up();
          assert.ok(await firstRow.locator('label,.nm').evaluate(element => element.getBoundingClientRect().width) > originalNameWidth + 20, 'Column drag');
          const rowLine = firstRow.locator('.property-row-handle');
          const rowBox = await rowLine.boundingBox();
          const originalRowHeight = (await firstRow.boundingBox()).height;
          await page.mouse.move(rowBox.x + 30, rowBox.y + 3);
          await page.mouse.down();
          await page.mouse.move(rowBox.x + 30, rowBox.y + 21);
          await page.mouse.up();
          assert.ok((await firstRow.boundingBox()).height > originalRowHeight + 12, 'Row drag');
          const search = page.locator('.property-search');
          const modes = page.locator('.property-modes');
          assert.ok((await search.boundingBox()).y >= (await modes.boundingBox()).y + (await modes.boundingBox()).height, 'Search below sort buttons');
          const firstLabel = await firstRow.locator('label,.nm').innerText();
          await search.fill(firstLabel);
          assert.equal(await page.locator('#pGrid .rw:visible').count(), 1, 'Property search');
          await search.fill('');
          await page.locator('[data-mode="alphabetical"]').click();
          assert.equal(await page.locator('#pGrid .cat:visible').count(), 0, 'Alphabetical view');
          await page.locator('[data-mode="categorized"]').click();
          assert.ok(await page.locator('#pGrid .cat:visible').count() > 0, 'Categorized view');
        }
        assert.deepEqual(await page.evaluate(() => window.auditGuardErrors), [], 'Caught prototype exceptions');
        assert.doesNotMatch(await page.locator('#logP').innerText(), /JS 异常\[/, 'Startup guard exception');
        // 重新加载未编辑状态，单独验证默认文案随切语更新。
        await page.reload();
        await page.waitForFunction(() => window.CodeWFDemoCatalog && typeof openDoc === 'function' && document.querySelector('#dBody[data-page]'));
        await properties.verifyLiveDefaults(page);
        await standalone.verify(page);
        assert.deepEqual(errors, [], 'Browser errors');
      } catch (error) {
        await page.screenshot({ path: path.join(output, `failure-${width}.png`), fullPage: true });
        throw error;
      } finally { await context.close(); }
    }
    const propertyPrototype = standaloneFixture(path.join(design, '属性面板原型.html'));
    const propertyPage = await browser.newPage();
    try {
      const errors = [];
      propertyPage.on('pageerror', error => errors.push(error.message));
      await propertyPrototype.attach(propertyPage);
      await propertyPage.goto(propertyPrototype.url);
      assert.ok(await propertyPage.locator('#rows .row').count() > 0);
      for (const palette of ['dotnet', 'aurora', 'ember']) {
        await propertyPage.selectOption('#propertyPalette', palette);
        assert.equal(await propertyPage.locator('html').getAttribute('data-palette'), palette);
      }
      await propertyPage.locator('#btnAlpha').click();
      assert.equal(await propertyPage.locator('#rows .group-row').count(), 0);
      await propertyPage.locator('#btnCat').click();
      assert.ok(await propertyPage.locator('#rows .group-row').count() > 0);
      await propertyPage.locator('#search').fill('does-not-exist');
      assert.equal(await propertyPage.locator('#rows .row').count(), 0);
      await propertyPage.locator('#search').fill('');
      assert.ok(await propertyPage.locator('#rows .row').count() > 0);
      await propertyPrototype.verify(propertyPage);
      assert.deepEqual(errors, []);
    } finally { await propertyPage.close(); propertyPrototype.dispose(); }
    fs.writeFileSync(path.join(output, 'routes.json'), JSON.stringify(results, null, 2));
    console.log(`Passed: ${catalog.length} Ribbon groups, ${catalog.flatMap(group => group.items).length} routes, ${results.length} route/language/viewport checks.`);
  } finally { await browser.close(); standalone.dispose(); }
})().catch(error => { console.error(error); process.exitCode = 1; });

