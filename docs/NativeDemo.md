# CodeWF.Avalonia 原生 Demo

保留 Ribbon + Dock，按功能分类查找单个控件。原型与实现由 `design/catalog.json` 共享导航契约，详细清单见 [设计目录](../design/README.md)。

## 页面结构

- 首页仅包含介绍、关于；介绍先展示 API 目录。
- 主功能组的一个菜单叶子对应一个控件或公开 API。
- 综合示例统一放在末组，首页的 Markdown 源码编辑与预览、自然排序表格、Dock 面板切换 默认折叠。
- 演示始终可见；源码按需展开，有内容才显示相应语言标签。
- 标题栏色圈和下拉同步切换 .NET 紫、AI 极光、AI 暖陶与 Semi 原有配色。

## 共享库接口

`StudioTheme` 位于 `CodeWF.Avalonia.Controls`，是可选样式层。它不替换第三方控件，需放在 Semi 与 CodeWF 控件主题之后：

```xml
<Application.Styles>
  <semi:SemiTheme />
  <codewf:ACSemiTheme />
  <codewf:DataGridSemiTheme />
  <codewf:TreeDataGridSemiTheme />
  <codewf:DockSemiTheme />
  <codewf:StudioTheme />
  <Style Selector="codewf|PropertyGrid">
    <Setter Property="ThemeOverrides" Value="{DynamicResource StudioPropertyGridTheme}" />
  </Style>
</Application.Styles>
```

命名空间与 README 的快速开始一致。`StudioTheme.DotNetLight / DotNetDark`、`AuroraLight / AuroraDark`、`EmberLight / EmberDark` 均继承 Semi 的 Light / Dark，赋给 `Application.Current.RequestedThemeVariant` 即可切换。

颜色资源包含 `StudioBackground`、`StudioSurface`、`StudioRaised`、`StudioBorder`、`StudioText`、`StudioMuted`、`StudioAccent`、`StudioSecondary`、`StudioGradient`、`StudioTextGradient`。`studio-page`、`studio-hero`、`studio-card`、`studio-capability` 等 Classes 按需使用。标题栏色圈用现有 ListBox 实现选中、键盘切换与悬停动画，和下拉框共享选中项。

新增配色遵循 Semi 的资源结构：`Themes/Studio/Tokens/_index.axaml` 按显式 `ThemeVariant` 加载各配色字典；各字典声明颜色令牌、画刷及控件资源别名。标题栏色圈也读取 `StudioSwatch*` 资源。`StudioTheme.axaml.cs` 仅声明六个变体并加载 AXAML，不根据 RGB 色值判断主题，不在 C# 中拼装颜色字典。默认 Light / Dark 字典只补充 Studio 资源，Semi 的水生、沙漠等扩展配色继续使用自身控件资源。

现有 `PropertyGrid.ThemeOverrides` 接收包含 `Pg*` 键的平面 `ResourceDictionary`。控件复制覆盖值，允许多个面板共享同一份配置；设置 null 恢复内置主题。这个扩展不改变 `PropertyItem` 的纯数据模型，也不要求使用 Studio 主题。

`CodeWF.Avalonia.Controls.Presentation.StudioUi` 组合现有控件来构建 C# 视图，样式随 `StudioTheme` 一起加载。`Heading`、`Sub`、`Hint`、`Key`、`Code`、`Card`、`Button` 和 `Table` 的文本参数支持 `@资源键`，自动创建可随语言切换更新的绑定：

```csharp
var heading = StudioUi.Heading("@App.Overview.Title");
var label = new TextBlock();
label.Bind(TextBlock.TextProperty, new I18nExtension("App.Overview.Title").CreateBinding());
```

调用前需初始化 `I18nManager` 并加载应用语言配置。普通参数是原样文本；用户编辑的样例内容与运行结果不会因语言切换而丢失。

## MVVM 与国际化

外壳、首页、数据表格、属性表、状态组件、语言切换、Markdown 阅读 / 导出与日志页面的状态和命令由 ViewModel 管理。AXAML 后置保留初始化、控件事件接线和剪贴板 / 文件选择 / 窗口等视图适配。PropertyGrid 的属性提交委托给对应 ViewModel。

界面中文与代码示例位于 `demo/CodeWF.Avalonia.Demo/I18n/`。AXAML 使用 `I18n`，C# 动态视图使用资源绑定。文化名称保留语言自身的显示名。测试检查中英文键集合、引用缺键、英文翻译与 C# / AXAML 的中文界面字面量。


## 目录与回归检查

目录以源生成 JSON 元数据读取，页面工厂使用显式分支，避免 AOT 中依赖反射发现页面。测试检查重复菜单 / API、双语标题、显式工厂和原型生成数据一致性；修改目录后执行 `pwsh scripts/sync_design_catalog.ps1`。

```powershell
dotnet test CodeWF.Avalonia.slnx -c Release
```

## Windows NativeAOT 验收

验收环境为 Windows x64、.NET SDK 10.0.303，版本 `12.1.4.10`。`global.json` 选择稳定版 .NET 10 SDK，允许使用后续 10.0 功能版本，不自动选择预览版 SDK。语言源生成器以 Roslyn 5.0 为依赖基线，可在稳定版 .NET 10 编译器中加载。用本仓脚本发布后直接启动原生 EXE：

```powershell
pwsh scripts/publish_avalonia_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Avalonia.Demo/CodeWF.Avalonia.Demo.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

冒烟退出 0：32 个页面、13 个场景步骤、416 次主题渲染、5 次外壳渲染；另外验证首页绑定、六个导航入口以及 PNG 解码、PDF 文件头、Word 文档和 HTML 导出内容。

`--capture` 忽略用户存档布局，输出新配色的深浅、实验、英文、紧凑窗口和独立 API 页面截图，结束后退出。README 的 PNG 与 GIF 来自本轮原生 EXE。

展开控件额外验证默认展开首屏、横向 / 纵向动画、快速反向、关闭动效及卸载取消。`studio-expansion.gif` 来自原生控件展开 / 折叠时的 32 个渲染帧；`studio-interactions.gif` 是主题与实验截图巡览。

## 验收限制

Markdown 的第三方渲染和导出路径仍有裁剪 / AOT 分析警告；Linux / macOS 未在本轮本机验收。 页面渲染通过不等于全部 API、异常路径和平台行为均已验证。

版本标签 `v12.1.4.10` 触发现有 NuGet 与 Demo 发布流程。

## 本轮改进

- 调整为“输入与选择”“反馈与引导”，列管理移入数据展示；新增 ContentExpansionAnimator 独立页，目录共 32 页。
- LiveEditor 原型改为真实的单栏块编辑；MarkdownViewer 默认折叠输入，先展示渲染结果。
- TreeDataGrid 展示平面源的三态排序、长文本提示、多选与计数；状态卡片统一尺寸和样例。
- 当前命令 / 导航与 Dock 页签统一选中，按钮使用 Semi OutlineButton，避免覆盖第三方控件内部按钮；右侧空参数页回退全局设置，调小属性与日志面板初始占比。
- 修复 ContentExpansionAnimator 默认状态与进度不一致、收起目标错误及旧动画覆盖反向状态；状态属性改为下拉，样例文案适配卡片尺寸。

以上不是全部公开 API 和视觉状态的逐项验收声明。键盘焦点、跨屏浮动、所有 Markdown 扩展语法及长时间性能仍应按具体应用继续验证。

本轮记录：2026-10-14，Windows x64 / .NET SDK 10.0.303。本仓 290 项单元测试通过。本仓发布脚本成功，最终原生 EXE 的 `--smoke`、`--capture` 均退出 0；捕获 31 个非首页目录页和 32 个 Ribbon 状态，实际按钮的数量与顺序均与目录一致。浏览器 32 个路由和重点交互检查无脚本异常。

## 字体、布局与分类对照

| 项目 | 原型与原生采用的指标 |
|---|---|
| 标题栏 / 分类行 | 52px / 42px |
| Ribbon 分类 / 命令 / 子组字号 | 12px / 11px / 10px |
| 页面标题 / 说明 / 导航叶子 | 19px / 12px / 11.5px |
| 命令排列 | 按用途分子组；密集分类双行，完整 API 名不换行，窄窗口可横向滚动 |
| 检查尺寸 / 语言 | 1440×860、1120×640；中文、英文 |
| Markdown Basic 排版 | 正文 16px、一级标题 32px、二级标题 24px；原型读取库内排版令牌 |

Markdown 导出归入 Markdown，控件的多个状态与方法保留在同页，综合流程放在末组。属性栏有页面参数时显示参数，否则回退全局设置；原型不再额外堆放空参数和状态统计，全局标签与配色名称从原生双语资源生成。

逐页检查发现并修复两个库缺陷：`SwitchBinding` 现在解析绑定形式的 `When` 分支键，键和结果变化都会重新求值；梯形 TabControl / TabStrip 模板恢复内边距、最小高度和选中形状。新增回归测试，最终原生截图也确认运行中分支与梯形页签正确显示。

README 的 PNG 与 GIF 已替换为本轮原生 EXE 截图。字体度量、Dock 页签、PropertyGrid 编辑器和原生窗口行为仍取决于 Semi / Dock 与平台，HTML 也不运行 Markdig，不承诺所有扩展语法及像素完全一致。第三方 Prism 默认对话框及旧运行时 XAML 加载器存在兼容性分析警告，本轮验收覆盖 Demo 实际调用路径。

## 目录和本地化维护门禁（2026-10-14 补充）

页面工厂、导航常量和原生/浏览器图标统一由 `design/catalog.json` 管理；`scripts/sync_design_catalog.ps1 -Check` 会拒绝过期的 C# 与 JavaScript 生成物。该 JSON 是 Demo 构建输入，生成文件随源码提交。

全部历史编号资源键改为固定语义键，保留原有翻译内容；切换语言重建属性行时保留提交与浏览命令实例，运行冒烟覆盖这条路径。UTF-8 检查接受有或无 BOM，不把 BOM 视为防止错误转码的保证。

本次单元测试 291 项通过；可复跑的 Chromium 审计执行 128 次路由/语言/窗口宽度检查，覆盖实际 Ribbon 和导航点击、内容非空、源码面板及被原型 guard 捕获的脚本异常。审计命令和维护步骤见 [设计目录](../design/README.md)。
