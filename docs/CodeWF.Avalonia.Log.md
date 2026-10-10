# CodeWF.Avalonia.Log

面向 Avalonia 的日志查看控件与桌面通知，配合 `CodeWF.Toolkit.Logging` 轻量日志核心使用。

## 安装

```bash
dotnet add package CodeWF.Avalonia.Log
dotnet add package CodeWF.Toolkit.Logging
```

## 用法

1. 应用启动时初始化日志核心（控制台 + 文件轮转 + UI 事件流）：

```csharp
Logger.TryInitialize(new LoggerOptions
{
    File = new FileLogOptions { DirectoryPath = logDirectory },
});
```

2. XAML 中放置日志视图，控件订阅核心的 UI 日志事件流实时展示，支持级别过滤与检索。
3. 附带桌面通知能力（Notifications 目录：ViewModels/Views/Behaviors），可将关键日志以系统通知形式弹出。

## 文件夹打开

控件内"打开日志目录"能力为跨平台内联实现（Windows 资源管理器 / Linux xdg-open / macOS open），不再依赖基础工具库。

## 注意事项

- 对 `CodeWF.Toolkit.Logging` 有包依赖（preview 期最低 1.0.0.0-preview.1）。
- AOT 发布已验证（CI 冒烟覆盖）。
