# CodeWF.Avalonia.Dock

基于开源 [Dock.Avalonia](https://www.nuget.org/packages/Dock.Avalonia) 的布局扩展控件与 Semi 风格主题，含中英文本地化资源。

## 安装

```bash
dotnet add package CodeWF.Avalonia.Dock
```

## 主题接入

```xml
<semi:SemiTheme />
<codewf:DockSemiTheme />
```

主题以 Dock 官方的 Dock.Avalonia.Themes.Simple 为基座（不依赖 Fluent），叠加 Semi 风格配色令牌与皮肤覆盖：面板圆角/描边、文档页签（Simple/Documents.axaml）、工具标题栏（Simple/Tools.axaml）。配色令牌覆盖基础浅深色、Semi 扩展变体与 .NET / AI 配色，启动早期不依赖动态资源查找。宿主仍需加载 SemiTheme 以提供全局控件样式。

## 主要能力

| 组件 | 命名空间 | 说明 |
|---|---|---|
| FixedToolGridDockBehavior | CodeWF.Avalonia.Dock.Controls | 工具区固定尺寸的 GridDock 行为适配 |
| GridDockLayoutAdapter | CodeWF.Avalonia.Dock.Controls | 网格 Dock 布局适配器 |
| TrapezoidShapedDocumentTabStripItemBorder | CodeWF.Avalonia.Dock.Controls | 文档页签的梯形边框绘制，与基础库梯形页签视觉一致 |
| DockFactoryCommandConverter | CodeWF.Avalonia.Dock.Converters | Dock 工厂命令转换器 |
| 本地化资源 | CodeWF.Avalonia.Dock.Themes.Locales | zh-cn / en-us 资源字典，覆盖 Dock 内置文案 |

## 注意事项

- 依赖 Dock.Avalonia 与 Dock.Avalonia.Themes.Simple（当前 12.1.0.6），不使用 Fluent 主题。
- 主题自带 `{x:Type Button}` / `{x:Type Window}` 基类主题（Semi.Avalonia 12.x 不再提供，旧版由 FluentTheme 提供），因此宿主无需再引入 FluentTheme。
- 资源 URI 指向本程序集（avares://CodeWF.Avalonia.Dock/Themes/...），图片资源随包分发。

## 为什么保留 Dock Simple

`Semi.Avalonia.Dock` 12.0.0.2 是另一套 Dock 主题。它的 NuGet README 明确要求另外安装并加载 `Semi.Avalonia`，因此不能代替全局 `SemiTheme`。若切换到该主题，可以去掉 `Dock.Avalonia.Themes.Simple`，但必须重新验证 CodeWF 页签和布局覆盖。

该 Dock 主题免费但闭源，使用独立 SDK 许可，分发还有告知和接受协议的要求，不能按 Semi.Avalonia 本体的 MIT 许可理解。本库保留 MIT 的 Dock Simple 基础模板与现有 CodeWF 覆盖，避免给开源使用者新增该许可要求。`Dock.Avalonia.Themes.Simple` 与 `Avalonia.Themes.Simple` 是不同的包；这里并未引入后者。

依据：[NuGet 包及随包 README / LICENSE](https://www.nuget.org/packages/Semi.Avalonia.Dock/12.0.0.2)。
