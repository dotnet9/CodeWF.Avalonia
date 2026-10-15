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
    public static readonly StyledProperty<MarkdownMindMapNode?> RootProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, MarkdownMindMapNode?>(nameof(Root));
    public static readonly StyledProperty<Color> AccentColorProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, Color>(nameof(AccentColor), Color.Parse("#7457E8"));
    public static readonly StyledProperty<bool> EnableMotionProperty =
        AvaloniaProperty.Register<MarkdownMindMapView, bool>(nameof(EnableMotion), true);
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

    public MarkdownMindMapNode? Root { get => GetValue(RootProperty); set => SetValue(RootProperty, value); }
    public Color AccentColor { get => GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
    public bool EnableMotion { get => GetValue(EnableMotionProperty); set => SetValue(EnableMotionProperty, value); }
    public double Zoom => _scale.ScaleX;
    public MarkdownMindMapNode? SelectedNode => _selected;
    public event EventHandler<MarkdownMindMapNode>? NodeSelected;
    public event EventHandler<MarkdownMindMapNode>? SourceRequested;
    public event EventHandler? ZoomChanged;
    public event EventHandler? VisibleNodesChanged;
    public int VisibleNodeCount => _bounds.Count;
    public void ClearSelection() { _selected = null; Rebuild(); }

    /// <summary>展开祖先、选择并居中节点。用于文档大纲导航。</summary>
    public bool RevealNode(string id)
    {
        var node = Flatten(Root).FirstOrDefault(n => n.Id == id);
        if (node is null) return false;
        Reveal(Root!); _selected = node; Rebuild(); _fitMinimum = null;
        if (_bounds.TryGetValue(node, out var rect))
        { _translate.X = Bounds.Width / 2 - rect.Center.X * Zoom; _translate.Y = Bounds.Height / 2 - rect.Center.Y * Zoom; _hasViewport = true; }
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
        _scale.ScaleX = _scale.ScaleY = Math.Clamp(state.Zoom, .15, 2.5);
        _translate.X = state.X; _translate.Y = state.Y;
        _hasViewport = true;
        _fitMinimum = null;
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
        };
        ActualThemeVariantChanged += (_, _) => Rebuild();
        AttachedToVisualTree += (_, _) => { Rebuild(); Fit(minimum: .75); };
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
        };
        _viewport.PointerReleased += (_, e) => { _pan = null; e.Pointer.Capture(null); };
        _viewport.PointerCaptureLost += (_, _) => _pan = null;
        _viewport.PointerWheelChanged += (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) SetZoom(Zoom * Math.Pow(1.12, e.Delta.Y), e.GetPosition(_viewport));
            else { _fitMinimum = null; _translate.X += e.Delta.X * 40; _translate.Y += e.Delta.Y * 40; }
            e.Handled = true;
        };
        KeyDown += (_, e) =>
        {
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
            var sameDocument = oldRoot is not null && newRoot is not null && oldRoot.Id == newRoot.Id && oldRoot.Title == newRoot.Title;
            var collapsed = Flatten(oldRoot).Where(n => n.IsCollapsed).Select(n => n.Id).ToHashSet();
            foreach (var node in Flatten(newRoot)) node.IsCollapsed = collapsed.Contains(node.Id);
            _selected = Flatten(newRoot).FirstOrDefault(n => n.Id == _selected?.Id);
            Rebuild();
            if (!sameDocument) { _hasViewport = false; EnsureViewport(); }
            if (_pendingViewport is { } pending) { _pendingViewport = null; RestoreViewport(pending); }
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
        zoom = Math.Clamp(zoom, .15, 2.5);
        var ratio = zoom / Zoom;
        _translate.X = point.X - (point.X - _translate.X) * ratio;
        _translate.Y = point.Y - (point.Y - _translate.Y) * ratio;
        _scale.ScaleX = _scale.ScaleY = zoom;
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }
    public void Fit(double minimum = .15)
    {
        if (_width <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;
        var scale = Math.Clamp(Math.Min((Bounds.Width - 80) / _width, (Bounds.Height - 80) / _height), minimum, 1);
        _scale.ScaleX = _scale.ScaleY = scale;
        _translate.X = Math.Max(40, (Bounds.Width - _width * scale) / 2);
        _translate.Y = Math.Max(40, (Bounds.Height - _height * scale) / 2);
        _hasViewport = true;
        _fitMinimum = minimum;
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }
    public void EnsureViewport() { if (!_hasViewport && IsEffectivelyVisible) Fit(.75); }

    private void Rebuild()
    {
        if (_canvas is null) return;
        _canvas.Children.Clear(); _bounds.Clear(); _cards.Clear(); _branches.Clear();
        var dark = ActualThemeVariant == ThemeVariant.Dark || ActualThemeVariant?.InheritVariant == ThemeVariant.Dark;
        var line = LineBrush ?? Brush.Parse(dark ? "#3A3D50" : "#E3E5F0");
        var background = CardBrush ?? Brush.Parse(dark ? "#242633" : "#FFFFFF");
        var foreground = TextBrush ?? Brush.Parse(dark ? "#E8E9F2" : "#323445");
        _viewport.Background = CanvasBrush ?? Brush.Parse(dark ? "#171821" : "#F7F8FC");
        _dots.Brush = DotBrush ?? line; _dots.InvalidateVisual();
        if (Root is null) { VisibleNodesChanged?.Invoke(this, EventArgs.Empty); return; }
        double row = 0; _width = 0;
        Place(Root, 0, AccentColor);
        _height = Math.Max(90, row - 18);
        foreach (var (node, rect) in _bounds)
        foreach (var child in node.Children)
        {
            if (!_bounds.TryGetValue(child, out var childRect)) continue;
            var a = new Point(rect.Right, rect.Center.Y); var b = new Point(childRect.Left, childRect.Center.Y);
            _canvas.Children.Add(new global::Avalonia.Controls.Shapes.Path
            {
                Data = Geometry.Parse(FormattableString.Invariant($"M {a.X},{a.Y} C {a.X + 40},{a.Y} {b.X - 40},{b.Y} {b.X},{b.Y}")),
                Stroke = new SolidColorBrush(_branches[child], dark ? .6 : .38), StrokeThickness = 1.5, IsHitTestVisible = false
            });
        }
        foreach (var (node, rect) in _bounds)
        {
            var root = ReferenceEquals(node, Root);
            var heading = node.Kind == MarkdownMindMapNodeKind.Heading;
            var color = _branches[node];
            var body = new StackPanel { Spacing = 5, VerticalAlignment = VerticalAlignment.Center };
            if (root) body.Children.Add(new TextBlock { Text = MindViewLocalization.GetResource(MindViewL.MarkdownDocument), FontSize = 9, Foreground = MindViewThemeResources.AccentForeground(AccentColor), Opacity = .75 });
            if (!root && heading) body.Children.Add(new TextBlock { Text = $"H{node.HeadingLevel}", FontSize = 9, Foreground = new SolidColorBrush(color) });
            body.Children.Add(new TextBlock
            {
                Text = node.Title, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.CharacterEllipsis, MaxLines = root ? 1 : 4,
                FontSize = root ? 14 : heading ? 12 : 11.5, LineHeight = root ? 20 : 18,
                FontWeight = root || heading ? FontWeight.SemiBold : FontWeight.Normal,
                Foreground = root ? MindViewThemeResources.AccentForeground(AccentColor) : foreground
            });
            var content = new Grid();
            content.Children.Add(new CardDecoration { IsSummary = !root && !heading, Stripe = !root && heading ? new SolidColorBrush(color) : null, Line = line, Margin = new Thickness(-16, -11), IsHitTestVisible = false });
            content.Children.Add(body);
            var card = new Border
            {
                Width = rect.Width, Height = rect.Height, Padding = new Thickness(16, 11), CornerRadius = new CornerRadius(8),
                Background = root ? new SolidColorBrush(AccentColor) : background,
                BorderBrush = node == _selected ? new SolidColorBrush(AccentColor) : !root && !heading ? Brushes.Transparent : line,
                BorderThickness = new Thickness(node == _selected ? 2 : 1), Child = content,
                BoxShadow = BoxShadows.Parse(dark ? "0 3 12 0 #16000000" : "0 3 12 0 #07000000"),
                Cursor = new Cursor(StandardCursorType.Hand), Focusable = true
            };
            if (EnableMotion) card.Transitions = new Transitions { new BrushTransition { Property = Border.BorderBrushProperty, Duration = TimeSpan.FromMilliseconds(160) } };
            ToolTip.SetTip(card, node.FullText);
            Canvas.SetLeft(card, rect.X); Canvas.SetTop(card, rect.Y);
            card.PointerPressed += (_, e) =>
            {
                if (!e.GetCurrentPoint(card).Properties.IsLeftButtonPressed) return;
                if (_selected is { } previous && _cards.TryGetValue(previous, out var previousCard))
                { previousCard.BorderBrush = previous.Kind is MarkdownMindMapNodeKind.Heading or MarkdownMindMapNodeKind.Document ? line : Brushes.Transparent; previousCard.BorderThickness = new Thickness(1); }
                _selected = node; card.BorderBrush = new SolidColorBrush(AccentColor); card.BorderThickness = new Thickness(2);
                Focus(); NodeSelected?.Invoke(this, node);
                if (e.ClickCount >= 2) SourceRequested?.Invoke(this, node);
                e.Handled = true;
            };
            _cards[node] = card; _canvas.Children.Add(card);
            if (node.Children.Count > 0)
            {
                var fold = new Button
                {
                    Content = node.IsCollapsed ? $"+{node.Children.Count}" : "−", Width = 22, Height = 22, MinWidth = 0, MinHeight = 0,
                    Padding = new Thickness(0), FontSize = 10, CornerRadius = new CornerRadius(11), Background = background,
                    Foreground = foreground, BorderBrush = line, BorderThickness = new Thickness(1)
                };
                ToolTip.SetTip(fold, MindViewLocalization.GetResource(node.IsCollapsed ? MindViewL.ExpandBranch : MindViewL.CollapseBranch));
                Canvas.SetLeft(fold, rect.Right - 8); Canvas.SetTop(fold, rect.Center.Y - 11);
                fold.Click += (_, e) => { ToggleCollapse(node); e.Handled = true; };
                _canvas.Children.Add(fold);
            }
        }
        VisibleNodesChanged?.Invoke(this, EventArgs.Empty);

        double Place(MarkdownMindMapNode node, int depth, Color branch)
        {
            var root = ReferenceEquals(node, Root);
            var heading = node.Kind == MarkdownMindMapNodeKind.Heading;
            var height = root ? 60d : heading ? 64d : Math.Clamp(Math.Ceiling(new System.Globalization.StringInfo(node.Title).LengthInTextElements / 16d), 1, 4) * 18 + 24;
            var start = row;
            double y;
            if (node.IsCollapsed || node.Children.Count == 0) { y = row; row += height + 18; }
            else
            {
                var centers = new List<double>();
                for (var i = 0; i < node.Children.Count; i++)
                {
                    var childColor = root ? (i % 5) switch { 1 => SuccessColor, 2 => WarningColor, 4 => Color.Parse(dark ? "#9398B3" : "#8B91A8"), _ => AccentColor } : branch;
                    centers.Add(Place(node.Children[i], depth + 1, childColor));
                }
                var subtreeHeight = row - start - 18;
                if (subtreeHeight < height) row += height - subtreeHeight;
                y = Math.Max(start, (centers[0] + centers[^1]) / 2 - height / 2);
            }
            var rect = new Rect(depth == 0 ? 0 : 294 + (depth - 1) * 304, y, root ? 214 : 224, height);
            _bounds[node] = rect; _branches[node] = branch; _width = Math.Max(_width, rect.Right + 16);
            return rect.Center.Y;
        }
    }

    private sealed class DotGrid : Control
    {
        public IBrush Brush { get; set; } = Brushes.LightGray;
        public override void Render(DrawingContext context)
        {
            using var opacity = context.PushOpacity(.55);
            for (var y = 12d; y < Bounds.Height; y += 22)
            for (var x = 12d; x < Bounds.Width; x += 22) context.DrawEllipse(Brush, null, new Point(x, y), .8, .8);
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
}
