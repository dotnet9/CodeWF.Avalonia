using CodeWF.Avalonia.Lang.MarkupExtensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace CodeWF.Avalonia.Controls.Presentation;

/// <summary>Composes existing controls styled by StudioTheme. Prefix text with @ for a live localization key.</summary>
public static class StudioUi
{
    /// <summary>演示页根：滚动容器 + 限宽堆叠面板。</summary>
    public static ScrollViewer Page(params Control[] children)
    {
        var panel = new StackPanel { Spacing = 12, Margin = new Thickness(16, 14) };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return new ScrollViewer { Content = panel };
    }

    public static TextBlock Heading(string text) => BindText(new TextBlock { Classes = { "studio-heading" } }, TextBlock.TextProperty, text);

    public static TextBlock Sub(string text) => BindText(new TextBlock { Classes = { "studio-description" } }, TextBlock.TextProperty, text);

    public static TextBlock Hint(string text) => BindText(new TextBlock { Classes = { "studio-hint" } }, TextBlock.TextProperty, text);

    public static TextBlock Key(string text) => BindText(new TextBlock { Classes = { "studio-key" } }, TextBlock.TextProperty, text);

    /// <summary>代码/输出块（等宽、可选中复制）。</summary>
    public static SelectableTextBlock Code(string text, bool output = false) =>
        BindText(new SelectableTextBlock { Classes = { output ? "studio-output" : "studio-code" } }, TextBlock.TextProperty, text);

    /// <summary>卡片：可选小标题 + 内容。</summary>
    public static Border Card(string? header, params Control[] content)
    {
        var panel = new StackPanel { Spacing = 8 };
        if (!string.IsNullOrEmpty(header))
        {
            panel.Children.Add(BindText(new TextBlock { FontWeight = FontWeight.SemiBold, FontSize = 12 }, TextBlock.TextProperty, header));
        }

        foreach (var child in content)
        {
            panel.Children.Add(child);
        }

        var border = new Border { Classes = { "studio-card" }, Child = panel };
        return border;
    }

    /// <summary>两列等宽网格（左 / 右）。</summary>
    public static Grid TwoColumns(Control left, Control right)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 12 };
        Grid.SetColumn(left, 0);
        Grid.SetColumn(right, 1);
        grid.Children.Add(left);
        grid.Children.Add(right);
        return grid;
    }

    public static StackPanel Row(params Control[] children)
    {
        var panel = new StackPanel { Classes = { "studio-row" } };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    public static StackPanel RowLoose(double spacing, params Control[] children)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = spacing, VerticalAlignment = VerticalAlignment.Center };
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>小按钮（样式类 b；primary=true 用主色填充）。</summary>
    public static Button Button(string text, Action onClick, bool primary = false)
    {
        var button = BindText(new Button { Classes = { "studio-button" } }, ContentControl.ContentProperty, text);
        if (!primary)
        {
            button.Background = Brushes.Transparent;
        }
        else
        {
            button.Classes.Add("accent");
        }

        button.Click += (_, _) => onClick();
        return button;
    }

    /// <summary>标签 + 控件的一行。</summary>
    public static StackPanel Field(string label, Control control, double labelWidth = 92)
    {
        var text = Key(label);
        text.Width = labelWidth;
        control.VerticalAlignment = VerticalAlignment.Center;
        return RowLoose(8, text, control);
    }

    public static TextBox Input(string text, double width = double.NaN, bool multiline = false)
    {
        var box = new TextBox
        {
            Text = text,
            FontSize = 12,
            MinHeight = multiline ? 64 : 26,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
        };

        if (!double.IsNaN(width))
        {
            box.Width = width;
        }
        else
        {
            box.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        return box;
    }

    public static TextBox Number(double value, double width = 84)
    {
        var box = new TextBox { Text = value.ToString("0.##"), Width = width, FontSize = 12, MinHeight = 26 };
        return box;
    }

    public static ToggleSwitch Switch(bool value) => new() { IsChecked = value, OnContent = null, OffContent = null };

    /// <summary>表头 + 数据行的简易表格（用于参数全览 / 能力矩阵这类只读索引）。</summary>
    public static Grid Table(IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(string.Join(",", Enumerable.Repeat("Auto", headers.Count))),
            RowDefinitions = new RowDefinitions(string.Join(",", Enumerable.Repeat("Auto", rows.Count + 1))),
        };

        for (var column = 0; column < headers.Count; column++)
        {
            var header = BindText(new TextBlock
            {
                FontWeight = FontWeight.SemiBold,
                FontSize = 11,
                Margin = new Thickness(0, 0, 16, 6),
            }, TextBlock.TextProperty, headers[column]);
            Grid.SetRow(header, 0);
            Grid.SetColumn(header, column);
            grid.Children.Add(header);
        }

        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < headers.Count && column < rows[row].Length; column++)
            {
                var cell = BindText(new TextBlock
                {
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = column == 0 ? 0.95 : 0.75,
                    Margin = new Thickness(0, 0, 16, 4),
                }, TextBlock.TextProperty, rows[row][column]);
                Grid.SetRow(cell, row + 1);
                Grid.SetColumn(cell, column);
                grid.Children.Add(cell);
            }
        }

        return grid;
    }

    public static ComboBox Select(IEnumerable<string> options, int selectedIndex = 0, double minWidth = 120)
    {
        var combo = new ComboBox { ItemsSource = options.ToList(), SelectedIndex = selectedIndex, MinWidth = minWidth, FontSize = 12 };
        return combo;
    }
    private static T BindText<T>(T control, AvaloniaProperty property, string text) where T : AvaloniaObject
    {
        if (text.StartsWith("@", StringComparison.Ordinal))
            control.Bind(property, new I18nExtension(text[1..]).CreateBinding());
        else
            control.SetValue(property, text);
        return control;
    }
}
