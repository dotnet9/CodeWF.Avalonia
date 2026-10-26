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
public sealed class MarkdownMindMapView : UserControl
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
            ? new Rect(rect.X * Zoom + _translate.X, rect.Y * Zoom + _translate.Y, rect.Width * Zoom, rect.Height * Zoom) : null;
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
        { _translate.X = Bounds.Width / 2 - rect.Center.X * Zoom; _translate.Y = Bounds.Height / 2 - rect.Center.Y * Zoom; _hasViewport = true; }
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
        else if (change.Property == AccentColorProperty || change.Property == EnableMotionProperty || change.Property == CanvasBrushProperty || change.Property == CardBrushProperty || change.Property == TextBrushProperty || change.Property == LineBrushProperty || change.Property == DotBrushProperty || change.Property == SuccessColorProperty || change.Property == WarningColorProperty) Rebuild();
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
        _dots.Brush = DotBrush ?? line; _dots.InvalidateVisual();
        if (Root is null) { VisibleNodesChanged?.Invoke(this, EventArgs.Empty); return; }
        double row = 54; _width = 0;
        if (geometry)
        {
            Place(Root, 0, AccentColor);
            ApplyLayoutKind();
            _height = Math.Max(90, _bounds.Values.Max(rect => rect.Bottom) + 20);
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
                a = new Point(rect.Right, rect.Center.Y); b = new Point(childRect.Left, childRect.Center.Y);
            }
            if (_bounds.Count >= 200 && !new Rect(new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), new Size(Math.Abs(b.X - a.X) + 1, Math.Abs(b.Y - a.Y) + 1)).Intersects(_realizedRegion)) continue;
            var data = LayoutKind is MindMapLayoutKind.Down or MindMapLayoutKind.Outline or MindMapLayoutKind.Radial or MindMapLayoutKind.Fishbone
                ? FormattableString.Invariant($"M {a.X},{a.Y} L {b.X},{b.Y}")
                : FormattableString.Invariant($"M {a.X},{a.Y} C {a.X + 40},{a.Y} {b.X - 40},{b.Y} {b.X},{b.Y}");
            // 原型连线配色：缤纷=分支色、黑白=前景、其余样式统一强调色系（polish.css .map-links）。
            // 参考软件样式：缤纷=Mermaid cScale 实色粗线（4px 实色，非半透明细线）。
            var linkColor = StyleKind switch
            {
                MindMapStyleKind.Colorful => BranchSolid(_branches[child], dark),
                MindMapStyleKind.Mono => dark ? Color.Parse("#E4E5E7") : Color.Parse("#30343B"),
                _ => AccentColor
            };
            var isColorfulLink = StyleKind == MindMapStyleKind.Colorful;
            _canvas.Children.Add(new global::Avalonia.Controls.Shapes.Path
            {
                Data = Geometry.Parse(data),
                Stroke = new SolidColorBrush(linkColor, isColorfulLink ? 1.0 : .42),
                StrokeThickness = isColorfulLink ? 3.5 : 1.5,
                IsHitTestVisible = false
            });
        }
        foreach (var (node, rect) in _bounds)
        {
            if (_bounds.Count >= 200 && !rect.Intersects(_realizedRegion)) continue;
            var root = ReferenceEquals(node, Root);
            var heading = node.Kind == MarkdownMindMapNodeKind.Heading;
            var color = _branches[node];
            // 原型 Quiet 卡片：标题左对齐垂直居中，级别/文档徽标在右缘（标题节点靠上、根节点居中）。
            var body = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), VerticalAlignment = VerticalAlignment.Center };
            IBrush? nodeForeground = StyleKind == MindMapStyleKind.Colorful && !root ? new SolidColorBrush(ContrastText(_branches[node])) : null;
            var badge = root ? MindViewLocalization.GetResource(MindViewL.MarkdownDocument) : heading ? $"H{node.HeadingLevel}" : null;
            body.Children.Add(new TextBlock
            {
                Text = node.Title, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = root ? 1 : 4,
                FontSize = root || heading ? 13 : 12, LineHeight = 20,
                FontWeight = root || heading ? FontWeight.SemiBold : FontWeight.Normal,
                Foreground = root ? MindViewThemeResources.AccentForeground(AccentColor)
                    : nodeForeground ?? foreground,
                VerticalAlignment = VerticalAlignment.Center
            });
            if (badge is not null)
            {
                var badgeText = new TextBlock
                {
                    Text = badge, FontSize = 9, LineHeight = 14,
                    Foreground = root ? MindViewThemeResources.AccentForeground(AccentColor) : foreground,
                    Opacity = .75, HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = root ? VerticalAlignment.Center : VerticalAlignment.Top,
                    Margin = new Thickness(6, 0, 0, 0)
                };
                Grid.SetColumn(badgeText, 1);
                body.Children.Add(badgeText);
            }
            var content = new Grid();
            content.Children.Add(new CardDecoration { IsSummary = !root && !heading, Stripe = !root && heading && StyleKind != MindMapStyleKind.Quiet ? new SolidColorBrush(color) : null, Line = line, Margin = new Thickness(-12, -10), IsHitTestVisible = false });
            content.Children.Add(body);
            var card = new Border
            {
                Width = rect.Width, Height = rect.Height, Padding = new Thickness(12, 10), CornerRadius = new CornerRadius(8),
                Background = root ? new SolidColorBrush(AccentColor) : background,
                BorderBrush = node == _selected ? new SolidColorBrush(AccentColor) : !root && !heading ? Brushes.Transparent : line,
                BorderThickness = new Thickness(node == _selected ? 2 : 1), Child = content,
                BoxShadow = BoxShadows.Parse(dark ? "0 3 12 0 #16000000" : "0 3 12 0 #07000000"),
                Cursor = new Cursor(StandardCursorType.Hand), Focusable = true
            };
            // 原型根节点：实底外描一圈留白再套强调色细环（双环），不用投影。
            if (root) card.BoxShadow = BoxShadows.Parse(FormattableString.Invariant(
                $"0 0 0 2 {(dark ? "#202124" : "#FFFFFF")}, 0 0 0 3.5 {AccentColor}"));
            // 样式层：清雅为默认；缤纷/线框/胶囊/脉络/黑白只调整卡片视觉（对应原型 map-appearance）。
            if (!root && StyleKind != MindMapStyleKind.Quiet)
            {
                switch (StyleKind)
                {
                    case MindMapStyleKind.Colorful:
                        // 参考软件：节点实色填充（深色态亮一档），文字按底色亮度取黑/白。
                        var solid = dark ? Brighten(color, 0.18) : color;
                        card.Background = new SolidColorBrush(solid);
                        card.BorderBrush = new SolidColorBrush(color);
                        break;
                    case MindMapStyleKind.Outline:
                        card.Background = Brushes.Transparent;
                        card.BorderBrush = new SolidColorBrush(color);
                        card.BoxShadow = BoxShadows.Parse("0 0 0 0 #00000000");
                        card.CornerRadius = new CornerRadius(6);
                        break;
                    case MindMapStyleKind.Capsule:
                        card.CornerRadius = new CornerRadius(rect.Height / 2);
                        card.Padding = new Thickness(16, 8);
                        break;
                    case MindMapStyleKind.Underline:
                        card.Background = Brushes.Transparent;
                        card.BorderBrush = new SolidColorBrush(color);
                        card.BorderThickness = new Thickness(0, 0, 0, 2.2);
                        card.CornerRadius = new CornerRadius(0);
                        card.BoxShadow = BoxShadows.Parse("0 0 0 0 #00000000");
                        break;
                    case MindMapStyleKind.Mono:
                        card.Background = Brush.Parse(dark ? "#2E3038" : "#EDEEF2");
                        card.BorderBrush = node == _selected ? Brush.Parse(dark ? "#C8CAD4" : "#5A5C66") : line;
                        break;
                }
            }
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
                { previousCard.BorderBrush = previous.Kind is MarkdownMindMapNodeKind.Heading or MarkdownMindMapNodeKind.Document ? line : Brushes.Transparent; previousCard.BorderThickness = new Thickness(1); }
                _selected = node; card.BorderBrush = new SolidColorBrush(AccentColor); card.BorderThickness = new Thickness(2);
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
                    targetCard.BorderBrush = candidate == dropTarget ? new SolidColorBrush(AccentColor) : line;
                    targetCard.BorderThickness = candidate == dropTarget
                        ? dropPosition == "before" ? new Thickness(0, 3, 0, 0) : dropPosition == "after" ? new Thickness(0, 0, 0, 3) : new Thickness(3)
                        : new Thickness(1);
                }
                e.Handled = true;
            };
            card.PointerReleased += (_, e) =>
            {
                var target = dropTarget; var moved = dragging;
                dragStart = null; dragging = false; dropTarget = null; e.Pointer.Capture(null);
                _draggingChapter = false;
                if (moved) { Rebuild(); if (target is not null) ChapterMoveRequested?.Invoke(this, new(node, target, dropPosition)); e.Handled = true; }
            };
            card.PointerCaptureLost += (_, _) => { dragStart = null; dragging = false; dropTarget = null; _draggingChapter = false; };
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
                maxWidth: root ? 188 : heading ? 196 : 198, lineHeight: 20);
            var height = (root || heading ? 40 : 22) + Math.Clamp(textLayout.TextLines.Count, 1, 4) * 20;
            var start = row;
            double y;
            if (node.IsCollapsed || node.Children.Count == 0) { y = row; row += height + 14; }
            else
            {
                var centers = new List<double>();
                for (var i = 0; i < node.Children.Count; i++)
                {
                    var childColor = root ? (i % 5) switch { 1 => SuccessColor, 2 => WarningColor, 4 => Color.Parse(dark ? "#9398B3" : "#8B91A8"), _ => AccentColor } : dark ? Brighten(branch, 0.18) : branch;
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

    /// <summary>Place 先按单侧树铺出尺寸，再按所选布局重排坐标（尺寸沿用测量结果）。</summary>
    private void ApplyLayoutKind()
    {
        if (Root is null || LayoutKind == MindMapLayoutKind.Right) return;
        if (LayoutKind == MindMapLayoutKind.Balanced) { BalancedBounds(Root); return; }
        if (LayoutKind == MindMapLayoutKind.Down) { var top = _bounds[Root].Top; var cursor = _bounds[Root].X; MeasureDownBounds(Root, ref cursor); PlaceDownBounds(Root, top); return; }
        if (LayoutKind == MindMapLayoutKind.Outline) { var cursor = _bounds.Values.Min(rect => rect.Top); OutlineBounds(Root, 0, ref cursor); return; }
        if (LayoutKind == MindMapLayoutKind.Radial) { RadialBounds(Root); return; }
        if (LayoutKind == MindMapLayoutKind.Fishbone) { FishboneBounds(Root); return; }
    }

    private double NodeHeight(MarkdownMindMapNode node) => _bounds.TryGetValue(node, out var rect) ? rect.Height : 40;
    private double NodeWidth(MarkdownMindMapNode node) => _bounds.TryGetValue(node, out var rect) ? rect.Width : 224;

    private double SubtreeHeightOf(MarkdownMindMapNode node)
    {
        var height = NodeHeight(node);
        if (node.IsCollapsed || node.Children.Count == 0) return height;
        return Math.Max(height, node.Children.Sum(SubtreeHeightOf) + 14 * (node.Children.Count - 1));
    }

    private void BalancedBounds(MarkdownMindMapNode root)
    {
        var rootRect = _bounds[root];
        var sides = new List<MarkdownMindMapNode>[2] { new(), new() };
        var loads = new double[2];
        foreach (var child in root.Children)
        {
            var side = loads[0] <= loads[1] ? 0 : 1;
            sides[side].Add(child);
            loads[side] += SubtreeHeightOf(child);
        }
        var centers = new List<double>();
        for (var side = 0; side < 2; side++)
        {
            if (sides[side].Count == 0) continue;
            var sign = side == 0 ? 1 : -1;
            var cursor = rootRect.Center.Y - loads[side] / 2;
            var top = cursor;
            foreach (var child in sides[side])
            {
                PlaceSide(child, 1, sign, rootRect.Right + 34, rootRect.Left - 34, ref cursor);
            }
            centers.Add((top + cursor - 14) / 2);
        }
        var center = centers.Count == 0 ? rootRect.Center.Y : centers.Average();
        ShiftTree(root, center - rootRect.Center.Y, 0);
    }

    private void PlaceSide(MarkdownMindMapNode node, int depth, int sign, double rightAnchor, double leftAnchor, ref double cursor)
    {
        var height = NodeHeight(node);
        var x = sign > 0 ? rightAnchor + (depth - 1) * 272 + 32 : leftAnchor - (depth - 1) * 272 - 32 - NodeWidth(node);
        if (node.IsCollapsed || node.Children.Count == 0)
        {
            _bounds[node] = new Rect(x, cursor, NodeWidth(node), height);
            cursor += height + 14;
            return;
        }
        foreach (var child in node.Children) PlaceSide(child, depth + 1, sign, rightAnchor, leftAnchor, ref cursor);
        var center = (SubtreeFirstCenter(node) + SubtreeLastCenter(node)) / 2;
        _bounds[node] = new Rect(x, center - height / 2, NodeWidth(node), height);
    }

    private double SubtreeFirstCenter(MarkdownMindMapNode node)
    {
        if (node.IsCollapsed || node.Children.Count == 0) return _bounds[node].Center.Y;
        return SubtreeFirstCenter(node.Children[0]);
    }

    private double SubtreeLastCenter(MarkdownMindMapNode node)
    {
        if (node.IsCollapsed || node.Children.Count == 0) return _bounds[node].Center.Y;
        return SubtreeLastCenter(node.Children[^1]);
    }

    private void ShiftTree(MarkdownMindMapNode node, double dy, double dx)
    {
        if (_bounds.TryGetValue(node, out var rect)) _bounds[node] = new Rect(rect.X + dx, rect.Y + dy, rect.Width, rect.Height);
        foreach (var child in node.Children) ShiftTree(child, dy, dx);
    }

    private double MeasureDownBounds(MarkdownMindMapNode node, ref double cursor)
    {
        var width = NodeWidth(node);
        if (node.IsCollapsed || node.Children.Count == 0)
        {
            var rect = _bounds[node];
            _bounds[node] = new Rect(cursor, rect.Y, width, rect.Height);
            cursor += width + 22;
            return _bounds[node].Center.X;
        }
        var centers = new List<double>();
        foreach (var child in node.Children) centers.Add(MeasureDownBounds(child, ref cursor));
        var center = (centers[0] + centers[^1]) / 2;
        var existing = _bounds[node];
        _bounds[node] = new Rect(center - width / 2, existing.Y, width, existing.Height);
        return center;
    }

    private void PlaceDownBounds(MarkdownMindMapNode node, double top)
    {
        var existing = _bounds[node];
        _bounds[node] = new Rect(existing.X, top, existing.Width, existing.Height);
        foreach (var child in node.Children) PlaceDownBounds(child, top + existing.Height + 40);
    }

    private void OutlineBounds(MarkdownMindMapNode node, int depth, ref double cursor)
    {
        var existing = _bounds[node];
        var indent = 32 + depth * 64;
        _bounds[node] = new Rect(indent, cursor, Math.Min(existing.Width, 340 - indent), existing.Height);
        cursor += existing.Height + 12;
        foreach (var child in node.Children) OutlineBounds(child, depth + 1, ref cursor);
    }

    private int SubtreeLeafCountOf(MarkdownMindMapNode node) =>
        node.IsCollapsed || node.Children.Count == 0 ? 1 : node.Children.Sum(SubtreeLeafCountOf);

    private void RadialBounds(MarkdownMindMapNode root)
    {
        var rootRect = _bounds[root];
        var centerX = rootRect.Center.X;
        var centerY = rootRect.Center.Y;
        var leaves = root.Children.Select(SubtreeLeafCountOf).ToList();
        var total = Math.Max(1, leaves.Sum());
        var angle = -Math.PI / 2;
        const double ring = 236;
        for (var i = 0; i < root.Children.Count; i++)
        {
            var span = Math.PI * 2 * leaves[i] / total;
            PlaceRadial(root.Children[i], 1, centerX, centerY, angle + span / 2, ring, leaves[i]);
            angle += span;
        }
        var maxRight = _bounds.Values.Max(rect => rect.Right);
        var minLeft = _bounds.Values.Min(rect => rect.Left);
        _width = Math.Max(_width, maxRight + 32);
        if (minLeft < 24) ShiftTree(root, 0, 24 - minLeft);
    }

    private void PlaceRadial(MarkdownMindMapNode node, int depth, double parentCenterX, double parentCenterY, double angle, double ring, int leaves)
    {
        var radius = depth * ring;
        var size = new Size(NodeWidth(node), NodeHeight(node));
        node = Bind(node, parentCenterX + Math.Cos(angle) * radius - size.Width / 2,
            parentCenterY + Math.Sin(angle) * radius * 0.74 - size.Height / 2, size);
        if (node.IsCollapsed || node.Children.Count == 0) return;
        var childLeaves = node.Children.Select(SubtreeLeafCountOf).ToList();
        var childTotal = Math.Max(1, childLeaves.Sum());
        var total = Math.Max(1, SubtreeLeafCountOf(Root!));
        var span = Math.Max(Math.PI * 2 * leaves / total, 0.24);
        var start = angle - span / 2;
        var accumulated = 0;
        for (var i = 0; i < node.Children.Count; i++)
        {
            var childAngle = start + span * (accumulated + childLeaves[i] / 2d) / childTotal;
            var nodeRect = _bounds[node];
            PlaceRadial(node.Children[i], depth + 1, nodeRect.Center.X, nodeRect.Center.Y, childAngle, ring, childLeaves[i]);
            accumulated += childLeaves[i];
        }
    }

    private MarkdownMindMapNode Bind(MarkdownMindMapNode node, double x, double y, Size size)
    {
        _bounds[node] = new Rect(x, y, size.Width, size.Height);
        return node;
    }

    private void FishboneBounds(MarkdownMindMapNode root)
    {
        var rootRect = _bounds[root];
        var spineY = rootRect.Center.Y;
        var branchX = rootRect.Right + 56;
        var index = 0;
        foreach (var branch in root.Children)
        {
            var direction = index % 2 == 0 ? -1 : 1;
            var branchRect = _bounds[branch];
            _bounds[branch] = new Rect(branchX, spineY + direction * 64 - branchRect.Height / 2, branchRect.Width, branchRect.Height);
            var childX = branchX;
            var cursor = spineY + direction * (64 + branchRect.Height / 2 + 18);
            foreach (var child in branch.Children)
            {
                var childRect = _bounds[child];
                Bind(child, childX, direction > 0 ? cursor : cursor - childRect.Height, childRect.Size);
                StackFishChildren(child, childX, ref cursor, direction);
                childX += childRect.Width + 30;
            }
            branchX += branchRect.Width + 30;
            index++;
        }
        var maxRight = _bounds.Values.Max(rect => rect.Right);
        var minTop = _bounds.Values.Min(rect => rect.Top);
        if (minTop < 24) ShiftTree(root, 24 - minTop, 0);
        _width = Math.Max(_width, maxRight + 32);
        _height = Math.Max(_height, _bounds.Values.Max(rect => rect.Bottom) + 20);
    }

    private void StackFishChildren(MarkdownMindMapNode node, double x, ref double cursor, int direction)
    {
        var local = cursor;
        foreach (var child in node.Children)
        {
            var rect = _bounds[child];
            Bind(child, x + 30, direction > 0 ? local : local - rect.Height, rect.Size);
            local += direction > 0 ? rect.Height + 12 : -(rect.Height + 12);
            StackFishChildren(child, x + 30, ref local, direction);
        }
        cursor = local;
    }

    private static IBrush Tint(Color accent, double targetRatio)
    {
        const int target = 255;
        return new SolidColorBrush(Color.FromRgb(
            (byte)(accent.R + (target - accent.R) * targetRatio),
            (byte)(accent.G + (target - accent.G) * targetRatio),
            (byte)(accent.B + (target - accent.B) * targetRatio)));
    }

    private sealed class DotGrid : Control
    {
        public IBrush Brush { get; set; } = Brushes.LightGray;
        public override void Render(DrawingContext context)
        {
            using var opacity = context.PushOpacity(.55);
            for (var y = 9d; y < Bounds.Height; y += 18)
            for (var x = 9d; x < Bounds.Width; x += 18) context.DrawEllipse(Brush, null, new Point(x, y), .8, .8);
        }
    }
    private sealed class CardDecoration : Control
    {
        public bool IsSummary { get; init; }
        public IBrush? Stripe { get; init; }
        public IBrush? Line { get; init; }
        public override void Render(DrawingContext context)
        {
            if (IsSummary) context.DrawRectangle(null, new Pen { Brush = Line, Thickness = 1, DashStyle = new DashStyle(new double[] { 4, 3 }, 0) }, new Rect(Bounds.Size).Deflate(.5), 8, 8);
            if (Stripe is not null) context.DrawRectangle(Stripe, null, new Rect(0, 8, 3, Math.Max(0, Bounds.Height - 16)), 1, 1);
        }
    }

    /// <summary>cScale 系列实色化：深色态向白提亮 18%（与参考软件 mindmap/timeline 观感一致）。</summary>
    private static Color BranchSolid(Color branch, bool dark) => dark ? Brighten(branch, 0.18) : branch;

    private static Color Brighten(Color color, double amount) => new(
        (byte)(color.R + (255 - color.R) * amount),
        (byte)(color.G + (255 - color.G) * amount),
        (byte)(color.B + (255 - color.B) * amount),
        color.A);

    /// <summary>按底色亮度取黑/白前景（WCAG 对比度）。</summary>
    private static Color ContrastText(Color background)
    {
        static double Luminance(Color c)
        {
            static double Channel(byte v) { var d = v / 255.0; return d <= 0.04045 ? d / 12.92 : System.Math.Pow((d + 0.055) / 1.055, 2.4); }
            return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
        }
        var l = Luminance(background);
        return (l + 0.05) / 0.05 > 0.05 / (l + 0.05) && l > 0.18 ? Colors.Black : Colors.White;
    }
}