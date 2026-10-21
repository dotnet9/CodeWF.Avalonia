using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Markdown.Themes;
using ReactiveUI;
using System.Windows.Input;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>MarkdownViewer 演示页 VM：源文本编辑 / 增量渲染 / 排版主题 / 虚拟化。</summary>
public class MarkdownViewerPageViewModel : ReactiveObject
{
    private const string IdTheme = "mdv.theme";
    private const string IdSize = "mdv.size";
    private const string IdVirtualization = "mdv.virtualization";
    private const string IdThreshold = "mdv.threshold";

    public static string SampleMarkdown => L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Samples.Markdown);

    private string _source = SampleMarkdown;
    private string _appliedMarkdown = SampleMarkdown;
    private string _theme = MarkdownTypographyThemes.Basic;
    private string _size = MarkdownTypographySizes.Normal;
    private bool _enableVirtualization = true;
    private int _threshold = 40;

    private string _metrics = string.Empty;
    public string Metrics { get => _metrics; private set => this.RaiseAndSetIfChanged(ref _metrics, value); }
    public ICommand ApplyCommand { get; }
    public ICommand AppendCommand { get; }
    public ICommand ResetCommand { get; }

    public MarkdownViewerPageViewModel()
    {
        ApplyCommand = ReactiveCommand.Create(Apply);
        AppendCommand = ReactiveCommand.Create(AppendParagraph);
        ResetCommand = ReactiveCommand.Create(ResetSample);
    }

    public void UpdateMetrics(string mode, double milliseconds, int blocks) => Metrics =
        string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.MarkdownViewer.UpdateMetrics.ModeParsingMsRealizedBlocksFormat), mode, milliseconds, blocks);

    /// <summary>源文本（编辑区）。</summary>
    public string Source
    {
        get => _source;
        set => this.RaiseAndSetIfChanged(ref _source, value);
    }

    /// <summary>已应用的文本（触发渲染）。</summary>
    public string AppliedMarkdown
    {
        get => _appliedMarkdown;
        private set => this.RaiseAndSetIfChanged(ref _appliedMarkdown, value);
    }

    public string Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    public string Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }

    public bool EnableVirtualization
    {
        get => _enableVirtualization;
        set => this.RaiseAndSetIfChanged(ref _enableVirtualization, value);
    }

    public int Threshold
    {
        get => _threshold;
        set => this.RaiseAndSetIfChanged(ref _threshold, value);
    }

    public string[] ThemeOptions { get; } =
    [
        MarkdownTypographyThemes.Basic,
        MarkdownTypographyThemes.OrangeHeart,
        MarkdownTypographyThemes.InkBlack,
        MarkdownTypographyThemes.ColorfulPurple,
        MarkdownTypographyThemes.TenderGreen,
        MarkdownTypographyThemes.Verdant,
        MarkdownTypographyThemes.RedScarlet,
        MarkdownTypographyThemes.BlueGlow,
        MarkdownTypographyThemes.TechnologyBlue,
        MarkdownTypographyThemes.LanQing,
        MarkdownTypographyThemes.Yamabuki,
        MarkdownTypographyThemes.FrontendPeak,
        MarkdownTypographyThemes.GeekBlack,
        MarkdownTypographyThemes.Simple,
        MarkdownTypographyThemes.RosePurple,
        MarkdownTypographyThemes.CuteGreen,
        MarkdownTypographyThemes.FullStackBlue,
        MarkdownTypographyThemes.WeChatFormat,
    ];

    public string[] SizeOptions { get; } = [MarkdownTypographySizes.Normal, MarkdownTypographySizes.Compact];

    public void Apply() => AppliedMarkdown = Source;

    public void AppendParagraph()
    {
        Source += L.Instance.GetResource(DemoResources.Content.MarkdownViewer.AppendParagraph.NewParagraphIncrementalRenderingReparsesAndLaysOut)
                  + DateTime.Now.ToString("HH:mm:ss") + L.Instance.GetResource(DemoResources.Studio.SampleSuffix);
        Apply();
    }

    public void ResetSample()
    {
        Source = SampleMarkdown;
        Apply();
    }

    /// <summary>任务勾选回写：把控件给出的最新 Markdown 应用回源文本。</summary>
    public void ApplyExternalEdit(string markdown)
    {
        Source = markdown;
        AppliedMarkdown = markdown;
    }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                Theme = theme;
                break;
            case IdSize when change.NewValue is string size:
                Size = size;
                break;
            case IdVirtualization when change.NewValue is bool enable:
                EnableVirtualization = enable;
                break;
            case IdThreshold when change.NewValue is decimal threshold:
                Threshold = (int)threshold;
                break;
        }

    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {

        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.TypographyTheme),
            Kind = PropertyItemKind.Choice,
            Options = ThemeOptions,
            Value = Theme,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties._18TypographyThemesChangesRenderImmediately),
        };
        var size = new PropertyItem
        {
            Id = IdSize,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.TypographySize),
            Kind = PropertyItemKind.Choice,
            Options = SizeOptions,
            Value = Size,
            DefaultValue = MarkdownTypographySizes.Normal,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.NormalSmallSmallUsesCompactTypography),
        };
        var virtualization = new PropertyItem
        {
            Id = IdVirtualization,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.Virtualization),
            Kind = PropertyItemKind.Boolean,
            Value = EnableVirtualization,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.RealizeVisibleBlocksAboveTheThresholdToReduce),
        };
        var threshold = new PropertyItem
        {
            Id = IdThreshold,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.VirtualizationThreshold),
            Kind = PropertyItemKind.Number,
            Value = (decimal)Threshold,
            DefaultValue = 40m,
            Minimum = 1,
            Maximum = 2000,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Properties.EnableVirtualizationAboveThisBlockCount),
        };

        return [theme, size, virtualization, threshold];
    }

}
