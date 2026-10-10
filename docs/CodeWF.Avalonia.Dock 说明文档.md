# CodeWF.Avalonia.Dock

基于开源 [Dock.Avalonia](https://www.nuget.org/packages/Dock.Avalonia) 的布局扩展控件与 Semi 风格主题，含中英文本地化资源。

## 安装

```bash
dotnet add package CodeWF.Avalonia.Dock
```

## 主题接入

```xml
<codewf:DockSemiTheme />
```

主题内部复用 Dock.Avalonia.Themes.Fluent 的样式链路并叠加 Semi 风格资源键（`fluent:DockFluentTheme` 已包含在内，无需重复引入），同时提供 Fluent/浅色/深色变体样式（CodeWF、ToolTitleBar、DockPanelBorder）。

## 主要能力

| 组件 | 命名空间 | 说明 |
|---|---|---|
| FixedToolGridDockBehavior | CodeWF.Avalonia.Dock.Controls | 工具区固定尺寸的 GridDock 行为适配 |
| GridDockLayoutAdapter | CodeWF.Avalonia.Dock.Controls | 网格 Dock 布局适配器 |
| TrapezoidShapedDocumentTabStripItemBorder | CodeWF.Avalonia.Dock.Controls | 文档页签的梯形边框绘制，与基础库梯形页签视觉一致 |
| DockFactoryCommandConverter | CodeWF.Avalonia.Dock.Converters | Dock 工厂命令转换器 |
| 本地化资源 | CodeWF.Avalonia.Dock.Themes.Locales | zh-cn / en-us 资源字典，覆盖 Dock 内置文案 |

## 注意事项

- 依赖 Dock.Avalonia 与 Dock.Avalonia.Themes.Fluent（12.1.0.x 线）。
- 资源 URI 指向本程序集（avares://CodeWF.Avalonia.Dock/Themes/...），图片资源随包分发。
