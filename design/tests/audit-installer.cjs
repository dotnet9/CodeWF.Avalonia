const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');
const standaloneFixture = require('./standalone-fixture.cjs');

// 在空目录中验证单文件交付；输出只进入统一临时目录。
process.env.CODEWF_AUDIT_TEMP ||= 'E:/github/temp';
const output = process.env.CODEWF_INSTALLER_AUDIT_OUTPUT || 'E:/github/temp/installer-prototype-audit';
fs.mkdirSync(output, { recursive: true });
const source = process.env.CODEWF_INSTALLER_PROTOTYPE || path.resolve(__dirname, '../CodeWF.安装器原型.html');
const standalone = standaloneFixture(source);

(async () => {
  const browser = await chromium.launch({ headless: true });
  const errors = [];
  const results = [];
  try {
    const page = await browser.newPage({ viewport: { width: 1440, height: 1080 }, reducedMotion: 'reduce' });
    // 弹层只覆盖安装器内容，不能侵入原型工具栏或窗口标题栏。
    const verifyModal = async name => {
      const geometry = await page.locator('.modal-backdrop').evaluate(element => {
        const backdrop = element.getBoundingClientRect();
        const frame = document.querySelector('#installerWindow').getBoundingClientRect();
        const dialog = element.querySelector('.dialog').getBoundingClientRect();
        return {
          inset: [backdrop.left - frame.left, backdrop.top - frame.top, frame.right - backdrop.right, frame.bottom - backdrop.bottom],
          expectedInset: element.closest('[data-installer-platform="windows"]') ? [1, 60, 1, 17] : [1, 43, 1, 1],
          dialogFits: dialog.left >= backdrop.left && dialog.right <= backdrop.right && dialog.top >= backdrop.top && dialog.bottom <= backdrop.bottom,
        };
      });
      geometry.inset.forEach((value, index) => assert.ok(Math.abs(value - geometry.expectedInset[index]) <= 1, name + ': modal stays within window content'));
      assert.equal(geometry.dialogFits, true, name + ': dialog fits the backdrop');
      await page.locator('#installerWindow').screenshot({ path: path.join(output, name + '.png') });
      results.push(name + ': modal geometry');
    };
    await standalone.attach(page);
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.goto(standalone.url);
    await page.waitForFunction(() => window.CodeWFInstallerPrototype);
    assert.equal(await page.locator('#productSelect option').count(), 13);
    const defaults = await page.evaluate(() => ({ config: CodeWFInstallerPrototype.config, state: CodeWFInstallerPrototype.state, products: CodeWFInstallerPrototype.products }));
    const defaultProduct = defaults.products.find(product => product.id === defaults.config.defaultProduct);
    assert.equal(defaults.state.product, defaults.config.defaultProduct, 'Standalone copy selects its own repository');
    assert.equal(await page.locator('#productSelect').inputValue(), defaultProduct.id);
    assert.equal(await page.locator('.release-version').innerText(), defaultProduct.version);
    assert.ok((await page.title()).includes(defaultProduct.name));
    results.push('Standalone defaults: repository product, embedded version and page title');
    await page.clock.install();

    const variants = [
      { platform: 'windows', macMode: 'dmg', steps: ['welcome', 'license', 'destination', 'options', 'ready', 'complete', 'maintenance', 'remove', 'error'] },
      { platform: 'macos', macMode: 'dmg', steps: ['welcome', 'complete'] },
      { platform: 'macos', macMode: 'pkg', steps: ['welcome', 'license', 'destination', 'ready', 'complete', 'maintenance', 'remove', 'error'] },
      { platform: 'linux', macMode: 'dmg', steps: ['welcome', 'complete', 'removed', 'error'] },
    ];
    for (const width of [1440, 1120]) {
      await page.setViewportSize({ width, height: 1080 });
      for (const language of ['zh', 'en']) {
        for (const theme of ['light', 'dark']) {
          for (const variant of variants) {
            for (const step of variant.steps) {
              const options = { ...variant, step, language, theme, product: 'avalonia', scenario: 'new', reduced: true };
              delete options.steps;
              await page.evaluate(options => CodeWFInstallerPrototype.set(options), options);
              const geometry = await page.evaluate(() => {
                const frame = document.querySelector('#installerWindow');
                const screen = frame.querySelector('.screen,.finder-content,.linux-content');
                const rect = frame.getBoundingClientRect();
                return {
                  pageOverflow: document.documentElement.scrollWidth > innerWidth,
                  contentOverflow: screen.scrollWidth > screen.clientWidth + 1,
                  left: rect.left, right: rect.right, width: rect.width,
                  theme: document.documentElement.dataset.theme,
                  language: document.documentElement.lang,
                };
              });
              const name = `${width}/${language}/${theme}/${variant.platform}/${variant.macMode}/${step}`;
              assert.equal(geometry.pageOverflow, false, name + ': page has no horizontal overflow');
              assert.equal(geometry.contentOverflow, false, name + ': content has no horizontal overflow');
              assert.ok(geometry.left >= 0 && geometry.right <= width, name + ': frame fits the viewport');
              assert.equal(geometry.theme, theme);
              assert.equal(geometry.language, language === 'zh' ? 'zh-CN' : 'en');
              results.push(name);
              if (width === 1440 && (step === 'welcome' || variant.platform === 'windows' && language === 'zh' && theme === 'dark')) {
                await page.locator('#installerWindow').screenshot({ path: path.join(output, `${variant.platform}-${variant.macMode}-${language}-${theme}-${step}.png`) });
              }
            }
          }
        }
      }
    }

    await page.setViewportSize({ width: 1440, height: 1080 });
    await page.evaluate(() => CodeWFInstallerPrototype.set({ platform: 'windows', language: 'zh', theme: 'light', product: 'avalonia', scenario: 'new' }));
    const state = () => page.evaluate(() => CodeWFInstallerPrototype.state);
    await page.locator('#primaryAction').click();
    assert.equal((await state()).step, 'license');
    assert.equal(await page.locator('#primaryAction').isDisabled(), true);
    assert.ok((await page.locator('.license').innerText()).includes('MIT License\n\nCopyright'));
    await page.locator('#licenseAccepted').check();
    await page.locator('#primaryAction').click();
    assert.equal((await state()).step, 'destination');
    assert.equal(await page.locator('#installPath').getAttribute('aria-invalid'), 'false');
    assert.equal((await page.locator('#installPath').inputValue()).includes('\\\\'), false, 'Windows paths use one separator');
    await page.locator('#installPath').fill('relative-path');
    assert.equal(await page.locator('#primaryAction').isDisabled(), true);
    await page.locator('#installPath').fill('C:\\Windows\\System32');
    assert.equal(await page.locator('#primaryAction').isDisabled(), true);
    await page.locator('[data-action="browse"]').click();
    await page.locator('[data-action="pick-folder"]').nth(1).click();
    assert.equal(await page.locator('#installPath').inputValue(), 'D:\\Apps\\CodeWF.Avalonia Demo');
    await page.locator('#primaryAction').click();
    await page.locator('#desktopShortcut').check();
    await page.locator('#primaryAction').click();
    assert.equal((await state()).step, 'ready');
    assert.ok((await page.locator('.summary').innerText()).includes('D:\\Apps\\CodeWF.Avalonia Demo'));
    await page.locator('#primaryAction').click();
    assert.equal((await state()).step, 'installing');
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    assert.equal(await page.locator('#launchApp').isChecked(), true);
    await page.locator('#primaryAction').click();
    assert.equal(await page.locator('#installerWindow').isVisible(), false);
    await page.locator('#restoreButton').click();
    assert.equal(await page.locator('#installerWindow').isVisible(), true);
    results.push('Windows: license gate, invalid and protected path, folder picker, options, summary, completion, restore');

    await page.locator('#productSelect').selectOption('toolbox');
    await page.locator('[data-jump="destination"]').click();
    await page.locator('input[name="scope"][value="all"]').check();
    assert.equal((await state()).scope, 'all');
    assert.ok((await state()).path.startsWith('C:\\Program Files\\'));
    await page.locator('[data-jump="ready"]').click();
    await page.locator('#primaryAction').click();
    assert.equal((await state()).modal, 'auth');
    assert.ok((await page.locator('.dialog').innerText()).includes('未知发布者'));
    await verifyModal('windows-authorization');
    await page.locator('[data-action="modal-dismiss"]').click();
    assert.equal((await state()).step, 'ready');
    await page.locator('#primaryAction').click();
    await page.locator('#authorizeButton').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    results.push('Windows: all-user scope, authorization denial and approval');

    await page.locator('#scenarioSelect').selectOption('installed');
    await page.locator('#primaryAction').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).operation, 'repair');
    assert.ok((await page.locator('#screen').innerText()).includes('修复已完成'));
    await page.locator('#scenarioSelect').selectOption('new');
    await page.locator('#scenarioSelect').selectOption('installed');
    await page.locator('[data-action="choose-uninstall"]').click();
    await page.locator('#primaryAction').click();
    assert.equal(await page.locator('#keepData').isChecked(), true);
    await page.locator('#keepData').uncheck();
    assert.ok((await page.locator('#screen').innerText()).includes('无法撤销'));
    await page.locator('#keepData').check();
    await page.locator('#primaryAction').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'removed');
    assert.equal((await state()).keepData, true);
    results.push('Windows: repair and uninstall with explicit personal-data choice');

    await page.locator('#scenarioSelect').selectOption('error');
    await page.locator('[data-jump="installing"]').click();
    await page.clock.runFor(1800);
    assert.equal((await state()).step, 'error');
    await page.locator('[data-action="retry"]').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    await page.locator('#scenarioSelect').selectOption('new');
    await page.locator('[data-jump="installing"]').click();
    await page.locator('[data-action="cancel"]').click();
    assert.equal((await state()).modal, 'cancel');
    await verifyModal('windows-cancel-confirmation');
    await page.locator('[data-action="stop"]').focus();
    await page.keyboard.press('Tab');
    assert.equal(await page.locator('[data-action="modal-dismiss"]').evaluate(element => element === document.activeElement), true);
    await page.locator('[data-action="modal-dismiss"]').click();
    assert.equal((await state()).step, 'installing');
    await page.keyboard.press('Escape');
    await page.locator('[data-action="stop"]').click();
    assert.equal((await state()).step, 'welcome');
    results.push('Windows: recoverable failure, cancellation confirmation, focus trap, resume and stop');

    for (const item of await page.evaluate(() => CodeWFInstallerPrototype.products)) {
      await page.locator('#productSelect').selectOption(item.id);
      await page.locator('[data-jump="destination"]').click();
      assert.equal(await page.locator('input[value="user"]').isDisabled(), item.policy === 'admin');
      assert.equal(await page.locator('input[value="all"]').isDisabled(), item.policy === 'user');
    }
    await page.locator('#productSelect').selectOption('vex');
    await page.locator('[data-jump="options"]').click();
    assert.equal(await page.locator('#fileAssociations').count(), 1);
    assert.ok((await page.locator('#screen').innerText()).includes('.md、.markdown、.mdown'));
    results.push('Products: all 13 scope policies and Vex opening-option capability');

    await page.locator('[data-platform="macos"]').click();
    assert.equal(await page.locator('#installPath').count(), 0);
    await page.locator('#dmgApp').focus();
    await page.keyboard.press('Enter');
    assert.equal((await state()).step, 'copying');
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    await page.locator('#scenarioSelect').selectOption('installed');
    await page.locator('#dmgApp').dragTo(page.locator('#applicationsTarget'));
    assert.equal((await state()).modal, 'replace');
    await verifyModal('macos-dmg-replace');
    await page.locator('[data-action="replace"]').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    await page.locator('[data-action="mode-pkg"]').click();
    await page.locator('[data-jump="ready"]').click();
    await page.locator('#primaryAction').click();
    assert.equal((await state()).modal, 'auth');
    assert.equal(await page.locator('#previewPassword').getAttribute('readonly'), '');
    await verifyModal('macos-pkg-authorization');
    await page.locator('#authorizeButton').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    results.push('macOS: keyboard copy, drag-and-drop, replace-existing prompt, optional PKG authorization');

    await page.locator('[data-platform="linux"]').click();
    assert.equal(await page.locator('#installPath').count(), 0);
    assert.equal(await page.locator('input[name="scope"]').count(), 0);
    await page.locator('[data-action="linux-install"]').click();
    assert.equal((await state()).modal, 'auth');
    await verifyModal('linux-authorization');
    await page.locator('#authorizeButton').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'complete');
    await page.locator('[data-action="linux-remove"]').click();
    await page.locator('#authorizeButton').click();
    await page.clock.runFor(3900);
    assert.equal((await state()).step, 'removed');
    results.push('Linux: system-managed scope and destination, authorization, install and removal');

    await page.evaluate(() => CodeWFInstallerPrototype.set({ platform: 'windows', theme: 'system', scenario: 'new' }));
    await page.clock.resume();
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'dark');
    assert.equal(await page.locator('html').getAttribute('data-theme'), 'dark');
    await page.emulateMedia({ colorScheme: 'light' });
    await page.waitForFunction(() => document.documentElement.dataset.theme === 'light');
    assert.equal(await page.locator('html').getAttribute('data-theme'), 'light');
    assert.equal(await page.locator('#screen').evaluate(element => getComputedStyle(element).animationName), 'none');
    await page.locator('#reducedMotion').uncheck();
    await page.emulateMedia({ reducedMotion: 'no-preference' });
    await page.locator('#reducedMotion').check();
    assert.equal(await page.locator('#screen').evaluate(element => getComputedStyle(element).animationName), 'none');
    await page.locator('#resetButton').click();
    assert.equal((await state()).step, 'welcome');
    assert.equal((await state()).scenario, 'new');
    results.push('Accessibility: system appearance, reduced motion and reset');

    await page.emulateMedia({ reducedMotion: 'no-preference' });
    await page.evaluate(() => CodeWFInstallerPrototype.set({ platform: 'windows', language: 'zh', theme: 'dark', scenario: 'new', reduced: false }));
    assert.equal(await page.locator('#installerWindow').evaluate(element => getComputedStyle(element).getPropertyValue('--purple').trim()), '#512bd4');
    assert.notEqual(await page.locator('#installerWindow').evaluate(element => getComputedStyle(element).clipPath), 'none');
    assert.equal(await page.locator('#installerWindow').evaluate(element => {
      const rect = element.getBoundingClientRect();
      return element.contains(document.elementFromPoint(rect.left + 60, rect.top + 35));
    }), false, 'The top-left cutout has a real transparent hit region');
    assert.equal(await page.locator('#reactorLabel').innerText(), 'CodeWF');
    for (const link of await page.locator('[data-brand-link]').evaluateAll(links => links.map(link => ({ href: link.href, rel: link.rel, label: link.getAttribute('aria-label') })))) {
      assert.equal(link.href, 'https://codewf.com/');
      assert.ok(link.rel.includes('noopener') && link.rel.includes('noreferrer'));
      assert.ok(link.label);
    }
    assert.notEqual((await page.locator('.release-version').innerText()), '1.0.0');
    results.push('Brand: CodeWF identity, accessible website links, embedded product version');
    await page.screenshot({ path: path.join(output, 'sculpted-welcome-dark.png'), fullPage: true });
    const canvasImage = () => page.locator('#reactorCanvas').evaluate(canvas => canvas.toDataURL());
    const idleImage = await canvasImage();
    await page.clock.runFor(120);
    assert.notEqual(await canvasImage(), idleImage, 'Idle particle field animates');
    // 切换语言或步骤不能重建核心；更新进度不能重启阶段旋转图标。
    await page.evaluate(() => { window.retainedMotion = { canvas: document.querySelector('#reactorCanvas'), core: document.querySelector('.reactor-core'), orbit: document.querySelector('.ring-two'), animation: document.querySelector('.ring-two').getAnimations()[0] }; });
    await page.locator('#languageButton').click();
    await page.locator('[data-jump="license"]').click();
    assert.equal(await page.evaluate(() => retainedMotion.canvas === document.querySelector('#reactorCanvas') && retainedMotion.core === document.querySelector('.reactor-core') && retainedMotion.orbit === document.querySelector('.ring-two') && retainedMotion.animation === document.querySelector('.ring-two').getAnimations()[0]), true);
    await page.locator('#languageButton').click();
    await page.locator('#effectsButton').click();
    await page.evaluate(() => { window.retainedSpinner = document.querySelector('.phase.active .spinner'); });
    await page.clock.runFor(360);
    assert.equal(await page.evaluate(() => retainedSpinner === document.querySelector('.phase.active .spinner')), true, 'Progress ticks preserve the running spinner');
    assert.equal(await page.evaluate(() => retainedMotion.canvas === document.querySelector('#reactorCanvas') && retainedMotion.core === document.querySelector('.reactor-core')), true);
    results.push('Motion continuity: core/canvas/orbit survive render updates; spinner survives progress ticks');
    await page.clock.runFor(900);
    assert.equal((await state()).step, 'installing');
    assert.ok((await state()).progress > 0);
    assert.equal(await page.locator('#reactorLabel').innerText(), (await state()).progress + '%');
    await page.screenshot({ path: path.join(output, 'sculpted-installing-dark.png'), fullPage: true });
    await page.locator('#reducedMotion').check();
    const reducedImage = await canvasImage();
    await page.clock.runFor(120);
    assert.equal(await canvasImage(), reducedImage, 'Reduce motion stops canvas particle animation');
    assert.equal(await page.locator('.reactor-core').evaluate(element => getComputedStyle(element).animationName), 'none');
    await page.clock.runFor(2800);
    assert.equal((await state()).step, 'complete');
    await page.screenshot({ path: path.join(output, 'sculpted-complete-dark.png'), fullPage: true });
    results.push('Sculpted CodeWF: .NET purple, transparent cutout, idle particles, progress-synchronized energy core, completion, reduced-motion pause');

    await page.locator('[data-platform="macos"]').click();
    assert.equal(await page.locator('#effectsButton').isVisible(), false, 'Windows-only motion preview stays outside native macOS flow');
    await page.locator('[data-platform="linux"]').click();
    assert.equal(await page.locator('#effectsButton').isVisible(), false);
    results.push('Native platforms: sculpted motion preview does not replace DMG or DEB behavior');

    await standalone.verify(page);
    assert.deepEqual(errors, [], 'No script or browser errors');
    fs.rmSync(path.join(output, 'failure.txt'), { force: true });
    fs.writeFileSync(path.join(output, 'results.json'), JSON.stringify({ status: 'passed', count: results.length, results, errors }, null, 2));
    console.log(`PASS installer prototype: ${results.length} layout/state/interaction checks; ${output}`);
  } catch (error) {
    fs.writeFileSync(path.join(output, 'failure.txt'), error.stack || String(error));
    throw error;
  } finally {
    await browser.close();
    standalone.dispose();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
