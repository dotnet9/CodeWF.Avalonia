using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Log;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;
using Microsoft.Extensions.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class LogPage : DemoPageBase
{
    private const string IdMinimumLevel = "log.minimumLevel";
    private const string IdMaxDisplay = "log.maxDisplay";

    public const string XamlSnippet = """
        <!-- 初始化 Logger 后，LogView 自动订阅其 UI 事件流 -->
        Logger.TryInitialize(new LoggerOptions
        {
            File = new FileLogOptions { DirectoryPath = logDirectory },
        });

        <codewf:CodeWFLogTheme />   <!-- App.Styles：Light / Dark / HighContrast 三套日志配色 -->

        <codewf:LogView MinimumLevel="Information"
                        MaxDisplayCount="2000"
                        LogLineHeightMultiplier="1.5" />
        """;

    public const string CSharpSnippet = """
        // 事件流也可显式指定（多窗口场景共享同一数据源）
        logView.Source = Logger.Events;

        // 写入各级别日志（微软 ILogger 语义：Trace → Critical）
        Logger.Info("服务启动完成");
        Logger.Warn("磁盘空间不足");
        Logger.Error("下游服务超时");

        // 级别过滤（控件侧，只影响显示）
        logView.MinimumLevel = LogLevel.Warning;
        logView.MaxDisplayCount = 2000;

        // 右键菜单：复制 / 清空 / 打开日志目录（跨平台内联实现）
        """;

    public LogPage()
    {
        InitializeComponent();
    }


    public override string TitleKey => "Demo.Page.Log.Title";

    public override string DescriptionKey => "Demo.Page.Log.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdMinimumLevel when change.NewValue is string level:
                LogViewControl.MinimumLevel = level switch
                {
                    "跟踪" => LogLevel.Trace,
                    "调试" => LogLevel.Debug,
                    "警告" => LogLevel.Warning,
                    "错误" => LogLevel.Error,
                    _ => LogLevel.Information,
                };
                break;
            case IdMaxDisplay when change.NewValue is decimal max:
                LogViewControl.MaxDisplayCount = (int)max;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var minimumLevel = new PropertyItem
        {
            Id = IdMinimumLevel,
            Name = "最低显示级别",
            Kind = PropertyItemKind.Choice,
            Options = ["跟踪", "调试", "消息", "警告", "错误"],
            Value = "消息",
            DefaultValue = "消息",
            Description = "低于该级别的日志不显示（MinimumLevel）。",
        };
        var maxDisplay = new PropertyItem
        {
            Id = IdMaxDisplay,
            Name = "最大显示条数",
            Kind = PropertyItemKind.Number,
            Value = 2000m,
            DefaultValue = 1000m,
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            Description = "控件保留的最大条目数（MaxDisplayCount），超出后滚动丢弃。",
        };

        return [minimumLevel, maxDisplay];
    }

    private void OnWriteTrace(object? sender, RoutedEventArgs e) =>
        Logger.Trace("演示：跟踪日志（默认级别下不显示）");

    private void OnWriteDebug(object? sender, RoutedEventArgs e) =>
        Logger.Debug("演示：调试日志（默认级别下不显示）");

    private void OnWriteInfo(object? sender, RoutedEventArgs e) =>
        Logger.Info("演示：信息日志 " + DateTime.Now.ToString("HH:mm:ss.fff"));

    private void OnWriteWarning(object? sender, RoutedEventArgs e) =>
        Logger.Warn("演示：警告日志——磁盘空间不足 10%");

    private void OnWriteError(object? sender, RoutedEventArgs e) =>
        Logger.Error("演示：错误日志——下游服务超时 3000ms");

    private void OnWriteCritical(object? sender, RoutedEventArgs e) =>
        Logger.Fatal("演示：严重日志——数据库连接失败");

    private async void OnWriteBurst(object? sender, RoutedEventArgs e)
    {
        for (var i = 1; i <= 200; i++)
        {
            Logger.Info($"压测日志 {i:000}：验证事件流吞吐与展示节流");
            if (i % 50 == 0)
            {
                await Task.Delay(50);
            }
        }
    }
}
