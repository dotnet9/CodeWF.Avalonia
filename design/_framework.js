<!DOCTYPE html>
<html lang="zh-CN" data-theme="light">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>CodeWF.Avalonia.Demo</title>
<style>
:root {
  --primary: #0064fa; --primary-hover: #0060e0; --primary-light: rgba(0,100,250,.08);
  --bg: #ffffff; --bg-sec: #f7f8fa; --bg-hover: #f2f3f5;
  --text: #1c1f23; --text-2: #646a73; --text-3: #8f959e;
  --border: #dee0e3; --border-light: #eceef1;
  --success: #00b42a; --warning: #ff7d00; --danger: #f53f3f;
  --shadow: 0 4px 10px rgba(0,0,0,.08);
  --radius: 6px; --row-h: 32px;
}
[data-theme=dark] {
  --primary: #4080ff; --primary-hover: #5c94ff; --primary-light: rgba(64,128,255,.12);
  --bg: #17171a; --bg-sec: #202024; --bg-hover: #2a2a2e;
  --text: #e8e8e8; --text-2: #a0a4ab; --text-3: #6e7278;
  --border: #3a3a3f; --border-light: #2a2a2e;
  --shadow: 0 4px 12px rgba(0,0,0,.5);
}
* { box-sizing: border-box; margin: 0; padding: 0; }
html, body { height: 100%; overflow: hidden; }
body { font: 13px/1.6 "Segoe UI","PingFang SC","Microsoft YaHei",sans-serif; color: var(--text); background: var(--bg); }

/* ── 标题栏 ── */
.tb { display:flex; align-items:center; height:36px; padding:0 0 0 12px; background:var(--bg-sec); border-bottom:1px solid var(--border); user-select:none; }
.tb .logo { font-weight:700; font-size:13px; color:var(--primary); margin-right:8px; }
.tb .ttl { font-size:12px; color:var(--text-2); flex:1; }
.tb .wbtns { display:flex; }
.tb .wbtn { width:40px; height:36px; display:flex; align-items:center; justify-content:center; border:none; background:transparent; cursor:pointer; color:var(--text-2); font-size:13px; }
.tb .wbtn:hover { background:var(--bg-hover); }
.tb .wbtn.close:hover { background:var(--danger); color:#fff; }

/* ── Ribbon ── */
.rbn-tabs { display:flex; align-items:flex-end; padding:0 8px; background:var(--bg-sec); border-bottom:1px solid var(--border); height:34px; gap:0; }
.rbn-tab { padding:0 18px; height:34px; line-height:34px; cursor:pointer; color:var(--text-2); font-size:13px; user-select:none; position:relative; }
.rbn-tab:hover { color:var(--text); }
.rbn-tab.on { color:var(--primary); font-weight:600; }
.rbn-tab.on::after { content:""; position:absolute; bottom:0; left:12px; right:12px; height:2px; background:var(--primary); border-radius:1px; }
.rbn-body { min-height:82px; background:var(--bg); border-bottom:1px solid var(--border); display:flex; align-items:stretch; padding:6px 12px; overflow:hidden; }
body.rbn-min .rbn-body { display:none; }
.rbn-grp { display:flex; flex-direction:column; padding:0 14px 0; border-right:1px solid var(--border-light); justify-content:space-between; }
.rbn-grp:last-child { border-right:none; }
.rbn-grp .cmds { display:flex; gap:2px; align-items:flex-start; flex:1; flex-wrap:wrap; }
.rbn-grp .cmds.col { flex-direction:column; }
.rbn-grp .glbl { text-align:center; font-size:10px; color:var(--text-3); padding-top:2px; }
.rbn-big { display:flex; flex-direction:column; align-items:center; gap:3px; width:64px; padding:6px 2px; border:none; background:transparent; cursor:pointer; border-radius:var(--radius); color:var(--text); }
.rbn-big:hover { background:var(--bg-hover); }
.rbn-big .ic { font-size:22px; line-height:1; }
.rbn-big .lb { font-size:11px; text-align:center; line-height:1.2; }
.rbn-sml { display:flex; align-items:center; gap:6px; width:100%; padding:3px 8px; border:none; background:transparent; cursor:pointer; border-radius:3px; color:var(--text); font-size:12px; text-align:left; white-space:nowrap; }
.rbn-sml:hover { background:var(--bg-hover); }
.rbn-sml .ic { font-size:12px; width:14px; text-align:center; color:var(--primary); flex:none; }

/* ── 工作区 ── */
.wk { display:grid; grid-template-columns:220px 6px 1fr 6px 264px; grid-template-rows:28px 1fr 160px; grid-template-areas:"lt dt rt" "lf dc rg" "lf bt rg"; height:calc(100vh - 152px); min-height:0; }
.wk.no-lt { grid-template-columns:0 6px 1fr 6px 264px; }
.wk.no-rt { grid-template-columns:220px 6px 1fr 6px 0; }
.wk.no-bt { grid-template-rows:28px 1fr 0; }
.wk.no-lt .pane-lt, .wk.no-lt .pane-lt-h, .wk.no-rt .pane-rt, .wk.no-rt .pane-rt-h, .wk.no-bt .pane-bt, .wk.no-bt .pane-bt-h { display:none !important; }
.pane { overflow:auto; background:var(--bg); }
.pane-h { display:flex; align-items:center; height:28px; padding:0 10px; background:var(--bg-sec); border-bottom:1px solid var(--border); font-size:11px; font-weight:600; color:var(--text-2); }
.pane-lt { grid-area:lf; } .pane-lt-h { grid-area:lt; }
.pane-rt { grid-area:rt; } .pane-rt-h { grid-area:rt; }
.pane-bt { grid-area:bt; background:var(--bg-sec); } .pane-bt-h { grid-area:bt; }
.sep-v { grid-area:none; width:6px; cursor:col-resize; }
.sep-h { grid-area:none; height:6px; cursor:row-resize; }

/* 文档页签 */
.dt-tabs { display:flex; align-items:flex-end; padding:4px 8px 0; gap:2px; background:var(--bg-sec); overflow-x:auto; }
.dt-tab { display:flex; align-items:center; gap:6px; padding:5px 12px; background:var(--bg); border:1px solid var(--border); border-bottom:none; border-radius:6px 6px 0 0; cursor:pointer; font-size:12px; color:var(--text-2); }
.dt-tab.on { color:var(--text); font-weight:600; border-bottom:2px solid var(--primary); }
.dt-tab .x { width:14px; height:14px; line-height:14px; text-align:center; border-radius:3px; font-size:11px; opacity:.5; }
.dt-tab .x:hover { background:var(--bg-hover); opacity:1; }
.dt-body { overflow:auto; }

/* ── Demo 页面 ── */
.pg-body { padding:16px 20px 40px; }
.pg-body .crumb { font-size:11px; color:var(--text-3); margin-bottom:4px; }
.pg-body h1 { font-size:18px; margin-bottom:4px; }
.pg-body .dsc { color:var(--text-2); font-size:12px; margin-bottom:14px; line-height:1.6; max-width:680px; }
.feat-tags { display:flex; gap:6px; flex-wrap:wrap; margin-bottom:14px; }
.feat-tags span { font-size:10px; padding:1px 8px; border-radius:10px; background:var(--primary-light); color:var(--primary); }
.demo-card { border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; margin-bottom:16px; }
.demo-card .dc-h { display:flex; align-items:center; padding:6px 12px; background:var(--bg-sec); border-bottom:1px solid var(--border); font-size:11px; color:var(--text-2); }
.demo-card .dc-h .sp { flex:1; }
.demo-card .dc-b { padding:16px; }
.code-blk { background:var(--bg-sec); border:1px solid var(--border-light); border-radius:var(--radius); padding:12px 14px; font:11px/1.6 Consolas,"Cascadia Code",monospace; white-space:pre; overflow-x:auto; color:var(--text-2); }

/* ── 属性面板 ── */
.pgp { padding:4px 8px 16px; }
.pgp .sbtn { font-size:10px; color:var(--text-3); padding:6px 4px 2px; font-weight:600; }
.prow2 { display:grid; grid-template-columns:96px 1fr; align-items:center; min-height:28px; padding:2px 4px; border-radius:3px; }
.prow2:hover { background:var(--bg-hover); }
.prow2 label { font-size:11px; color:var(--text-2); overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.prow2 .ctl { display:flex; align-items:center; gap:4px; }
.pgp select, .pgp input[type=text] { width:100%; height:24px; font:11px inherit; padding:0 6px; border:1px solid var(--border); border-radius:3px; background:var(--bg); color:var(--text); outline:none; }
.pgp select:focus, .pgp input:focus { border-color:var(--primary); }
.pgp input[type=range] { width:100%; accent-color:var(--primary); }
.pgp input[type=checkbox] { accent-color:var(--primary); }
.pgp .rv { font-size:10px; color:var(--text-3); min-width:28px; }
.chip { width:18px; height:18px; border-radius:3px; cursor:pointer; border:2px solid transparent; }
.chip.on { border-color:var(--primary); box-shadow:0 0 0 1px var(--bg); }
.pgp-empty { padding:16px 12px; color:var(--text-3); font-size:11px; line-height:1.6; }

/* ── 日志面板 ── */
.logp { padding:4px 10px; font:11px/1.8 Consolas,monospace; color:var(--text-2); }
.logp .t { color:var(--text-3); margin-right:6px; }
.logp .inf { color:var(--primary); } .logp .wrn { color:var(--warning); } .logp .err { color:var(--danger); }

/* ── 状态栏 ── */
.sb { display:flex; align-items:center; height:26px; padding:0 12px; gap:12px; background:var(--bg-sec); border-top:1px solid var(--border); font-size:11px; color:var(--text-2); }
.sb .sp { flex:1; }
.sb span { cursor:default; }

/* ── 控件 mock 样式 ── */
.mock-box { border:1px solid var(--border); border-radius:var(--radius); background:var(--bg); padding:12px; }
.mock-btn { display:inline-flex; align-items:center; gap:6px; height:28px; padding:0 14px; border:1px solid var(--border); border-radius:var(--radius); background:var(--bg); color:var(--text); cursor:pointer; font-size:12px; }
.mock-btn:hover { border-color:var(--primary); color:var(--primary); }
.mock-btn.pri { background:var(--primary); border-color:var(--primary); color:#fff; }
.mock-btn.pri:hover { background:var(--primary-hover); }
.gd-ov { position:relative; min-height:200px; background:var(--bg-sec); border-radius:var(--radius); overflow:hidden; }
.gd-ov .und { padding:16px; opacity:.25; }
.gd-ov .card { position:absolute; width:240px; background:var(--bg); border:1px solid var(--border); border-radius:8px; padding:12px; box-shadow:var(--shadow); z-index:2; }
.gd-ov .mask { position:absolute; inset:0; background:rgba(0,0,0,.4); }
.gd-ov .stp { font-size:10px; color:var(--text-3); }
.gd-ov .gtt { font-weight:700; margin:2px 0; }
.gd-ov .gds { font-size:11px; color:var(--text-2); margin-bottom:10px; }
.gd-ov .gbt { display:flex; gap:6px; justify-content:flex-end; }
.xfer2 { display:grid; grid-template-columns:1fr auto 1fr; gap:10px; align-items:center; width:100%; }
.xfer2 .lst { border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; }
.xfer2 .lst .h { padding:6px 10px; background:var(--bg-sec); font-weight:600; font-size:11px; border-bottom:1px solid var(--border); }
.xfer2 .lst ul { list-style:none; max-height:180px; overflow:auto; }
.xfer2 .lst li { padding:5px 12px; cursor:pointer; font-size:12px; }
.xfer2 .lst li:hover { background:var(--bg-hover); }
.xfer2 .lst li.sel { background:var(--primary-light); color:var(--primary); }
.xfer2 .mids { display:flex; flex-direction:column; gap:6px; }
.xfer2 .mids button { width:30px; height:26px; border-radius:15px; border:1px solid var(--border); background:var(--bg); cursor:pointer; color:var(--primary); font-size:13px; }
.xfer2 .mids button:hover { border-color:var(--primary); }
.slb { border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; max-width:320px; }
.slb .sin { width:100%; padding:6px 10px; border:none; border-bottom:1px solid var(--border); background:var(--bg); color:var(--text); outline:none; font:inherit; font-size:12px; }
.slb .scnt { padding:3px 10px; font-size:10px; color:var(--text-3); border-bottom:1px solid var(--border-light); }
.slb ul { list-style:none; max-height:180px; overflow:auto; }
.slb li { padding:5px 12px; cursor:pointer; }
.slb li:hover { background:var(--bg-hover); }
.slb li mark { background:rgba(0,100,250,.15); color:var(--primary); border-radius:2px; padding:0 1px; }
.trpz-tabs { display:flex; border-bottom:2px solid var(--border); }
.trpz-tabs .tt { padding:6px 20px; cursor:pointer; color:var(--text-2); font-size:12px; position:relative; }
.trpz-tabs .tt.on { color:var(--primary); font-weight:600; }
.trpz-tabs .tt.on::after { content:""; position:absolute; bottom:-2px; left:0; right:0; height:2px; background:var(--primary); }
.st-badge { display:inline-flex; border-radius:4px; overflow:hidden; font-size:11px; font-weight:500; }
.st-badge .l { padding:2px 8px; color:#fff; }
.st-badge .r { padding:2px 10px; }
.st-card { display:flex; gap:10px; border:1px solid var(--border); border-radius:8px; padding:12px 14px; background:var(--bg); min-width:240px; }
.st-card .ic { width:36px; height:36px; border-radius:8px; display:flex; align-items:center; justify-content:centers; font-size:18px; color:#fff; align-items:center; }
.st-card .tp { font-size:11px; color:var(--text-2); }
.st-card .bt { font-size:15px; font-weight:600; margin-top:2px; }
.dock-mock { display:grid; grid-template-columns:160px 1fr 200px; grid-template-rows:28px 1fr 22px; height:280px; border:1px solid var(--border); border-radius:var(--radius); overflow:hidden; font-size:11px; }
.dock-mock > div { border:1px solid var(--border); padding:6px; color:var(--text-2); display:flex; flex-direction:column; }
.dock-mock .dt { border:none; background:var(--bg-sec); flex-direction:row; align-items:center; gap:8px; }
.dock-mock .lt { background:var(--bg-sec); } .dock-mock .rt { background:var(--bg-sec); }
.dock-mock .bt { background:var(--bg-sec); flex-direction:row; align-items:center; }
.kv-form { display:grid; grid-template-columns:auto 1fr; gap:6px 16px; font-size:12px; align-items:center; }
.kv-form .k { color:var(--text-2); text-align:right; }
.kv-form .v { color:var(--text); font-weight:500; }
</style>
</head>
<body>

<div class="tb">
  <span class="logo">◆</span>
  <span class="ttl">CodeWF.Avalonia.Demo</span>
  <span class="ttl" style="opacity:.5">— v12.1.4.2</span>
  <span style="flex:1"></span>
  <button class="wbtn">—</button><button class="wbtn">□</button><button class="wbtn close">✕</button>
</div>

<div class="rbn-tabs" id="rbnTabs">
  <span class="rbn-tab on" data-t="home">首 页</span>
  <span class="rbn-tab" data-t="markdown">Markdown</span>
  <span class="rbn-tab" data-t="data">数 据</span>
  <span class="rbn-tab" data-t="controls">控 件</span>
  <span class="rbn-tab" data-t="layout">布 局</span>
  <span class="rbn-tab" data-t="infra">基础设施</span>
</div>
<div class="rbn-body" id="rbnBody"></div>

<div class="wk" id="wk">
  <div class="pane-h pane-lt-h"><span>◈ 控件资源管理器</span></div>
  <div class="dt-tabs" id="dTabs"></div>
  <div class="pane-h pane-rt-h"><span>⚙ 属性</span></div>
  <div class="pane pane-lt" id="paneLt"><div class="ltree" id="ltree"></div></div>
  <div class="pane dt-body" id="dBody"></div>
  <div class="pane pane-rt" id="paneRt"><div class="pgp" id="pgrid"></div></div>
  <div class="pane-h pane-bt-h"><span>▤ 日志</span></div>
  <div class="pane pane-bt" id="paneBt"><div class="logp" id="logP"></div></div>
</div>

<div class="sb">
  <span>就绪</span>
  <span class="sp"></span>
  <span id="langTgl" style="cursor:pointer">文 A</span>
  <span id="themeTgl" style="cursor:pointer">◐</span>
</div>

<script>
"use strict";
/* ════════════════════════════════════════════
   CodeWF.Avalonia.Demo 原型
   数据驱动 · 每控件独立展示页 · PropertyGrid 联动
   ════════════════════════════════════════════ */

var $id = function(id) { return document.getElementById(id); };
var esc = function(s) { return String(s || "").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;"); };
function dlog(msg, cls) {
  var box = $id("logP");
  var el = document.createElement("div");
  el.innerHTML = '<span class="t">[' + stamp() + ']</span><span class="' + (cls||"inf") + '">' + esc(msg) + '</span>';
  box.appendChild(el);
  while (box.children.length > 50) box.removeChild(box.firstChild);
  box.scrollTop = box.scrollHeight;
}
function stamp() {
  var d = new Date();
  return ("0"+d.getHours()).slice(-2)+":"+("0"+d.getMinutes()).slice(-2)+":"+("0"+d.getSeconds()).slice(-2);
}

/* ═══ 属性项数据模型 ═══ */
/* PropertyItem: { id, name, kind, value, def, desc, children, options, min, max, step, unit, fmt, watermark, ro, commit, theme, summary, visible } */

var TREE = [
  { grp:"Markdown" },
  { page:"md-viewer",    ic:"▤", name:"MarkdownViewer" },
  { page:"md-lite",      ic: "▦", name:"Markdown Lite" },
  { page:"md-editor",    ic:"✎", name:"Markdown 编辑器" },
  { page:"md-live",      ic:"⟳", name:"Markdown 实时编辑" },
  { page:"md-export",    ic:"⇩", name:"导出与富复制" },
  { grp:"数据" },
  { page:"datagrid",     ic:"▥", name:"DataGrid" },
  { page:"propertygrid", ic:"⚙", name:"PropertyGrid" },
  { grp:"控件" },
  { page:"codewfwindow", ic:"❐", name:"CodeWFWindow" },
  { page:"searchlistbox",ic:"☰", name:"SearchListBox" },
  { page:"transfer",     ic:"⇄", name:"Transfer" },
  { page:"status",       ic:"◉", name:"状态组件" },
  { page:"guide",        ic:"➤", name:"Guide 新手引导" },
  { page:"windowedstack",ic:"◫", name:"WindowedStackPanel" },
  { page:"columndisplay",ic:"⛭", name:"ColumnDisplayEditor" },
  { page:"trapezoid",    ic:"▰", name:"梯形页签" },
  { page:"markup",       ic:"{}", name:"Markup 扩展" },
  { grp:"基础设施" },
  { page:"log",          ic:"▤", name:"Log 日志" },
  { page:"lang",         ic:"文", name:"Lang 多语言" }
];

/* ═══ 演示页定义 ═══ */
var PAGES = {

"home": {
  title:"控件总览", crumb:"首页",
  desc:"CodeWF.Avalonia 全部控件的交互演示。从 Ribbon 菜单或左侧资源管理器打开，右侧属性面板实时调整。",
  render: function(el) {
    var cards = [
      { p:"propertygrid", ic:"⚙", n:"PropertyGrid", pkg:"TreeDataGrid",
        d:"VS 风格属性面板：分类/字母视图、无限层级、搜索过滤、描述面板、11 种编辑器、属性联动。",
        feat:["分类/字母","搜索","属性联动"] },
      { p:"md-viewer", ic:"▤", n:"MarkdownViewer", pkg:"Markdown",
        d:"后台解析 + 增量渲染 + 块级虚拟化；18 套排版主题；图片/公式/Mermaid/代码高亮（完整版）。",
        feat:["18 主题","虚拟化","增量渲染"] },
      { p:"datagrid", ic:"▥", n:"DataGrid", pkg:"DataGrid",
        d:"Semi 风格 DataGrid：三态排序、自然排序、智能 ToolTip、网格线、行状态。",
        feat:["三态排序","自然排序","Semi 主题"] },
      { p:"guide", ic:"➤", n:"Guide", pkg:"Controls",
        d:"新手引导系统：聚光遮罩、13 种弹出方位、箭头对齐、键盘导航、动态步骤。",
        feat:["聚光遮罩","13 方位","键盘导航"] },
      { p:"codewfwindow", ic:"❐", n:"CodeWFWindow", pkg:"Controls",
        d:"无边框窗口：8 向缩放、标题栏拖拽、全屏按钮、内容钻入、Win11 原生圆角。",
        feat:["无边框","8 向缩放","DWM 圆角"] },
      { p:"transfer", ic:"⇄", n:"Transfer", pkg:"Controls",
        d:"双列表穿梭框：按钮/双击移动，内建搜索过滤，GridSplitter 调宽。",
        feat:["穿梭","搜索过滤","GridSplitter"] },
      { p:"status", ic:"◉", n:"状态组件", pkg:"Controls",
        d:"StatusBadge / StatusCard / StatusLabel：五种语义状态，深浅主题自动换肤。",
        feat:["5 种状态","AntD 配色","深浅换肤"] },
      { p:"dock", ic:"◆", n:"Dock 布局", pkg:"Dock",
        d:"Dock.Avalonia Semi 皮肤：文档页签、工具停靠/浮动、GridDock 自动折叠、布局持久化。",
        feat:["文档/工具","停靠/浮动","持久化"] },
      { p:"log", ic:"▤", n:"Log", pkg:"Log",
        d:"文件日志 + LogView 应用内查看器：级别过滤、节流刷新、InApp/Desktop 通知。",
        feat:["级别过滤","实时流","桌面通知"] },
      { p:"lang", ic:"文", n:"Lang 多语言", pkg:"Lang",
        d:"插件化多语言：Json / Xml / Resx 三种后端，运行时切换，强类型 Key 源生成。",
        feat:["Json/Xml/Resx","运行时切换","强类型"] },
      { p:"searchlistbox", ic:"☰", n:"SearchListBox", pkg:"Controls",
        d:"搜索 + 多选列表：400ms 防抖过滤、计数条、双击/选择事件。",
        feat:["防抖过滤","计数条","多选"] },
      { p:"trapezoid", ic:"▰", n:"梯形页签", pkg:"Controls",
        d:"梯形带斜边页签边框：首/中/末三种路径自适应，随增删自动切换。",
        feat:["梯形路径","自适应","圆角"] },
    ];
    var html = '<p class="dsc">' + this.desc + '</p><div class="card-grid" style="display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:14px">';
    cards.forEach(function(c) {
      var feats = c.feat.map(function(f){ return "<span>"+f+"</span>"; }).join("");
      html += '<div class="ccard" data-goto="'+c.p+'" style="border:1px solid var(--border);border-radius:8px;padding:14px;cursor:pointer;background:var(--bg);transition:all .15s" onmouseover="this.style.borderColor=\'var(--primary)\';this.style.boxShadow=\'var(--shadow)\'" onmouseout="this.style.borderColor=\'var(--border)\';this.style.boxShadow=\'none\'">' +
        '<div style="display:flex;align-items:center;gap:8px;font-weight:700"><span>'+c.ic+'</span><span>'+c.n+'</span></div>' +
        '<div style="margin-top:6px;color:var(--text-2);font-size:12px;line-height:1.5;min-height:48px">'+c.d+'</div>' +
        '<div style="margin-top:8px;display:flex;gap:6px;flex-wrap:wrap">'+feats+'</div>' +
        '<div style="margin-top:8px;font-size:10px;color:var(--primary)">'+c.pkg+'</div></div>';
    });
    html += "</div>";
    el.innerHTML = html;
    el.querySelectorAll(".ccard[data-goto]").forEach(function(c) {
      c.addEventListener("click", function() { goto(c.dataset.goto); });
    });
  }
},

"md-viewer": {
  title:"MarkdownViewer", crumb:"Markdown / MarkdownViewer",
  desc:"后台解析 + 70ms 合并窗口 + 块级增量渲染 + WindowedStackPanel 块级虚拟化。完整版叠加 TextMate 代码高亮、图片（LRU 缓存 + 点击预览窗）、数学公式（CSharpMath）、Mermaid 图表、导出 PNG/PDF/Word/HTML。",
  feats:["增量渲染","块级虚拟化","18 排版主题","任务回写","阅读位置","导出"],
  props: [
    { grp:"排版" },
    { id:"md-theme", label:"TypographyTheme", type:"combo", options:["Basic","OrangeHeart","InkBlack","ColorfulPurple","TenderGreen","Verdant","RedScarlet","BlueGlow","TechnologyBlue","LanQing","Yamabuki","FrontendPeak","GeekBlack","Simple","RosePurple","CuteGreen","FullStackBlue","WeChatFormat"], value:"CuteGreen",
      apply:function(v, el) { el.style.setProperty("--md-accent", THEME_COLORS[v] || "#48B378"); } },
    { id:"md-size", label:"TypographySize", type:"combo", options:["Normal","Small"], value:"Normal",
      apply:function(v, el) { el.classList.toggle("md-small", v === "Small"); } },
    { id:"md-fs", label:"ParagraphFontSize", type:"slider", min:12, max:20, value:14,
      apply:function(v, el) { el.style.fontSize = v + "px"; } },
    { grp:"行为" },
    { id:"md-virt", label:"EnableVirtualization", type:"bool", value:true,
      apply:function(v) { dlog("EnableVirtualization = " + v); } }
  ],
  stage:function(el) {
    var accent = "#48B378";
    el.innerHTML =
      '<div class="md-prev" id="mdPrev" style="--md-accent:' + accent + ';max-width:620px;font-size:14px;line-height:1.8">' +
      '<h2 style="color:var(--md-accent);font-size:1.4em;margin:0 0 10px">Quick Start</h2>' +
      '<p style="margin:8px 0">支持 <b style="color:var(--md-accent)">GFM 表格</b>、围栏代码块、<s>删除线</s>、任务列表：</p>' +
      '<div class="task" style="display:flex;gap:6px;align-items:center;margin:4px 0"><input type="checkbox" checked style="accent-color:var(--md-accent)"><span>设计 ShowMeCode 控件</span></div>' +
      '<div class="task" style="display:flex;gap:6px;align-items:center;margin:4px 0"><input type="checkbox"><span>实现 Dock 布局</span></div>' +
      '<pre style="background:var(--bg-sec);border:1px solid var(--border);border-radius:4px;padding:10px;font:12px Consolas,monospace;overflow:auto;margin:10px 0">var viewer = new MarkdownViewer {\n    TypographyTheme = "CuteGreen",\n    EnableVirtualization = true\n};</pre>' +
      '<table style="border-collapse:collapse;width:100%;font-size:12px;margin:10px 0">' +
      '<tr><th style="border:1px solid var(--border);padding:4px 10px;background:var(--bg-sec)">Feature</th><th style="border:1px solid var(--border);padding:4px 10px;background:var(--bg-sec)">Status</th></tr>' +
      '<tr><td style="border:1px solid var(--border);padding:4px 10px">Virtualization</td><td style="border:1px solid var(--border);padding:4px 10px">✓ Built-in</td></tr>' +
      '<tr><td style="border:1px solid var(--border);padding:4px 10px">Incremental render</td><td style="border:1px solid var(--border);padding:4px 10px">✓ 70ms debounce</td></tr>' +
      '</table>' +
      '<p style="margin:8px 0">公式：$E = mc^2$ （完整版渲染为排版公式）</p>' +
      '</div>';
  }
},

"md-lite": {
  title:"Markdown Lite", crumb:"Markdown / Markdown Lite",
  desc:"仅依赖 Avalonia + Markdig 的轻量渲染：标题 / 段落 / 列表 / 引用 / 表格 / 链接 / 单色代码块；图片显示替代文本、公式显示原文；内置 18 套排版主题。适合包体积敏感或只读预览场景。",
  feats:["零额外依赖","18 排版主题","化学式"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="md-prev" style="max-width:520px"><h3 style="color:var(--primary);margin-bottom:8px">Lite 预览</h3><p>常规元素完整支持；公式 <b>$E=mc^2$</b> 显示原文、图片显示 <span style="color:var(--text-3)">[image] alt</span>。</p></div>';
  }
},

"md-editor": {
  title:"Markdown 编辑器", crumb:"Markdown / 编辑器",
  desc:"MarkdownEditorView 基于 AvaloniaEdit：语法着色、行号、当前行高亮、智能续行、成对符号自动插入与选区包裹、粘贴 CSV 自动转表格、查找替换、40 项编辑动作。",
  feats:["AvaloniaEdit","智能续行","自动配对","CSV→表格"],
  props: [
    { id:"ed-linenum", label:"ShowLineNumbers", type:"bool", value:true, apply:function(){} },
    { id:"ed-fs", label:"EditorFontSize", type:"slider", min:11, max:20, value:15, apply:function(){} },
    { id:"ed-pair", label:"EnableAutoPair", type:"bool", value:true, apply:function(){} }
  ],
  stage:function(el) {
    el.innerHTML = '<div class="mock-box" style="display:grid;grid-template-columns:1fr 1fr;gap:0;min-height:220px">' +
      '<div style="border-right:1px solid var(--border);padding:12px;font:12px Consolas,monospace;line-height:1.8">' +
      '<span style="color:var(--primary)"># Title</span><br><span style="color:var(--text-2)">Some **bold** text here</span><br>' +
      '<span style="color:var(--text-2)">- item 1</span><br><span style="color:var(--text-2)">- item 2</span><br>' +
      '<span style="color:var(--text-3)">// comment</span><br><span style="color:var(--text-2)">```csharp</span></div>' +
      '<div style="padding:12px"><h4 style="font-size:12px;margin-bottom:6px">Preview</h4>' +
      '<b style="color:var(--primary)">Title</b><br>Some <b>bold</b> text here<br>• item 1<br>• item 2</div></div>';
  }
},

"md-live": {
  title:"Markdown 实时编辑", crumb:"Markdown / 实时编辑",
  desc:"MarkdownLiveEditorView：所见即所得块编辑——每个 Markdown 块渲染为富文本，点入该块就地编辑源码，失焦即回渲染；表格直接增删行列（MarkdownTableEditor）、任务列表直接勾选。",
  feats:["所见即所得","块编辑","表格编辑"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="mock-box"><p style="color:var(--text-3);font-size:11px;margin-bottom:8px">点击任意块进入编辑模式，失焦自动回渲染</p>' +
      '<div style="border:1px solid var(--primary);border-radius:4px;padding:8px;margin:4px 0">' +
      '<span style="color:var(--text-3);font-size:10px">编辑中</span><br>' +
      '<input type="text" value="# 实时编辑标题" style="width:100%;border:none;background:transparent;color:var(--text);font:14px inherit;outline:none" /></div>' +
      '<div style="padding:8px;margin:4px 0;border-radius:4px">这是普通段落，点击可进入编辑。</div></div>';
  }
},

"md-export": {
  title:"导出与富复制", crumb:"Markdown / 导出",
  desc:"MarkdownDocumentExporter：一键导出 PNG / PDF / Word / HTML（自包含单文件，本地图片内嵌 data URI）；公众号 / 知乎 / 掘金富复制（SetMarkdownHtmlAsync，粘贴即保留排版）。",
  feats:["PNG","PDF","Word","HTML","公众号复制"],
  props: [],
  stage:function(el) {
    var btns = ["PNG","PDF","Word","HTML","公众号复制"];
    var html = '<div style="display:flex;gap:8px;flex-wrap:wrap">';
    btns.forEach(function(b) { html += '<button class="mock-btn pri" data-x="' + b + '">' + b + '</button>'; });
    html += '</div>';
    el.innerHTML = html;
  },
  wire:function(root) {
    root.querySelectorAll("button[data-x]").forEach(function(b) {
      b.addEventListener("click", function() { dlog("导出：" + b.dataset.x); });
    });
  }
},

"datagrid": {
  title:"DataGrid", crumb:"数据 / DataGrid",
  desc:"Semi 风格 DataGrid + DataGridExtension 增强：三态排序（升 → 降 → 恢复原序）、自然排序（item2 < item10）、智能 ToolTip（超长文本才显示）。",
  feats:["三态排序","自然排序","智能 ToolTip","Semi 主题"],
  props: [
    { id:"dg-lines", label:"GridLinesVisibility", type:"combo", options:["Horizontal","All","None"], value:"Horizontal",
      apply:function(v, root) {
        var t = root.querySelector(".dg-mock-tbl");
        if (!t) return;
        t.classList.toggle("ln-h", v === "Horizontal");
        t.classList.toggle("ln-a", v === "All");
      } },
    { id:"dg-zebra", label:"Zebra rows", type:"bool", value:false,
      apply:function(v, root) { var t = root.querySelector(".dg-mock-tbl"); if (t) t.classList.toggle("zebra", v); } }
  ],
  stage:function(el) {
    var rows = [
      ["Zhang San","28","Hangzhou"], ["Li Si","35","Beijing"],
      ["Wang Wu","19","Chengdu"], ["item2","22","Suzhou"], ["item10","31","Wuhan"]
    ];
    var html = '<div class="stage" style="display:block;padding:0"><table class="dg-mock-tbl ln-h" style="border-collapse:collapse;width:100%;font-size:12px">';
    html += '<thead><tr>';
    ["Name ▲","Age","City"].forEach(function(h) { html += '<th style="padding:6px 14px;text-align:left;background:var(--bg-sec);border-bottom:2px solid var(--primary);font-weight:600">' + h + '</th>'; });
    html += '</tr></thead><tbody>';
    rows.forEach(function(r) {
      html += '<tr>';
      r.forEach(function(c) { html += '<td style="padding:5px 14px;border-bottom:1px solid var(--border-light)">' + c + '</td>'; });
      html += '</tr>';
    });
    html += '</tbody></table></div>';
    el.innerHTML = html;
  }
},

"propertygrid": {
  title:"PropertyGrid", crumb:"数据 / PropertyGrid",
  desc:"分类 / 字母视图、无限层级、搜索过滤、描述面板、11 种编辑器、属性联动。右侧面板即本控件——下方属性实时改变它自身。",
  feats:["分类/字母","搜索","属性联动","11 种编辑器"],
  props: [
    { id:"pg-colw", label:"NameColumnWidth", type:"slider", min:70, max:160, value:110,
      apply:function(v, root) { root.querySelectorAll("#miniPg .mrow").forEach(function(r) { r.style.gridTemplateColumns = v + "px 1fr"; }); } },
    { id:"pg-rowh", label:"RowHeight", type:"slider", min:18, max:40, value:28,
      apply:function(v, root) { root.querySelectorAll("#miniPg .mrow div").forEach(function(d) { d.style.minHeight = (v - 8) + "px"; }); } }
  ],
  stage:function(el) {
    var html = '<div class="stage" style="display:block;padding:0"><div id="miniPg" style="border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;font-size:12px">';
    html += '<div style="display:grid;grid-template-columns:110px 1fr;background:var(--bg-sec);padding:4px 10px;font-weight:600"><span>Appearance</span></div>';
    html += '<div style="display:grid;grid-template-columns:110px 1fr;min-height:28px;align-items:center"><span style="padding-left:20px;color:var(--text-2)">Name</span><span style="padding:0 10px">Valve-01</span></div>';
    html += '<div style="display:grid;grid-template-columns:110px 1fr;min-height:28px;align-items:center"><span style="padding-left:20px;color:var(--text-2)">Visible</span><span style="padding:0 10px">True</span></div>';
    html += '<div style="display:grid;grid-template-columns:110px 1fr;background:var(--bg-sec);padding:4px 10px;font-weight:600"><span>Layout</span></div>';
    html += '<div style="display:grid;grid-template-columns:110px 1fr;min-height:28px;align-items:center"><span style="padding-left:20px;color:var(--text-2)">X</span><span style="padding:0 10px">120</span></div>';
    html += '</div></div>';
    el.innerHTML = html;
  }
},

"codewfwindow": {
  title:"CodeWFWindow", crumb:"控件 / CodeWFWindow",
  desc:"无边框窗口：8 方向缩放热区、标题栏拖拽 / 双击最大化、全屏按钮可选、内容钻入标题栏、Win11 DWM 原生圆角（旧系统自绘退回）、标题栏三层内容槽。",
  feats:["无边框","8 向缩放","全屏按钮","内容钻入","DWM 圆角"],
  props: [],
  stage:function(el) {
    var wins = [
      { t:"默认窗口", extra:"", body:"标准标题栏 + 内容区域" },
      { t:"全屏按钮", extra:'<span style="color:var(--text-3)">⛶</span>', body:"带全屏切换按钮" },
      { t:"内容钻入标题栏", extra:"", body:"工具栏嵌入标题栏区域" },
      { t:"自定义标题栏", extra:"", body:"LeftContent + CenterContent + RightContent 三层内容槽" }
    ];
    var html = '<div style="display:flex;gap:14px;flex-wrap:wrap">';
    wins.forEach(function(w, i) {
      html += '<div style="border:1px solid var(--border);border-radius:8px;overflow:hidden;width:260px">' +
        '<div style="display:flex;align-items:center;height:30px;background:var(--bg-sec);padding:0 6px 0 10px;gap:4px">' +
        '<span style="color:var(--primary);font-size:11px">◆</span>' +
        '<span style="color:var(--text-2);font-size:11px;flex:1">' + w.t + '</span>' +
        '<span style="color:var(--text-3);font-size:11px">—</span>' +
        '<span style="color:var(--text-3);font-size:11px">□</span>' +
        (i === 1 ? '<span style="color:var(--text-3);font-size:11px">⛶</span>' : '') +
        '<span style="color:var(--text-3);font-size:11px;padding:0 4px">✕</span></div>' +
        '<div style="padding:14px;color:var(--text-2);font-size:11px;min-height:60px">' + w.body + '</div></div>';
    });
    html += '</div>';
    el.innerHTML = html;
  }
},

"searchlistbox": {
  title:"SearchListBox", crumb:"控件 / SearchListBox",
  desc:"搜索框 400ms 防抖过滤 + 计数条（命中/总数）+ 多选 ListBox；双击与选择变化事件可供 Transfer 等场景复用；AddRange / RemoveRange 批量操作自动重筛。",
  feats:["400ms 防抖","计数条","多选","双击事件"],
  props: [
    { id:"slb-q", label:"SearchText", type:"text", value:"",
      apply:function(v, root) {
        root.querySelectorAll(".slb-demo li").forEach(function(li) {
          li.style.display = li.textContent.toLowerCase().includes(String(v).toLowerCase()) ? "" : "none";
        });
        var hits = root.querySelectorAll(".slb-demo li:not([style*='none'])").length;
        var cnt = root.querySelector(".slb-cnt"); if (cnt) cnt.textContent = hits + " / 4";
      } }
  ],
  stage:function(el) {
    var cities = ["Hangzhou","Shanghai","Shaoxing","Beijing"];
    var html = '<div class="stage" style="display:block;padding:0;max-width:320px"><div class="slb slb-demo">' +
      '<input class="sin" placeholder="Search…" />' +
      '<div class="scnt slb-cnt">4 / 4</div><ul>';
    cities.forEach(function(c) { html += '<li>' + c + '</li>'; });
    html += '</ul></div></div>';
    el.innerHTML = html;
  },
  wire:function(root) {
    var input = root.querySelector(".sin");
    if (!input) return;
    input.addEventListener("input", function() {
      var q = input.value.toLowerCase();
      var hits = 0;
      root.querySelectorAll(".slb-demo li").forEach(function(li) {
        var show = li.textContent.toLowerCase().includes(q);
        li.style.display = show ? "" : "none";
        if (show) hits++;
      });
      var cnt = root.querySelector(".slb-cnt");
      if (cnt) cnt.textContent = hits + " / 4";
    });
  }
},

"transfer": {
  title:"Transfer", crumb:"控件 / Transfer",
  desc:"左右两个 SearchListBox + 中间圆形按钮；双击列表项或选中后点按钮移动；GridSplitter 可拖拽调宽；内建搜索过滤在移动时只作用于可见项。",
  feats:["双列穿梭","双击/按钮","搜索过滤","GridSplitter"],
  props: [],
  stage:function(el) {
    var left = ["Hangzhou","Beijing","Suzhou"], right = ["Shanghai"];
    var html = '<div class="stage" style="display:block"><div style="display:grid;grid-template-columns:1fr auto 1fr;gap:10px;align-items:center;max-width:560px">';
    html += '<div style="border:1px solid var(--border);border-radius:var(--radius);overflow:hidden"><div style="padding:5px 10px;background:var(--bg-sec);font-weight:600;font-size:11px">候选</div><ul style="list-style:none">';
    left.forEach(function(c) { html += '<li style="padding:4px 12px">' + c + '</li>'; });
    html += '</ul></div>';
    html += '<div style="display:flex;flex-direction:column;gap:6px">' +
      '<button style="width:30px;height:26px;border-radius:15px;border:1px solid var(--border);background:var(--bg);cursor:pointer;color:var(--primary)">›</button>' +
      '<button style="width:30px;height:26px;border-radius:15px;border:1px solid var(--border);background:var(--bg);cursor:pointer;color:var(--primary)">‹</button></div>';
    html += '<div style="border:1px solid var(--border);border-radius:var(--radius);overflow:hidden"><div style="padding:5px 10px;background:var(--bg-sec);font-weight:600;font-size:11px">已选</div><ul style="list-style:none">';
    right.forEach(function(c) { html += '<li style="padding:4px 12px">' + c + '</li>'; });
    html += '</ul></div></div></div>';
    el.innerHTML = html;
  }
},

"status": {
  title:"状态组件", crumb:"控件 / 状态组件",
  desc:"StatusBadge / StatusCard / StatusLabel：五种语义状态（Debug / Info / Warn / Error / Fatal），AntD 风格配色，深浅主题自动换肤，文本可选中复制。",
  feats:["5 种状态","AntD 配色","Badge/Card/Label"],
  props: [
    { id:"st-kind", label:"Kind", type:"combo", options:["Info","Warn","Faulted"], value:"Warn",
      apply:function(v, root) {
        var map = { Info:{c:"#52c41a",bg:"#f6ffed",ic:"✓"}, Warn:{c:"#fa8c16",bg:"#fff7e6",ic:"⚠"}, Faulted:{c:"#f5222d",bg:"#fff1f0",ic:"✕"} };
        var s = map[v] || map.Warn;
        var badge = root.querySelector(".st-bdg"); if (badge) { badge.querySelector(".l").style.background = s.c; badge.querySelector(".r").style.color = s.c; }
        var card = root.querySelector(".st-crd"); if (card) { card.querySelector(".ic").style.background = s.c; card.querySelector(".bt").style.color = s.c; }
      } }
  ],
  stage:function(el) {
    el.innerHTML =
      '<div class="stage" style="display:flex;flex-direction:column;gap:14px;align-items:flex-start">' +
      '<div><span class="st-bdg" style="display:inline-flex;border-radius:4px;overflow:hidden;font-size:11px"><span class="l" style="padding:2px 8px;color:#fff;background:#fa8c16">警告</span><span class="r" style="padding:2px 10px;color:#fa8c16">12</span></span>' +
      ' <span class="st-bdg" style="display:inline-flex;border-radius:4px;overflow:hidden;font-size:11px"><span class="l" style="padding:2px 8px;color:#fff;background:#52c41a">消息</span><span class="r" style="padding:2px 10px;color:#52c41a">3</span></span></div>' +
      '<div class="st-crd" style="display:flex;gap:10px;border:1px solid var(--border);border-radius:8px;padding:12px 14px;min-width:260px">' +
      '<div class="ic" style="width:36px;height:36px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff;background:#fa8c16">⚠</div>' +
      '<div><div class="tp" style="font-size:11px;color:var(--text-2)">泵站服务</div><div class="bt" style="font-size:15px;font-weight:600;color:#fa8c16">Running</div></div></div>' +
      '</div>';
  }
},

"guide": {
  title:"Guide 新手引导", crumb:"控件 / Guide",
  desc:"聚光遮罩（EvenOdd 挖洞）+ 13 种弹出方位 + 箭头自动对齐 + 键盘导航；StepsSource 绑定、StepOpening 取消、目标缺失三策略（Center / Skip / Close）；DefaultGuideIndicator 圆点 / TextGuideIndicator 文本。",
  feats:["聚光遮罩","13 方位","键盘导航","动态步骤"],
  props: [
    { id:"gd-idx", label:"CurrentIndex", type:"slider", min:1, max:3, value:1,
      apply:function(v, root) {
        var steps = [{t:"搜索",d:"输入关键字可实时过滤列表。"},{t:"导出",d:"支持多格式导出。"},{t:"主题",d:"18 套排版主题。"}];
        var idx = v - 1;
        var card = root.querySelector(".gcard"); if (!card) return;
        card.querySelector(".stp").textContent = "步骤 " + v + " / 3";
        card.querySelector(".gtt").textContent = steps[idx].t;
        card.querySelector(".gds").textContent = steps[idx].d;
        root.querySelector(".gbtns .pri").textContent = v >= 3 ? "完成" : "下一步";
        card.style.left = (10 + idx * 30) + "%";
        card.style.top = (15 + (idx % 2) * 40) + "%";
      } }
  ],
  stage:function(el) {
    el.innerHTML =
      '<div class="gd-ov" style="position:relative;min-height:220px;background:var(--bg-sec);border-radius:var(--radius);overflow:hidden">' +
      '<div class="und"><h3>演示页面</h3><p style="margin-top:8px">这里是被引导的真实页面内容，聚光遮罩在目标控件周围挖出高亮区。</p>' +
      '<p style="margin-top:20px">目标 A：搜索框<br>目标 B：导出按钮</p></div>' +
      '<div class="mask" style="position:absolute;inset:0;background:rgba(0,0,0,.4)"></div>' +
      '<div class="card" style="left:30%;top:20%">' +
      '<div class="stp">步骤 1 / 3</div><div class="gtt">搜索</div>' +
      '<div class="gds">输入关键字可实时过滤列表。</div>' +
      '<div class="gbt"><button class="mock-btn" style="height:22px;padding:0 10px;font-size:11px">上一步</button>' +
      '<button class="mock-btn pri" style="height:22px;padding:0 10px;font-size:11px">下一步</button></div></div></div>';
  }
},

"windowedstack": {
  title:"WindowedStackPanel", crumb:"控件 / WindowedStackPanel",
  desc:"索引管理的窗口化虚拟化面板：只挂载「视口 ± OverscanViewports 屏」内的子项，其余按估算尺寸占位；pinned / pinnedByKind 强制物化；MarkdownViewer 内部即用它。",
  feats:["窗口化","pinnedByKind","索引插删","Overscan"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="stage"><p style="line-height:1.8">' +
      '<b>WindowedStackPanel</b>：与 VirtualizingStackPanel 的区别是索引由调用方维护（InsertItem / RemoveAt 不会错位）。' +
      '支持 pinned（强制物化锚点项）与 pinnedByKind（按类型固定物化，如代码块/表格防抖动）。' +
      '关闭 EnableVirtualization 或子项数低于阈值(40)时退化为普通面板。</p>' +
      '<div style="display:flex;gap:16px;margin-top:10px;font-size:11px;color:var(--text-2)">' +
      '<span>ItemCount: <b>5000</b></span><span>RealizedItemCount: <b>~28</b></span><span>IsVirtualizing: <b>true</b></span></div></div>';
  }
},

"columndisplay": {
  title:"ColumnDisplayEditor", crumb:"控件 / ColumnDisplayEditor",
  desc:"表格「列设置」面板：拖拽手柄上下拖动排序（直接操作 IList），Visible 复选框双向绑定列可见性；拖动过程显示插入位置预览线。",
  feats:["拖拽排序","Visible 联动","插入预览线"],
  props: [],
  stage:function(el) {
    var cols = ["Name","Age","City","Email"];
    var html = '<div class="stage" style="display:flex;gap:16px">' +
      '<div style="border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;min-width:180px">';
    html += '<div style="display:grid;grid-template-columns:20px 1fr 40px;padding:4px 8px;background:var(--bg-sec);font-size:10px;color:var(--text-3)"><span>☰</span><span>列名</span><span>显示</span></div>';
    cols.forEach(function(c, i) {
      html += '<div style="display:grid;grid-template-columns:20px 1fr 40px;padding:5px 8px;border-top:1px solid var(--border-light);align-items:center;cursor:grab">' +
        '<span style="color:var(--text-3)">⋮⋮</span><span>' + c + '</span>' +
        '<span><input type="checkbox" checked style="accent-color:var(--primary)"></span></div>';
    });
    html += '</div><div style="align-self:center;color:var(--text-3);font-size:11px">←→<br>联动 DataGrid</div></div>';
    el.innerHTML = html;
  }
},

"trapezoid": {
  title:"梯形页签", crumb:"控件 / 梯形页签",
  desc:"梯形带斜边页签边框（TrapezoidShapedTabItemBorder）：首/中/末三种路径自适应，斜边圆角比例 DiagonalFilletRatio=0.8，随 TabItem.CornerRadius 缩放。配套 TabControl / TabStrip 主题。",
  feats:["梯形路径","首/中/末自适应","圆角缩放"],
  props: [],
  stage:function(el) {
    var tabs = ["文档", "设计", "预览"];
    var html = '<div class="stage" style="display:block"><div style="display:flex;border-bottom:2px solid var(--border)">';
    tabs.forEach(function(t, i) {
      var on = i === 0;
      html += '<div style="padding:6px 22px;cursor:pointer;font-size:12px;' +
        (on ? 'color:var(--primary);font-weight:600;border-bottom:2px solid var(--primary);margin-bottom:-2px' : 'color:var(--text-2)') + '">' + t + '</div>';
    });
    html += '</div><div style="padding:14px;color:var(--text-2);font-size:11px">选中页签内容区…</div></div></div>';
    el.innerHTML = html;
  }
},

"markup": {
  title:"Markup 扩展", crumb:"控件 / Markup 扩展",
  desc:"{If} 条件三元（三槽均可 Binding）、{Switch} XAML switch-case（宽松匹配：Equals / 枚举名互匹 / IConvertible）、RangeObservableCollection（批量操作只发一次通知）。",
  feats:["{If} 三元","{Switch} case","RangeObservableCollection"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="stage" style="display:flex;flex-direction:column;gap:10px">' +
      '<div><b>{If}</b><br><code style="font-size:11px;color:var(--text-2)">Text="{codewf:If Condition={Binding IsVip}, True=\'尊享版\', False=\'普通版\'}"</code></div>' +
      '<div><b>{Switch}</b><br><code style="font-size:11px;color:var(--text-2)">Background="{codewf:Switch Value={Binding Kind}, Cases={codewf:SwitchCase When=Debug, Then=Green}, Default=Gray}"</code></div>' +
      '<div><b>RangeObservableCollection</b><br><span style="font-size:11px;color:var(--text-2)">AddRange / RemoveRange / Sort — 批量操作只发一次 Reset</span></div></div>';
  }
},

"log": {
  title:"Log 日志", crumb:"基础设施 / Log",
  desc:"Logger 文件日志（级别 / 滚动 / 保留策略）+ LogView 应用内查看器（级别过滤 / 节流刷新 / 右键复制 / 打开目录）+ InApp / DesktopWindow 通知（ShakeAndPulse 动效）。底部日志面板即实时事件流。",
  feats:["文件日志","LogView","级别过滤","桌面通知"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="stage"><p style="line-height:1.8">点击按钮写入不同级别日志，观察底部日志面板与右键菜单（复制 / 清空 / 打开目录）。</p>' +
      '<div style="display:flex;gap:8px;flex-wrap:wrap;margin-top:10px">' +
      '<button class="mock-btn" data-l="Info">Info</button>' +
      '<button class="mock-btn" data-l="Warn">Warn</button>' +
      '<button class="mock-btn" data-l="Error">Error</button>' +
      '<button class="mock-btn" data-l="Fatal">Fatal</button></div></div>';
  },
  wire:function(root) {
    root.querySelectorAll("button[data-l]").forEach(function(b) {
      b.addEventListener("click", function() { dlog("[" + b.dataset.l + "] 演示：一条" + b.dataset.l + "日志", b.dataset.l.toLowerCase()); });
    });
  }
},

"lang": {
  title:"Lang 多语言", crumb:"基础设施 / Lang",
  desc:"插件化多语言：Json / Xml / Resx 三种后端，运行时切换（I18nManager.Culture），强类型 Key 源生成器，父文化回退。本演示台界面文案即由 Lang 驱动。",
  feats:["Json/Xml/Resx","运行时切换","强类型 Key","父文化回退"],
  props: [],
  stage:function(el) {
    el.innerHTML = '<div class="stage"><p style="line-height:1.8">后端切换：<button class="mock-btn" style="margin-right:6px">Json</button><button class="mock-btn" style="margin-right:6px">Xml</button><button class="mock-btn">Resx</button>' +
      '<br><br>当前文化：<b>zh-CN</b>（状态栏「文 A」可切换）<br>强类型 Key：<code style="font-size:11px">PropertiesLanguage.SearchPlaceholder</code></p></div>';
  }
}
};

/* ═══ ShowMeCode 辅助 ═══ */
function showme(pageId) {
  var p = PAGES[pageId];
  var el = $id("dBody");
  el.innerHTML = '<div class="pg-body">' +
    '<div class="crumb">' + p.crumb + '</div>' +
    '<h1>' + p.title + '</h1>' +
    '<div class="feat-tags">' + (p.feats || []).map(function(f) { return '<span>' + f + '</span>'; }).join("") + '</div>' +
    '<p class="dsc">' + p.desc + '</p>' +
    '<div class="demo-card"><div class="dc-h"><span>▌ ' + t("panel.demo") + '</span><span class="sp"></span>' +
    '<span class="smc-tgl">' +
    '<button class="on" data-smc="demo" style="border:none;background:var(--primary);color:#fff;padding:2px 10px;cursor:pointer;font-size:11px;border-radius:3px 0 0 3px">演示</button>' +
    '<button data-smc="xaml" style="border:none;background:var(--bg-sec);color:var(--text-2);padding:2px 10px;cursor:pointer;font-size:11px;border-left:1px solid var(--border)">XAML</button>' +
    '<button data-smc="cs" style="border:none;background:var(--bg-sec);color:var(--text-2);padding:2px 10px;cursor:pointer;font-size:11px;border-left:1px solid var(--border)">C#</button>' +
    '</span></div>' +
    '<div class="dc-b"><div class="smc-demo" id="smcDemo"></div>' +
    '<div class="smc-code" id="smcXaml" style="display:none"><div class="code-blk">' + esc2(p.xaml || "") + '</div></div>' +
    '<div class="smc-code" id="smcCs" style="display:none"><div class="code-blk">' + esc2(p.cs || "") + '</div></div>' +
    '</div></div>' +
    '<div class="demo-card"><div class="dc-h">▌ ' + t("panel.opts") + '</div><div class="dc-b"><div class="pgp" id="pgp"></div></div></div>' +
    '</div>';
  if (p.stage) p.stage($id("smcDemo"));
  renderPgp(p.props || []);
}
function esc2(s) { return s.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }
function renderPgp(props) {
  var pgp = $id("pgp");
  if (!props.length) { pgp.innerHTML = '<div class="pgp-empty">当前演示无可调属性。</div>'; return; }
  var html = "";
  props.forEach(function(p) {
    if (p.grp) { html += '<div class="sbtn">▌ ' + p.grp + '</div>'; return; }
    var ctrl = "";
    if (p.type === "combo") ctrl = "<select>" + p.options.map(function(o) { return "<option" + (o === p.value ? " selected" : "") + ">" + o + "</option>"; }).join("") + "</select>";
    else if (p.type === "bool") ctrl = '<input type="checkbox"' + (p.value ? " checked" : "") + ">";
    else if (p.type === "slider") ctrl = '<input type="range" min="' + p.min + '" max="' + p.max + '" value="' + p.value + '"><span class="rv">' + p.value + '</span>';
    else if (p.type === "chips") ctrl = p.options.map(function(o) { return '<span class="chip on" data-n="' + o.n + '" data-c="' + o.c + '" style="background:' + o.c + '"></span>'; }).join("");
    else if (p.type === "text") ctrl = '<input type="text" value="' + p.value + '">';
    html += '<div class="prow2"><label>' + p.label + '</label><div class="ctl">' + ctrl + "</div></div>";
  });
  pgp.innerHTML = html;
  pgp.querySelectorAll("select").forEach(function(sel, i) {
    sel.addEventListener("change", function() { props[i].apply(sel.value, $id("smcDemo")); });
  });
  pgp.querySelectorAll("input[type=checkbox]").forEach(function(cb, i) {
    cb.addEventListener("change", function() { props[i].apply(cb.checked, $id("smcDemo")); });
  });
  pgp.querySelectorAll("input[type=range]").forEach(function(rg, i) {
    rg.addEventListener("input", function() { props[i].apply(parseFloat(rg.value), $id("smcDemo")); });
  });
  pgp.querySelectorAll("input[type=text]").forEach(function(tx, i) {
    tx.addEventListener("change", function() { props[i].apply(tx.value, $id("smcDemo")); });
  });
}
function esc2(s) { return s.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }
