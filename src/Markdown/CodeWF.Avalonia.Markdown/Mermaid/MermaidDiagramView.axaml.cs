using System.ComponentModel;
using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;
using Markdig.Syntax;

namespace CodeWF.Avalonia.Markdown.Mermaid;

internal partial class MermaidDiagramView : UserControl
{
    private MermaidDiagramViewModel _state;
    private readonly MarkdownViewer _viewer;
    private readonly DispatcherTimer _previewTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    private TopLevel? _topLevel;
    private readonly string? _imageBasePath;
    private bool _waitingForModel;

    public MermaidDiagramView(MarkdownViewer viewer, MermaidDiagramViewModel state, Control preview, IMarkdownRenderContext context)
    {
        InitializeComponent();
        _viewer = viewer;
        _imageBasePath = viewer.ImageBasePath;
        _state = state;
        DataContext = state;
        PreviewContent.Content = preview;
        context.BindTheme(SourceInput, TextBox.ForegroundProperty, MarkdownViewer.ForegroundProperty);
        context.BindTheme(SourceInput, TextBox.BackgroundProperty, MarkdownViewer.CodeBackgroundBrushProperty);
        context.BindTheme(SourceInput, TextBox.BorderBrushProperty, MarkdownViewer.BorderLineBrushProperty);
        context.BindTheme(SourceInput, TextBox.FontFamilyProperty, MarkdownViewer.CodeFontFamilyProperty);
        context.BindTheme(SourceBadge, Button.BackgroundProperty, MarkdownViewer.BackgroundProperty);
        context.BindTheme(SourceBadge, Button.BorderBrushProperty, MarkdownViewer.BorderLineBrushProperty);
        context.BindTheme(SourceBadge, Button.ForegroundProperty, MarkdownViewer.ForegroundProperty);
        PreviewButton.Click += (_, args) => { BeginEdit(); args.Handled = true; };
        SourceBadge.Click += (_, args) => { BeginEdit(); args.Handled = true; };
        PreviewButton.AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
        SourceBadge.AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
        SourceInput.KeyDown += (_, args) =>
        {
            if (args.Key != Key.Escape && !(args.Key == Key.Enter && args.KeyModifiers.HasFlag(KeyModifiers.Control))) return;
            Commit(); args.Handled = true; PreviewButton.Focus();
        };
        SourceInput.LostFocus += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            if (!IsKeyboardFocusWithin) Commit();
        });
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            if (_state.IsEditing) PreviewContent.Content = new MarkdownViewer
            {
                Markdown = _state.PreviewMarkdown,
                TypographyTheme = _viewer.TypographyTheme,
                TypographySize = _viewer.TypographySize,
            };
        };
        _state.PropertyChanged += OnStateChanged;
        DetachedFromVisualTree += (_, _) => Commit();
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (!args.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        // 上一张源码收起后按钮会移位，不能等原坐标上的 MouseUp 再激活。
        args.Handled = true;
        BeginEdit();
    }

    private void BeginEdit()
    {
        if (_state.IsEditing) { SourceInput.Focus(); return; }
        // 点另一张图会先提交上一张；等异步正文模型落地后继续这次点击。
        if (_viewer.CurrentModel.Source != _viewer.Markdown)
        {
            if (_waitingForModel) return;
            _waitingForModel = true;
            _viewer.LayoutUpdated += OnModelLayoutUpdated;
            WatchOutsidePointer();
            return;
        }
        StopWaitingForModel();
        if (ResolveState() is not { } state) return;
        _state.PropertyChanged -= OnStateChanged;
        DataContext = _state = state;
        _state.PropertyChanged += OnStateChanged;
        _state.IsEditing = true;
        _viewer.PropertyChanged += OnViewerChanged;
        WatchOutsidePointer();
        SourceInput.Focus();
    }

    private void WatchOutsidePointer()
    {
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.RemoveHandler(PointerPressedEvent, OnOutsidePointerPressed);
        _topLevel?.AddHandler(PointerPressedEvent, OnOutsidePointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void OnModelLayoutUpdated(object? sender, EventArgs args)
    {
        if (_viewer.CurrentModel.Source == _viewer.Markdown) BeginEdit();
    }

    private void StopWaitingForModel()
    {
        _waitingForModel = false;
        _viewer.LayoutUpdated -= OnModelLayoutUpdated;
        _topLevel?.RemoveHandler(PointerPressedEvent, OnOutsidePointerPressed);
        _topLevel = null;
    }

    private MermaidDiagramViewModel? ResolveState()
    {
        if (_viewer.CurrentModel.Source != _viewer.Markdown) return null;
        var host = this.GetVisualAncestors().OfType<WindowedStackPanel>().FirstOrDefault();
        if (host is null) return null;
        var ancestors = this.GetVisualAncestors().ToHashSet();
        var blocks = _viewer.CurrentModel.Blocks;
        for (var index = 0; index < Math.Min(blocks.Count, host.ItemCount); index++)
        {
            if (host.GetRealizedItem(index) is not { } realized || !ancestors.Contains(realized)) continue;
            var syntax = blocks[index].SyntaxBlock;
            var fences = syntax is FencedCodeBlock fenced ? new[] { fenced }
                : syntax.Descendants<FencedCodeBlock>().ToArray();
            var ordinal = realized.GetVisualDescendants().OfType<MermaidDiagramView>()
                .Where(view => ReferenceEquals(view._viewer, _viewer)).TakeWhile(view => !ReferenceEquals(view, this)).Count();
            var target = fences.Where(fence => string.Equals(fence.Info?.Trim(), "mermaid", StringComparison.OrdinalIgnoreCase))
                .ElementAtOrDefault(ordinal);
            return target is null ? null : new(_viewer.Markdown!, target, Math.Max(0, blocks[index].SourceSpan.Start - syntax.Span.Start));
        }
        return null;
    }

    private void OnViewerChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == MarkdownViewer.MarkdownProperty || args.Property == MarkdownViewer.ImageBasePathProperty) Commit();
    }

    private void OnOutsidePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        // 从顶层监听，工具条/侧栏以及已经处理过指针的控件也能结束图表编辑。
        if (args.Source is Visual visual && (ReferenceEquals(visual, this) || visual.GetVisualAncestors().Contains(this))) return;
        Commit();
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(MermaidDiagramViewModel.Source)) return;
        _previewTimer.Stop();
        if (_state.IsEditing) _previewTimer.Start();
    }

    internal void Commit()
    {
        if (_waitingForModel) StopWaitingForModel();
        if (!_state.IsEditing) return;
        _state.IsEditing = false;
        _viewer.PropertyChanged -= OnViewerChanged;
        _previewTimer.Stop();
        _topLevel?.RemoveHandler(PointerPressedEvent, OnOutsidePointerPressed);
        _topLevel = null;
        if (_viewer.Markdown == _state.OriginalMarkdown && _viewer.ImageBasePath == _imageBasePath && _state.CreateEdit() is { } edit)
            _viewer.RaiseEvent(edit);
    }
}
