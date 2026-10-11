using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownViewerPage : DemoPageBase
{
    private const string IdTheme = "mdv.theme";
    private const string IdSize = "mdv.size";
    private const string IdVirtualization = "mdv.virtualization";
    private const string IdThreshold = "mdv.threshold";

    public const string XamlSnippet = """
        <!-- 18 套排版主题、增量渲染、块级虚拟化、任务勾选回写 -->
        <codewf:MarkdownViewer Markdown="{Binding Content}"
                               TypographyTheme="{Binding Theme}"
                               TypographySize="{Binding Size}"
                               EnableVirtualization="True"
                               VirtualizationThreshold="40" />
        """;

    public const string CSharpSnippet = """
        // 主题切换：单控件覆盖
        viewer.TypographyTheme = MarkdownTypographyThemes.GeekBlack;
        viewer.TypographySize = MarkdownTypographySizes.Small;

        // 应用级全局覆盖
        MarkdownThemes.OverrideTypographyResources(Application.Current, MarkdownTypographyThemes.Verdant);

        // 渲染指标与任务回写
        viewer.TaskWriteBackRequested += (_, e) =>
            editor.ApplyExternalEdit(e.Markdown);   // 勾选任务后同步回编辑器
        var parseMs = viewer.LastParseDuration.TotalMilliseconds;
        var mode = viewer.LastRenderMode;           // Incremental / Full
        var realized = viewer.RealizedBlockCount;
        """;

    private MarkdownViewerPageViewModel Model => (MarkdownViewerPageViewModel)DataContext!;

    public MarkdownViewerPage()
    {
        InitializeComponent();
        DataContext = new MarkdownViewerPageViewModel();

        Viewer.TaskWriteBackRequested += (_, e) =>
        {
            Model.ApplyExternalEdit(e.Markdown);
            UpdateMetrics();
        };
        DataContextChanged += (_, _) => UpdateMetrics();
    }


    public override string TitleKey => "Demo.Page.MarkdownViewer.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownViewer.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateMetrics();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        var vm = Model;
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                vm.Theme = theme;
                break;
            case IdSize when change.NewValue is string size:
                vm.Size = size;
                break;
            case IdVirtualization when change.NewValue is bool enable:
                vm.EnableVirtualization = enable;
                break;
            case IdThreshold when change.NewValue is decimal threshold:
                vm.Threshold = (int)threshold;
                break;
        }

        UpdateMetrics();
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var vm = (MarkdownViewerPageViewModel)DataContext!;

        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = "排版主题",
            Kind = PropertyItemKind.Choice,
            Options = vm.ThemeOptions,
            Value = vm.Theme,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = "18 套排版主题，提交后立即重渲染。",
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = "排版尺寸",
            Kind = PropertyItemKind.Choice,
            Options = vm.SizeOptions,
            Value = vm.Size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = "Normal / Small，Small 为紧凑排版。",
        };
        var virtualization = new PropertyItem
        {
            Id = IdVirtualization,
            Name = "虚拟化",
            Kind = PropertyItemKind.Boolean,
            Value = vm.EnableVirtualization,
            DefaultValue = true,
            Description = "超过阈值后按窗口实现块级内容，长文档显存占用更低。",
        };
        var threshold = new PropertyItem
        {
            Id = IdThreshold,
            Name = "虚拟化阈值",
            Kind = PropertyItemKind.Number,
            Value = (decimal)vm.Threshold,
            DefaultValue = 40m,
            Minimum = 1,
            Maximum = 2000,
            Description = "块数超过该值才启用虚拟化。",
        };

        return [theme, size, virtualization, threshold];
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        Model.Apply();
        UpdateMetrics();
    }

    private void OnAppend(object? sender, RoutedEventArgs e)
    {
        Model.AppendParagraph();
        UpdateMetrics();
    }

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        Model.ResetSample();
        UpdateMetrics();
    }

    private void UpdateMetrics()
    {
        if (MetricsText is null || Viewer is null)
        {
            return;
        }

        MetricsText.Text =
            $"渲染模式：{Viewer.LastRenderMode} · 解析耗时：{Viewer.LastParseDuration.TotalMilliseconds:F1} ms · 已实现块：{Viewer.RealizedBlockCount}";
    }
}
