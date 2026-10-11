using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class WindowedStackPanelPage : DemoPageBase
{
    private const string IdVirtualization = "wsp.virtualization";
    private const string IdThreshold = "wsp.threshold";
    private const string IdOverscan = "wsp.overscan";
    private const string IdSpacing = "wsp.spacing";
    private const int BlockCount = 5000;

    public const string XamlSnippet = """
        <!-- 与 ScrollViewer 配合使用；子项可通过 XAML 子元素或代码 AddItem -->
        <ScrollViewer>
          <codewf:WindowedStackPanel x:Name="BlocksPanel"
                                     EnableVirtualization="True"
                                     VirtualizationThreshold="40"
                                     OverscanViewports="2"
                                     Spacing="8" />
        </ScrollViewer>
        """;

    public const string CSharpSnippet = """
        var panel = new WindowedStackPanel
        {
            EnableVirtualization = true,
            VirtualizationThreshold = 40,
            OverscanViewports = 2,
            Spacing = 8,
        };
        scrollViewer.Content = panel;

        for (var i = 0; i < 5000; i++)
        {
            panel.AddItem(new TextBlock { Text = $"块 {i}" });
        }

        // 实时指标
        panel.ItemCount;           // 5000
        panel.RealizedItemCount;   // 视口 + 缓冲内已实现的块数（几十）
        panel.IsVirtualizing;      // 是否处于虚拟化状态

        // 定点操作
        panel.RealizeItem(100);          // 强制实现
        panel.TryGetItemTop(100, out var top);
        panel.ScrollHost?.Offset = new Vector(0, top);   // 滚动到指定块
        panel.ClearItems();
        """;

    private readonly DispatcherTimer _metricsTimer;

    public WindowedStackPanelPage()
    {
        InitializeComponent();

        _metricsTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _metricsTimer.Tick += (_, _) => UpdateMetrics();
        _metricsTimer.Start();
    }



    public override string TitleKey => "Demo.Page.WindowedStackPanel.Title";

    public override string DescriptionKey => "Demo.Page.WindowedStackPanel.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdVirtualization when change.NewValue is bool enable:
                BlocksPanel.EnableVirtualization = enable;
                break;
            case IdThreshold when change.NewValue is decimal threshold:
                BlocksPanel.VirtualizationThreshold = (int)threshold;
                break;
            case IdOverscan when change.NewValue is decimal overscan:
                BlocksPanel.OverscanViewports = (double)overscan;
                break;
            case IdSpacing when change.NewValue is decimal spacing:
                BlocksPanel.Spacing = (double)spacing;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var virtualization = new PropertyItem
        {
            Id = IdVirtualization,
            Name = "虚拟化",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "关闭后全部块常驻可视化树（5000 块时可感受差异）。",
        };
        var threshold = new PropertyItem
        {
            Id = IdThreshold,
            Name = "虚拟化阈值",
            Kind = PropertyItemKind.Number,
            Value = 40m,
            DefaultValue = 40m,
            Minimum = 1,
            Maximum = 5000,
            Description = "块数超过阈值才启用窗口化实现。",
        };
        var overscan = new PropertyItem
        {
            Id = IdOverscan,
            Name = "视口缓冲",
            Kind = PropertyItemKind.Number,
            Value = 2m,
            DefaultValue = 2m,
            Minimum = 0,
            Maximum = 10,
            Format = "0",
            Description = "视口上下各多实现的视口数量，影响滚动平滑度。",
        };
        var spacing = new PropertyItem
        {
            Id = IdSpacing,
            Name = "块间距",
            Kind = PropertyItemKind.Number,
            Value = 8m,
            DefaultValue = 0m,
            Minimum = 0,
            Maximum = 32,
            Unit = "px",
            Description = "相邻块之间的间距。",
        };

        return [virtualization, threshold, overscan, spacing];
    }

    private void OnGenerate(object? sender, RoutedEventArgs e)
    {
        BlocksPanel.ClearItems();
        for (var i = 1; i <= BlockCount; i++)
        {
            BlocksPanel.AddItem(new Border
            {
                Height = 44,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromArgb(24, 0x80, 0x80, 0x90)),
                Padding = new Thickness(12, 0),
                Child = new TextBlock
                {
                    Text = $"块 {i} / {BlockCount}",
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                },
            });
        }

        Logger.Info($"WindowedStackPanel：已生成 {BlockCount} 块");
        UpdateMetrics();
    }

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        BlocksPanel.ClearItems();
        UpdateMetrics();
    }

    private void UpdateMetrics()
    {
        MetricsText.Text =
            $"总块数：{BlocksPanel.ItemCount} · 已实现：{BlocksPanel.RealizedItemCount} · 虚拟化中：{(BlocksPanel.IsVirtualizing ? "是" : "否")}";
    }
}
