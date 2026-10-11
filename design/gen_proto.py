# -*- coding: utf-8 -*-
"""Generate CodeWF.Avalonia.Demo prototype v4"""
import os

DIR = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(DIR, "CodeWF.Avalonia.Demo原型.html")

# ═══ Page data ═══
PAGES_JS = r"""
var PAGES={
"home":{title:"控件总览",crumb:"首页",
 desc:"CodeWF.Avalonia 全部控件的交互演示。从 Ribbon 菜单或左侧资源管理器打开，右侧属性面板实时调整。",
 render:function(el){el.innerHTML='<div class="stg"><p style="line-height:1.8">从顶部 <b>Ribbon 菜单</b> 或左侧 <b>控件资源管理器</b> 打开任意控件演示。<br>右侧 <b>属性面板</b> 实时显示并修改当前演示的属性。<br>底部 <b>日志</b> 记录每次属性变更。</p></div>'},
 props:[]},
"md-viewer":{title:"MarkdownViewer",crumb:"Markdown / MarkdownViewer",
 desc:"后台解析+增量渲染+块级虚拟化。18 套排版主题，任务列表可勾选回写。",
 render:function(el){el.innerHTML='<div class="stg"><div class="mdoc"><h3>Quick Start</h3><p>支持 <b>GFM 表格</b>、代码块、任务列表。</p><pre>var v = new MarkdownViewer();</pre></div></div>'},
 props:[{label:"TypographyTheme",type:"combo",options:["Basic","BlueGlow","CuteGreen"],value:"BlueGlow",apply:function(v){dlog("TypographyTheme = "+v)}},
 {label:"EnableVirtualization",type:"bool",value:true,apply:function(v){dlog("EnableVirtualization = "+v)}}]},
"data-grid":{title:"DataGrid (Semi)",crumb:"数据 / DataGrid",
 desc:"Semi 风格 DataGrid + 三态排序、自然排序、智能 ToolTip。",
 render:function(el){el.innerHTML='<div class="stg"><table class="dgt lh" style="width:100%"><thead><tr><th>Name ▲</th><th>Age</th><th>City</th></tr></thead><tbody><tr><td>Zhang San</td><td>28</td><td>Hangzhou</td></tr><tr><td>Li Si</td><td>35</td><td>Beijing</td></tr><tr><td>Wang Wu</td><td>19</td><td>Chengdu</td></tr></tbody></table></div>'},
 props:[{label:"GridLinesVisibility",type:"combo",options:["Horizontal","All","None"],value:"Horizontal",apply:function(v){dlog("GridLines = "+v)}}]},
"prop-grid":{title:"PropertyGrid",crumb:"数据 / PropertyGrid",
 desc:"分类/字母视图、无限层级、搜索过滤、描述面板、11 种编辑器、属性联动。右侧面板即本控件。",
 render:function(el){el.innerHTML='<div class="stg"><p>PropertyGrid 自我演示——右侧面板的属性行即本控件渲染。</p></div>'},
 props:[]},
"code-wf-window":{title:"CodeWFWindow",crumb:"控件 / CodeWFWindow",
 desc:"无边框窗口：8 向缩放、标题栏拖拽、全屏按钮、内容钻入、Win11 DWM 圆角。支持标题栏三层内容槽。多种自定义配置：",
 render:function(el){var wins=[
  {t:"默认",body:"标准标题栏 + 内容",btns:"— □ ✕"},
  {t:"全屏按钮",body:"带全屏切换按钮",btns:"— ⛶ □ ✕"},
  {t:"内容钻入标题栏",body:"工具栏嵌入标题栏",btns:"— □ ✕"},
  {t:"自定义标题栏",body:"三层内容槽 Left/Center/Right",btns:"— □ ✕"}];
 var h='<div style="display:flex;gap:14px;flex-wrap:wrap">';
 wins.forEach(function(w){h+='<div style="border:1px solid var(--bd);border-radius:8px;overflow:hidden;width:240px">'+
  '<div style="display:flex;align-items:center;height:30px;background:var(--bg2);padding:0 6px 0 10px;gap:4px">'+
  '<span style="color:var(--pri);font-size:11px">◆</span><span style="color:var(--tx2);font-size:11px;flex:1">'+w.t+'</span>'+
  '<span style="color:var(--tx3);font-size:11px">'+w.btns+'</span></div>'+
  '<div style="padding:14px;color:var(--tx2);font-size:11px;min-height:60px">'+w.body+'</div></div>'});
 h+='</div>';el.innerHTML=h},
 props:[]},
"search-list":{title:"SearchListBox",crumb:"控件 / SearchListBox",
 desc:"搜索框 400ms 防抖过滤+计数条+多选 ListBox。",
 render:function(el){var items=["Hangzhou","Shanghai","Shaoxing","Beijing"];
  var h='<div class="stage" style="display:block;max-width:320px"><div style="border:1px solid var(--bd);border-radius:var(--r);overflow:hidden">'+
  '<input placeholder="Search…" style="width:100%;padding:6px 10px;border:none;border-bottom:1px solid var(--bd);background:var(--bg);color:var(--tx);outline:none;font:inherit;font-size:12px">'+
  '<div style="padding:3px 10px;font-size:10px;color:var(--tx3);border-bottom:1px solid var(--bd2)">4 / 4</div><ul style="list-style:none">';
  items.forEach(function(c){h+='<li style="padding:5px 12px">'+c+'</li>'});
  h+='</ul></div></div>';el.innerHTML=h},
 props:[]},
"transfer":{title:"Transfer",crumb:"控件 / Transfer",
 desc:"双列表穿梭框：双击或选中后按钮移动。",
 render:function(el){el.innerHTML='<div class="stage"><div style="display:grid;grid-template-columns:1fr auto 1fr;gap:10px;align-items:center;max-width:560px">'+
  '<div style="border:1px solid var(--bd);border-radius:var(--r)"><div style="padding:5px 10px;background:var(--bg2);font-weight:600;font-size:11px">候选</div><ul style="list-style:none;padding:4px 0"><li style="padding:3px 12px">Hangzhou</li><li style="padding:3px 12px">Beijing</li></ul></div>'+
  '<div style="display:flex;flex-direction:column;gap:6px"><button style="width:30px;height:26px;border-radius:15px;border:1px solid var(--bd);background:var(--bg);cursor:pointer;color:var(--pri)">›</button><button style="width:30px;height:26px;border-radius:15px;border:1px solid var(--bd);background:var(--bg);cursor:pointer;color:var(--pri)">‹</button></div>'+
  '<div style="border:1px solid var(--bd);border-radius:var(--r)"><div style="padding:5px 10px;font-weight:600">已选</div><ul style="list-style:none;padding:4px 0"><li style="padding:3px 12px">Shanghai</li></ul></div></div></div>'},
 props:[]},
"status":{title:"状态组件",crumb:"控件 / 状态组件",
 desc:"StatusBadge/StatusCard/StatusLabel：五种语义状态（Debug/Info/Warn/Error/Fatal），AntD 风格配色，深浅主题自动换肤。",
 render:function(el){el.innerHTML='<div class="stage" style="display:flex;flex-direction:column;gap:14px;align-items:flex-start">'+
  '<div><span style="display:inline-flex;border-radius:4px;font-size:11px"><span style="padding:2px 8px;color:#fff;background:#52c41a">消息</span><span style="padding:2px 10px;color:#52c41a">3</span></span>'+
  ' <span style="display:inline-flex;border-radius:4px;font-size:11px"><span style="padding:2px 8px;color:#fff;background:#fa8c16">警告</span><span style="padding:2px 10px;color:#fa8c16">12</span></span></div>'+
  '<div style="display:flex;gap:10px;border:1px solid var(--bd);border-radius:8px;padding:12px 14px;min-width:260px">'+
  '<div style="width:36px;height:36px;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff;background:#fa8c16">⚠</div>'+
  '<div><div style="font-size:11px;color:var(--tx2)">泵站服务</div><div style="font-size:15px;font-weight:600;color:#fa8c16">Running</div></div></div></div>'},
 props:[]},
"guide":{title:"Guide",crumb:"控件 / Guide",
 desc:"聚光遮罩（EvenOdd 挖洞）+13 种弹出方位+箭头自动对齐+键盘导航。应用引导按钮可对整个演示台做引导。",
 render:function(el){el.innerHTML='<div class="gdov" style="position:relative;min-height:220px;background:var(--bg2);border-radius:var(--r);overflow:hidden">'+
  '<div style="padding:16px;opacity:.25"><h3>演示页面</h3><p>被引导的真实页面内容。</p></div>'+
  '<div style="position:absolute;inset:0;background:rgba(0,0,0,.4)"></div>'+
  '<div style="position:absolute;left:30%;top:20%;width:240px;background:var(--bg);border:1px solid var(--bd);border-radius:8px;padding:12px;box-shadow:var(--sh)">'+
  '<div style="font-size:10px;color:var(--tx3)">步骤 1 / 3</div><div style="font-weight:700;margin:2px 0">搜索</div>'+
  '<div style="font-size:11px;color:var(--tx2);margin-bottom:10px">输入关键字可实时过滤列表。</div>'+
  '<div style="display:flex;gap:6px;justify-content:flex-end"><button class="mock-btn" style="height:22px;padding:0 10px;font-size:11px">上一步</button><button class="mock-btn pri" style="height:22px;padding:0 10px;font-size:11px">下一步</button></div></div></div>'},
 props:[]},
"dock":{title:"Dock 布局",crumb:"布局 / Dock",
 desc:"Dock.Avalonia Semi 皮肤+布局行为扩展：文档页签、工具停靠/浮动/自动隐藏、GridDock 自动折叠、布局持久化。",
 render:function(el){el.innerHTML='<div class="stage" style="display:block"><div style="display:grid;grid-template-columns:150px 1fr 180px;grid-template-rows:26px 1fr 22px;height:240px;border:1px solid var(--bd);border-radius:var(--r);overflow:hidden;font-size:11px">'+
  '<div style="border:1px solid var(--bd);padding:4px">Tools</div><div style="border:1px solid var(--bd);padding:4px">[文档1] [文档2]</div>'+
  '<div style="border:1px solid var(--bd);padding:4px">Properties</div><div style="border:1px solid var(--bd);padding:4px">Explorer</div>'+
  '<div style="border:1px solid var(--bd);padding:4px">Document area</div><div style="border:1px solid var(--bd);padding:4px">PropertyGrid</div>'+
  '<div style="border:1px solid var(--bd);padding:4px">Status bar</div></div></div>'},
 props:[]},
"log":{title:"Log 日志",crumb:"基础设施 / Log",
 desc:"Logger 文件日志+LogView 应用内查看器（级别过滤/节流刷新/右键复制/打开目录）+InApp/DesktopWindow 通知。底部日志面板即实时事件流。",
 render:function(el){el.innerHTML='<div class="stage"><p>点击按钮写入不同级别日志，观察底部日志面板。</p><button class="mock-btn" onclick="dlog(\'[INF] 演示日志\')">Info</button> <button class="mock-btn" onclick="dlog(\'[WRN] 警告日志\')">Warn</button></div>'},
 props:[]},
"lang":{title:"Lang 多语言",crumb:"基础设施 / Lang",
 desc:"JsonLangPlugin+I18nManager 运行时切换；Json/Xml/Resx 三种后端；强类型 Key 源生成器。本演示台界面文案由 Lang 驱动。",
 render:function(el){el.innerHTML='<div class="stage"><p>后端：<button class="mock-btn">Json</button> <button class="mock-btn">Xml</button> <button class="mock-btn">Resx</button><br>当前文化：<b>zh-CN</b></p></div>'},
 props:[]},
"trapezoid":{title:"梯形页签",crumb:"控件 / 梯形页签",
 desc:"梯形带斜边页签：首/中/末三种路径自适应。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>梯形页签演示。</p></div>'},props:[]},
"markup":{title:"Markup 扩展",crumb:"控件 / Markup 扩展",
 desc:"{If}/{Switch} XAML 条件取值、RangeObservableCollection。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>Markup 扩展演示。</p></div>'},props:[]},
"windowedstack":{title:"WindowedStackPanel",crumb:"控件 / 虚拟化面板",
 desc:"窗口化虚拟化面板。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>虚拟化面板演示。</p></div>'},props:[]},
"columndisplay":{title:"ColumnDisplayEditor",crumb:"控件 / 列设置",
 desc:"列设置面板：拖拽排序+勾选可见。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>列设置演示。</p></div>'},props:[]},
"md-live":{title:"实时编辑",crumb:"Markdown / 实时编辑",
 desc:"所见即所得块编辑。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>实时编辑演示。</p></div>'},props:[]},
"md-export":{title:"导出与富复制",crumb:"Markdown / 导出",
 desc:"PNG/PDF/Word/HTML/公众号富复制。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>导出演示。</p></div>'},props:[]},
"md-themes":{title:"排版主题画廊",crumb:"Markdown / 主题",
 desc:"18 套排版主题。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>18 套排版主题画廊演示。</p></div>'},props:[]},
"md-capabilities":{title:"能力演示",crumb:"Markdown / 能力",
 desc:"图片预览/数学公式/Mermaid/代码高亮。",feats:[],
 render:function(el){el.innerHTML='<div class="stage"><p>能力演示。</p></div>'},props:[]}
};
"""

print("PAGES_JS defined")
