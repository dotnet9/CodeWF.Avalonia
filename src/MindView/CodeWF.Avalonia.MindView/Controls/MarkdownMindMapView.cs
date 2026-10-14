using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Animation;
using CodeWF.Avalonia.MindView.Markdown;

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
    private ViewportState? _pendingViewport;

    public MarkdownMindMapNode? Root { get => GetValue(RootProperty); set => SetValue(RootProperty, value); }
    public Color AccentColor { get => GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
    public bool EnableMotion { get => GetValue(EnableMotionProperty); set => SetValue(EnableMotionProperty, value); }
    public double Zoom => _scale.ScaleX;
    public MarkdownMindMapNode? SelectedNode => _selected;
    public event EventHandler<MarkdownMindMapNode>? NodeSelected;
    public event EventHandler<MarkdownMindMapNode>? SourceRequested;
    public event EventHandler? ZoomChanged;
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
        _viewport.Children.Add(_canvas);
        Content = _viewport;
        SizeChanged += (_, _) => EnsureViewport();
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
            var delta = e.GetPosition(_viewport) - start;
            _translate.X = _panOffset.X + delta.X; _translate.Y = _panOffset.Y + delta.Y;
        };
        _viewport.PointerReleased += (_, e) => { _pan = null; e.Pointer.Capture(null); };
        _viewport.PointerCaptureLost += (_, _) => _pan = null;
        _viewport.PointerWheelChanged += (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) SetZoom(Zoom * Math.Pow(1.12, e.Delta.Y), e.GetPosition(_viewport));
            else { _translate.X += e.Delta.X * 40; _translate.Y += e.Delta.Y * 40; }
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
        else if (change.Property == AccentColorProperty || change.Property == EnableMotionProperty) Rebuild();
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
        ZoomChanged?.Invoke(this, EventArgs.Empty);
    }
    public void EnsureViewport() { if (!_hasViewport && IsEffectivelyVisible) Fit(.75); }

    private void Rebuild()
    {
        if (_canvas is null) return;
        _canvas.Children.Clear(); _bounds.Clear(); _cards.Clear();
        var dark = ActualThemeVariant == ThemeVariant.Dark || ActualThemeVariant?.InheritVariant == ThemeVariant.Dark;
        _viewport.Background = Brush.Parse(dark ? "#171821" : "#F7F8FC");
        if (Root is null) return;
        double row = 0; _width = 0;
        Place(Root, 0);
        _height = Math.Max(72, row * 92);
        foreach (var (node, rect) in _bounds)
        {
            foreach (var child in node.Children)
            {
                if (!_bounds.TryGetValue(child, out var childRect)) continue;
                var a = new Point(rect.Right, rect.Center.Y); var b = new Point(childRect.Left, childRect.Center.Y);
                var path = new global::Avalonia.Controls.Shapes.Path
                {
                    Data = Geometry.Parse(FormattableString.Invariant($"M {a.X},{a.Y} C {a.X + 40},{a.Y} {b.X - 40},{b.Y} {b.X},{b.Y}")),
                    Stroke = new SolidColorBrush(AccentColor, .4), StrokeThickness = 1.5, IsHitTestVisible = false
                };
                _canvas.Children.Add(path);
            }
        }
        foreach (var (node, rect) in _bounds)
        {
            var root = node == Root;
            var text = new TextBlock
            {
                Text = node.Title, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = root ? 16 : 13, FontWeight = root || node.Kind == MarkdownMindMapNodeKind.Heading ? FontWeight.SemiBold : FontWeight.Normal,
                Foreground = root ? MindViewThemeResources.AccentForeground(AccentColor) : Brush.Parse(dark ? "#E8E9F2" : "#323445"), VerticalAlignment = VerticalAlignment.Center
            };
            var card = new Border
            {
                Width = rect.Width, Height = rect.Height, Padding = new Thickness(16, 8), CornerRadius = new CornerRadius(12),
                Background = root ? new SolidColorBrush(AccentColor) : Brush.Parse(dark ? "#242633" : "#FFFFFF"),
                BorderBrush = node == _selected ? new SolidColorBrush(AccentColor) : Brush.Parse(dark ? "#3A3D50" : "#E3E5F0"),
                BorderThickness = new Thickness(node == _selected ? 2 : 1), Child = text,
                BoxShadow = BoxShadows.Parse(dark ? "0 4 16 0 #22000000" : "0 4 16 0 #09000000"),
                Cursor = new Cursor(StandardCursorType.Hand), Focusable = true
            };
            if (EnableMotion) card.Transitions = new Transitions { new BrushTransition { Property = Border.BorderBrushProperty, Duration = TimeSpan.FromMilliseconds(160) } };
            ToolTip.SetTip(card, node.FullText);
            Canvas.SetLeft(card, rect.X); Canvas.SetTop(card, rect.Y);
            card.PointerPressed += (_, e) =>
            {
                if (!e.GetCurrentPoint(card).Properties.IsLeftButtonPressed) return;
                if (_selected is { } previous && _cards.TryGetValue(previous, out var previousCard))
                { previousCard.BorderBrush = Brush.Parse(dark ? "#3A3D50" : "#E3E5F0"); previousCard.BorderThickness = new Thickness(1); }
                _selected = node; card.BorderBrush = new SolidColorBrush(AccentColor); card.BorderThickness = new Thickness(2);
                Focus(); NodeSelected?.Invoke(this, node);
                if (e.ClickCount >= 2) SourceRequested?.Invoke(this, node);
                e.Handled = true;
            };
            _cards[node] = card;
            _canvas.Children.Add(card);
            if (node.Children.Count > 0)
            {
                var fold = new Button { Content = node.IsCollapsed ? $"+{node.Children.Count}" : "−", Width = 28, Height = 26, MinWidth = 0, MinHeight = 0, Padding = new Thickness(0), FontSize = 11 };
                ToolTip.SetTip(fold, node.IsCollapsed ? "展开分支" : "折叠分支");
                Canvas.SetLeft(fold, rect.Right - 8); Canvas.SetTop(fold, rect.Center.Y - 13);
                fold.Click += (_, e) => { ToggleCollapse(node); e.Handled = true; };
                _canvas.Children.Add(fold);
            }
        }

        double Place(MarkdownMindMapNode node, int depth)
        {
            double y;
            if (node.IsCollapsed || node.Children.Count == 0) y = row++ * 92;
            else
            {
                var first = Place(node.Children[0], depth + 1);
                var last = first;
                foreach (var child in node.Children.Skip(1)) last = Place(child, depth + 1);
                y = (first + last) / 2;
            }
            var rect = new Rect(depth * 280, y, 210, 68);
            _bounds[node] = rect; _width = Math.Max(_width, rect.Right);
            return y;
        }
    }
}
