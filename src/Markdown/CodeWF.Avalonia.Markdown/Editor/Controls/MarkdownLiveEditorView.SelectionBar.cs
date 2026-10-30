using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;

namespace CodeWF.Avalonia.Markdown.Editor.Controls;

/// <summary>
/// 即时编辑的选区浮动条（对齐原型 selection-menu）：选中文本后在选区附近弹出，
/// 加粗/斜体/删除线/行内代码/标题/列表 + 字号与前景/背景色板 + 清除样式；
/// 全部动作走既有 ExecuteAsync/ApplySpanStyle 管线，按钮不抢焦点以保住选区。
/// </summary>
public partial class MarkdownLiveEditorView
{
    private Popup? _selectionBarPopup;
    private StackPanel? _selectionBarRows;
    private Panel? _selectionBarSizeRow;
    private Panel? _selectionBarForegroundRow;
    private Panel? _selectionBarBackgroundRow;
    private Point _selectionBarAnchor;

    private void ShowSelectionBar()
    {
        if (_selectionBarPopup is null)
        {
            _selectionBarPopup = new Popup
            {
                PlacementTarget = this,
                Placement = PlacementMode.BottomEdgeAlignedLeft,

            };
            _selectionBarPopup.Child = BuildSelectionBar();
        }

        _selectionBarPopup.HorizontalOffset = Math.Clamp(_selectionBarAnchor.X - 40, 4, Math.Max(4, Bounds.Width - 460));
        _selectionBarPopup.VerticalOffset = Math.Max(4, _selectionBarAnchor.Y - 96);
        CloseSelectionBarRows();
        _selectionBarPopup.IsOpen = true;
    }

    private void HideSelectionBar()
    {
        if (_selectionBarPopup is { IsOpen: true } popup)
        {
            popup.IsOpen = false;
        }
    }

    private void CloseSelectionBarRows()
    {
        if (_selectionBarSizeRow is { } sizeRow) sizeRow.IsVisible = false;
        if (_selectionBarForegroundRow is { } fgRow) fgRow.IsVisible = false;
        if (_selectionBarBackgroundRow is { } bgRow) bgRow.IsVisible = false;
    }

    private Control BuildSelectionBar()
    {
        var options = _blockOptions;
        var bar = new Border
        {
            Background = options.CodeBackgroundBrush,
            BorderBrush = options.SeparatorBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(4),
            BoxShadow = BoxShadows.Parse("0 6 18 0 #33000000"),
        };
        _selectionBarRows = new StackPanel { Orientation = Orientation.Vertical };
        _selectionBarRows.Children.Add(BuildSelectionBarButtons(options));
        _selectionBarSizeRow = BuildSizeRow();
        _selectionBarForegroundRow = BuildSwatchRow(foreground: true);
        _selectionBarBackgroundRow = BuildSwatchRow(foreground: false);
        _selectionBarRows.Children.Add(_selectionBarSizeRow);
        _selectionBarRows.Children.Add(_selectionBarForegroundRow);
        _selectionBarRows.Children.Add(_selectionBarBackgroundRow);
        bar.Child = _selectionBarRows;
        return bar;
    }

    private StackPanel BuildSelectionBarButtons(MarkdownBlockViewOptions options)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };

        Button AddGlyph(string glyph, IBrush? foreground = null, IBrush? background = null)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = glyph,
                    FontSize = 13,
                    FontWeight = FontWeight.Bold,
                    Foreground = foreground ?? options.TextBrush,
                    Background = background ?? Brushes.Transparent,
                },
                Focusable = false,
                Width = 30,
                Height = 28,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            row.Children.Add(button);
            return button;
        }

        void Hook(string glyph, string tooltip, Action click, IBrush? fg = null, IBrush? bg = null)
        {
            var button = AddGlyph(glyph, fg, bg);
            button.SetValue(ToolTip.TipProperty, new ToolTip { Content = tooltip });
            button.Click += (_, _) => { click(); HideSelectionBar(); };
        }

        Hook("B", "加粗", () => _ = ExecuteAsync(MarkdownEditorAction.Bold));
        Hook("I", "斜体", () => _ = ExecuteAsync(MarkdownEditorAction.Italic), fg: options.MutedBrush);
        Hook("S", "删除线", () => _ = ExecuteAsync(MarkdownEditorAction.Strikethrough), fg: options.MutedBrush);
        Hook("`", "行内代码", () => _ = ExecuteAsync(MarkdownEditorAction.InlineCode), fg: options.CodeBrush);
        Hook("H1", "一级标题", () => _ = ExecuteAsync(MarkdownEditorAction.Heading1), fg: options.AccentBrush);
        Hook("H2", "二级标题", () => _ = ExecuteAsync(MarkdownEditorAction.Heading2), fg: options.AccentBrush);
        Hook("•", "列表", () => _ = ExecuteAsync(MarkdownEditorAction.UnorderedList));
        row.Children.Add(new Separator { Width = 1, Height = 18, Margin = new Thickness(3, 5) });
        Hook("A▾", "字号", () =>
        {
            var show = _selectionBarSizeRow is not { IsVisible: true };
            CloseSelectionBarRows();
            if (_selectionBarSizeRow is { } sizeRow) sizeRow.IsVisible = show;
        });
        Hook("A▾", "文字颜色", () =>
        {
            var show = _selectionBarForegroundRow is not { IsVisible: true };
            CloseSelectionBarRows();
            if (_selectionBarForegroundRow is { } fgRow) fgRow.IsVisible = show;
        }, fg: new SolidColorBrush(Color.FromRgb(0x00, 0x64, 0xFA)));
        Hook("A▾", "背景颜色", () =>
        {
            var show = _selectionBarBackgroundRow is not { IsVisible: true };
            CloseSelectionBarRows();
            if (_selectionBarBackgroundRow is { } bgRow) bgRow.IsVisible = show;
        }, bg: new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xBF)));
        row.Children.Add(new Separator { Width = 1, Height = 18, Margin = new Thickness(3, 5) });
        Hook("✕", "清除样式", () => ClearSpanStyles(), fg: options.MutedBrush);
        return row;
    }

    private WrapPanel BuildSizeRow()
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal, IsVisible = false, Margin = new Thickness(2, 3) };
        foreach (var size in MarkdownWysiwygPalette.SpanSizes)
        {
            var captured = size;
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = size.ToString("0", System.Globalization.CultureInfo.InvariantCulture),
                    FontSize = 12,
                    Foreground = _blockOptions.TextBrush,
                },
                Focusable = false,
                Width = 44,
                Height = 26,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,

            };
            button.SetValue(ToolTip.TipProperty, new ToolTip { Content = size.ToString("0 px", System.Globalization.CultureInfo.InvariantCulture) });
            button.Click += (_, _) =>
            {
                ApplySpanStyle("font-size", captured.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "px");
                HideSelectionBar();
            };
            row.Children.Add(button);
        }

        return row;
    }

    private WrapPanel BuildSwatchRow(bool foreground)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal, IsVisible = false, Margin = new Thickness(2, 3) };
        var shades = foreground ? MarkdownWysiwygPalette.SpanForegroundShades : MarkdownWysiwygPalette.SpanBackgroundShades;
        foreach (var (name, hex) in shades)
        {
            var captured = hex;
            var button = new Button
            {
                Focusable = false,
                Width = 22,
                Height = 22,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(MarkdownWysiwygPalette.ParseHex(hex)),

            };
            button.SetValue(ToolTip.TipProperty, new ToolTip { Content = name + " " + hex });
            button.Click += (_, _) =>
            {
                ApplySpanStyle(foreground ? "color" : "background-color", captured);
                HideSelectionBar();
            };
            row.Children.Add(button);
        }

        return row;
    }
}
