# CodeWF.Avalonia

CodeWF.Avalonia 是一套面向 Avalonia 12 的 UI 控件与能力库合集：基础自定义控件、表格增强、Dock 布局、Markdown 渲染与编辑、国际化、日志查看，均以 `CodeWF.Avalonia.*` 前缀发布，XAML 命名空间统一挂 `https://codewf.com`。

CodeWF.Avalonia is a suite of UI control and capability libraries for Avalonia 12, published with the `CodeWF.Avalonia.*` prefix and a unified XAML namespace `https://codewf.com`.

> 版本号前三位跟随 Avalonia（当前 12.1.4），第四位为我们自己的修订号。

## 包一览 / Packages

| 包 | 说明 |
|---|---|
| [CodeWF.Avalonia.Controls](https://www.nuget.org/packages/CodeWF.Avalonia.Controls) | 基础自定义控件 + Semi 主题：Guide 引导、Transfer/SearchListBox、StatusBadge/StatusCard、CodeWFWindow/标题栏、WindowedStackPanel、梯形页签、If/Switch 标记扩展与转换器 |
| [CodeWF.Avalonia.DataGrid](https://www.nuget.org/packages/CodeWF.Avalonia.DataGrid) | 官方 DataGrid 增强：三态排序（升/降/取消）、自然排序、智能 ToolTip，内置 Semi 主题入口 |
| [CodeWF.Avalonia.TreeDataGrid](https://www.nuget.org/packages/CodeWF.Avalonia.TreeDataGrid) | 社区版 TreeDataGrid.Avalonia 增强：三态排序、Ctrl+A 全选、智能 ToolTip，内置 Semi 主题（本地 vendor，不依赖商业版） |
| [CodeWF.Avalonia.Dock](https://www.nuget.org/packages/CodeWF.Avalonia.Dock) | 基于 Dock.Avalonia 的布局扩展与 Semi 风格主题，含中英文本地化 |
| [CodeWF.Avalonia.Markdown.Lite](https://www.nuget.org/packages/CodeWF.Avalonia.Markdown.Lite) | Markdown 渲染引擎（常规元素）+ 18 套排版主题 |
| [CodeWF.Avalonia.Markdown](https://www.nuget.org/packages/CodeWF.Avalonia.Markdown) | 完整包：图片/公式/Mermaid/代码高亮/多格式导出/源码与所见即所得编辑器 |
| [CodeWF.Avalonia.Log](https://www.nuget.org/packages/CodeWF.Avalonia.Log) | 日志查看控件与桌面通知（配合 CodeWF.Toolkit.Logging） |
| [CodeWF.Avalonia.Lang](https://www.nuget.org/packages/CodeWF.Avalonia.Lang) | 插件化国际化：I18n 标记扩展 + 内置 Json/Xml/Resx 加载器 |
| [CodeWF.Avalonia.Lang.Generator](https://www.nuget.org/packages/CodeWF.Avalonia.Lang.Generator) | 编译期源生成器：从语言资源生成强类型 Key |

## 安装 / Install

```bash
dotnet add package CodeWF.Avalonia.Controls
dotnet add package CodeWF.Avalonia.Markdown
```

## 主题接入

所有主题入口类都注册在 `https://codewf.com` 下，在 App.axaml 中按需引入：

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:codewf="https://codewf.com">
  <Application.Styles>
    <semi:SemiTheme Locale="zh-CN" />
    <codewf:ACSemiTheme />
    <codewf:DataGridSemiTheme />
  </Application.Styles>
</Application>
```

## 文档 / Docs

按包名命名的详细文档见 [docs](docs/) 目录。

## 许可 / License

[MIT](LICENSE)
