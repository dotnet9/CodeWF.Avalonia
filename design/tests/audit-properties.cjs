const assert = require('node:assert/strict');

// 契约来自原生页面的 BuildProperties；效果检查通过实际表单事件执行。
const contracts = {
  welcome:[],about:[],searchlist:['text:r','text:r','text:r'],transfer:['text:r','text:r'],
  'status-badge':['combo','text'],'status-card':['combo','number'],guide:['text:r'],notifications:['text'],
  datagrid:['bool','number'],colde:['text:r'],treegrid:['bool'],propgrid:['combo','bool','bool','number'],log:['combo','number'],
  codewfwindow:['number','number','number','bool'],titlebar:['text'],dock:['text:r'],trapezoid:['text:r'],
  wsp:['bool','number','number','number'],'diagonal-line':['number'],expansion:[],
  'md-viewer':['combo','combo','bool','number'],'md-lite':['combo','combo'],'md-editor':['number','bool','bool'],
  'md-live':['combo','number'],'md-export':['combo','combo','radio'],if:['text:r'],switch:['text:r'],lang:[],
  'flow-write':['combo','combo','bool','number'],'flow-data':['bool','number'],'flow-docs':['text:r'],appguide:['text:r']
};
exports.verifyContract = async function (page, route) {
  assert.ok(Object.hasOwn(contracts,route), `Missing native property contract: ${route}`);
  const contract = await page.evaluate(id => window.CodeWFPrototypePropertyContract(id), route);
  assert.deepEqual(contract.map(item => item.type+(item.readonly?':r':'')),contracts[route],`Native property kinds: ${route}`);
  const expected=contract.length||4;
  assert.equal(await page.locator('#pGrid .rw').count(),expected,`Property rows: ${route}`);
  assert.equal(await page.locator('#pgCnt').innerText(),`(${expected})`,`Property count: ${route}`);
  for(const item of contract){
    assert.ok(item.label.trim(),`Empty property label: ${route}/${item.id}`);
    const control=page.locator('#pGrid [data-id="'+item.id+'"]');
    assert.equal(await control.count(),1,`Property editor: ${route}/${item.id}`);
    assert.equal(await control.locator('xpath=ancestor::div[contains(@class,"rw")]').locator(':scope > label').innerText(),item.label);
    if(item.readonly)assert.equal(await control.getAttribute('readonly'),'');
  }
};
exports.verifyEffects = async function (page,culture) {
  async function open(route){await page.evaluate(id=>openDoc(id),route);}
  function control(id){return page.locator('#pGrid [data-id="'+id+'"]');}
  async function edit(id,text){await control(id).fill(String(text));await control(id).press('Tab');}
  async function choose(id,index){if(await control(id).evaluate(el=>el.tagName==='SELECT'))await control(id).selectOption({index});else await control(id).locator('input[type=radio]').nth(index).check();}
  async function toggle(id){await control(id).click();}
  await open('status-badge');
  await choose('st.kind',3);await edit('st.badgeText','Badge '+culture);
  assert.equal(await page.locator('.catalog-driven').getAttribute('data-level'),'Error');
  assert.equal(await page.locator('.catalog-driven span').innerText(),'Badge '+culture);
  await choose('st.kind',1);
  await open('status-card');await choose('st.kind',4);await edit('st.iconWidth',52);
  assert.equal(await page.locator('.catalog-driven').getAttribute('data-level'),'Fatal');
  assert.equal(await page.locator('.catalog-driven .catalog-status-icon').evaluate(el=>getComputedStyle(el).width),'52px');
  await choose('st.kind',1);await edit('st.iconWidth',32);
  await open('titlebar');await edit('Title','CodeWF '+culture);
  assert.equal(await page.locator('#seg-titlebar strong').innerText(),'CodeWF '+culture);
  await open('diagonal-line');await edit('Thickness',9);
  assert.equal(await page.locator('#seg-diagonal-line line').getAttribute('stroke-width'),'9');
  await open('notifications');await edit('Message','Notification '+culture);await page.locator('#seg-notifications button').click();
  assert.ok((await page.locator('body').innerText()).includes('Notification '+culture));
  await open('treegrid');await page.locator('#seg-treegrid table').press('Control+a');
  assert.equal(await page.locator('#seg-treegrid tr.selected').count(),6);
  await control('SingleSelect').press('Space');assert.equal(await control('SingleSelect').getAttribute('aria-checked'),'true');assert.equal(await page.locator('#seg-treegrid tr.selected').count(),1);
  await page.locator('#seg-treegrid table').press('Control+a');assert.equal(await page.locator('#seg-treegrid tr.selected').count(),1);
  await toggle('SingleSelect');
  await open('searchlist');const total=Number(await control('sl.total').inputValue());
  await page.locator('#seg-searchlist [data-search]').fill('StatusBadge');await page.waitForTimeout(450);
  assert.equal(await control('sl.filtered').inputValue(),'1');assert.ok(total>10);
  await page.locator('#seg-searchlist .fl').click();assert.equal(await control('sl.selected').inputValue(),'1');
  await open('transfer');await page.locator('#seg-transfer [data-side="left"] .it').first().click();await page.locator('#seg-transfer [data-move="right"]').click();
  assert.equal(await control('tr.available').inputValue(),'4');assert.equal(await control('tr.selected').inputValue(),'3');
  await page.locator('#seg-transfer [data-side="right"] .it').last().dblclick();assert.equal(await control('tr.available').inputValue(),'5');
  await open('datagrid');await edit('dg.rowHeight',48);
  assert.equal(await page.locator('#seg-datagrid tbody tr').first().evaluate(el=>el.getBoundingClientRect().height),48);
  await page.locator('#seg-datagrid th').first().click();assert.equal(await page.locator('#seg-datagrid tbody td').first().innerText(),'item1');
  await page.locator('#seg-datagrid th').first().click();assert.equal(await page.locator('#seg-datagrid tbody td').first().innerText(),'item20');
  await page.locator('#seg-datagrid th').first().click();assert.equal(await page.locator('#seg-datagrid tbody td').first().innerText(),'item10');
  await toggle('dg.defaults');assert.equal(await page.locator('#seg-datagrid td span[title]').count(),0);await toggle('dg.defaults');await edit('dg.rowHeight',32);
  await open('propgrid');await choose('pg.mode',1);assert.equal(await page.locator('.catalog-property-demo th').first().innerText(),'Description');
  await toggle('pg.toolbar');assert.equal(await page.locator('.catalog-property-tools').isVisible(),false);
  await toggle('pg.description');assert.equal(await page.locator('.catalog-property-description').isVisible(),false);
  await edit('pg.nameWidth',190);assert.equal(await page.locator('.catalog-property-demo th').first().evaluate(el=>el.getBoundingClientRect().width),190);
  await toggle('pg.toolbar');await toggle('pg.description');await choose('pg.mode',0);await edit('pg.nameWidth',148);
  await open('codewfwindow');await edit('win.height',54);await edit('win.caption',44);await edit('win.corner',16);await toggle('win.extend');
  assert.equal(await page.locator('.catalog-window .catalog-titlebar').first().evaluate(el=>el.getBoundingClientRect().height),54);
  assert.equal(await page.locator('.catalog-window .catalog-caption').first().evaluate(el=>el.getBoundingClientRect().width),44);
  assert.equal(await page.locator('.catalog-window').first().evaluate(el=>getComputedStyle(el).borderRadius),'16px');
  assert.equal(await page.locator('.catalog-window.content-extended').count(),3);
  await toggle('win.extend');await edit('win.height',36);await edit('win.caption',32);await edit('win.corner',8);
  await open('log');await page.locator('#seg-log [data-batch]').click();await choose('log.min',4);
  assert.ok(await page.locator('.catalog-log [data-level="Error"]').count()>0);
  assert.equal(await page.locator('.catalog-log [data-level="Information"]').count(),0);
  await choose('log.min',0);await edit('log.max',100);assert.equal(await page.locator('.catalog-log>div').count(),100);
  await open('wsp');assert.ok(await page.locator('.catalog-wsp .blk').count()<5000);
  await toggle('wsp.virtual');assert.equal(await page.locator('.catalog-wsp .blk').count(),5000);
  await toggle('wsp.virtual');await edit('wsp.spacing',20);assert.equal(await page.locator('.catalog-wsp .blk').nth(1).evaluate(el=>el.offsetTop),60);
  await page.locator('#seg-wsp [data-clear]').click();assert.equal(await page.locator('.catalog-wsp .blk').count(),0);
  await page.locator('#seg-wsp [data-generate]').click();assert.ok(await page.locator('.catalog-wsp .blk').count()>0);
  await open('md-viewer');await edit('md.threshold',1);assert.equal(await page.locator('#seg-md-viewer .catalog-markdown-basic').getAttribute('data-virtualizing'),'true');
  await toggle('md.virtual');assert.equal(await page.locator('#seg-md-viewer .catalog-markdown-basic').getAttribute('data-virtualizing'),'false');
  await toggle('md.virtual');await edit('md.threshold',40);
  for(const route of ['md-viewer','md-lite']){await open(route);await choose('typography-size',1);assert.equal(await page.locator('#seg-'+route+' .catalog-markdown-basic').evaluate(el=>getComputedStyle(el).fontSize),'14px');await choose('typography-size',0);}
  await open('md-editor');await edit('editor.font',22);assert.equal(await page.locator('#edTa').evaluate(el=>getComputedStyle(el).fontSize),'22px');
  await toggle('editor.lines');assert.equal(await page.locator('.catalog-editor-lines').isVisible(),false);
  await page.locator('#edTa').fill('');await page.locator('#edTa').press('[');assert.equal(await page.locator('#edTa').inputValue(),'[]');
  await toggle('editor.pair');await page.locator('#edTa').fill('');await page.locator('#edTa').press('[');assert.equal(await page.locator('#edTa').inputValue(),'[');
  await toggle('editor.pair');await toggle('editor.lines');await edit('editor.font',15);
  await open('md-live');await choose('typography-theme',0);await edit('live.font',23);await page.locator('#seg-md-live [data-focus]').click();
  assert.equal(await page.locator('#seg-md-live textarea').evaluate(el=>getComputedStyle(el).fontSize),'23px');
  await open('md-export');await choose('export.destination',2);await page.locator('#seg-md-export [data-copy]').click();assert.match(await page.locator('.catalog-export-result').innerText(),/CopyKind.Juejin/);
  const scenes={'flow-write':[4,3,2,3,2],'flow-data':[2,1,2,4],'flow-docs':[1,1,2,2]};
  for(const [route,counts]of Object.entries(scenes)){await open(route);for(let index=0;index<counts.length;index++){await page.locator('#seg-'+route+' [data-step="'+index+'"]').click();assert.equal(await page.locator('#pGrid .rw').count(),counts[index],`${route} step ${index}`);}await page.locator('#seg-'+route+' [data-step="0"]').click();}
};
