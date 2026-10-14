using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
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

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Source.CSharp);

    private readonly DispatcherTimer _metricsTimer;

    public WindowedStackPanelPage()
    {
        InitializeComponent();

        _metricsTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _metricsTimer.Tick += (_, _) => UpdateMetrics();
        _metricsTimer.Start();
    }



    public override string TitleKey => DemoResources.Page.WindowedStackPanel.Title;

    public override string DescriptionKey => DemoResources.Page.WindowedStackPanel.Desc;

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
            Name = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.Virtualization),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.KeepAllBlocksInTheVisualTreeWhen),
        };
        var threshold = new PropertyItem
        {
            Id = IdThreshold,
            Name = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.VirtualizationThreshold),
            Kind = PropertyItemKind.Number,
            Value = 40m,
            DefaultValue = 40m,
            Minimum = 1,
            Maximum = 5000,
            Description = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.EnableWindowedRealizationAboveThisBlockCount),
        };
        var overscan = new PropertyItem
        {
            Id = IdOverscan,
            Name = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.Overscan),
            Kind = PropertyItemKind.Number,
            Value = 2m,
            DefaultValue = 2m,
            Minimum = 0,
            Maximum = 10,
            Format = "0",
            Description = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.ExtraViewportBuffersAboveAndBelowTheVisible),
        };
        var spacing = new PropertyItem
        {
            Id = IdSpacing,
            Name = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.BlockSpacing),
            Kind = PropertyItemKind.Number,
            Value = 8m,
            DefaultValue = 0m,
            Minimum = 0,
            Maximum = 32,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.Properties.SpacingBetweenAdjacentBlocks),
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
                    Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.OnGenerate.BlockFormat), i, BlockCount),
                    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
                },
            });
        }

        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.OnGenerate.WindowedStackPanelGeneratedBlocksFormat), BlockCount));
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
            string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.UpdateMetrics.BlocksRealizedVirtualizingFormat), BlocksPanel.ItemCount, BlocksPanel.RealizedItemCount, (BlocksPanel.IsVirtualizing ? L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.UpdateMetrics.Yes) : L.Instance.GetResource(DemoResources.Content.WindowedStackPanel.UpdateMetrics.No)));
    }
}
