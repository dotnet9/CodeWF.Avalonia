# CodeWF.Avalonia.Markdown / CodeWF.Avalonia.Markdown.Lite

Markdown 渲染与编辑全家桶，按依赖重量分两包：

- **Lite**：Markdig 解析 + Avalonia 渲染引擎，只渲染常规元素（标题/段落/列表/引用/表格/链接/代码块单色/图片替代文本/公式原文），内置 12 套排版主题。
- **完整包**：在 Lite 之上叠加图片（GIF/SVG/预览）、数学公式、Mermaid 图表、TextMate 代码高亮、PNG/PDF/Word/公众号 HTML 导出、源码编辑器与单栏所见即所得编辑。

## Mermaid 配色与加载

Mermaid 支持应用级主题资源：Background、Foreground、Accent、Default、Line、Border、Surface，均以 `CodeWFMarkdownMermaid` 为前缀，值为 `SolidColorBrush`。`CodeWFMarkdownMermaidDataPalette` 为逗号分隔的十六进制颜色字符串；可另设 `CodeWFMarkdownMermaidFillPalette`，用于饼图、旅程、时间线、矩形树与象限图的大面积填充，缺省沿用数据色板。放入 Light / Dark 主题字典即可分别配置。

图表在后台有限并行绘制，同源码和同配色的并发请求复用同一任务；48 项 SVG 缓存包含所有颜色选项。离开界面的请求停止应用结果，保留缓存供后续重新挂载。等待期间保留高度，避免暂时零高让大量离屏块同时进入视口；阅读控件中的 Mermaid 块无论文档长短均延迟到量测时创建。

全文重绘同步推进后台解析的文本快照，避免切换章节后回到先前内容时被误判为“没有变化”。异步解析仍只应用当前源码对应的结果。增量替换的源码块清除旧折叠状态，未改动的代码块及同文档换主题、换字号继续保留折叠状态。

桑基图的左右外置标签计入 SVG 画布宽度，再随正文等比缩放；英文、中文长标签均保留完整文字。

## 安装

```bash
# 只要渲染
dotnet add package CodeWF.Avalonia.Markdown.Lite
# 要编辑器/导出/图表等完整能力
dotnet add package CodeWF.Avalonia.Markdown
```

## 渲染控件

```xml
<markdown:MarkdownViewer Markdown="{Binding Content}" />
```

完整包另有 `MarkdownEditor`（源码编辑）、`MarkdownLiveEditor`（单栏实时编辑/所见即所得）与图片预览窗口（缩放/旋转/另存）。

## 排版主题

主题按"排版风格 + 明暗变体"组织，通过注册表选择：

```csharp
MarkdownTypographyThemeRegistry.Register(...);   // 注册自定义主题
```

XAML 中经 `MarkdownThemes`（Lite）或 `MarkdownFullThemes`（完整包）入口引入主题资源字典。

## 多语言

控件内置 UI 文案（图片预览、复制菜单等）由 Markdown.Lite 内嵌的中英日繁体 JSON 提供，资源键常量由 `I18n/Language.tt` 生成到 `MarkdownL`。宿主无需复制或注册库资源；已有 `CodeWF.Avalonia.Lang` 语言切换会自动生效，缺失文化回退英文。应用自己的 JSON 继续输出到 `I18n/`，可选用同名键替换默认文案；覆盖与缺键回退由 Lang 引擎统一处理。

## 导出

完整包提供 `MarkdownDocumentExporter` 与公众号 HTML 复制（`MarkdownHtmlClipboard`）、PNG 渲染（`MarkdownPngRenderer`）等扩展。

## 注意事项

- 仅 net10.0；完整包依赖 AvaloniaEdit、SkiaSharp、Svg.Skia、Mermaider、TextMateSharp 等较重组件，按需选择。
- 渲染管线可通过 `IMarkdownRenderContext` / 渲染器管道扩展自定义块级与行内元素。
