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

颜色资源包含 `StudioBackground`、`StudioSurface`、`StudioRaised`、`StudioBorder`、`StudioText`、`StudioMuted`、`StudioAccent`、`StudioSecondary`、`StudioGradient`。`studio-page`、`studio-hero`、`studio-card`、`studio-capability` 等 Classes 按需使用。

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

这些新增接口先随源码提供；本次改造不发布 NuGet。运行检查以 Windows / .NET 10 为基准，跨平台发布与 NativeAOT 应继续由对应发布任务验证。
