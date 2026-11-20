# CodeWF.Avalonia.Dock

基于开源 [Dock.Avalonia](https://www.nuget.org/packages/Dock.Avalonia) 的布局扩展控件与 Semi 风格主题：GridDock 布局适配、梯形文档页签与中英文本地化资源。Dock layout extensions with a Semi-styled theme on top of the open-source Dock.Avalonia.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Dock?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Dock)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.Dock
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

依赖 Dock.Avalonia 与 Dock.Avalonia.Themes.Simple（随包自动安装，不使用 Fluent 主题）。

## 主题接入

宿主仍需加载 Semi 基础主题提供全局控件样式：

```xml
<semi:SemiTheme Locale="zh-CN" />
<codewf:DockSemiTheme />
```

主题以 Dock 官方 `Dock.Avalonia.Themes.Simple` 为基座，叠加 Semi 风格配色令牌与皮肤覆盖（面板圆角/描边、文档页签、工具标题栏）。自带 `{x:Type Button}` / `{x:Type Window}` 基类主题（Semi.Avalonia 12.x 不再提供这两个基类主题），因此宿主无需再引入 `FluentTheme`。

## 主要能力

| 组件 | 说明 |
|---|---|
| FixedToolGridDockBehavior / GridDockLayoutAdapter | 工具区固定尺寸的 GridDock 行为与布局适配 |
| TrapezoidShapedDocumentTabStripItemBorder | 文档页签梯形边框，与基础库梯形页签视觉一致 |
| DockFactoryCommandConverter | Dock 工厂命令转换器 |
| 本地化资源 | zh-cn / en-us 资源字典，覆盖 Dock 内置文案 |

## 为什么保留 Dock Simple

`Semi.Avalonia.Dock` 是另一套 Dock 主题：免费但闭源、独立 SDK 许可，且不能代替全局 `SemiTheme`。本库保留 MIT 的 Dock Simple 基础模板与现有覆盖，避免给使用者新增许可要求，详见完整文档。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.Dock%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
