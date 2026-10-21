/* Browser-only capability previews. Actual .NET examples remain in the original pages. */
"use strict";
(function () {
  var toolkit = document.body.dataset.product === "toolkit";
  /* 配色清单/名称/色卡来自 interact.js 读取的 assets/palette-meta.js（生成产物）。
   * 配色与明暗的唯一入口 = 外观菜单（#appearanceMenu），旧的 palDots/darkBtn/属性面板条目已移除。 */
  setPalette = function (p) {
    p = palKey(p); /* 旧 Key 别名归一 */
    document.documentElement.dataset.palette = p;
    syncAppearanceMenu();
    writeAppearance();
    renderPGrid(ACTIVE_DOC);
    log2both(tt("toast.palette") + PAL_NAME[p]);
  };
  /* 外观菜单（与 Vex 原型 theme.js 同构）：明暗模式三选 + 界面配色网格（推荐置顶、18px 圆形 8 色饼色卡）。
   * 切换即持久化（localStorage），刷新后恢复并经 palKey 归一旧 Key。 */
  var APPEARANCE_KEY = "codewf-avalonia-appearance";
  function readAppearance() { try { return JSON.parse(localStorage.getItem(APPEARANCE_KEY) || "null"); } catch (_) { return null; } }
  function writeAppearance() {
    try {
      localStorage.setItem(APPEARANCE_KEY, JSON.stringify({
        palette: document.documentElement.dataset.palette,
        themeMode: document.documentElement.dataset.themeMode || "system"
      }));
    } catch (_) {}
  }
  var appearanceMenu = $("appearanceMenu"), paletteGrid = $("paletteGrid");
  function paletteOrder() {
    var entries = PAL_LIST.map(function (key) { return window.CodeWFPalettes.entries[key]; });
    return entries.filter(function (entry) { return entry.recommended; }).concat(entries.filter(function (entry) { return !entry.recommended; }));
  }
  function renderPaletteGrid() {
    if (!paletteGrid) return;
    paletteGrid.innerHTML = paletteOrder().map(function (entry) {
      var name = LANG === "en-US" ? entry.en : entry.label;
      var dots = entry.swatch.map(function (color) { return '<i style="background:' + color + '"></i>'; }).join("");
      return '<button type="button" data-palette="' + entry.key + '" aria-pressed="false" aria-label="' + esc(name) + '">' +
        (entry.recommended ? '<em class="palette-badge">' + label("推荐", "Pick") + '</em>' : "") +
        '<span class="swatch-dots">' + dots + '</span>' +
        '<span class="palette-label"><b>' + esc(name) + '</b><small>' + esc(entry.note) + '</small></span></button>';
    }).join("");
    paletteGrid.querySelectorAll("button").forEach(function (b) { b.onclick = function () { setPalette(b.dataset.palette); }; });
    syncAppearanceMenu();
  }
  function syncAppearanceMenu() {
    if (!appearanceMenu) return;
    var current = document.documentElement.dataset.palette, mode = document.documentElement.dataset.themeMode || "system";
    var summary = appearanceMenu.querySelector("summary");
    summary.title = tt("appearance.menu");
    summary.setAttribute("aria-label", tt("appearance.menu"));
    appearanceMenu.querySelectorAll("[data-palette]").forEach(function (button) {
      var entry = window.CodeWFPalettes.entries[button.dataset.palette];
      var name = LANG === "en-US" ? entry.en : entry.label;
      button.title = name + " · " + (LANG === "en-US" ? entry.label : entry.en);
      button.setAttribute("aria-label", name);
      button.setAttribute("aria-pressed", String(button.dataset.palette === current));
    });
    appearanceMenu.querySelectorAll("input[name=codewf-theme-mode]").forEach(function (radio) { radio.checked = radio.value === mode; });
  }
  if (appearanceMenu) {
    appearanceMenu.querySelector(".theme-mode-options").addEventListener("change", function (event) { setTheme(event.target.value); });
    document.addEventListener("click", function (event) { if (!event.target.closest("#appearanceMenu")) appearanceMenu.open = false; });
    document.addEventListener("keydown", function (event) { if (event.key === "Escape" && appearanceMenu.open) { appearanceMenu.open = false; appearanceMenu.querySelector("summary").focus(); } });
    var originalSetLang = setLang;
    setLang = function (culture) { originalSetLang(culture); renderPaletteGrid(); };
  }
  var originalTheme = setTheme;
  var systemTheme = matchMedia("(prefers-color-scheme: dark)");
  var themeMode = "system";
  var savedAppearance = readAppearance();
  try { themeMode = JSON.parse(localStorage.getItem(LAYOUT_KEY) || "null")?.theme || "system"; } catch (_) {}
  if (savedAppearance && ["system", "light", "dark"].indexOf(savedAppearance.themeMode) >= 0) themeMode = savedAppearance.themeMode; /* 外观存档比手动布局存档新 */
  setTheme = function (mode) {
    themeMode = ["system", "light", "dark"].includes(mode) ? mode : "system";
    document.documentElement.dataset.themeMode = themeMode;
    originalTheme(themeMode === "system" ? (systemTheme.matches ? "dark" : "light") : themeMode);
    syncAppearanceMenu();
    writeAppearance();
    renderPGrid(ACTIVE_DOC);
  };
  var languageButton = $("langBtn"), languageMenu = $("languageMenu");
  var languageNames = {"zh-CN":"简体中文", "en-US":"English"};
  languageMenu.innerHTML = Object.keys(I18N).map(function(culture) { return '<button type="button" role="menuitemradio" data-language="' + culture + '">' + esc(languageNames[culture] || culture) + '</button>'; }).join('');
  function closeLanguages() { languageMenu.hidden = true; languageButton.setAttribute("aria-expanded", "false"); }
  languageButton.onclick = function(event) { event.stopPropagation(); languageMenu.hidden = !languageMenu.hidden; languageButton.setAttribute("aria-expanded", String(!languageMenu.hidden)); };
  languageButton.onkeydown = function(event) { if(event.key === "ArrowDown") { event.preventDefault(); languageMenu.hidden = false; languageButton.setAttribute("aria-expanded", "true"); languageMenu.querySelector('button').focus(); } };
  languageMenu.querySelectorAll('button').forEach(function(button) { button.onclick = function() { setLang(button.dataset.language); closeLanguages(); languageButton.focus(); }; });
  document.addEventListener('click', function(event) { if(!event.target.closest('.titlebar-language')) closeLanguages(); });
  document.addEventListener('keydown', function(event) { if(event.key === 'Escape' && !languageMenu.hidden) { closeLanguages(); languageButton.focus(); } });
  systemTheme.addEventListener("change", function() { if (themeMode === "system") setTheme("system"); });
  var originalRender = renderDoc;
  renderDoc = function (id) {
    $("dBody").dataset.page = id;
    originalRender(id);
    if (!matchMedia("(prefers-reduced-motion: reduce)").matches && $("dBody").animate) {
      $("dBody").animate([{ opacity:.4, transform:"translateY(5px)" },{ opacity:1, transform:"translateY(0)" }], { duration:230, easing:"ease-out" });
    }
  };
  var originalToggle = togglePanel;
  togglePanel = function (which) {
    originalToggle(which);
    if (!matchMedia("(prefers-reduced-motion: reduce)").matches && document.querySelector(".dockrow").animate) {
      document.querySelector(".dockrow").animate([{ opacity:.65 },{ opacity:1 }], { duration:220 });
    }
  };
  var wordmark = document.createElement("span");
  wordmark.className = "studio-wordmark";
  wordmark.textContent = toolkit ? "INFRASTRUCTURE LAB" : "INTERFACE STUDIO";
  document.querySelector(".cap .vbadge").after(wordmark);
  var originalRibbon = renderRBody;
  renderRBody = function () {
    if (curTab !== 'home') { originalRibbon(); return; }
    var quick = toolkit ? ['welcome','flow-runtime','flow-lab','net-packet','log-core'] : ['welcome','flow-write','flow-data','flow-docs','propgrid'];
    $('rBody').innerHTML = '<div class="grp"><div class="cmds">' + quick.map(function (id) { return cmdBtn({id:id}); }).join('') + '</div><div class="gname">' + (LANG === 'en-US' ? 'QUICK START' : '快速体验') + '</div></div><div class="studio-ribbon-note"><span>' + (toolkit ? 'MESSAGES → PROTOCOLS → DATA' : 'WRITE → EXPLORE → CREATE') + '</span><small>' + (LANG === 'en-US' ? 'Explore the libraries through interactive workflows' : '在完整场景中体验开源库的能力') + '</small></div>';
    $('rBody').querySelectorAll('.cbtn').forEach(function (b) { b.onclick = function () { openDoc(b.dataset.p); }; });
  };

  var panels = toolkit ? [
    ["events","消息流","flow-runtime"], ["packet","协议字节","net-packet"], ["pipeline","文件流水线","flow-lab"]
  ] : [
    ["markdown","实时 Markdown","flow-write"], ["table","自然排序","datagrid"], ["dock","Dock 布局","flow-docs"]
  ];
  var capabilities = toolkit ? [
    ["bus","事件驱动","Pub/Sub、Command / Query、特性扫描注册。","EventBus · Prism · AspNetCore"],
    ["bus-integrations","跨进程消息","TCP 总线、心跳重连与离线消息补发。","EventBus.Socket"],
    ["net-packet","二进制协议","字段布局声明、序列化与 TCP / UDP 收发。","NetWeaver · NetWrapper"],
    ["files-json","文件与数据","JSON / YAML / CSV / Excel 读写、7z 归档。","Files"],
    ["image-icon","图像工具链","缩放、水印、格式转换、ICO 与二维码。","Image"],
    ["log-core","运行期观测","文件轮转、UI 日志流、MEL 桥接与核心工具。","Logging · Microsoft · Core"]
  ] : [
    ["flow-write","Markdown 工作区","从源码编辑、实时预览到多格式导出。","Markdown · Markdown.Lite"],
    ["flow-data","数据交互","三态自然排序、列配置与树形属性编辑。","DataGrid · TreeDataGrid"],
    ["flow-docs","多文档工作区","停靠、面板显隐与梯形文档页签。","Dock · Controls"],
    ["transfer","输入与选择","搜索列表、穿梭框与 If / Switch 标记扩展。","Controls"],
    ["guide","引导与状态","页面引导、状态徽标、状态卡片与虚拟面板。","Controls"],
    ["log","日志与国际化","实时日志、桌面通知与强类型语言资源。","Log · Lang · Lang.Generator"]
  ];
  var selected = panels[0][0];
  var markdown = "# 让界面更有表现力\n\n用 **CodeWF.Avalonia** 构建桌面体验。\n\n- 实时 Markdown 预览\n- 三态排序与属性编辑\n- `Dock` 多文档工作区";
  var tableRows = [ ["item10","Markdown","就绪"], ["item2","DataGrid","就绪"], ["item1","Dock","就绪"], ["item100","PropertyGrid","就绪"] ];
  var sortState = 0;
  var dockNav = true, dockLog = true;
  var stageVersion = 0;
  var busy = false;
  function english() { return LANG === "en-US"; }
  function label(zh, en) { return english() ? en : zh; }
  function overview() {
    busy = false;
    stageVersion++;
    return '<div class="studio-overview"><div class="studio-eyebrow">CODEWF / ' + (toolkit ? 'INFRASTRUCTURE LAB' : 'INTERFACE STUDIO') + '</div>' +
      '<div class="studio-hero"><div><h2>' + (toolkit ? label('让底层能力，','Make infrastructure') : label('让桌面界面，','Give desktop interfaces')) + '<em>' + (toolkit ? label('看得见，跑得通。','visible. Make it flow.') : label('拥有表现力。','a little more possibility.')) + '</em></h2>' +
      '<p>' + (toolkit ? label('无 UI 依赖的 .NET 基础设施库。把消息、协议、文件与日志串起来，在交互中理解每个包的作用。','UI-independent .NET infrastructure. Explore messaging, binary protocols, files and logging through hands-on previews.') : label('面向 Avalonia 12 的开源控件与能力库。从一个控件，到完整的写作、数据与多文档工作区。','Open-source controls for Avalonia 12. Go from a single component to complete writing, data and document workspaces.')) + '</p></div>' +
      '<div class="studio-sigil" aria-hidden="true"><span>' + (toolkit ? '{ }' : '.NET') + '</span></div></div>' +
      '<div class="studio-meta"><span><b>' + (toolkit ? '11' : '9') + '</b> NUGET PACKAGES</span><span><b>' + allPageIds().length + '</b> ' + label('交互页面','DEMO PAGES') + '</span><span><b>.NET 10</b> / ' + (toolkit ? 'NO UI DEPENDENCY' : 'AVALONIA 12') + '</span><span>MIT LICENSE</span></div>' +
      '<section class="studio-stage" aria-label="' + label('能力体验','Capability preview') + '"><div class="studio-stage-head"><strong>' + label('动手试一试','Try it live') + '</strong><small>INTERACTIVE PREVIEW</small><div class="studio-tabs" role="tablist" aria-label="' + label('演示选择','Preview selection') + '">' +
      panels.map(function (p,i) { var ens = toolkit ? ['Message flow','Protocol bytes','File pipeline'] : ['Live Markdown','Natural sort','Dock layout']; return '<button type="button" id="studio-tab-' + p[0] + '" role="tab" aria-controls="studio-sandbox" aria-selected="' + (p[0] === selected) + '" data-stage="' + p[0] + '">' + label(p[1],ens[i]) + '</button>'; }).join('') +
      '</div></div><div class="studio-sandbox" id="studio-sandbox" role="tabpanel" aria-labelledby="studio-tab-' + selected + '"></div><div class="studio-stage-foot"><span id="studio-stage-hint"></span><button type="button" class="studio-link" id="studio-open-full">' + label('打开完整演示','Open full demo') + ' ↗</button></div></section>' +
      '<div class="studio-section-label"><span>' + label('从能力出发','Explore the capabilities') + '</span><small>EXPLORE THE LIBRARIES ↙</small></div>' +
      '<div class="studio-capabilities">' + capabilities.map(function (c,i) { var titles = toolkit ? ['Event-driven apps','Distributed messaging','Binary protocols','Files & data','Image toolchain','Runtime observability'] : ['Markdown workspace','Data interaction','Document workspace','Input & selection','Guides & status','Logging & localization']; return '<button type="button" class="studio-capability" data-open="' + c[0] + '"><span class="studio-icon">' + iconSvg(c[0],'s18') + '</span><span class="studio-arrow">↗</span><h3>' + label(c[1],titles[i]) + '</h3><p>' + label(c[2],c[3]) + '</p><small>' + c[3] + '</small></button>'; }).join('') + '</div>' +
      '<div class="studio-install"><code id="studio-install-command">dotnet add package CodeWF.' + (toolkit ? 'Toolkit.EventBus' : 'Avalonia.Controls') + '</code><button type="button" class="studio-link" id="studio-copy">' + label('复制安装命令','Copy install command') + ' ⧉</button></div>' +
      '<p class="studio-note">' + label('浏览器交互示意 · 完整页面展示对应 .NET API；配色遵循高对比令牌规则（8 套）。','Browser interaction previews · Full demos show the .NET APIs. Colors follow the high-contrast token rules (8 palettes).') + '</p></div>';
  }
  function bindOverview(el) {
    el.querySelectorAll('[data-stage]').forEach(function (b) { b.onclick = function () { selected = b.dataset.stage; drawStage(); }; });
    el.querySelectorAll('[data-open]').forEach(function (b) { b.onclick = function () { openDoc(b.dataset.open); }; });
    el.querySelector('#studio-open-full').onclick = function () { openDoc(panels.find(function (p) { return p[0] === selected; })[2]); };
    el.querySelector('#studio-copy').onclick = async function () {
      try { await navigator.clipboard.writeText($('studio-install-command').textContent); notify('ok',label('已复制','Copied'),label('安装命令已复制到剪贴板','Install command copied to clipboard')); }
      catch (e) { notify('warn',label('请手动复制','Copy manually'),$('studio-install-command').textContent); }
    };
    drawStage();
  }
  function drawStage() {
    stageVersion++;
    busy = false;
    var box = $('studio-sandbox'); if (!box) return;
    document.querySelectorAll('[data-stage]').forEach(function (b) { b.setAttribute('aria-selected',String(b.dataset.stage === selected)); });
    box.setAttribute('aria-labelledby','studio-tab-' + selected);
    var hint = $('studio-stage-hint');
    if (selected === 'markdown') {
      box.innerHTML = '<div class="studio-split"><div><div class="studio-label">SOURCE <span>MarkdownEditorView</span></div><textarea aria-label="Markdown 源码" class="studio-editor" id="studio-md"></textarea></div><div><div class="studio-label">PREVIEW <span>MarkdownViewer</span></div><div class="studio-preview" id="studio-md-preview"></div></div></div>';
      $('studio-md').value = markdown;
      function preview() {
        markdown = $('studio-md').value;
        var lines = esc(markdown).replace(/\*\*(.+?)\*\*/g,'<strong>$1</strong>').replace(/`([^`]+)`/g,'<code>$1</code>').split('\n');
        $('studio-md-preview').innerHTML = lines.map(function (line) { if (line.startsWith('# ')) return '<h3>' + line.slice(2) + '</h3>'; if (line.startsWith('- ')) return '<div>• ' + line.slice(2) + '</div>'; return '<div>' + (line || '<br>') + '</div>'; }).join('');
      }
      $('studio-md').oninput = preview; preview();
      hint.textContent = label('编辑左侧源码，右侧即时更新','Edit the source to update the preview');
    } else if (selected === 'table') {
      var rows = tableRows.slice();
      if (sortState) rows.sort(function (a,b) { return sortState === 1 ? a[0].localeCompare(b[0],undefined,{numeric:true}) : b[0].localeCompare(a[0],undefined,{numeric:true}); });
      box.innerHTML = '<div class="studio-label">DATAGRID <span>' + label('升序 → 降序 → 原始顺序','Ascending → descending → original order') + '</span></div><table class="studio-table"><thead><tr><th><button type="button" id="studio-sort">' + label('名称','Name') + ' ' + (sortState === 1 ? '↑' : sortState === 2 ? '↓' : '↕') + '</button></th><th>' + label('模块','Module') + '</th><th>' + label('状态','Status') + '</th></tr></thead><tbody>' + rows.map(function (r) { return '<tr><td>' + r[0] + '</td><td>' + r[1] + '</td><td><span class="studio-pill">' + label(r[2],'Ready') + '</span></td></tr>'; }).join('') + '</tbody></table>';
      $('studio-sort').onclick = function () { sortState = (sortState+1)%3; drawStage(); log2both('DataGrid: ' + ['original','ascending','descending'][sortState]); };
      hint.textContent = label('点击「名称」，观察 item2 与 item10 的自然排序','Click Name to compare item2 and item10 with natural sorting');
    } else if (selected === 'dock') {
      box.innerHTML = '<div class="studio-tools"><button type="button" class="studio-action" id="studio-toggle-nav" aria-pressed="' + dockNav + '">' + label('导航面板','Explorer') + '</button><button type="button" class="studio-action" id="studio-toggle-log" aria-pressed="' + dockLog + '">' + label('日志面板','Log panel') + '</button><span class="studio-label" style="margin:0">DockControl / Trapezoid tabs</span></div><div class="studio-dock" style="grid-template-columns:' + (dockNav ? '110px 1fr' : '1fr') + '">' + (dockNav ? '<aside class="studio-dock-nav">EXPLORER<br><br>▸ README.md<br>▸ App.axaml<br>▸ Theme.axaml</aside>' : '') + '<div class="studio-dock-doc"><div class="studio-tabs"><button type="button" aria-selected="true">README.md</button><button type="button" aria-selected="false">App.axaml</button></div><div class="studio-dock-copy">' + label('文档与工具面板共享一个工作区。<br>试着隐藏面板，把空间留给内容。','Documents and tool panels share a workspace.<br>Toggle a panel to give your content more room.') + '</div></div>' + (dockLog ? '<div class="studio-dock-log">[Info] Dock workspace ready · 2 documents</div>' : '') + '</div>';
      $('studio-toggle-nav').onclick = function () { dockNav = !dockNav; drawStage(); };
      $('studio-toggle-log').onclick = function () { dockLog = !dockLog; drawStage(); };
      box.querySelectorAll('.studio-dock-doc button').forEach(function (b,i) { b.onclick = function () { box.querySelectorAll('.studio-dock-doc button').forEach(function (a) { a.setAttribute('aria-selected',String(a === b)); }); box.querySelector('.studio-dock-copy').textContent = i ? '<codewf:CodeWFWindow> · DockControl · PropertyGrid' : label('文档与工具面板共享一个工作区。','Documents and tool panels share a workspace.'); }; });
      hint.textContent = label('面板显隐与文档切换的布局示意','Preview panel visibility and document switching');
    } else if (selected === 'events') {
      box.innerHTML = '<div class="studio-flow"><div class="studio-node">Publisher<code>GreetCommand</code></div><div class="studio-wire"></div><div class="studio-node">EventBus<code>PublishAsync</code></div><div class="studio-wire"></div><div class="studio-node">Subscriber<code>[EventHandler]</code></div><div class="studio-wire"></div><div class="studio-node">Logger<code>UI event feed</code></div></div><div class="studio-tools"><input id="studio-message" aria-label="消息内容" value="Hello, CodeWF!"><button type="button" class="studio-action primary" id="studio-publish">' + label('发布消息','Publish message') + ' →</button></div><div class="studio-console" id="studio-event-output" aria-live="polite">' + label('等待发布一条消息…','Waiting for a message…') + '</div>';
      $('studio-publish').onclick = async function () {
        if (busy) return;
        busy = true;
        var version = stageVersion, message = $('studio-message').value, button = $('studio-publish');
        button.disabled = true;
        var nodes = Array.from(box.querySelectorAll('.studio-node')), wires = Array.from(box.querySelectorAll('.studio-wire'));
        $('studio-event-output').textContent = 'PublishAsync(new GreetCommand("' + message + '"))';
        for (var i=0;i<nodes.length;i++) {
          if (version !== stageVersion || !box.isConnected || ACTIVE_DOC !== 'welcome') { busy=false; return; }
          nodes[i].classList.add('active'); if (wires[i]) wires[i].classList.add('active');
          await new Promise(function (resolve) { setTimeout(resolve,320); });
          if (wires[i]) wires[i].classList.remove('active');
        }
        if (version !== stageVersion || ACTIVE_DOC !== 'welcome') { busy=false; return; }
        $('studio-event-output').textContent += '\n[EventHandler] received: ' + message + '\n[Logger] message delivered';
        log2both('EventBus preview: ' + esc(message),'info');
        button.disabled=false; busy=false;
      };
      hint.textContent = label('发布 → 分发 → 订阅 → 日志，逐步点亮消息路径','Publish → dispatch → subscribe → log: follow the message');
    } else if (selected === 'packet') {
      box.innerHTML = '<div class="studio-label">NETWEAVER <span>Int32 · little-endian preview</span></div><div class="studio-tools"><input id="studio-packet-id" type="number" min="-2147483648" max="2147483647" aria-label="Int32 数值" value="42"><button type="button" class="studio-action primary" id="studio-serialize">' + label('编码字节','Encode bytes') + ' →</button></div><div class="studio-bytes" id="studio-bytes"></div><div class="studio-console" id="studio-packet-output" aria-live="polite"></div>';
      function serialize() {
        var input = $('studio-packet-id'), value = Number(input.value);
        if (input.value === '' || !Number.isInteger(value) || value < -2147483648 || value > 2147483647) { $('studio-packet-output').textContent = label('请输入有效的 Int32 整数。','Enter a valid Int32 integer.'); $('studio-bytes').innerHTML=''; return; }
        var buffer = new ArrayBuffer(4), view = new DataView(buffer); view.setInt32(0,value,true);
        var bytes = Array.from(new Uint8Array(buffer));
        $('studio-bytes').innerHTML = bytes.map(function (b,i) { return '<span style="animation-delay:' + (i*50) + 'ms">' + b.toString(16).toUpperCase().padStart(2,'0') + '</span>'; }).join('');
        $('studio-packet-output').textContent = 'Int32: ' + value + ' → 4 bytes → decoded: ' + view.getInt32(0,true) + '\n' + label('仅展示字段编码；完整数据包头与字段布局见完整演示。','Field encoding only. See the full demo for packet headers and layout.');
      }
      $('studio-serialize').onclick = serialize; $('studio-packet-id').oninput = serialize; serialize();
      hint.textContent = label('修改整数，查看真实字节变化与解码结果','Change the integer to inspect the bytes and decoded value');
    } else {
      box.innerHTML = '<div class="studio-label">FILES / IMAGE <span>' + label('场景编排示意','Workflow preview') + '</span></div><div class="studio-pipeline">' + ['JSON → YAML','7z archive','ICO / QR','Export'].map(function (name,i) { return '<div class="studio-node">' + name + '<code>STEP 0' + (i+1) + '</code></div>'; }).join('') + '</div><div class="studio-tools"><button type="button" class="studio-action primary" id="studio-run-pipeline">' + label('运行示意流程','Run preview') + ' →</button><span style="font-size:11px;color:var(--tx3)">' + label('4 个步骤 · 点击观察状态流转','4 steps · observe state transitions') + '</span></div><div class="studio-console" id="studio-pipeline-output" aria-live="polite">' + label('等待运行…','Waiting to run…') + '</div>';
      $('studio-run-pipeline').onclick = async function () {
        if (busy) return;
        busy=true;
        var version=stageVersion, button=$('studio-run-pipeline'), nodes=Array.from(box.querySelectorAll('.studio-node'));
        button.disabled=true;
        nodes.forEach(function (n) { n.classList.remove('done'); });
        for (var i=0;i<nodes.length;i++) {
          if (version !== stageVersion || ACTIVE_DOC !== 'welcome') { busy=false; return; }
          nodes[i].classList.add('active');
          $('studio-pipeline-output').textContent = label('正在演示步骤 ','Previewing step ') + (i+1) + '/4: ' + nodes[i].firstChild.textContent;
          await new Promise(function (resolve) { setTimeout(resolve,450); });
          if (version !== stageVersion || ACTIVE_DOC !== 'welcome') { busy=false; return; }
          nodes[i].classList.remove('active'); nodes[i].classList.add('done'); nodes[i].querySelector('code').textContent='✓ DONE';
        }
        $('studio-pipeline-output').textContent = label('流程示意完成。打开能力实验室查看 Files / Image 的 API 与详细操作。','Preview complete. Open the lab for Files / Image APIs and detailed operations.');
        log2both('Files / Image workflow preview completed','info'); button.disabled=false; busy=false;
      };
      hint.textContent = label('格式转换 → 归档 → 图像处理 → 导出','Convert → archive → image processing → export');
    }
  }
  // renderInto moves HTML out of its temporary element; bind after that move.
  REG('welcome', { zh:{ t:'CodeWF Studio',d:'',f:[] },render:overview });
  var originalRenderInto = renderInto;
  renderInto = function (id,el) { originalRenderInto(id,el); if (id === 'welcome' && el) bindOverview(el); };
  var current = savedAppearance && savedAppearance.palette ? palKey(savedAppearance.palette) : palKey(document.documentElement.dataset.palette);
  document.documentElement.dataset.palette = current; /* 恢复外观存档（含旧 Key 归一），不额外打启动日志 */
  setTheme(themeMode);
  renderPaletteGrid();
  writeAppearance();
  renderRBody();
  renderPGrid(ACTIVE_DOC);
  if (ACTIVE_DOC) renderDoc(ACTIVE_DOC);
})();
