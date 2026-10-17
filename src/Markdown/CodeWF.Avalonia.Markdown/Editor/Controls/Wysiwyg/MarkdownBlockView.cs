using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.VisualTree;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Toolkit.Lang;
using global::Avalonia.Threading;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Data;

namespace CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;

/// <summary>在 TextBox 默认插入之前处理已提交文本，保留输入法预编辑流程。</summary>
internal sealed class MarkdownInputTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);
    public Action<TextInputEventArgs>? BeforeTextInput { get; set; }
    protected override void OnTextInput(TextInputEventArgs e)
    {
        BeforeTextInput?.Invoke(e);
        if (!e.Handled) base.OnTextInput(e);
    }
}

/// <summary>
/// 一个块的就地编辑视图：默认渲染为富文本（只读显示，可选中），
/// 点入后使用原位富文本输入（<see cref="TextBox"/>），失焦或按 Esc 结束编辑，
/// 按 Tab 进入下一个块。文本变化通过 <see cref="TextCommitted"/> 回抛给宿主视图。
/// </summary>
internal sealed class MarkdownBlockView : UserControl
{
    private readonly MarkdownRichTextView _richText = new();
    private readonly MarkdownInputTextBox _editor = new();
    private readonly TextBox? _codeLanguage;
    private readonly Border _root;
    private readonly Grid _layer = new();
    private readonly Canvas _editingLayer = new() { IsVisible = false };
    private SelectableTextBlock? _editingText;
    private bool _inlineEditing;
    private MarkdownViewer? _preview;
    private readonly bool _isCode;
    private readonly bool _isTableCell;
    private readonly MarkdownBlockViewOptions _options;

    private double _runFontSize = 15;

    private bool _editing;
    private bool _syncingText;
    private IPointer? _selectionPointer;
    private int _pointerSelectionAnchor;
    private string _editOriginalText = string.Empty;
    private string _editOriginalLanguage = string.Empty;

    public MarkdownBlockView(MarkdownBlockModel model, bool isCode, MarkdownBlockViewOptions options, bool isTableCell = false)
    {
        Model = model;
        _isCode = isCode;
        _isTableCell = isTableCell;
        _options = options;

        if (isCode)
        {
            // 语言选择占用代码块原有的下方留白，出现光标时不推动正文。
            _layer.RowDefinitions = new RowDefinitions("Auto,20");
            Grid.SetRowSpan(_editingLayer, 2);
            _codeLanguage = new TextBox
            {
                Classes = { "MdCodeLanguageInput" }, Width = 180, Height = 24, MinHeight = 0,
                Padding = new Thickness(8, 2), FontSize = 12, TextAlignment = TextAlignment.Center, PlaceholderText = "text",
                Background = options.CodeBackgroundBrush, Foreground = options.TextBrush,
                BorderBrush = options.SeparatorBrush, BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3)
            };
            _codeLanguage.TextChanged += (_, _) =>
            {
                if (!_editing || _syncingText) return;
                var language = (_codeLanguage.Text ?? string.Empty).Trim().Replace("`", string.Empty);
                if (Model.Language == language) return;
                Model.Language = language; UpdateRichText(); TextCommitted?.Invoke(this, ModelText);
            };
            _codeLanguage.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter) { _editor.Focus(); e.Handled = true; }
                else if (e.Key == Key.Escape) { CancelEdit(); e.Handled = true; }
            };
            _codeLanguage.LostFocus += (_, _) => CommitIfFocusLeft();
            _editingLayer.Children.Add(_codeLanguage);
        }

        _editor.AcceptsReturn = true;
        _editor.TextWrapping = TextWrapping.Wrap;
        _editor.FontFamily = isCode ? options.CodeFontFamily : FontFamily.Default;
        _editor.FontSize = _runFontSize;
        _editor.Foreground = options.TextBrush;
        _editor.CaretBrush = options.AccentBrush;
        _editor.HorizontalAlignment = HorizontalAlignment.Stretch;
        _editor.Text = ModelText;
        _editor.Padding = new Thickness(0);
        _editor.BorderThickness = new Thickness(0);
        _editor.MinHeight = 0;
        _editor.MinWidth = 0;
        _editor.Background = Brushes.Transparent;
        _editor.Template = new FuncControlTemplate<TextBox>((box, scope) =>
        {
            var presenter = new MarkdownInlinePresenter(options, isCode || model.Kind == MarkdownBlockKind.Raw) { Name = "PART_TextPresenter" };
            scope.Register("PART_TextPresenter", presenter);
            foreach (var property in new[] { "Text", "CaretIndex", "SelectionStart", "SelectionEnd", "FontFamily", "FontSize", "FontWeight", "FontStyle", "Foreground", "CaretBrush", "SelectionBrush", "SelectionForegroundBrush", "TextWrapping", "LineHeight" })
                presenter.Bind(property switch
                {
                    "Text" => TextPresenter.TextProperty, "CaretIndex" => TextPresenter.CaretIndexProperty,
                    "SelectionStart" => TextPresenter.SelectionStartProperty, "SelectionEnd" => TextPresenter.SelectionEndProperty,
                    "FontFamily" => global::Avalonia.Controls.Documents.TextElement.FontFamilyProperty, "FontSize" => global::Avalonia.Controls.Documents.TextElement.FontSizeProperty,
                    "FontWeight" => global::Avalonia.Controls.Documents.TextElement.FontWeightProperty, "FontStyle" => global::Avalonia.Controls.Documents.TextElement.FontStyleProperty,
                    "Foreground" => global::Avalonia.Controls.Documents.TextElement.ForegroundProperty, "CaretBrush" => TextPresenter.CaretBrushProperty,
                    "SelectionBrush" => TextPresenter.SelectionBrushProperty, "SelectionForegroundBrush" => TextPresenter.SelectionForegroundBrushProperty,
                    "LineHeight" => TextPresenter.LineHeightProperty, _ => TextPresenter.TextWrappingProperty
                }, new Binding(property) { Source = box, Mode = BindingMode.TwoWay });
            var scroll = new ScrollViewer { Name = "PART_ScrollViewer", Content = presenter,
                HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
            scope.Register("PART_ScrollViewer", scroll); return scroll;
        });
        _editor.TextChanged += (_, _) =>
        {
            if (!_editing || _syncingText) return;
            var text = _editor.Text ?? string.Empty;
            if (string.Equals(ModelText, text, StringComparison.Ordinal)) return;
            ModelText = text;
            UpdateRichText();
            TextCommitted?.Invoke(this, ModelText);
        };
        _editor.LostFocus += (_, _) => CommitIfFocusLeft();
        _editor.KeyDown += OnEditorKeyDown;
        _editor.PropertyChanged += (_, e) =>
        {
            if (_editing && e.Property.Name is "CaretIndex" or "SelectionStart" or "SelectionEnd") EditorSelectionChanged?.Invoke();
        };

        _editingLayer.Children.Add(_editor);
        _layer.Children.Add(_richText); _layer.Children.Add(_editingLayer);
        _root = new Border { Child = _layer };
        Content = _root;

        // 先接管渲染块的点击，避免 SelectableTextBlock 在冒泡前后抢回焦点，
        // 导致刚打开的编辑框立即触发 LostFocus 并消失。
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        PointerCaptureLost += (_, _) => _selectionPointer = null;
        UpdateRichText();
        LayoutUpdated += (_, _) => { if (_inlineEditing) PositionInlineEditor(); };
    }

    /// <summary>块内容变化的回调（参数为新文本）。</summary>
    public Action<MarkdownBlockView, string>? TextCommitted { get; set; }

    /// <summary>请求把焦点移到相邻块（参数为相对方向：-1 上一块，+1 下一块）。</summary>
    public Action<int>? NavigateRequested { get; set; }
    public Func<int, double?, bool>? BoundaryNavigateRequested { get; set; }
    public Func<int, bool>? MergeRequested { get; set; }

    public Action<MarkdownBlockView>? EditingStarted { get; set; }
    public Action? EditorSelectionChanged { get; set; }
    internal Action<TextInputEventArgs>? TextEntering { get => _editor.BeforeTextInput; set => _editor.BeforeTextInput = value; }

    public Action<MarkdownBlockView, int>? SplitRequested { get; set; }

    public MarkdownBlockModel Model { get; }

    private bool _renderQuoteAsParagraph;
    internal bool RenderQuoteAsParagraph
    {
        get => _renderQuoteAsParagraph;
        set { _renderQuoteAsParagraph = value; UpdateRichText(); }
    }

    /// <summary>正文字号；标题与代码块按类型设置。</summary>
    public double RunFontSize
    {
        get => _runFontSize;
        set
        {
            _runFontSize = value;
            _editor.FontSize = value;
            UpdateRichText();
        }
    }

    /// <summary>外部修改模型后刷新显示并通知宿主。</summary>
    public void NotifyModelChanged()
    {
        RefreshFromModel();
        TextCommitted?.Invoke(this, Model.Text);
    }

    public bool IsEditing => _editing;

    public void InsertText(string text)
    {
        if (!_editing) BeginEditAtEnd();
        var start = Math.Min(_editor.SelectionStart, _editor.SelectionEnd);
        var length = Math.Abs(_editor.SelectionEnd - _editor.SelectionStart);
        _editor.Text = CurrentText.Remove(start, length).Insert(start, text);
        _editor.CaretIndex = start + text.Length;
    }

    /// <summary>正在编辑时的底层文本框（用于格式化动作），否则为 null。</summary>
    internal TextBox? ActiveEditor => _editing ? _editor : null;
    internal TextBox EditingInput => _editor;

    /// <summary>当前编辑文本（非编辑态返回模型文本）。</summary>
    internal string CurrentText => _editing ? _editor.Text ?? string.Empty : ModelText;

    private string ModelText
    {
        get => Model.Kind == MarkdownBlockKind.Raw ? Model.Raw : Model.Text;
        set { if (Model.Kind == MarkdownBlockKind.Raw) Model.Raw = value; else Model.Text = value; }
    }

    /// <summary>进入编辑态并把光标放到行尾（等价于点入该块）。</summary>
    public void BeginEditAtEnd() => BeginEdit(ModelText.Length);

    /// <summary>进入编辑态，并把光标放到指定偏移。</summary>
    public void BeginEdit(int caretIndex)
    {
        if (_editing)
        {
            _editor.CaretIndex = Math.Clamp(caretIndex, 0, _editor.Text?.Length ?? 0);
            _editor.Focus();
            return;
        }

        _editOriginalText = ModelText;
        _editOriginalLanguage = Model.Language;
        _syncingText = true;
        _editor.Text = ModelText;
        if (_codeLanguage is not null) _codeLanguage.Text = Model.Language;
        _syncingText = false;
        _editing = true;
        UpdateRichText();
        _inlineEditing = _isCode || Model.Kind != MarkdownBlockKind.Raw && !ModelText.Contains("![", StringComparison.Ordinal) && !ModelText.Contains('$');
        if (_inlineEditing)
        {
            _editingLayer.IsVisible = true;
            PositionInlineEditor();
        }
        else
        {
            _editingLayer.Children.Remove(_editor);
            _root.Child = _editor;
        }
        _editor.Focus();
        _editor.CaretIndex = Math.Clamp(caretIndex, 0, _editor.Text?.Length ?? 0);
        EditingStarted?.Invoke(this);
    }

    /// <summary>回渲染（提交文本）。</summary>
    public void CommitAndRender()
    {
        if (!_editing)
        {
            return;
        }

        _editing = false;
        EndInlineEditor();
        var text = _editor.Text ?? string.Empty;
        if (!string.Equals(text, ModelText, StringComparison.Ordinal))
        {
            ModelText = text;
            TextCommitted?.Invoke(this, text);
        }
        UpdateRichText();
    }

    private void CommitIfFocusLeft() => Dispatcher.UIThread.Post(() =>
    {
        if (!_editor.IsKeyboardFocusWithin && _codeLanguage?.IsKeyboardFocusWithin != true) CommitAndRender();
    }, DispatcherPriority.Input);

    private void CancelEdit()
    {
        ModelText = _editOriginalText; Model.Language = _editOriginalLanguage;
        _editing = false; EndInlineEditor(); UpdateRichText(); TextCommitted?.Invoke(this, ModelText);
    }

    /// <summary>外部替换内容（例如预览回写）时刷新显示。</summary>
    public void RefreshFromModel()
    {
        if (_editing)
        {
            return;
        }

        UpdateRichText();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Visual interactive && IsInteractiveChild(interactive)) return;
        if (e.Source is Visual languageSource && _codeLanguage is not null && (languageSource == _codeLanguage || languageSource.GetVisualAncestors().Contains(_codeLanguage))) return;
        if (_editing || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var offset = GetPointerTextOffset(e);
        BeginEdit(offset);
        _pointerSelectionAnchor = offset;
        _editor.SelectionStart = _editor.SelectionEnd = offset;
        // 初次按下发生在只读预览上，TextBox 没有收到按下事件，必须保留拖选起点。
        _selectionPointer = e.Pointer;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    internal static bool IsInteractiveChild(Visual source) => source.GetVisualAncestors().Prepend(source)
        .Any(element => element is global::Avalonia.Controls.Primitives.ScrollBar or Button);

    internal int GetPointerTextOffset(PointerEventArgs e)
    {
        if (_editing && _editor.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault() is { } presenter)
            return Math.Clamp(presenter.TextLayout.HitTestPoint(e.GetPosition(presenter)).TextPosition, 0, ModelText.Length);
        var offset = _preview is null
            ? MarkdownTextRunParser.GetSourceOffset(ModelText, _richText.GetTextOffset(e.GetPosition(_richText)))
            : ModelText.Length;
        if (_preview is not null)
        {
            var text = _preview.GetVisualDescendants().OfType<SelectableTextBlock>()
                .FirstOrDefault(block => !_isCode || block.Classes.Contains("MdCodeBlockText"));
            if (text?.TextLayout is { } layout)
            {
                var visible = layout.HitTestPoint(e.GetPosition(text)).TextPosition;
                offset = _isCode || Model.Kind == MarkdownBlockKind.Raw ? visible
                    : MarkdownTextRunParser.GetSourceOffset(ModelText, visible);
            }
        }
        return Math.Clamp(offset, 0, ModelText.Length);
    }

    internal void SetTextSelection(int start, int end)
    {
        start = Math.Clamp(start, 0, ModelText.Length); end = Math.Clamp(end, 0, ModelText.Length);
        if (_editing)
        {
            _editor.CaretIndex = end; _editor.SelectionStart = start; _editor.SelectionEnd = end;
            return;
        }
        if (_preview is null) return;
        var text = _preview.GetVisualDescendants().OfType<SelectableTextBlock>()
            .FirstOrDefault(block => !_isCode || block.Classes.Contains("MdCodeBlockText"));
        if (text is null) return;
        text.SelectionStart = _isCode || Model.Kind == MarkdownBlockKind.Raw ? start : MarkdownTextRunParser.GetTextOffset(ModelText, start);
        text.SelectionEnd = _isCode || Model.Kind == MarkdownBlockKind.Raw ? end : MarkdownTextRunParser.GetTextOffset(ModelText, end);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_selectionPointer != e.Pointer || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var offset = GetPointerTextOffset(e);
        _editor.CaretIndex = offset;
        _editor.SelectionStart = _pointerSelectionAnchor;
        _editor.SelectionEnd = offset;
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_selectionPointer != e.Pointer) return;
        _selectionPointer = null;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.None && _editor.SelectionStart == _editor.SelectionEnd)
        {
            var caret = _editor.CaretIndex;
            var length = CurrentText.Length;
            var direction = e.Key is Key.Left or Key.Up or Key.Back ? -1 : 1;
            if (e.Key is Key.Left or Key.Right && (direction < 0 ? caret == 0 : caret == length)
                && BoundaryNavigateRequested?.Invoke(direction, null) == true) { e.Handled = true; return; }
            if (e.Key is Key.Up or Key.Down && _editor.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault() is { } presenter)
            {
                var rect = presenter.TextLayout.HitTestTextPosition(caret);
                if ((direction < 0 ? rect.Top < rect.Height : rect.Bottom >= presenter.TextLayout.Height - .5)
                    && BoundaryNavigateRequested?.Invoke(direction, rect.X) == true) { e.Handled = true; return; }
            }
            if (e.Key is Key.Back or Key.Delete && (direction < 0 ? caret == 0 : caret == length)
                && MergeRequested?.Invoke(direction) == true) { e.Handled = true; return; }
        }
        switch (e.Key)
        {
            case Key.Escape:
                CancelEdit();
                e.Handled = true;
                break;

            case Key.Enter when e.KeyModifiers == KeyModifiers.Control:
                CommitAndRender(); e.Handled = true; break;

            case Key.Enter when !_isCode && Model.Kind != MarkdownBlockKind.Raw && e.KeyModifiers == KeyModifiers.None:
                e.Handled = true;
                var caret = _editor.CaretIndex;
                CommitAndRender();
                SplitRequested?.Invoke(this, caret);
                break;

            case Key.Enter when e.KeyModifiers == KeyModifiers.Shift:
                var start = Math.Min(_editor.SelectionStart, _editor.SelectionEnd);
                var length = Math.Abs(_editor.SelectionEnd - _editor.SelectionStart);
                _editor.Text = CurrentText.Remove(start, length).Insert(start, _isCode ? "\n" : "  \n");
                _editor.CaretIndex = start + (_isCode ? 1 : 3);
                e.Handled = true;
                break;

            case Key.Tab:
                e.Handled = true;
                CommitAndRender();
                NavigateRequested?.Invoke(e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -1 : 1);
                break;
        }
    }

    private void UpdateRichText()
    {
        if (_editing && !_inlineEditing) return;
        var useViewer = !_isTableCell || ModelText.Contains("![", StringComparison.Ordinal) || ModelText.Contains('$');
        if (useViewer)
        {
            var preview = _preview ??= CreatePreview();

            preview.ImageBasePath = _options.ImageBasePath;
            preview.TypographyTheme = _options.TypographyTheme;
            preview.TypographySize = _options.TypographySize;
            preview.Markdown = Model.Kind == MarkdownBlockKind.ListItem || RenderQuoteAsParagraph
                ? Model.Text : MarkdownBlockParser.WriteCanonical([Model]);
            SetDisplay(preview);
            return;
        }

        _richText.BindPalette(_options);
        _richText.FontWeight = Model.HeadingLevel > 0 ? FontWeight.Bold : FontWeight.Normal;
        _richText.RunFontSize = _runFontSize;
        _richText.SetRuns(_isCode
            ? MarkdownTextRunParser.Plain(Model.Text, code: true)
            : MarkdownTextRunParser.Parse(Model.Text));
        SetDisplay(_richText);
    }

    private void SetDisplay(Control display)
    {
        if (!_layer.Children.Contains(display)) { _layer.Children.Clear(); _layer.Children.Add(display); _layer.Children.Add(_editingLayer); }
        if (!ReferenceEquals(_root.Child, _layer)) _root.Child = _layer;
    }
    private void PositionInlineEditor()
    {
        var text = _preview?.GetVisualDescendants().OfType<SelectableTextBlock>()
            .FirstOrDefault(block => !_isCode || block.Classes.Contains("MdCodeBlockText"));
        if (text is not null && text.TranslatePoint(default, _layer) is { } point)
        {
            if (_editingText is not null && !ReferenceEquals(_editingText, text)) _editingText.Opacity = 1;
            _editingText = text; text.Opacity = _isCode ? 1 : 0;
            // 代码沿用预览的语法高亮；选区只由上方的输入层绘制，避免遗留只读选区。
            if (_isCode) text.SelectionStart = text.SelectionEnd = 0;
            Canvas.SetLeft(_editor, point.X); Canvas.SetTop(_editor, point.Y); _editor.Width = Math.Max(1, text.Bounds.Width);
            _editor.FontSize = text.FontSize; _editor.FontWeight = text.FontWeight; _editor.FontFamily = text.FontFamily;
            _editor.LineHeight = text.LineHeight; _editor.Foreground = _isCode ? Brushes.Transparent : text.Foreground;
            _editor.TextWrapping = _isCode ? TextWrapping.NoWrap : TextWrapping.Wrap;
            if (_isCode && text.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault() is { } codeScroll)
            {
                // 输入层位于预览上方，只能覆盖代码的可见文字范围，不能挡住滚动条。
                var clip = new Rect(text.Bounds.Size);
                if (codeScroll.GetVisualDescendants().OfType<ScrollContentPresenter>().FirstOrDefault() is { } viewport
                    && viewport.TranslatePoint(default, text) is { } viewportOrigin)
                    clip = clip.Intersect(new Rect(viewportOrigin, viewport.Bounds.Size));
                foreach (var bar in codeScroll.GetVisualDescendants().OfType<global::Avalonia.Controls.Primitives.ScrollBar>().Where(bar => bar.IsVisible))
                {
                    if (bar.TranslatePoint(default, text) is not { } origin) continue;
                    if (bar.Orientation == Orientation.Horizontal)
                        clip = clip.Intersect(new Rect(clip.X, clip.Y, clip.Width, Math.Max(0, origin.Y - clip.Y)));
                }
                _editor.Clip = new RectangleGeometry(clip);
            }
            if (_codeLanguage is not null && text.GetVisualAncestors().OfType<Border>().FirstOrDefault(border => border.Classes.Contains("MdCodeBlock")) is { } frame
                && frame.TranslatePoint(new Point(frame.Bounds.Width, frame.Bounds.Height), _layer) is { } bottom)
            {
                Canvas.SetLeft(_codeLanguage, Math.Max(0, bottom.X - _codeLanguage.Width));
                Canvas.SetTop(_codeLanguage, bottom.Y + 3);
            }
        }
        else if (_preview is null)
        {
            _richText.Opacity = 0; _editor.Width = Math.Max(1, _richText.Bounds.Width); _editor.FontSize = _richText.FontSize;
            _editor.FontWeight = _richText.FontWeight;
        }
    }
    private void EndInlineEditor()
    {
        _editor.Clip = null;
        _inlineEditing = false; _editingLayer.IsVisible = false; _richText.Opacity = 1;
        if (ReferenceEquals(_root.Child, _editor)) _root.Child = null;
        if (!_editingLayer.Children.Contains(_editor)) _editingLayer.Children.Add(_editor);
        if (_editingText is not null) _editingText.Opacity = 1;
        _editingText = null;
    }

    private MarkdownViewer CreatePreview() => new() { DocumentBottomPadding = 0, IsTaskEditingEnabled = false, ShowCodeBlockHeader = false, CodeFontFamily = _options.CodeFontFamily };
}

/// <summary>所见即所得视图里一个表格块的编辑视图（每个单元格就地编辑）。</summary>
internal sealed class MarkdownTableView : UserControl
{
    private readonly Grid _grid = new global::CodeWF.Avalonia.Markdown.Rendering.MarkdownTableGrid();
    private readonly List<List<MarkdownBlockView>> _cells = [];
    private readonly MarkdownBlockModel _model;
    private readonly MarkdownBlockViewOptions _options;

    private static readonly string[] MenuResourceKeys =
    [
        MarkdownL.EditorTableAddRow, MarkdownL.EditorTableRemoveLastRow,
        MarkdownL.EditorTableAddColumn, MarkdownL.EditorTableRemoveLastColumn
    ];

    public MarkdownTableView(MarkdownBlockModel model, MarkdownBlockViewOptions options)
    {
        _model = model;
        _options = options;
        _grid.Classes.Add("MdTable");
        Rebuild();
        var container = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = _grid
        };
        container.Classes.Add("MdTableContainer");
        container.SetValue(Border.BorderBrushProperty, _options.SeparatorBrush, global::Avalonia.Data.BindingPriority.Style);
        container.SetValue(Control.HorizontalAlignmentProperty, HorizontalAlignment.Left, global::Avalonia.Data.BindingPriority.Style);
        Content = container;
        var menu = new ContextMenu();
        for (var action = 0; action < MenuResourceKeys.Length; action++)
        {
            var capturedAction = action;
            var item = new MenuItem { Header = MarkdownLocalization.GetResource(MenuResourceKeys[action]) };
            item.Click += (_, _) => StructureChangeRequested?.Invoke(this, capturedAction);
            menu.Items.Add(item);
        }
        ContextMenu = menu;
        AttachedToVisualTree += (_, _) =>
        {
            I18nManager.Instance.CultureChanged += OnLocalizationChanged;
            I18nManager.Instance.ResourcesChanged += OnLocalizationChanged;
            UpdateMenuHeaders();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            I18nManager.Instance.CultureChanged -= OnLocalizationChanged;
            I18nManager.Instance.ResourcesChanged -= OnLocalizationChanged;
        };
    }

    private void OnLocalizationChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess()) UpdateMenuHeaders();
        else Dispatcher.UIThread.Post(UpdateMenuHeaders);
    }

    private void UpdateMenuHeaders()
    {
        if (ContextMenu is null) return;
        for (var index = 0; index < MenuResourceKeys.Length; index++)
        {
            if (ContextMenu.Items[index] is MenuItem item)
                item.Header = MarkdownLocalization.GetResource(MenuResourceKeys[index]);
        }
    }

    /// <summary>单元格内容变化回调。</summary>
    public Action<MarkdownTableView>? CellsChanged { get; set; }
    public Action<MarkdownBlockView>? CellEditingStarted { get; set; }
    public Action? CellSelectionChanged { get; set; }
    private MarkdownBlockView? _activeCell;
    internal MarkdownBlockView? ActiveCell => _activeCell;
    internal MarkdownBlockView? FirstCell => _cells.SelectMany(row => row).FirstOrDefault();

    /// <summary>请求增删行列（参数为 0=加行 1=删行 2=加列 3=删列）。</summary>
    public Action<MarkdownTableView, int>? StructureChangeRequested { get; set; }

    public MarkdownBlockModel Model => _model;

    public void RefreshFromModel() => Rebuild();

    private void Rebuild()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        _cells.Clear();
        _activeCell = null;

        var rows = _model.Cells;
        var columnCount = rows.Count == 0 ? 0 : rows.Max(row => row.Count);
        if (columnCount == 0)
        {
            return;
        }

        for (var column = 0; column < columnCount; column++)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        for (var row = 0; row < rows.Count; row++)
        {
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var rowBoxes = new List<MarkdownBlockView>();
            for (var column = 0; column < columnCount; column++)
            {
                var value = column < rows[row].Count ? rows[row][column] : string.Empty;
                var isHeader = row == 0 && _model.HasHeader;
                var cellModel = new MarkdownBlockModel { Kind = MarkdownBlockKind.Paragraph, Text = value,
                    HeadingLevel = isHeader ? 6 : 0 };
                var box = new MarkdownBlockView(cellModel, false, _options, isTableCell: true) { RunFontSize = _options.FontSize };
                var capturedRow = row;
                var capturedColumn = column;
                box.TextCommitted = (_, text) => CommitCell(capturedRow, capturedColumn, text);
                box.EditingStarted = cell =>
                {
                    if (_activeCell != cell) _activeCell?.CommitAndRender();
                    _activeCell = cell;
                    CellEditingStarted?.Invoke(cell);
                };
                box.EditorSelectionChanged = () => CellSelectionChanged?.Invoke();
                box.SplitRequested = (_, _) => MoveFocus(capturedRow + 1, capturedColumn);
                box.NavigateRequested = direction =>
                {
                    var next = capturedRow * columnCount + capturedColumn + direction;
                    MoveFocus(next / columnCount, next % columnCount);
                };
                var cell = new Border { Padding = new Thickness(10, 8), Child = box,
                    BorderThickness = new Thickness(0, 0, 1, 1) };
                cell.Classes.Add(isHeader ? "MdTableHeaderCell" : "MdTableCell");
                if (!isHeader && (row - (_model.HasHeader ? 1 : 0)) % 2 == 1) cell.Classes.Add("MdTableAlternateCell");
                if (isHeader) cell.Background = _options.TableHeaderBackgroundBrush ?? _options.CodeBackgroundBrush;
                cell.SetValue(Border.BorderBrushProperty, _options.SeparatorBrush, global::Avalonia.Data.BindingPriority.Style);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);
                _grid.Children.Add(cell);
                rowBoxes.Add(box);
            }

            _cells.Add(rowBoxes);
        }
    }

    private void CommitCell(int row, int column, string value)
    {
        if (_model.Cells.Count <= row || _model.Cells[row].Count <= column)
        {
            return;
        }

        if (string.Equals(_model.Cells[row][column], value, StringComparison.Ordinal))
        {
            return;
        }

        _model.Cells[row][column] = value;
        CellsChanged?.Invoke(this);
    }

    private void MoveFocus(int row, int column)
    {
        if (row >= 0 && row < _cells.Count && column >= 0 && column < _cells[row].Count)
        {
            _cells[row][column].BeginEditAtEnd();
        }
    }
}
