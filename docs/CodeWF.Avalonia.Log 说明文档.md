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

- 右键菜单的中英文 JSON 随 NuGet 包内嵌，无需应用复制或注册；缺失语言回退到英文。修改 Lang 的当前语言后，已挂载控件自动刷新菜单，重新挂载时也会应用最新语言。
- 宿主照常加载自身 `I18n/` JSON 即可按 `Log.View.Copy`、`Log.View.Clear`、`Log.View.Location` 覆盖文案；同键覆盖由共享 Lang 引擎处理。强类型键由 `I18n/Language.tt` 生成。
- 可设置 `LogDirectory` 指向应用实际日志目录；Demo 外壳直接复用右键复制、清空和打开目录功能。

- 对 `CodeWF.Toolkit.Logging` 有包依赖（preview 期最低 1.0.0.0-preview.1）。
- AOT 发布已验证（CI 冒烟覆盖）。
