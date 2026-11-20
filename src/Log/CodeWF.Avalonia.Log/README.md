# CodeWF.Avalonia.Log

面向 Avalonia 的日志查看控件与桌面通知：基于轻量日志核心展示实时日志、级别过滤与界面友好内容，适合桌面应用内嵌日志观察台。Avalonia log viewer control with desktop notifications on top of the lightweight logging core.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Log?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Log)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.Log
dotnet add package CodeWF.Toolkit.Logging
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

本包不依赖 Semi.Avalonia，与宿主使用的主题体系无耦合。AOT 发布已验证（CI 冒烟覆盖）。

## 初始化与主题

应用启动时初始化日志核心（控制台 + 文件轮转 + UI 事件流）：

```csharp
Logger.TryInitialize(new LoggerOptions
{
    File = new FileLogOptions { DirectoryPath = logDirectory },
});
```

XAML 样式中加载主题入口，放置日志视图（控件订阅核心的 UI 日志事件流实时展示，支持级别过滤与检索）：

```xml
<Application.Styles>
  <codewf:CodeWFLogTheme />
</Application.Styles>

<codewf:LogView LogDirectory="..." />
```

日志事件回调在后台线程，界面订阅方自行调度到 UI 线程。"打开日志目录"为跨平台内联实现（Windows 资源管理器 / Linux xdg-open / macOS open）。

## 桌面通知

`LogNotifications` 可将关键日志以系统通知形式弹出，详见完整文档。

## 本地化

右键菜单中英文 JSON 已内嵌程序集，跟随 `CodeWF.Toolkit.Lang` 当前语言，缺失文化回退英文；宿主在自身 `I18n/` 放同名键（`Log.View.Copy` / `Log.View.Clear` / `Log.View.Location`）即可覆盖文案。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.Log%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
