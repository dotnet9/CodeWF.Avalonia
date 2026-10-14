/* Focused examples of the public controls; labels are shared with the native demo. */
"use strict";
(function () {
  function text(key) { return window.CodeWFCatalogText[LANG][key] || key; }
  function register(id, options) {
    var entry = window.CodeWFDemoCatalog.flatMap(function(g){return g.items;}).find(function(e){return e.prototypeId===id;});
    options.zh = {t:entry.titleZh,d:entry.package,f:[]};
    options.en = {t:entry.titleEn,d:entry.package,f:[]};
    options.props = options.props || [];
    REG(id, options);
  }
  ['if','switch'].forEach(function(id){
    PROTO[id].xaml = window.CodeWFCatalogDescriptions[LANG][id === 'if' ? 'If' : 'Switch'].Xaml;
  });
  function samples() { return window.CodeWFMarkdownSamples[LANG]; }
  function applyTypography(id) {
    var page = document.querySelector('#seg-'+id+' .catalog-markdown-basic');
    if(!page)return;
    var props=PROTO[id].props, theme=props.find(function(p){return p.id==='typography-theme';}), size=props.find(function(p){return p.id==='typography-size';});
    var tokens=window.CodeWFMarkdownTypography;
    var metrics=Object.assign({},tokens.Common,tokens[theme.value],size.value==='Small'?tokens.CompactTypographyResources:{});
    page.style.fontSize=metrics.CodeWFMarkdownParagraphFontSize+'px';
    page.style.lineHeight=metrics.CodeWFMarkdownParagraphLineHeight+'px';
    page.style.gap=metrics.CodeWFMarkdownBlockSpacing+'px';
    page.querySelectorAll('h1,h2,h3,h4,h5,h6').forEach(function(heading){heading.style.fontSize=metrics['CodeWFMarkdownHeading'+heading.tagName.slice(1)+'FontSize']+'px';});
    page.querySelectorAll('pre').forEach(function(code){code.style.fontSize=metrics.CodeWFMarkdownCodeBlockFontSize+'px';code.style.lineHeight=metrics.CodeWFMarkdownCodeBlockLineHeight+'px';});
  }
  function typographyProperties(id, themes) {
    return [
      {id:'typography-theme',get label(){return samples().themeLabel;},type:'combo',options:themes,value:'Basic',apply:function(){applyTypography(id);}},
      {id:'typography-size',get label(){return samples().sizeLabel;},type:'combo',options:['Normal','Small'],value:'Normal',apply:function(){applyTypography(id);}}
    ];
  }
  // Browser rendering covers the elements in the shared examples; the native page runs Markdig.
  function renderSample(source) {
    var lines = source.replace(/\r\n/g, '\n').split('\n'), output = [], fence = false, code = [], list = false, table = false;
    function inline(value) { return esc(value).replace(/`([^`]+)`/g, '<code>$1</code>').replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>').replace(/\*([^*]+)\*/g, '<em>$1</em>').replace(/\[([^\]]+)\]\((https?:\/\/[^)]+)\)/g, '<a href="$2">$1</a>'); }
    lines.forEach(function(line, index) {
      if (line.startsWith('```')) { if (fence) { output.push('<pre>'+esc(code.join('\n'))+'</pre>'); code=[]; } fence=!fence; return; }
      if (fence) { code.push(line); return; }
      if (!line.startsWith('- ') && list) { output.push('</ul>'); list=false; }
      if (!line.startsWith('|') && table) { output.push('</tbody></table>'); table=false; }
      var heading = line.match(/^(#{1,6}) (.+)/);
      if (heading) { var level=heading[1].length; output.push('<h'+level+'>'+inline(heading[2])+'</h'+level+'>'); }
      else if (line.startsWith('- ')) {
        if (!list) { output.push('<ul>'); list=true; }
        var task=line.match(/^- \[([ xX])\] (.*)/);
        output.push('<li'+(task?' class="markdown-task"':'')+'>'+(task?'<label><input type="checkbox" data-task-line="'+index+'" '+(task[1].trim()?'checked':'')+'> '+inline(task[2])+'</label>':inline(line.slice(2)))+'</li>');
      } else if (line.startsWith('> ')) output.push('<blockquote>'+inline(line.slice(2))+'</blockquote>');
      else if (line.startsWith('|') && !line.includes('---')) {
        var header=!table;
        if(header){output.push('<table><thead>');table=true;}
        output.push('<tr>'+line.split('|').slice(1,-1).map(function(cell){var tag=header?'th':'td';return '<'+tag+'>'+inline(cell.trim())+'</'+tag+'>';}).join('')+'</tr>');
        if(header)output.push('</thead><tbody>');
      }
      else if (line.trim() && !line.startsWith('|')) output.push('<p>'+inline(line)+'</p>');
    });
    if(list)output.push('</ul>');
    if(table)output.push('</tbody></table>');
    return output.join('');
  }
  register('md-viewer', {props:typographyProperties('md-viewer',MD_THEMES),xaml:'<codewf:MarkdownViewer Markdown="{Binding AppliedMarkdown}"\n  TypographyTheme="{Binding Theme}" TypographySize="{Binding Size}" />', render:function(el) {
    var source=samples().viewer;
    el.classList.add('catalog-markdown-page');
    el.innerHTML='<details class="catalog-markdown-input"><summary>'+esc(text('Input'))+'</summary><textarea class="catalog-input"></textarea><div class="row">'+['apply','append','reset'].map(function(action){return '<button class="btn" data-markdown-action="'+action+'">'+esc(samples()[action])+'</button>';}).join('')+'</div></details><div class="catalog-markdown-basic"></div>';
    var input=el.querySelector('textarea'), viewer=el.querySelector('.catalog-markdown-basic'); input.value=source;
    function paint(){
      viewer.innerHTML=renderSample(source);
      applyTypography('md-viewer');
      viewer.querySelectorAll('[data-task-line]').forEach(function(task){task.onchange=function(){var lines=source.replace(/\r\n/g,'\n').split('\n');lines[task.dataset.taskLine]=lines[task.dataset.taskLine].replace(/\[[ xX]\]/,task.checked?'[x]':'[ ]');source=lines.join('\n');input.value=source;markDirty('md-viewer');};});
    }
    el.querySelectorAll('[data-markdown-action]').forEach(function(button){button.onclick=function(){var action=button.dataset.markdownAction;source=action==='reset'?samples().viewer:action==='append'?source+'\n\n'+text('LiveParagraph'):input.value;input.value=source;paint();};});
    paint();
  }});
  register('md-lite', {props:typographyProperties('md-lite',['Basic','Simple','GeekBlack','WeChatFormat']),xaml:'<codewf:MarkdownViewer Markdown="{Binding SampleMarkdown}" />', render:function(el) {el.innerHTML='<div class="catalog-markdown-basic">'+renderSample(samples().lite)+'</div>';applyTypography('md-lite');}});
  var originalEditorRender = PROTO['md-editor'].render;
  PROTO['md-editor'].render = function(el) {
    el.innerHTML = originalEditorRender(el);
    var input = el.querySelector('#edTa');
    if (input) { input.value = samples().editor; input.style.fontSize = '15px'; input.style.height = '300px'; }
  };
  register('status-card',{xaml:'<codewf:StatusCard Width="180" Height="80" Kind="Info" TopText="INFO"\n  BottomText="{codewf:I18n Demo.Catalog.StatusSampleInfo}" />',render:function(el){
    var levels=['Debug','Info','Warn','Error','Fatal'];
    el.innerHTML='<div class="demo"><h4>StatusCard</h4><div class="row catalog-status-row">'+levels.map(function(level){return '<div class="catalog-status card" data-level="'+level+'"><strong>'+level.toUpperCase()+'</strong><span>'+esc(text('StatusSample'+level))+'</span></div>';}).join('')+'</div></div><div class="demo"><h4>Kind / IconWidth</h4><select>'+levels.map(function(l){return '<option'+(l==='Info'?' selected':'')+'>'+l+'</option>';}).join('')+'</select><div class="catalog-status card catalog-driven" data-level="Info"><strong>DRIVEN</strong><span>'+esc(text('StatusDriven'))+'</span></div></div>';
    el.querySelector('select').onchange=function(e){el.querySelector('.catalog-driven').dataset.level=e.target.value;};
  }});
  register('treegrid', {cs:'var source = new FlatTreeDataGridSource<Row>(rows);\ntreeDataGrid.EnableDefaults(source);',render:function(el){
    var rows=[10,2,1,20,3,11], direction=0, selected=new Set();
    el.innerHTML='<div class="demo"><p>'+esc(text('TreeHints'))+'</p><table class="catalog-grid" tabindex="0"><thead><tr><th>Name ↕</th><th>Value</th><th>Description</th></tr></thead><tbody></tbody></table><p class="catalog-selection"></p></div>';
    function paint(){
      el.querySelector('tbody').innerHTML=rows.map(function(n){return '<tr data-row="'+n+'" class="'+(selected.has(n)?'selected':'')+'"><td>item'+n+'</td><td>'+n+'</td><td><span title="'+esc(text('TreeLongText'))+'">'+esc(text('TreeLongText'))+'</span></td></tr>';}).join('');
      el.querySelectorAll('[data-row]').forEach(function(row){row.onclick=function(e){var n=Number(row.dataset.row);if(!e.ctrlKey)selected.clear();if(selected.has(n))selected.delete(n);else selected.add(n);paint();};});
      el.querySelector('.catalog-selection').textContent=text('TreeSelection').replace('{0}',selected.size).replace('{1}',rows.length);
    }
    el.querySelector('th').onclick=function(){direction=(direction+1)%3;rows=direction?[10,2,1,20,3,11].sort(function(a,b){return direction===1?a-b:b-a;}):[10,2,1,20,3,11];paint();};
    el.querySelector('table').onkeydown=function(e){if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='a'){e.preventDefault();rows.forEach(function(n){selected.add(n);});paint();}};
    paint();
  }});
  register('expansion',{xaml:'<codewf:ContentExpansionAnimator IsExpanded="{Binding IsExpanded}"\n  Orientation="Horizontal" Duration="{Binding Duration}" Animate="{Binding Animate}"\n  ExpandedSize="{Binding ExpandedSize}" />',render:function(el){
    el.innerHTML='<div class="demo"><p>'+esc(text('ExpansionHint'))+'</p><div class="row"><button class="btn pri" data-toggle>'+esc(text('ExpansionToggle'))+'</button><label><input type="checkbox" checked data-animate> '+esc(text('ExpansionAnimate'))+'</label></div><p>Duration (ms) <input type="range" min="0" max="1000" value="300" data-duration></p><p>ExpandedSize <input type="range" min="100" max="300" value="200" data-size></p></div><div class="demo"><h4>'+esc(text('ExpansionHorizontal'))+'</h4><div class="catalog-expand horizontal"><div>'+esc(text('ExpansionContent'))+'</div></div></div><div class="demo"><h4>'+esc(text('ExpansionVertical'))+'</h4><div class="catalog-expand vertical"><div>'+esc(text('ExpansionContent'))+'</div></div></div>';
    var expanded=true;
    function update(){el.querySelectorAll('.catalog-expand').forEach(function(panel){panel.style.transitionDuration=(el.querySelector('[data-animate]').checked?el.querySelector('[data-duration]').value:0)+'ms';panel.style[panel.classList.contains('horizontal')?'width':'height']=(expanded?el.querySelector('[data-size]').value:0)+'px';panel.style.opacity=expanded?1:0;});}
    el.querySelector('[data-toggle]').onclick=function(){expanded=!expanded;update();};
    el.querySelectorAll('input').forEach(function(input){input.oninput=update;});update();
  }});
  register('md-live',{xaml:'<codewf:MarkdownLiveEditorView Markdown="{Binding Doc}" />',cs:'liveEditor.FocusBlock(0);\nawait liveEditor.ExecuteAsync(MarkdownEditorAction.Table);',render:function(el){
    var blocks=samples().live.replace(/\r\n/g,'\n').split(/\n\n+/), active=-1;
    el.innerHTML='<div class="demo"><p>'+esc(text('LiveHint'))+'</p><div class="row"><button class="btn" data-insert>'+esc(text('LiveInsert'))+'</button><button class="btn" data-focus>'+esc(text('LiveFocus'))+'</button><button class="btn" data-table>'+esc(text('LiveTable'))+'</button></div><div class="catalog-live mdr"></div></div>';
    function renderBlock(s){
      return renderSample(s);
    }
    function paint(focus){
      el.querySelector('.catalog-live').innerHTML=blocks.map(function(s,i){return '<div class="catalog-block" tabindex="0" data-block="'+i+'">'+(i===active?'<textarea>'+esc(s)+'</textarea>':renderBlock(s))+'</div>';}).join('');
      el.querySelectorAll('[data-block]').forEach(function(block){
        var i=Number(block.dataset.block), input=block.querySelector('textarea');
        if(input){input.oninput=function(){blocks[i]=input.value;markDirty('md-live');};input.onblur=function(){active=-1;paint();};if(focus)input.focus();}
        else {block.onclick=function(){active=i;paint(true);};block.onkeydown=function(e){if(e.key==='Enter'){active=i;paint(true);}};}
      });
    }
    el.querySelector('[data-focus]').onclick=function(){active=0;paint(true);};
    el.querySelector('[data-insert]').onclick=function(){blocks.push(text('LiveParagraph'));active=blocks.length-1;paint(true);};
    el.querySelector('[data-table]').onclick=function(){blocks.push('| Name | Value |\n| --- | --- |\n| CodeWF | 42 |');active=blocks.length-1;paint(true);};
    paint();
  }});
  var originalPropertyGrid = renderPGrid;
  renderPGrid = function(id) {
    originalPropertyGrid(id);
    if(id!=='md-viewer' && id!=='md-lite')return;
    var panel=$('pGrid');
    if($('pgCnt'))$('pgCnt').textContent='('+PROTO[id].props.length+')';
    var note=document.createElement('p');note.className='hint';note.textContent=text('Preview');panel.appendChild(note);
  };
  if(ACTIVE_DOC) renderDoc(ACTIVE_DOC);
})();
