const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');
const standaloneFixture = require('./standalone-fixture.cjs');

// 在交付的单文件中切换真实路由，验证文字与最终合成背景；不只检查 CSS 变量是否存在。
(async () => {
  const design = path.resolve(__dirname, '..');
  const output = process.env.CODEWF_AUDIT_OUTPUT || path.join(__dirname, 'artifacts');
  fs.mkdirSync(output, { recursive: true });
  const standalone = standaloneFixture(path.join(design, 'CodeWF.Avalonia.Demo原型.html'));
  const browser = await chromium.launch({ headless: true });
  // 关闭进入动画，避免在淡入的中间帧测量文字。
  const page = await browser.newPage({ viewport: { width: 1440, height: 860 }, reducedMotion: 'reduce' });
  const errors = [];
  const measurements = [];
  page.on('pageerror', error => errors.push(error.message));
  try {
    await standalone.attach(page);
    await page.goto(standalone.url);
    await page.waitForFunction(() => typeof openDoc === 'function' && window.CodeWFDemoCatalog);
    for (const icon of ['#darkBtn svg', '#langBtn svg', '#notificationDemo svg'])
      assert.equal(await page.locator(icon).evaluate(element => element.getBoundingClientRect().width), 16, icon);
    for (const culture of ['zh-CN', 'en-US'])
    for (const palette of ['dotnet', 'aurora', 'ember', 'default', 'aqua', 'desert', 'dusk', 'night'])
    for (const mode of ['light', 'dark'])
    for (const route of ['status-badge', 'status-card']) {
      await page.evaluate(({ culture, palette, mode, route }) => {
        setLang(culture); setPalette(palette); setTheme(mode); openDoc(route);
        notify('err', 'Palette', 'Count');
      }, { culture, palette, mode, route });
      assert.equal(await page.locator('#notificationDemo').getAttribute('title'), culture === 'en-US' ? 'Notification demo' : '通知演示');
      const readings = await page.locator('#dBody .catalog-status :is(strong,span), #notificationCount').evaluateAll(elements => {
        const color = value => value.match(/[\d.]+/g).map(Number);
        const over = (front, back) => {
          const alpha = front[3] ?? 1;
          return [0, 1, 2].map(index => front[index] * alpha + back[index] * (1 - alpha));
        };
        const luminance = rgb => rgb.map(channel => channel / 255).map(value => value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4)
          .reduce((sum, value, index) => sum + value * [.2126, .7152, .0722][index], 0);
        return elements.filter(element => element.textContent.trim() && element.getClientRects().length).map(element => {
          const ancestors = [];
          for (let node = element; node; node = node.parentElement) ancestors.push(node);
          let background = [255, 255, 255];
          for (const ancestor of ancestors.reverse()) background = over(color(getComputedStyle(ancestor).backgroundColor), background);
          const style = getComputedStyle(element);
          const foreground = color(style.color);
          let opacity = Number(style.opacity);
          for (let node = element.parentElement; node; node = node.parentElement) opacity *= Number(getComputedStyle(node).opacity);
          foreground[3] = (foreground[3] ?? 1) * opacity;
          const text = luminance(over(foreground, background));
          const canvas = luminance(background);
          return { text: element.textContent.trim(), contrast: (Math.max(text, canvas) + .05) / (Math.min(text, canvas) + .05) };
        });
      });
      assert.ok(readings.length >= 12, `${route}: missing visible control samples`);
      for (const reading of readings) {
        measurements.push({ culture, palette, mode, route, ...reading });
        assert.ok(reading.contrast >= 4.5, `${culture}/${palette}/${mode}/${route}/${reading.text}: ${reading.contrast.toFixed(2)}:1`);
      }
    }
    await page.evaluate(() => { document.getElementById('nc').replaceChildren(); updateNotificationCount(); });
    assert.equal(await page.locator('#notificationCount').isVisible(), false);
    await page.locator('#notificationDemo').click();
    assert.equal(await page.locator('#notificationCount').textContent(), '1');
    await page.locator('#nc .nx').click();
    assert.equal(await page.locator('#notificationCount').isVisible(), false);
    await page.evaluate(() => { for (let i = 0; i < 10; i++) notify('err', 'Palette', 'Count'); });
    assert.equal(await page.locator('#notificationCount').textContent(), '4');
    await standalone.verify(page);
    assert.deepEqual(errors, []);
    fs.writeFileSync(path.join(output, 'palette-contrast.json'), JSON.stringify(measurements, null, 2));
    console.log(`PASS: 16 palette/mode combinations × 2 languages × 2 status routes; ${measurements.length} rendered text comparisons`);
  } catch (error) {
    await page.screenshot({ path: path.join(output, 'palette-failure.png') });
    throw error;
  } finally { await browser.close(); standalone.dispose(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
