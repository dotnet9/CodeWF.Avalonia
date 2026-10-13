/* ═══════════════════════════════════════════════════════════════════════════
   CodeWF.Avalonia.Demo 原型 · 交互脚本
   信息架构：场景优先（3 个完整工作流）+ 组件库分区（3 个页签）
   外壳：Ribbon（7 页签 / 双击折叠）+ Dock 工作区 + 属性面板 + 日志面板 + 状态栏
   ═══════════════════════════════════════════════════════════════════════════ */
"use strict";

var $ = function (id) { return document.getElementById(id); };
var NL = String.fromCharCode(10);
function esc(s) { return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"); }
function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }
function guard(fn, tag) { try { fn(); } catch (e) { dlog("JS 异常[" + tag + "]：" + e.message, "error"); } }

/* ═══ 双语文案表（所有界面文案都在这里，zh-CN / en-US 两份） ═══ */
var LANG = "zh-CN";
var I18N = {
 "zh-CN": {
  "tabs.home": "首页", "tabs.write": "Markdown 工作区", "tabs.data": "数据工作台", "tabs.docs": "文档工作区",
  "tabs.input": "组件库·输入", "tabs.present": "组件库·呈现", "tabs.infra": "组件库·基础设施",
  "nav": "导航树", "props": "属性", "log": "日志", "ready": "就绪",
  "dock.nav": "导航树", "dock.props": "属性", "dock.log": "日志", "dock.max": "最大化", "dock.reset": "重置",
  "dock.hide": "隐藏", "qk.save": "保存",
  "log.opendir": "打开日志目录", "log.clear": "清空", "status.app": "演示台", "status.logpanel": "日志面板",
  "st.where": "未打开文档", "st.docs": " 篇文档", "st.level": "日志级别 ", "st.ready": "就绪", "st.sync": "最近同步 ",
  "tree.filter": "过滤页面…", "tree.empty": "无匹配页面", "tree.rename": "双击或点 ✎ 重命名", "tree.reset": "恢复原名",
  "nc.ok": "成功", "nc.warn": "警告", "nc.err": "错误", "nc.info": "提示",
  "cmd.ph": "搜索页面或命令（Esc 关闭，↑↓ 选择，Enter 执行）",
  "cmd.pages": "页面", "cmd.cmds": "命令",
  "dlg.cancel": "取消", "dlg.yes": "确定",
  "dlg.del.t": "删除确认", "dlg.del.d": "此操作不可撤销，确定要执行吗？",
  "dlg.reset.t": "重置布局确认", "dlg.reset.d": "将恢复面板显隐、色板、深浅与密度为默认值。",
  "dlg.leave.t": "有未保存的更改", "dlg.leave.d": "当前文档被修改过，关闭后将丢失改动。确定关闭吗？",
  "empty.title": "还没有打开文档", "empty.desc": "从顶部 Ribbon 页签或左侧导航树选择一个页面开始。", "empty.action": "打开欢迎页",
  "busy.export": "正在导出，可取消…", "busy.cancel": "取消", "busy.done": "完成",
  "err.title": "操作失败", "err.retry": "重试",
  "toast.saved": "布局已保存到浏览器本地", "toast.reset": "布局已重置为默认值",
  "toast.density": "密度：", "toast.palette": "色板：", "toast.theme": "主题：", "toast.lang": "语言：",
  "toast.copied": "已复制到剪贴板", "toast.exported": "导出完成",
  "density.cozy": "舒适", "density.compact": "紧凑", "theme.light": "浅色", "theme.dark": "深色",
  "g.prev": "上一步", "g.next": "下一步", "g.skip": "跳过", "g.close": "关闭",
  "g.1.t": "欢迎使用演示台", "g.1.d": "顶部 Ribbon 是 7 个页签：首页 + 3 个场景工作流 + 3 个组件库分区；双击页签可折叠命令区。",
  "g.2.t": "导航树", "g.2.d": "与 Ribbon 同源；顶部可过滤页面，双击条目可就地重命名（原型内只改显示名）。",
  "g.3.t": "文档区与空态", "g.3.d": "所有演示以文档页签承载：无文档时是空态，打开瞬间显示骨架屏。",
  "g.4.t": "属性面板", "g.4.d": "当前文档的可调属性；被修改过的项左侧会出现色条并加粗。",
  "g.5.t": "日志面板与状态栏", "g.5.d": "底部是 LogView 形态的实时日志；状态栏显示当前位置、文档数与日志级别。"
 },
 "en-US": {
  "tabs.home": "Home", "tabs.write": "Markdown", "tabs.data": "Data", "tabs.docs": "Workspace",
  "tabs.input": "Lib · Input", "tabs.present": "Lib · Present", "tabs.infra": "Lib · Infra",
  "nav": "Explorer", "props": "Properties", "log": "Log", "ready": "Ready",
  "dock.nav": "Explorer", "dock.props": "Properties", "dock.log": "Log", "dock.max": "Maximize", "dock.reset": "Reset",
  "dock.hide": "Hide", "qk.save": "Save",
  "log.opendir": "Open Log Dir", "log.clear": "Clear", "status.app": "Demo Shell", "status.logpanel": "Log Panel",
  "st.where": "No document", "st.docs": " docs", "st.level": "Log level ", "st.ready": "Ready", "st.sync": "Synced ",
  "tree.filter": "Filter pages…", "tree.empty": "No matching page", "tree.rename": "Double-click or ✎ to rename", "tree.reset": "Restore names",
  "nc.ok": "Success", "nc.warn": "Warning", "nc.err": "Error", "nc.info": "Info",
  "cmd.ph": "Search pages or commands (Esc close, arrows move, Enter run)",
  "cmd.pages": "Pages", "cmd.cmds": "Commands",
  "dlg.cancel": "Cancel", "dlg.yes": "OK",
  "dlg.del.t": "Confirm", "dlg.del.d": "This cannot be undone. Continue?",
  "dlg.reset.t": "Confirm layout reset", "dlg.reset.d": "Panel visibility, palette, theme and density go back to defaults.",
  "dlg.leave.t": "Unsaved changes", "dlg.leave.d": "This document was edited. Closing discards the changes. Continue?",
  "empty.title": "No document open", "empty.desc": "Pick a page from the Ribbon tabs or the explorer on the left.", "empty.action": "Open welcome",
  "busy.export": "Exporting, cancellable…", "busy.cancel": "Cancel", "busy.done": "Done",
  "err.title": "Operation failed", "err.retry": "Retry",
  "toast.saved": "Layout saved to browser storage", "toast.reset": "Layout reset to defaults",
  "toast.density": "Density: ", "toast.palette": "Palette: ", "toast.theme": "Theme: ", "toast.lang": "Language: ",
  "toast.copied": "Copied to clipboard", "toast.exported": "Export finished",
  "density.cozy": "Cozy", "density.compact": "Compact", "theme.light": "Light", "theme.dark": "Dark",
  "g.prev": "Back", "g.next": "Next", "g.skip": "Skip", "g.close": "Close",
  "g.1.t": "Welcome", "g.1.d": "Seven Ribbon tabs: home + three scenario workflows + three library sections. Double-click a tab to collapse the command area.",
  "g.2.t": "Explorer", "g.2.d": "Same source as the Ribbon. Filter on top; double-click an entry to rename it in place.",
  "g.3.t": "Documents & empty state", "g.3.d": "Every demo is a document tab: an empty state when none is open, a skeleton while the first one loads.",
  "g.4.t": "Properties", "g.4.d": "Editable properties of the current document; changed rows get an accent bar and a bold label.",
  "g.5.t": "Log panel & status bar", "g.5.d": "Bottom is the live log (LogView shape); the status bar shows location, document count and log level."
 }
};
function tt(k) { return (I18N[LANG] && I18N[LANG][k]) || (I18N["zh-CN"] && I18N["zh-CN"][k]) || k; }
/* 供内联 onclick 使用的公共入口（原型无打包器，统一挂在 window 上） */
function expose() {
 ["closeAllDocsAsk", "layoutResetAsk", "clearLogAsk", "saveLayout", "togglePanel", "setLang", "setPalette", "setTheme",
  "setDensity", "toggleDark", "guideOpen", "guideGo", "gCloseGuide", "openDoc", "closeDoc", "markDirty", "clearRenames",
  "notify", "confirmDlg", "closeDlg", "cpmOpen", "cpmClose", "toggleRibbon", "startRename", "dlog", "log2both",
  "renderTree", "renderRBody", "renderDoc", "renderPGrid", "setBudgetNote"].forEach(function (n) {
   try { window[n] = eval(n); } catch (e) { }
  });
}

/* ═══ 日志面板 / 状态栏 / 通知中心 / 对话框 ═══ */
var LOG_LEVEL = "Info";
function dlog(msg, lv) {
 var b = $("logP"); if (!b) return;
 var d = new Date(), tm = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2);
 var cls = lv === "info" ? "ll1" : lv === "warn" ? "ll2" : lv === "error" ? "ll3" : "";
 var e = document.createElement("div"); e.className = cls;
 e.innerHTML = '<span class="ltm">' + tm + "</span>" + msg;
 b.appendChild(e); b.scrollTop = b.scrollHeight;
 if (lv === "info" || lv === "warn" || lv === "error") LOG_LEVEL = lv === "info" ? "Info" : lv === "warn" ? "Warn" : "Error";
 var st = $("stLogLv"); if (st) st.textContent = LOG_LEVEL;
}
function p_append(b, e) { b.appendChild(e); b.scrollTop = b.scrollHeight; }
function setStatus(msg) {
 var el = $("stTx"); if (el) el.textContent = msg;
 var f = $("stFlush");
 if (f) { var d = new Date(); f.textContent = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2); }
}
function log2both(msg, lv) {
 dlog(msg, lv);
 var s = $("lgStream");
 if (s) {
  var e = document.createElement("div");
  e.className = lv === "warn" ? "w" : lv === "error" ? "e" : lv === "debug" ? "d" : "";
  e.innerHTML = "<span style='color:var(--tx3);margin-right:10px'>" + new Date().toLocaleTimeString() + "</span>" + msg;
  p_append(s, e);
 }
 setStatus(msg);
}
function notify(kind, title, body) {
 var box = $("nc"); if (!box) return;
 var k = kind === "ok" ? "s" : kind === "warn" ? "w" : kind === "err" ? "e" : "i";
 var card = document.createElement("div");
 card.className = "ncard " + k;
 card.innerHTML = '<span>●</span><div style="flex:1"><div class="nt">' + esc(title) + '</div><div class="nb">' + esc(body || "") + "</div></div>" +
  '<button class="nx" title="' + esc(tt("g.close")) + '">✕</button>';
 card.querySelector(".nx").onclick = function () { if (card.parentNode === box) box.removeChild(card); };
 box.appendChild(card);
 while (box.children.length > 4) box.removeChild(box.children[0]);
 setTimeout(function () { if (card.parentNode === box) box.removeChild(card); }, 6000);
}
var _dlgYes = null;
function confirmDlg(title, body, onYes, danger) {
 var m = $("dlgm"); if (!m) { onYes(); return; }
 $("dlgT").textContent = title; $("dlgD").textContent = body;
 $("dlgYes").textContent = tt("dlg.yes"); $("dlgNo").textContent = tt("dlg.cancel");
 $("dlgYes").className = "btn " + (danger === false ? "pri" : "danger");
 _dlgYes = onYes; m.classList.add("on"); $("dlgYes").focus();
}
function closeDlg() { $("dlgm").classList.remove("on"); _dlgYes = null; }

/* ═══ Markdown 排版主题（与 MarkdownTypographyThemes 的 18 套一致） ═══ */
var MD_THEMES = ["Basic", "OrangeHeart", "InkBlack", "ColorfulPurple", "TenderGreen", "Verdant", "RedScarlet",
 "BlueGlow", "TechnologyBlue", "LanQing", "Yamabuki", "FrontendPeak", "GeekBlack", "Simple", "RosePurple",
 "CuteGreen", "FullStackBlue", "WeChatFormat"];
/* 字号由排版主题决定（验证「外观字号令牌被排版主题接管」） */
var MD_THEME_FS = { Basic: "13px", GeekBlack: "12.5px", TechnologyBlue: "13.5px", WeChatFormat: "15px" };

/* ═══ 演示目录：7 个 Ribbon 页签 / 24 页（3 个场景页 + 21 个组件页） ═══ */
var CATALOG = [
 { id: "home", icon: "🏠", items: [{ id: "welcome" }] },
 { id: "write", icon: "📝", items: [{ id: "flow-write" }, { id: "md-viewer" }, { id: "md-lite" }, { id: "md-editor" }, { id: "md-live" }, { id: "md-export" }] },
 { id: "data", icon: "📊", items: [{ id: "flow-data" }, { id: "datagrid" }, { id: "propgrid" }] },
 { id: "docs", icon: "🪟", items: [{ id: "flow-docs" }, { id: "dock" }, { id: "trapezoid" }] },
 { id: "input", icon: "🔎", items: [{ id: "searchlist" }, { id: "transfer" }, { id: "markup" }] },
 { id: "present", icon: "🏷️", items: [{ id: "status" }, { id: "guide" }, { id: "appguide" }, { id: "wsp" }, { id: "colde" }] },
 { id: "infra", icon: "🧱", items: [{ id: "log" }, { id: "lang" }, { id: "codewfwindow" }] }
];
/* 页面短名：中文 ≤ 4 字，保证 Ribbon 按钮与树条目都放得下 */
var PAGE_NAME = {
 "welcome": "欢迎", "flow-write": "写作流程", "flow-data": "数据流程", "flow-docs": "文档流程",
 "md-viewer": "阅读器", "md-lite": "轻量渲染", "md-editor": "源码编辑", "md-live": "实时编辑", "md-export": "导出复制",
 "datagrid": "数据表", "propgrid": "属性表", "dock": "停靠布局", "trapezoid": "梯形页签",
 "searchlist": "搜索列表", "transfer": "穿梭框", "markup": "标记扩展",
 "status": "状态组件", "guide": "页面引导", "appguide": "应用引导", "wsp": "虚拟面板", "colde": "列配置",
 "log": "日志控件", "lang": "国际化", "codewfwindow": "无边框窗口"
};
/* 页面原名（英文 API / 控件名）：页面正文的小字与命令面板右侧备注用 */
var PAGE_ALIAS = {
 "welcome": "Welcome", "flow-write": "Markdown 工作区", "flow-data": "数据工作台", "flow-docs": "文档工作区",
 "md-viewer": "MarkdownViewer", "md-lite": "Markdown Lite", "md-editor": "MarkdownEditorView", "md-live": "MarkdownLiveEditorView", "md-export": "MarkdownDocumentExporter",
 "datagrid": "DataGrid", "propgrid": "PropertyGrid", "dock": "DockControl", "trapezoid": "TrapezoidShapedTabControl",
 "searchlist": "SearchListBox", "transfer": "Transfer", "markup": "If / Switch",
 "status": "StatusBadge / StatusCard", "guide": "Guide", "appguide": "Guide（应用级）", "wsp": "WindowedStackPanel", "colde": "ColumnDisplayEditor",
 "log": "LogView", "lang": "I18n", "codewfwindow": "CodeWFWindow"
};
/* 图标：统一用 24 网格线性图标（图标思路对齐 IconPark / TDesign，实现期可直接映射到 Semi / Ursa / Iconica） */
function svg(view, extra) {
 return '<svg class="svgic ' + (extra || "s20") + '" viewBox="0 0 24 24" aria-hidden="true">' + view + "</svg>";
}
var TAB_ICON = {
 "welcome": '<path d="M4.5 10.8L12 4l7.5 6.8"/><path d="M6.5 10v9.5h11V10"/><path d="M10.5 19.5v-4.8h3v4.8"/>',
 "flow-write": '<circle cx="12" cy="12" r="8.5"/><path d="M14.6 9.4l-1.7 3.8-3.8 1.7 1.7-3.8z"/>',
 "flow-data": '<path d="M4 19.5V4.5"/><path d="M4 19.5h15.5"/><rect x="7" y="11" width="3" height="6"/><rect x="12" y="7.5" width="3" height="9.5"/><rect x="17" y="14" width="3" height="3"/>',
 "flow-docs": '<rect x="3.5" y="7" width="10" height="12.5" rx="1.6"/><path d="M7.5 7V4.5h11a1.5 1.5 0 0 1 1.5 1.5v11a1.5 1.5 0 0 1-1.5 1.5H17"/>',
 "md-viewer": '<path d="M13.5 3.5H7a1.6 1.6 0 0 0-1.6 1.6v13.8A1.6 1.6 0 0 0 7 20.5h10a1.6 1.6 0 0 0 1.6-1.6V8.6z"/><path d="M13.5 3.5v5.1h5.1"/><path d="M9 13.5h6M9 16.8h4"/>',
 "md-lite": '<path d="M4.5 5.5h15v13h-15z"/><path d="M7.5 15.5l2-6 2.5 4.5 1.8-3 1.7 4.5"/>',
 "md-editor": '<path d="M4.5 19.5l.9-3.6L16 5.3l2.7 2.7L8.1 18.6z"/><path d="M14.6 6.7l2.7 2.7"/><path d="M4.5 12.5V4.5h8"/>',
 "md-live": '<path d="M13 3.5L5.5 13.5H11l-.6 7L18 10.5h-5.4z"/>',
 "md-export": '<path d="M12 3.5v10.5"/><path d="M8.5 7L12 3.5 15.5 7"/><path d="M5 13.5v5.5a1.5 1.5 0 0 0 1.5 1.5h11a1.5 1.5 0 0 0 1.5-1.5v-5.5"/>',
 "datagrid": '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><path d="M3.5 9.5h17M3.5 14.5h17M9.5 9.5v10M15 9.5v10"/>',
 "propgrid": '<path d="M4 6.5h9M17.5 6.5H20"/><circle cx="15" cy="6.5" r="2.1"/><path d="M4 17.5h3.5M11 17.5h9"/><circle cx="9" cy="17.5" r="2.1"/><path d="M4 12h6M13.5 12H20"/><circle cx="11.5" cy="12" r="2.1"/>',
 "dock": '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><path d="M10 4.5v15"/><path d="M13.5 9.5h4"/>',
 "trapezoid": '<path d="M7.5 6.5h9l3 11h-15z"/><path d="M9.5 10h5"/>',
 "searchlist": '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><path d="M6.5 9.5h11M6.5 13.5h7"/><circle cx="16.5" cy="15.8" r="2.3"/><path d="M18.3 17.6l2.2 2.2"/>',
 "transfer": '<path d="M4 6.5h10l-2.5-2.5M20 17.5H10l2.5 2.5"/><path d="M17.5 6.5H20"/><path d="M4 17.5h2.5"/>',
 "markup": '<path d="M9 7.5L4.5 12 9 16.5"/><path d="M15 7.5L19.5 12 15 16.5"/><path d="M13.2 5.5l-2.4 13"/>',
 "status": '<path d="M4.5 5.5h15v10l-3.5 3.5h-11z"/><path d="M15.6 19.2v-3.7h3.7"/><path d="M8 10h8M8 13h5"/>',
 "guide": '<circle cx="12" cy="12" r="8.5"/><path d="M14.8 9.2l-1.9 4.1-4.1 1.9 1.9-4.1z"/>',
 "appguide": '<circle cx="9" cy="9" r="5"/><path d="M12.8 12.8L20 20"/><path d="M9 6.5a2.5 2.5 0 0 0-2.5 2.5"/>',
 "wsp": '<path d="M12 3.5L21 8.5 12 13.5 3 8.5z"/><path d="M3 12.8L12 17.8l9-5"/><path d="M3 16.8L12 21.8l9-5"/>',
 "colde": '<path d="M4 7.5h4M4 12h4M4 16.5h4"/><rect x="10" y="5.5" width="10" height="4" rx="1.2"/><rect x="10" y="10" width="10" height="4" rx="1.2"/><rect x="10" y="14.5" width="10" height="4" rx="1.2"/>',
 "log": '<path d="M5.5 4.5h13v15h-13z"/><path d="M8.5 9h7M8.5 12.5h7M8.5 16h4"/>',
 "lang": '<circle cx="12" cy="12" r="8.5"/><path d="M3.5 12h17"/><path d="M12 3.5c2.4 2.4 3.6 5.3 3.6 8.5S14.4 18.1 12 20.5C9.6 18.1 8.4 15.2 8.4 12S9.6 5.9 12 3.5z"/>',
 "codewfwindow": '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><path d="M3.5 9h17"/><path d="M6.5 6.7h.01M9 6.7h.01"/>'
};
function iconSvg(id, extra) { return svg(TAB_ICON[id] || TAB_ICON["welcome"], extra || "s20"); }
/* 页签图标（Ribbon 页签行的极简小图标） */
var TAB_TAB_ICON = {
 home: '<path d="M4.5 10.8L12 4l7.5 6.8"/><path d="M6.5 10v9.5h11V10"/>',
 write: '<path d="M4.5 19.5l.9-3.6L16 5.3l2.7 2.7L8.1 18.6z"/>',
 data: '<path d="M4 19.5V4.5M4 19.5h15.5"/><rect x="7" y="11" width="3" height="6"/><rect x="12" y="7.5" width="3" height="9.5"/><rect x="17" y="14" width="3" height="3"/>',
 docs: '<rect x="3.5" y="7" width="10" height="12.5" rx="1.6"/><path d="M7.5 7V4.5h11a1.5 1.5 0 0 1 1.5 1.5v11a1.5 1.5 0 0 1-1.5 1.5H17"/>',
 input: '<rect x="3.5" y="4.5" width="17" height="15" rx="1.7"/><path d="M6.5 9.5h11M6.5 13.5h7"/><circle cx="16.5" cy="15.8" r="2.3"/><path d="M18.3 17.6l2.2 2.2"/>',
 present: '<path d="M4.5 5.5h15v10l-3.5 3.5h-11z"/><path d="M8 10h8M8 13h5"/>',
 infra: '<path d="M5.5 4.5h13v15h-13z"/><path d="M8.5 9h7M8.5 12.5h7M8.5 16h4"/>'
};
function tabIconSvg(id) { return svg(TAB_TAB_ICON[id] || TAB_TAB_ICON["home"], "s14"); }
function tabName(id) { return tt("tabs." + id); }
function pt(id) { return PAGE_NAME[id] || id; }
function pa(id) { return PAGE_ALIAS[id] || ""; }
/* 页面索引表（欢迎页用）：把每个页签下的页面按短名 / 原名列出，保证「功能没被藏起来」 */
function pageIndexHtml() {
 return CATALOG.map(function (g) {
  return '<tr><td style="white-space:nowrap"><span style="color:var(--pri);display:inline-flex;vertical-align:-3px;margin-right:5px">' +
   tabIconSvg(g.id) + "</span>" + esc(tabName(g.id)) + "</td><td>" +
   g.items.map(function (it) { return esc(pt(it.id)) + (pa(it.id) ? ' <span style="color:var(--tx3)">(' + esc(pa(it.id)) + ")</span>" : ""); }).join(" · ") +
   "</td></tr>";
 }).join("");
}
function groupOf(id) {
 for (var i = 0; i < CATALOG.length; i++) {
  if (CATALOG[i].items.some(function (x) { return x.id === id; })) return CATALOG[i];
 }
 return null;
}
function allPageIds() {
 var n = [];
 CATALOG.forEach(function (g) { g.items.forEach(function (it) { n.push(it.id); }); });
 return n;
}
function docCrumb(id) { var g = groupOf(id); return g ? tabName(g.id) + " / " + pt(id) : pt(id); }

/* ═══ 页面注册表 ═══ */
var PROTO = {};
function REG(id, o) { PROTO[id] = o; }
function renderInto(id, el) {
 var pg = PROTO[id]; if (!pg || !pg.render || !el) return;
 el.replaceChildren();
 var out = pg.render(el);
 if (typeof out === "string") el.innerHTML = out;
}

/* ═══════════════════════ 页面：欢迎（入口） ═══════════════════════ */
REG("welcome", {
 zh: {
  t: "欢迎总览",
  d: "本演示台自身即自研控件的综合实例：外壳是 CodeWFWindow、页签是梯形 TabStrip、右栏是 PropertyGrid、底栏是 LogView、文案走 Lang、内容区是 Dock 工作区。",
  f: ["24 页", "3 个场景工作流", "6 色板", "中英双语"]
 },
 render: function () {
  return '<div class="demo"><h4>快速开始</h4>' +
   '<div class="smcode">dotnet add package CodeWF.Avalonia.Controls' + NL + 'dotnet add package CodeWF.Avalonia.DataGrid' + NL + NL +
   'dotnet run --project demo/CodeWF.Avalonia.Demo' + NL + 'dotnet run --project demo/CodeWF.Avalonia.Demo -- --smoke</div>' +
   '<div class="hint">主题入口：<code>semi:SemiTheme</code> + <code>codewf:ACSemiTheme</code> / <code>codewf:DataGridSemiTheme</code>，其余按包引入。</div></div>' +
   '<div class="demo"><h4>能力矩阵</h4><table class="dg" style="cursor:default"><thead><tr><th>场景</th><th>包</th><th>对应页签</th></tr></thead><tbody>' +
   '<tr><td>桌面外壳 / 引导 / 状态</td><td>Controls</td><td>组件库·输入 / 呈现 / 基础设施</td></tr>' +
   '<tr><td>数据表增强</td><td>DataGrid</td><td>数据工作台</td></tr>' +
   '<tr><td>多文档工作区</td><td>Dock</td><td>文档工作区</td></tr>' +
   '<tr><td>Markdown 写作与导出</td><td>Markdown.Lite / Markdown</td><td>Markdown 工作区</td></tr>' +
   '<tr><td>日志与本地化</td><td>Log / Lang</td><td>组件库·基础设施</td></tr>' +
   '</tbody></table></div>' +
   '<div class="demo"><h4>页面索引</h4><table class="dg" style="cursor:default"><tbody>' + pageIndexHtml() + "</tbody></table>" +
   '<div class="hint">共 24 页：3 个场景工作流 + 21 个组件页，全部可在 Ribbon 页签或导航树里打开。</div></div>' +
   '<div class="demo"><h4>近期更新</h4><table class="dg" style="cursor:default"><tbody>' +
   '<tr><td>v12.1.4.4</td><td>Dock 主题去掉 Fluent 依赖，改以 Dock Simple 为基座并换装 Semi 皮肤</td></tr>' +
   '<tr><td>v12.1.4.3</td><td>高对比色板按变体落地 Semi 语义色，修复深色变体文字看不清</td></tr>' +
   '<tr><td>v12.1.4.2</td><td>Markdown 文档块虚拟化保留重块控件并补滚动锚点补偿</td></tr>' +
   '</tbody></table></div>' +
   '<div class="demo"><h4>快捷键</h4><div class="hint" style="margin:0">Ctrl+K 命令面板 · Ctrl+S 保存布局 · Ctrl+D 深浅 · Ctrl+L 语言 · F1 引导 · 双击 Ribbon 页签折叠命令区</div></div>';
 }
});

/* ═══════════════ 场景一：Markdown 工作区（写作全流程） ═══════════════ */
var FLOW_WRITE_STEPS = ["打开文档", "编辑", "实时预览", "导出 / 复制", "任务回写"];
var flowWriteStep = 0, flowWriteSub = "md-viewer";
var FLOW_WRITE_SUB = ["md-viewer", "md-editor", "md-live", "md-export", "md-viewer"];
REG("flow-write", {
 zh: {
  t: "写作全流程",
  d: "一个完整的 Markdown 工作流：打开 → 编辑 → 实时预览 → 导出 / 公众号复制 → 任务勾选回写源文。子标签直接复用组件页，避免重复实现。",
  f: ["5 步工作流", "子标签复用组件页", "任务回写"]
 },
 props: [
  { id: "md.theme", label: "排版主题", type: "combo", options: MD_THEMES.slice(0, 8), value: mdTheme,
   apply: function (v) { mdTheme = v; applyMdTheme(); log2both("TypographyTheme = " + v); } },
  { id: "md.wrap", label: "软换行", type: "bool", value: true, apply: function (v) { log2both("SoftLineBreak = " + v); } }
 ],
 render: function (el) {
  el.innerHTML = '<div class="flow" id="fwFlow">' + FLOW_WRITE_STEPS.map(function (s, i) {
   return '<span class="st' + (i === flowWriteStep ? " cur" : i < flowWriteStep ? " done" : "") + '" data-s="' + i +
    '" title="' + esc(s) + '"><span class="n">' + (i < flowWriteStep ? "✓" : i + 1) + '</span>' + esc(s) +
    '<span class="si">' + '<svg class="svgic s14" viewBox="0 0 24 24"><path d="M5 12.5l4.5 4.5L19 7"/></svg>' + "</span></span>";
  }).join("") + "</div>" +
   '<div class="subtabs" id="fwSub">' +
   [["md-viewer", "渲染"], ["md-editor", "源码编辑"], ["md-live", "实时编辑"], ["md-export", "导出 / 复制"]].map(function (t) {
    return '<button data-p="' + t[0] + '"' + (t[0] === flowWriteSub ? ' class="on"' : "") + ">" + t[1] + "</button>";
   }).join("") + "</div>" +
   '<div id="fwBody"></div>' +
   '<div class="demo"><h4>工作流状态</h4><div class="hint" id="fwState" style="margin:0"></div></div>';
  el.querySelectorAll("#fwFlow .st").forEach(function (n) {
   n.onclick = function () {
    flowWriteStep = +n.dataset.s; flowWriteSub = FLOW_WRITE_SUB[flowWriteStep];
    log2both("工作流步骤 " + (flowWriteStep + 1) + "：" + FLOW_WRITE_STEPS[flowWriteStep]);
    renderInto("flow-write", $("seg-flow-write") || el);
   };
  });
  el.querySelectorAll("#fwSub button").forEach(function (n) {
   n.onclick = function () {
    flowWriteSub = n.dataset.p;
    log2both("工作流子标签：" + pt(flowWriteSub));
    document.querySelectorAll("#fwSub button").forEach(function (x) { x.classList.toggle("on", x === n); });
    renderInto(flowWriteSub, el.querySelector("#fwBody"));
   };
  });
  var st = el.querySelector("#fwState");
  if (st) st.textContent = "当前步骤 " + (flowWriteStep + 1) + " / 5 · " + FLOW_WRITE_STEPS[flowWriteStep] + " · 子标签 " + pt(flowWriteSub);
  renderInto(flowWriteSub, el.querySelector("#fwBody"));
 }
});

/* ═══════════════ 场景二：数据工作台（表数据工作流） ═══════════════ */
var FLOW_DATA_STEPS = ["导入数据", "列配置", "排序 / 过滤", "导出"];
var flowDataStep = 0, flowDataSub = "datagrid";
var FLOW_DATA_SUB = ["datagrid", "colde", "datagrid", "datagrid"];
REG("flow-data", {
 zh: {
  t: "表数据工作流",
  d: "导入 → 列配置 → 排序过滤 → 导出；右侧属性检查器实时联动，子标签复用 DataGrid / PropertyGrid / ColumnDisplayEditor 组件页。",
  f: ["4 步工作流", "属性检查器联动", "导出带进度"]
 },
 props: [
  { id: "fd.rows", label: "行数上限", type: "range", min: 100, max: 50000, value: 10000, step: 100,
   apply: function (v) { log2both("RowsLimit = " + v); } },
  { id: "fd.natural", label: "自然排序", type: "bool", value: true, apply: function (v) { log2both("AddNaturalSorting = " + v); } }
 ],
 render: function (el) {
  el.innerHTML = '<div class="flow" id="fdFlow">' + FLOW_DATA_STEPS.map(function (s, i) {
   return '<span class="st' + (i === flowDataStep ? " cur" : i < flowDataStep ? " done" : "") + '" data-s="' + i +
    '" title="' + esc(s) + '"><span class="n">' + (i < flowDataStep ? "✓" : i + 1) + '</span>' + esc(s) +
    '<span class="si">' + '<svg class="svgic s14" viewBox="0 0 24 24"><path d="M5 12.5l4.5 4.5L19 7"/></svg>' + "</span></span>";
  }).join("") + "</div>" +
   '<div class="demo"><h4>数据源</h4><div class="row" style="margin-bottom:8px">' +
   '<button class="btn pri" onclick="fdImport()">导入示例（CSV 1 万行）</button>' +
   '<button class="btn" onclick="fdExport()">导出（长任务 · 可取消）</button>' +
   '<button class="btn danger" onclick="fdFail()">模拟一次失败</button></div>' +
   '<div class="barwrap" id="fdBar" style="display:none"><i></i></div>' +
   '<div class="hint" id="fdMsg"></div><div id="fdErr"></div></div>' +
   '<div class="subtabs" id="fdSub">' +
   [["datagrid", "表数据"], ["propgrid", "属性检查器"], ["colde", "列配置"]].map(function (t) {
    return '<button data-p="' + t[0] + '"' + (t[0] === flowDataSub ? ' class="on"' : "") + ">" + t[1] + "</button>";
   }).join("") + "</div><div id=\"fdBody\"></div>";
  el.querySelectorAll("#fdFlow .st").forEach(function (n) {
   n.onclick = function () {
    flowDataStep = +n.dataset.s; flowDataSub = FLOW_DATA_SUB[flowDataStep];
    log2both("工作流步骤 " + (flowDataStep + 1) + "：" + FLOW_DATA_STEPS[flowDataStep]);
    renderInto("flow-data", $("seg-flow-data") || el);
   };
  });
  el.querySelectorAll("#fdSub button").forEach(function (n) {
   n.onclick = function () {
    flowDataSub = n.dataset.p;
    document.querySelectorAll("#fdSub button").forEach(function (x) { x.classList.toggle("on", x === n); });
    renderInto(flowDataSub, el.querySelector("#fdBody"));
   };
  });
  renderInto(flowDataSub, el.querySelector("#fdBody"));
 }
});
var _fdTimer = null;
window.fdImport = function () {
 var msg = $("fdMsg");
 if (msg) msg.textContent = "导入 order-2026.csv：12 列 / 10 000 行 · 编码探测 UTF-8";
 notify("ok", tt("nc.ok"), "已导入 10 000 行示例数据");
 log2both("导入示例数据：10 000 行", "info");
};
window.fdExport = function () {
 var bar = $("fdBar"), msg = $("fdMsg");
 if (bar) { bar.style.display = "block"; bar.querySelector("i").style.width = "0%"; }
 if (msg) msg.innerHTML = tt("busy.export") + ' <button class="btn" onclick="fdCancel()">' + tt("busy.cancel") + "</button>";
 log2both("导出开始：xlsx / csv", "info");
 var i = 0;
 if (_fdTimer) clearInterval(_fdTimer);
 _fdTimer = setInterval(function () {
  i += 6;
  if (bar) bar.querySelector("i").style.width = Math.min(i, 100) + "%";
  if (i >= 100) {
   clearInterval(_fdTimer); _fdTimer = null;
   if (msg) msg.textContent = tt("busy.done") + "：order-2026.xlsx（4.1 MB）";
   notify("ok", tt("nc.ok"), tt("toast.exported") + "：order-2026.xlsx");
   log2both("导出完成：order-2026.xlsx", "info");
  }
 }, 110);
};
window.fdCancel = function () {
 if (_fdTimer) { clearInterval(_fdTimer); _fdTimer = null; }
 var bar = $("fdBar"); if (bar) bar.style.display = "none";
 var m = $("fdMsg"); if (m) m.textContent = "导出已取消";
 notify("warn", tt("nc.warn"), "导出已取消");
 log2both("导出已取消", "warn");
};
window.fdFail = function () {
 var box = $("fdErr"); if (!box) return;
 box.innerHTML = '<div class="errc"><div class="eh">' + esc(tt("err.title")) + '：IOException</div>' +
  '<div class="eb">无法写入 D:\\export\\order-2026.xlsx：目标文件被占用。</div>' +
  '<button class="btn" onclick="fdRetry(this)">' + esc(tt("err.retry")) + "</button></div>";
 notify("err", tt("nc.err"), "导出失败：目标文件被占用");
 log2both("导出失败：IOException（原型示例）", "error");
};
window.fdRetry = function (btn) {
 var card = btn.closest(".errc"); if (card) card.remove();
 notify("ok", tt("nc.ok"), "重试成功：order-2026.xlsx");
 log2both("重试导出成功", "info");
};

/* ═══════════════ 场景三：文档工作区（多文档工作流） ═══════════════ */
var flowDocsSub = "dock";
REG("flow-docs", {
 zh: {
  t: "多文档工作流",
  d: "Dock 工作区 + 布局记忆：面板显隐、文档页签、梯形页签视觉一致；布局可保存与重置（原型内写 localStorage）。",
  f: ["Dock 工作区", "布局保存 / 重置", "面板联动"]
 },
 props: [
  { id: "dock.remember", label: "记住布局", type: "bool", value: true, apply: function (v) { log2both("RememberLayout = " + v); } },
  { id: "dock.float", label: "允许浮动", type: "bool", value: true, apply: function (v) { log2both("AllowFloating = " + v); } }
 ],
 render: function (el) {
  el.innerHTML = '<div class="demo"><h4>布局操作</h4><div class="row">' +
   '<button class="btn pri" onclick="saveLayout()">保存布局</button>' +
   '<button class="btn danger" onclick="layoutResetAsk()">重置布局（二次确认）</button>' +
   '<button class="btn" onclick="togglePanel(\'L\')">切换导航树</button>' +
   '<button class="btn" onclick="togglePanel(\'R\')">切换属性面板</button>' +
   '<button class="btn" onclick="togglePanel(\'Log\')">切换日志面板</button>' +
   '<button class="btn danger" onclick="closeAllDocsAsk()">关闭全部文档（二次确认）</button></div>' +
   '<div class="hint" id="fdocState"></div></div>' +
   '<div class="subtabs" id="fdSub">' +
   [["dock", "Dock 布局"], ["trapezoid", "梯形页签"], ["propgrid", "属性联动"]].map(function (t) {
    return '<button data-p="' + t[0] + '"' + (t[0] === flowDocsSub ? ' class="on"' : "") + ">" + t[1] + "</button>";
   }).join("") + "</div><div id=\"fdDocBody\"></div>";
  el.querySelectorAll("#fdSub button").forEach(function (n) {
   n.onclick = function () {
    flowDocsSub = n.dataset.p;
    document.querySelectorAll("#fdSub button").forEach(function (x) { x.classList.toggle("on", x === n); });
    renderInto(flowDocsSub, el.querySelector("#fdDocBody"));
   };
  });
  renderInto(flowDocsSub, el.querySelector("#fdDocBody"));
  var st = el.querySelector("#fdocState");
  if (st) {
   st.textContent = "当前面板：导航树 " + (document.body.classList.contains("hideL") ? "隐藏" : "显示") +
    " · 属性 " + (document.body.classList.contains("hideR") ? "隐藏" : "显示") +
    " · 日志 " + (document.body.classList.contains("hideLog") ? "隐藏" : "显示") +
    " · 文档页签 " + OPEN_DOCS.length + " 个";
  }
 }
});

/* ═══════════════ 组件页：MarkdownViewer / Lite / 编辑器 / 实时编辑 / 导出 ═══════════════ */
var mdTheme = "GeekBlack";
function applyMdTheme() {
 var m = document.querySelector(".mdr"); if (!m) return;
 m.style.fontSize = MD_THEME_FS[mdTheme] || "13px";
}
REG("md-viewer", {
 zh: {
  t: "MarkdownViewer",
  d: "后台线程解析 + 增量渲染 + 块级虚拟化；排版主题即时切换（共 18 套），正文字号由排版主题接管；任务勾选回写 Markdown 源文。",
  f: ["增量渲染", "块级虚拟化", "18 主题", "任务回写"]
 },
 xaml: '<codewf:MarkdownViewer\n    Markdown="{Binding Content}"\n    TypographyTheme="{Binding Theme}"\n    EnableVirtualization="True" />',
 cs: 'viewer.TaskWriteBackRequested += (_, e) =>\n    e.Result.Markdown = e.Rendered;\n\nMarkdownThemes.OverrideTypographyResources(\n    Application.Current, MarkdownTypographyThemes.GeekBlack);',
 props: [
  { id: "tt", label: "排版主题", type: "combo", options: MD_THEMES.slice(0, 8), value: mdTheme,
   apply: function (v) { mdTheme = v; applyMdTheme(); log2both("TypographyTheme = " + v); } },
  { id: "fs", label: "字号（主题接管）", type: "range", min: 12, max: 20, value: 13, readonly: true,
   apply: function (v) { var m = document.querySelector(".mdr"); if (m) m.style.fontSize = v + "px"; log2both("FontSize = " + v); } },
  { id: "vw", label: "虚拟化", type: "bool", value: true, apply: function (v) { log2both("EnableVirtualization = " + v); } }
 ],
 render: function () {
  return '<div class="mdr" data-tt="geek"><h2>增量渲染 + 块级虚拟化</h2><p>后台线程解析，只重绘<b>变更块</b>；长文档启用<b>虚拟化</b>。</p>' +
   '<blockquote>GFM：表格 / 任务列表 / 代码块</blockquote><pre>viewer.TaskWriteBackRequested += (_, e) =&gt;\n    e.Result.Markdown = e.Rendered;</pre>' +
   '<table><tr><th>指标</th><th>说明</th></tr><tr><td>LastRenderMode</td><td>Incremental / Full</td></tr><tr><td>RealizedBlockCount</td><td>实例化块数</td></tr></table>' +
   '<p><label><input type="checkbox" checked onchange="log2both(\'任务回写 → Markdown 已更新\',\'warn\')"> 采购需求评审</label>　<label><input type="checkbox" onchange="log2both(\'任务回写 → Markdown 已更新\',\'warn\')"> 供应商比价</label></p></div>';
 }
});

REG("md-lite", {
 zh: {
  t: "Markdown Lite",
  d: "与完整包同名同命名空间的轻量渲染器：只依赖 Avalonia + Markdig，不引入 Semi 与重型能力栈，换包不换代码。",
  f: ["轻量", "零重依赖", "同命名空间"]
 },
 xaml: '<PackageReference Include="CodeWF.Avalonia.Markdown.Lite" />\n\n<codewf:MarkdownThemes />',
 render: function () {
  return '<div class="demo"><div class="mdr"><h3>Lite 渲染</h3><p>常规元素全支持：标题 / 段落 / 列表 / 引用 / 表格 / <code>行内代码</code>。</p>' +
   '<ul><li>不依赖 Semi.Avalonia</li><li>嵌入更干净</li></ul></div>' +
   '<div class="hint">Lite 传递依赖 Controls 与 Lang；这两个包本体也不依赖 Semi。</div></div>';
 }
});

REG("md-editor", {
 zh: {
  t: "源码编辑器",
  d: "AvaloniaEdit 宿主：语法着色、约 40 项动作（ExecuteAsync）、自动配对、行号。此处可编辑，改动后关闭页签会触发未保存确认。",
  f: ["语法着色", "40 项动作", "自动配对"]
 },
 xaml: '<codewf:MarkdownEditorView\n    Markdown="{Binding Source}"\n    ShowLineNumbers="True" />',
 cs: 'await editor.ExecuteAsync(MarkdownEditorAction.Bold);\n\nvar caret = editor.CaretPosition;  // 元组属性，不可解构\nstatusBar.Text = $"行 {caret.Line} 列 {caret.Column}";',
 render: function () {
  return '<div class="demo"><div class="row" style="margin-bottom:8px">' +
   '<button class="btn" style="font-weight:800" title="加粗（Ctrl+B）" onclick="edTaWrap(\'**\')">B</button>' +
   '<button class="btn" style="font-style:italic" title="斜体（Ctrl+I）" onclick="edTaWrap(\'*\')">I</button>' +
   '<button class="btn" style="font-family:Consolas" title="行内代码" onclick="edTaWrap(\'`\')">&lt;/&gt;</button>' +
   '<button class="btn" title="二级标题" onclick="edTaPre(\'## \')">H2</button>' +
   '<button class="btn" title="无序列表" onclick="edTaPre(\'- \')">列表</button>' +
   '<button class="btn" title="任务列表" onclick="edTaPre(\'- [ ] \')">任务</button>' +
   '<button class="btn" title="引用" onclick="edTaPre(\'&gt; \')">引用</button>' +
   '<button class="btn" title="分隔线" onclick="edTaPre(\'---\')">分隔</button></div>' +
   '<textarea id="edTa" oninput="markDirty(\'md-editor\')" style="width:100%;height:180px;font-family:Consolas,monospace;font-size:12px;border:1px solid var(--bd);border-radius:6px;padding:10px;background:var(--bg);color:var(--tx)"># 演示文档' + NL + NL + '正文内容。' + NL + '</textarea>' +
   '<div class="hint">ExecuteAsync(MarkdownEditorAction) 约 40 项动作 · CaretPosition 元组属性（不可解构）</div></div>';
 }
});
window.edTaWrap = function (w) {
 var ta = $("edTa"); if (!ta) return;
 var s = ta.selectionStart, e = ta.selectionEnd;
 ta.value = ta.value.slice(0, s) + w + ta.value.slice(s, e) + w + ta.value.slice(e);
 ta.selectionStart = ta.selectionEnd = s + w.length;
 markDirty("md-editor"); log2both("动作：包裹 " + w);
};
window.edTaPre = function (p) {
 var ta = $("edTa"); if (!ta) return;
 var s = ta.selectionStart;
 ta.value = ta.value.slice(0, s) + p + ta.value.slice(s);
 ta.selectionStart = ta.selectionEnd = s + p.length;
 markDirty("md-editor"); log2both("动作：插入 " + p);
};

REG("md-live", {
 zh: {
  t: "实时编辑",
  d: "分块所见即所得：点击块直接编辑，退出即渲染；FocusBlock 定位块。原型用左右分栏示意渲染结果。",
  f: ["所见即所得", "块编辑"]
 },
 xaml: '<codewf:MarkdownLiveEditorView Markdown="{Binding Doc}" />',
 cs: 'liveEditor.FocusBlock(0);\nawait liveEditor.ExecuteAsync(MarkdownEditorAction.Table);',
 render: function () {
  setTimeout(function () {
   var src = $("lvSrc"), out = $("lvOut"); if (!src || !out) return;
   var sync = function () {
    out.innerHTML = src.value.split(/\n\n+/).map(function (p) {
     if (p.indexOf("##") === 0) return "<h3>" + esc(p.replace(/^##\s*/, "")) + "</h3>";
     if (p.indexOf("# ") === 0) return "<h2>" + esc(p.replace(/^#\s*/, "")) + "</h2>";
     if (p.indexOf("- ") === 0) return "<ul>" + p.split("\n").map(function (l) { return "<li>" + esc(l.replace(/^-\s*/, "")) + "</li>"; }).join("") + "</ul>";
     return "<p>" + esc(p) + "</p>";
    }).join("");
   };
   src.oninput = function () { sync(); markDirty("md-live"); };
   sync();
  }, 0);
  return '<div class="row" style="align-items:stretch">' +
   '<div style="flex:1"><div class="hint" style="margin:0 0 4px">编辑（Markdown 源）</div>' +
   '<textarea id="lvSrc" style="width:100%;height:220px;font-family:Consolas,monospace;font-size:12px;border:1px solid var(--bd);border-radius:6px;padding:10px;background:var(--bg);color:var(--tx)">## 实时编辑' + NL + NL + '左侧修改，右侧即时渲染。' + NL + NL + '- 块级编辑' + NL + '- 退出即渲染</textarea></div>' +
   '<div style="flex:1"><div class="hint" style="margin:0 0 4px">预览</div>' +
   '<div class="mdr" id="lvOut" style="height:220px;overflow:auto;border:1px solid var(--bd);border-radius:6px;padding:10px"></div></div></div>' +
   '<div class="hint">FocusBlock(index) 可把光标定位到指定块；退出编辑即写回源文。</div>';
 }
});

REG("md-export", {
 zh: {
  t: "导出与复制",
  d: "MarkdownDocumentExporter：PNG / PDF / Word / HTML；MarkdownHtmlClipboard 一键公众号 / 知乎 / 掘金富文本。导出为长任务，带进度与取消。",
  f: ["PNG", "PDF", "Word", "公众号复制"]
 },
 cs: 'MarkdownDocumentExporter.ExportMarkdown(\n    md, ExportKind.Pdf, "Simple", "out.pdf");\n\nawait MarkdownHtmlClipboard.SetHtmlAsync(\n    md, CopyKind.Wechat, "Simple");',
 render: function () {
  return '<div class="demo"><h4>导出目标（ExportKind）</h4><div class="row" style="margin-bottom:4px">' +
   ["Png｜PNG", "Pdf｜PDF", "Word｜Word", "Html｜HTML"].map(function (x) {
    return '<span class="ftg" style="margin:0"><span style="padding:2px 10px">' + x + "</span></span>";
   }).join("") + '</div>' +
   '<div class="hint" style="margin-top:0">CopyKind：Wechat / Zhihu / Juejin 富文本一键入剪贴板 · 主题与字号可作为参数</div></div>' +
   '<div class="demo"><h4>动作</h4><div class="row">' +
   '<button class="btn pri" id="wxExport" onclick="wxExport()">开始导出（长任务 · 可取消）</button>' +
   '<button class="btn" onclick="log2both(\'MarkdownDocumentExporter.ExportMarkdown(md, ExportKind.Pdf, &quot;Simple&quot;, &quot;out.pdf&quot;)\')">ExportMarkdown</button>' +
   '<button class="btn" onclick="log2both(\'MarkdownHtmlClipboard.SetHtmlAsync(md, CopyKind.Wechat, &quot;Simple&quot;)\')">公众号复制</button></div>' +
   '<div class="barwrap" id="wxBar" style="display:none;margin-top:12px"><i></i></div>' +
   '<div class="hint" id="wxMsg">导出 / 压缩 / 下载这类长任务统一给进度条 + 取消；失败给错误卡片（本页用示意数据）。</div></div>';
 }
});
var _wxTimer = null;
window.wxExport = function () {
 var bar = $("wxBar"), msg = $("wxMsg"), btn = $("wxExport");
 if (!bar) return;
 bar.style.display = "block";
 bar.querySelector("i").style.width = "0%";
 if (msg) msg.textContent = tt("busy.export");
 if (btn) btn.disabled = true;
 var i = 0;
 if (_wxTimer) clearInterval(_wxTimer);
 _wxTimer = setInterval(function () {
  i += 7;
  bar.querySelector("i").style.width = Math.min(i, 100) + "%";
  if (i >= 100) {
   clearInterval(_wxTimer); _wxTimer = null;
   if (msg) msg.textContent = tt("busy.done");
   if (btn) btn.disabled = false;
   notify("ok", tt("nc.ok"), tt("toast.exported"));
   log2both("长任务完成（原型示意）", "info");
  }
 }, 120);
};

/* ═══════════════ 组件页：DataGrid / PropertyGrid ═══════════════ */
REG("datagrid", {
 zh: {
  t: "DataGrid",
  d: "codewf:DataGridEnhancement.UseDefaults=True 一键启用：三态排序（升 → 降 → 取消）、自然排序（item2 < item10）、智能 ToolTip、Ctrl+A 全选。",
  f: ["三态排序", "自然排序", "智能 ToolTip", "Ctrl+A"]
 },
 xaml: '<DataGrid codewf:DataGridEnhancement.UseDefaults="True"\n    ItemsSource="{Binding Rows}" />',
 cs: 'dataGrid.EnableDefaults(itemsSource);\ndataGrid.AddNaturalSorting();\ndataGrid.EnableSmartTooltips();\ngrid.ApplyPerformancePreset();',
 props: [{ id: "perf", label: "性能预设", type: "bool", value: false, apply: function (v) { log2both("ApplyPerformancePreset = " + v); } }],
 render: function (el) {
  var rows = [["item10", 28, "杭州"], ["item2", 35, "北京"], ["item1", 19, "成都"], ["item100", 41, "西雅图"], ["item33", 26, "柏林"]];
  var live = document.createElement("div");
  live.innerHTML = '<div class="demo"><h4>点击表头三态排序：升 → 降 → 取消（自然排序：item2 &lt; item10）</h4>' +
   '<table class="dg" id="dgT"><thead><tr><th data-k="0">名称</th><th data-k="1">年龄</th><th data-k="2">城市</th></tr></thead><tbody></tbody></table>' +
   '<div class="hint">悬停截断单元格显示完整内容（智能 ToolTip）· Ctrl+A 全选行</div></div>';
  var originalRows = rows.slice();
  var tb = live.querySelector("tbody"), dir = [null, null, null], last = -1;
  var key = function (s) { return String(s).replace(/(\d+)/g, function (m) { return ("00000000" + m).slice(-8); }); };
  function draw() {
   tb.innerHTML = rows.map(function (r) { return "<tr><td>" + esc(r[0]) + "</td><td>" + r[1] + "</td><td>" + esc(r[2]) + "</td></tr>"; }).join("");
  }
  draw();
  live.querySelectorAll("th").forEach(function (th, i) {
   th.onclick = function () {
    if (last !== i) dir = [null, null, null];
    last = i;
    dir[i] = dir[i] === null ? 1 : dir[i] === 1 ? -1 : null;
    live.querySelectorAll(".arr").forEach(function (a) { a.remove(); });
    var names = ["名称", "年龄", "城市"];
    if (dir[i] !== null) {
     th.innerHTML = names[i] + '<span class="arr">' + (dir[i] === 1 ? "▲" : "▼") + "</span>";
     rows.sort(function (a, b) {
      if (i === 0) return dir[i] === 1 ? (key(a[0]) > key(b[0]) ? 1 : -1) : (key(b[0]) > key(a[0]) ? 1 : -1);
      return dir[i] * (typeof a[i] === "number" ? a[i] - b[i] : String(a[i]).localeCompare(String(b[i])));
     });
    } else { th.innerHTML = names[i]; rows = originalRows.slice(); }
    log2both("排序：第 " + (i + 1) + " 列 " + (dir[i] === null ? "取消（恢复原始顺序）" : dir[i] === 1 ? "升序" : "降序"));
    draw();
   };
  });
  el.appendChild(live);
 }
});

REG("propgrid", {
 zh: {
  t: "PropertyGrid",
  d: "分类 / 字母视图、无限层级、搜索过滤、描述面板、11 种编辑器、修改加粗、动态可见性、Lang 内建。右侧面板就是本控件。",
  f: ["11 种编辑器", "分类 / 字母", "动态可见性"]
 },
 xaml: '<codewf:PropertyGrid\n    Items="{Binding Items}"\n    ViewMode="Categorized"\n    ShowDescriptionPane="True" />',
 cs: 'grid.ValueCommitted += (_, e) =>\n{\n    Apply(e.Item.Id, e.NewValue);\n    e.Item.RefreshSummary();\n};',
 render: function () {
  return '<div class="demo"><p style="font-size:12px;color:var(--tx2);line-height:1.8">PropertyGrid 自我演示：<b>右栏即本控件</b>。修改右栏任意属性会即时联动本页；被改动过的行左侧出现色条并加粗（对应 ValueCommitted 后刷新摘要）。</p></div>' +
   '<div class="demo"><h4>编辑器形态</h4><table class="dg" style="cursor:default"><tbody>' +
   '<tr><td>文本 / 数字 / 滑块 / 开关</td><td>右侧面板正在使用</td></tr>' +
   '<tr><td>下拉枚举 / 只读 / 颜色 / 文件</td><td>按属性类型自动匹配</td></tr>' +
   '<tr><td>分类视图 ↔ 字母视图</td><td>ViewMode 切换</td></tr>' +
   '</tbody></table><div class="hint">搜索过滤与描述面板由 PropertyGrid 内置。</div></div>';
 }
});

/* ═══════════════ 组件页：Dock / 梯形页签 ═══════════════ */
REG("dock", {
 zh: {
  t: "Dock 布局",
  d: "Dock.Avalonia 布局引擎 + DockSemiTheme：工具面板 / 文档区可拖拽停靠、浮动、最大化。12.x 内容承载走 Context + DataTemplates。本演示台的内容区就是 Dock 工作区。",
  f: ["停靠 / 浮动", "最大化", "Context + DataTemplates"]
 },
 xaml: '<dock:DockControl Layout="{Binding Layout}">\n  <DockControl.DataTemplates>\n    <DataTemplate DataType="vm:DocContext">\n      <md:MarkdownViewer Markdown="{Binding Markdown}" />\n    </DataTemplate>\n  </DockControl.DataTemplates>\n</dock:DockControl>',
 cs: 'var doc = new Document\n{\n    Title = "说明",\n    Context = new DocContext(markdown),   // 12.x 无 Content：内容放 Context\n};\nfactory.InitLayout(rootDock);',
 render: function () {
  return '<div class="dockws" id="dkWs">' +
   '<div class="tool"><div class="th"><span>工具 · 概览</span></div><div class="tc">' +
   '左侧工具面板（ToolDock）。<br><br>可拖拽到文档区停靠，或双击标题最大化。<br><br>点上方「日志」按钮可折叠底部日志面板——与本演示台外壳行为一致。</div></div>' +
   '<div class="main">' +
   '<div class="dtool"><button class="on" onclick="dkToggleT(this)">工具面板</button><button onclick="closeAllDocsAsk()">关闭全部文档</button><button onclick="layoutResetAsk()">重置布局</button></div>' +
   '<div class="dtabs"><span class="t on">设计说明</span><span class="t">接口草稿</span></div>' +
   '<div class="dbody"><div class="mdr"><h3>Dock.Avalonia 布局</h3><p>RootDock → ProportionalDock → ToolDock / DocumentDock。</p>' +
   '<pre>var doc = new Document {' + NL + '    Title = "说明",' + NL + '    Context = new DocContext(markdown),  // 12.x 无 Content' + NL + '};</pre>' +
   '<p>本页即 Dock 工作区实况：<b>外壳的内容区就是这套布局</b>——导航树 / 属性 / 日志都是可停靠面板。</p></div></div></div></div>' +
   '<div class="hint">DockSemiTheme 自带 {x:Type Button} / {x:Type Window} 基类主题，宿主无需再引 FluentTheme。</div>';
 }
});
window.dkToggleT = function (btn) {
 var ws = $("dkWs"); if (!ws) return;
 ws.classList.toggle("hideT");
 if (btn) btn.classList.toggle("on", !ws.classList.contains("hideT"));
 log2both("Dock 工具面板：" + (ws.classList.contains("hideT") ? "隐藏" : "显示"));
};

REG("trapezoid", {
 zh: {
  t: "梯形页签",
  d: "TrapezoidShapedTabControl / TabStrip / TabItem：梯形路径边框、首 / 中 / 末自适应。本演示台的文档页签即实例（看文档区上方页签形状）。",
  f: ["梯形路径", "首中末自适应"]
 },
 xaml: '<TabControl Theme="{StaticResource TrapezoidShapedTabControl}">\n  <TabItem Header="设计" />\n  <TabItem Header="接口" />\n</TabControl>',
 cs: '// TrapezoidShapedTabItemBorder（含 .Top 分部）绘制三态边框\n// 首 / 中 / 末自适应路径',
 render: function () {
  return '<div class="demo"><p style="font-size:12px;color:var(--tx2);line-height:1.9">梯形 ControlTheme 叠加于 Semi TabControl 体系：<br>' +
   '· TrapezoidShapedTabItemBorder 梯形边框（.Top 分部变体）<br>' +
   '· 首 / 中 / 末自适应路径与分隔线<br>' +
   '· 本演示台的文档页签即实例——看文档区上方页签形状<br>' +
   '· Dock 侧同名实现：TrapezoidShapedDocumentTabStripItemBorder</p></div>';
 }
});

/* ═══════════════ 组件页：SearchListBox / Transfer / 标记扩展 ═══════════════ */
REG("searchlist", {
 zh: {
  t: "SearchListBox",
  d: "内置 400ms 防抖搜索（ToString 包含匹配）、多选、实时计数条、双击事件 ListItemDoubleTapped(item, e)。原型按输入即时过滤。",
  f: ["400ms 防抖", "多选", "计数条"]
 },
 xaml: '<codewf:SearchListBox ItemsSource="{Binding Items}" />',
 cs: 'list.ListItemDoubleTapped += (item, _) =>\n    vm.Log($"双击：{item}");\n\nlist.AddRange(moreItems);',
 render: function (el) {
  var all = ["DataGrid 增强控件", "PropertyGrid 属性面板", "MarkdownViewer 渲染", "Transfer 穿梭框", "SearchListBox 搜索列表",
   "CodeWFWindow 无边框窗口", "Guide 新手引导", "WindowedStackPanel 虚拟化", "ColumnDisplayEditor 列编辑",
   "StatusBadge 状态徽标", "StatusCard 状态卡片", "梯形页签", "Markup If/Switch", "LogView 日志"];
  var sel = {};
  var live = document.createElement("div");
  live.innerHTML = '<div class="demo"><input id="slq" placeholder="输入过滤…" style="width:100%;height:28px;padding:0 10px;border:1px solid var(--bd);border-radius:5px;background:var(--bg);color:var(--tx);margin-bottom:8px">' +
   '<div id="slls" class="filelist" style="max-height:200px;overflow:auto"></div><div class="hint" id="slct"></div></div>';
  el.appendChild(live);
  window.slToggle = function (n) { sel[n] = !sel[n]; slDraw(); };
  window.slDraw = function () {
   var q = ($("slq").value || "").toLowerCase();
   var hit = all.filter(function (x) { return x.toLowerCase().indexOf(q) >= 0; });
   $("slls").innerHTML = hit.length ? hit.map(function (x) {
    return '<div class="fl' + (sel[x] ? " on" : "") + '" onclick="slToggle(\'' + x + '\')"><span>' + esc(x) + "</span>" +
     '<span class="sz">' + (sel[x] ? "✓ 已选" : "双击打开") + "</span></div>";
   }).join("") : '<div class="empty" style="padding:14px"><div class="et">' + esc(tt("tree.empty")) + '</div></div>';
   $("slct").textContent = "共 " + all.length + " 项 · 匹配 " + hit.length + " 项 · 已选 " +
    Object.keys(sel).filter(function (k) { return sel[k]; }).length + " 项";
  };
  setTimeout(function () { var q = $("slq"); if (!q) return; q.oninput = slDraw; slDraw(); }, 0);
 }
});

REG("transfer", {
 zh: {
  t: "Transfer",
  d: "双列穿梭：LeftItemsSource / RightItemsSource、MoveLeftToRight() / MoveRightToLeft()、双击自动移动、两侧内嵌 SearchListBox。",
  f: ["双列穿梭", "双击移动", "内嵌搜索"]
 },
 xaml: '<codewf:Transfer\n    LeftItemsSource="{Binding Available}"\n    RightItemsSource="{Binding Chosen}" />',
 cs: 'transfer.MoveLeftToRight();\ntransfer.MoveRightToLeft();   // 双击亦可',
 render: function (el) {
  var L = ["图标库", "主题引擎", "表格增强", "虚拟化面板", "引导服务"];
  var R = ["日志组件", "国际化"];
  var live = document.createElement("div");
  live.innerHTML = '<div class="demo"><div class="trf">' +
   '<div class="col"><div class="hd"><span>可用控件</span><span id="lc"></span></div><div class="ls" id="tl"></div></div>' +
   '<div class="mid"><button class="btn" style="padding:2px 10px" title="移入右侧" onclick="trfMove(\'R\')">›</button>' +
   '<button class="btn" style="padding:2px 10px" title="移回左侧" onclick="trfMove(\'L\')">‹</button></div>' +
   '<div class="col"><div class="hd"><span>已启用</span><span id="rc"></span></div><div class="ls" id="tr"></div></div></div>' +
   '<div class="hint">单击选中 · 双击条目自动移动 · 按钮移动选中项</div></div>';
  el.appendChild(live);
  var empty = '<div class="empty" style="padding:12px"><div class="ed">空</div></div>';
  window.trfDraw = function () {
   $("tl").innerHTML = L.map(function (x) {
    return '<div class="it" data-x="' + esc(x) + '" onclick="this.classList.toggle(\'on\')" ondblclick="trfDbl(\'' + esc(x) + '\',\'R\')"><span>' + esc(x) + "</span><span>«</span></div>";
   }).join("") || empty;
   $("tr").innerHTML = R.map(function (x) {
    return '<div class="it" data-x="' + esc(x) + '" onclick="this.classList.toggle(\'on\')" ondblclick="trfDbl(\'' + esc(x) + '\',\'L\')"><span>' + esc(x) + "</span><span>»</span></div>";
   }).join("") || empty;
   $("lc").textContent = L.length; $("rc").textContent = R.length;
  };
  window.trfMove = function (dir) {
   var src = dir === "R" ? L : R, dst = dir === "R" ? R : L;
   var moved = 0;
   document.querySelectorAll(dir === "R" ? "#tl .it.on" : "#tr .it.on").forEach(function (n) {
    var x = n.getAttribute("data-x"); var i = src.indexOf(x);
    if (i >= 0) { src.splice(i, 1); dst.push(x); moved++; }
   });
   trfDraw();
   log2both("穿梭移动 " + moved + " 项（左右 " + L.length + " / " + R.length + "）");
  };
  window.trfDbl = function (x, dir) {
   var src = dir === "L" ? L : R, dst = dir === "L" ? R : L;
   var i = src.indexOf(x);
   if (i >= 0) { src.splice(i, 1); dst.push(x); trfDraw(); log2both("双击穿梭：" + x); }
  };
  setTimeout(function () { trfDraw(); }, 0);
 }
});

REG("markup", {
 zh: {
  t: "标记扩展",
  d: "{codewf:If Condition, True, False} 条件取值；{codewf:Switch Value} + SwitchCase(When, Then) 集合 + Default；MultiBindingExtensionBase 多绑。",
  f: ["If", "Switch", "多绑"]
 },
 xaml: '<TextBlock Text="{codewf:If Condition={Binding IsBusy},\n    True=\'加载中…\', False=\'就绪\'}" />\n\n<ContentControl Content="{codewf:Switch Value={Binding State}}">\n  <codewf:SwitchCase When="Run" Then="运行视图" />\n</ContentControl>',
 cs: '// 编译期展开为 IfBinding / SwitchBinding，零反射',
 render: function (el) {
  var live = document.createElement("div");
  live.innerHTML = '<div class="demo"><h4>If 扩展（拖动滑块模拟状态）</h4>' +
   '<input type="range" min="0" max="100" value="30" id="mkIf" oninput="mkIfSync(this.value)" style="width:260px;accent-color:var(--pri)">' +
   '<div style="margin:10px 0;font-size:15px">状态：<b id="mkIfV" style="color:var(--pri)">就绪</b></div>' +
   '<div class="hint">XAML：Text="{codewf:If Condition={Binding Busy}, True=\'处理中…\', False=\'就绪\'}"</div></div>' +
   '<div class="demo"><h4>Switch 扩展</h4><div class="row">' +
   ["运行", "停止", "暂停"].map(function (s, i) { return '<button class="btn' + (i === 0 ? " pri" : "") + '" data-s="' + s + '" onclick="mkSwSet(this)">' + s + "</button>"; }).join("") +
   '</div><div style="margin-top:10px;font-size:15px">当前： <b id="mkSwV" style="color:var(--pri)">运行</b></div>' +
   '<div class="hint">SwitchCase(When, Then) 集合 + Default 兜底；编译期展开为零反射绑定</div></div>';
  el.appendChild(live);
 }
});
window.mkIfSync = function (v) { var el = $("mkIfV"); if (el) el.textContent = +v > 50 ? "处理中…" : "就绪"; };
window.mkSwSet = function (b) {
 var el = $("mkSwV"); if (el) el.textContent = b.dataset.s;
 document.querySelectorAll("button[data-s]").forEach(function (x) { x.classList.toggle("pri", x === b); });
 log2both("Switch 值 = " + b.dataset.s);
};

/* ═══════════════ 组件页：状态组件 / 引导 / 虚拟化 / 列配置 ═══════════════ */
REG("status", {
 zh: {
  t: "状态组件",
  d: "StatusBadge（左值右标签）/ StatusCard（Kind 驱动伪类 :debug/:info/:warn/:error/:fatal），五档语义 × 深浅自动换肤。",
  f: ["5 档语义", "伪类换肤"]
 },
 xaml: '<codewf:StatusBadge LeftText="92%" RightText="CPU" />\n\n<codewf:StatusCard Kind="{Binding Kind}"\n    Header="部署服务" Content="运行正常" />',
 cs: '// StatusLabelKind 五档：Debug / Info / Warn / Error / Fatal\ncard.Kind = StatusLabelKind.Error;',
 render: function () {
  var ks = [["Debug", "调试输出"], ["Info", "服务运行中"], ["Warn", "内存水位偏高"], ["Error", "连接已超时"], ["Fatal", "进程已终止"]];
  return '<div class="demo"><h4>StatusBadge（左值 + 右标签）</h4><div style="margin-bottom:14px">' +
   ks.map(function (k) { return '<span class="sbadge k-' + k[0].toLowerCase() + '">' + k[0] + "</span>"; }).join("") + "</div>" +
   '<h4>StatusCard（Kind 驱动头部色带）</h4><div>' +
   ks.map(function (k) {
    return '<div class="scard"><div class="h k-' + k[0].toLowerCase() + '"><span>' + k[1] + "</span><span>" + k[0] + '</span></div>' +
     '<div class="b">卡片内容区。伪类 :' + k[0].toLowerCase() + ' 驱动头部色带与图标颜色，深浅主题自动换肤。</div></div>';
   }).join("") + '</div>' +
   '<div class="row" style="margin-top:6px">' +
   '<button class="btn" onclick="notify(\'info\',tt(\'nc.info\'),\'StatusCard 也用于通知中心\')">Info 通知</button>' +
   '<button class="btn" onclick="notify(\'warn\',tt(\'nc.warn\'),\'内存水位偏高\')">Warn 通知</button>' +
   '<button class="btn danger" onclick="notify(\'err\',tt(\'nc.err\'),\'连接已超时\')">Error 通知</button></div></div>';
 }
});

REG("guide", {
 zh: {
  t: "页面引导",
  d: "聚光遮罩引导：Popup 窗口级覆盖、13 方位（GuidePlacementMode）、键盘导航、MissingTargetBehavior（Center / Skip / Close）。",
  f: ["聚光遮罩", "13 方位", "键盘导航"]
 },
 xaml: '<codewf:Guide Steps="{Binding Steps}" MissingTargetBehavior="Skip" />\n\n<codewf:GuideStep Target="{Binding ElementName=NavTree}"\n    Title="导航树" Placement="Right" />',
 cs: 'guide.Show();\nguide.GoNext();\nguide.StepOpened += (_, e) => log(e.Step.Title);',
 render: function () {
  return '<div class="demo"><p style="font-size:12px;color:var(--tx2);margin-bottom:12px">对本页区域逐个引导（模拟 GuideOverlay 聚光遮罩 + 13 方位气泡）。Esc / 跳过 / 关闭退出。</p>' +
   '<button class="btn pri" onclick="guideThisPage()">▶ 开始引导本页</button>' +
   '<div class="hint">StepOpened 事件可上报每一步；MissingTargetBehavior 决定目标缺失时 Center / Skip / Close。</div></div>';
 }
});
window.guideThisPage = function () {
 guideGo([
  { sel: ".rtabs", t: "Ribbon 页签", d: "七个页签对应 3 个场景 + 3 个组件库分区，双击可折叠命令区。" },
  { sel: ".p-nav", t: "导航树面板", d: "单击叶子节点打开演示文档，Dock 工具面板形态。" },
  { sel: ".p-props", t: "属性面板", d: "PropertyGrid 实时联动当前演示页。" },
  { sel: ".p-log", t: "日志面板", d: "LogView 实时接收 Logger 事件流，可折叠。" },
  { sel: ".p-doc .dbody", t: "文档区", d: "演示主体 + 演示 / XAML / C# 三段切换。" }
 ]);
};

REG("appguide", {
 zh: {
  t: "应用引导",
  d: "对整个演示台逐区域引导：目标跨控件树，用 TopLevel.GetTopLevel 按名称递归查找。",
  f: ["跨控件树", "按名查找"]
 },
 cs: 'var root = TopLevel.GetTopLevel(this);\nvar target = FindByName(root, "RibbonRoot");',
 render: function () {
  return '<div class="demo"><p style="font-size:12px;color:var(--tx2);margin-bottom:12px">对整个演示台逐区域引导（5 步，可跳过）。</p>' +
   '<button class="btn pri" onclick="guideOpen()">▶ 开始应用引导</button></div>';
 }
});

REG("wsp", {
 zh: {
  t: "WindowedStackPanel",
  d: "窗口化虚拟化堆叠面板：EnableVirtualization + VirtualizationThreshold；实时指标 ItemCount / IsVirtualizing / RealizedItemCount。",
  f: ["5000 块", "实时指标"]
 },
 xaml: '<codewf:WindowedStackPanel\n    EnableVirtualization="True"\n    VirtualizationThreshold="200" />',
 cs: 'panel.AddItem(block);\npanel.AddRange(blocks);\n\nmetrics.Text = $"实例化 {panel.RealizedItemCount} / {panel.ItemCount}";',
 props: [{ id: "count", label: "块数量", type: "range", min: 100, max: 5000, value: 2000, step: 100, apply: function (v) { wspFill(v); log2both("块数量 = " + v); } }],
 render: function () {
  setTimeout(function () { wspFill(2000); }, 0);
  return '<div class="demo"><div style="margin-bottom:8px">' +
   '<span style="display:inline-block;background:var(--pri-lt);color:var(--pri);font-size:10px;font-weight:700;padding:3px 8px;border-radius:3px">虚拟化中</span> ' +
   '<span style="font-size:11px;color:var(--tx2)" id="wspM"></span></div>' +
   '<div class="wsp" id="wspBox"></div><div class="hint">原型截断到 120 块以示虚拟化效果。</div></div>';
 }
});
function wspFill(n) {
 var b = $("wspBox"); if (!b) return;
 var html = "";
 for (var i = 0; i < Math.min(n, 120); i++) {
  html += '<div class="blk"><span>块 ' + (i + 1) + " / " + n + "</span><span>#" + (1000 + i) + "</span></div>";
 }
 b.innerHTML = html + (n > 120 ? '<div style="padding:8px;color:var(--tx3);font-size:10px;text-align:center">… 虚拟化截断演示（实际渲染 ' + Math.min(n, 120) + " / " + n + " 块）</div>" : "");
 var m = $("wspM"); if (m) m.textContent = "块总数 " + n + " · 实例化 " + Math.min(n, 120);
}

REG("colde", {
 zh: {
  t: "ColumnDisplayEditor",
  d: "列显示配置：拖拽排序（IList.Move）+ 勾选可见；模型 ColumnDisplayItem(Key, DisplayText, Visible) 实现 INPC。",
  f: ["拖拽排序", "勾选可见"]
 },
 xaml: '<codewf:ColumnDisplayEditor Items="{Binding Columns}" />',
 cs: '// 拖拽完成直接 IList.Move\nColumns.Move(oldIndex, newIndex);',
 props: [{ id: "keep", label: "至少保留一列", type: "bool", value: true, apply: function (v) { log2both("至少保留一列 = " + v); } }],
 render: function () {
  setTimeout(function () { cdeDraw(); }, 0);
  return '<div class="demo cde"><div class="hint" style="margin-bottom:8px">拖拽手柄排序 · 勾选控制可见（对应 Columns.Move）</div><div id="cdeLs"></div></div>';
 }
});
var _cde = [{ n: "名称", v: 1 }, { n: "大小", v: 1 }, { n: "修改时间", v: 1 }, { n: "类型", v: 0 }, { n: "备注", v: 1 }];
function cdeDraw() {
 var ls = $("cdeLs"); if (!ls) return;
 ls.innerHTML = _cde.map(function (c, i) {
  return '<div class="it" draggable="true" data-i="' + i + '"><span class="hnd">⠿</span>' +
   '<input type="checkbox" ' + (c.v ? "checked" : "") + ' data-i="' + i + '"><span style="flex:1">' + esc(c.n) + "</span>" +
   '<span style="margin-left:auto;color:var(--tx3);font-size:10px">位置 ' + (i + 1) + "</span></div>";
 }).join("");
 ls.querySelectorAll(".it").forEach(function (n) {
  var i = +n.getAttribute("data-i");
  n.addEventListener("dragstart", function (e) { window._di = i; e.dataTransfer.effectAllowed = "move"; });
  n.addEventListener("dragover", function (e) { e.preventDefault(); n.classList.add("over"); });
  n.addEventListener("dragleave", function () { n.classList.remove("over"); });
  n.addEventListener("drop", function (e) {
   e.preventDefault();
   var j = +n.getAttribute("data-i");
   var x = _cde.splice(i, 1)[0];
   _cde.splice(j, 0, x);
   cdeDraw();
   log2both("列排序移动：" + x.n + " → 位置 " + (j + 1));
  });
  n.querySelector("input").addEventListener("change", function (e) {
   _cde[i].v = e.target.checked ? 1 : 0;
   log2both("列可见：" + _cde[i].n + " = " + !!e.target.checked);
  });
 });
}

/* ═══════════════ 组件页：Log / Lang / CodeWFWindow ═══════════════ */
REG("log", {
 zh: {
  t: "Log 日志",
  d: "LogView 绑定 Logger.Events 实时事件流；MinimumLevel / MaximumLevel 过滤、MaxDisplayCount、右键打开日志目录。底部日志面板即 Dock 底部工具面板形态。",
  f: ["实时流", "级别过滤", "MaxDisplayCount"]
 },
 xaml: '<codewf:LogView MinimumLevel="Debug"\n    MaximumLevel="Critical"\n    MaxDisplayCount="1200" />\n\n<codewf:CodeWFLogTheme />',
 cs: 'Logger.Info("服务已启动");\nLogger.Warn("内存偏高");\nLogger.Error("处理失败", ex);\n\nawait Logger.ShutdownAsync();',
 props: [{
  id: "min", label: "最低级别", type: "combo", options: ["Debug", "Info", "Warn", "Error"], value: "Info",
  apply: function (v) { LOG_LEVEL = v; var st = $("stLogLv"); if (st) st.textContent = v; log2both("MinimumLevel = " + v); }
 }],
 render: function () {
  return '<div class="demo"><div class="row" style="margin-bottom:10px">' +
   '<button class="btn" title="Logger.Info" onclick="log2both(\'演示：一条信息日志\',\'info\')">Info</button>' +
   '<button class="btn" title="Logger.Warn" onclick="log2both(\'演示：一条警告日志\',\'warn\')">Warn</button>' +
   '<button class="btn danger" title="Logger.Error" onclick="log2both(\'演示：一条错误日志\',\'error\')">Error</button>' +
   '<button class="btn" title="连续写入日志" onclick="lgBurst()">批量 ×10</button>' +
   '<button class="btn" onclick="togglePanel(\'Log\')">切换底部日志面板</button></div>' +
   '<div class="hint">LogView 绑定 Logger.Events 事件流 · 底部日志面板与这里同源 · CodeWFLogTheme 为 sealed，要自定义配色请覆盖同名画笔键</div></div>';
 }
});
window.lgBurst = function () { for (var i = 0; i < 10; i++) log2both("批量日志 #" + (i + 1), "info"); };

REG("lang", {
 zh: {
  t: "Lang 国际化",
  d: "Json / Xml / Resx 三后端 + 内嵌资源；I18nManager.Culture 切换即时刷新；缺键回退链 zh-CN → zh → 默认。注意实际 API 是 Instance.Register(plugin, culture)，不是文档里的 Init。",
  f: ["3 后端", "即时切换", "回退链"]
 },
 cs: 'I18nManager.Instance.Register(\n    new JsonLangPlugin { ResourceFolder = "I18n" },\n    new CultureInfo("zh-CN"));\n\nI18nManager.Instance.Culture = new CultureInfo("en-US");',
 props: [{
  id: "lang.sel", label: "界面语言", type: "combo", options: ["简体中文", "English"], value: "简体中文",
  apply: function (v) { setLang(v === "English" ? "en-US" : "zh-CN"); }
 }],
 render: function () {
  return '<div class="demo"><h4>运行时切换（与演示台共享 I18nManager）</h4><div class="row">' +
   '<button class="btn pri" onclick="setLang(\'zh-CN\')">简体中文</button>' +
   '<button class="btn" onclick="setLang(\'en-US\')">English</button></div>' +
   '<div class="hint">标题栏「文A」按钮同样切换 · 缺键回退：zh-CN → zh → 默认 · Markup：{codewf:I18n Key}</div></div>' +
   '<div class="demo"><h4>本演示台的文案来源</h4><table class="dg" style="cursor:default"><tbody>' +
   '<tr><td>界面文案</td><td>全部走 I18N 表（zh-CN / en-US 两份），切换语言即时刷新</td></tr>' +
   '<tr><td>强类型 Key</td><td>CodeWF.Avalonia.Lang.Generator 编译期生成，避免魔法字符串</td></tr>' +
   '</tbody></table></div>';
 }
});

REG("codewfwindow", {
 zh: {
  t: "CodeWFWindow",
  d: "无边框窗口：WindowDecorations=None + DWM 圆角（Windows）、8 向缩放、标题栏三层内容槽（Left / Center / Right / TitleBarContent）。本演示台的外壳标题栏就是它。",
  f: ["无边框", "DWM 圆角", "8 向缩放"]
 },
 xaml: '<codewf:CodeWFWindow\n    TitleBarHeight="34"\n    ContentExtendsIntoTitleBar="True"\n    IsFullScreenButtonVisible="True" />',
 cs: 'window.LeftContent = new TextBlock { Text = "◆" };\nwindow.RightContent = searchBox;',
 render: function () {
  var cfg = [["默认标题栏", 0, 0], ["全屏按钮 + 内容钻入", 1, 0], ["隐藏最小化", 1, 1]];
  return '<div class="row">' + cfg.map(function (c) {
   return '<div style="border:1px solid var(--bd);border-radius:8px;overflow:hidden;width:250px;cursor:pointer" onclick="log2both(\'打开演示窗口：' + c[0] + '\')">' +
    '<div style="display:flex;align-items:center;height:32px;background:linear-gradient(90deg,var(--pri),transparent);padding:0 8px 0 12px">' +
    '<span style="color:#fff;font-size:11px">◆</span><span style="color:#fff;font-size:11px;flex:1;margin-left:8px">' + c[0] + "</span>" +
    '<span style="color:rgba(255,255,255,.8);font-size:11px">' + (c[1] ? "— ⛶ □ ✕" : "— □ ✕") + "</span></div>" +
    '<div style="padding:18px 12px;color:var(--tx2);font-size:11px;line-height:1.8">全屏按钮：' + (c[1] ? "显示" : "隐藏") +
    "<br>最小化按钮：" + (c[2] ? "隐藏" : "显示") + "</div></div>";
  }).join("") + "</div>" +
   '<div class="hint">TitleBarHeight 默认 34px；内容钻入标题栏时交互区自动避让按钮。</div>';
 }
});

/* ═══════════════ 属性面板：全局项 + 当前页项 ═══════════════ */
var SELF_PROPS = [
 { id: "appearance.density", label: "密度", type: "combo", options: ["舒适", "紧凑"], value: "舒适", global: true,
  apply: function (v) { setDensity(v === "紧凑" ? "compact" : "cozy"); } },
 { id: "appearance.palette", label: "色板", type: "combo", options: ["默认", "水生", "沙漠", "黄昏", "夜空", "高对比"], value: "默认", global: true,
  apply: function (v) {
   var m = { "默认": "default", "水生": "aqua", "沙漠": "desert", "黄昏": "dusk", "夜空": "night", "高对比": "contrast" };
   setPalette(m[v] || "default");
  } },
 { id: "appearance.dark", label: "深色模式", type: "bool", value: true, global: true,
  apply: function (v) { setTheme(v ? "dark" : "light"); } },
 { id: "appearance.lang", label: "语言", type: "combo", options: ["简体中文", "English"], value: "简体中文", global: true,
  apply: function (v) { setLang(v === "English" ? "en-US" : "zh-CN"); } }
];

/* ═══════════════ Ribbon / 树 / 文档 / 属性 渲染 ═══════════════ */
var curTab = "home", OPEN_DOCS = [], ACTIVE_DOC = null, DIRTY = {}, RENAMES = {}, NAV_FILTER = "";
var SEG = {};
var MODIFIED = {};

function renderRTabs() {
 $("rTabs").innerHTML = CATALOG.map(function (g) {
  return '<div class="t2' + (g.id === curTab ? " on" : "") + '" data-t="' + g.id + '" role="tab" tabindex="0" title="' +
   esc(tabName(g.id)) + '（双击折叠命令区）">' + tabIconSvg(g.id) + "<span>" + esc(tabName(g.id)) + "</span></div>";
 }).join("");
 document.querySelectorAll(".rt2 .t2").forEach(function (n) {
  n.onclick = function () { switchTab(n.dataset.t); };
  n.ondblclick = function () { toggleRibbon(); };
  n.onkeydown = function (e) { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); switchTab(n.dataset.t); } };
 });
 renderRBody();
}
function renderRBody() {
 var g = CATALOG.filter(function (x) { return x.id === curTab; })[0];
 if (!g) { $("rBody").innerHTML = ""; return; }
 var sceneFirst = g.items[0].id.indexOf("flow-") === 0;
 var html = "";
 if (sceneFirst) html += '<div class="grp"><div class="cmds">' + cmdBtn(g.items[0]) + '</div><div class="gname">场景</div></div>';
 var rest = sceneFirst ? g.items.slice(1) : g.items;
 html += '<div class="grp"><div class="cmds">' + rest.map(cmdBtn).join("") + '</div><div class="gname">' +
  esc(sceneFirst ? "组件" : tabName(g.id)) + "</div></div>";
 $("rBody").innerHTML = html;
 document.querySelectorAll(".rbn .cbtn").forEach(function (b) {
  b.onclick = function () { openDoc(b.dataset.p); };
 });
}
function cmdBtn(it) {
 var label = pt(it.id), alias = pa(it.id);
 return '<button class="cbtn" data-p="' + it.id + '" title="' + esc(label + (alias ? "（" + alias + "）" : "")) +
  '" aria-pressed="' + (ACTIVE_DOC === it.id) + '">' +
  '<span class="ic">' + iconSvg(it.id) + '</span><span class="lb">' + esc(label) + "</span></button>";
}
function switchTab(id) {
 curTab = id;
 renderRTabs();
 renderTree();
 var g = CATALOG.filter(function (x) { return x.id === id; })[0];
 if (g && g.items.length) openDoc(g.items[0].id);
 log2both("切换页签：" + tabName(id));
}
function toggleRibbon() {
 document.body.classList.toggle("rmn");
 log2both((document.body.classList.contains("rmn") ? "折叠" : "展开") + "命令区");
}

function renderTree() {
 var q = NAV_FILTER.toLowerCase();
 var total = 0;
 var html = '<div class="fnd"><input id="navQ" placeholder="' + esc(tt("tree.filter")) + '" value="' + esc(NAV_FILTER) + '" title="' + esc(tt("tree.filter")) + '">' +
  (Object.keys(RENAMES).length ? '<button class="hbtn" style="visibility:visible" onclick="clearRenames()" title="' + esc(tt("tree.reset")) + '">↺</button>' : "") + "</div>";
 CATALOG.forEach(function (g) {
  var items = g.items.filter(function (it) {
   return !q || pt(it.id).toLowerCase().indexOf(q) >= 0 || pa(it.id).toLowerCase().indexOf(q) >= 0 || tabName(g.id).toLowerCase().indexOf(q) >= 0;
  });
  total += items.length;
  if (!items.length && q) return;
  var open = (g.id === curTab || (q && items.length)) ? " open" : "";
  html += '<div class="nd' + open + '" data-g="' + g.id + '">' +
   '<div class="hd" tabindex="0"><span class="arw">▶</span><span class="gi">' + tabIconSvg(g.id) + "</span><span>" + esc(tabName(g.id)) + "</span>" +
   '<span style="margin-left:auto;color:var(--tx3);font-size:10px">' + items.length + "</span></div><div class=\"kids\">" +
   items.map(function (it) {
    return '<div class="lf' + (it.id === ACTIVE_DOC ? " on" : "") + '" data-p="' + it.id + '" title="' + esc(pt(it.id)) + '">' +
     '<span class="ic">' + iconSvg(it.id, "s14") + '</span><span>' + esc(RENAMES[it.id] || pt(it.id)) + '</span><span class="sp2"></span>' +
     '<button class="hbtn" data-rn="' + it.id + '" title="' + esc(tt("tree.rename")) + '">✎</button></div>';
   }).join("") + "</div></div>";
 });
 if (!total) html += '<div class="empty"><div class="et">' + esc(tt("tree.empty")) + "</div></div>";
 $("lTree").innerHTML = html;
 var qi = $("navQ");
 if (qi) {
  qi.oninput = function () {
   NAV_FILTER = qi.value;
   var pos = qi.selectionStart;
   renderTree();
   var n2 = $("navQ");
   if (n2) { n2.focus(); try { n2.setSelectionRange(pos, pos); } catch (e) { } }
  };
 }
 document.querySelectorAll(".tree .nd .hd").forEach(function (h) {
  h.onclick = function (e) { if (e.target.tagName === "INPUT") return; switchTab(h.parentNode.dataset.g); };
 });
 document.querySelectorAll(".tree .lf").forEach(function (l) {
  l.onclick = function (e) { if (e.target.classList.contains("hbtn")) return; openDoc(l.dataset.p); };
  l.ondblclick = function (e) { if (e.target.classList.contains("hbtn")) return; startRename(l.dataset.p, l); };
 });
 document.querySelectorAll(".tree .hbtn[data-rn]").forEach(function (b) {
  b.onclick = function (e) { e.stopPropagation(); startRename(b.dataset.rn, b.closest(".lf")); };
 });
 var c = $("navCnt"); if (c) c.textContent = "(" + total + ")";
}
function startRename(id, lf) {
 if (!lf) return;
 lf.innerHTML = '<input class="rnm" value="' + esc(RENAMES[id] || pt(id)) + '">';
 var inp = lf.querySelector("input");
 inp.focus(); inp.select();
 var done = function (save) {
  if (save && inp.value.trim()) {
   RENAMES[id] = inp.value.trim();
   log2both("就地重命名：" + pt(id) + " → " + inp.value.trim());
  }
  renderTree(); renderDocTabs(); renderRBody();
 };
 inp.onblur = function () { done(true); };
 inp.onkeydown = function (e) { if (e.key === "Enter") done(true); if (e.key === "Escape") done(false); };
}
window.clearRenames = function () {
 RENAMES = {};
 renderTree(); renderDocTabs(); renderRBody();
 notify("info", tt("nc.info"), tt("tree.reset"));
};

function openDoc(id) {
 if (!PROTO[id]) return;
 if (OPEN_DOCS.indexOf(id) < 0) OPEN_DOCS.push(id);
 ACTIVE_DOC = id;
 renderDocTabs(); renderDoc(id); renderTree(); renderRBody();
 log2both("打开文档：" + pt(id));
}
function closeDoc(id) {
 if (DIRTY[id]) {
  confirmDlg(tt("dlg.leave.t"), tt("dlg.leave.d"), function () { DIRTY[id] = false; doCloseDoc(id); });
  return;
 }
 doCloseDoc(id);
}
function doCloseDoc(id) {
 OPEN_DOCS = OPEN_DOCS.filter(function (x) { return x !== id; });
 if (ACTIVE_DOC === id) ACTIVE_DOC = OPEN_DOCS[OPEN_DOCS.length - 1] || null;
 renderDocTabs(); renderTree(); renderRBody();
 if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
 renderPGrid(ACTIVE_DOC);
 log2both("关闭文档：" + pt(id));
}
 window.closeAllDocsAsk = function () {
  confirmDlg(tt("dlg.del.t"), tt("dlg.del.d"), function () {
   OPEN_DOCS = []; ACTIVE_DOC = null; DIRTY = {};
   renderDocTabs(); renderEmptyDoc(); renderTree(); renderRBody(); renderPGrid(null);
   notify("warn", tt("nc.warn"), "已关闭全部文档");
   log2both("已关闭全部文档", "warn");
  });
 };
function markDirty(id) {
 if (DIRTY[id]) return;
 DIRTY[id] = true;
 renderDocTabs();
 log2both("文档已修改：" + pt(id), "warn");
}
function renderDocTabs() {
 $("dT").innerHTML = OPEN_DOCS.map(function (id) {
  var name = (RENAMES[id] || pt(id)) + (DIRTY[id] ? " ●" : "");
  return '<div class="dt' + (id === ACTIVE_DOC ? " on" : "") + '" data-p="' + id + '" title="' + esc(name) + '">' +
   "<span>" + esc(name) + '</span><span class="x" data-x="' + id + '" title="关闭（未保存会二次确认）">✕</span></div>';
 }).join("");
 document.querySelectorAll(".dt").forEach(function (n) {
  n.onclick = function (e) {
   var x = e.target.closest("[data-x]");
   if (x) closeDoc(x.getAttribute("data-x")); else openDoc(n.getAttribute("data-p"));
  };
 });
 var st = $("stDocs"); if (st) st.textContent = OPEN_DOCS.length;
}
function renderEmptyDoc() {
 $("dBody").innerHTML = '<div class="empty"><div class="ei">' +
  '<svg class="svgic" style="width:34px;height:34px;color:var(--tx3)" viewBox="0 0 24 24" aria-hidden="true">' +
  '<rect x="3.5" y="7" width="10" height="12.5" rx="1.6"/><path d="M7.5 7V4.5h11a1.5 1.5 0 0 1 1.5 1.5v11a1.5 1.5 0 0 1-1.5 1.5H17"/></svg>' +
  '</div><div class="et">' + esc(tt("empty.title")) + "</div>" +
  '<div class="ed">' + esc(tt("empty.desc")) + '</div>' +
  '<button class="btn pri" onclick="openDoc(\'welcome\')">' + esc(tt("empty.action")) + "</button></div>";
 var w = $("stWhere"); if (w) w.textContent = tt("st.where");
}
function renderDoc(id) {
 var pg = PROTO[id]; if (!pg) return;
 SEG[id] = SEG[id] || "demo";
 var seg = SEG[id];
 $("dBody").innerHTML =
  '<div class="crumb">' + esc(docCrumb(id)) + "</div><h1>" + esc(RENAMES[id] || pg.zh.t) + "</h1>" +
  (pa(id) ? '<div class="alias">' + esc(pa(id)) + "</div>" : "") +
  '<div class="dsc">' + esc(pg.zh.d) + "</div>" +
  '<div class="ftg">' + pg.zh.f.map(function (f) { return "<span>" + esc(f) + "</span>"; }).join("") + "</div>" +
  '<div class="smt" id="smt-' + id + '">' +
  [["demo", "演示"], ["xaml", "XAML"], ["cs", "C#"]].filter(function (s) { return s[0] === "demo" || pg[s[0]]; }).map(function (s) {
   return '<button data-s="' + s[0] + '"' + (seg === s[0] ? ' class="on"' : "") + ">" + s[1] + "</button>";
  }).join("") + '</div><div id="seg-' + id + '"></div>';
 var target = $("seg-" + id);
 if (seg === "demo") {
  target.innerHTML = '<div class="skel"><i></i><i></i><i></i><i></i><i></i><i></i></div>';
  setTimeout(function () {
   if (ACTIVE_DOC !== id) return;
   var t2 = $("seg-" + id); if (!t2) return;
   renderInto(id, t2);
   if (id === "md-viewer") applyMdTheme();
  }, 90);
 } else {
  target.innerHTML = '<div class="smcode">' + esc(pg[seg]) + "</div>";
 }
 document.querySelectorAll("#smt-" + id + " button").forEach(function (b) {
  b.onclick = function () {
   SEG[id] = b.dataset.s;
   document.querySelectorAll("#smt-" + id + " button").forEach(function (x) { x.classList.toggle("on", x === b); });
   renderDoc(id);
  };
 });
 renderPGrid(id);
 var w = $("stWhere"); if (w) w.textContent = docCrumb(id);
}

/* ═══ 属性面板 ═══ */
function propKey(pageId, p) { return (p.global ? "" : (pageId || "")) + "|" + p.id; }
function rowHtml(pageId, p) {
 var mod = MODIFIED[propKey(pageId, p)] ? " mod" : "";
 var ctl;
 if (p.type === "combo") {
  ctl = '<select data-id="' + p.id + '"' + (p.readonly ? " disabled" : "") + ">" +
   p.options.map(function (o) { return "<option" + (o === p.value ? " selected" : "") + ">" + esc(o) + "</option>"; }).join("") + "</select>";
 } else if (p.type === "bool") {
  ctl = '<div class="switch' + (p.value ? " on" : "") + '" data-id="' + p.id + '" role="switch" tabindex="0" title="' + esc(p.label) + '"></div>';
 } else {
  ctl = '<input type="range" data-id="' + p.id + '" min="' + p.min + '" max="' + p.max + '" step="' + (p.step || 1) +
   '" value="' + p.value + '"' + (p.readonly ? " disabled" : "") + ">";
 }
 return '<div class="rw' + mod + '"><label title="' + esc(p.label) + '">' + esc(p.label) + '</label><div class="ctl">' + ctl + "</div></div>";
}
function renderPGrid(id) {
 var pageProps = (id && PROTO[id] && PROTO[id].props) || [];
 var html = '<div class="cat">外观 · 语言（全局）</div>' + SELF_PROPS.map(function (p) { return rowHtml(id, p); }).join("");
 html += '<div class="cat">' + esc(id ? pt(id) : "当前文档") + "</div>";
 html += pageProps.length ? pageProps.map(function (p) { return rowHtml(id, p); }).join("")
  : '<div class="pgp-empty">当前文档无属性</div>';
 html += '<div class="cat">状态</div>' +
  '<div class="rw"><label>打开文档数</label><div class="ctl"><span style="font-size:11px">' + OPEN_DOCS.length + "</span></div></div>" +
  '<div class="rw"><label>日志级别</label><div class="ctl"><span style="font-size:11px">' + esc(LOG_LEVEL) + "</span></div></div>";
 $("pGrid").innerHTML = html;
 var cnt = $("pgCnt"); if (cnt) cnt.textContent = "(" + (SELF_PROPS.length + pageProps.length) + " 项)";
 var all = SELF_PROPS.concat(pageProps);
 all.forEach(function (p) {
  var el = document.querySelector('#pGrid [data-id="' + p.id + '"]');
  if (!el) return;
  if (p.type === "combo") el.onchange = function () { applyProp(id, p, el.value); };
  else if (p.type === "range") el.oninput = function () { applyProp(id, p, +el.value, true); };
  else if (p.type === "bool") el.onclick = function () { el.classList.toggle("on"); applyProp(id, p, el.classList.contains("on"), true); };
 });
}
function applyProp(pageId, p, v, light) {
 p.value = v;
 var k = propKey(pageId, p);
 var first = !MODIFIED[k];
 MODIFIED[k] = true;
 if (p.apply) guard(function () { p.apply(v); }, "prop:" + p.id);
 if (light && !first) return;
 renderPGrid(ACTIVE_DOC);
}

/* ═══ 外壳：色板 / 深浅 / 语言 / 密度 / 面板 / 布局 ═══ */
var PAL_LIST = ["default", "aqua", "desert", "dusk", "night", "contrast"];
var PAL_COLOR = { default: "#0064fa", aqua: "#0091a8", desert: "#c26a1d", dusk: "#7a5cc4", night: "#2b6cb0", contrast: "#888" };
function setPalette(v) {
 document.documentElement.dataset.palette = v;
 var sel = $("palSel"); if (sel) sel.value = v;
 var p = SELF_PROPS[1]; p.value = { default: "默认", aqua: "水生", desert: "沙漠", dusk: "黄昏", night: "夜空", contrast: "高对比" }[v] || "默认";
 renderPalDots();
 log2both(tt("toast.palette") + p.value);
}
function renderPalDots() {
 var cur = document.documentElement.dataset.palette;
 $("palDots").innerHTML = PAL_LIST.map(function (p) {
  return '<i style="background:' + PAL_COLOR[p] + '" data-p="' + p + '" title="' + p + '"' + (p === cur ? ' class="on"' : "") + "></i>";
 }).join("");
 document.querySelectorAll(".pal i").forEach(function (i) { i.onclick = function () { setPalette(i.getAttribute("data-p")); }; });
}
function setTheme(v) {
 document.documentElement.dataset.theme = v;
 SELF_PROPS[2].value = (v === "dark");
 log2both(tt("toast.theme") + (v === "dark" ? tt("theme.dark") : tt("theme.light")));
}
function toggleDark() { setTheme(document.documentElement.dataset.theme === "dark" ? "light" : "dark"); }
function setDensity(v) {
 document.documentElement.dataset.density = v;
 SELF_PROPS[0].value = (v === "compact" ? "紧凑" : "舒适");
 log2both(tt("toast.density") + (v === "compact" ? tt("density.compact") : tt("density.cozy")));
}
function setLang(l) {
 LANG = l;
 document.documentElement.lang = l;
 SELF_PROPS[3].value = (l === "en-US" ? "English" : "简体中文");
 document.querySelectorAll("[data-i18n]").forEach(function (n) { n.textContent = tt(n.dataset.i18n); });
 var cp = $("cpmQ"); if (cp) cp.placeholder = tt("cmd.ph");
 renderRTabs(); renderTree(); renderDocTabs();
 if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
 renderPGrid(ACTIVE_DOC);
 log2both(tt("toast.lang") + l);
}
function togglePanel(which) {
 var key = which === "L" ? "hideL" : which === "R" ? "hideR" : "hideLog";
 document.body.classList.toggle(key);
 var hidden = document.body.classList.contains(key);
 var label = which === "L" ? "导航树" : which === "R" ? "属性" : "日志";
 log2both(label + "面板：" + (hidden ? "隐藏" : "显示"));
 if (ACTIVE_DOC === "flow-docs") renderDoc("flow-docs");
 return hidden;
}
var LAYOUT_KEY = "codewf-avalonia-demo-layout";
function saveLayout() {
 var st = {
  palette: document.documentElement.dataset.palette, theme: document.documentElement.dataset.theme,
  density: document.documentElement.dataset.density, lang: LANG, tab: curTab, densityLabel: SELF_PROPS[0].value,
  hideL: document.body.classList.contains("hideL"), hideR: document.body.classList.contains("hideR"),
  hideLog: document.body.classList.contains("hideLog"), docs: OPEN_DOCS.slice(0, 8), active: ACTIVE_DOC
 };
 var ok = true;
 try { localStorage.setItem(LAYOUT_KEY, JSON.stringify(st)); } catch (e) { ok = false; }
 notify(ok ? "ok" : "warn", ok ? tt("nc.ok") : tt("nc.warn"), ok ? tt("toast.saved") : "浏览器本地存储不可用（file:// 下部分浏览器会禁用）");
 log2both(tt("toast.saved"));
}
function loadLayout() {
 var raw = null;
 try { raw = localStorage.getItem(LAYOUT_KEY); } catch (e) { return false; }
 if (!raw) return false;
 try {
  var st = JSON.parse(raw);
  if (st.palette) setPalette(st.palette);
  if (st.theme) setTheme(st.theme);
  if (st.density) setDensity(st.density);
  ["hideL", "hideR", "hideLog"].forEach(function (k) { document.body.classList.toggle(k, !!st[k]); });
  if (st.tab) curTab = st.tab;
  if (st.docs && st.docs.length) {
   OPEN_DOCS = st.docs.filter(function (d) { return !!PROTO[d]; });
   ACTIVE_DOC = OPEN_DOCS.indexOf(st.active) >= 0 ? st.active : OPEN_DOCS[OPEN_DOCS.length - 1];
  }
  if (st.lang) setLang(st.lang);
  return true;
 } catch (e) { return false; }
}
window.layoutResetAsk = function () {
 confirmDlg(tt("dlg.reset.t"), tt("dlg.reset.d"), function () {
  document.body.classList.remove("hideL", "hideR", "hideLog", "rmn");
  setDensity("cozy"); setPalette("dotnet"); setTheme("dark");
  try { localStorage.removeItem(LAYOUT_KEY); } catch (e) { }
  notify("warn", tt("nc.warn"), tt("toast.reset"));
  log2both(tt("toast.reset"), "warn");
  if (ACTIVE_DOC) renderDoc(ACTIVE_DOC); else renderEmptyDoc();
  renderPGrid(ACTIVE_DOC);
 });
};
window.saveLayout = saveLayout;
window.togglePanel = togglePanel;
window.setLang = setLang;

/* ═══ Guide 引导（5 步，可跳过） ═══ */
var gSteps = [], gIdx = 0;
function buildDefaultGuide() {
 return [{ t: tt("g.1.t"), d: tt("g.1.d"), sel: ".rtabs" },
  { t: tt("g.2.t"), d: tt("g.2.d"), sel: ".p-nav" },
  { t: tt("g.3.t"), d: tt("g.3.d"), sel: ".p-doc" },
  { t: tt("g.4.t"), d: tt("g.4.d"), sel: ".p-props" },
  { t: tt("g.5.t"), d: tt("g.5.d"), sel: ".p-log" }];
}
function guideOpen() { gSteps = buildDefaultGuide(); gIdx = 0; $("gMask").classList.add("on"); $("gTip").classList.add("on"); showStep(); }
window.guideOpen = guideOpen;
window.guideGo = function (steps) { gSteps = steps; gIdx = 0; $("gMask").classList.add("on"); $("gTip").classList.add("on"); showStep(); };
function showStep() {
 var s = gSteps[gIdx];
 if (!s) { gCloseGuide(); return; }
 var el = document.querySelector(s.sel);
 if (!el) { gIdx++; showStep(); return; }
 var r = el.getBoundingClientRect(), sp = $("gSpot");
 sp.style.display = "block";
 sp.style.left = (r.left - 6) + "px"; sp.style.top = (r.top - 6) + "px";
 sp.style.width = (r.width + 12) + "px"; sp.style.height = (r.height + 12) + "px";
 var tip = $("gTip");
 tip.style.display = "block";
 tip.classList.remove("on"); void tip.offsetWidth; tip.classList.add("on");
 var tw = tip.offsetWidth || 300;
 var tx = clamp(r.left, 10, Math.max(10, innerWidth - tw - 10));
 var ty = r.bottom + 12;
 if (ty + 170 > innerHeight) ty = Math.max(10, r.top - 170);
 tip.style.left = tx + "px"; tip.style.top = ty + "px";
 $("gT").textContent = s.t; $("gD").textContent = s.d;
 $("gN").textContent = (gIdx + 1) + " / " + gSteps.length;
 $("gPrev").textContent = tt("g.prev"); $("gNext").textContent = tt("g.next");
 $("gSkip").textContent = tt("g.skip"); $("gClose").textContent = tt("g.close");
 log2both("引导：" + s.t);
}
function gNextStep() { gIdx++; if (gIdx >= gSteps.length) gCloseGuide(); else showStep(); }
function gPrevStep() { gIdx = Math.max(0, gIdx - 1); showStep(); }
function gCloseGuide() { $("gMask").classList.remove("on"); $("gTip").classList.remove("on"); $("gSpot").style.display = "none"; $("gTip").style.display = "none"; }
window.gCloseGuide = gCloseGuide;

/* ═══ 命令面板（Ctrl+K） ═══ */
var cpmAll = [], cpmIdx = 0;
function cmdList() {
 var list = allPageIds().map(function (id) {
  return {
   label: pt(id), group: tt("cmd.pages") + " · " + docCrumb(id), icon: TAB_ICON[id] || "◆",
   run: function () { curTab = (groupOf(id) || { id: "home" }).id; renderRTabs(); renderTree(); openDoc(id); }
  };
 });
 var C = [
  { label: "切换深色 / 浅色", icon: "◐", run: toggleDark },
  { label: "切换语言 中 / EN", icon: "文A", run: function () { setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); } },
  { label: "切换密度 舒适 / 紧凑", icon: "▤", run: function () { setDensity(document.documentElement.dataset.density === "compact" ? "cozy" : "compact"); } },
  { label: "保存布局", icon: "💾", run: saveLayout },
  { label: "重置布局", icon: "↺", run: function () { layoutResetAsk(); } },
  { label: "切换日志面板", icon: "▤", run: function () { togglePanel("Log"); } },
  { label: "切换导航树", icon: "◈", run: function () { togglePanel("L"); } },
  { label: "切换属性面板", icon: "⚙", run: function () { togglePanel("R"); } },
  { label: "关闭全部文档", icon: "✕", run: function () { closeAllDocsAsk(); } },
  { label: "打开引导", icon: "❔", run: guideOpen }
 ];
 C.forEach(function (c) { c.group = tt("cmd.cmds"); list.push(c); });
 var palLabels = SELF_PROPS.filter(function (p) { return p.id === "appearance.palette"; })[0].options;
 PAL_LIST.forEach(function (p, i) {
  list.push({ label: tt("toast.palette") + palLabels[i], group: tt("cmd.cmds"), icon: "🎨", run: function () { setPalette(p); } });
 });
 return list;
}
function cpmOpen() { $("cpm").classList.add("on"); $("cpmQ").value = ""; cpmIdx = 0; cpmDraw(); $("cpmQ").focus(); }
function cpmClose() { $("cpm").classList.remove("on"); }
function cpmDraw() {
 var q = ($("cpmQ").value || "").toLowerCase();
 cpmAll = cmdList().filter(function (c) {
  return !q || c.label.toLowerCase().indexOf(q) >= 0 || c.group.toLowerCase().indexOf(q) >= 0;
 });
 if (cpmIdx >= cpmAll.length) cpmIdx = 0;
 $("cpmLs").innerHTML = cpmAll.length ? cpmAll.map(function (c, i) {
  return '<div class="it' + (i === cpmIdx ? " on" : "") + '" data-i="' + i + '"><span>' + c.icon + "</span><span>" +
   esc(c.label) + '</span><span class="g">' + esc(c.group) + "</span></div>";
 }).join("") : '<div class="empty"><div class="et">无匹配</div></div>';
 document.querySelectorAll("#cpmLs .it").forEach(function (n) { n.onclick = function () { cpmIdx = +n.dataset.i; cpmRun(); }; });
}
function cpmMove(d) {
 if (!cpmAll.length) return;
 cpmIdx = (cpmIdx + d + cpmAll.length) % cpmAll.length;
 cpmDraw();
 var on = document.querySelector("#cpmLs .it.on");
 if (on) on.scrollIntoView({ block: "nearest" });
}
function cpmRun() {
 var c = cpmAll[cpmIdx]; if (!c) return;
 cpmClose();
 log2both("命令面板执行：" + c.label);
 guard(c.run, "cmd");
}

/* ═══ 事件绑定 ═══ */
function bindShell() {
 $("rTabsBar").ondblclick = function (e) { if (e.target.classList.contains("t2")) toggleRibbon(); };
 $("palSel").onchange = function () { setPalette(this.value); };
 $("darkBtn").onclick = toggleDark;
 $("darkBtn2").onclick = toggleDark;
 $("langBtn").onclick = function () { setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); };
 $("langBtn2").onclick = $("langBtn").onclick;
 $("qkGuide").onclick = guideOpen;
 $("qkCmd").onclick = cpmOpen;
 $("qkDensity").onclick = function () { setDensity(document.documentElement.dataset.density === "compact" ? "cozy" : "compact"); };
 $("qkSave").onclick = saveLayout;
 $("qkReset").onclick = function () { layoutResetAsk(); };
 $("qkPalette").onclick = function () {
  var i = PAL_LIST.indexOf(document.documentElement.dataset.palette);
  setPalette(PAL_LIST[(i + 1) % PAL_LIST.length]);
 };
 $("hideNav").onclick = function () { togglePanel("L"); };
 $("hideProp2").onclick = function () { togglePanel("R"); };
 $("hideLogBtn").onclick = function () { togglePanel("Log"); };
 $("logToggleBtn").onclick = function () { togglePanel("Log"); };
 $("dkNav").onclick = $("hideNav").onclick;
 $("dkProps").onclick = $("hideProp2").onclick;
 $("dkLog").onclick = $("hideLogBtn").onclick;
 $("dkMax").onclick = function () { document.body.classList.add("hideL", "hideR"); log2both("文档区最大化"); };
 $("dkReset").onclick = function () { layoutResetAsk(); };
 $("openLogDir").onclick = function () { log2both("打开日志目录（跨平台内联：Explorer / xdg-open / open）", "warn"); };
 $("clrLogBtn").onclick = clearLogAsk;
 $("cpmQ").oninput = cpmDraw;
 $("cpmQ").onkeydown = function (e) {
  if (e.key === "ArrowDown") { e.preventDefault(); cpmMove(1); }
  else if (e.key === "ArrowUp") { e.preventDefault(); cpmMove(-1); }
  else if (e.key === "Enter") { e.preventDefault(); cpmRun(); }
  else if (e.key === "Escape") { cpmClose(); }
 };
 $("cpm").onclick = function (e) { if (e.target === $("cpm")) cpmClose(); };
 $("dlgYes").onclick = function () { var f = _dlgYes; closeDlg(); if (f) guard(f, "dlg"); };
 $("dlgNo").onclick = closeDlg;
 $("dlgm").onclick = function (e) { if (e.target === $("dlgm")) closeDlg(); };
 $("gNext").onclick = gNextStep;
 $("gPrev").onclick = gPrevStep;
 $("gSkip").onclick = function () { gCloseGuide(); log2both("引导已跳过"); };
 $("gClose").onclick = gCloseGuide;
 $("gMask").onclick = gCloseGuide;
 document.addEventListener("keydown", function (e) {
  var k = (e.key || "").toLowerCase();
  var mod = e.ctrlKey || e.metaKey;
  if (mod && k === "k") { e.preventDefault(); $("cpm").classList.contains("on") ? cpmClose() : cpmOpen(); return; }
  if (mod && k === "d") { e.preventDefault(); toggleDark(); return; }
  if (mod && k === "l") { e.preventDefault(); setLang(LANG === "zh-CN" ? "en-US" : "zh-CN"); return; }
  if (mod && k === "s") { e.preventDefault(); saveLayout(); return; }
  if (e.key === "F1") { e.preventDefault(); guideOpen(); return; }
  if (e.key === "Escape") {
   if ($("cpm").classList.contains("on")) cpmClose();
   else if ($("dlgm").classList.contains("on")) closeDlg();
   else gCloseGuide();
  }
 });
}
window.clearLogAsk = function () {
 confirmDlg(tt("dlg.del.t"), "清空底部日志面板？该操作不影响日志文件。", function () {
  $("logP").innerHTML = "";
  notify("warn", tt("nc.warn"), "日志面板已清空");
  log2both("日志面板已清空", "warn");
 });
};

/* ═══ 启动 ═══ */
function boot() {
 bindShell();
 expose();
 var restored = false;
 guard(function () { restored = loadLayout(); }, "loadLayout");
 if (!restored) renderPalDots();
 renderRTabs();
 renderTree();
 renderPalDots();
 if (OPEN_DOCS.length) { renderDocTabs(); renderDoc(ACTIVE_DOC); }
 else openDoc("welcome");
 renderPGrid(ACTIVE_DOC);
 dlog("演示台启动：Lang(I18n) + Logger 已初始化", "info");
 dlog("信息架构：3 个场景工作流（写作 / 表数据 / 多文档）+ 3 个组件库分区（输入 / 呈现 / 基础设施）", "info");
 if (restored) dlog("已从浏览器本地恢复上次布局", "info");
}
boot();
