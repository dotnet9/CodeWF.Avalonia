# 配色回归与验收范围

## 资源规则

Studio 的八套配色各自定义浅色、深色资源；系统模式只选择明暗，不改变配色。配色源为 `src/Controls/CodeWF.Avalonia.Controls/Themes/Studio/Tokens/`，不在 C# 中根据颜色字面量推断主题。

- `StudioAccent` 用于文字、图标、焦点和装饰；`StudioPrimary` 用于实心主操作。明亮装饰色不能直接与白字组合。
- 主操作的 normal / hover / pressed 分别使用 Primary / PrimaryHover / PrimaryPressed，文字使用 OnPrimary。禁用态另用禁用资源，不以禁用样式覆盖正常态。
- 成功、警告、失败和调试保留各自语义色；徽章的实心背景、卡片的柔和背景分别配套前景，不将所有状态都改成品牌色。
- 文字优先使用动态资源。Markdown 编辑器、脑图和通知先尊重控件、窗口、宿主资源，再使用库默认值；切换主题时已有控件也需要更新。
- 通知默认资源位于样式资源的低优先级位置，避免库默认值压过宿主同键资源。通知文案来自内嵌 JSON，与宿主使用同一个语言实例。

库不引用 Studio 时仍提供独立 Semi Light / Dark 和原有高对比变体。Studio 原型的 CSS 色值由 `scripts/sync_design_catalog.ps1` 从同一批原生资源生成，禁止手工维护第二份颜色表。

## 本轮修复

`12.1.4.25` 修复徽章、状态卡片、Guide、列编辑器、下拉框、斜线、传输箭头、表格、Dock、属性面板、Markdown 行号与选区、脑图、日志及通知中的实际颜色问题。Demo 同步修复 Ribbon、命令搜索、工作流、窗口示例和状态文字；默认蓝色的 Light / Dark 也覆盖实心按钮、勾选框、滑块等第三方主题资源别名，避免只有扩展配色生效。

Markdown 代码行号按代码块实际背景决定前景，不用窗口明暗猜测；代码选区采用不透明背景，避免与不同排版主题合成后使白字不可读。装饰渐变与承载白字的操作渐变分开，操作渐变连同中间插值一起检查。

## 自动回归入口

```powershell
dotnet test tests/CodeWF.Avalonia.Controls.Tests -c Release
dotnet test tests/CodeWF.Avalonia.Content.Tests -c Release
pwsh scripts/sync_design_catalog.ps1 -Check
pwsh scripts/sync_localization_keys.ps1 -Check
$env:CODEWF_AUDIT_OUTPUT = 'E:/github/temp/codewf-prototype-audit'
node design/tests/audit-prototype.cjs
node design/tests/audit-palette.cjs
dotnet run --project scripts/localization-verification -c Release -- --palette-audit E:/github/temp/codewf-palette-audit
```

测试加载真实模板，连续切换同一批控件的主题，并操作鼠标、编辑器和已打开的通知；不只检查资源字典中有没有颜色。普通启用态文字对最终背景至少 4.5:1，作为可辨认控件边界的图形另按 3:1 检查；禁用控件验证禁用、恢复和资源归属，不套用启用文字阈值。表格同时检查正文、表头、选中项、浮动窗口与属性弹层。

原型检查使用浏览器读取最终前景和逐层合成背景，覆盖两种语言、16 个配色与明暗组合。安装器原型另有 `design/tests/audit-installer.cjs`，属于待验收提案，不修改现有安装器。

两套 Demo 的 CI 在完整原生 UI 回归后，使用同一个 NativeAOT 验证器执行 `--palette-audit`；配色审计失败会阻断 CI，不只依赖本机手动运行。该检查不增加另一次原生编译。

## 验收边界

颜色对比通过不等于所有页面视觉一致，也不等于整页已达到 95% 还原。主题跟随、控件交互、库资源覆盖、正式 NativeAOT EXE 自检与独立原生 UI 验证分别留存结果，不能互相替代。

自动离屏验证不操作用户桌面。系统窗口装饰、多 DPI、跨屏、IME、读屏软件、手工拖出和安装/升级/卸载仍需实机验收；Linux/macOS 的构建产物也不能替代对应系统上的交互验收。
