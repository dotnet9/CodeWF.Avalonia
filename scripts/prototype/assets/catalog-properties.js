/* 原型参数以原生 Demo 的 BuildProperties 为准；交互预览不替代 .NET API 验证。 */
"use strict";
(function () {
  function resource(path) {
    var value = window.CodeWFPrototypeResources[LANG];
    path.split('.').forEach(function (part) { value = value && value[part]; });
    if (value == null) throw new Error('Missing prototype resource: ' + path);
    return value;
  }
  function label(page, key) { return resource('Content.' + page + '.Properties.' + key); }
  function view(page, key) { return resource('Content.' + page + '.View.' + key); }
  function text(key) { return window.CodeWFCatalogText[LANG][key]; }
  function roots(id) { return document.querySelectorAll('[data-control="' + id + '"]'); }
  function each(id, action) { roots(id).forEach(action); }
  function prop(id, page, key, type, value, options) {
    var item = Object.assign({ id:id, type:type, value:value, apply:function () {} }, options || {});
    Object.defineProperty(item, 'label', { configurable:true, get:function () { return page ? label(page, key) : key; } });
    return item;
  }
  function number(id, page, key, value, min, max, apply) {
    return prop(id, page, key, 'number', value, { min:min, max:max, apply:apply || function () {} });
  }
  function readonly(id, page, key, value) {
    var item = prop(id, page, key, 'text', '', { readonly:true });
    Object.defineProperty(item, 'value', { get:typeof value === 'function' ? value : function () { return value; } });
    return item;
  }
  function choice(id, page, key, options, value, apply, localized) {
    var selected = value, item = prop(id, page, key, 'combo', value, { apply:function () { apply(selected); } });
    function display(option) { return localized ? label(page, option) : option; }
    Object.defineProperties(item, {
      options:{ get:function () { return options.map(display); } },
      value:{ get:function () { return display(selected); }, set:function (value) { selected = options.find(function (option) { return display(option) === value; }) || selected; } },
      selected:{ get:function () { return selected; } }
    });
    return item;
  }
  function updateProperties() { renderPGrid(ACTIVE_DOC); }
  function value(id, key) { return PROTO[id].props.find(function (item) { return item.id === key; }).value; }
  function replace(id, render) { PROTO[id].render = render; }

  // 五档状态的名称和参数选项均复用原生语言资源。
  var levels = ['Debug','Info','Warn','Error','Fatal'], levelKeys = ['Debug','Information','Warning','Error','Critical'];
  function badgeTextProperty(id) {
    var edited = false, custom = '';
    var item = prop('st.badgeText', null, 'RightText', 'text', '', { apply:function (text) {
      each(id,function(el){el.querySelector('.catalog-driven span').textContent=text;});
    } });
    // 未编辑的默认文案随语言读取；用户输入是内容，不在切语时覆盖。
    Object.defineProperty(item,'value',{get:function(){return edited?custom:view('StatusComponents','ChangeTheStatusAndIconWidthInThe');},set:function(value){edited=true;custom=value;}});
    return item;
  }
  function statusProps(id, badge) {
    var kind = choice('st.kind', 'StatusComponents', 'CardStatus', levelKeys, 'Information', function (selected) {
      each(id, function (el) { el.querySelectorAll('.catalog-driven').forEach(function (card) {
        card.dataset.level = levels[levelKeys.indexOf(selected)];
        if(card.classList.contains('badge'))card.querySelector('strong').textContent = levels[levelKeys.indexOf(selected)];
      }); });
    }, true);
    if (badge) Object.defineProperty(kind, 'label', { get:function () { return 'Classes / LeftText'; } });
    return [kind, badge
      ? badgeTextProperty(id)
      : number('st.iconWidth','StatusComponents','IconWidth',32,16,64,function (width) { each(id,function (el) { el.querySelectorAll('.catalog-driven.card').forEach(function(card){card.style.setProperty('--status-icon-width',width+'px');}); }); })];
  }
  function statusRender(id, badge) {
    return function (el) {
      function card(level, index, driven) {
        var top=badge?level:driven?'DRIVEN':level.toUpperCase();
        var bottom=driven?(badge&&id!=='status'?value(id,'st.badgeText'):text('StatusDriven')):badge?view('StatusComponents',levelKeys[index]):text('StatusSample'+level);
        var item = '<strong>'+top+'</strong><span>'+esc(bottom)+'</span>';
        return '<div class="catalog-status '+(badge?'badge':'card')+(driven?' catalog-driven':'')+'" data-level="'+level+'">'+(!badge?'<span class="catalog-status-icon" aria-hidden="true"></span>':'')+item+'</div>';
      }
      var selected = levels[levelKeys.indexOf(PROTO[id].props[0].selected)];
      el.innerHTML='<div class="catalog-status-page"><h4>'+(badge?'StatusBadge':'StatusCard')+'</h4><div class="catalog-status-row">'+levels.map(function(level,index){return card(level,index,false);}).join('')+'</div><h4>'+esc(view('StatusComponents','DrivenByPropertyPanel'))+'</h4>'+card(selected,0,true)+'</div>';
      if(!badge)el.querySelector('.catalog-driven').style.setProperty('--status-icon-width',value(id,'st.iconWidth')+'px');
    };
  }
  ['status-badge','status-card'].forEach(function (id) { PROTO[id].props=statusProps(id,id==='status-badge');replace(id,statusRender(id,id==='status-badge')); });

  PROTO.titlebar.props=[prop('Title',null,'Title','text','CodeWFTitleBar',{ apply:function (title) { each('titlebar',function(el){el.querySelector('.catalog-titlebar strong').textContent=title;}); } })];
  function apiCard(content) {return '<div class="demo catalog-api-card"><h4>'+esc(resource('Segment.Demo'))+'</h4>'+content+'</div>';}
  replace('titlebar',function(el){el.innerHTML=apiCard('<div class="catalog-titlebar catalog-titlebar-preview"><strong>'+esc(value('titlebar','Title'))+'</strong><span>.NET</span></div>');});
  PROTO['diagonal-line'].props=[number('Thickness',null,'BorderThickness',3,1,12,function(thickness){each('diagonal-line',function(el){el.querySelector('line').setAttribute('stroke-width',thickness);});})];
  replace('diagonal-line',function(el){el.innerHTML=apiCard('<svg class="catalog-line" viewBox="0 0 280 180"><line x1="0" y1="0" x2="280" y2="180" stroke="currentColor" stroke-width="'+value('diagonal-line','Thickness')+'" /></svg>');});
  PROTO.notifications.props=[prop('Message',null,'Message','text',text('Notification'))];
  replace('notifications',function(el){el.innerHTML='<div class="demo"><button class="btn pri">'+esc(text('Notifications'))+'</button></div>';el.querySelector('button').onclick=function(){notify('warn','LogNotifications',value('notifications','Message'));};});
  PROTO.treegrid.props=[prop('SingleSelect',null,'SingleSelect','bool',false,{apply:function(){each('treegrid',function(el){el.dispatchEvent(new Event('selection-mode-change'));});}})];
  replace('treegrid',function(el){
    var original=[10,2,1,20,3,11], rows=original.slice(), direction=0, selected=new Set();
    el.innerHTML='<div class="demo"><p>'+esc(text('TreeHints'))+'</p><table class="catalog-grid" tabindex="0"><thead><tr><th>Name ↕</th><th>Value</th><th>Description</th></tr></thead><tbody></tbody></table><p class="catalog-selection"></p></div>';
    function paint(){
      el.querySelector('tbody').innerHTML=rows.map(function(n){return '<tr data-row="'+n+'" class="'+(selected.has(n)?'selected':'')+'"><td>item'+n+'</td><td>'+n+'</td><td><span title="'+esc(text('TreeLongText'))+'">'+esc(text('TreeLongText'))+'</span></td></tr>';}).join('');
      el.querySelectorAll('[data-row]').forEach(function(row){row.onclick=function(event){var n=Number(row.dataset.row);if(value('treegrid','SingleSelect')||!event.ctrlKey)selected.clear();if(selected.has(n))selected.delete(n);else selected.add(n);paint();};});
      el.querySelector('.catalog-selection').textContent=text('TreeSelection').replace('{0}',selected.size).replace('{1}',rows.length);
    }
    el.querySelector('th').onclick=function(){direction=(direction+1)%3;rows=direction?original.slice().sort(function(a,b){return direction===1?a-b:b-a;}):original.slice();paint();};
    el.querySelector('table').onkeydown=function(event){if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='a'){event.preventDefault();selected.clear();(value('treegrid','SingleSelect')?rows.slice(0,1):rows).forEach(function(n){selected.add(n);});paint();}};
    el.addEventListener('selection-mode-change',function(){if(value('treegrid','SingleSelect')&&selected.size>1)selected=new Set([selected.values().next().value]);paint();});paint();
  });

  var search={total:0,filtered:0,selected:0}, transfer={available:0,selected:0};
  PROTO.searchlist.props=[readonly('sl.total','SearchListBox','Total',function(){return search.total;}),readonly('sl.filtered','SearchListBox','Filtered',function(){return search.filtered;}),readonly('sl.selected','SearchListBox','SelectedItems',function(){return search.selected;})];
  replace('searchlist',function(el){
    var items=window.CodeWFDemoCatalog.flatMap(function(group){return group.items;}).filter(function(entry){return entry.api;}).map(function(entry){return entry.api;}),selected=new Set(),anchor=-1, timer;
    el.innerHTML='<div class="demo"><input type="search" class="catalog-input" data-search placeholder="'+esc(label('SearchListBox','Filtered'))+'"><div class="filelist" tabindex="0"></div></div>';
    function paint(){var query=el.querySelector('[data-search]').value.toLowerCase(),matches=items.filter(function(item){return item.toLowerCase().includes(query);});search={total:items.length,filtered:matches.length,selected:selected.size};el.querySelector('.filelist').innerHTML=matches.map(function(item,index){return '<div class="fl'+(selected.has(item)?' on':'')+'" data-index="'+index+'"><span>'+esc(item)+'</span></div>';}).join('');el.querySelectorAll('[data-index]').forEach(function(row){row.onclick=function(event){var index=Number(row.dataset.index),item=matches[index];if(!event.ctrlKey&&!event.shiftKey)selected.clear();if(event.shiftKey&&anchor>=0)matches.slice(Math.min(anchor,index),Math.max(anchor,index)+1).forEach(function(item){selected.add(item);});else if(selected.has(item))selected.delete(item);else selected.add(item);anchor=index;paint();};});updateProperties();}
    el.querySelector('[data-search]').oninput=function(){clearTimeout(timer);timer=setTimeout(paint,400);};paint();
  });
  PROTO.transfer.props=[readonly('tr.available','Transfer','AvailableCount',function(){return transfer.available;}),readonly('tr.selected','Transfer','SelectedCount',function(){return transfer.selected;})];
  replace('transfer',function(el){
    var left=['SearchListBox','Transfer','StatusBadge','StatusCard','Guide'],right=['LogView','I18nManager'],selected={left:new Set(),right:new Set()};
    el.innerHTML='<div class="demo"><div class="trf"><div class="col"><div class="hd">'+esc(view('Transfer','FeatureBacklogAvailable'))+'</div><input type="search" class="catalog-input" data-search="left"><div class="ls" data-side="left"></div></div><div class="mid"><button class="btn" data-move="right">›</button><button class="btn" data-move="left">‹</button></div><div class="col"><div class="hd">'+esc(view('Transfer','CurrentDeliverySelected'))+'</div><input type="search" class="catalog-input" data-search="right"><div class="ls" data-side="right"></div></div></div></div>';
    function move(side,items){var source=side==='right'?left:right,target=side==='right'?right:left;items.forEach(function(item){var index=source.indexOf(item);if(index>=0){source.splice(index,1);target.push(item);}});selected.left.clear();selected.right.clear();paint();}
    function paint(){transfer={available:left.length,selected:right.length};['left','right'].forEach(function(side){var query=el.querySelector('[data-search="'+side+'"]').value.toLowerCase(),items=side==='left'?left:right;el.querySelector('[data-side="'+side+'"]').innerHTML=items.filter(function(item){return item.toLowerCase().includes(query);}).map(function(item){return '<div class="it'+(selected[side].has(item)?' on':'')+'" data-item="'+item+'">'+item+'</div>';}).join('');el.querySelectorAll('[data-side="'+side+'"] [data-item]').forEach(function(row){row.onclick=function(){var item=row.dataset.item;if(selected[side].has(item))selected[side].delete(item);else selected[side].add(item);paint();};row.ondblclick=function(){move(side==='left'?'right':'left',[row.dataset.item]);};});});updateProperties();}
    el.querySelectorAll('[data-move]').forEach(function(button){button.onclick=function(){move(button.dataset.move,Array.from(selected[button.dataset.move==='right'?'left':'right']));};});el.querySelectorAll('[data-search]').forEach(function(input){input.oninput=paint;});paint();
  });
  PROTO.guide.props=[readonly('placement','Guide','PlacementDetails','Center / Left / Right / Top / Bottom / LeftTop / LeftBottom / RightTop / RightBottom / TopLeft / TopRight / BottomLeft / BottomRight')];
  PROTO.colde.props=[readonly('contract','ColumnDisplayEditor','DataContract','IList<ColumnDisplayItem> (Key / DisplayText / Visible)')];
  PROTO.dock.props=[readonly('composition','Dock','Composition','Dock.Avalonia 12.1 + Dock.Model.ReactiveUI + CodeWF DockSemiTheme')];
  PROTO.trapezoid.props=[readonly('themeKeys','TrapezoidTab','ThemeKeys','TrapezoidShapedTabControl / TrapezoidShapedTabStrip / TrapezoidShapedTabItem')];
  ['if','switch'].forEach(function(id){PROTO[id].props=[readonly('useCases','MarkupExtension','UseCases',function(){return label('MarkupExtension','ConditionalDisplayTextAndIcons');})];});
  PROTO.appguide.props=[readonly('targetLookup','AppGuide','TargetLookup','RibbonRoot / ExplorerPanel / DocumentHost / PropertyPanel / LogPanel')];
  PROTO.about.props=[];PROTO.lang.props=[];PROTO.expansion.props=[];

  PROTO.datagrid.props=[prop('dg.defaults','DataGrid','EnableDefaults','bool',true,{apply:function(){each('datagrid',function(el){el.dispatchEvent(new Event('grid-defaults-change'));});}}),number('dg.rowHeight','DataGrid','RowHeight',32,24,64,function(height){each('datagrid',function(el){el.style.setProperty('--demo-row-height',height+'px');});})];
  replace('datagrid',function(el){
    var original=[10,2,1,20,3,11],rows=original.slice(),direction=0,selected=new Set();
    el.innerHTML='<div class="demo"><table class="catalog-grid catalog-data-grid" tabindex="0"><thead><tr><th>Name ↕</th><th>Value</th><th>Description</th></tr></thead><tbody></tbody></table></div>';el.style.setProperty('--demo-row-height',value('datagrid','dg.rowHeight')+'px');
    function paint(){el.querySelector('tbody').innerHTML=rows.map(function(n){return '<tr data-row="'+n+'" class="'+(selected.has(n)?'selected':'')+'"><td>item'+n+'</td><td>'+n+'</td><td><span'+(value('datagrid','dg.defaults')?' title="'+esc(text('TreeLongText'))+'"':'')+'>'+esc(text('TreeLongText'))+'</span></td></tr>';}).join('');el.querySelectorAll('[data-row]').forEach(function(row){row.onclick=function(event){if(!event.ctrlKey)selected.clear();selected.add(Number(row.dataset.row));paint();};});}
    el.querySelector('th').onclick=function(){direction=(direction+1)%(value('datagrid','dg.defaults')?3:2);rows=direction?original.slice().sort(function(a,b){return value('datagrid','dg.defaults')?(direction===1?a-b:b-a):String(a).localeCompare(String(b));}):original.slice();paint();};
    el.querySelector('table').onkeydown=function(event){if(value('datagrid','dg.defaults')&&(event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='a'){event.preventDefault();rows.forEach(function(n){selected.add(n);});paint();}};el.addEventListener('grid-defaults-change',function(){direction=0;rows=original.slice();paint();});paint();
  });
  function gridEffect(){each('propgrid',function(el){el.style.setProperty('--demo-name-width',value('propgrid','pg.nameWidth')+'px');el.querySelector('.catalog-property-tools').hidden=!value('propgrid','pg.toolbar');el.querySelector('.catalog-property-description').hidden=!value('propgrid','pg.description');el.querySelectorAll('tbody').forEach(function(body){var rows=Array.from(body.children);if(PROTO.propgrid.props[0].selected==='Alphabetical')rows.sort(function(a,b){return a.dataset.name.localeCompare(b.dataset.name,LANG);});else rows.sort(function(a,b){return Number(a.dataset.order)-Number(b.dataset.order);});rows.forEach(function(row){body.appendChild(row);});});});}
  PROTO.propgrid.props=[choice('pg.mode','PropertyGrid','ViewMode',['Categorized','Alphabetical'],'Categorized',gridEffect,true),prop('pg.toolbar','PropertyGrid','ShowToolbar','bool',true,{apply:gridEffect}),prop('pg.description','PropertyGrid','ShowDescriptions','bool',true,{apply:gridEffect}),number('pg.nameWidth','PropertyGrid','NameColumnWidth',148,90,340,gridEffect)];
  replace('propgrid',function(el){var rows=[['Name','Valve-01'],['Enabled',true],['Pressure',32],['Description','CodeWF']];el.innerHTML='<div class="demo"><div class="catalog-property-tools row"><button class="btn" data-view="Categorized">'+esc(label('PropertyGrid','Categorized'))+'</button><button class="btn" data-view="Alphabetical">'+esc(label('PropertyGrid','Alphabetical'))+'</button></div><table class="catalog-property-demo"><tbody>'+rows.map(function(row,index){return '<tr data-name="'+row[0]+'" data-order="'+index+'"><th>'+row[0]+'</th><td><input type="'+(typeof row[1]==='boolean'?'checkbox':typeof row[1]==='number'?'number':'text')+'" '+(row[1]===true?'checked':'value="'+row[1]+'"')+'></td></tr>';}).join('')+'</tbody></table><p class="catalog-property-description">'+esc(label('PropertyGrid','NameColumnWidth'))+'</p></div>';el.querySelectorAll('[data-view]').forEach(function(button){button.onclick=function(){PROTO.propgrid.props[0].value=label('PropertyGrid',button.dataset.view);gridEffect();updateProperties();};});gridEffect();});

  function windowEffect(){each('codewfwindow',function(el){el.querySelectorAll('.catalog-window').forEach(function(frame){frame.style.borderRadius=value('codewfwindow','win.corner')+'px';frame.querySelector('.catalog-titlebar').style.height=value('codewfwindow','win.height')+'px';frame.querySelectorAll('.catalog-caption').forEach(function(button){button.style.width=value('codewfwindow','win.caption')+'px';});frame.classList.toggle('content-extended',value('codewfwindow','win.extend'));});});}
  PROTO.codewfwindow.props=[number('win.height','CodeWFWindow','TitleBarHeight',36,24,64,windowEffect),number('win.caption','CodeWFWindow','CaptionButtonWidth',32,24,64,windowEffect),number('win.corner','CodeWFWindow','WindowCornerRadius',8,0,24,windowEffect),prop('win.extend','CodeWFWindow','ExtendContentIntoTitleBar','bool',false,{apply:windowEffect})];
  replace('codewfwindow',function(el){el.innerHTML='<div class="row">'+['Example1DefaultTitleBar','Example2CustomLeftRightContent','Example3CenteredContent'].map(function(key){return '<div class="catalog-window"><div class="catalog-titlebar"><strong>'+esc(view('CodeWFWindow',key))+'</strong><button class="catalog-caption">−</button><button class="catalog-caption">□</button><button class="catalog-caption">×</button></div><div class="catalog-window-content">CodeWFWindow</div></div>';}).join('')+'</div>';windowEffect();});
  var logRows=[];
  function logEffect(){each('log',function(el){var order=['Trace','Debug','Information','Warning','Error'],min=order.indexOf(PROTO.log.props[0].selected),max=value('log','log.max');logRows=logRows.slice(-max);el.querySelector('.catalog-log').innerHTML=logRows.filter(function(row){return order.indexOf(row.level)>=min;}).map(function(row){return '<div data-level="'+row.level+'"><code>'+row.time+'</code> '+row.level+' · '+row.number+'</div>';}).join('');});}
  PROTO.log.props=[choice('log.min','Log','MinimumLevel',['Trace','Debug','Information','Warning','Error'],'Information',logEffect,true),number('log.max','Log','MaximumEntries',2000,100,10000,logEffect)];
  replace('log',function(el){el.innerHTML='<div class="demo"><div class="row">'+['Trace','Debug','Information','Warning','Error'].map(function(level){return '<button class="btn" data-level="'+level+'">'+esc(label('Log',level))+'</button>';}).join('')+'<button class="btn" data-batch>'+esc(view('Log','WriteBatchEventStreamLoadTest'))+'</button></div><div class="catalog-log"></div></div>';function append(level){logRows.push({level:level,time:new Date().toLocaleTimeString(LANG),number:logRows.length+1});}el.querySelectorAll('button[data-level]').forEach(function(button){button.onclick=function(){append(button.dataset.level);logEffect();};});el.querySelector('[data-batch]').onclick=function(){for(var i=0;i<250;i++)append(['Trace','Debug','Information','Warning','Error'][i%5]);logEffect();};logEffect();});

  function wspEffect(){each('wsp',function(el){el.dispatchEvent(new Event('virtualization-change'));});}
  PROTO.wsp.props=[prop('wsp.virtual','WindowedStackPanel','Virtualization','bool',true,{apply:wspEffect}),number('wsp.threshold','WindowedStackPanel','VirtualizationThreshold',40,1,5000,wspEffect),number('wsp.overscan','WindowedStackPanel','Overscan',2,0,10,wspEffect),number('wsp.spacing','WindowedStackPanel','BlockSpacing',8,0,32,wspEffect)];
  replace('wsp',function(el){
    var count=0;
    el.innerHTML='<div class="catalog-wsp-page"><div class="catalog-wsp-toolbar"><button class="btn" data-generate>'+esc(view('WindowedStackPanel','Generate5000Blocks'))+'</button><button class="btn" data-clear>'+esc(view('WindowedStackPanel','Clear'))+'</button><span>'+esc(view('WindowedStackPanel','WindowedStackPanelRealizesBlocksNearTheViewportWithOverscan'))+'</span></div><div class="wsp catalog-wsp"><div class="catalog-wsp-content"></div></div><div class="catalog-wsp-metrics"></div></div>';
    var box=el.querySelector('.catalog-wsp'),content=el.querySelector('.catalog-wsp-content');
    function format(key,args){return resource('Content.WindowedStackPanel.'+key).replace(/\{(\d+)\}/g,function(_,index){return args[Number(index)];});}
    function paint(){
      var height=44+Number(value('wsp','wsp.spacing')),virtual=value('wsp','wsp.virtual')&&count>=value('wsp','wsp.threshold'),overscan=Number(value('wsp','wsp.overscan')),viewport=box.clientHeight;
      var first=virtual?Math.max(0,Math.floor(box.scrollTop/height)-Math.ceil(viewport/height)*overscan):0,last=virtual?Math.min(count,Math.ceil((box.scrollTop+viewport)/height)+Math.ceil(viewport/height)*overscan):count;
      content.style.height=count*height+'px';content.innerHTML=Array.from({length:last-first},function(_,index){var n=first+index;return '<div class="blk" data-block="'+n+'" style="top:'+(n*height)+'px;height:44px">'+esc(format('OnGenerate.BlockFormat',[n+1,count]))+'</div>';}).join('');
      el.querySelector('.catalog-wsp-metrics').textContent=format('UpdateMetrics.BlocksRealizedVirtualizingFormat',[count,last-first,resource('Content.WindowedStackPanel.UpdateMetrics.'+(virtual?'Yes':'No'))]);box.dataset.virtualizing=String(virtual);
    }
    box.onscroll=paint;el.addEventListener('virtualization-change',paint);el.querySelector('[data-generate]').onclick=function(){count=5000;paint();};el.querySelector('[data-clear]').onclick=function(){count=0;box.scrollTop=0;paint();};
    var observer=new ResizeObserver(function(){if(!box.isConnected){observer.disconnect();return;}paint();});observer.observe(box);paint();
  });

  // Markdown 复用已有的源文、排版度量和渲染器，增加真实参数联动。
  function typography(id){each(id,function(el){var page=el.querySelector('.catalog-markdown-basic,.catalog-live,.catalog-export-preview');if(!page)return;var theme=PROTO[id].props.find(function(p){return p.id==='typography-theme';}),size=PROTO[id].props.find(function(p){return p.id==='typography-size';}),tokens=window.CodeWFMarkdownTypography,metrics=Object.assign({},tokens.Common,tokens[theme?.value||'Basic'],size?.value==='Small'?tokens.CompactTypographyResources:{});page.dataset.typographyTheme=theme?.value||'Basic';page.style.fontSize=metrics.CodeWFMarkdownParagraphFontSize+'px';page.style.lineHeight=metrics.CodeWFMarkdownParagraphLineHeight+'px';page.querySelectorAll('h1,h2,h3,h4,h5,h6').forEach(function(heading){heading.style.fontSize=metrics['CodeWFMarkdownHeading'+heading.tagName.slice(1)+'FontSize']+'px';});page.querySelectorAll('pre').forEach(function(code){code.style.fontSize=metrics.CodeWFMarkdownCodeBlockFontSize+'px';});});}
  ['md-viewer','md-lite'].forEach(function(id){PROTO[id].props.forEach(function(p){p.apply=function(){typography(id);};});});
  function markdownVirtualization(){each('md-viewer',function(el){var page=el.querySelector('.catalog-markdown-basic'),enabled=value('md-viewer','md.virtual')&&page.children.length>=value('md-viewer','md.threshold');Array.from(page.children).forEach(function(block){block.style.contentVisibility=enabled?'auto':'visible';block.style.containIntrinsicSize=enabled?'auto 48px':'none';});page.dataset.virtualizing=String(enabled);});}
  PROTO['md-viewer'].props.push(prop('md.virtual','MarkdownViewer','Virtualization','bool',true,{apply:markdownVirtualization}),number('md.threshold','MarkdownViewer','VirtualizationThreshold',40,1,2000,markdownVirtualization));
  function editorEffect(){each('md-editor',function(el){var input=el.querySelector('#edTa');input.style.fontSize=value('md-editor','editor.font')+'px';el.querySelector('.catalog-editor-lines').hidden=!value('md-editor','editor.lines');el.querySelector('.catalog-editor-lines').style.fontSize=input.style.fontSize;});}
  PROTO['md-editor'].props=[number('editor.font','MarkdownEditor','EditorFontSize',15,10,28,editorEffect),prop('editor.lines','MarkdownEditor','ShowLineNumbers','bool',true,{apply:editorEffect}),prop('editor.pair','MarkdownEditor','AutoPairing','bool',true)];
  function editorRender(el){var commands=['Bold','Italic','InlineCode','Heading2','Table','TaskList','CodeBlock','Quote'];el.innerHTML='<div class="demo"><div class="row">'+commands.map(function(command){return '<button class="btn" data-command="'+command+'">'+esc(view('MarkdownEditor',command))+'</button>';}).join('')+'</div><textarea id="edTa" style="height:300px">'+esc(window.CodeWFMarkdownSamples[LANG].editor)+'</textarea></div>';var input=el.querySelector('#edTa');el.querySelectorAll('[data-command]').forEach(function(button){button.onclick=function(){var command=button.dataset.command,start=input.selectionStart,end=input.selectionEnd,selected=input.value.slice(start,end),markers={Bold:'**',Italic:'*',InlineCode:'`'},prefixes={Heading2:'## ',TaskList:'- [ ] ',Quote:'> '},insert=markers[command]?markers[command]+selected+markers[command]:prefixes[command]?prefixes[command]+selected:command==='Table'?'| Name | Value |\n| --- | --- |\n| CodeWF | 42 |':'```\n'+selected+'\n```';input.setRangeText(insert,start,end,'end');input.dispatchEvent(new Event('input'));input.focus();markDirty('md-editor');};});}
  replace('md-editor',function(el){editorRender(el);var input=el.querySelector('#edTa'),shell=document.createElement('div'),lines=document.createElement('pre');shell.className='catalog-editor-shell';lines.className='catalog-editor-lines';input.before(shell);shell.append(lines,input);function paintLines(){lines.textContent=input.value.split('\n').map(function(_,index){return index+1;}).join('\n');}input.addEventListener('input',paintLines);input.addEventListener('scroll',function(){lines.scrollTop=input.scrollTop;});input.addEventListener('keydown',function(event){if(!value('md-editor','editor.pair')||event.ctrlKey||event.metaKey||!['*','`','[','('].includes(event.key))return;event.preventDefault();var close={'*':'*','`':'`','[':']','(':')'}[event.key],start=input.selectionStart,end=input.selectionEnd;input.setRangeText(event.key+input.value.slice(start,end)+close,start,end,'select');input.selectionStart=start+1;input.selectionEnd=end+1;paintLines();markDirty('md-editor');});paintLines();editorEffect();});
  function liveEffect(){typography('md-live');each('md-live',function(el){el.style.setProperty('--live-editor-font',value('md-live','live.font')+'px');});}
  PROTO['md-live'].props=[choice('typography-theme','MarkdownLiveEditor','TypographyTheme',['Basic','Verdant','TenderGreen','GeekBlack','WeChatFormat'],'Verdant',liveEffect),number('live.font','MarkdownLiveEditor','EditorFontSize',15,10,28,liveEffect)];
  PROTO['md-export'].props=[choice('typography-theme','MarkdownExport','ExportTheme',['Basic','Simple','GeekBlack','WeChatFormat'],'WeChatFormat',function(){typography('md-export');}),choice('typography-size','MarkdownExport','TypographySize',['Normal','Small'],'Normal',function(){typography('md-export');}),choice('export.destination','MarkdownExport','CopyDestination',['WeChat','Zhihu','Juejin'],'WeChat',function(selected){each('md-export',function(el){el.querySelector('[data-copy]').textContent=label('MarkdownExport',selected);});},true)];
  PROTO['md-export'].props[2].type='radio';
  replace('md-export',function(el){el.innerHTML='<div class="demo"><div class="row">'+['PNG','PDF','Word','HTML'].map(function(format){return '<button class="btn" data-format="'+format+'">'+format+'</button>';}).join('')+'<button class="btn pri" data-copy>'+esc(label('MarkdownExport',PROTO['md-export'].props[2].selected))+'</button></div><pre class="catalog-export-result" hidden></pre></div><div class="demo catalog-export-preview catalog-markdown-basic">'+window.CodeWFRenderMarkdownSample(resource('Content.MarkdownExport.Samples.Markdown'))+'</div>';el.querySelectorAll('[data-format]').forEach(function(button){button.onclick=function(){var result=el.querySelector('.catalog-export-result');result.hidden=false;result.textContent='ExportKind.'+button.dataset.format+' · '+value('md-export','typography-theme')+' · '+value('md-export','typography-size');};});el.querySelector('[data-copy]').onclick=function(){var result=el.querySelector('.catalog-export-result');result.hidden=false;result.textContent='CopyKind.'+PROTO['md-export'].props[2].selected+' · '+value('md-export','typography-theme');};typography('md-export');});

  // 工作流参数直接委托当前子页，避免另造一套只写日志的参数。
  var scenes={
    'flow-write':[['Open','md-viewer'],['Edit','md-editor'],['Preview','md-live'],['Export','md-export'],['WriteBack','md-lite']],
    'flow-data':[['Import','datagrid'],['Columns','colde'],['SortFilter','datagrid'],['ExportData','propgrid']],
    'flow-docs':[['Dock','dock'],['Tabs','trapezoid'],['Panels','log'],['Status','status']]
  },steps={};
  PROTO.status.props=statusProps('status',false);replace('status',function(el){el.innerHTML='<div data-status-badges></div><div data-status-cards></div>';statusRender('status',true)(el.querySelector('[data-status-badges]'));statusRender('status',false)(el.querySelector('[data-status-cards]'));});
  Object.keys(scenes).forEach(function(id){steps[id]=0;Object.defineProperty(PROTO[id],'props',{get:function(){return PROTO[scenes[id][steps[id]][1]].props;}});replace(id,function(el){el.innerHTML='<div class="subtabs catalog-scene-steps">'+scenes[id].map(function(step,index){return '<button class="'+(steps[id]===index?'on':'')+'" data-step="'+index+'">'+esc(window.CodeWFPrototypeResources[LANG].Scene['Step.'+step[0]])+'</button>';}).join('')+'</div><div class="catalog-scene-body"></div>';el.querySelectorAll('[data-step]').forEach(function(button){button.onclick=function(){steps[id]=Number(button.dataset.step);renderInto(id,el);updateProperties();};});renderInto(scenes[id][steps[id]][1],el.querySelector('.catalog-scene-body'));});});
  var originalRender=renderInto;
  renderInto=function(id,el){if(!el)return;el.dataset.control=id;originalRender(id,el);if(id==='md-viewer'){typography(id);markdownVirtualization();var viewer=el.querySelector('.catalog-markdown-basic');new MutationObserver(function(){typography(id);markdownVirtualization();}).observe(viewer,{childList:true});}if(id==='md-lite')typography(id);if(id==='md-live')liveEffect();};
  // 暴露只读契约给审计；测试须另外操作真实 DOM 检验参数效果。
  window.CodeWFPrototypePropertyContract=function(id){return (PROTO[id].props||[]).map(function(p){return {id:p.id,label:p.label,type:p.type,readonly:!!p.readonly};});};
  if(ACTIVE_DOC)renderDoc(ACTIVE_DOC);
})();
