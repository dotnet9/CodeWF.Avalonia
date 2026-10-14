/* Focused examples of the public controls; labels are shared with the native demo. */
"use strict";
(function () {
  function text(key) { return window.CodeWFCatalogText[LANG][key] || key; }
  function register(id, options) {
    var entry = window.CodeWFDemoCatalog.flatMap(function(g){return g.items;}).find(function(e){return e.prototypeId===id;});
    options.zh = {t:entry.titleZh,d:entry.package,f:[]};
    options.en = {t:entry.titleEn,d:entry.package,f:[]};
    options.props = [];
    REG(id, options);
  }
  ['if','switch'].forEach(function(id){
    PROTO[id].xaml = window.CodeWFCatalogDescriptions[LANG][id === 'if' ? 'If' : 'Switch'].Xaml;
  });
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
    var blocks=['# MarkdownLiveEditorView',text('LiveParagraph'),'- Markdown\n- FocusBlock\n- ExecuteAsync'], active=-1;
    el.innerHTML='<div class="demo"><p>'+esc(text('LiveHint'))+'</p><div class="row"><button class="btn" data-insert>'+esc(text('LiveInsert'))+'</button><button class="btn" data-focus>'+esc(text('LiveFocus'))+'</button><button class="btn" data-table>'+esc(text('LiveTable'))+'</button></div><div class="catalog-live mdr"></div></div>';
    function renderBlock(s){
      if(s.startsWith('# '))return '<h2>'+esc(s.slice(2))+'</h2>';
      if(s.startsWith('- '))return '<ul>'+s.split('\n').map(function(l){return '<li>'+esc(l.slice(2))+'</li>';}).join('')+'</ul>';
      if(s.startsWith('|'))return '<table>'+s.split('\n').filter(function(l){return !l.includes('---');}).map(function(l){return '<tr>'+l.split('|').slice(1,-1).map(function(c){return '<td>'+esc(c.trim())+'</td>';}).join('')+'</tr>';}).join('')+'</table>';
      return '<p>'+esc(s)+'</p>';
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
  if(ACTIVE_DOC) renderDoc(ACTIVE_DOC);
})();
