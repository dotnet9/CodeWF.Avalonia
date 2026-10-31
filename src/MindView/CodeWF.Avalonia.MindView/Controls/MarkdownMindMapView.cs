using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Animation;
using CodeWF.Avalonia.MindView.Markdown;
using CodeWF.Avalonia.MindView.I18n;

namespace CodeWF.Avalonia.MindView.Controls;

/// <summary>只读文档导图。节点选择、折叠、平移与缩放不会修改 Markdown。</summary>
public sealed partial class MarkdownMindMapView : UserControl
{
    /// <summary>宿主处理章节编辑意图；控件本身不写入 Markdown。</summary>
    public bool IsChapterEditingEnabled { get; set; }
    public bool PreserveViewportOnRootChange { get; set; }
    public sealed record ChapterAction(MarkdownMindMapNode Node, string Action);
    public sealed record ChapterMove(MarkdownMindMapNode Node, MarkdownMindMapNode Target, string Position);
    public event EventHandler<ChapterAction>? ChapterActionRequested;
    public event EventHandler<ChapterMove>? ChapterMoveRequested;
    public Rect? GetNodeBounds(string id)
    {
        var node = _bounds.Keys.FirstOrDefault(node => node.Id == id);
        return node is not null && _bounds.TryGetValue(node, out var rect)
            ? new Rect(rect.X * Zoom + _translate.X, rect.Y * Zoom + _translate.Y, rect.Width * Zoom, RenderedHeight(node, rect) * Zoom) : null;
    }
    public static readonly StyledProperty<MarkdownMindMapNode?> RootProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, MarkdownMindMapNode?>(nameof(Root));
    public static readonly StyledProperty<Color> AccentColorProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, Color>(nameof(AccentColor), Color.Parse("#7457E8"));
    public static readonly StyledProperty<bool> EnableMotionProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, bool>(nameof(EnableMotion), true);
    public static readonly StyledProperty<MindMapLayoutKind> LayoutKindProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, MindMapLayoutKind>(nameof(LayoutKind), MindMapLayoutKind.Right);
    public static readonly StyledProperty<MindMapStyleKind> StyleKindProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, MindMapStyleKind>(nameof(StyleKind), MindMapStyleKind.Quiet);
    /// <summary>缩放下限。宿主给了拖动条（原型 .map-controls 下探到 1%）就放开这里，控件不再自己钉死档位。</summary>
    public static readonly StyledProperty<double> MinZoomProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, double>(nameof(MinZoom), .15);
    /// <summary>缩放上限，与 <see cref="MinZoomProperty"/> 同族。</summary>
    public static readonly StyledProperty<double> MaxZoomProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, double>(nameof(MaxZoom), 2.5);
    public static readonly StyledProperty<IBrush?> CanvasBrushProperty = AvaloniaProperty.Register<MarkdownMindMapView, IBrush?>(nameof(CanvasBrush));
    public static readonly StyledProperty<IBrush?> CardBrushProperty = AvaloniaProperty.Register<MarkdownMindMapView, IBrush?>(nameof(CardBrush));
    public static readonly StyledProperty<IBrush?> TextBrushProperty = AvaloniaProperty.Register<MarkdownMindMapView, IBrush?>(nameof(TextBrush));
    public static readonly StyledProperty<IBrush?> LineBrushProperty = AvaloniaProperty.Register<MarkdownMindMapView, IBrush?>(nameof(LineBrush));
    public static readonly StyledProperty<IBrush?> DotBrushProperty = AvaloniaProperty.Register<MarkdownMindMapView, IBrush?>(nameof(DotBrush));
    public static readonly StyledProperty<Color> SuccessColorProperty = AvaloniaProperty.Register<MarkdownMindMapView, Color>(nameof(SuccessColor), Color.Parse("#3BAA84"));
    public static readonly StyledProperty<Color> WarningColorProperty = AvaloniaProperty.Register<MarkdownMindMapView, Color>(nameof(WarningColor), Color.Parse("#CE9840"));
    public IBrush? CanvasBrush { get => GetValue(CanvasBrushProperty); set => SetValue(CanvasBrushProperty, value); }
    public IBrush? CardBrush { get => GetValue(CardBrushProperty); set => SetValue(CardBrushProperty, value); }
    public IBrush? TextBrush { get => GetValue(TextBrushProperty); set => SetValue(TextBrushProperty, value); }
    public IBrush? LineBrush { get => GetValue(LineBrushProperty); set => SetValue(LineBrushProperty, value); }
    public IBrush? DotBrush { get => GetValue(DotBrushProperty); set => SetValue(DotBrushProperty, value); }
    public Color SuccessColor { get => GetValue(SuccessColorProperty); set => SetValue(SuccessColorProperty, value); }
    public Color WarningColor { get => GetValue(WarningColorProperty); set => SetValue(WarningColorProperty, value); }
    private readonly DotGrid _dots = new() { IsHitTestVisible = false };
    private readonly Dictionary<MarkdownMindMapNode, Color> _branches = [];
    private readonly Canvas _canvas = new();
    private readonly Panel _viewport = new() { ClipToBounds = true };
    private readonly Dictionary<MarkdownMindMapNode, Rect> _bounds = [];
    private readonly TransformGroup _transform = new();
    private readonly ScaleTransform _scale = new(1, 1);
    private readonly TranslateTransform _translate = new();
    private Point? _pan;
    private Vector _panOffset;
    private double _height;
    private double _width;
    private MarkdownMindMapNode? _selected;
    private readonly Dictionary<MarkdownMindMapNode, Border> _cards = [];
    private bool _hasViewport;
    private double? _fitMinimum;
    private Size _lastViewportSize;
    private ViewportState? _pendingViewport;
    private Rect _realizedRegion;
    private bool _draggingChapter;
    /// <summary>实际创建的节点卡片数；完整的章节几何仍保留用于定位与移动。</summary>
    public int RealizedNodeCount => _cards.Count;

    public MarkdownMindMapNode? Root { get => GetValue(RootProperty); set => SetValue(RootProperty, value); }
    public Color AccentColor { get => GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
    public bool EnableMotion { get => GetValue(EnableMotionProperty); set => SetValue(EnableMotionProperty, value); }
    /// <summary>导图布局（右树/平衡/放射/向下/目录/鱼骨），只改几何不改结构。</summary>
    public MindMapLayoutKind LayoutKind { get => GetValue(LayoutKindProperty); set => SetValue(LayoutKindProperty, value); }
    /// <summary>导图样式（清雅/缤纷/线框/胶囊/脉络/黑白），只改节点视觉。</summary>
    public MindMapStyleKind StyleKind { get => GetValue(StyleKindProperty); set => SetValue(StyleKindProperty, value); }
    public double MinZoom { get => GetValue(MinZoomProperty); set => SetValue(MinZoomProperty, value); }
    public double MaxZoom { get => GetValue(MaxZoomProperty); set => SetValue(MaxZoomProperty, value); }
    public double Zoom => _scale.ScaleX;
    public MarkdownMindMapNode? SelectedNode => _selected;
    public event EventHandler<MarkdownMindMapNode>? NodeSelected;
    public event EventHandler<MarkdownMindMapNode>? SourceRequested;
    public event EventHandler? ZoomChanged;
    public event EventHandler? VisibleNodesChanged;
    public int VisibleNodeCount => _bounds.Count;
    public void ClearSelection() { _selected = null; Rebuild(); }
    /// <summary>同步宿主选区，不改变视口、不再次发送用户选择事件。</summary>
    public void SelectNode(string? id)
    {
        var selected = id is null ? null : Flatten(Root).FirstOrDefault(node => node.Id == id);
        if (ReferenceEquals(_selected, selected)) return;
        _selected = selected; Rebuild();
    }

    /// <summary>展开祖先、选择并居中节点。用于文档大纲导航。</summary>
    public bool RevealNode(string id)
    {
        var node = Flatten(Root).FirstOrDefault(n => n.Id == id);
        if (node is null) return false;
        Reveal(Root!); _selected = node; Rebuild(); _fitMinimum = null;
        if (_bounds.TryGetValue(node, out var rect))
        { _translate.X = Bounds.Width / 2 - rect.Center.X * Zoom; _translate.Y = Bounds.Height / 2 - (rect.Top + RenderedHeight(node, rect) / 2) * Zoom; _hasViewport = true; }
        Rebuild(false);
        NodeSelected?.Invoke(this, node);
        return true;
        bool Reveal(MarkdownMindMapNode current)
        {
            if (ReferenceEquals(current, node)) return true;
            if (!current.Children.Any(Reveal)) return false;
            current.IsCollapsed = false; return true;
        }
    }
    public readonly record struct ViewportState(double Zoom, double X, double Y, string? SelectedId, IReadOnlyList<string> CollapsedIds);
    public ViewportState CaptureViewport() => new(Zoom, _translate.X, _translate.Y, _selected?.Id,
        Flatten(Root).Where(n => n.IsCollapsed).Select(n => n.Id).ToArray());
    public void RestoreViewport(ViewportState state, bool onNextRoot = false)
    {
        if (onNextRoot) _pendingViewport = state;
        foreach (var node in Flatten(Root)) node.IsCollapsed = state.CollapsedIds.Contains(node.Id);
        _selected = Flatten(Root).FirstOrDefault(n => n.Id == state.SelectedId);
        Rebuild();
        _scale.ScaleX = _scale.ScaleY = Math.Clamp(state.Zoom, MinZoom, MaxZoom);
        _translate.X = state.X; _translate.Y = state.Y;
        _hasViewport = true;
        _fitMinimum = null;
        Rebuild(false);
        ZoomChanged?.Invoke(this, EventArgs.Empty);
        if (_selected is { } selected) NodeSelected?.Invoke(this, selected);
    }
    private static IEnumerable<MarkdownMindMapNode> Flatten(MarkdownMindMapNode? node)
    {
        if (node is null) yield break;
        yield return node;
        foreach (var child in node.Children) foreach (var descendant in Flatten(child)) yield return descendant;
    }

    public MarkdownMindMapView()
    {
        UseLayoutRounding = false;
        Focusable = true;
        _transform.Children.Add(_scale);
        _transform.Children.Add(_translate);
        _canvas.RenderTransform = _transform;
        _canvas.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        _viewport.Children.Add(_dots);
        _viewport.Children.Add(_canvas);
        Content = _viewport;
        SizeChanged += (_, _) =>
        {
            var next = Bounds.Size;
            if (_fitMinimum is { } minimum) Fit(minimum);
            else if (_hasViewport && _lastViewportSize.Width > 0 && _lastViewportSize.Height > 0 && next.Width > 0 && next.Height > 0)
            { _translate.X += (next.Width - _lastViewportSize.Width) / 2; _translate.Y += (next.Height - _lastViewportSize.Height) / 2; }
            _lastViewportSize = next;
            EnsureViewport();
            Rebuild(false);
        };
        ActualThemeVariantChanged += (_, _) => Rebuild();
        AttachedToVisualTree += (_, _) => { Rebuild(); EnsureViewport(); };
        _viewport.PointerPressed += (_, e) =>
        {
            if (e.Handled || !e.GetCurrentPoint(_viewport).Properties.IsLeftButtonPressed) return;
            _pan = e.GetPosition(_viewport); _panOffset = new(_translate.X, _translate.Y);
            e.Pointer.Capture(_viewport); e.Handled = true;
        };
        _viewport.PointerMoved += (_, e) =>
        {
            if (_pan is not { } start) return;
            _fitMinimum = null;
            var delta = e.GetPosition(_viewport) - start;
            _translate.X = _panOffset.X + delta.X; _translate.Y = _panOffset.Y + delta.Y;
            Rebuild(false);
        };
        _viewport.PointerReleased += (_, e) => { _pan = null; e.Pointer.Capture(null); };
        _viewport.PointerCaptureLost += (_, _) => _pan = null;
        _viewport.PointerWheelChanged += (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) SetZoom(Zoom * Math.Pow(1.12, e.Delta.Y), e.GetPosition(_viewport));
            else { _fitMinimum = null; _translate.X += e.Delta.X * 40; _translate.Y += e.Delta.Y * 40; }
            Rebuild(false);
            e.Handled = true;
        };
        KeyDown += (_, e) =>
        {
            if (IsChapterEditingEnabled && _selected is { } chapter && e.KeyModifiers == KeyModifiers.None)
            {
                var action = e.Key switch { Key.Enter => ReferenceEquals(chapter, Root) ? "child" : "sibling", Key.Tab => "child", Key.F2 => "rename", Key.Delete => "delete", _ => null };
                if (action is not null) { ChapterActionRequested?.Invoke(this, new(chapter, action)); e.Handled = true; return; }
            }
            if (e.Key == Key.Enter && _selected is { } node) { SourceRequested?.Invoke(this, node); e.Handled = true; }
            if (e.Key == Key.Space && _selected is { Children.Count: > 0 } selected)
            { ToggleCollapse(selected); e.Handled = true; }
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RootProperty)
        {
            var oldRoot = change.GetOldValue<MarkdownMindMapNode?>();
            var newRoot = Root;
            var sameDocument = oldRoot is not null && newRoot is not null && (PreserveViewportOnRootChange || oldRoot.Id == newRoot.Id && oldRoot.Title == newRoot.Title);
            var collapsed = Flatten(oldRoot).Where(n => n.IsCollapsed).Select(n => n.Id).ToHashSet();
            foreach (var node in Flatten(newRoot)) node.IsCollapsed = collapsed.Contains(node.Id);
            _selected = Flatten(newRoot).FirstOrDefault(n => n.Id == _selected?.Id);
            Rebuild();
            if (!sameDocument) { _hasViewport = false; EnsureViewport(); }
            if (newRoot is not null && _pendingViewport is { } pending) { _pendingViewport = null; RestoreViewport(pending); }
        }
        else if (change.Property == LayoutKindProperty || change.Property == StyleKindProperty)
        {
            Rebuild();
        }
        else if (change.Property == AccentColorProperty || change.Property == EnableMotionProperty || change.Property == CanvasBrushProperty || change.Property == CardBrushProperty || change.Property == PanelBrushProperty || change.Property == MutedTextBrushProperty || change.Property == TextBrushProperty || change.Property == LineBrushProperty || change.Property == DotBrushProperty || change.Property == SuccessColorProperty || change.Property == WarningColorProperty) Rebuild();
    }

    public void ToggleCollapse(MarkdownMindMapNode node) { node.IsCollapsed = !node.IsCollapsed; Rebuild(); }
    public void ExpandAll()
    {
        if (Root is { } root) Expand(root);
        Rebuild();
        static void Expand(MarkdownMindMapNode n) { n.IsCollapsed = false; foreach (var child in n.Children) Expand(child); }
    }
    public void CollapseAll()
    {
        foreach (var node in Flatten(Root).Where(n => !ReferenceEquals(n, Root))) node.IsCollapsed = node.Children.Count > 0;
        Rebuild();
    }
    public void ZoomIn() => SetZoom(Zoom * 1.15);
    public void ZoomOut() => SetZoom(Zoom / 1.15);
    public void ResetZoom() => SetZoom(1);
    public void SetZoom(double zoom, Point? anchor = null)
    {
        _fitMinimum = null;
        var point = anchor ?? new Point(Bounds.Width / 2, Bounds.Height / 2);
        zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        var ratio = zoom / Zoom;
        _translate.X = point.X - (point.X - _translate.X) * ratio;
        _translate.Y = point.Y - (point.Y - _translate.Y) * ratio;
        _scale.ScaleX = _scale.ScaleY = zoom;
        Rebuild(false);
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }
    public void Fit(double? minimum = null)
    {
        if (_width <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        var floor = minimum ?? MinZoom;
        var scale = Math.Clamp(Math.Min((Bounds.Width - 64) / _width, (Bounds.Height - 84) / _height), floor, 1);
        _scale.ScaleX = _scale.ScaleY = scale;
        _translate.X = Math.Max(floor >= .75 ? 24 : 0, (Bounds.Width - _width * scale) / 2);
        _translate.Y = (Bounds.Height - _height * scale) / 2;
        if (floor >= .75 && LayoutKind == MindMapLayoutKind.Right && Root is { } root && _bounds.TryGetValue(root, out var rootRect))
        {
            _translate.X = 32 - rootRect.X * scale;
            _translate.Y = Bounds.Height / 2 - rootRect.Center.Y * scale;
        }
        _hasViewport = true;
        _fitMinimum = minimum;
        Rebuild(false);
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }
    public void EnsureViewport() { if (!_hasViewport && IsEffectivelyVisible) Fit(.75); }

    private void Rebuild(bool geometry = true)
    {
        if (_canvas is null) return;
        var viewport = new Rect(-_translate.X / Zoom, -_translate.Y / Zoom, Math.Max(0, Bounds.Width / Zoom), Math.Max(0, Bounds.Height / Zoom));
        if (!geometry && (_bounds.Count < 200 || _draggingChapter || _realizedRegion.Contains(viewport))) return;
        var buffer = 160 / Zoom;
        _realizedRegion = new Rect(viewport.X - buffer, viewport.Y - buffer, viewport.Width + buffer * 2, viewport.Height + buffer * 2);
        _canvas.Children.Clear(); _cards.Clear();
        if (geometry) { _bounds.Clear(); _branches.Clear(); }
        var dark = MindViewThemeResources.IsDarkVariant(ActualThemeVariant);
        var line = LineBrush ?? Brush.Parse(dark ? "#3A3D50" : "#E3E5F0");
        var background = CardBrush ?? Brush.Parse(dark ? "#242633" : "#FFFFFF");
        var foreground = TextBrush ?? Brush.Parse(dark ? "#E8E9F2" : "#323445");
        _viewport.Background = CanvasBrush ?? Brush.Parse(dark ? "#171821" : "#F7F8FC");
        _dots.Brush = DotBrush ?? new SolidColorBrush(AccentColor, .18); _dots.InvalidateVisual();
        if (Root is null) { VisibleNodesChanged?.Invoke(this, EventArgs.Empty); return; }
        double row = 54; _width = 0;
        if (geometry)
        {
            Place(Root, 0, AccentColor);
            ApplyLayoutKind();
            _width = _bounds.Values.Max(rect => rect.Right) + 40;
            _height = Math.Max(90, _bounds.Values.Max(rect => rect.Bottom) + 48);
        }
        foreach (var (node, rect) in _bounds)
        foreach (var child in node.Children)
        {
            if (!_bounds.TryGetValue(child, out var childRect)) continue;
            Point a, b;
            if (LayoutKind is MindMapLayoutKind.Down or MindMapLayoutKind.Outline or MindMapLayoutKind.Radial or MindMapLayoutKind.Fishbone)
            {
                a = new Point(rect.Center.X, rect.Bottom);
                b = new Point(childRect.Center.X, childRect.Top);
                if (childRect.Center.Y < rect.Center.Y) { a = new Point(rect.Center.X, rect.Top); b = new Point(childRect.Center.X, childRect.Bottom); }
            }
            else
            {
                var right = childRect.Center.X >= rect.Center.X;
                a = new Point(right ? rect.Right : rect.Left, rect.Center.Y);
                b = new Point(right ? childRect.Left : childRect.Right, childRect.Center.Y);
            }
            if (_bounds.Count >= 200 && !new Rect(new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), new Size(Math.Abs(b.X - a.X) + 1, Math.Abs(b.Y - a.Y) + 1)).Intersects(_realizedRegion)) continue;
            var data = LayoutKind is MindMapLayoutKind.Down or MindMapLayoutKind.Outline or MindMapLayoutKind.Radial or MindMapLayoutKind.Fishbone
                ? FormattableString.Invariant($"M {a.X},{a.Y} L {b.X},{b.Y}")
                : FormattableString.Invariant($"M {a.X},{a.Y} C {(a.X + b.X) / 2},{a.Y} {(a.X + b.X) / 2},{b.Y} {b.X},{b.Y}");
            var linkColor = StyleKind switch
            {
                MindMapStyleKind.Colorful => _branches[child],
                MindMapStyleKind.Mono => (MutedTextBrush as ISolidColorBrush)?.Color ?? (dark ? Color.Parse("#A4ABAE") : Color.Parse("#65716B")),
                _ => (Mix(AccentColor, line, .58, dark) as ISolidColorBrush)!.Color
            };
            _canvas.Children.Add(new global::Avalonia.Controls.Shapes.Path
            {
                Data = Geometry.Parse(data),
                Stroke = new SolidColorBrush(linkColor, StyleKind == MindMapStyleKind.Colorful ? .85 : StyleKind == MindMapStyleKind.Mono ? .72 : .5),
                StrokeThickness = StyleKind == MindMapStyleKind.Colorful ? 2 : 1.7,
                IsHitTestVisible = false
            });
        }
        foreach (var (node, rect) in _bounds)
        {
            if (_bounds.Count >= 200 && !rect.Intersects(_realizedRegion)) continue;
            var root = ReferenceEquals(node, Root);
            var heading = node.Kind == MarkdownMindMapNodeKind.Heading;
            var card = CreateNodeCard(node, rect, dark, line, background, foreground);
            if (EnableMotion) card.Transitions = new Transitions { new BrushTransition { Property = Border.BorderBrushProperty, Duration = TimeSpan.FromMilliseconds(160) } };
            ToolTip.SetTip(card, node.FullText);
            Canvas.SetLeft(card, rect.X); Canvas.SetTop(card, rect.Y);
            Point? dragStart = null;
            bool dragging = false;
            MarkdownMindMapNode? dropTarget = null;
            string dropPosition = "child";
            card.PointerPressed += (_, e) =>
            {
                if (!e.GetCurrentPoint(card).Properties.IsLeftButtonPressed) return;
                if (_selected is { } previous && _cards.TryGetValue(previous, out var previousCard))
                { _selected = null; ApplyCardAppearance(previous, previousCard, dark, line, background); }
                _selected = node; ApplyCardAppearance(node, card, dark, line, background);
                Focus(); NodeSelected?.Invoke(this, node);
                if (e.ClickCount >= 2)
                {
                    if (IsChapterEditingEnabled && heading) ChapterActionRequested?.Invoke(this, new(node, "rename"));
                    else SourceRequested?.Invoke(this, node);
                }
                else if (IsChapterEditingEnabled && heading && !root) { dragStart = e.GetPosition(_viewport); e.Pointer.Capture(card); }
                e.Handled = true;
            };
            card.PointerMoved += (_, e) =>
            {
                if (dragStart is not { } start) return;
                var point = e.GetPosition(_viewport);
                var distance = point - start;
                if (!dragging && Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y) < 6) return;
                dragging = true; dropTarget = null;
                _draggingChapter = true;
                foreach (var (candidate, targetRect) in _bounds)
                {
                    if (candidate == node || candidate.Kind is not (MarkdownMindMapNodeKind.Document or MarkdownMindMapNodeKind.Heading)) continue;
                    var local = new Point((point.X - _translate.X) / Zoom, (point.Y - _translate.Y) / Zoom);
                    if (!targetRect.Contains(local)) continue;
                    dropTarget = candidate;
                    var ratio = (local.Y - targetRect.Top) / targetRect.Height;
                    dropPosition = ReferenceEquals(candidate, Root) ? "child" : ratio < .25 ? "before" : ratio > .75 ? "after" : "child";
                    break;
                }
                foreach (var (candidate, targetCard) in _cards)
                {
                    ApplyCardAppearance(candidate, targetCard, dark, line, background);
                    if (candidate == dropTarget)
                    {
                        targetCard.BorderBrush = new SolidColorBrush(AccentColor);
                        targetCard.BorderThickness = dropPosition == "before" ? new Thickness(0, 3, 0, 0) : dropPosition == "after" ? new Thickness(0, 0, 0, 3) : new Thickness(3);
                    }
                }
                e.Handled = true;
            };
            card.PointerReleased += (_, e) =>
            {
                var target = dropTarget; var moved = dragging;
                _draggingChapter = false;
                dragStart = null; dragging = false; dropTarget = null; e.Pointer.Capture(null);
                if (moved) { Rebuild(); if (target is not null) ChapterMoveRequested?.Invoke(this, new(node, target, dropPosition)); e.Handled = true; }
            };
            card.PointerCaptureLost += (_, _) =>
            {
                var cancelled = dragging;
                dragStart = null; dragging = false; dropTarget = null; _draggingChapter = false;
                if (cancelled) Rebuild();
            };
            _cards[node] = card; _canvas.Children.Add(card);
            if (node.Children.Count > 0)
            {
                var fold = new Button
                {
                    Content = node.IsCollapsed ? $"+{node.Children.Count}" : "−", Width = 18, Height = 18, MinWidth = 0, MinHeight = 0,
                    Padding = new Thickness(0), FontSize = 10, CornerRadius = new CornerRadius(11), Background = background,
                    Foreground = foreground, BorderBrush = line, BorderThickness = new Thickness(1)
                };
                ToolTip.SetTip(fold, MindViewLocalization.GetResource(node.IsCollapsed ? MindViewL.ExpandBranch : MindViewL.CollapseBranch));
                Canvas.SetLeft(fold, rect.Right - 9); Canvas.SetTop(fold, rect.Center.Y - 9);
                fold.Click += (_, e) => { ToggleCollapse(node); e.Handled = true; };
                _canvas.Children.Add(fold);
            }
        }
        if (geometry) VisibleNodesChanged?.Invoke(this, EventArgs.Empty);

        double Place(MarkdownMindMapNode node, int depth, Color branch)
        {
            var root = ReferenceEquals(node, Root);
            var heading = node.Kind == MarkdownMindMapNodeKind.Heading;
            using var textLayout = new global::Avalonia.Media.TextFormatting.TextLayout(node.Title,
                new Typeface(FontFamily, FontStyle.Normal, root || heading ? FontWeight.SemiBold : FontWeight.Normal),
                root || heading ? 13 : 12, TextBrush ?? Brushes.Black, textWrapping: TextWrapping.Wrap,
                maxWidth: root ? 156 : heading ? 166 : 198, lineHeight: 20);
            var height = 22 + Math.Clamp(textLayout.TextLines.Count, 1, 4) * 20;
            var start = row;
            double y;
            if (node.IsCollapsed || node.Children.Count == 0) { y = row; row += height + 14; }
            else
            {
                var centers = new List<double>();
                for (var i = 0; i < node.Children.Count; i++)
                {
                    // 根层分支取色：缤纷档优先吃宿主覆写的 DefaultPalette 色表（与编辑器 NextColor、
                    // 图表 series 同源，解决缤纷偏素）；其余档与未覆写宿主维持强调色家族五档不变。
                    var childColor = root
                        ? StyleKind == MindMapStyleKind.Colorful ? BranchPalette()[i % BranchPalette().Length] : LegacyRootChildColor(i, dark)
                        : branch;
                    centers.Add(Place(node.Children[i], depth + 1, childColor));
                }
                var subtreeHeight = row - start - 14;
                if (subtreeHeight < height) row += height - subtreeHeight;
                y = Math.Max(start, (centers[0] + centers[^1]) / 2 - height / 2);
            }
            var rect = new Rect(depth * 272 + 32, y, root ? 214 : 224, height);
            _bounds[node] = rect; _branches[node] = branch; _width = Math.Max(_width, rect.Right + 32);
            return rect.Center.Y;
        }
    }

    /// <summary>缤纷档根层色表：读宿主覆写的 <see cref="MindViewStyleKeys.DefaultPaletteResource"/>
    /// （分号分隔的十六进制色串），缺失或不可解析时回退强调色家族五档，保持既有观感。</summary>
    private Color[] BranchPalette()
    {
        var paletteText = MindViewThemeResources.GetString(this, MindViewStyleKeys.DefaultPaletteResource, string.Empty);
        var colors = new List<Color>();
        foreach (var value in paletteText.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try { colors.Add(Color.Parse(value)); } catch { /* 单个坏值跳过，不影响其余颜色。 */ }
        }
        return colors.Count > 0 ? [.. colors] : [LegacyRootChildColor(0, false), LegacyRootChildColor(1, false), LegacyRootChildColor(2, false), LegacyRootChildColor(3, false), LegacyRootChildColor(4, false)];
    }

    private Color LegacyRootChildColor(int index, bool dark) => (index % 5) switch
    {
        1 => SuccessColor,
        2 => WarningColor,
        4 => Color.Parse(dark ? "#9398B3" : "#8B91A8"),
        _ => AccentColor
    };

    private sealed class DotGrid : Control
    {
        public IBrush Brush { get; set; } = Brushes.LightGray;
        public override void Render(DrawingContext context)
        {
            for (var y = 11d; y < Bounds.Height; y += 22)
            for (var x = 11d; x < Bounds.Width; x += 22) context.DrawEllipse(Brush, null, new Point(x, y), .65, .65);
        }
    }
    private sealed class CardDecoration : Control
    {
        public bool IsSummary { get; init; }
        public IBrush? Line { get; init; }
        public override void Render(DrawingContext context)
        {
            if (IsSummary) context.DrawRectangle(null, new Pen { Brush = Line, Thickness = 1, DashStyle = new DashStyle(new double[] { 4, 3 }, 0) }, new Rect(Bounds.Size).Deflate(.5), 8, 8);
        }
    }

}
