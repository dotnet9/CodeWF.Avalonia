# -*- coding: utf-8 -*-
"""
生成 CodeWF.Avalonia.Demo 原型 v4
Office Ribbon + Dock 工作区 + PropertyGrid 联动
"""
import os, textwrap

DIR = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(DIR, "..", "design", "CodeWF.Avalonia.Demo原型.html")

# ═══════════════════════════════════════════════════════════════
# CSS
# ═══════════════════════════════════════════════════════════════
CSS = """
:root{--pri:#0064fa;--pri-lt:rgba(0,100,250,.08);--bg:#fff;--bg2:#f7f8fa;--bg3:#f2f3f5;--tx:#1c1f23;--tx2:#646a73;--tx3:#8f959e;--bd:#dee0e3;--bd2:#eceef1;--hov:#f2f3f5;--r:6px;--sh:0 4px 12px rgba(0,0,0,.08)}
[data-theme=dark]{--bg:#17171a;--bg2:#202024;--bg3:#2a2a2e;--tx:#e8e8e8;--tx2:#a0a4ab;--tx3:#6e7278;--bd:#3a3a3f;--bd2:#2a2a2e;--hov:#2a2a2e;--sh:0 4px 12px rgba(0,0,0,.5)}
*{box-sizing:border-box;margin:0;padding:0}
html,body{height:100%;overflow:hidden}
body{font:13px/1.6 "Segoe UI","PingFang SC","Microsoft YaHei",sans-serif;color:var(--tx);background:var(--bg);display:grid;grid-template-rows:36px auto 1fr 26px}
button{font:inherit;color:inherit}
.tbr{display:flex;align-items:center;height:36px;padding:0 0 0 12px;background:var(--bg2);border-bottom:1px solid var(--bd);user-select:none}
.tbr .lg{font-weight:700;color:var(--pri);margin-right:8px}
.tbr .tt{font-size:12px;color:var(--tx2);flex:1}
.tbr .wb{width:40px;height:36px;display:flex;align-items:center;justify-content:center;border:none;background:transparent;cursor:pointer;color:var(--tx2);font-size:13px}
.tbr .wb:hover{background:var(--hov)}
.rt{display:flex;align-items:center;height:30px;padding:0 6px;gap:2px}
.rt .tb2{padding:0 16px;height:26px;line-height:26px;cursor:pointer;border-radius:4px 4px 0 0;color:var(--tx2);user-select:none}
.rt .tb2:hover{background:var(--hov);color:var(--tx)}
.rt .tb2.on{color:var(--pri);font-weight:600;background:var(--bg)}
.rb{min-height:82px;background:var(--bg);border-bottom:1px solid var(--bd);display:flex;align-items:stretch;padding:6px 12px;overflow:hidden}
body.rbn-min .rb{display:none}
.rg{display:flex;flex-direction:column;padding:0 14px;border-right:1px solid var(--bd2);justify-content:space-between}
.rg:last-child{border-right:none}
.rg .cmds{display:flex;gap:2px;align-items:flex-start;flex:1;flex-wrap:wrap}
.rg .cmds.col{flex-direction:column}
.rg .glbl{text-align:center;font-size:10px;color:var(--tx3);padding-top:2px}
.rbn-big{display:flex;flex-direction:column;align-items:center;gap:3px;width:64px;padding:6px 2px;border:none;background:transparent;cursor:pointer;border-radius:var(--r);color:var(--tx)}
.rbn-big:hover{background:var(--hov)}
.rbn-big .ic{font-size:22px;line-height:1}
.rbn-big .lb{font-size:11px;text-align:center;line-height:1.2}
.rbn-sml{display:flex;align-items:center;gap:6px;width:100%;padding:3px 8px;border:none;background:transparent;cursor:pointer;border-radius:3px;color:var(--tx);font-size:12px;text-align:left;white-space:nowrap}
.rbn-sml:hover{background:var(--hov)}
.rbn-sml .ic{font-size:12px;width:14px;text-align:center;color:var(--pri);flex:none}
.wk{display:grid;grid-template-columns:220px 6px 1fr 6px 264px;grid-template-rows:28px 1fr 160px;grid-template-areas:"lt dt rt" "lf dc rg" "lf bt rg";height:calc(100vh - 152px);min-height:0}
.wk.no-lt{grid-template-columns:0 6px 1fr 6px 264px}
.wk.no-rt{grid-template-columns:220px 6px 1fr 6px 0}
.wk.no-bt{grid-template-rows:28px 1fr 0}
.wk.no-lt .plh,.wk.no-lt .plf,.wk.no-rt .prh,.wk.no-rt .prt,.wk.no-bt .pbh,.wk.no-bt .pbt{display:none!important}
.ph{display:flex;align-items:center;height:28px;padding:0 10px;background:var(--bg2);border-bottom:1px solid var(--bd);font-size:11px;font-weight:600;color:var(--tx2)}
.plh{grid-area:lt}.prh{grid-area:rt}.pbh{grid-area:bt}
.plf{grid-area:lf;overflow:auto}.prt{grid-area:rt;overflow:auto}.pbt{grid-area:bt;overflow:auto;background:var(--bg2)}
.dts{grid-area:dt;display:flex;align-items:flex-end;padding:4px 8px 0;gap:2px;background:var(--bg2);overflow-x:auto}
.dt{display:flex;align-items:center;gap:6px;padding:5px 12px;background:var(--bg);border:1px solid var(--bd);border-bottom:none;border-radius:6px 6px 0 0;cursor:pointer;font-size:12px;color:var(--tx2)}
.dt.on{color:var(--tx);font-weight:600;border-bottom:2px solid var(--pri)}
.dt .x{width:14px;height:14px;line-height:14px;text-align:center;border-radius:3px;font-size:11px;opacity:.5}
.dt .x:hover{background:var(--hov);opacity:1}
.dt-c{grid-area:dc;overflow:auto;background:var(--bg)}
.sb{display:flex;align-items:center;height:26px;padding:0 12px;gap:12px;background:var(--bg2);border-top:1px solid var(--bd);font-size:11px;color:var(--tx2)}
.sb .sp{flex:1}.sb span{cursor:default}
.pgp{padding:4px 8px 16px}
.pgs{font-size:10px;color:var(--pri);padding:8px 4px 2px;font-weight:700}
.prw{display:grid;grid-template-columns:96px 1fr;align-items:center;min-height:28px;padding:2px 4px;border-radius:3px}
.prw:hover{background:var(--hov)}
.prw label{font-size:11px;color:var(--tx2);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.prw .ctl{display:flex;align-items:center;gap:4px}
.pgp select,.pgp input[type=text]{width:100%;height:24px;font-size:11px;padding:0 6px;border:1px solid var(--bd);border-radius:3px;background:var(--bg);color:var(--tx);outline:none}
.pgp select:focus,.pgp input:focus{border-color:var(--pri)}
.pgp input[type=range]{width:100%;accent-color:var(--pri)}
.pgp input[type=checkbox]{accent-color:var(--pri)}
.pgp .rv{font-size:10px;color:var(--tx3);min-width:28px}
.chp{width:18px;height:18px;border-radius:3px;cursor:pointer;border:2px solid transparent}
.chp.on{border-color:var(--pri);box-shadow:0 0 0 1px var(--bg)}
.pgp-empty{padding:16px 12px;color:var(--tx3);font-size:11px;line-height:1.6}
.pg-body{padding:16px 20px 40px}
.pg-body .crumb{font-size:11px;color:var(--tx3);margin-bottom:4px}
.pg-body h1{font-size:18px;margin-bottom:4px}
.pg-body .dsc{color:var(--tx2);font-size:12px;margin-bottom:14px;line-height:1.6;max-width:680px}
.ftags{display:flex;gap:6px;flex-wrap:wrap;margin-bottom:14px}
.ftags span{font-size:10px;padding:1px 8px;border-radius:10px;background:var(--pri-lt);color:var(--pri)}
.dcard{border:1px solid var(--bd);border-radius:var(--r);overflow:hidden;margin-bottom:16px}
.dcard .dh{display:flex;align-items:center;padding:6px 12px;background:var(--bg2);border-bottom:1px solid var(--bd);font-size:11px;color:var(--tx2)}
.dcard .dh .sp{flex:1}
.dcard .db{padding:16px}
.cblk{background:var(--bg3);border:1px solid var(--bd2);border-radius:var(--r);padding:12px 14px;font:11px/1.6 Consolas,"Cascadia Code",monospace;white-space:pre;overflow-x:auto;color:var(--tx2)}
.stg{border:1px dashed var(--bd);border-radius:var(--r);background:var(--bg2);padding:16px}
.ltr{padding:6px 4px}
.ltr .grp{font-size:10px;color:var(--tx3);padding:8px 8px 3px;font-weight:600}
.ltr .nd{display:flex;align-items:center;gap:7px;padding:4px 8px;border-radius:4px;cursor:pointer}
.ltr .nd:hover{background:var(--hov)}
.ltr .nd.on{background:var(--pri-lt);color:var(--pri);font-weight:600}
.ltr .nd .ic{width:16px;text-align:center;flex:none}
.pal-chip{width:22px;height:22px;border-radius:4px;cursor:pointer;border:2px solid var(--bd);display:inline-flex;align-items:center;justify-content:center}
.pal-chip:hover{transform:scale(1.15)}
.pal-chip.on{border-color:var(--pri);box-shadow:0 0 0 2px var(--bg),0 0 0 3px var(--pri)}
.gdov{position:relative;min-height:220px;background:var(--bg2);border-radius:var(--r);overflow:hidden}
.gdov .und{padding:16px;opacity:.25}
.gdov .msk{position:absolute;inset:0;background:rgba(0,0,0,.4)}
.gdov .crd{position:absolute;width:250px;background:var(--bg);border:1px solid var(--bd);border-radius:8px;padding:12px;box-shadow:var(--sh);z-index:2}
.gdov .stp{font-size:10px;color:var(--tx3)}.gdov .gtt{font-weight:700;margin:2px 0}
.gdov .gds{font-size:11px;color:var(--tx2);margin-bottom:10px}
.gdov .gbt{display:flex;gap:6px;justify-content:flex-end}
.dkm{display:grid;grid-template-columns:160px 1fr 200px;grid-template-rows:28px 1fr 22px;height:260px;border:1px solid var(--bd);border-radius:var(--r);overflow:hidden;font-size:11px}
.dkm>div{border:1px solid var(--bd);padding:6px;color:var(--tx2);display:flex;flex-direction:column}
.dkm .dt2{border:none;background:var(--bg2);flex-direction:row;align-items:center;gap:8px}
.dkm .lf2{background:var(--bg2)}.dkm .rt2{background:var(--bg2)}.dkm .bt2{background:var(--bg2);flex-direction:row;align-items:center}
.mdoc{font-size:14px;line-height:1.8;max-width:640px}
.mdoc.small{font-size:12px;line-height:1.6}
.mdoc h3{color:var(--mdc,var(--pri));font-size:1.35em;margin:6px 0 10px}
.mdoc p{margin:8px 0}
.mdoc pre{background:var(--bg3);border:1px solid var(--bd2);border-radius:5px;padding:10px 12px;font-family:Consolas,monospace;font-size:.85em;margin:10px 0;overflow:auto}
.mdoc b{color:var(--mdc,var(--pri))}
.dgt{border-collapse:collapse;width:100%;max-width:640px;font-size:12px}
.dgt th,.dgt td{padding:6px 14px;text-align:left}
.dgt th{font-weight:600;background:var(--bg2);cursor:pointer;user-select:none}
.dgt th .arr{color:var(--pri)}
.dgt.lh td,.dgt.lh th{border-bottom:1px solid var(--bd2)}
.dgt.la td,.dgt.la th{border:1px solid var(--bd2)}
.dgt.zb tbody tr:nth-child(even){background:var(--hov)}
.dgt tbody tr:hover td{background:var(--pri-lt)}
.stb{display:inline-flex;border-radius:4px;overflow:hidden;font-size:11px;font-weight:500}
.stb .l{padding:2px 8px;color:#fff}
.stb .r{padding:2px 10px}
.stc{display:flex;gap:10px;border:1px solid var(--bd);border-radius:8px;padding:12px 14px;min-width:240px;background:var(--bg)}
.stc .ic{width:36px;height:36px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff}
.stc .tp{font-size:11px;color:var(--tx2)}.stc .bt{font-size:15px;font-weight:600;margin-top:2px}
.kvf{display:grid;grid-template-columns:auto 1fr;gap:6px 16px;font-size:12px;align-items:center}
.kvf .k{color:var(--tx2);text-align:right}
.dkm{display:grid;grid-template-columns:160px 1fr 200px;grid-template-rows:28px 1fr 22px;height:260px;border:1px solid var(--bd);border-radius:var(--r);overflow:hidden;font-size:11px}
.dkm>div{border:1px solid var(--bd);padding:6px;color:var(--tx2);display:flex;flex-direction:column}
.dkm .dt2{border:none;background:var(--bg2);flex-direction:row;align-items:center;gap:8px}
.dkm .lf2{background:var(--bg2)}.dkm .rt2{background:var(--bg2)}.dkm .bt2{background:var(--bg2);flex-direction:row;align-items:center}
.ovl{position:fixed;inset:0;background:rgba(0,0,0,.45);z-index:90;display:flex;align-items:center;justify-content:center}
.dlg{width:520px;max-width:90vw;max-height:80vh;display:flex;flex-direction:column;background:var(--bg);border:1px solid var(--bd);border-radius:8px;box-shadow:var(--sh);overflow:hidden}
.dlg .dth{display:flex;align-items:center;padding:0 6px 0 12px;height:32px;background:var(--bg2);border-bottom:1px solid var(--bd);font-weight:600}
.dlg .dth .x{margin-left:auto;width:22px;height:22px;text-align:center;line-height:22px;border-radius:3px;cursor:pointer;color:var(--tx2)}
.dlg .dbd{padding:14px 16px;overflow:auto}
.kvf{display:grid;grid-template-columns:auto 1fr;gap:6px 16px;font-size:12px;align-items:center}
.kvf .k{color:var(--tx2);text-align:right}
"""

# ═══════════════════════════════════════════════════════════════
# JS framework + pages (compact but complete)
# ═══════════════════════════════════════════════════════════════
JS = r"""
"use strict";
var $=function(id){return document.getElementById(id)};
var esc=function(s){return String(s||"").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;")};
function dlog(m){var b=$("logP");if(!b)return;var e=document.createElement("div");e.innerHTML='<span style="opacity:.5">['+stamp()+']</span> '+esc(m);b.appendChild(e);while(b.children.length>50)b.removeChild(b.firstChild);b.scrollTop=b.scrollHeight}
function stamp(){var d=new Date();return("0"+d.getHours()).slice(-2)+":"+("0"+d.getMinutes()).slice(-2)+":"+("0"+d.getSeconds()).slice(-2)}

/* pages data */
var PAGES = {};
""" + _PAGES_JS + """

/* ═══ framework ═══ */
var OPEN=["home"],ACTIVE="home",currentTab="home";
var dtabs=$("dT"),dBody=$("dBody"),pGrid=$("pGrid"),rBody=$("rB");

function boot(){
  buildRibbonTabs();renderTree();renderRibbon("home");renderTabs();renderDoc();renderProps();
  dlog("演示台就绪");
}
function buildRibbonTabs(){
  var host=$("rT");host.innerHTML="";
  var defs=[["home","首 页"],["markdown","Markdown"],["data","数 据"],["controls","控 件"],["layout","布 局"],["infra","基础设施"]];
  defs.forEach(function(d){
    var el=document.createElement("span");el.className="rt"+(d[0]===currentTab?" on":"");el.textContent=d[1];el.dataset.t=d[0];
    el.addEventListener("click",function(){currentTab=d[0];host.querySelectorAll(".rt").forEach(function(x){x.classList.toggle("on",x===el)});renderRibbon(currentTab)});
    host.appendChild(el);
  });
  host.addEventListener("dblclick",function(e){
    var t=e.target.closest(".rt");if(!t)return;
    document.body.classList.toggle("rbn-min");
  });
}
function renderRibbon(mode){
  var host=$("rB");host.innerHTML="";
  var def=RIBBON[mode];if(!def)return;
  def.groups.forEach(function(g){
    var grp=document.createElement("div");grp.className="rg";
    var cmds=document.createElement("div");cmds.className="cmds"+(g.palette?"":"");
    if(g.palette){
      g.palette.forEach(function(p){
        var c=document.createElement("span");c.className="pal-chip";c.style.background=p.c;c.title=p.n;
        c.addEventListener("click",function(){applyPalette(p.act.replace("pal-",""))});
        cmds.appendChild(c);
      });
    } else {
      (g.btns||[]).forEach(function(b){
        var btn=document.createElement("button");btn.className="rbn-sml";
        btn.innerHTML='<span class="ic">'+b.ic+'</span><span class="lb">'+b.lb+'</span>';
        btn.addEventListener("click",function(){if(b.doc)openDoc(b.doc);if(b.act)doAction(b.act)});
        cmds.appendChild(btn);
      });
    }
    grp.appendChild(cmds);
    var lbl=document.createElement("div");lbl.className="glbl";lbl.textContent=g.label;
    grp.appendChild(lbl);host.appendChild(grp);
  });
}
function renderTree(){
  var host=$("lTree");host.innerHTML="";
  TREE.forEach(function(n){
    if(n.grp){var g=document.createElement("div");g.className="ltr-grp";g.textContent=n.grp;host.appendChild(g)}
    else{var nd=document.createElement("div");nd.className="ltr-nd";nd.textContent=n.name;nd.addEventListener("click",function(){openDoc(n.page)});host.appendChild(nd)}
  });
}
function renderTabs(){
  var host=$("dT");host.innerHTML="";
  OPEN.forEach(function(id){
    var p=PAGES[id];if(!p)return;
    var tab=document.createElement("div");tab.className="dtab"+(id===ACTIVE?" on":"");
    tab.textContent=p.title;tab.addEventListener("click",function(){ACTIVE=id;renderDoc();renderProps()});
    host.appendChild(tab);
  });
}
function renderDoc(){
  var p=PAGES[ACTIVE];if(!p)return;
  var body=$("dBody");body.innerHTML="";
  var wrap=document.createElement("div");wrap.className="pg-body";
  wrap.innerHTML='<div class="crumb">'+p.crumb+'</div><h1>'+p.title+'</h1><p class="dsc">'+p.desc+'</p>';
  body.appendChild(wrap);
  if(p.render)p.render(wrap);
  if(p.props&&p.props.length){
    var pg=document.createElement("div");pg.className="pgp";
    p.props.forEach(function(pr){
      var row=document.createElement("div");row.className="prw";
      row.innerHTML='<label>'+pr.label+'</label>';
      var ctl=document.createElement("div");ctl.className="ctl";
      if(pr.type==="bool"){var cb=document.createElement("input");cb.type="checkbox";cb.checked=pr.value;cb.addEventListener("change",function(){pr.apply(cb.checked);dlog(pr.label+" = "+cb.checked)});ctl.appendChild(cb)}
      else if(pr.type==="slider"){var sl=document.createElement("input");sl.type="range";sl.min=pr.min;sl.max=pr.max;sl.value=pr.value;var lb=document.createElement("span");lb.textContent=pr.value+" "+(pr.unit||"");sl.addEventListener("input",function(){lb.textContent=sl.value+" "+(pr.unit||"");pr.apply(parseFloat(sl.value));dlog(pr.label+" = "+sl.value)});ctl.appendChild(sl);ctl.appendChild(lb)}
      else if(pr.type==="combo"){var sel=document.createElement("select");pr.options.forEach(function(o){var op=document.createElement("option");op.textContent=o;if(o===pr.value)op.selected=true;sel.appendChild(op)});sel.addEventListener("change",function(){pr.apply(sel.value);dlog(pr.label+" = "+sel.value)});ctl.appendChild(sel)}
      row.appendChild(ctl);pg.appendChild(row);
    });
    body.appendChild(pg);
  }
}
function openDoc(id){
  var p=PAGES[id];if(!p)return;
  var old=$("pgrid");old.innerHTML="";
  var pg=document.createElement("div");pg.className="pgp";
  (p.props||[]).forEach(function(pr){
    var row=document.createElement("div");row.className="prw";
    row.innerHTML='<label>'+pr.label+'</label>';
    var ctl=document.createElement("div");ctl.className="ctl";
    if(pr.type==="bool"){var cb=document.createElement("input");cb.type="checkbox";cb.checked=pr.value;cb.addEventListener("change",function(){pr.apply(cb.checked);dlog(pr.label+" = "+cb.checked)});ctl.appendChild(cb)}
    else if(pr.type==="combo"){var sel=document.createElement("select");pr.options.forEach(function(o){var op=document.createElement("option");op.textContent=o;if(o===pr.value)op.selected=true;sel.appendChild(op)});sel.addEventListener("change",function(){pr.apply(sel.value);dlog(pr.label+" = "+sel.value)});ctl.appendChild(sel)}
    else if(pr.type==="slider"){var sl=document.createElement("input");sl.type="range";sl.min=pr.min;sl.max=pr.max;sl.value=pr.value;var lb=document.createElement("span");lb.textContent=pr.value+" "+(pr.unit||"");sl.addEventListener("input",function(){lb.textContent=sl.value+" "+(pr.unit||"")});sl.addEventListener("change",function(){pr.apply(parseFloat(sl.value));dlog(pr.label+" = "+sl.value)});ctl.appendChild(sl)}
    else if(pr.type==="text"){var tx=document.createElement("input");tx.type="text";tx.value=pr.value||"";tx.addEventListener("change",function(){pr.apply(tx.value);dlog(pr.label+" = "+tx.value)});ctl.appendChild(tx)}
    row.appendChild(ctl);pg.appendChild(row);
  });
  body.appendChild(pgWrap);
}

var logEntries=[];
function dlog(msg){
  var box=$("logP");if(!box)return;
  var el=document.createElement("div");
  el.innerHTML='<span style="color:var(--tx3)">['+stamp()+']</span> '+esc(msg);
  box.appendChild(el);while(box.children.length>50)box.removeChild(box.firstChild);box.scrollTop=box.scrollHeight;
}
function stamp(){return new Date().toTimeString().slice(0,8)}
function esc(s){return String(s||"").replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;")}
function boot(){
  buildRibbonTabs();renderTree();renderDoc();renderProps();
}
document.addEventListener("DOMContentLoaded",boot);
</script>
</body>
</html>
