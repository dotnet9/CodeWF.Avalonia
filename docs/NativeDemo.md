# 原生演示台与 Studio 外观

`design/` 中的 HTML 是冻结的设计基准。原生 Demo 使用 Avalonia 12、Semi 的现有控件模板及 Dock 工作区，将库功能放到可操作的界面里。

## 功能入口

- 首页直接试用 Markdown 实时预览、DataGrid 三态自然排序、Dock 工作区。
- 六张能力卡片进入完整演示；导航、属性编辑、源码查看、日志和场景步骤保留。
- 默认 .NET 紫色暗色外观；另外提供 AI 极光（紫色 / 青色渐变）、AI 暖陶，以及原有六套 Semi 配色。三套新增主题均支持深浅模式，原有 Semi 配色沿用各自的主题行为。
- 卡片悬停、按压、首页淡入、品牌轨道呼吸由 Avalonia 原生动画与 Transitions 完成。
- `Ctrl+K` 打开命令面板，`Ctrl+D` 切换深浅，`Ctrl+L` 切换语言，`Ctrl+S` 保存工作区布局。

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

## 运行与检查

```powershell
dotnet run --project demo/CodeWF.Avalonia.Demo -f net10.0
dotnet run --project demo/CodeWF.Avalonia.Demo -f net10.0 -- --smoke
dotnet run --project demo/CodeWF.Avalonia.Demo -f net10.0 -- --capture ./artifacts/studio-qa
dotnet test tests/CodeWF.Avalonia.Controls.Tests --filter FullyQualifiedName~StudioThemeTests
dotnet test tests/CodeWF.Avalonia.Content.Tests --filter "FullyQualifiedName~Lang|FullyQualifiedName~PropertyGrid"
```

冒烟覆盖全部页面、主题变体和场景步骤，并验证首页六个导航入口。`--capture` 忽略用户保存的布局，导出三套新主题的深浅截图、各实验页签、能力卡片、英文与 1120×640 窗口截图，结束后自动退出。

## 原型与库功能映射

首页的品牌标题、双行文案、轨道徽标、三个实验、六张能力卡、五个 Ribbon 快捷入口、整行日志与主题色圈按冻结原型实现。原生控件沿用 Semi / Dock 的模板，字体、窗口按钮、属性编辑器与浏览器 CSS 有平台差异，不作为像素级复刻。

| 原型入口 | 原生页面 / 控件 | 实际库能力 | 验证 |
|---|---|---|---|
| Markdown 工作区 | MarkdownFlow、阅读 / 源码 / 实时编辑 / 导出 | Markdown、Markdown.Lite | 页面渲染；PNG 解码、PDF 签名、Word ZIP 文档与 HTML 产物 |
| 首页 Markdown 实验 | MarkdownEditorView + MarkdownViewer | 编辑器 Text 双向绑定、真实渲染、主题资源 | 绑定回归、首页与深浅截图 |
| 数据交互 | DataFlow、DataGrid、PropertyGrid | DataGrid / TreeDataGrid 增强与属性提交 | 页面渲染、PropertyGrid 回归；首页四行自然排序样例 |
| 多文档工作区 | DocsFlow、Dock、梯形页签 | Dock 布局、Controls 页签与窗口 | 布局保存 / 恢复、显隐、文档路由与渲染 |
| 输入与选择 | Transfer、SearchListBox、MarkupExtension | Controls 穿梭 / 搜索 / If 与 Switch | 页面渲染与对应交互入口 |
| 引导与状态 | Guide、AppGuide、Status、WindowedStackPanel | Controls 引导、状态、虚拟布局 | 页面渲染与场景步骤 |
| 日志与国际化 | Log、Lang、外壳通知与语言切换 | Log、Lang | 日志事件流、双语资源、通知与外壳冒烟 |
| 强类型语言资源 | Lang 页面显示生成的资源键 | Lang.Generator 编译 Demo 的 AdditionalFiles | 生成器真实编译、资源键命名冲突回归；生成器不参与运行期 AOT |

首页预览使用紧凑字号和间距；完整 Markdown 页面保留 18 套可选排版主题，以及公式、图表、代码、编辑和导出能力。`MarkdownFullThemes` 复用 AvaloniaEdit 原生模板并提供编辑器资源，无需加载 Fluent / Simple 应用主题；编辑器调色板沿派生主题识别深浅模式。`MarkdownEditorView.TextProperty` 默认双向绑定，程序装载文本仍不会触发 `MarkdownChanged`，用户输入会同时通知绑定与原有事件。

## Windows NativeAOT 验收

当前验收环境为 Windows x64、.NET SDK 10.0.401，版本 `12.1.4.6`。执行仓库脚本后，直接运行生成的原生 EXE，而不是 `dotnet run`：

```powershell
pwsh scripts/publish_avalonia_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Avalonia.Demo/CodeWF.Avalonia.Demo.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

冒烟需退出 0：24 个页面、13 个场景步骤、312 次页面渲染、5 次外壳渲染，并验证上述导出产物和首页路由。日志位于系统临时目录 `CodeWFAvaloniaDemo/logs`。README 截图来自这个原生产物；GIF 是主题 / 实验切换的原生界面帧。

直接依赖不再声明 Fluent、Simple 或 CommunityToolkit.Mvvm。Dock 的 `Dock.Avalonia.Themes.Simple` 仍提供停靠模板；`Tmds.DBus.Protocol` 由 Avalonia 的 Linux 后端传递引入，用于 D-Bus 桌面服务通信。

发布仍有第三方裁剪 / AOT 分析警告；旧 Prism 对话框模板与动态 XAML Loader 存在版本兼容警告，当前 Demo 不调用这些路径。验收涵盖已列出的页面、产物与操作，不代表所有库 API 和异常路径均已验证。Linux / macOS 留给既有发布流程验证。本版本通过 `v12.1.4.6` 标签触发 NuGet 和演示应用发布。
