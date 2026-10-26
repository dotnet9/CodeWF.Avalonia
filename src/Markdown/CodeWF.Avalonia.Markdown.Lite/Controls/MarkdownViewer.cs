using Avalonia;
using global::Avalonia.Automation;
using global::Avalonia.Automation.Peers;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Controls.Metadata;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Data;
using global::Avalonia.Input;
using global::Avalonia.Input.Platform;
using global::Avalonia.Interactivity;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Markdown.Helpers;
using CodeWF.Avalonia.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Shared.Rendering;
using CodeWF.Toolkit.Lang;
using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;
using System.Text.RegularExpressions;
using Inline = global::Avalonia.Controls.Documents.Inline;

namespace CodeWF.Avalonia.Markdown.Controls;

public enum MarkdownRenderMode
{
    /// <summary>
    /// 尽量复用已渲染块，只替换文本变化影响到的区段。
    /// </summary>
    Incremental,

    /// <summary>
    /// 丢弃现有渲染块并从 Markdown 文本完整重建文档。
    /// </summary>
    Full
}

/// <summary>
/// 将 Markdown 文本渲染为 Avalonia 控件树的只读预览控件。
/// </summary>
[TemplatePart(DocumentHostPartName, typeof(Panel), IsRequired = true)]
public partial class MarkdownViewer : TemplatedControl, Rendering.IMarkdownRenderContext
{
    private const string DocumentHostPartName = "PART_DocumentHost";
    private const string DefaultTypographyTheme = "Basic";
    private const string DefaultTypographySize = "Normal";

    internal static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private MarkdownPipeline _documentPipeline = Pipeline;
    public static readonly StyledProperty<MarkdownSyntaxOptions?> SyntaxOptionsProperty =
        AvaloniaProperty.Register<MarkdownViewer, MarkdownSyntaxOptions?>(nameof(SyntaxOptions));
    public MarkdownSyntaxOptions? SyntaxOptions { get => GetValue(SyntaxOptionsProperty); set => SetValue(SyntaxOptionsProperty, value); }

    private readonly List<RenderedBlock> _renderedBlocks = [];
    private readonly List<IDisposable> _currentBlockDisposables = [];
    private readonly MarkdownSelectionController _selectionController = new();
    private Panel? _documentHost;
    private WindowedStackPanel? _virtualizingHost;
    private string _renderedMarkdown = string.Empty;
    private MarkdownDocumentModel _renderedModel = MarkdownDocumentModel.Empty;
    private MarkdownRenderMode _queuedRenderMode = MarkdownRenderMode.Incremental;
    private bool _isRenderQueued;
    private MarkdownPointerSelectionState? _pointerSelectionState;
    private SelectableTextBlock? _nativeSelectionTextBlock;
    private bool _isPointerSelecting;
    private string _selectedText = string.Empty;
    private bool _hasSelection;
    private double _lastScrollOffsetY;

    public static readonly StyledProperty<string?> MarkdownProperty =
        AvaloniaProperty.Register<MarkdownViewer, string?>(nameof(Markdown));

    public static readonly StyledProperty<string?> TypographyThemeProperty =
        AvaloniaProperty.Register<MarkdownViewer, string?>(nameof(TypographyTheme));

    public static readonly StyledProperty<string?> TypographySizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, string?>(nameof(TypographySize));

    public static readonly StyledProperty<string?> ImageBasePathProperty =
        AvaloniaProperty.Register<MarkdownViewer, string?>(nameof(ImageBasePath));

    /// <summary>
    /// 图片源解析回调：对 http(s) 图片源在远程获取前调用，返回本地文件路径即
    /// 离线加载（CDN URL 反查本地文件），返回 null 按原样远程获取。
    /// </summary>
    public static readonly StyledProperty<Func<string, string?>?> ImageSourceResolverProperty =
        AvaloniaProperty.Register<MarkdownViewer, Func<string, string?>?>(nameof(ImageSourceResolver));

    /// <summary>
    /// 流式文档源：设置后 Viewer 持续应用其快照（追加型输入只解析增量），
    /// 适合 AI 输出、文件下载等逐片到达的场景；此后对 <see cref="Markdown"/>
    /// 赋值会解除流式绑定并回退到整串渲染。
    /// </summary>
    public static readonly StyledProperty<Streaming.MarkdownStreamingDocument?> StreamingSourceProperty =
        AvaloniaProperty.Register<MarkdownViewer, Streaming.MarkdownStreamingDocument?>(nameof(StreamingSource));

    /// <summary>
    /// 启用代码块折叠：可折叠长代码块在头部显示折叠按钮，状态按块键保留
    /// （重新渲染、虚拟化回收、明暗主题切换后不丢失，更换文档时重置）。
    /// </summary>
    public static readonly StyledProperty<bool> EnableCodeBlockFoldingProperty =
        AvaloniaProperty.Register<MarkdownViewer, bool>(nameof(EnableCodeBlockFolding), true);

    /// <summary>
    /// 文档宿主的块虚拟化开关；关闭后所有块始终物化，可在异常场景退回非虚拟化宿主。
    /// </summary>
    public static readonly StyledProperty<bool> EnableVirtualizationProperty =
        AvaloniaProperty.Register<MarkdownViewer, bool>(nameof(EnableVirtualization), true);

    /// <summary>
    /// 启用虚拟化的最小块数量；低于该值时按普通面板渲染，避免小文档付出额外开销。
    /// </summary>
    public static readonly StyledProperty<int> VirtualizationThresholdProperty =
        AvaloniaProperty.Register<MarkdownViewer, int>(nameof(VirtualizationThreshold), 40);

    public bool EnableVirtualization
    {
        get => GetValue(EnableVirtualizationProperty);
        set => SetValue(EnableVirtualizationProperty, value);
    }

    public bool EnableCodeBlockFolding
    {
        get => GetValue(EnableCodeBlockFoldingProperty);
        set => SetValue(EnableCodeBlockFoldingProperty, value);
    }

    public int VirtualizationThreshold
    {
        get => GetValue(VirtualizationThresholdProperty);
        set => SetValue(VirtualizationThresholdProperty, value);
    }

    public static readonly DirectProperty<MarkdownViewer, string> SelectedTextProperty =
        AvaloniaProperty.RegisterDirect<MarkdownViewer, string>(
            nameof(SelectedText),
            viewer => viewer.SelectedText);

    public static readonly DirectProperty<MarkdownViewer, bool> HasSelectionProperty =
        AvaloniaProperty.RegisterDirect<MarkdownViewer, bool>(
            nameof(HasSelection),
            viewer => viewer.HasSelection);

    public static readonly StyledProperty<IBrush?> TextBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(TextBrush), Brushes.Black);

    public static readonly StyledProperty<IBrush?> MutedTextBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(MutedTextBrush), Brushes.Gray);

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(AccentBrush), Brushes.DodgerBlue);

    public static readonly StyledProperty<IBrush?> AccentForegroundBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(AccentForegroundBrush), Brushes.White);

    public static readonly StyledProperty<IBrush?> BorderLineBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(BorderLineBrush), Brushes.LightGray);

    public static readonly StyledProperty<IBrush?> QuoteBackgroundBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(QuoteBackgroundBrush), Brushes.Transparent);

    public static readonly StyledProperty<IBrush?> CodeBackgroundBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(CodeBackgroundBrush), Brushes.Transparent);

    public static readonly StyledProperty<IBrush?> InlineCodeBackgroundBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(InlineCodeBackgroundBrush), Brushes.Transparent);

    public static readonly StyledProperty<IBrush?> TableHeaderBackgroundBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(nameof(TableHeaderBackgroundBrush), Brushes.Transparent);

    /// <summary>
    /// 重块（代码块、表格、图片、公式、Mermaid 图表）滚动离屏后的保留预算（设备无关像素高度）。
    /// 面板按「从大到小」保留离屏重块的控件，避免滚动时反复重建（重新高亮、重新解码、重新光栅化）。
    /// 0 表示不保留。
    /// </summary>
    public static readonly StyledProperty<double> HeavyBlockKeepAliveBudgetProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(HeavyBlockKeepAliveBudget), 3000d);

    /// <summary>
    /// 重块「尺寸未定」时的额外保留数量：按滚动方向优先保住接下来要滚回来的那一侧。
    /// 变高块在离屏状态下若被解体，尺寸会退回估算值，回来时会造成视口跳动，
    /// 因此这里额外给未测量过的重块留出配额。
    /// </summary>
    public static readonly StyledProperty<int> HeavyBlockKeepAliveUnknownCountProperty =
        AvaloniaProperty.Register<MarkdownViewer, int>(nameof(HeavyBlockKeepAliveUnknownCount), 6);

    public double HeavyBlockKeepAliveBudget
    {
        get => GetValue(HeavyBlockKeepAliveBudgetProperty);
        set => SetValue(HeavyBlockKeepAliveBudgetProperty, value);
    }

    public int HeavyBlockKeepAliveUnknownCount
    {
        get => GetValue(HeavyBlockKeepAliveUnknownCountProperty);
        set => SetValue(HeavyBlockKeepAliveUnknownCountProperty, value);
    }

    public static readonly StyledProperty<double> ParagraphFontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(ParagraphFontSize), 16);

    public static readonly StyledProperty<double> ParagraphLineHeightProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(ParagraphLineHeight), 28);

    public static readonly StyledProperty<double> Heading1FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading1FontSize), 30);

    public static readonly StyledProperty<double> Heading2FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading2FontSize), 26);

    public static readonly StyledProperty<double> Heading3FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading3FontSize), 22);

    public static readonly StyledProperty<double> Heading4FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading4FontSize), 20);

    public static readonly StyledProperty<double> Heading5FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading5FontSize), 18);

    public static readonly StyledProperty<double> Heading6FontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(Heading6FontSize), 16);

    public static readonly StyledProperty<double> BlockSpacingProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(BlockSpacing), 8);

    public static readonly StyledProperty<double> DocumentBottomPaddingProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(DocumentBottomPadding), 64);

    public static readonly StyledProperty<Thickness> ParagraphMarginProperty =
        AvaloniaProperty.Register<MarkdownViewer, Thickness>(nameof(ParagraphMargin), new Thickness(0, 4, 0, 10));

    public static readonly StyledProperty<Thickness> HeadingMarginProperty =
        AvaloniaProperty.Register<MarkdownViewer, Thickness>(nameof(HeadingMargin), new Thickness(0, 18, 0, 10));

    private static readonly FontFamily DefaultContentFontFamily =
        new("Inter, Microsoft YaHei UI, Microsoft YaHei, Segoe UI, PingFang SC, Hiragino Sans GB, Noto Sans CJK SC, Noto Sans SC, sans-serif");

    public static readonly StyledProperty<FontFamily> ContentFontFamilyProperty =
        AvaloniaProperty.Register<MarkdownViewer, FontFamily>(nameof(ContentFontFamily), DefaultContentFontFamily);

    public static readonly StyledProperty<FontFamily> CodeFontFamilyProperty =
        AvaloniaProperty.Register<MarkdownViewer, FontFamily>(
            nameof(CodeFontFamily),
            new FontFamily("Consolas, Cascadia Mono, JetBrains Mono, monospace"));

    public static readonly StyledProperty<double> CodeBlockFontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(CodeBlockFontSize), 13);
    public static readonly StyledProperty<bool> CodeBlockWrapProperty =
        AvaloniaProperty.Register<MarkdownViewer, bool>(nameof(CodeBlockWrap));
    public bool CodeBlockWrap { get => GetValue(CodeBlockWrapProperty); set => SetValue(CodeBlockWrapProperty, value); }

    public static readonly StyledProperty<double> CodeBlockLineHeightProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(CodeBlockLineHeight), 20);

    public static readonly StyledProperty<double> CodeLanguageFontSizeProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(CodeLanguageFontSize), 12);

    /// <summary>是否显示代码块顶部的语言与工具栏；专注阅读和即时编辑可关闭。</summary>
    public static readonly StyledProperty<bool> ShowCodeBlockHeaderProperty =
        AvaloniaProperty.Register<MarkdownViewer, bool>(nameof(ShowCodeBlockHeader), true);

    public static readonly StyledProperty<double> UnorderedListMarkerWidthProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(UnorderedListMarkerWidth), 24);

    public static readonly StyledProperty<double> OrderedListMarkerMinWidthProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(OrderedListMarkerMinWidth), 28);

    public static readonly StyledProperty<double> OrderedListMarkerCharacterWidthProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(OrderedListMarkerCharacterWidth), 9);

    public static readonly StyledProperty<double> OrderedListMarkerExtraWidthProperty =
        AvaloniaProperty.Register<MarkdownViewer, double>(nameof(OrderedListMarkerExtraWidth), 6);

    public static readonly StyledProperty<Thickness> ListFirstParagraphMarginProperty =
        AvaloniaProperty.Register<MarkdownViewer, Thickness>(nameof(ListFirstParagraphMargin), new Thickness(0, 0, 0, 2));

    public static readonly StyledProperty<Thickness> ListNestedParagraphMarginProperty =
        AvaloniaProperty.Register<MarkdownViewer, Thickness>(nameof(ListNestedParagraphMargin), new Thickness(0, 2, 0, 2));

    /// <summary>
    /// Markdown 原文；控件只暴露这一处文本输入入口。与 <see cref="StreamingSource"/>
    /// 互斥：后设置的生效，赋值本属性会解除流式绑定。
    /// </summary>
    public string? Markdown
    {
        get => GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    /// <summary>流式文档源；语义见 <see cref="StreamingSourceProperty"/>。</summary>
    public Streaming.MarkdownStreamingDocument? StreamingSource
    {
        get => GetValue(StreamingSourceProperty);
        set => SetValue(StreamingSourceProperty, value);
    }

    /// <summary>
    /// 单个 MarkdownViewer 的排版主题 Key；未设置或设置为空时使用全局 MarkdownThemes 资源，默认行为为 Basic。
    /// </summary>
    public string? TypographyTheme
    {
        get => GetValue(TypographyThemeProperty) ?? DefaultTypographyTheme;
        set => SetValue(TypographyThemeProperty, value);
    }

    /// <summary>
    /// 单个 MarkdownViewer 的排版尺寸 Key；未设置或设置为空时使用全局 MarkdownThemes 资源，默认行为为 Normal。
    /// </summary>
    public string? TypographySize
    {
        get => GetValue(TypographySizeProperty) ?? DefaultTypographySize;
        set => SetValue(TypographySizeProperty, value);
    }

    /// <summary>
    /// Base file or directory path used to resolve relative Markdown image URLs.
    /// </summary>
    public string? ImageBasePath
    {
        get => GetValue(ImageBasePathProperty);
        set => SetValue(ImageBasePathProperty, value);
    }

    /// <summary>图片源解析回调（http(s) 源反查本地文件离线显示），经渲染上下文传给图片能力包。</summary>
    public Func<string, string?>? ImageSourceResolver
    {
        get => GetValue(ImageSourceResolverProperty);
        set => SetValue(ImageSourceResolverProperty, value);
    }

    public string SelectedText
    {
        get => _selectedText;
        private set => SetAndRaise(SelectedTextProperty, ref _selectedText, value);
    }

    public bool HasSelection
    {
        get => _hasSelection;
        private set => SetAndRaise(HasSelectionProperty, ref _hasSelection, value);
    }

    public IBrush? TextBrush
    {
        get => GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    public IBrush? MutedTextBrush
    {
        get => GetValue(MutedTextBrushProperty);
        set => SetValue(MutedTextBrushProperty, value);
    }

    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public IBrush? AccentForegroundBrush
    {
        get => GetValue(AccentForegroundBrushProperty);
        set => SetValue(AccentForegroundBrushProperty, value);
    }

    public IBrush? BorderLineBrush
    {
        get => GetValue(BorderLineBrushProperty);
        set => SetValue(BorderLineBrushProperty, value);
    }

    public IBrush? QuoteBackgroundBrush
    {
        get => GetValue(QuoteBackgroundBrushProperty);
        set => SetValue(QuoteBackgroundBrushProperty, value);
    }

    public IBrush? CodeBackgroundBrush
    {
        get => GetValue(CodeBackgroundBrushProperty);
        set => SetValue(CodeBackgroundBrushProperty, value);
    }

    public IBrush? InlineCodeBackgroundBrush
    {
        get => GetValue(InlineCodeBackgroundBrushProperty);
        set => SetValue(InlineCodeBackgroundBrushProperty, value);
    }

    public IBrush? TableHeaderBackgroundBrush
    {
        get => GetValue(TableHeaderBackgroundBrushProperty);
        set => SetValue(TableHeaderBackgroundBrushProperty, value);
    }

    public double ParagraphFontSize
    {
        get => GetValue(ParagraphFontSizeProperty);
        set => SetValue(ParagraphFontSizeProperty, value);
    }

    public double ParagraphLineHeight
    {
        get => GetValue(ParagraphLineHeightProperty);
        set => SetValue(ParagraphLineHeightProperty, value);
    }

    public double Heading1FontSize
    {
        get => GetValue(Heading1FontSizeProperty);
        set => SetValue(Heading1FontSizeProperty, value);
    }

    public double Heading2FontSize
    {
        get => GetValue(Heading2FontSizeProperty);
        set => SetValue(Heading2FontSizeProperty, value);
    }

    public double Heading3FontSize
    {
        get => GetValue(Heading3FontSizeProperty);
        set => SetValue(Heading3FontSizeProperty, value);
    }

    public double Heading4FontSize
    {
        get => GetValue(Heading4FontSizeProperty);
        set => SetValue(Heading4FontSizeProperty, value);
    }

    public double Heading5FontSize
    {
        get => GetValue(Heading5FontSizeProperty);
        set => SetValue(Heading5FontSizeProperty, value);
    }

    public double Heading6FontSize
    {
        get => GetValue(Heading6FontSizeProperty);
        set => SetValue(Heading6FontSizeProperty, value);
    }

    public double BlockSpacing
    {
        get => GetValue(BlockSpacingProperty);
        set => SetValue(BlockSpacingProperty, value);
    }

    public double DocumentBottomPadding
    {
        get => GetValue(DocumentBottomPaddingProperty);
        set => SetValue(DocumentBottomPaddingProperty, value);
    }

    public Thickness ParagraphMargin
    {
        get => GetValue(ParagraphMarginProperty);
        set => SetValue(ParagraphMarginProperty, value);
    }

    public Thickness HeadingMargin
    {
        get => GetValue(HeadingMarginProperty);
        set => SetValue(HeadingMarginProperty, value);
    }

    public FontFamily ContentFontFamily
    {
        get => GetValue(ContentFontFamilyProperty);
        set => SetValue(ContentFontFamilyProperty, value);
    }

    public FontFamily CodeFontFamily
    {
        get => GetValue(CodeFontFamilyProperty);
        set => SetValue(CodeFontFamilyProperty, value);
    }

    public double CodeBlockFontSize
    {
        get => GetValue(CodeBlockFontSizeProperty);
        set => SetValue(CodeBlockFontSizeProperty, value);
    }

    public double CodeBlockLineHeight
    {
        get => GetValue(CodeBlockLineHeightProperty);
        set => SetValue(CodeBlockLineHeightProperty, value);
    }

    public double CodeLanguageFontSize
    {
        get => GetValue(CodeLanguageFontSizeProperty);
        set => SetValue(CodeLanguageFontSizeProperty, value);
    }

    public bool ShowCodeBlockHeader
    {
        get => GetValue(ShowCodeBlockHeaderProperty);
        set => SetValue(ShowCodeBlockHeaderProperty, value);
    }

    public double UnorderedListMarkerWidth
    {
        get => GetValue(UnorderedListMarkerWidthProperty);
        set => SetValue(UnorderedListMarkerWidthProperty, value);
    }

    public double OrderedListMarkerMinWidth
    {
        get => GetValue(OrderedListMarkerMinWidthProperty);
        set => SetValue(OrderedListMarkerMinWidthProperty, value);
    }

    public double OrderedListMarkerCharacterWidth
    {
        get => GetValue(OrderedListMarkerCharacterWidthProperty);
        set => SetValue(OrderedListMarkerCharacterWidthProperty, value);
    }

    public double OrderedListMarkerExtraWidth
    {
        get => GetValue(OrderedListMarkerExtraWidthProperty);
        set => SetValue(OrderedListMarkerExtraWidthProperty, value);
    }

    public Thickness ListFirstParagraphMargin
    {
        get => GetValue(ListFirstParagraphMarginProperty);
        set => SetValue(ListFirstParagraphMarginProperty, value);
    }

    public Thickness ListNestedParagraphMargin
    {
        get => GetValue(ListNestedParagraphMarginProperty);
        set => SetValue(ListNestedParagraphMarginProperty, value);
    }

    public event EventHandler? CopyClick;

    public event EventHandler? SelectionChanged;

    /// <summary>
    /// 在代码块工具栏创建完成后触发，调用方可追加自定义按钮。
    /// </summary>
    public event EventHandler<CodeBlockToolRenderEventArgs>? CodeBlockToolRender;

    static MarkdownViewer()
    {
        SyntaxOptionsProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) =>
        {
            viewer._documentPipeline = viewer.SyntaxOptions?.CreatePipeline() ?? Pipeline;
            viewer.QueueRenderDocument(MarkdownRenderMode.Full);
        });
        MarkdownProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) =>
        {
            // 整串赋值优先：解除流式绑定，回退编辑驱动渲染。
            if (viewer.StreamingSource is not null)
            {
                viewer.SetCurrentValue(StreamingSourceProperty, null);
            }

            viewer.QueueRenderDocument(MarkdownRenderMode.Incremental);
        });
        StreamingSourceProperty.Changed.AddClassHandler<MarkdownViewer>(
            (viewer, args) => viewer.OnStreamingSourceChanged(
                args.OldValue as Streaming.MarkdownStreamingDocument,
                args.NewValue as Streaming.MarkdownStreamingDocument));
        TypographyThemeProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        TypographySizeProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        ImageBasePathProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        ImageSourceResolverProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        // 高亮 token 的颜色在生成时固定；代码背景资源变更也需重建，
        // 防止主题切换后仍保留与新底色相反的 token 配色。
        CodeBackgroundBrushProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        CodeBlockFontSizeProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        CodeFontFamilyProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
        CodeBlockWrapProperty.Changed.AddClassHandler<MarkdownViewer>((viewer, _) => viewer.QueueRenderDocument(MarkdownRenderMode.Full));
    }

    private MenuItem? _viewerCopyMenuItem;

    private readonly MarkdownBlockRendererPipeline _blockPipeline = CreateDefaultPipeline();

    private static readonly List<Rendering.IMarkdownBlockRenderer> s_externalRenderers = [];

    private static Rendering.MarkdownMathViewFactory? s_mathViewFactory;

    private static Rendering.MarkdownImageControlFactory? s_imageControlFactory;

    /// <summary>
    /// 注册全局图片控件工厂（CodeWF.Avalonia.Markdown.Images 包注册）；
    /// 未注册时图片由 Core 降级为替代文本渲染。
    /// </summary>
    public static void RegisterImageControlFactory(Rendering.MarkdownImageControlFactory? factory) =>
        s_imageControlFactory = factory;

    /// <summary>
    /// 注册全局数学视图工厂（CodeWF.Avalonia.Markdown.Math 包注册）；
    /// 未注册时数学公式由 Core 降级为原文渲染。
    /// </summary>
    public static void RegisterMathViewFactory(Rendering.MarkdownMathViewFactory? factory) =>
        s_mathViewFactory = factory;
    private static readonly object s_externalRenderersLock = new();

    /// <summary>
    /// 注册外部块级渲染器（能力包扩展点）：注册后创建的所有 Viewer 实例都会
    /// 优先尝试该渲染器，未受理再走内置渲染器（能力包可覆盖内置行为，
    /// 未安装能力包时由内置实现降级）。典型用法见 CodeWF.Avalonia.Markdown.Mermaid 包。
    /// </summary>
    public static void RegisterBlockRenderer(Rendering.IMarkdownBlockRenderer renderer)
    {
        lock (s_externalRenderersLock)
        {
            s_externalRenderers.Add(renderer);
        }
    }

    private MarkdownInlineRenderer? _inlineRenderer;

    private static Rendering.MarkdownCodeHighlighter? s_codeHighlighter;

    /// <summary>
    /// 注册全局代码高亮能力（CodeWF.Avalonia.Markdown.Highlighting 包注册）；
    /// 未注册时代码块由 Core 降级为单色等宽渲染。实例属性可按实例覆盖。
    /// </summary>
    public static void RegisterCodeHighlighter(Rendering.MarkdownCodeHighlighter? highlighter) =>
        s_codeHighlighter = highlighter;

    /// <summary>
    /// 代码高亮能力实例覆盖；为 null 时回退到全局注册的能力。
    /// </summary>
    public Rendering.MarkdownCodeHighlighter? CodeHighlighter { get; set; }

    private MarkdownInlineRenderer InlineRenderer => _inlineRenderer ??= new(this);

    private static MarkdownBlockRendererPipeline CreateDefaultPipeline()
    {
        var pipeline = new MarkdownBlockRendererPipeline();
        lock (s_externalRenderersLock)
        {
            // 能力包渲染器优先（如 Mermaid 接管 mermaid 围栏块），内置渲染器兜底：
            // 未安装能力包时由内置代码块渲染器降级显示，符合能力降级契约。
            foreach (var renderer in s_externalRenderers)
            {
                pipeline.Register(renderer);
            }
        }

        pipeline.Register(new SpecialBlockRenderer());
        pipeline.Register(new ParagraphRenderer());
        pipeline.Register(new MathBlockRenderer());
        pipeline.Register(new CodeBlockRenderer());
        pipeline.Register(new ListRenderer());
        pipeline.Register(new QuoteRenderer());
        pipeline.Register(new TableRenderer());
        pipeline.Register(new ThematicBreakRenderer());
        pipeline.Register(new HeadingRenderer());
        pipeline.Register(new FootnoteRenderer());
        pipeline.Register(new HtmlBlockRenderer());
        return pipeline;
    }

    public MarkdownViewer()
    {
        _scheduler = new MarkdownRenderScheduler(ParseMarkdownWithTiming);
        _scheduler.Completed += OnSchedulerParseCompleted;
        _scheduler.Failed += OnSchedulerParseFailed;
        Focusable = true;
        ContextMenu = CreateViewerContextMenu();
        TextOptions.SetBaselinePixelAlignment(this, BaselinePixelAlignment.Aligned);
        AddHandler(PointerPressedEvent, OnViewerPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnViewerPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnViewerPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnViewerKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// 根自动化 peer：文档角色 + 纯文本名称（能力边界见
    /// <see cref="Automation.MarkdownViewerAutomationPeer"/>）。
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() =>
        new Automation.MarkdownViewerAutomationPeer(this);

    /// <summary>
    /// 复制当前渲染结果的纯文本，作为跨多个渲染块选择时的兜底复制方式。
    /// </summary>
    public async Task CopyRenderedTextAsync()
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(HasSelection ? SelectedText : GetRenderedText());
        }
    }

    public async Task CopySelectionAsync()
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && HasSelection)
        {
            await clipboard.SetTextAsync(SelectedText);
        }
    }

    public string GetRenderedText()
    {
        return MarkdownParser.Parse(Markdown, _documentPipeline).PlainText;
    }

    public void Rerender()
    {
        QueueRenderDocument(MarkdownRenderMode.Full);
    }

    public void RenderIncremental()
    {
        QueueRenderDocument(MarkdownRenderMode.Incremental);
    }

    /// <summary>
    /// Try to locate the rendered block that contains the specified 1-based Markdown source line.
    /// </summary>
    public bool TryGetSourceLineBounds(int sourceLine, out Rect bounds)
    {
        var text = Markdown ?? string.Empty;
        var offset = GetSourceLineOffset(text, sourceLine);
        return TryGetSourceOffsetBounds(offset, out bounds);
    }

    /// <summary>
    /// Try to locate the rendered block that contains the specified Markdown source offset.
    /// The returned bounds are relative to this <see cref="MarkdownViewer"/>.
    /// 同步返回最近一块已物化块的边界；后台解析尚未完成时不会强制同步解析。
    /// </summary>
    public bool TryGetSourceOffsetBounds(int sourceOffset, out Rect bounds)
    {
        var text = Markdown ?? string.Empty;
        return TryGetSourceOffsetBoundsNow(Math.Clamp(sourceOffset, 0, text.Length), out bounds);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _documentHost = e.NameScope.Find<Panel>(DocumentHostPartName);
        if (_documentHost is WindowedStackPanel panel)
        {
            _virtualizingHost = panel;
            panel.ScrollHost ??= ResolveScrollHost();
            // 只有重块参与离屏保留：轻量段落照常解体，内存占用最小。
            panel.KeepAliveCandidateResolver = IsHeavyBlockAt;
            if (panel.ScrollHost is { } host)
            {
                host.ScrollChanged += OnViewerScrollHostChanged;
                _documentHost.DetachedFromVisualTree += (_, _) => host.ScrollChanged -= OnViewerScrollHostChanged;
            }
        }

        RenderDocument(MarkdownRenderMode.Full);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Application.Current is { } app)
        {
            app.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        }

        I18nManager.Instance.CultureChanged += OnCultureChanged;
        I18nManager.Instance.ResourcesChanged += OnCultureChanged;
        // 控件离开界面期间语言可能已改变，重新挂载时同步菜单文案。
        ContextMenu = CreateViewerContextMenu();
        QueueRenderDocument(MarkdownRenderMode.Full);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (Application.Current is { } app)
        {
            app.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        }

        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        I18nManager.Instance.ResourcesChanged -= OnCultureChanged;
        base.OnDetachedFromVisualTree(e);
    }

    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (ReferenceEquals(e.Source, this)
            && e.Key == Key.C
            && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            await CopyRenderedTextAsync();
            e.Handled = true;
        }
    }

    private async void OnViewerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.C
            && (e.KeyModifiers & KeyModifiers.Control) != 0
            && HasSelection)
        {
            await CopySelectionAsync();
            e.Handled = true;
        }
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        // 代码高亮颜色由 TextMate 直接生成，明暗主题变化时需要重建。
        QueueRenderDocument(MarkdownRenderMode.Full);
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnCultureChanged(sender, e));
            return;
        }
        ContextMenu = CreateViewerContextMenu();
        QueueRenderDocument(MarkdownRenderMode.Full);
    }

    /// <summary>
    /// 滚动时把「最靠近视口、沿滚动方向尚未到来」的 1-2 个已量测块固定物化，
    /// 并解除上一次的固定。这样滚动方向的窗口外边界始终留着一个尺寸已知的驻留块，
    /// 变高块（图片、图表、公式）量测带来的高度变化不会让视口内容整体跳动。
    /// </summary>
    private void OnViewerScrollHostChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_virtualizingHost is not { } host || _renderedBlocks.Count == 0)
        {
            return;
        }

        // 直接读宿主偏移，而不是累加 OffsetDelta：事件合并时会丢增量，累加值会漂移。
        var offsetY = host.ScrollHost?.Offset.Y ?? _lastScrollOffsetY;
        var scrollDown = offsetY > _lastScrollOffsetY;
        _lastScrollOffsetY = offsetY;

        var center = offsetY + Math.Max(0, host.ScrollHost?.Viewport.Height ?? 0) / 2;
        var anchor = -1;
        for (var i = 0; i < _renderedBlocks.Count; i++)
        {
            if (!host.IsItemMeasured(i))
            {
                continue;
            }

            if (host.TryGetItemTop(i, out var top) && top + host.GetItemHeight(i) < center)
            {
                continue;
            }

            anchor = i;
            break;
        }

        var secondary = anchor < 0 ? -1 : anchor + (scrollDown ? 1 : -1);
        for (var i = 0; i < _renderedBlocks.Count; i++)
        {
            if (i != anchor && i != secondary)
            {
                host.ReleaseItem(i);
            }
        }

        foreach (var index in stackalloc[] { anchor, secondary })
        {
            if (index >= 0 && index < host.ItemCount)
            {
                host.PinItem(index);
            }
        }
    }

    /// <summary>块序号是否为重块（保留候选），供面板决定离屏后是否保留控件。</summary>
    private bool IsHeavyBlockAt(int index) =>
        index >= 0 && index < _renderedBlocks.Count && _renderedBlocks[index].IsHeavy;

    private void QueueRenderDocument(MarkdownRenderMode mode)
    {
        if (mode == MarkdownRenderMode.Full)
        {
            _queuedRenderMode = MarkdownRenderMode.Full;
        }

        if (_documentHost is null)
        {
            return;
        }

        if (_isRenderQueued)
        {
            return;
        }

        _isRenderQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _isRenderQueued = false;

            var queuedMode = _queuedRenderMode;
            _queuedRenderMode = MarkdownRenderMode.Incremental;
            RenderDocument(queuedMode);
        }, DispatcherPriority.Render);
    }

    private void RenderDocument(MarkdownRenderMode mode)
    {
        if (_documentHost is null)
        {
            return;
        }

        ScheduleRender(mode);
    }

    /// <summary>
    /// 流式 append-only 更新判定：绑定流式源且新文本以当前渲染文本为前缀——
    /// 是同一文档在生长而非替换（规格：源码缓冲只追加）。此场景下块键绝对
    /// 稳定，用户状态（折叠、高亮会话）跨快照保留。
    /// </summary>
    private bool IsAppendOnlyStreamingUpdate(string text) =>
        StreamingSource is not null
        && text.StartsWith(_renderedMarkdown ?? string.Empty, StringComparison.Ordinal);

    private void RenderDocumentFull(string text, MarkdownDocumentModel? parsedModel = null)
    {
        if (_documentHost is null)
        {
            return;
        }

        foreach (var block in _renderedBlocks)
        {
            block.Cleanup();
        }
        DisposePendingBlockDisposables(0);
        // 整篇重建后块序号全部失效，先清掉上一次的滚动锚点固定。
        _virtualizingHost?.DetachAllPinned();
        // 换文档时折叠状态/高亮会话（按绝对源码偏移键控）随之失效；同文档重渲染
        //（主题/字号）保留；流式 append-only 增长同样保留——块键绝对稳定，
        // 新文本以旧文本为前缀说明是同一文档在生长而非替换。
        if (!string.Equals(text, _renderedMarkdown, StringComparison.Ordinal)
            && !IsAppendOnlyStreamingUpdate(text))
        {
            _codeBlockFoldedStates.Clear();
            _codeHighlightSessions.Clear();
        }

        ClearBlockControls();
        _renderedBlocks.Clear();
        _renderedMarkdown = text;
        // 全文重绘也推进调度快照，避免 A → B → 全文 A → B 被误判为未变更。
        _renderedModel = parsedModel ?? _scheduler.ParseNow(text, out _lastParseDuration);

        _lastRenderMode = MarkdownRenderMode.Full;
        ResetSelectionState();

        if (string.IsNullOrWhiteSpace(text))
        {
            InvalidateDocumentLayout();
            return;
        }

        foreach (var renderedBlock in CreateRenderedBlocks(_renderedModel.Blocks, text))
        {
            AddBlockControl(renderedBlock.Control);
            _renderedBlocks.Add(renderedBlock);
        }

        RefreshSelectionBlocks();
        InvalidateDocumentLayout();
    }

    private bool TryRenderIncremental(string text, MarkdownDocumentModel? parsedModel, MarkdownTextSpan? dirtySpan)
    {
        if (_documentHost is null)
        {
            return false;
        }

        if (text == _renderedMarkdown)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(_renderedMarkdown) || _renderedBlocks.Count == 0)
        {
            return false;
        }

        var newModel = parsedModel ?? MarkdownParser.Parse(text, _documentPipeline);
        var diff = dirtySpan is { Length: > 0 } span
            ? MarkdownDiffService.TryCompareWithinDirtySpan(_renderedModel, newModel, span)
              ?? MarkdownDiffService.Compare(_renderedModel, newModel)
            : MarkdownDiffService.Compare(_renderedModel, newModel);
        if (diff.RequiresFullRender)
        {
            return false;
        }

        if (diff.OldRemoveCount == 0 && diff.NewInsertCount == 0)
        {
            _renderedMarkdown = text;
            _renderedModel = newModel;
            _lastRenderMode = MarkdownRenderMode.Incremental;
            return true;
        }

        // 被替换的源码块不再沿用旧折叠状态；未变更的块保留用户操作。
        // 流式 append-only 增长例外：被替换的尾块是同一逻辑块在生长（块键绝对
        // 稳定），用户的折叠操作必须跨快照保留。
        if (!IsAppendOnlyStreamingUpdate(text))
        {
            foreach (var block in _renderedModel.Blocks.Skip(diff.ReplaceStartIndex).Take(diff.OldRemoveCount))
            {
                _codeBlockFoldedStates.Remove(block.SourceSpan.Start);
            }

            foreach (var block in newModel.Blocks.Skip(diff.NewStartIndex).Take(diff.NewInsertCount))
            {
                _codeBlockFoldedStates.Remove(block.SourceSpan.Start);
            }
        }

        var renderedNewBlocks = CreateRenderedBlocks(
            newModel.Blocks.Skip(diff.NewStartIndex).Take(diff.NewInsertCount),
            text, newModel.Blocks.Count);
        ReplaceRenderedBlocks(diff.ReplaceStartIndex, diff.ReplaceEndIndex, renderedNewBlocks, newModel);

        _renderedMarkdown = text;
        _renderedModel = newModel;
        _lastRenderMode = MarkdownRenderMode.Incremental;
        return true;
    }

    private IReadOnlyList<RenderedBlock> CreateRenderedBlocks(IEnumerable<MarkdownDocumentBlock> modelBlocks, string markdown, int? documentBlockCount = null)
    {
        var renderedBlocks = new List<RenderedBlock>();
        foreach (var modelBlock in modelBlocks)
        {
            // 增量更新时当前快照仍是旧文档；从短节恢复全文须按新模型决定延迟创建。
            if (_virtualizingHost is { EnableVirtualization: true }
                && ((documentBlockCount ?? _renderedModel.Blocks.Count) >= 200
                    || modelBlock.SyntaxBlock is FencedCodeBlock fenced && string.Equals(fenced.Info?.Trim(), "mermaid", StringComparison.OrdinalIgnoreCase)))
            {
                var deferred = new DeferredMarkdownBlock(() => CreateBlockContent(modelBlock, markdown));
                renderedBlocks.Add(RenderedBlock.FromModel(modelBlock, deferred, null,
                    ContainsRemoteImage(modelBlock), TryGetTaskMarkerOffset(markdown, modelBlock)));
                continue;
            }
            var (control, blockDisposables) = CreateBlockContent(modelBlock, markdown);
            renderedBlocks.Add(RenderedBlock.FromModel(modelBlock, control, blockDisposables,
                ContainsRemoteImage(modelBlock), TryGetTaskMarkerOffset(markdown, modelBlock)));
        }

        return renderedBlocks;
    }

    private (Control Control, List<IDisposable>? Bindings) CreateBlockContent(MarkdownDocumentBlock modelBlock, string markdown)
    {
            // 块键原点：解析输入（分段/尾部文本）的绝对起始 = 模型块绝对起点 - 其相对 Span 起点。
            // 代码块渲染器用它把 Markdig 的相对 Span 平移回原始源码坐标（规格：块键 = 原始源码起始偏移）。
            var syntaxStart = Math.Max(modelBlock.SyntaxBlock.Span.Start, 0);
            _currentBlockOrigin = Math.Max(modelBlock.SourceSpan.Start - syntaxStart, 0);
            var previousDisposableCount = _currentBlockDisposables.Count;
            var control = ConvertBlock(modelBlock.SyntaxBlock, markdown);
            if (control is null)
            {
                DisposePendingBlockDisposables(previousDisposableCount);
                // 差分和源位置映射使用模型索引；不可见块也必须保留对应的控件槽位。
                control = new Border { Height = 0, IsHitTestVisible = false };
            }

            var blockDisposables = ExtractCurrentBlockDisposables(previousDisposableCount);
            return (control, blockDisposables);
    }

    /// <summary>长文档只为视口内的块建立高亮、表格与公式控件，回收时释放绑定与子树。</summary>
    private sealed class DeferredMarkdownBlock(Func<(Control Control, List<IDisposable>? Bindings)> create) : Decorator, IDisposable
    {
        private List<IDisposable>? _bindings;
        private bool _disposed;
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Child is null && !_disposed)
            {
                var content = create(); _bindings = content.Bindings; Child = content.Control;
            }
            return base.MeasureOverride(availableSize);
        }
        protected override void OnDetachedFromLogicalTree(global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e); ReleaseContent();
        }
        private void ReleaseContent()
        {
            if (_bindings is not null) { foreach (var binding in _bindings) binding.Dispose(); _bindings = null; }
            Child = null;
        }
        public void Dispose() { _disposed = true; ReleaseContent(); }
    }

    private List<IDisposable>? ExtractCurrentBlockDisposables(int startIndex)
    {
        var count = _currentBlockDisposables.Count - startIndex;
        if (count <= 0)
        {
            return null;
        }

        var blockDisposables = _currentBlockDisposables.GetRange(startIndex, count);
        _currentBlockDisposables.RemoveRange(startIndex, count);
        return blockDisposables;
    }

    private void DisposePendingBlockDisposables(int startIndex)
    {
        for (var i = _currentBlockDisposables.Count - 1; i >= startIndex; i--)
        {
            _currentBlockDisposables[i].Dispose();
            _currentBlockDisposables.RemoveAt(i);
        }
    }

    private void ReplaceRenderedBlocks(
        int replaceStartIndex,
        int replaceEndIndex,
        IReadOnlyList<RenderedBlock> newBlocks,
        MarkdownDocumentModel newModel)
    {
        if (_documentHost is null)
        {
            return;
        }

        var removeCount = replaceEndIndex - replaceStartIndex;
        for (var i = 0; i < removeCount; i++)
        {
            _renderedBlocks[replaceStartIndex].Cleanup();
            RemoveBlockControlAt(replaceStartIndex);
            _renderedBlocks.RemoveAt(replaceStartIndex);
        }

        for (var i = 0; i < newBlocks.Count; i++)
        {
            var renderedBlock = newBlocks[i];
            AddBlockControl(replaceStartIndex + i, renderedBlock.Control);
            _renderedBlocks.Insert(replaceStartIndex + i, renderedBlock);
        }

        for (var i = 0; i < _renderedBlocks.Count && i < newModel.Blocks.Count; i++)
        {
            _renderedBlocks[i] = _renderedBlocks[i].WithModel(newModel.Blocks[i]);
        }

        ResetSelectionState();
        RefreshSelectionBlocks();
        InvalidateDocumentLayout();
    }

    private void ReplaceRenderedBlocks(
        int replaceStartIndex,
        int replaceEndIndex,
        IReadOnlyList<RenderedBlock> newBlocks,
        int delta)
    {
        if (_documentHost is null)
        {
            return;
        }

        var removeCount = replaceEndIndex - replaceStartIndex;
        for (var i = 0; i < removeCount; i++)
        {
            _renderedBlocks[replaceStartIndex].Cleanup();
            RemoveBlockControlAt(replaceStartIndex);
            _renderedBlocks.RemoveAt(replaceStartIndex);
        }

        for (var i = 0; i < newBlocks.Count; i++)
        {
            var renderedBlock = newBlocks[i];
            AddBlockControl(replaceStartIndex + i, renderedBlock.Control);
            _renderedBlocks.Insert(replaceStartIndex + i, renderedBlock);
        }

        for (var i = replaceStartIndex + newBlocks.Count; i < _renderedBlocks.Count; i++)
        {
            _renderedBlocks[i] = _renderedBlocks[i].Shift(delta);
        }

        InvalidateDocumentLayout();
    }

    private void OnViewerPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_documentHost is null
            || e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed
            || IsLinkPointerSource(e)
            || IsInteractiveSelectionSource(e.Source as Visual))
        {
            return;
        }

        var documentPoint = e.GetPosition(_documentHost);
        if (_selectionController.Begin(documentPoint))
        {
            _pointerSelectionState = new MarkdownPointerSelectionState(e.GetPosition(this));
            _nativeSelectionTextBlock = FindSelectableTextBlock(e.Source as Visual);
            _isPointerSelecting = false;
            UpdateSelectionState();
        }
    }

    private void OnViewerPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_documentHost is null || _pointerSelectionState is not { } state)
        {
            return;
        }

        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed)
        {
            _pointerSelectionState = null;
            _nativeSelectionTextBlock = null;
            _isPointerSelecting = false;
            e.Pointer.Capture(null);
            return;
        }

        if (!state.IsDragging(e.GetPosition(this)))
        {
            return;
        }

        if (!_isPointerSelecting
            && _nativeSelectionTextBlock is { } nativeTextBlock
            && IsPointerInside(nativeTextBlock, e))
        {
            return;
        }

        if (_selectionController.Extend(e.GetPosition(_documentHost)))
        {
            _isPointerSelecting = true;
            e.Pointer.Capture(this);
            ClearNativeTextSelections();
            UpdateSelectionState();
            e.Handled = true;
            return;
        }

        if (_isPointerSelecting)
        {
            e.Handled = true;
        }
    }

    private void OnViewerPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_pointerSelectionState is null)
        {
            return;
        }

        if (_isPointerSelecting)
        {
            e.Handled = true;
        }
        else
        {
            _selectionController.CommitClickWithoutDrag();
            UpdateSelectionState();
        }

        _pointerSelectionState = null;
        _nativeSelectionTextBlock = null;
        _isPointerSelecting = false;
        e.Pointer.Capture(null);
    }

    private void RefreshSelectionBlocks()
    {
        _selectionController.SetBlocks(_renderedBlocks.Select(block =>
            new MarkdownSelectionBlock(
                block.Control,
                new MarkdownTextSpan(block.PlainTextStart, block.PlainTextEnd),
                block.PlainText)), _documentHost);
        UpdateSelectionState();
    }

    private void ResetSelectionState()
    {
        _selectionController.Clear();
        _pointerSelectionState = null;
        _nativeSelectionTextBlock = null;
        _isPointerSelecting = false;
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        var selectedText = _selectionController.SelectedText;
        var hasSelection = !string.IsNullOrEmpty(selectedText);
        var changed = selectedText != SelectedText || hasSelection != HasSelection;
        SelectedText = selectedText;
        HasSelection = hasSelection;
        if (changed)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ClearNativeTextSelections()
    {
        if (_documentHost is null)
        {
            return;
        }

        foreach (var textBlock in _documentHost.GetVisualDescendants().OfType<SelectableTextBlock>())
        {
            if (!string.IsNullOrEmpty(textBlock.SelectedText))
            {
                textBlock.ClearSelection();
            }
        }
    }

    private static bool IsInteractiveSelectionSource(Visual? source)
    {
        for (var current = source; current is not null; current = current.GetVisualParent())
        {
            if (current is Button
                or MenuItem
                or CheckBox
                or ScrollBar
                or IMarkdownInteractiveVisual)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPointerInside(Control control, PointerEventArgs e)
    {
        var point = e.GetPosition(control);
        return new Rect(control.Bounds.Size).Contains(point);
    }

    private static bool IsLinkPointerSource(PointerEventArgs e)
    {
        if (FindSelectableTextBlock(e.Source as Visual) is not { } textBlock)
        {
            return false;
        }

        string? url;
        return MarkdownLinkInteraction.TryGetLinkAtPointer(textBlock, e, out url);
    }

    private void InvalidateDocumentLayout()
    {
        _documentHost?.InvalidateMeasure();
        _documentHost?.InvalidateArrange();
        _virtualizingHost?.InvalidateMeasure();
        _virtualizingHost?.InvalidateArrange();
        InvalidateMeasure();
        InvalidateArrange();
    }

    private RenderedBlock? FindRenderedBlockBySourceOffset(int sourceOffset)
    {
        if (_renderedBlocks.Count == 0)
        {
            return null;
        }

        foreach (var block in _renderedBlocks)
        {
            if (sourceOffset >= block.Start && sourceOffset <= block.End)
            {
                return block;
            }

            if (sourceOffset < block.Start)
            {
                return block;
            }
        }

        return _renderedBlocks[^1];
    }

    private static int GetSourceLineOffset(string text, int sourceLine)
    {
        if (sourceLine <= 1 || string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var line = 1;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n')
            {
                continue;
            }

            line++;
            if (line == sourceLine)
            {
                return Math.Min(i + 1, text.Length);
            }
        }

        return text.Length;
    }

    private int FindReplaceStartIndex(int oldChangeStart)
    {
        if (_renderedBlocks.Count == 0)
        {
            return -1;
        }

        for (var i = 0; i < _renderedBlocks.Count; i++)
        {
            var block = _renderedBlocks[i];
            if (oldChangeStart <= block.End)
            {
                return i > 0 && oldChangeStart <= block.Start ? i - 1 : i;
            }
        }

        return _renderedBlocks.Count - 1;
    }

    private int FindReplaceEndIndex(TextChange change, int replaceStartIndex)
    {
        var boundary = Math.Max(change.OldEnd, change.OldStart);
        var endIndex = replaceStartIndex;
        while (endIndex < _renderedBlocks.Count && _renderedBlocks[endIndex].Start < boundary)
        {
            endIndex++;
        }

        if (endIndex == replaceStartIndex)
        {
            endIndex++;
        }

        if (change.OldStart == change.OldEnd
            && endIndex < _renderedBlocks.Count
            && _renderedBlocks[endIndex].Start == change.OldStart)
        {
            endIndex++;
        }

        return Math.Min(endIndex, _renderedBlocks.Count);
    }

    private static TextRange GetBlockRange(Block block, int sourceOffset, int markdownLength)
    {
        var start = block.Span.Start >= 0 ? block.Span.Start : 0;
        var end = block.Span.End >= start ? block.Span.End + 1 : markdownLength;

        start = Math.Clamp(start, 0, markdownLength);
        end = Math.Clamp(end, start, markdownLength);

        return new TextRange(sourceOffset + start, sourceOffset + end);
    }

    private static TextChange CalculateTextChange(string oldText, string newText)
    {
        var prefixLength = 0;
        var minLength = Math.Min(oldText.Length, newText.Length);
        while (prefixLength < minLength && oldText[prefixLength] == newText[prefixLength])
        {
            prefixLength++;
        }

        var suffixLength = 0;
        while (suffixLength < oldText.Length - prefixLength
               && suffixLength < newText.Length - prefixLength
               && oldText[oldText.Length - suffixLength - 1] == newText[newText.Length - suffixLength - 1])
        {
            suffixLength++;
        }

        return new TextChange(
            prefixLength,
            oldText.Length - suffixLength,
            prefixLength,
            newText.Length - suffixLength,
            newText.Length - oldText.Length);
    }

    private static bool ShouldFullRender(TextChange change, int oldLength, int newLength)
    {
        var preservedLength = change.OldStart + oldLength - change.OldEnd;
        if (oldLength > 0 && preservedLength < oldLength / 2)
        {
            return true;
        }

        var newChangedLength = change.NewEnd - change.NewStart;
        return newLength > 4096 && newChangedLength > Math.Max(4096, newLength * 9 / 10);
    }

    private static int MapOldStartOffsetToNew(int oldOffset, TextChange change)
    {
        if (oldOffset <= change.OldStart)
        {
            return oldOffset;
        }

        if (oldOffset >= change.OldEnd)
        {
            return oldOffset + change.Delta;
        }

        return change.NewStart;
    }

    private static int MapOldEndOffsetToNew(int oldOffset, TextChange change)
    {
        if (oldOffset < change.OldStart)
        {
            return oldOffset;
        }

        if (oldOffset >= change.OldEnd)
        {
            return oldOffset + change.Delta;
        }

        return change.NewEnd;
    }

    private Control? ConvertBlock(Block block, string? sourceMarkdown = null)
    {
        // 管线渲染器优先（特殊块/数学/代码/排版块等），未受理再走内置兜底。
        if (_blockPipeline.Render(block, sourceMarkdown, this) is { } pipelineBlock)
        {
            return pipelineBlock;
        }

        if (block is LinkReferenceDefinitionGroup or LinkReferenceDefinition)
        {
            return null;
        }

        return block switch
        {
            LinkReferenceDefinitionGroup => null,
            LinkReferenceDefinition => null,
            _ => CreateUnknownBlock(block, sourceMarkdown)
        };
    }

    /// <summary>
    /// 决定代码块 token 配色是否使用深色主题：
    /// 以实际生效的代码块背景（排版主题按变体提供的 <see cref="TextBlockCodeBackground"/> 或
    /// 显式设置的 <see cref="CodeBackgroundBrush"/>）的感知亮度为准。
    /// 此前按 <c>ActualThemeVariant == ThemeVariant.Dark</c> 判定，自定义主题变体
    /// （Desert/NightSky 等，基座为 Light/Dark 但变体本身不等于 Dark）恒判为浅色，
    /// 导致深色代码块底上渲染浅色主题的深色 token（键名/标点不可读）。
    /// </summary>
    private bool ResolveCodeBlockIsDark()
    {
        if (GetValue(CodeBackgroundBrushProperty) is ISolidColorBrush solid
            && solid.Color.A > 0)
        {
            return GetPerceivedLuminance(ResolveVisibleCodeBackground(solid)) < 0.5;
        }

        // 排版主题未定义代码背景资源（如 Simple）时解析失败，默认按深色底处理：
        // 内置排版主题的代码块底色即为深色系，DarkPlus token 在其上可读。
        if (TryGetResource(MarkdownStyleKeys.CodeBackgroundBrushResource, ActualThemeVariant, out var resource)
            && resource is ISolidColorBrush themeBrush)
        {
            return GetPerceivedLuminance(ResolveVisibleCodeBackground(themeBrush)) < 0.5;
        }

        return true;
    }

    /// <summary>将半透明代码底色与实际文档背景合成，避免浅底透明黑被误判为深色。</summary>
    private Color ResolveVisibleCodeBackground(ISolidColorBrush codeBrush)
    {
        static Color Over(Color foreground, Color background, double opacity = 1)
        {
            var alpha = foreground.A / 255d * opacity;
            var backAlpha = background.A / 255d * (1 - alpha);
            var total = alpha + backAlpha;
            if (total <= 0) return Colors.Transparent;
            byte Channel(byte first, byte second) => (byte)Math.Round((first * alpha + second * backAlpha) / total);
            return Color.FromArgb((byte)Math.Round(total * 255), Channel(foreground.R, background.R),
                Channel(foreground.G, background.G), Channel(foreground.B, background.B));
        }

        var background = Colors.Transparent;
        foreach (var ancestor in this.GetVisualAncestors().Prepend(this))
        {
            var brush = ancestor switch
            {
                Border border => border.Background,
                Panel panel => panel.Background,
                TemplatedControl control => control.Background,
                _ => null
            };
            if (brush is ISolidColorBrush solid)
                background = Over(background, Over(solid.Color, Colors.Transparent, solid.Opacity));
            if (background.A == 255) break;
        }
        // 尚未附加到可视树时仍依据主题基座选择画布，不以透明色 RGB 判断明暗。
        var variant = ActualThemeVariant;
        while (variant.InheritVariant is { } inherited) variant = inherited;
        var canvas = variant == ThemeVariant.Dark ? Colors.Black : Colors.White;
        background = Over(background, canvas);
        return Over(codeBrush.Color, background, codeBrush.Opacity);
    }

    private static double GetPerceivedLuminance(Color color)
    {
        return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
    }

    private SelectableTextBlock CreateFallbackText(string text, string className)
    {
        var textBlock = CreateSelectableText(className);
        textBlock.Text = text;
        BindTheme(textBlock, SelectableTextBlock.ForegroundProperty, MutedTextBrushProperty);
        BindTheme(textBlock, SelectableTextBlock.FontFamilyProperty, ContentFontFamilyProperty);
        BindTheme(textBlock, SelectableTextBlock.FontSizeProperty, ParagraphFontSizeProperty);
        BindTheme(textBlock, SelectableTextBlock.LineHeightProperty, ParagraphLineHeightProperty);
        return textBlock;
    }

    private Control? CreateUnknownBlock(Block block, string? sourceMarkdown)
    {
        var text = block.ToString() ?? string.Empty;
        if (MarkdownPlainTextExtractor.IsTypeNameFallback(text, block.GetType()))
        {
            // 扩展块（如 ::: warning）的 ToString() 可能只返回类型名，回退到实际源码。
            if (sourceMarkdown is null || block.Span.Start < 0 || block.Span.End < block.Span.Start
                || block.Span.End >= sourceMarkdown.Length)
            {
                return null;
            }

            text = sourceMarkdown[block.Span.Start..(block.Span.End + 1)];
        }

        return CreateFallbackText(text, MarkdownStyleKeys.UnknownBlock);
    }

    private static SelectableTextBlock? FindSelectableTextBlock(Visual? source)
    {
        for (var current = source; current is not null; current = current.GetVisualParent())
        {
            if (current is SelectableTextBlock textBlock)
            {
                return textBlock;
            }
        }

        return null;
    }

    private SelectableTextBlock CreateSelectableText(params string[] classes)
    {
        var textBlock = new MarkdownSelectableTextBlock
        {
            Inlines = new InlineCollection(),
            TextWrapping = TextWrapping.Wrap
        };
        TextOptions.SetBaselinePixelAlignment(textBlock, BaselinePixelAlignment.Aligned);
        AddMarkdownClass(textBlock, classes);
        textBlock.ContextMenu = CreateSelectableTextContextMenu(textBlock);
        return textBlock;
    }

    private ContextMenu CreateViewerContextMenu()
    {
        if (_viewerCopyMenuItem is null)
        {
            _viewerCopyMenuItem = new MenuItem();
            _viewerCopyMenuItem.Click += async (_, _) => await CopyRenderedTextAsync();
        }
        _viewerCopyMenuItem.Header = MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText);
        return new ContextMenu { ItemsSource = new[] { _viewerCopyMenuItem } };
    }

    private ContextMenu CreateSelectableTextContextMenu(SelectableTextBlock textBlock)
    {
        var copySelectionItem = new MenuItem
        {
            Header = MarkdownLocalization.GetResource(MarkdownL.CopySelectedText)
        };
        copySelectionItem.Click += async (_, _) =>
        {
            if (HasSelection)
            {
                await CopySelectionAsync();
            }
            else
            {
                textBlock.Copy();
            }
        };

        var copyRenderedTextItem = new MenuItem
        {
            Header = MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText)
        };
        copyRenderedTextItem.Click += async (_, _) => await CopyRenderedTextAsync();

        return new ContextMenu { ItemsSource = new[] { copySelectionItem, copyRenderedTextItem } };
    }

    private void AddMarkdownClass(Control control, params string[] classes)
    {
        foreach (var className in classes.Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            control.Classes.Add(className);
        }
    }

    private static bool TryGetSingleTextLink(ContainerInline? container, out string? url)
    {
        url = null;
        if (container?.FirstChild is LinkInline { IsImage: false } link
            && link.NextSibling is null
            && !string.IsNullOrWhiteSpace(link.Url))
        {
            url = link.Url;
            return true;
        }

        return false;
    }

    private IDisposable BindTheme<T>(
        AvaloniaObject target,
        AvaloniaProperty<T> targetProperty,
        StyledProperty<T> sourceProperty,
        BindingPriority priority = BindingPriority.LocalValue)
    {
        var disposable = target.Bind(targetProperty, this.GetObservable(sourceProperty), priority);
        _currentBlockDisposables.Add(disposable);
        return disposable;
    }

    IDisposable Rendering.IMarkdownRenderContext.BindTheme<T>(
        AvaloniaObject target,
        AvaloniaProperty<T> targetProperty,
        StyledProperty<T> sourceProperty) => BindTheme(target, targetProperty, sourceProperty);

    IDisposable Rendering.IMarkdownRenderContext.BindTheme<T>(AvaloniaObject target,
        AvaloniaProperty<T> targetProperty, StyledProperty<T> sourceProperty, BindingPriority priority) =>
        BindTheme(target, targetProperty, sourceProperty, priority);

    SelectableTextBlock Rendering.IMarkdownRenderContext.CreateSelectableText(params string[] classes) =>
        CreateSelectableText(classes);

    void Rendering.IMarkdownRenderContext.AddMarkdownClass(Control control, params string[] classes) =>
        AddMarkdownClass(control, classes);

    Rendering.MarkdownMathViewFactory? Rendering.IMarkdownRenderContext.MathViewFactory => s_mathViewFactory;

    Rendering.MarkdownImageControlFactory? Rendering.IMarkdownRenderContext.ImageControlFactory => s_imageControlFactory;

    Control? Rendering.IMarkdownRenderContext.CreateImageControl(string source, string altText) =>
        s_imageControlFactory?.Invoke(this, source, altText);

    Control? Rendering.IMarkdownRenderContext.CreateMathView(string latex, double fontSize, Rendering.MarkdownMathLineStyle lineStyle) =>
        s_mathViewFactory?.Invoke(this, latex, fontSize, lineStyle);

    Control Rendering.IMarkdownRenderContext.CreateFallbackText(string text, string className) =>
        CreateFallbackText(text, className);

    Rendering.MarkdownCodeHighlighter? Rendering.IMarkdownRenderContext.CodeHighlighter => CodeHighlighter ?? s_codeHighlighter;

    bool Rendering.IMarkdownRenderContext.CodeBlockIsDark => ResolveCodeBlockIsDark();

    FontFamily Rendering.IMarkdownRenderContext.CodeFontFamily => CodeFontFamily;

    double Rendering.IMarkdownRenderContext.CodeBlockFontSize => CodeBlockFontSize;

    double Rendering.IMarkdownRenderContext.CodeBlockLineHeight => CodeBlockLineHeight;

    bool Rendering.IMarkdownRenderContext.HasSelection => HasSelection;

    void Rendering.IMarkdownRenderContext.CopyCodeToClipboard(string code)
    {
        CopyClick?.Invoke(this, EventArgs.Empty);
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            _ = clipboard.SetTextAsync(code);
        }
    }

    Task Rendering.IMarkdownRenderContext.CopySelectionAsync() => CopySelectionAsync();

    void Rendering.IMarkdownRenderContext.RaiseCodeBlockToolRender(StackPanel header, StackPanel content, CodeBlock codeBlock) =>
        CodeBlockToolRender?.Invoke(this, new CodeBlockToolRenderEventArgs(header, content, codeBlock));

    Control? Rendering.IMarkdownRenderContext.ConvertBlock(Block block, string? sourceMarkdown) =>
        ConvertBlock(block, sourceMarkdown);

    SelectableTextBlock Rendering.IMarkdownRenderContext.CreateParagraph(ParagraphBlock paragraph, bool stripTaskPrefix, Thickness? marginOverride) =>
        ParagraphRenderer.CreateParagraph(paragraph, stripTaskPrefix, marginOverride, this);

    private System.Collections.Generic.IEnumerable<global::Avalonia.Controls.Documents.Inline> ConvertInlines(
        Markdig.Syntax.Inlines.ContainerInline? container,
        bool stripTaskPrefix = false) => InlineRenderer.ConvertInlines(container, stripTaskPrefix);

    System.Collections.Generic.IEnumerable<global::Avalonia.Controls.Documents.Inline> Rendering.IMarkdownRenderContext.ConvertInlines(
        Markdig.Syntax.Inlines.ContainerInline? container,
        bool stripTaskPrefix) => ConvertInlines(container, stripTaskPrefix);

    void Rendering.IMarkdownRenderContext.AttachLinkInteraction(
        SelectableTextBlock textBlock,
        Markdig.Syntax.Inlines.ContainerInline? container,
        bool stripTaskPrefix) =>
        MarkdownLinkInteraction.AttachLinkInteraction(textBlock, MarkdownLinkInteraction.ExtractLinkSpans(container, stripTaskPrefix), this);

    /// <summary>
    /// 链接被点击（打开之前触发）：<see cref="MarkdownLinkClickEventArgs.RequiresConfirmation"/>
    /// 为 true 时建议宿主先弹确认框（防钓鱼），置 <see cref="MarkdownLinkClickEventArgs.Cancel"/>
    /// 可阻止打开。未订阅时链接按白名单策略直接打开。
    /// </summary>
    public event EventHandler<MarkdownLinkClickEventArgs>? LinkClicked;

    void Rendering.IMarkdownRenderContext.RaiseLinkClicked(MarkdownLinkClickEventArgs args) =>
        LinkClicked?.Invoke(this, args);

    // 代码块折叠状态：键 = 块源码起始偏移。存放在 Viewer 而非控件上，
    // 因此虚拟化回收、块重建、主题切换（同文档全量重渲染）都不丢状态。
    private readonly Dictionary<int, bool> _codeBlockFoldedStates = [];

    /// <summary>查询折叠状态（宿主与渲染器共用；未折叠过返回 false）。</summary>
    public bool IsCodeBlockFolded(int blockKey) =>
        EnableCodeBlockFolding && _codeBlockFoldedStates.TryGetValue(blockKey, out var folded) && folded;

    /// <summary>切换折叠状态并就地位刷新该代码块。</summary>
    public void SetCodeBlockFolded(int blockKey, bool folded)
    {
        _codeBlockFoldedStates[blockKey] = folded;

        var index = _renderedBlocks.FindIndex(
            rendered => rendered.Start == blockKey && rendered.Kind == MarkdownBlockKind.Code);
        if (index < 0 || _documentHost is null || _renderedModel.Blocks.Count <= index)
        {
            return;
        }

        var renderedNew = CreateRenderedBlocks(
            [_renderedModel.Blocks[index]], Markdown ?? string.Empty, _renderedModel.Blocks.Count);
        if (renderedNew.Count > 0)
        {
            ReplaceRenderedBlocks(index, index + 1, renderedNew, _renderedModel);
        }
    }

    bool Rendering.IMarkdownRenderContext.IsCodeBlockFolded(int blockKey) => IsCodeBlockFolded(blockKey);

    void Rendering.IMarkdownRenderContext.SetCodeBlockFolded(int blockKey, bool folded) =>
        SetCodeBlockFolded(blockKey, folded);

    // 代码块高亮行级缓存会话：键 = 块源码起始偏移。流式增长场景下同一代码块
    // 跨快照复用会话，未变行不重算；换文档时随折叠状态一起清理（见 RenderDocumentFull）。
    private readonly Dictionary<int, Rendering.MarkdownCodeHighlightSession> _codeHighlightSessions = [];

    // 当前顶层块解析输入坐标系的原点（见 IMarkdownRenderContext.CurrentBlockOrigin）。
    private int _currentBlockOrigin;

    int Rendering.IMarkdownRenderContext.CurrentBlockOrigin => _currentBlockOrigin;

    Rendering.MarkdownCodeHighlightSession? Rendering.IMarkdownRenderContext.GetCodeHighlightSession(int blockKey)
    {
        if (!_codeHighlightSessions.TryGetValue(blockKey, out var session))
        {
            session = new Rendering.MarkdownCodeHighlightSession();
            _codeHighlightSessions[blockKey] = session;
        }

        return session;
    }

    string? Rendering.IMarkdownRenderContext.MarkdownText => Markdown;

    string? Rendering.IMarkdownRenderContext.ImageBasePath => ImageBasePath;

    Func<string, string?>? Rendering.IMarkdownRenderContext.ImageSourceResolver => ImageSourceResolver;

    private sealed record RenderedBlock(
        int Start,
        int End,
        int PlainTextStart,
        int PlainTextEnd,
        string PlainText,
        MarkdownBlockKind Kind,
        MarkdownDependencyFlags DependencyFlags,
        ulong ContentHash,
        Control Control,
        List<IDisposable>? Bindings,
        bool HasRemoteImage,
        int TaskMarkerOffset,
        bool IsHeavy)
    {
        public static RenderedBlock FromModel(
            MarkdownDocumentBlock modelBlock,
            Control control,
            List<IDisposable>? bindings,
            bool hasRemoteImage = false,
            int taskMarkerOffset = -1)
        {
            return new RenderedBlock(
                modelBlock.SourceSpan.Start,
                modelBlock.SourceSpan.End,
                modelBlock.PlainTextSpan.Start,
                modelBlock.PlainTextSpan.End,
                modelBlock.PlainText,
                modelBlock.Kind,
                modelBlock.DependencyFlags,
                modelBlock.ContentHash,
                control,
                bindings,
                hasRemoteImage,
                taskMarkerOffset,
                modelBlock.IsHeavy);
        }

        public void Cleanup()
        {
            if (Control is DeferredMarkdownBlock deferred) deferred.Dispose();
            if (Bindings is not null)
            {
                foreach (var d in Bindings)
                {
                    d.Dispose();
                }
                Bindings.Clear();
            }
        }

        public RenderedBlock WithModel(MarkdownDocumentBlock modelBlock)
        {
            return this with
            {
                Start = modelBlock.SourceSpan.Start,
                End = modelBlock.SourceSpan.End,
                PlainTextStart = modelBlock.PlainTextSpan.Start,
                PlainTextEnd = modelBlock.PlainTextSpan.End,
                PlainText = modelBlock.PlainText,
                Kind = modelBlock.Kind,
                DependencyFlags = modelBlock.DependencyFlags,
                ContentHash = modelBlock.ContentHash,
                IsHeavy = modelBlock.IsHeavy
            };
        }

        public RenderedBlock Shift(int delta)
        {
            return this with
            {
                Start = Start + delta,
                End = End + delta
            };
        }
    }

    private readonly record struct TextRange(int Start, int End);

    private readonly record struct TextChange(int OldStart, int OldEnd, int NewStart, int NewEnd, int Delta);

    /// <summary>
    /// 追加块控件：宿主为虚拟化面板时按块序号登记，否则退回普通面板行为。
    /// </summary>
    private void AddBlockControl(Control control)
    {
        if (_virtualizingHost is { } host)
        {
            host.AddItem(control);
            return;
        }

        _documentHost?.Children.Add(control);
    }

    private void AddBlockControl(int index, Control control)
    {
        if (_virtualizingHost is { } host)
        {
            host.InsertItem(index, control);
            return;
        }

        _documentHost?.Children.Insert(index, control);
    }

    private void RemoveBlockControlAt(int index)
    {
        if (_virtualizingHost is { } host)
        {
            host.RemoveAt(index);
            return;
        }

        _documentHost?.Children.RemoveAt(index);
    }

    /// <summary>重块（代码块 / 表格 / 图片 / 公式 / 图表）由面板按保留预算决定去留，见 <see cref="RenderedBlock.IsHeavy"/>。</summary>
    private static bool IsAlwaysRealized(MarkdownDocumentBlock block) => block.IsHeavy;

    private void ReplaceBlockControl(int index, Control control)
    {
        RemoveBlockControlAt(index);
        AddBlockControl(index, control);
    }

    private void ClearBlockControls()
    {
        if (_virtualizingHost is { } host)
        {
            host.ClearItems();
            return;
        }

        _documentHost?.Children.Clear();
    }
}
