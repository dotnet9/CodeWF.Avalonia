using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.DataGrid.Themes;
using CodeWF.Avalonia.Dock.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Avalonia.TreeDataGrid.Themes;
using Dock.Avalonia.Controls;
using ReactiveUI.Builder;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.TreeDataGrid;

[Collection("AvaloniaPlatform")]
public sealed class GridPaletteContrastTests(AvaloniaPlatformFixture platform)
{
    static GridPaletteContrastTests() => RxAppBuilder.CreateReactiveUIBuilder().WithCoreServices().BuildApp();

    [Fact]
    public void ExistingGridAndDockControlsExposeTheirActualStateColorsAcrossPalettes() => platform.Run(() =>
    {
        var window = new Window { Width = 1100, Height = 780 };
        window.Styles.Add(new Semi.Avalonia.SemiTheme());
        window.Styles.Add(new ACSemiTheme());
        window.Styles.Add(new DataGridSemiTheme());
        window.Styles.Add(Loaded(new TreeDataGridSemiTheme()));
        window.Styles.Add(new DockSemiTheme());
        window.Styles.Add(new StudioTheme());
        window.Bind(Window.BackgroundProperty, window.GetResourceObservable("StudioBackground"));
        var data = new[] { new Sample("Selected", "Editable"), new Sample("Normal", "Read only") };
        var grid = new global::Avalonia.Controls.DataGrid { ItemsSource = data, AutoGenerateColumns = false, SelectedIndex = 0 };
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding(nameof(Sample.Name)) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Value", Binding = new Binding(nameof(Sample.Value)) });
        var source = new FlatTreeDataGridSource<Sample>(data);
        source.Columns.Add(new TextColumn<Sample, string>("Name", item => item.Name));
        source.Columns.Add(new TextColumn<Sample, string>("Value", item => item.Value));
        source.RowSelection!.SelectedIndex = 0;
        var tree = new global::Avalonia.Controls.TreeDataGrid { Source = source };
        var property = new PropertyGrid { Items = new[] { new PropertyItem { Name = "Selected property", Value = "Editable" }, new PropertyItem { Name = "Read only", Value = "Description", IsReadOnly = true }, new PropertyItem { Name = "Choice", Kind = PropertyItemKind.Choice, Options = ["First", "Second"], Value = "First" }, new PropertyItem { Name = "Color", Kind = PropertyItemKind.Color, Value = "#336699" } } };
        property.Bind(PropertyGrid.ThemeOverridesProperty, property.GetResourceObservable("StudioPropertyGridTheme"));
        var standalone = new PropertyGrid { Items = new[] { new PropertyItem { Name = "Standalone", Value = "Value" } } };
        var document = new global::Dock.Model.ReactiveUI.Controls.Document { Title = "Selected document" };
        var documentDock = new global::Dock.Model.ReactiveUI.Controls.DocumentDock { VisibleDockables = new System.Collections.ObjectModel.ObservableCollection<global::Dock.Model.Core.IDockable> { document }, ActiveDockable = document };
        var tabs = new DocumentDockControl { DataContext = documentDock };
        var tool = new global::Dock.Model.ReactiveUI.Controls.Tool { Title = "Selected tool" };
        var toolDock = new global::Dock.Model.ReactiveUI.Controls.ToolDock { VisibleDockables = new System.Collections.ObjectModel.ObservableCollection<global::Dock.Model.Core.IDockable> { tool }, ActiveDockable = tool };
        var tools = new ToolDockControl { DataContext = toolDock };
        var factory = new global::Dock.Model.ReactiveUI.Factory();
        var root = new global::Dock.Model.ReactiveUI.Controls.RootDock { VisibleDockables = factory.CreateList<global::Dock.Model.Core.IDockable>(documentDock, toolDock), ActiveDockable = documentDock };
        factory.InitLayout(root);
        var panel = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), RowDefinitions = new RowDefinitions("*,*,100") };
        Add(grid, 0, 0); Add(tree, 1, 0); Add(property, 0, 1); Add(standalone, 1, 1); Add(tabs, 0, 2); Add(tools, 1, 2);
        window.Content = panel;
        var evidence = new List<object>();
        try
        {
            window.Show();
            window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            // 测量主题切换后的稳定状态，消除过渡动画对取色时机的影响。
            foreach (var control in window.GetVisualDescendants().OfType<Control>()) control.Transitions = null;
            grid.SelectedItem = data[0];
            foreach (var palette in Enum.GetValues<StudioPalette>())
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
                window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                var background = Brush(window, "StudioBackground");
                var row = grid.GetVisualDescendants().OfType<DataGridRow>().First(item => item.IsSelected);
                var header = grid.GetVisualDescendants().OfType<DataGridColumnHeader>().First(item => Equals(item.Content, "Name"));
                var rowSurface = row.GetVisualDescendants().OfType<Border>().Single(item => item.Name == "BackgroundBorder");
                var treeRow = tree.GetVisualDescendants().OfType<TreeDataGridRow>().First(item => item.IsSelected);
                var treeSurface = treeRow.GetVisualDescendants().OfType<Border>().Single(item => item.Name == "RowBorder");
                var selectedName = property.GetVisualDescendants().OfType<Border>().Single(item => item.Classes.Contains("pg-name") && item.Classes.Contains("selected"));
                var nameText = selectedName.GetVisualDescendants().OfType<TextBlock>().First(item => item.Classes.Contains("pg-name"));
                var selectedTab = tabs.GetVisualDescendants().OfType<DocumentTabStripItem>().First();
                var toolHeader = Assert.IsType<Border>(tools.GetVisualDescendants().OfType<Grid>().Single(item => item.Name == "PART_Grip").GetVisualParent());
                var toolChrome = tools.GetVisualDescendants().OfType<ToolChromeControl>().Single();
                var toolTitle = toolHeader.GetVisualDescendants().OfType<TextBlock>().Single(item => item.Name == "PART_Title");
                var closeIcon = tools.GetVisualDescendants().OfType<PathIcon>().Single(item => item.Name == "PART_CloseIcon");
                var pinIcon = tools.GetVisualDescendants().OfType<PathIcon>().Single(item => item.Name == "PART_PinIcon");
                var pgRoot = property.FindControl<Grid>("RootGrid")!;
                var standaloneRoot = standalone.FindControl<Grid>("RootGrid")!;
                var standaloneName = standalone.GetVisualDescendants().OfType<Border>().Single(item => item.Classes.Contains("pg-name") && item.Classes.Contains("selected"));
                var standaloneNameText = standaloneName.GetVisualDescendants().OfType<TextBlock>().First(item => item.Classes.Contains("pg-name"));
                evidence.Add(new { Palette = palette.ToString(), Dark = dark,
                    Background = Describe(background), GridBackground = Describe(grid.Background), GridForeground = Describe(grid.Foreground), GridHeader = Describe(header.Foreground), GridSelected = Describe(rowSurface.Background),
                    GridBodyRatio = Ratio(grid.Foreground, grid.Background, background), GridHeaderRatio = Ratio(header.Foreground, grid.Background, background), GridSelectedRatio = Ratio(grid.Foreground, rowSurface.Background, grid.Background, background),
                    TreeForeground = Describe(tree.Foreground), TreeSelected = Describe(treeSurface.Background), TreeSelectedRatio = Ratio(tree.Foreground, treeSurface.Background, tree.Background, background),
                    PropertyBackground = Describe(pgRoot.Background), PropertySelected = Describe(selectedName.Background), PropertySelectedForeground = Describe(nameText.Foreground), PropertySelectedRatio = Ratio(nameText.Foreground, selectedName.Background, pgRoot.Background, background),
                    StandaloneBackground = Describe(standaloneRoot.Background), DocumentForeground = Describe(selectedTab.Foreground), DocumentBackground = Describe(selectedTab.Background), DocumentRatio = Ratio(selectedTab.Foreground, selectedTab.Background, background),
                    StandaloneSelectedRatio = Ratio(standaloneNameText.Foreground, standaloneName.Background, standaloneRoot.Background, background),
                    ToolForeground = Describe(toolTitle.Foreground), ToolBackground = Describe(toolHeader.Background), ToolRatio = Ratio(toolTitle.Foreground, toolHeader.Background, background),
                    CloseIconRatio = Ratio(closeIcon.Foreground, toolHeader.Background, background), PinIconRatio = Ratio(pinIcon.Foreground, toolHeader.Background, background) });
                Capture(window, palette + (dark ? "-dark" : "-light"));
                var label = palette + (dark ? " dark" : " light");
                Assert.Equal(Describe(Brush(window, "StudioSurface")), Describe(grid.Background));
                Assert.Equal(Describe(grid.Background), Describe(tree.Background));
                Assert.Equal(Describe(grid.Background), Describe(toolHeader.Background));
                Readable(label + " grid body", 4.5, grid.Foreground, grid.Background, background);
                Readable(label + " grid header", 4.5, header.Foreground, header.Background, grid.Background, background);
                Readable(label + " grid selected", 4.5, grid.Foreground, rowSurface.Background, grid.Background, background);
                Readable(label + " tree selected", 4.5, tree.Foreground, treeSurface.Background, tree.Background, background);
                Readable(label + " property selected", 4.5, nameText.Foreground, selectedName.Background, pgRoot.Background, background);
                Readable(label + " standalone selected", 4.5, standaloneNameText.Foreground, standaloneName.Background, standaloneRoot.Background, background);
                foreach (var readOnly in property.GetVisualDescendants().OfType<TextBlock>().Where(item => item.Classes.Contains("pg-readonly")))
                    Readable(label + " read only", 4.5, readOnly.Foreground, pgRoot.Background, background);
                Readable(label + " document", 4.5, selectedTab.Foreground, selectedTab.Background, background);
                Readable(label + " tool title", 4.5, toolTitle.Foreground, toolHeader.Background, background);
                Assert.NotNull(closeIcon.Data); Assert.NotNull(pinIcon.Data);
                foreach (var active in new[] { false, true })
                foreach (var pinned in new[] { false, true })
                {
                    toolChrome.IsActive = active;
                    toolChrome.IsPinned = pinned;
                    Dispatcher.UIThread.RunJobs();
                    Readable(label + " close icon", 3, closeIcon.Foreground, toolHeader.Background, background);
                    Readable(label + " pin icon", 3, pinIcon.Foreground, toolHeader.Background, background);
                }
                toolChrome.IsPinned = false;
                Hover(header);
                Readable(label + " grid header hover", 4.5, header.Foreground, header.Background, grid.Background, background);
                var treeHeader = tree.GetVisualDescendants().OfType<TreeDataGridColumnHeader>().First();
                Hover(treeHeader);
                Readable(label + " tree header hover", 4.5, treeHeader.Foreground, Brush(tree, "TreeDataGridHeaderPointerOverBackground"), tree.Background, background);
                window.MouseDown(treeHeader.TranslatePoint(new Point(12, 12), window)!.Value, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                Readable(label + " tree header pressed", 4.5, treeHeader.Foreground, Brush(tree, "TreeDataGridHeaderPressedBackground"), tree.Background, background);
                window.MouseUp(treeHeader.TranslatePoint(new Point(12, 12), window)!.Value, MouseButton.Left);
                window.MouseMove(new Point(1090, 770));
                grid.IsEnabled = false; tree.IsEnabled = false; tools.IsEnabled = false;
                Dispatcher.UIThread.RunJobs();
                Assert.False(row.IsEffectivelyEnabled); Assert.False(treeRow.IsEffectivelyEnabled); Assert.False(closeIcon.IsEffectivelyEnabled);
                Assert.NotNull(grid.Background); Assert.NotNull(tree.Foreground); Assert.NotNull(closeIcon.Foreground);
                grid.IsEnabled = true; tree.IsEnabled = true; tools.IsEnabled = true;
                var choice = property.GetVisualDescendants().OfType<ComboBox>().Single();
                choice.IsDropDownOpen = true;
                window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                var choicePopup = choice.GetVisualDescendants().OfType<Popup>().Single();
                Assert.True(choicePopup.IsOpen);
                Assert.Equal(window.ActualThemeVariant, choicePopup.Child!.ActualThemeVariant);
                Readable(label + " choice popup", 4.5, Brush(choice, "ComboBoxItemForeground"), Brush(choice, "ComboBoxPopupBackground"), background);
                choice.IsDropDownOpen = false;
                var colorPopup = property.GetLogicalDescendants().OfType<Popup>().Single();
                colorPopup.IsOpen = true;
                window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                Assert.Equal(window.ActualThemeVariant, colorPopup.Child!.ActualThemeVariant);
                colorPopup.IsOpen = false;

                void Hover(Control target)
                {
                    window.MouseMove(target.TranslatePoint(new Point(12, 12), window)!.Value);
                    window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                }
            }
        }
        finally
        {
            var directory = Environment.GetEnvironmentVariable("CODEWF_GRID_THEME_EVIDENCE");
            if (!string.IsNullOrWhiteSpace(directory)) { Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, "colors.json"), JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true })); }
            window.Close();
        }
        Assert.Equal(32, evidence.Count);
        void Add(Control control, int column, int row) { Grid.SetColumn(control, column); Grid.SetRow(control, row); panel.Children.Add(control); }
    });

    [Fact]
    public void TreeHeaderStateResourcesReachItsRenderedText() => platform.Run(() =>
    {
        var source = new FlatTreeDataGridSource<Sample>([new("One", "Value")]);
        source.Columns.Add(new TextColumn<Sample, string>("Name", item => item.Name));
        var tree = new global::Avalonia.Controls.TreeDataGrid { Source = source };
        var window = new Window { Width = 320, Height = 200, Content = tree, RequestedThemeVariant = ThemeVariant.Light };
        window.Styles.Add(new Semi.Avalonia.SemiTheme());
        window.Styles.Add(Loaded(new TreeDataGridSemiTheme()));
        window.Resources["TreeDataGridHeaderPointerOverForeground"] = Brushes.DarkGreen;
        window.Resources["TreeDataGridHeaderPressedForeground"] = Brushes.DarkRed;
        tree.Theme = Assert.IsType<ControlTheme>(window.FindResource(typeof(global::Avalonia.Controls.TreeDataGrid)));
        try
        {
            window.Show(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var header = tree.GetVisualDescendants().OfType<TreeDataGridColumnHeader>().Single();
            var text = header.GetVisualDescendants().OfType<TextBlock>().Single(item => item.Text == "Name");
            var point = header.TranslatePoint(new Point(12, 12), window)!.Value;
            window.MouseMove(point); Dispatcher.UIThread.RunJobs();
            Assert.Equal(Describe(Brushes.DarkGreen), Describe(text.Foreground));
            window.MouseDown(point, MouseButton.Left); Dispatcher.UIThread.RunJobs();
            Assert.Equal(Describe(Brushes.DarkRed), Describe(text.Foreground));
            window.MouseUp(point, MouseButton.Left);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void FloatingToolWindowFollowsApplicationPaletteSwitches() => platform.Run(() =>
    {
        var app = Application.Current!;
        var originalVariant = app.RequestedThemeVariant;
        var styles = new Styles { new Semi.Avalonia.SemiTheme(), Loaded(new ACSemiTheme()), Loaded(new DockSemiTheme()), new StudioTheme() };
        app.Styles.Add(styles);
        app.RequestedThemeVariant = ThemeVariant.Light;
        var tool = new global::Dock.Model.ReactiveUI.Controls.Tool { Title = "Floating tool" };
        var factory = new global::Dock.Model.ReactiveUI.Factory();
        var dock = new global::Dock.Model.ReactiveUI.Controls.ToolDock { VisibleDockables = factory.CreateList<global::Dock.Model.Core.IDockable>(tool), ActiveDockable = tool };
        var root = new global::Dock.Model.ReactiveUI.Controls.RootDock { VisibleDockables = factory.CreateList<global::Dock.Model.Core.IDockable>(dock), ActiveDockable = dock };
        factory.InitLayout(root);
        var dockWindow = new global::Dock.Model.ReactiveUI.Core.DockWindow { Factory = factory, Layout = root };
        var floating = new HostWindow { Width = 420, Height = 160, Window = dockWindow, IsToolWindow = true };
        dockWindow.Host = floating;
        floating.DataTemplates.Add(new FuncDataTemplate<global::Dock.Model.Controls.IRootDock>((_, _) => new ToolDockControl { DataContext = dock }));
        try
        {
            floating.Show(); floating.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            foreach (var control in floating.GetVisualDescendants().OfType<Control>()) control.Transitions = null;
            foreach (var palette in Enum.GetValues<StudioPalette>())
            foreach (var dark in new[] { false, true })
            {
                app.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
                floating.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                Assert.Equal(app.ActualThemeVariant, floating.ActualThemeVariant);
                var header = Assert.IsType<Border>(floating.GetVisualDescendants().OfType<Grid>().Single(item => item.Name == "PART_Grip").GetVisualParent());
                var title = header.GetVisualDescendants().OfType<TextBlock>().Single(item => item.Name == "PART_Title");
                Assert.Equal(Describe(Brush(floating, "StudioSurface")), Describe(header.Background));
                Readable("floating title", 4.5, title.Foreground, header.Background, floating.Background);
                foreach (var icon in header.GetVisualDescendants().OfType<PathIcon>())
                    Readable("floating tool icon", 3, icon.Foreground, header.Background, floating.Background);
                Capture(floating, "floating-" + palette + (dark ? "-dark" : "-light"));
            }
        }
        finally { floating.Close(); app.RequestedThemeVariant = originalVariant; app.Styles.Remove(styles); }
    });

    private static T Loaded<T>(T theme) where T : Styles
    {
        if (theme.Count == 0) AvaloniaXamlLoader.Load(theme);
        return theme;
    }

    private static IBrush Brush(Control control, string key)
    {
        Assert.True(control.TryFindResource(key, control.ActualThemeVariant, out var value));
        return Assert.IsAssignableFrom<IBrush>(value);
    }
    private static string Describe(IBrush? brush) => brush is ISolidColorBrush solid ? solid.Color + "/" + solid.Opacity.ToString(System.Globalization.CultureInfo.InvariantCulture) : brush?.ToString() ?? "null";
    private static void Readable(string label, double minimum, IBrush? foreground, params IBrush?[] backgrounds)
    {
        var ratio = Ratio(foreground, backgrounds);
        Assert.True(ratio >= minimum, $"{label}: {ratio:F2}:1 < {minimum:F1}:1; {Describe(foreground)} on {string.Join(", ", backgrounds.Select(Describe))}");
    }
    private static double Ratio(IBrush? foreground, params IBrush?[] backgrounds)
    {
        var color = Colors.White;
        foreach (var brush in backgrounds.Reverse()) color = Over(brush, color);
        var text = Over(foreground, color);
        var first = Luminance(color); var second = Luminance(text);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }
    private static Color Over(IBrush? brush, Color background)
    {
        if (brush is not ISolidColorBrush solid) return background;
        var opacity = solid.Color.A / 255d * solid.Opacity;
        byte Blend(byte value, byte behind) => (byte)Math.Round(value * opacity + behind * (1 - opacity));
        return Color.FromRgb(Blend(solid.Color.R, background.R), Blend(solid.Color.G, background.G), Blend(solid.Color.B, background.B));
    }
    private static double Luminance(Color color)
    {
        static double Channel(byte channel) { var value = channel / 255d; return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4); }
        return .2126 * Channel(color.R) + .7152 * Channel(color.G) + .0722 * Channel(color.B);
    }
    private static void Capture(Window window, string name)
    {
        var directory = Environment.GetEnvironmentVariable("CODEWF_GRID_THEME_EVIDENCE");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        using var bitmap = window.CaptureRenderedFrame();
        bitmap?.Save(Path.Combine(directory, name + ".png"), new PngBitmapEncoderOptions());
    }
    private sealed record Sample(string Name, string Value);
}
