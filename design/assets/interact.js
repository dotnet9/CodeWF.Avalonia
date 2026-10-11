// CodeWF.Avalonia.Demo 界面交互逻辑

// ═══ 工具 ═══
var $ = function(id) { return document.getElementById(id); };
var $$ = function(sel, ctx) { return Array.from((ctx||document).querySelectorAll(sel)); };

// ═══ 状态 ═══
var state = {
  mode: "categorized",  // categorized | alphabetical
  searchQ: "",
  selectedId: null,
  collapsedGroups: new Set(),
  descVisible: true,
  nameWidth: 148
};

// ═══ 属性项模型（简化版，用于原型演示） ═══
var PROPERTY_ITEMS = [
  { cat:"外观", id:"name", label:"名称", kind:"text", value:"阀门-01" },
  { cat:"外观", id:"visible", label:"可见", kind:"boolean", value:true },
  { cat:"外观", id:"fill", label:"填充颜色", kind:"color", value:"#3B82F6" },
  { cat:"外观", id:"opacity", label:"不透明度", kind:"slider", value:72 },
  { cat:"行为", id:"mode", label:"参与计算", kind:"combo", options:["参与计算","仅预览","不参与"], value:"仅预览" },
  { cat:"行为", id:"trigger", label:"触发方式", kind:"radio", options:["手动","自动","定时"], value:"自动" },
  { cat:"行为", id:"note", label:"便签", kind:"multiline", value:"定期检查入口压力。" },
  { cat:"文件", id:"path", label:"完整路径", kind:"readonly", value:"D:\\models\\plant.sfc" },
  { cat:"文件", id:"encoding", label:"编码", kind:"combo", options:["UTF-8","UTF-16","GB2312"], value:"UTF-8" },
  { cat:"文件", id:"export", label:"导出目录", kind:"text", value:"D:\\exports" }
];

// ═══ 初始化 ═══
document.addEventListener("DOMContentLoaded", function() {
  initThemeToggle();
  initRibbonTabs();
  initTree();
  initSearch();
  initPaletteChips();
  showPage("home");
  dlog("演示台就绪");
});

function $(id) { return document.getElementById(id); }
function $$$(sel) { return Array.from(document.querySelectorAll(sel)); }
function dlog(msg, cls) {
  var box = $("logBox"); if (!box) return;
  var e = document.createElement("div");
  e.className = "log-line"; if (cls) e.classList.add(cls);
  e.textContent = "[" + stamp() + "] " + msg;
  box.appendChild(e);
  while (box.children.length > 50) box.removeChild(box.firstChild);
  box.scrollTop = box.scrollHeight;
}
function stamp() { var d = new Date(); return ("0"+d.getHours()).slice(-2)+":"+("0"+d.getMinutes()).slice(-2)+":"+("0"+d.getSeconds()).slice(-2); }

// ═══ 主题切换 ═══
function initThemeToggle() {
  var btn = $("themeToggle");
  if (!btn) return;
  btn.addEventListener("click", function() {
    var html = document.documentElement;
    html.dataset.theme = html.dataset.theme === "dark" ? "light" : "dark";
    dlog("主题 = " + html.dataset.theme);
  });
}

// ═══ Ribbon 页签切换 ═══
function initRibbonTabs() {
  $$$(".rbn-tab").forEach(function(tab) {
    tab.addEventListener("click", function() {
      var mode = tab.dataset.mode;
      setViewMode(mode);
      // 高亮当前页签
      $$$(".rbn-tab").forEach(function(t) { t.classList.toggle("on", t === tab); });
    });
  });
}

// ═══ 搜索过滤 ═══
function initSearch() {
  var input = $("searchInput");
  if (!input) return;
  input.addEventListener("input", function() {
    filterTree(input.value.trim().toLowerCase());
  });
}

function filterTree(query) {
  $$$("#treeBody .tree-item").forEach(function(item) {
    var text = item.dataset.searchText || "";
    item.style.display = (!query || text.includes(query)) ? "" : "none";
  });
}

// ═══ 视图模式 ═══
function setViewMode(mode) {
  state.mode = mode;
  renderTree();
  dlog("视图 = " + mode);
}

// ═══ Tree 渲染 ═══
function renderTree() {
  var body = $("treeBody"); if (!body) return;
  body.innerHTML = "";
  PROPERTY_ITEMS.forEach(function(item) {
    var row = document.createElement("div");
    row.className = "tree-item";
    row.dataset.searchText = (item.label + " " + item.value).toLowerCase();
    row.textContent = item.label;
    row.addEventListener("click", function() { selectItem(item); });
    body.appendChild(row);
  });
}

function selectItem(item) {
  $$$("#treeBody .tree-item").forEach(function(r) { r.classList.toggle("selected", r.dataset.id === item.id); });
  var descName = $("descName"), descText = $("descText");
  if (descName) descName.textContent = item.label;
  if (descText) descText.textContent = item.desc || "";
}
