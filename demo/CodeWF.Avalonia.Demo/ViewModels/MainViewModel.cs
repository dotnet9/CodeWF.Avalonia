using CodeWF.Avalonia.Markdown;
using CodeWF.Toolkit.Logging;
using ReactiveUI;
using ReactiveUI.Primitives;
using System.Collections.ObjectModel;
using ReactiveUI.Primitives;

namespace CodeWF.Avalonia.Demo.ViewModels;

public class MainViewModel : ReactiveObject
{
    public string MarkdownSample { get; } = """
        # CodeWF.Avalonia 演示

        **加粗**、*斜体*、`行内代码`、[链接](https://codewf.com)

        ## 列表
        - 基础控件
        - 表格增强
        - Markdown 渲染

        > 引用块：以 Markdig 驱动的渲染引擎

        | 列一 | 列二 |
        | --- | --- |
        | A | B |
        """;

    public ObservableCollection<PersonRow> People { get; } = new()
    {
        new PersonRow("张三", 28, "杭州"),
        new PersonRow("李四", 35, "北京"),
        new PersonRow("Adam", 41, "Seattle"),
        new PersonRow("王五", 19, "成都"),
        new PersonRow("Zoe", 33, "Berlin"),
    };

    public string LogFolder { get; } = Path.Combine(Path.GetTempPath(), "CodeWFAvaloniaDemo", "logs");

    public PropertyPanelDemoViewModel PropertyPanel { get; } = new();

    public ReactiveCommand<RxVoid, RxVoid> WriteLogsCommand { get; }

    public MainViewModel()
    {
        WriteLogsCommand = ReactiveCommand.Create(WriteSampleLogs);
    }

    public void WriteSampleLogs()
    {
        Logger.Info("演示：一条信息日志");
        Logger.Warn("演示：一条警告日志");
        Logger.Error("演示：一条错误日志");
    }

    public async Task<bool> RunSmokeAsync(CancellationToken cancellation)
    {
        // 冒烟：初始化日志核心并写入三类日志，验证渲染数据源与命令通道。
        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions { DirectoryPath = LogFolder },
        });
        Logger.Info("冒烟开始");
        WriteSampleLogs();
        await Task.Delay(500, cancellation);
        Logger.Info("冒烟结束");
        await Logger.ShutdownAsync();
        return true;
    }
}

public record PersonRow(string Name, int Age, string City);
