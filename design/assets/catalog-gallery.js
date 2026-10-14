/* The API catalog is generated from design/catalog.json; this is the browser prototype adapter. */
"use strict";
(function () {
  var data = window.CodeWFDemoCatalog, entries = {}, toolkit = document.body.dataset.product === "toolkit";
  function text(key) { return (window.CodeWFCatalogText || {})[LANG === "en-US" ? "en-US" : "zh-CN"][key] || key; }
  function title(entry) { return entry[LANG === "en-US" ? "titleEn" : "titleZh"]; }
  function description(entry) {
    var key = entry.titleKey.split(".").pop();
    return ((window.CodeWFCatalogDescriptions || {})[LANG] || {})[key]?.Desc || "";
  }
  CATALOG = data.map(function (group) {
    return { id:group.id, icon:"◆", items:group.items.map(function (entry) { entries[entry.prototypeId] = entry; return { id:entry.prototypeId }; }) };
  });
  tabName = function (id) { var group = data.find(function (item) { return item.id === id; }); return group ? title(group) : id; };
  pt = function (id) { return entries[id] ? title(entries[id]) : id; };
  pa = function (id) { return entries[id] ? entries[id].api : ""; };
  data.forEach(function (group) {
    I18N["zh-CN"]["tabs."+group.id] = group.titleZh;
    I18N["en-US"]["tabs."+group.id] = group.titleEn;
  });
  renderRBody = function () {
    var group = CATALOG.find(function (item) { return item.id === curTab; });
    $("rBody").innerHTML = group ? '<div class="grp"><div class="cmds">'+group.items.map(cmdBtn).join("")+'</div></div>' : "";
    $("rBody").querySelectorAll(".cbtn").forEach(function (button) { button.onclick = function () { openDoc(button.dataset.p); }; });
  };
  var originalOpenDoc = openDoc;
  openDoc = function (id) {
    var group = groupOf(id);
    if (group) { curTab = group.id; renderRTabs(); }
    originalOpenDoc(id);
  };
  window.openDoc = openDoc;
  function apiMeta(entry) {
    return { t:title(entry), d:entry.api ? entry.package : text("AboutDesc"), f:[] };
  }
  function generic(entry) {
    return { zh:apiMeta(entry), en:apiMeta(entry), cs:entry.code || entry.api, props:[], render:function (el) {
      el.innerHTML = '<div class="demo"><h4>'+esc(text("Input"))+'</h4><textarea class="catalog-input">'+esc(entry.input || "")+'</textarea><button class="btn pri catalog-run">'+esc(text("Run"))+'</button></div><div class="demo"><h4>'+esc(text("Output"))+'</h4><pre class="catalog-result">'+esc(text("Ready"))+'</pre></div>';
      el.querySelector(".catalog-run").onclick = function () {
        var input = el.querySelector("textarea").value, result = input;
        if (entry.api === "DoubleExtensions") result = Number(input) >= 1048576 ? (Number(input)/1048576).toFixed(2)+" MB" : (Number(input)/1024).toFixed(2)+" KB";
        else if (entry.api === "JsonExtensions") result = JSON.stringify({Name:input,Age:20,City:"CodeWF"},null,2);
        else if (entry.api === "YamlExtensions") result = "Name: "+JSON.stringify(input)+"\nAge: 20\nCity: CodeWF";
        else if (entry.api === "StringExtensions") result = input.split(",").filter(Boolean).join(" / ");
        else if (entry.api === "ObjectExtension") result = "IsNullOrEmpty = "+!input.trim();
        else if (entry.api === "DateTimeExtension") result = "seconds = "+Math.floor(Date.parse(input)/1000)+"\nmilliseconds = "+Date.parse(input);
        else if (entry.api === "IpHelper") { var parts = input.split(":"); result = "IP = "+parts[0]+"\nPort = "+parts[1]; }
        else result = entry.api+"\n"+input+"\n"+text("Preview");
        el.querySelector(".catalog-result").textContent = result;
        log2both(entry.api+": "+text("Run"),"info");
      };
    }};
  }
  if (!toolkit) {
    ["status-badge","status-card"].forEach(function (id) {
      var badge = id === "status-badge";
      REG(id,{zh:apiMeta(entries[id]),props:[],xaml:badge?'<codewf:StatusBadge Classes="Info" LeftText="Info" RightText="Ready" />':'<codewf:StatusCard Kind="Info" TopText="INFO" BottomText="Ready" />',render:function (el) {
        el.innerHTML = '<div class="demo"><h4>'+entries[id].api+'</h4><div class="row">'+["Debug","Info","Warn","Error","Fatal"].map(function(level){return '<div class="catalog-status '+(badge?'badge':'card')+'" data-level="'+level+'"><strong>'+level+'</strong><span>Ready</span></div>';}).join("")+'</div></div>';
      }});
    });
    ["if","switch"].forEach(function(id) { var entry=entries[id]; REG(id,{zh:apiMeta(entry),xaml:window.CodeWFCatalogDescriptions[LANG][entry.api].Xaml,props:[],render:function(el){
      el.innerHTML='<div class="demo"><h4>'+entry.api+'</h4><select><option>Idle</option><option>Busy</option><option>Done</option></select><p class="catalog-result">Idle</p></div>';
      el.querySelector('select').onchange=function(event){el.querySelector('p').textContent=id==='if'?(event.target.value==='Busy'?'True':'False'):event.target.value;};
    }}); });
    REG('treegrid',{zh:apiMeta(entries.treegrid),cs:'treeDataGrid.EnableDefaults(source);',props:[],render:PROTO.datagrid.render});
    REG('titlebar',{zh:apiMeta(entries.titlebar),xaml:'<codewf:CodeWFTitleBar Title="{Binding Title}" />',props:[],render:function(el){
      el.innerHTML='<div class="demo"><h4>Title</h4><input value="CodeWFTitleBar"><div class="catalog-titlebar"><span>CodeWF</span><strong>CodeWFTitleBar</strong><span>.NET</span></div></div>';
      el.querySelector('input').oninput=function(e){el.querySelector('strong').textContent=e.target.value;};
    }});
    REG('diagonal-line',{zh:apiMeta(entries['diagonal-line']),xaml:'<codewf:LeftTop2RightBottomLine BorderThickness="{Binding Thickness}" />',props:[],render:function(el){
      el.innerHTML='<div class="demo"><h4>BorderThickness</h4><input type="range" min="1" max="12" value="3"><svg class="catalog-line" viewBox="0 0 280 180"><line x1="0" y1="0" x2="280" y2="180" stroke="currentColor" stroke-width="3" /></svg></div>';
      el.querySelector('input').oninput=function(e){el.querySelector('line').setAttribute('stroke-width',e.target.value);};
    }});
    REG('notifications',{zh:apiMeta(entries.notifications),xaml:'<Application codewf:LogNotifications.Mode="InApp" />',props:[],render:function(el){
      el.innerHTML='<div class="demo"><h4>LogNotifications</h4><button class="btn pri">'+esc(text('Notifications'))+'</button></div>';
      el.querySelector('button').onclick=function(){notify('warn','LogNotifications',text('Notification'));};
    }});
  }
  Object.keys(entries).forEach(function(id) { if (!PROTO[id] || entries[id].page.indexOf("HelperDemoPage") === 0) REG(id,generic(entries[id])); });
  REG('about',{zh:apiMeta(entries.about),props:[],render:function(el){
    el.innerHTML='<div class="demo"><h4>'+esc(text('About'))+'</h4><p>'+esc(text('AboutDesc'))+'</p><pre>dotnet add package CodeWF.'+(toolkit?'Toolkit.Core':'Avalonia.Controls')+'</pre><p>MIT · .NET 10 · Avalonia 12</p></div>';
  }});
  var originalInto=renderInto;
  renderInto=function(id,el){
    originalInto(id,el);
    if(id!=='welcome'||!el)return;
    var index=document.createElement('section'); index.className='catalog-index';
    index.innerHTML='<h2>'+esc(text('Title'))+'</h2><p>'+esc(text('Intro'))+'</p>'+data.filter(function(g){return g.id!=='home'&&g.id!=='examples';}).map(function(g){return '<div class="catalog-category"><h3>'+esc(title(g))+'</h3><div>'+g.items.map(function(entry){return '<button type="button" class="btn" data-api="'+entry.prototypeId+'">'+esc(title(entry))+'</button>';}).join('')+'</div></div>';}).join('');
    index.querySelectorAll('[data-api]').forEach(function(button){button.onclick=function(){openDoc(button.dataset.api);};});
    var labs=el.querySelector('.studio-stage'), capabilities=el.querySelector('.studio-capabilities');
    var anchor=labs||el.querySelector('.studio-install');
    if(anchor)anchor.before(index);else el.appendChild(index);
    var details=document.createElement('details');details.className='catalog-experiments';details.innerHTML='<summary>'+esc(text('Examples'))+'</summary>';
    if(labs){labs.before(details);details.appendChild(labs);}
    if(capabilities)details.appendChild(capabilities); var caption=el.querySelector('.studio-section-label'); if(caption)caption.remove();
    el.querySelectorAll('[data-open="flow-write"]').forEach(function(button){button.onclick=function(){openDoc('md-viewer');};});
    el.querySelectorAll('[data-open="flow-data"]').forEach(function(button){button.onclick=function(){openDoc('datagrid');};});
    el.querySelectorAll('[data-open="flow-docs"]').forEach(function(button){button.onclick=function(){openDoc('dock');};});
  };
  renderDoc=function(id){
    var pg=PROTO[id],entry=entries[id];if(!pg||!entry)return;
    var hasSource = Boolean(pg.xaml || pg.cs || entry.code);
    var head=id==='welcome'?'':'<header class="catalog-header"><div><h1>'+esc(pt(id))+'</h1><p>'+esc(entry.package||text('AboutDesc'))+'</p><p class="catalog-description">'+esc(description(entry))+'</p></div>'+(hasSource?'<button class="btn" id="catalog-source-toggle">'+esc(text('Source'))+'</button>':'')+'</header>';
    $('dBody').innerHTML=head+'<div id="seg-'+id+'"></div>'+(id==='welcome'?'':'<div class="catalog-source" hidden><div class="smt"><button data-source="xaml">XAML</button><button data-source="cs">C#</button></div><pre></pre></div>');
    renderInto(id,$('seg-'+id));
    if(pg.after)guard(function(){pg.after();},'after:'+id);
    var source=$('dBody').querySelector('.catalog-source');
    if(source){
      if (!hasSource) source.remove();
      else {
      function showCode(language){source.querySelector('pre').textContent=pg[language]||entry.code||entry.api;}
      source.querySelector('[data-source="xaml"]').hidden = !pg.xaml;
      source.querySelector('[data-source="cs"]').hidden = !(pg.cs || entry.code);
      showCode(pg.cs || entry.code ? 'cs' : 'xaml');
      $('catalog-source-toggle').onclick=function(){source.hidden=!source.hidden;};
      source.querySelectorAll('button').forEach(function(button){button.onclick=function(){showCode(button.dataset.source);};});
      }
    }
    renderPGrid(id);
    if($('stWhere'))$('stWhere').textContent=docCrumb(id);
    if(!matchMedia('(prefers-reduced-motion: reduce)').matches)$('seg-'+id).animate([{opacity:.4,transform:'translateY(5px)'},{opacity:1,transform:'none'}],{duration:220,easing:'ease-out'});
  };
  PROTO.welcome.zh.t=pt('welcome');
  curTab='home';
  renderRTabs();renderTree();renderDocTabs();
  if(ACTIVE_DOC&&entries[ACTIVE_DOC])renderDoc(ACTIVE_DOC);else openDoc('welcome');
})();
