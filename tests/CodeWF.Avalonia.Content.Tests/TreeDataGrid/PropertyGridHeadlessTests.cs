using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

using Xunit;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaPath = Avalonia.Controls.Shapes.Path;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace CodeWF.Avalonia.Content.Tests.TreeDataGrid;

/// <summary>
/// PropertyGrid 无头实例化回归：控件 XAML 运行时加载、可见行重建、选中与提交流程。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class PropertyGridHeadlessTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public PropertyGridHeadlessTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void EditorsFillCellsAndPointerDragsResizeColumnsAndRows() => _platform.Run(() =>
    {
        var theme = new Semi.Avalonia.SemiTheme();
        Application.Current!.Styles.Add(theme);
        var text = new PropertyItem { Id = "text", Name = "Title", Kind = PropertyItemKind.Text, Value = "Sample" };
        var choice = new PropertyItem { Id = "choice", Name = "Mode", Kind = PropertyItemKind.Choice, Options = ["Light", "Dark"], Value = "Light" };
        var grid = new PropertyGrid { Items = new[] { text, choice }, NameColumnWidth = 140 };
        var window = new Window { Width = 480, Height = 360, Content = grid };
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var editors = grid.GetVisualDescendants().OfType<PropertyCellEditor>().ToArray();
            var input = Assert.IsType<TextBox>(editors.Single(editor => ((PropertyLeafRow)editor.DataContext!).Item == text).Content);
            var combo = Assert.IsType<ComboBox>(editors.Single(editor => ((PropertyLeafRow)editor.DataContext!).Item == choice).Content);
            foreach (var editor in new Control[] { input, combo })
            {
                Assert.Equal(HorizontalAlignment.Stretch, editor.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Stretch, editor.VerticalAlignment);
                var parent = Assert.IsAssignableFrom<Control>(editor.Parent);
                Assert.Equal(parent.Bounds.Width, editor.Bounds.Width, 1);
                Assert.Equal(parent.Bounds.Height, editor.Bounds.Height, 1);
            }
            Assert.Equal(new Thickness(0), input.BorderThickness);
            Assert.Equal(new Thickness(0), combo.BorderThickness);
            Assert.Equal(HorizontalAlignment.Left, combo.HorizontalContentAlignment);
            Assert.NotNull(grid.FindControl<ToggleButton>("CategorizedButton")!.GetVisualDescendants().OfType<PathIcon>().Single().Data);
            var search = grid.FindControl<TextBox>("SearchBox")!;
            Assert.True(search.TranslatePoint(default, grid)!.Value.Y > grid.FindControl<ToggleButton>("CategorizedButton")!.Bounds.Height);

            var column = grid.GetVisualDescendants().OfType<Border>().First(border => border.Classes.Contains("pg-col-handle"));
            Drag(column, new Point(3, 10), new Vector(40, 0));
            Assert.Equal(180, grid.NameColumnWidth, 1);
            var rowLine = grid.GetVisualDescendants().OfType<Border>().Single(border => border.Classes.Contains("pg-row-line") && ReferenceEquals(border.Tag, text));
            var initialHeight = Assert.IsAssignableFrom<Control>(rowLine.Parent).Bounds.Height;
            Drag(rowLine, new Point(15, 3), new Vector(0, 20));
            Assert.Equal(initialHeight + 20, text.RowHeight!.Value, 1);

            void Drag(Control handle, Point offset, Vector delta)
            {
                var start = handle.TranslatePoint(offset, window)!.Value;
                window.MouseMove(start);
                window.MouseDown(start, MouseButton.Left);
                window.MouseMove(start + delta);
                window.MouseUp(start + delta, MouseButton.Left);
                window.UpdateLayout();
            }
        }
        finally
        {
            window.Close();
            Application.Current.Styles.Remove(theme);
        }
    });

    [Fact]
    public void ReplacingItems_PreservesSelectionById_AndUsesTheNewDescription()
    {
        _platform.Run(() =>
        {
            var original = new PropertyItem { Id = "appearance", Name = "Appearance", Kind = PropertyItemKind.Text, Description = "Original" };
            var replacement = new PropertyItem { Id = "appearance", Name = "Appearance", Kind = PropertyItemKind.Text, Description = "Translated" };
            var grid = new PropertyGrid { Items = new[] { original } };
            grid.Items = new[] { replacement };
            Assert.Same(replacement, grid.SelectedItem);
            Assert.Equal("Translated", grid.SelectedItem.Description);
            Assert.False(original.IsSelected);
        });
    }

    [Fact]
    public void ExistingEditors_FollowProgrammaticValuesWithoutRebuildingRows() => _platform.Run(() =>
    {
        var item = new PropertyItem { Kind = PropertyItemKind.Text, Value = "Before" };
        var editor = new PropertyCellEditor { DataContext = new PropertyLeafRow(item, 0, false) };
        var text = Assert.IsType<TextBox>(editor.Content);
        item.Value = "After";
        Assert.Same(text, editor.Content);
        Assert.Equal("After", text.Text);

        item = new PropertyItem { Kind = PropertyItemKind.Path, Value = "before.md" };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var path = Assert.IsType<DockPanel>(editor.Content).Children.OfType<TextBox>().Single();
        item.Value = "after.md";
        Assert.Equal("after.md", path.Text);

        item = new PropertyItem { Kind = PropertyItemKind.Number, Value = 1m };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var number = Assert.IsType<NumericUpDown>(editor.Content);
        item.Value = 12m;
        Assert.Equal(12m, number.Value);

        item = new PropertyItem { Kind = PropertyItemKind.Boolean, Value = false };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var check = Assert.IsType<CheckBox>(editor.Content);
        item.Value = true;
        Assert.True(check.IsChecked);

        item = new PropertyItem { Kind = PropertyItemKind.Percent, Value = 10d };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var percent = Assert.IsType<DockPanel>(editor.Content);
        item.Value = 80d;
        Assert.Equal(80d, percent.Children.OfType<Slider>().Single().Value);
        Assert.Equal("80%", percent.Children.OfType<TextBlock>().Single().Text);

        item = new PropertyItem { Kind = PropertyItemKind.DateTime, Value = new DateTime(2026, 10, 15, 10, 0, 0) };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var date = Assert.IsType<TextBox>(editor.Content);
        item.Value = new DateTime(2026, 10, 16, 12, 30, 0);
        Assert.Equal("2026/10/16 12:30", date.Text);

        item = new PropertyItem { Kind = PropertyItemKind.Color, Value = "#000000" };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var color = Assert.IsType<StackPanel>(editor.Content);
        item.Value = "#FFFFFF";
        Assert.Equal(Colors.White, Assert.IsAssignableFrom<ISolidColorBrush>(color.Children.OfType<Border>().Single().Background).Color);
        Assert.Equal("#FFFFFF", color.Children.OfType<TextBlock>().Single().Text);
        Assert.Equal(Colors.White, Assert.IsType<ColorPicker>(color.Children.OfType<Popup>().Single().Child).Color);

        item = new PropertyItem { IsReadOnly = true, Value = "Before" };
        editor.DataContext = new PropertyLeafRow(item, 0, false);
        var readOnly = Assert.IsType<TextBlock>(editor.Content);
        item.Value = "After";
        Assert.Equal("After", readOnly.Text);
        Assert.Equal("After", ToolTip.GetTip(readOnly));
    });

    [Fact]
    public void CanceledEdits_RestoreControlsAndExternalUpdatesDoNotCommit() => _platform.Run(() =>
    {
        var theme = new Semi.Avalonia.SemiTheme();
        Application.Current!.Styles.Add(theme);
        var items = new[]
        {
            new PropertyItem { Kind = PropertyItemKind.Text, Value = "Before" },
            new PropertyItem { Kind = PropertyItemKind.Number, Value = 1m },
            new PropertyItem { Kind = PropertyItemKind.Boolean, Value = false },
            new PropertyItem { Kind = PropertyItemKind.Choice, Options = ["Light", "Dark"], Value = "Light" },
            new PropertyItem { Kind = PropertyItemKind.Radio, Options = ["Light", "Dark"], Value = "Light" },
            new PropertyItem { Kind = PropertyItemKind.Percent, Value = 10d },
            new PropertyItem { Kind = PropertyItemKind.Color, Value = "#000000" },
        };
        var grid = new PropertyGrid { Items = items };
        var window = new Window { Width = 600, Height = 700, Content = grid };
        var previews = 0;
        var commits = 0;
        grid.ValuePreviewing += (_, e) => { previews++; e.Cancel = true; };
        grid.ValueCommitted += (_, _) => commits++;
        try
        {
            window.Show();
            window.UpdateLayout();
            var editors = grid.GetVisualDescendants().OfType<PropertyCellEditor>().ToArray();
            Control Editor(int index) => Assert.Single(editors, editor => ReferenceEquals(((PropertyLeafRow)editor.DataContext!).Item, items[index])).Content as Control
                ?? throw new InvalidOperationException();
            var text = Assert.IsType<TextBox>(Editor(0));
            text.Text = "Rejected";
            text.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter });
            Assert.Equal("Before", text.Text);
            var number = Assert.IsType<NumericUpDown>(Editor(1));
            number.Value = 12m;
            Assert.Equal(1m, number.Value);
            var check = Assert.IsType<CheckBox>(Editor(2));
            check.IsChecked = true;
            Assert.False(check.IsChecked);
            var combo = Assert.IsType<ComboBox>(Editor(3));
            combo.SelectedItem = "Dark";
            Assert.Equal("Light", combo.SelectedItem);
            var radios = Assert.IsType<StackPanel>(Editor(4)).Children.OfType<RadioButton>().ToArray();
            radios[1].IsChecked = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(radios[0].IsChecked);
            Assert.False(radios[1].IsChecked);
            var slider = Assert.IsType<DockPanel>(Editor(5)).Children.OfType<Slider>().Single();
            slider.Value = 80d;
            Assert.Equal(10d, slider.Value);
            var picker = Assert.IsType<ColorPicker>(Assert.IsType<StackPanel>(Editor(6)).Children.OfType<Popup>().Single().Child);
            picker.Color = Colors.White;
            Assert.Equal(Colors.Black, picker.Color);
            Assert.Equal(7, previews);

            items[0].Value = "External";
            items[1].Value = 23m;
            items[2].Value = true;
            items[3].Value = "Dark";
            items[4].Value = "Dark";
            items[5].Value = 70d;
            items[6].Value = "#FFFFFF";
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("External", text.Text);
            Assert.Equal(23m, number.Value);
            Assert.True(check.IsChecked);
            Assert.Equal("Dark", combo.SelectedItem);
            Assert.True(radios[1].IsChecked);
            Assert.Equal(70d, slider.Value);
            Assert.Equal(Colors.White, picker.Color);
            Assert.Equal(7, previews);
            Assert.Equal(0, commits);
        }
        finally
        {
            window.Close();
            Application.Current.Styles.Remove(theme);
        }
    });

    [Fact]
    public void ReusedSummaryEditor_UnsubscribesFromItsPreviousItem() => _platform.Run(() =>
    {
        var original = new PropertyItem { Children = [new PropertyItem()], Summary = "Before" };
        var editor = new PropertyCellEditor { DataContext = new PropertyLeafRow(original, 0, false) };
        var oldSummary = Assert.IsType<TextBlock>(editor.Content);
        editor.DataContext = null;
        original.Summary = "After";
        Assert.Equal("Before", oldSummary.Text);
    });

    [Fact]
    public void ChoiceAndRadioEditors_FollowExternalValueChanges()
    {
        _platform.Run(() =>
        {
            var item = new PropertyItem { Kind = PropertyItemKind.Choice, Options = ["Light", "Dark"], Value = "Light" };
            var editor = new PropertyCellEditor { DataContext = new PropertyLeafRow(item, 0, false) };
            var combo = Assert.IsType<ComboBox>(editor.Content);
            Assert.Equal("Light", combo.SelectedItem);
            item.Value = "Dark";
            Assert.Equal("Dark", combo.SelectedItem);

            var mode = new PropertyItem { Kind = PropertyItemKind.Radio, Options = ["Light", "Dark"], Value = "Light" };
            editor.DataContext = new PropertyLeafRow(mode, 0, false);
            var panel = Assert.IsType<StackPanel>(editor.Content);
            var buttons = panel.Children.OfType<RadioButton>().ToArray();
            Assert.True(buttons[0].IsChecked);
            mode.Value = "Dark";
            Assert.False(buttons[0].IsChecked);
            Assert.True(buttons[1].IsChecked);
        });
    }

    [Fact]
    public void HostThemeOverrides_CanBeReplaced_Shared_AndRemoved()
    {
        _platform.Run(() =>
        {
            var first = new PropertyGrid();
            var second = new PropertyGrid();
            var palette = new ResourceDictionary { ["PgAccent"] = Brushes.Purple };
            first.ThemeOverrides = palette;
            second.ThemeOverrides = palette;
            Assert.True(first.Resources.TryGetResource("PgAccent", ThemeVariant.Dark, out var accent));
            Assert.Same(Brushes.Purple, accent);
            first.ThemeOverrides = new ResourceDictionary { ["PgAccent"] = Brushes.Teal };
            Assert.True(first.Resources.TryGetResource("PgAccent", ThemeVariant.Dark, out accent));
            Assert.Same(Brushes.Teal, accent);
            Assert.True(second.Resources.TryGetResource("PgAccent", ThemeVariant.Dark, out accent));
            Assert.Same(Brushes.Purple, accent);
            first.ThemeOverrides = null;
            Assert.True(first.Resources.TryGetResource("PgAccent", ThemeVariant.Dark, out accent));
            Assert.Equal(Color.Parse("#0F6FDF"), Assert.IsType<SolidColorBrush>(accent).Color);
        });
    }

    [Fact]
    public void PropertyGridResources_AreEmbeddedAndResolvable()
    {
        Assert.Contains(typeof(PropertyGrid).Assembly.GetManifestResourceNames(), name => name.EndsWith("zh-CN.json"));
        Assert.NotEqual("Localization.CodeWF_Avalonia_TreeDataGrid.PropertyGrid.SearchPlaceholder",
            PropertyGridLocalization.Get("Localization.CodeWF_Avalonia_TreeDataGrid.PropertyGrid.SearchPlaceholder"));
    }

    [Fact]
    public void PropertyGrid_Instantiates_Rebuilds_Selects_AndCommits()
    {
        _platform.Run(() =>
        {
            var grid = new PropertyGrid();
            var name = new PropertyItem { Name = "名称", Kind = PropertyItemKind.Text, Value = "阀门-01", DefaultValue = "" };
            var appearance = new PropertyItem
            {
                Name = "外观",
                Kind = PropertyItemKind.Category,
                Children = [name],
            };

            grid.Items = new PropertyItem[] { appearance };

            // 分类视图：分组行 + 属性行
            Assert.Equal(2, grid.Rows.Count);
            Assert.Equal(name, grid.SelectedItem); // 自动选中首个属性行

            // 选中与描述面板数据源
            grid.Select(name);
            Assert.Equal(name, grid.SelectedItem);
            Assert.True(name.IsSelected);

            // 提交：预览事件可取消
            PropertyValuePreviewEventArgs? previewArgs = null;
            EventHandler<PropertyValuePreviewEventArgs> cancelHandler = (_, e) =>
            {
                previewArgs = e;
                e.Cancel = true;
            };
            grid.ValuePreviewing += cancelHandler;
            Assert.False(grid.CommitValue(name, "被拒绝的值"));
            Assert.NotNull(previewArgs);
            Assert.NotEqual("被拒绝的值", name.Value);

            grid.ValuePreviewing -= cancelHandler;
            var committed = new List<PropertyValueChangedEventArgs>();
            grid.ValueCommitted += (_, e) => committed.Add(e);
            Assert.True(grid.CommitValue(name, "新名称"));
            Assert.Equal("新名称", name.Value);
            Assert.True(name.IsModified);
            Assert.Single(committed);
            Assert.Equal("阀门-01", committed[0].OldValue);

            // 字母视图：摊平为单行
            grid.ViewMode = PropertyGridViewMode.Alphabetical;
            Assert.Single(grid.Rows);

            // 搜索过滤
            grid.SearchText = "不存在的属性";
            Assert.Empty(grid.Rows);
            grid.SearchText = null;

            // 展开折叠与重置
            grid.ViewMode = PropertyGridViewMode.Categorized;
            grid.CollapseAll();
            Assert.Single(grid.Rows);
            grid.ExpandAll();
            Assert.Equal(2, grid.Rows.Count);
            grid.ResetAll();
            Assert.Equal(string.Empty, name.Value); // 默认值为空串，重置后回到默认
            Assert.False(name.IsModified);
        });
    }

    [Fact]
    public void PropertyGrid_DynamicVisibility_RowsOnRefresh()
    {
        _platform.Run(() =>
        {
            var hidden = new PropertyItem { Name = "退避系数", Kind = PropertyItemKind.Number, Value = 1.6m, DefaultValue = 1.5m };
            var toggle = new PropertyItem { Name = "指数退避", Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false };
            hidden.VisibleWhen = () => Equals(toggle.Value, true);

            var grid = new PropertyGrid
            {
                Items = new PropertyItem[] { hidden, toggle },
            };

            Assert.Single(grid.Rows); // 退避系数被隐藏
            toggle.Value = true;
            grid.Refresh();
            Assert.Equal(2, grid.Rows.Count);
        });
    }

    [Fact]
    public void ExpandersSitAtRowEndAndToggleCollapse() => _platform.Run(() =>
    {
        var theme = new Semi.Avalonia.SemiTheme();
        Application.Current!.Styles.Add(theme);
        var name = new PropertyItem { Name = "名称", Kind = PropertyItemKind.Text, Value = "阀门-01", DefaultValue = "" };
        var family = new PropertyItem { Name = "字体族", Kind = PropertyItemKind.Text, Value = "微软雅黑", DefaultValue = "微软雅黑" };
        var font = new PropertyItem { Name = "字体", Kind = PropertyItemKind.Text, Value = "微软雅黑", DefaultValue = "微软雅黑", Children = [family] };
        var group = new PropertyItem { Name = "外观", Kind = PropertyItemKind.Category, Children = [name, font] };
        var grid = new PropertyGrid { Items = new[] { group }, NameColumnWidth = 140 };
        var window = new Window { Width = 480, Height = 360, Content = grid };
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            // 层级参考线已移除，缩进为基础 12px 加每级 12px
            Assert.DoesNotContain(grid.GetVisualDescendants().OfType<ItemsControl>(), c => c.Classes.Contains("pg-guides"));
            var leafRow = grid.GetVisualDescendants().OfType<Grid>().First(g => g.Classes.Contains("pg-row") && g.DataContext is PropertyLeafRow leaf && ReferenceEquals(leaf.Item, name));
            var nameCell = leafRow.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("pg-name"));
            var indent = Assert.IsType<StackPanel>(nameCell.GetVisualDescendants().OfType<StackPanel>().First());
            Assert.Equal(new Thickness(24, 0, 8, 0), indent.Margin);

            // 展开按钮只在行尾：无子项的名称格内没有，字体行的箭头右缘贴合行右缘
            var fontRow = grid.GetVisualDescendants().OfType<Grid>().First(g => g.Classes.Contains("pg-row") && g.DataContext is PropertyLeafRow leaf && ReferenceEquals(leaf.Item, font));
            var fontNameCell = fontRow.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("pg-name"));
            Assert.DoesNotContain(fontNameCell.GetVisualDescendants(), c => c.Classes.Contains("pg-expander"));
            var leafExpander = fontRow.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("pg-expander"));
            var expanderRight = leafExpander.TranslatePoint(new Point(leafExpander.Bounds.Width, 0), fontRow)!.Value.X;
            Assert.Equal(fontRow.Bounds.Width, expanderRight, 1);
            Assert.NotNull(leafExpander.GetVisualDescendants().OfType<AvaloniaPath>().Single().Data);

            // 点击分组行尾箭头折叠：只剩分组行；再点展开恢复
            var groupExpander = grid.GetVisualDescendants().OfType<Border>()
                .First(b => b.Classes.Contains("pg-expander") && b.DataContext is PropertyCategoryRow);
            var center = groupExpander.TranslatePoint(new Point(groupExpander.Bounds.Width / 2, groupExpander.Bounds.Height / 2), window)!.Value;
            window.MouseMove(center);
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Single(grid.Rows);
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(4, grid.Rows.Count); // 外观、名称、字体与展开的字体族
        }
        finally
        {
            window.Close();
        }
    });
}
