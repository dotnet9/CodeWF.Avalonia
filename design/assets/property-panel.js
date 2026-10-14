/* 属性表格交互；图标使用 Semi.Avalonia 的 MIT 图标资源。 */
"use strict";
(function () {
  var states = {}, panel = document.getElementById("pGrid"), original = renderPGrid;
  var toolbar = document.createElement("div");
  toolbar.id = "propertyToolbar";
  toolbar.className = "property-toolbar";
  toolbar.innerHTML = '<div class="property-modes"><button type="button" data-mode="categorized"></button><button type="button" data-mode="alphabetical"></button></div><input type="search" class="property-search">';
  panel.before(toolbar);
  // SemiIconListView / SemiIconSort，来源：irihitech/Semi.Avalonia（MIT）。
  var paths = {
    categorized: "M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z",
    alphabetical: "M6.45 8.34l4.8-6.48a1 1 0 0 1 1.5 0l4.8 6.48A1 1 0 0 1 16.8 10H7.2a1 1 0 0 1-.75-1.66Z M17.55 15.66l-4.8 5.48a1 1 0 0 1-1.5 0l-4.8-5.48A1 1 0 0 1 7.2 14h9.6a1 1 0 0 1 .75 1.66Z"
  };
  toolbar.querySelectorAll("button").forEach(function (button) {
    button.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="' + paths[button.dataset.mode] + '"></path></svg>';
  });
  function drag(handle, axis, apply) {
    handle.onpointerdown = function (event) {
      if (event.button !== 0) return;
      event.preventDefault(); event.stopPropagation();
      var start = axis === "x" ? event.clientX : event.clientY;
      handle.setPointerCapture(event.pointerId);
      var update = apply();
      handle.onpointermove = function (move) { update((axis === "x" ? move.clientX : move.clientY) - start); };
      handle.onpointerup = handle.onpointercancel = function (end) {
        handle.onpointermove = null;
        if (handle.hasPointerCapture(end.pointerId)) handle.releasePointerCapture(end.pointerId);
      };
      handle.onlostpointercapture = function () { handle.onpointermove = null; };
    };
  }
  renderPGrid = function (id) {
    original(id);
    var state = states[id] || (states[id] = { width:118, heights:{}, mode:"categorized", query:"" });
    var english = LANG === "en-US", nodes = Array.from(panel.children), rows = nodes.filter(node => node.matches(".rw"));
    var search = toolbar.querySelector("input");
    search.placeholder = english ? "Search properties" : "搜索属性";
    search.setAttribute("aria-label", search.placeholder); search.value = state.query;
    function setWidth(value) {
      state.width = Math.max(60, Math.min(value, panel.clientWidth - 70));
      panel.style.setProperty("--property-name-width", state.width + "px");
    }
    setWidth(state.width);
    function applyView() {
      nodes.forEach(node => panel.appendChild(node));
      if (state.mode === "alphabetical") rows.slice().sort((a,b) => a.querySelector("label,.nm").textContent.localeCompare(b.querySelector("label,.nm").textContent, LANG)).forEach(row => panel.appendChild(row));
      rows.forEach(function(row) { row.hidden = !row.textContent.toLocaleLowerCase(LANG).includes(state.query.toLocaleLowerCase(LANG)); });
      nodes.filter(node => node.matches(".cat")).forEach(function(category) {
        var index = nodes.indexOf(category), end = nodes.findIndex((node,i) => i > index && node.matches(".cat"));
        category.hidden = state.mode === "alphabetical" || !nodes.slice(index + 1, end < 0 ? undefined : end).some(node => node.matches(".rw") && !node.hidden);
      });
      toolbar.querySelectorAll("button").forEach(button => button.setAttribute("aria-pressed", String(button.dataset.mode === state.mode)));
    }
    toolbar.querySelectorAll("button").forEach(function(button) {
      button.title = button.dataset.mode === "categorized" ? (english ? "Categorized" : "分类视图") : (english ? "Alphabetical" : "按字母排序");
      button.setAttribute("aria-label", button.title);
      button.onclick = function () { state.mode = button.dataset.mode; applyView(); };
    });
    search.oninput = function () { state.query = search.value; applyView(); };
    rows.forEach(function(row, index) {
      var key = row.querySelector("[data-id]")?.dataset.id || String(index);
      row.dataset.propertyKey = key;
      if (state.heights[key]) row.style.height = state.heights[key] + "px";
      var column = document.createElement("div"), line = document.createElement("div");
      column.className = "property-column-handle"; line.className = "property-row-handle";
      [column,line].forEach(function(handle) { handle.tabIndex = 0; handle.setAttribute("role", "separator"); });
      column.setAttribute("aria-orientation", "vertical"); line.setAttribute("aria-orientation", "horizontal");
      column.setAttribute("aria-label", english ? "Resize property columns" : "调整属性列宽");
      line.setAttribute("aria-label", english ? "Resize property row" : "调整属性行高");
      row.append(column,line);
      drag(column, "x", function() { var start = state.width; return delta => setWidth(start + delta); });
      function setHeight(height) { state.heights[key] = Math.max(row.querySelector("textarea") ? 74 : 31, Math.min(240, height)); row.style.height = state.heights[key] + "px"; }
      drag(line, "y", function() { var start = row.getBoundingClientRect().height; return delta => setHeight(start + delta); });
      line.ondblclick = function() { delete state.heights[key]; row.style.height = ""; };
      column.onkeydown = function(event) { if (["ArrowLeft","ArrowRight"].includes(event.key)) { event.preventDefault(); setWidth(state.width + (event.key === "ArrowLeft" ? -8 : 8)); } };
      line.onkeydown = function(event) { if (["ArrowUp","ArrowDown"].includes(event.key)) { event.preventDefault(); setHeight(row.getBoundingClientRect().height + (event.key === "ArrowUp" ? -4 : 4)); } };
    });
    applyView();
  };
  renderPGrid(ACTIVE_DOC);
})();
