# CodeWF.Avalonia

[![CI](https://github.com/dotnet9/CodeWF.Avalonia/actions/workflows/ci.yml/badge.svg)](https://github.com/dotnet9/CodeWF.Avalonia/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Controls?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Controls)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Target](https://img.shields.io/badge/target-net10.0-512bd4)](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)

面向 Avalonia 12 的 UI 控件与能力库合集：基础自定义控件、表格增强、Dock 布局、Markdown 渲染与编辑、国际化、日志查看，均以 `CodeWF.Avalonia.*` 前缀发布，XAML 命名空间统一挂 `https://codewf.com`。

A suite of UI control and capability libraries for Avalonia 12, published with the `CodeWF.Avalonia.*` prefix and a unified XAML namespace `https://codewf.com`.

> 版本号前三位跟随 Avalonia（当前 12.1.4），第四位为本仓修订号。

## 这个仓库解决什么 / What it solves

| 场景 | 用到的包 |
|---|---|
| 桌面应用外壳：统一标题栏、新手引导、状态徽标/卡片、穿梭框与可搜索列表 | `CodeWF.Avalonia.Controls` |
| 数据表格：三态排序（升/降/取消）、自然排序、截断 ToolTip；树形表格同款，另附通用属性面板 | `CodeWF.Avalonia.DataGrid`、`CodeWF.Avalonia.TreeDataGrid` |
| 多文档工作区：Dock 布局适配、梯形文档页签、中英文本地化 | `CodeWF.Avalonia.Dock` |
| Markdown 写作与发布：渲染、图片/公式/Mermaid/代码高亮、PNG/PDF/Word/公众号 HTML 导出、源码与所见即所得编辑器 | `CodeWF.Avalonia.Markdown.Lite`、`CodeWF.Avalonia.Markdown` |
| 运行期观测与本地化：实时日志查看、桌面通知、I18n 标记扩展与强类型 Key 生成 | `CodeWF.Avalonia.Log`、`CodeWF.Avalonia.Lang`、`CodeWF.Avalonia.Lang.Generator` |

## 包一览 / Packages

「依赖」列只列第三方与本仓包依赖；所有包均面向 `net10.0`（`Lang.Generator` 为 `netstandard2.0`）。**只有 4 个包依赖 Semi.Avalonia**。

| 包 | 说明 | 依赖 |
|---|---|---|
| [CodeWF.Avalonia.Controls](https://www.nuget.org/packages/CodeWF.Avalonia.Controls) | 基础自定义控件 + Semi 主题：Guide 引导、Transfer/SearchListBox、StatusBadge/StatusCard、CodeWFWindow/标题栏、WindowedStackPanel、梯形页签、If/Switch 标记扩展与转换器 | Avalonia、**Semi.Avalonia** |
| [CodeWF.Avalonia.DataGrid](https://www.nuget.org/packages/CodeWF.Avalonia.DataGrid) | 官方 DataGrid 增强：三态排序、自然排序、智能 ToolTip，内置 Semi 主题入口 | Avalonia.Controls.DataGrid、**Semi.Avalonia** |
| [CodeWF.Avalonia.TreeDataGrid](https://www.nuget.org/packages/CodeWF.Avalonia.TreeDataGrid) | 社区版 TreeDataGrid.Avalonia 增强：三态排序、Ctrl+A 全选、智能 ToolTip，内置 Semi 主题（本地 vendor，不依赖商业版）；含通用 PropertyGrid | TreeDataGrid.Avalonia、Avalonia.Controls.ColorPicker、**Semi.Avalonia**、CodeWF.Avalonia.Lang |
| [CodeWF.Avalonia.Dock](https://www.nuget.org/packages/CodeWF.Avalonia.Dock) | 基于 Dock.Avalonia 的布局扩展与 Semi 风格主题，含中英文本地化 | Dock.Avalonia、Dock.Avalonia.Themes.Simple、**Semi.Avalonia** |
| [CodeWF.Avalonia.Markdown.Lite](https://www.nuget.org/packages/CodeWF.Avalonia.Markdown.Lite) | Markdown 渲染引擎（常规元素）+ 18 套排版主题 | Avalonia、Markdig、CodeWF.Avalonia.Controls\*、CodeWF.Avalonia.Lang\* |
| [CodeWF.Avalonia.Markdown](https://www.nuget.org/packages/CodeWF.Avalonia.Markdown) | 完整包：图片/公式/Mermaid/代码高亮/多格式导出/源码与所见即所得编辑器 | Avalonia.AvaloniaEdit、AnimatedImage.Avalonia、Mermaider、SkiaSharp、Svg.Skia、Svg.Controls.Skia.Avalonia、Sylinko.CSharpMath.Avalonia、TextMateSharp(.Grammars)、CodeWF.Avalonia.Markdown.Lite、CodeWF.Avalonia.Controls\*、CodeWF.Avalonia.Lang\* |
| [CodeWF.Avalonia.Log](https://www.nuget.org/packages/CodeWF.Avalonia.Log) | 日志查看控件与桌面通知（配合 CodeWF.Toolkit.Logging） | Avalonia、Xaml.Behaviors、CodeWF.Toolkit.Logging |
| [CodeWF.Avalonia.Lang](https://www.nuget.org/packages/CodeWF.Avalonia.Lang) | 插件化国际化：I18n 标记扩展 + 内置 Json/Xml/Resx 加载器 | Avalonia |
| [CodeWF.Avalonia.Lang.Generator](https://www.nuget.org/packages/CodeWF.Avalonia.Lang.Generator) | 编译期源生成器：从语言资源生成强类型 Key | Roslyn（`PrivateAssets=all`，netstandard2.0） |

\* 标注的依赖由对应包经传递引用带入，无需单独安装；它们本身不依赖 Semi，见 [主题与依赖](#主题与依赖--themes-and-dependencies)。

## 快速开始 / Quick start

```bash
# 最小可用：控件 + 官方 DataGrid 增强
dotnet add package CodeWF.Avalonia.Controls
dotnet add package CodeWF.Avalonia.DataGrid
```

`App.axaml`（**注意 `xmlns:semi`，缺少它 `<semi:SemiTheme/>` 无法解析**）：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:semi="https://irihi.tech/semi"
             xmlns:codewf="https://codewf.com">
  <!-- xmlns:semi  → Semi.Avalonia 包；xmlns:codewf → CodeWF.Avalonia.* 包 -->
  <Application.Styles>
    <!-- Semi 基础主题：Controls / DataGrid / TreeDataGrid / Dock 四个包需要；Markdown、Log、Lang 不需要 -->
    <semi:SemiTheme Locale="zh-CN" />
    <!-- 基础控件样式（Guide / Transfer / StatusBadge / CodeWFWindow 等） -->
    <codewf:ACSemiTheme />
    <!-- 官方 DataGrid 的 Semi 样式 + 本仓增强样式，无需再引 Semi 的 DataGrid 主题 -->
    <codewf:DataGridSemiTheme />
  </Application.Styles>
</Application>
```

上面三行按需增删即可，下面是几种常见的最小组合：

| 你只需要 | Application.Styles 里放什么 |
|---|---|
| Markdown 渲染（Lite，不引入 Semi） | `<codewf:MarkdownThemes />` |
| Markdown 完整能力（编辑器/导出/图表） | `<codewf:MarkdownFullThemes />`（继承自 Lite 的 `MarkdownThemes`） |
| 日志查看控件 | `<codewf:CodeWFLogTheme />` + 先初始化 `CodeWF.Toolkit.Logging` 的 `Logger` |
| 树形表格 + 属性面板 | `<semi:SemiTheme />`、`<codewf:TreeDataGridSemiTheme />` |
| Dock 多文档工作区 | `<semi:SemiTheme />`、`<codewf:DockSemiTheme />` |
| 仅国际化 | 不需要任何主题入口，初始化 `I18nManager` 即可 |

## 主题与依赖 / Themes and dependencies

六个主题入口分别属于哪个包、是否依赖 Semi、能否省略：

| 主题入口 | 所属包 | 依赖 Semi | 可否省略 |
|---|---|---|---|
| `ACSemiTheme` | Controls | 是 | 用了 Guide/Transfer/StatusBadge/CodeWFWindow 等控件则必须引入 |
| `DataGridSemiTheme` | DataGrid | 是 | 用了 `EnableDefaults()` 增强或需要 Semi 风格表头则必须引入 |
| `TreeDataGridSemiTheme` | TreeDataGrid | 是 | 用了 TreeDataGrid 增强或 PropertyGrid 则必须引入 |
| `DockSemiTheme` | Dock | 是 | 用了 Dock 布局则必须引入 |
| `CodeWFLogTheme` | Log | 否 | 只在不使用日志查看控件时可省 |
| `MarkdownThemes` / `MarkdownFullThemes` | Markdown.Lite / Markdown | 否 | 用了 Markdown 控件则必须引入（后者包含前者） |

**6 套色板与 `RequestedThemeVariant` 的对应关系**（演示应用顶部切换的就是这 6 套）：

| 色板 | 取值 |
|---|---|
| Default | `ThemeVariant.Light` / `ThemeVariant.Dark` |
| Aquatic、Desert、Dusk、NightSky | `SemiTheme.Aquatic` / `.Desert` / `.Dusk` / `.NightSky`（Semi 提供的固定深色变体，不随浅深切换） |
| HighContrast | `CodeWFLogTheme.HighContrast`（深色高对比，供日志控件使用） |

运行时切换只需给 `Application.Current.RequestedThemeVariant` 赋值：

```csharp
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;          // Default 色板 · 深色
Application.Current!.RequestedThemeVariant = SemiTheme.Aquatic;          // 水生
Application.Current!.RequestedThemeVariant = CodeWFLogTheme.HighContrast; // 高对比
```

### 改色时的两个事实

- **`CodeWFLogTheme` 是 `sealed`**：它只能整体引入，不支持继承后覆盖配色。若需要自定义日志配色，请在应用资源里覆盖同名画笔键（`Application.Resources` 的优先级高于主题）。
- **`MarkdownThemes` 可以继承**：它是 public 无参构造的 `Styles`，`MarkdownFullThemes` 就是这么来的；排版主题/尺寸也支持运行时切换：

```csharp
public class MyMarkdownThemes : MarkdownThemes
{
    public MyMarkdownThemes()
    {
        TypographyTheme = MarkdownTypographyThemes.TechnologyBlue; // 默认 Basic
        TypographySize = MarkdownTypographySizes.Small;            // 默认 Normal
    }
}
```

```xml
<codewf:MarkdownThemes TypographyTheme="TechnologyBlue" TypographySize="Small" />
```

排版主题共 18 套，Key 见 `MarkdownTypographyThemes`（Basic 为默认），尺寸见 `MarkdownTypographySizes`。

全局/局部切换还可调用 `MarkdownThemes.OverrideTypographyResources(...)`，无需重建控件。

> 注意：主题资源以程序集内绝对 URI（`avares://CodeWF.Avalonia.*/Themes/...`）引用，不要手动改写资源字典路径。

## 需要宿主自备的主题 / Host-provided theming

| 情形 | 宿主需要做什么 |
|---|---|
| Dock | `DockSemiTheme` 以 Dock 官方的 `Dock.Avalonia.Themes.Simple` 为基座，并自带 `{x:Type Button}` / `{x:Type Window}` 基类主题（Semi.Avalonia 12.x 不再提供这两个基类主题），因此**不需要**再引入 `FluentTheme`；但 Semi 基础主题仍需引入。 |
| AvaloniaEdit（Markdown 完整包） | 编辑器需要宿主提供字体令牌（`FontSizeNormal` 等，通常来自所引用的主题包）。缺少时编辑器字号会回退到默认值，不影响功能。 |
| Markdown 语言资源 | 语言 JSON 随包分发到输出目录 `I18n/CodeWF.Markdown/`，用 `CodeWF.Avalonia.Lang` 的 Json 加载器指向该目录即可完成本地化。 |
| 日志控件 | 需先初始化 `CodeWF.Toolkit.Logging` 的 `Logger`，控件订阅其 UI 日志事件流；回调在后台线程，界面订阅方自行调度到 UI 线程。 |

## 包 → 源码 → 文档索引 / Source & docs index

| 包 | 源码 | 文档 |
|---|---|---|
| Controls | [src/Controls](src/Controls/CodeWF.Avalonia.Controls) | [说明文档](docs/CodeWF.Avalonia.Controls%20说明文档.md) |
| DataGrid | [src/DataGrid](src/DataGrid/CodeWF.Avalonia.DataGrid) | [说明文档](docs/CodeWF.Avalonia.DataGrid%20说明文档.md) |
| TreeDataGrid（含 PropertyGrid） | [src/TreeDataGrid](src/TreeDataGrid/CodeWF.Avalonia.TreeDataGrid) | [说明文档](docs/CodeWF.Avalonia.TreeDataGrid%20说明文档.md) |
| Dock | [src/Dock](src/Dock/CodeWF.Avalonia.Dock) | [说明文档](docs/CodeWF.Avalonia.Dock%20说明文档.md) |
| Markdown / Markdown.Lite | [src/Markdown](src/Markdown) | [说明文档](docs/CodeWF.Avalonia.Markdown%20说明文档.md) |
| Log | [src/Log](src/Log/CodeWF.Avalonia.Log) | [说明文档](docs/CodeWF.Avalonia.Log%20说明文档.md) |
| Lang / Lang.Generator | [src/Lang](src/Lang) | [说明文档](docs/CodeWF.Avalonia.Lang%20说明文档.md) |

## 演示应用 / Demo

```bash
# 桌面演示台：Ribbon 页签 + Dock 工作区（左导航 / 中文档 / 右属性面板 / 下日志），含 6 色板、深浅与双语切换
dotnet run --project demo/CodeWF.Avalonia.Demo

# 冒烟模式：按目录逐页构建页面后立即退出，退出码 0 表示全部页面构建成功（CI 使用）
dotnet run --project demo/CodeWF.Avalonia.Demo -- --smoke
```

安装包由 [GitHub Releases](https://github.com/dotnet9/CodeWF.Avalonia/releases) 提供（随 `v*` tag 构建）：Windows `setup.exe` / 便携 `zip`、Linux `deb`、macOS `dmg`，均附 `.sha256` 校验文件。

## CI/CD 与版本策略 / CI and versioning

- `ci.yml`：`dotnet build -c Release` + `dotnet test`，外加 NativeAOT 发布并在 Windows 上运行 `--smoke`。
- `publish-nuget.yml`：推送 `v*` tag 时打包并发布 NuGet（Trusted Publishing）。
- `release.yml`：同一 tag 触发全平台演示应用发布（6 个 RID：win-x64/x86、linux-x64/arm64、osx-x64/arm64）。
- 版本号前三跟随 Avalonia，第四位为本仓修订，例如 `v12.1.4.4`。
- 变更记录见 [UpdateLog.md](UpdateLog.md)。

## 许可与贡献 / License and contributing

[MIT](LICENSE)。Issue 与 PR 都欢迎，提交前请确保 `dotnet build CodeWF.Avalonia.slnx -c Release` 与 `dotnet test` 通过。
