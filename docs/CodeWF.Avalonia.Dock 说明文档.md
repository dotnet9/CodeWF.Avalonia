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

主题以 Dock 官方的 Dock.Avalonia.Themes.Simple 为基座（不依赖 Fluent），叠加 Semi 风格配色令牌与皮肤覆盖：面板圆角/描边、文档页签（Simple/Documents.axaml）、工具标题栏（Simple/Tools.axaml）。配色按 6 套变体（Default、Light、Dark，以及 Semi 的 Aquatic、Desert、Dusk、NightSky）给出字面色值，因此启动早期不依赖 Semi 包字典，也不需要在应用里额外引入其它主题包。

## 主要能力

| 组件 | 命名空间 | 说明 |
|---|---|---|
| FixedToolGridDockBehavior | CodeWF.Avalonia.Dock.Controls | 工具区固定尺寸的 GridDock 行为适配 |
| GridDockLayoutAdapter | CodeWF.Avalonia.Dock.Controls | 网格 Dock 布局适配器 |
| TrapezoidShapedDocumentTabStripItemBorder | CodeWF.Avalonia.Dock.Controls | 文档页签的梯形边框绘制，与基础库梯形页签视觉一致 |
| DockFactoryCommandConverter | CodeWF.Avalonia.Dock.Converters | Dock 工厂命令转换器 |
| 本地化资源 | CodeWF.Avalonia.Dock.Themes.Locales | zh-cn / en-us 资源字典，覆盖 Dock 内置文案 |

## 注意事项

- 依赖 Dock.Avalonia 与 Dock.Avalonia.Themes.Simple（12.1.0.x 线），不使用 Fluent 主题。
- 主题自带 `{x:Type Button}` / `{x:Type Window}` 基类主题（Semi.Avalonia 12.x 不再提供，旧版由 FluentTheme 提供），因此宿主无需再引入 FluentTheme。
- 资源 URI 指向本程序集（avares://CodeWF.Avalonia.Dock/Themes/...），图片资源随包分发。
