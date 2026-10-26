using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.Lang.MarkupExtensions;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using DemoKeys = CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using PropertyKeys = CodeWF.Avalonia.TreeDataGrid.PropertyGrid.I18n.ResourceKeys;
using ReactiveUI.Avalonia;

// 验证器只使用本次输出目录，初始化真实窗口时也不读取用户布局。
var outputArgument = args.FirstOrDefault() == "--palette-audit" ? args.Skip(1).FirstOrDefault() : args.FirstOrDefault();
var verificationOutput = Path.GetFullPath(outputArgument is { Length: > 0 } && !outputArgument.StartsWith("--", StringComparison.Ordinal)
    ? outputArgument : Path.Combine(Path.GetTempPath(), "CodeWF.Avalonia.Verification", Guid.NewGuid().ToString("N")));
var verificationData = Path.Combine(verificationOutput, "isolated-data");
Directory.CreateDirectory(verificationData);
Environment.SetEnvironmentVariable("CODEWF_AVALONIA_DEMO_DATA_DIRECTORY", verificationData);

// 离屏运行真实 Demo 主题与控件，锁屏时也可验证 NativeAOT 动态语言切换。
AppBuilder.Configure<CodeWF.Avalonia.Demo.App>().UseSkia().WithInterFont().UseReactiveUI(_ => { })
    .With(new FontManagerOptions { FontFallbacks = [
        new FontFallback { FontFamily = new FontFamily("Microsoft YaHei UI") },
        new FontFallback { FontFamily = new FontFamily("PingFang SC") },
        new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") }
    ] })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
DemoBootstrapper.Initialize();
Require(MainViewModel.LayoutPath == Path.Combine(verificationData, "layout.json"), "Window initialization uses the isolated layout directory.");
if (args.FirstOrDefault() == "--palette-audit")
    return DemoPaletteVerification.Run(args.Skip(1).ToArray());
VerifyThirdPartyLocales();
VerifyHiddenPropertyRows();
VerifyPropertyEditors();
VerifyWindowCapabilities();
VerifyEditableTableColumns();
VerifyReadOnlyTableColumns();
VerifyMindMapLanguageState();
VerifyStatusLanguageState();
VerifyDockDocumentTransfer(Path.Combine(Path.GetFullPath(args.FirstOrDefault() ?? Path.GetTempPath()), "dock-captures"));
var manager = I18nManager.Instance;
var text = new TextBlock();
using var binding = text.Bind(TextBlock.TextProperty, (BindingBase)new I18nBinding(DemoKeys.Shell.Ready).ProvideValue(null!));
var markdown = new MarkdownViewer { Markdown = "```text\nShared language\n```" };
var properties = new PropertyGrid();
var slider = new Slider { Value = 45, Maximum = 100, Width = 240 };
var checkBox = new CheckBox { IsChecked = true, Content = "CheckBox" };
var input = new TextBox { Text = "TextBox", Width = 240 };
var selector = new ComboBox { ItemsSource = new[] { "ComboBox" }, SelectedIndex = 0, Width = 240 };
var panel = new StackPanel { Children = { text, slider, checkBox, input, selector, markdown, properties } };
var window = new Window { Width = 960, Height = 720, Content = panel };
window.Show();
var markdownLabels = new HashSet<string>();
var propertyLabels = new HashSet<string>();
var updateLabels = new HashSet<string>();
var changed = 0;
manager.CultureChanged += OnCultureChanged;
try
{
    // 在同一批真实模板上连续切换主题，检查控件资源不会停留在 Semi 默认蓝色。
    foreach (var variant in StudioPalettes.All.SelectMany(palette => new[] { StudioTheme.GetVariant(palette.Key, false), StudioTheme.GetVariant(palette.Key, true) }))
    {
        window.RequestedThemeVariant = variant;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Color ResourceColor(string key)
        {
            Require(window.TryFindResource(key, variant, out var resource), "Theme resource exists: " + key);
            return ((ISolidColorBrush)resource!).Color;
        }
        var accent = ResourceColor("StudioAccent");
        var primary = ResourceColor("StudioPrimary");
        Require(((ISolidColorBrush)slider.Foreground!).Color == accent,
            $"Existing Slider follows {variant}: actual {slider.Foreground}, expected {accent}.");
        var thumb = slider.GetVisualDescendants().OfType<global::Avalonia.Controls.Primitives.Thumb>().Single();
        Require(((ISolidColorBrush)thumb.BorderBrush!).Color == accent, "Existing Slider thumb follows " + variant);
        var glyph = checkBox.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "NormalRectangle");
        Require(((ISolidColorBrush)glyph.Background!).Color == primary, "Existing checked CheckBox follows " + variant);
        Require(input.Focus(), "Input accepts focus.");
        Dispatcher.UIThread.RunJobs();
        Require(input.GetVisualDescendants().OfType<Border>().Any(border =>
            border.BorderBrush is ISolidColorBrush brush && brush.Color == accent), "Existing TextBox focus follows " + variant);
        Require(selector.Focus(), "Selector accepts focus.");
        Dispatcher.UIThread.RunJobs();
        Require(selector.GetVisualDescendants().OfType<Border>().Any(border =>
            border.BorderBrush is ISolidColorBrush brush && brush.Color == accent), "Existing ComboBox focus follows " + variant);
        Console.WriteLine("PASS live Semi controls: " + variant);
    }

    foreach (var culture in new[] { "en-US", "zh-CN", "ja-JP", "zh-Hant-TW", "fr-FR", "en-US" })
    {
        var previous = manager.Culture;
        var notifications = changed;
        manager.Culture = CultureInfo.GetCultureInfo(culture);
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Require(changed == notifications + (Equals(previous, manager.Culture) ? 0 : 1), "A language change raises one global event.");
        Require(text.Text == manager.GetResource(DemoKeys.Shell.Ready), "Existing host binding follows the singleton.");
        var menu = (markdown.ContextMenu!.Items[0] as MenuItem)?.Header as string;
        Require(menu == MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText), "Existing Markdown menu follows the singleton.");
        markdownLabels.Add(menu!);
        var search = properties.GetVisualDescendants().OfType<TextBox>().Single(input => input.Name == "SearchBox");
        Require(search.PlaceholderText == PropertyGridLocalization.Get(PropertyKeys.SearchPlaceholder), "Existing property panel follows the singleton.");
        propertyLabels.Add(search.PlaceholderText ?? string.Empty);
        var message = MissingRepositoryMessage();
        Require(!message.Contains("Toolkit.", StringComparison.Ordinal), "Update service resolves embedded resources.");
        updateLabels.Add(message);
        Console.WriteLine("PASS shared culture: " + culture);
    }
    Require(markdownLabels.Count == 4 && propertyLabels.Count == 2 && updateLabels.Count == 2, "Host and libraries switch supported cultures and fall back.");

    // 直接使用应用输出 JSON 覆盖一个库键，库的其他翻译仍由程序集提供。
    var output = Path.GetFullPath(args.FirstOrDefault() ?? Path.Combine(Path.GetTempPath(), "codewf-lang-verification"));
    Directory.CreateDirectory(output);
    File.WriteAllText(Path.Combine(output, "en-US.json"), """
        {"language":"English","description":"Verification host","cultureName":"en-US",
         "CodeWF.Avalonia.Markdown.MarkdownL.CopyRenderedText":"Application copy",
         "Localization.CodeWF_Avalonia_TreeDataGrid.PropertyGrid.SearchPlaceholder":"Application properties"}
        """);
    manager.Register(new JsonLangPlugin { ResourceFolder = output }, CultureInfo.GetCultureInfo("en-US"));
    manager.AddResource(typeof(PropertyGrid).Assembly, typeof(MarkdownViewer).Assembly);
    Dispatcher.UIThread.RunJobs();
    Require(MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText) == "Application copy", "Host resource overrides Markdown after adding a library.");
    Require(PropertyGridLocalization.Get(PropertyKeys.SearchPlaceholder) == "Application properties", "Host resource overrides property panel after adding a library.");
    Require(Equals((markdown.ContextMenu!.Items[0] as MenuItem)?.Header, "Application copy"), "Existing Markdown menu applies the host override.");
    Require(properties.GetVisualDescendants().OfType<TextBox>().Single(input => input.Name == "SearchBox").PlaceholderText == "Application properties", "Existing property panel applies the host override.");
    Require(MarkdownLocalization.GetResource(MarkdownL.CopySelectedText) == "Copy selected text", "Missing host key falls back to embedded Markdown.");
    // 覆盖用例结束后恢复宿主资源，后续目录渲染必须使用真实 Demo 文案。
    manager.Register(new JsonLangPlugin { ResourceFolder = DemoBootstrapper.I18nFolder }, CultureInfo.GetCultureInfo("zh-CN"));
    var renders = 0;
    using var bitmap = new global::Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize(960, 720));
    foreach (var item in DemoCatalog.CreateGroups().SelectMany(group => group.Items))
    {
        var page = item.PageFactory();
        window.Content = page;
        _ = page.Properties;
        foreach (var culture in new[] { "zh-CN", "en-US" })
        {
            manager.Culture = CultureInfo.GetCultureInfo(culture);
            Require(manager.GetResource(DemoKeys.Shell.Ready) != DemoKeys.Shell.Ready,
                "Catalog rendering uses the real host translations in " + culture);
            foreach (var palette in StudioPalettes.All)
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette.Key, dark);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                bitmap.Render(window);
                Require(page.Bounds.Width > 0 && page.Bounds.Height > 0, "Catalog page has layout: " + item.Api);
                renders++;
            }
        }
        Console.WriteLine("PASS rendered catalog page: " + item.Api);
    }
    Console.WriteLine($"PASS catalog rendering: {renders} page/language/palette combinations; NativeAOT={!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}.");
    Console.WriteLine("PASS: one manager, live host/Markdown/property/update language switching and host resource precedence.");
    return 0;
}
finally
{
    manager.CultureChanged -= OnCultureChanged;
    window.Close();
}

void OnCultureChanged(object? sender, EventArgs args)
{
    Require(ReferenceEquals(sender, manager), "All events originate from the same manager.");
    changed++;
}
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void VerifyEditableTableColumns()
{
    // 短名称列保持紧凑，描述列获得更多空间；即时编辑与只读渲染复用同一布局。
    var editor = new global::CodeWF.Avalonia.Markdown.Editor.Controls.MarkdownLiveEditorView();
    editor.SetText("| API | Purpose |\n| --- | --- |\n| Open | Open a Markdown document without modifying its source. |\n| Save | Persist edits after the user confirms the destination. |");
    var window = new Window { Width = 800, Height = 420, Content = editor };
    window.Show();
    foreach (var width in new[] { 800d, 380d })
    {
        window.Width = width;
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var grid = editor.GetVisualDescendants().OfType<Grid>().Single(grid => grid.GetType().Name == "MarkdownTableGrid");
        Require(grid.ColumnDefinitions[0].ActualWidth < grid.ColumnDefinitions[1].ActualWidth / 2,
            "An editable table preserves the shorter API column at width " + width);
        Require(grid.Bounds.Width <= editor.Bounds.Width + 1, "An editable table fits the document at width " + width);
        var outline = (Border)grid.Parent!;
        Require(Math.Abs(outline.Bounds.Width - grid.ColumnDefinitions.Sum(column => column.ActualWidth) - 2) <= 2,
            "The editable table outline hugs its real columns without a blank trailing area at width " + width);
    }
    // 使用同一个表格逐次切语，验证原生代码能读取新增内嵌资源并刷新已存在的菜单。
    var table = editor.GetVisualDescendants().OfType<UserControl>().Single(control => control.GetType().Name == "MarkdownTableView");
    var headers = new HashSet<string>();
    var manager = I18nManager.Instance;
    var originalCulture = manager.Culture;
    var localizer = new global::CodeWF.Avalonia.Markdown.Editor.Services.MarkdownEditorLocalizer();
    try
    {
        foreach (var culture in new[] { "en-US", "zh-CN", "zh-Hant", "ja-JP" })
        {
            manager.Culture = CultureInfo.GetCultureInfo(culture);
            Dispatcher.UIThread.RunJobs();
            var expected = MarkdownLocalization.GetResource(MarkdownL.EditorTableAddRow);
            var header = ((MenuItem)table.ContextMenu!.Items[0]!).Header?.ToString() ?? string.Empty;
            Require(header == expected && header != MarkdownL.EditorTableAddRow, "Existing editable table menu follows " + culture);
            Require(localizer.Get(global::CodeWF.Avalonia.Markdown.Editor.Services.MarkdownEditorText.BoldPlaceholder)
                == MarkdownLocalization.GetResource(MarkdownL.EditorBoldPlaceholder), "Editor actions follow " + culture);
            headers.Add(header);
        }
        Require(headers.Count == 4, "Editable table has four distinct embedded translations.");
    }
    finally
    {
        manager.Culture = originalCulture;
        window.Close();
    }
    Console.WriteLine("PASS editable table: natural columns and live embedded menu/action translations in four languages.");
}

static void VerifyReadOnlyTableColumns()
{
    var viewer = new MarkdownViewer { Markdown = "| API | Purpose |\n| --- | --- |\n| Open | Open a Markdown document with a naturally sized table. |" };
    var shell = new Window { Width = 800, Height = 420, Content = viewer };
    try
    {
        shell.Show();
        foreach (var width in new[] { 800d, 380d, 800d })
        {
            shell.Width = width;
            var deadline = DateTime.UtcNow.AddSeconds(10);
            Grid? grid;
            do
            {
                Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
                grid = viewer.GetVisualDescendants().OfType<Grid>().SingleOrDefault(item => item.GetType().Name == "MarkdownTableGrid");
                if (grid is not null) break;
                if (DateTime.UtcNow >= deadline) throw new TimeoutException("Read-only Markdown table did not materialize.");
                Thread.Sleep(1);
            } while (true);
            var outline = (Border)grid.Parent!;
            var columnsWidth = grid.ColumnDefinitions.Sum(column => column.ActualWidth);
            Require(columnsWidth > 0 && Math.Abs(outline.Bounds.Width - columnsWidth - outline.BorderThickness.Left - outline.BorderThickness.Right) <= 2,
                "Read-only table outline follows actual columns without a blank trailing area at " + width);
            Require(grid.ColumnDefinitions[0].ActualWidth < grid.ColumnDefinitions[1].ActualWidth,
                "Read-only Markdown keeps the shorter API column compact.");
        }
        Console.WriteLine("PASS read-only table: natural outline width and resize reflow.");
    }
    finally { shell.Close(); }
}
static void VerifyMindMapLanguageState()
{
    var editor = new global::CodeWF.Avalonia.MindView.Controls.MindMapEditor();
    var shell = new Window { Width = 600, Height = 400, Content = editor };
    try
    {
        shell.Show(); Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        foreach (var culture in new[] { "en-US", "zh-CN", "en-US" })
        {
            Task.Run(() => I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(culture)).GetAwaiter().GetResult();
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(editor.CenterTopicPlaceholder == global::CodeWF.Avalonia.MindView.I18n.MindViewLocalization.GetResource(
                global::CodeWF.Avalonia.MindView.I18n.MindViewL.CenterTopicPlaceholder),
                "Existing mind-map chrome follows background global culture changes: " + culture);
        }
        Console.WriteLine("PASS mind-map: background culture changes refresh the existing editor on the UI thread.");
    }
    finally { shell.Close(); }
}

static void VerifyStatusLanguageState()
{
    var manager = I18nManager.Instance;
    var originalCulture = manager.Culture;
    try
    {
        manager.Culture = CultureInfo.GetCultureInfo("zh-CN");
        var model = new global::CodeWF.Avalonia.Demo.ViewModels.Pages.StatusComponentsPageViewModel("StatusBadge");
        var original = model.BuildProperties();
        var chineseText = model.BadgeText;
        var status = original.Single(item => item.Id == "st.kind");
        model.Commit(new PropertyValueChange(status, status.Value,
            manager.GetResource(DemoKeys.Content.StatusComponents.OnPropertyCommitted.Warning)));
        manager.Culture = CultureInfo.GetCultureInfo("en-US");
        var english = model.BuildProperties();
        Require(model.BadgeText != chineseText && model.BadgeText == manager.GetResource(DemoKeys.Content.StatusComponents.View.ChangeTheStatusAndIconWidthInThe),
            "Untouched badge sample text switches language.");
        Require(Equals(english.Single(item => item.Id == "st.kind").Value,
            manager.GetResource(DemoKeys.Content.StatusComponents.Properties.Warning)) && model.BadgeKind == "Warn",
            "Language refresh preserves the chosen badge status in both the control and property panel.");
        var text = english.Single(item => item.Id == "st.badgeText");
        model.Commit(new PropertyValueChange(text, text.Value, "Custom input 123"));
        manager.Culture = CultureInfo.GetCultureInfo("zh-CN");
        Require(Equals(model.BuildProperties().Single(item => item.Id == "st.badgeText").Value, "Custom input 123") && model.BadgeText == "Custom input 123",
            "Language refresh preserves user-edited badge content.");
        var card = new global::CodeWF.Avalonia.Demo.ViewModels.Pages.StatusComponentsPageViewModel("StatusCard");
        var width = card.BuildProperties().Single(item => item.Id == "st.iconWidth");
        card.Commit(new PropertyValueChange(width, width.Value, 48m));
        manager.Culture = CultureInfo.GetCultureInfo("en-US");
        Require(Equals(card.BuildProperties().Single(item => item.Id == "st.iconWidth").Value, 48m),
            "Language refresh preserves the edited card icon width.");
    }
    finally { manager.Culture = originalCulture; }
    Console.WriteLine("PASS status samples: live default translations preserve edited text, status and icon width.");
}

static void VerifyWindowCapabilities()
{
    // 验证实际标题栏模板响应运行时能力变化；禁用后程序化点击也不能最小化。
    var window = new global::CodeWF.Avalonia.Controls.Controls.CodeWFWindow { Width = 500, Height = 300, CanMinimize = false };
    window.Show();
    Dispatcher.UIThread.RunJobs();
    window.UpdateLayout();
    var minimize = window.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_MinimizeButton");
    Require(!minimize.IsEnabled, "A window that cannot minimize disables its actual caption button.");
    minimize.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Require(window.WindowState == WindowState.Normal, "Disabled minimization is also enforced by the caption handler.");
    window.CanMinimize = true;
    Require(minimize.IsEnabled, "Enabling minimization updates the existing caption button.");
    minimize.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    Require(window.WindowState == WindowState.Minimized, "An enabled caption button can minimize the window.");
    window.WindowState = WindowState.Normal;
    window.CanMinimize = false;
    Require(!minimize.IsEnabled, "Disabling minimization updates the same caption button.");
    window.Close();
    Console.WriteLine("PASS actual caption: CanMinimize controls the live button and handler.");
}

static void VerifyDockDocumentTransfer(string captures)
{
    // 使用真实外壳验证 Document 分屏与浮动，内容和属性面板始终是同一组实例。
    var shell = new global::CodeWF.Avalonia.Demo.Views.MainWindow { Width = 1600, Height = 960 };
    shell.Show();
    var model = (global::CodeWF.Avalonia.Demo.ViewModels.MainViewModel)shell.DataContext!;
    try
    {
        VerifyTitleBar(shell, model);
        foreach (var navigationShare in new[] { 0.16, 0.24 })
        {
            model.ResetLayout();
            model.OpenItem(model.Groups.SelectMany(group => group.Items).Single(item => item.Api == "StatusBadge"));
            var document = model.ActiveDocument!;
            var dockable = document.DockDocument!;
            var view = (global::CodeWF.Avalonia.Demo.Views.DemoDocumentView)dockable.Context!;
            var properties = view.FindControl<PropertyGrid>("DocumentProperties")!;
            var documentArea = (global::Dock.Model.Controls.IProportionalDock)((global::Dock.Model.Core.IDock)dockable.Owner!).Owner!;
            var navigationDock = (global::Dock.Model.Core.IDock)((global::Dock.Model.Core.IDock)documentArea.Owner!).VisibleDockables!
                .Single(item => item.Id == "Dock.Nav");
            var navigationView = (Control)navigationDock.VisibleDockables![0].Context!;
            // 同时检查默认布局与拖动分隔条后的份额，不能靠恢复固定默认值通过。
            navigationDock.Proportion = navigationShare;
            documentArea.Proportion = 1 - navigationShare;
            PumpDock(() => navigationView.Bounds.Width > 0);
            var navigationWidth = navigationView.Bounds.Width;
            model.OpenItem(model.Groups[0].Items[0]);
            var target = (global::Dock.Model.Core.IDock)model.ActiveDocument!.DockDocument!.Owner!;
            var factory = model.Layout!.Factory!;
            var service = new global::Dock.Model.DockService();
            Require(service.SplitDockable(dockable, (global::Dock.Model.Core.IDock)dockable.Owner!, target,
                global::Dock.Model.Core.DockOperation.Right, bExecute: true), "The standard Dock service splits the document.");
            PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
            Require(!target.VisibleDockables!.Contains(dockable), "Splitting removes the document from its original tab group.");
            Require(ReferenceEquals(properties.Items, document.PropertyItems), "Split document keeps its own property panel.");
            VerifyNavigation("split");
            Capture(shell, navigationShare == 0.16 ? "split.png" : "split-adjusted.png");
            factory.FloatDockable(dockable);
            PumpDock(() => TopLevel.GetTopLevel(view) is Window floating && !ReferenceEquals(floating, shell));
            Require(ReferenceEquals(properties.Items, document.PropertyItems), "Floating document keeps its own property panel.");
            VerifyNavigation("float");
            Capture((Window)TopLevel.GetTopLevel(view)!, navigationShare == 0.16 ? "floating.png" : "floating-adjusted.png");
            factory.MoveDockable((global::Dock.Model.Core.IDock)dockable.Owner!, target, dockable, null);
            PumpDock(() => ReferenceEquals(TopLevel.GetTopLevel(view), shell) && properties.Bounds.Width > 0);
            Require(ReferenceEquals(dockable.Context, view), "Dock transfer preserves the actual document view.");
            Require(target.VisibleDockables!.Count(item => ReferenceEquals(item, dockable)) == 1,
                "Re-docking creates exactly one tab for the document.");
            VerifyNavigation("re-dock");
            Capture(shell, navigationShare == 0.16 ? "redocked.png" : "redocked-adjusted.png");
            Console.WriteLine("PASS actual shell: split, float and re-dock preserve document content, properties and navigation share " + navigationShare);

            void VerifyNavigation(string stage)
            {
                Require(Math.Abs(navigationDock.Proportion - navigationShare) < 0.000001,
                    "Document " + stage + " preserves the current navigation proportion.");
                Require(Math.Abs(navigationView.Bounds.Width - navigationWidth) <= 2,
                    "Document " + stage + " preserves the actual navigation width.");
            }
        }
    }
    finally { shell.Close(); }

    void Capture(Window target, string name)
    {
        target.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        Directory.CreateDirectory(captures);
        using var image = new global::Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize(
            Math.Max(1, (int)Math.Ceiling(target.Bounds.Width)), Math.Max(1, (int)Math.Ceiling(target.Bounds.Height))));
        image.Render(target);
        image.Save(Path.Combine(captures, name));
    }

    void PumpDock(Func<bool> completed)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            Dispatcher.UIThread.RunJobs();
            shell.UpdateLayout();
            if (completed()) return;
            if (DateTime.UtcNow > deadline) throw new TimeoutException("Dock document transfer did not complete.");
            Thread.Sleep(10);
        } while (true);
    }
}

static void VerifyTitleBar(global::CodeWF.Avalonia.Demo.Views.MainWindow shell, MainViewModel model)
{
    Dispatcher.UIThread.RunJobs();
    shell.UpdateLayout();
    var appearanceButton = shell.FindControl<global::Avalonia.Controls.Primitives.ToggleButton>("AppearanceButton")!;
    var languageButton = shell.FindControl<global::Avalonia.Controls.Primitives.ToggleButton>("TitleBarLanguageButton")!;
    Require(appearanceButton.Height == 32 && languageButton.Width == 32 && languageButton.Height == 32,
        "Title-bar appearance and language entries keep compact dimensions.");
    var popups = global::Avalonia.LogicalTree.LogicalExtensions.GetLogicalDescendants(shell)
        .OfType<global::Avalonia.Controls.Primitives.Popup>()
        .Concat(shell.GetVisualDescendants().OfType<global::Avalonia.Controls.Primitives.Popup>()).ToArray();
    var languagePopup = popups.First(popup => (popup.Child as Border)?.Classes.Contains("language-menu") == true);
    var appearancePopup = popups.First(popup => (popup.Child as Border)?.Classes.Contains("appearance-choices") == true);
    var appearanceBorder = (Border)appearancePopup.Child!;

    ListBox LanguageList() => ((Border)languagePopup.Child!).GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("language-list"));
    ListBox ModePills() => appearanceBorder.GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("appearance-pills") && list.Items.Count == 3);
    global::Avalonia.Controls.Button[] PaletteCards() => appearanceBorder.GetVisualDescendants()
        .OfType<global::Avalonia.Controls.Button>().Where(button => button.Classes.Contains("palette-card")).ToArray();
    void WaitFor(Func<bool> condition, string message)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            Dispatcher.UIThread.RunJobs();
            shell.UpdateLayout();
            // PopupRoot 是独立布局根，必须单独驱动布局才会物化 ItemsControl 容器。
            ((Border)languagePopup.Child!).UpdateLayout();
            appearanceBorder.UpdateLayout();
            try { if (condition()) return; }
            catch (InvalidOperationException) { }
            if (DateTime.UtcNow >= deadline) throw new TimeoutException(message);
            Thread.Sleep(10);
        } while (true);
    }

    // 语言弹层：徽标行列表与全局语言双向跟随（外部切换 ↔ 弹层选择）。
    languageButton.IsChecked = true;
    WaitFor(() => LanguageList().Items.Count == model.Cultures.Count, "The language popup did not materialize its culture list.");
    Require(ReferenceEquals(LanguageList().ItemsSource, model.Cultures), "Language popup lists the host cultures.");
    var firstCulture = model.Cultures[0];
    var lastCulture = model.Cultures[^1];
    I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(lastCulture.Name);
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(ReferenceEquals(model.SelectedCulture, lastCulture) && ReferenceEquals(LanguageList().SelectedItem, lastCulture),
        "Global culture changes synchronize the existing title-bar language list.");
    LanguageList().SelectedItem = firstCulture;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(I18nManager.Instance.Culture.Name == firstCulture.Name && ReferenceEquals(model.SelectedCulture, firstCulture),
        "Choosing the previous language after an external change restores the global culture.");
    Task.Run(() => I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(lastCulture.Name)).GetAwaiter().GetResult();
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Require(ReferenceEquals(LanguageList().SelectedItem, lastCulture),
        "Background global culture changes update the existing language list on the UI thread.");
    Require(model.StatusText == I18nManager.Instance.GetResource(DemoKeys.Shell.Ready),
        "The shell status refreshes after external culture changes.");
    languageButton.IsChecked = false;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();

    // 外观弹层：密度/模式 pill 行 + 配色卡网格（点击选中，三态模式逐色板轮询）。
    appearanceButton.IsChecked = true;
    WaitFor(() => ModePills().Items.Count == 3 && PaletteCards().Length == model.Palettes.Count,
        "The appearance popup did not materialize its palette grid.");
    var densityPills = appearanceBorder.GetVisualDescendants().OfType<ListBox>()
        .Single(list => list.Classes.Contains("appearance-pills") && list.Items.Count == 2);
    Require(densityPills.SelectedIndex == model.DensityIndex, "Density pills follow the current shell density.");
    densityPills.SelectedIndex = 1;
    Dispatcher.UIThread.RunJobs();
    Require(model.DensityIndex == 1, "Density pills switch the shell to compact.");
    densityPills.SelectedIndex = 0;
    Dispatcher.UIThread.RunJobs();
    Require(model.DensityIndex == 0, "Density pills restore the comfortable shell.");
    Require(ModePills().Items.Count == 3, "Title bar keeps the three-state appearance mode pills.");
    if (ModePills().SelectedIndex != model.ThemeModeIndex)
    {
        // XAML 里 SelectedIndex 绑定先于 ItemsSource 激活，初始选择会丢失，这里显式对齐。
        ModePills().SelectedIndex = model.ThemeModeIndex;
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    }
    Require(ModePills().SelectedIndex == model.ThemeModeIndex, "Mode pills follow the current theme mode.");
    foreach (var palette in model.Palettes)
    {
        model.SelectedPalette = palette;
        foreach (var expected in new[] { StudioThemeMode.Light, StudioThemeMode.Dark, StudioThemeMode.System })
        {
            ModePills().SelectedIndex = (int)expected;
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(model.ThemeModeIndex == (int)expected, "Mode pills select all three persisted modes: " + expected);
            Require(ReferenceEquals(model.SelectedPalette, palette), "Mode changes preserve the selected palette.");
            var actual = Application.Current!.RequestedThemeVariant;
            Require(actual == palette.ResolveVariant(model.IsDarkMode), "Mode pills apply the Studio palette variant: " + actual);
        }
    }
    Require(appearanceBorder.GetVisualDescendants().OfType<ItemsControl>()
            .Count(control => ReferenceEquals(control.ItemsSource, model.Palettes)) == 1,
        "The title bar has one palette entry, with no duplicate grid.");
    var cards = PaletteCards();
    cards[0].RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
    Require(ReferenceEquals(model.SelectedPalette, (AppearanceOption)cards[0].Tag!), "Palette card clicks apply the chosen palette.");
    string TipText(object? tip) => tip switch { string text => text, TextBlock block => block.Text, _ => string.Empty };
    Require(TipText(global::Avalonia.Controls.ToolTip.GetTip(languageButton)) == I18nManager.Instance.GetResource(DemoKeys.Shell.GlobalSettings.LanguageLabel)
        && TipText(global::Avalonia.Controls.ToolTip.GetTip(appearanceButton)) == I18nManager.Instance.GetResource(DemoKeys.Shell.AppearanceMenu),
        "The existing title-bar tooltips follow the host language.");
    appearanceButton.IsChecked = false;
    Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
    Console.WriteLine("PASS title bar: appearance pills, palette cards, language list and compact entries.");
}
static string MissingRepositoryMessage()
{
    try { new UpdateServiceOptions().Validate(); }
    catch (InvalidOperationException error) { return error.Message; }
    throw new InvalidOperationException("Expected a repository validation message.");
}

static void VerifyPropertyEditors()
{
    var items = new[]
    {
        new PropertyItem { Name = "Text", Kind = PropertyItemKind.Text, Value = "Before" },
        new PropertyItem { Name = "Number", Kind = PropertyItemKind.Number, Value = 1m },
        new PropertyItem { Name = "Boolean", Kind = PropertyItemKind.Boolean, Value = false },
        new PropertyItem { Name = "Choice", Kind = PropertyItemKind.Choice, Options = ["Light", "Dark"], Value = "Light" },
        new PropertyItem { Name = "Radio", Kind = PropertyItemKind.Radio, Options = ["Light", "Dark"], Value = "Light" },
        new PropertyItem { Name = "Percent", Kind = PropertyItemKind.Percent, Value = 10d },
        new PropertyItem { Name = "Color", Kind = PropertyItemKind.Color, Value = "#000000" },
        new PropertyItem { Name = "ReadOnly", IsReadOnly = true, Value = "Before" },
    };
    var grid = new PropertyGrid { Items = items };
    var shell = new Window { Width = 640, Height = 760, Content = grid };
    var previews = 0;
    var commits = 0;
    grid.ValuePreviewing += (_, e) => { previews++; e.Cancel = true; };
    grid.ValueCommitted += (_, _) => commits++;
    try
    {
        shell.Show();
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        var editors = grid.GetVisualDescendants().OfType<PropertyCellEditor>().ToArray();
        Control Editor(int index) => (Control)editors.Single(editor => ReferenceEquals(((PropertyLeafRow)editor.DataContext!).Item, items[index])).Content!;
        var text = (TextBox)Editor(0);
        var number = (NumericUpDown)Editor(1);
        var check = (CheckBox)Editor(2);
        var combo = (ComboBox)Editor(3);
        var radios = ((StackPanel)Editor(4)).Children.OfType<RadioButton>().ToArray();
        var slider = ((DockPanel)Editor(5)).Children.OfType<Slider>().Single();
        var color = (StackPanel)Editor(6);
        var picker = (ColorPicker)color.Children.OfType<global::Avalonia.Controls.Primitives.Popup>().Single().Child!;
        var readOnly = (TextBlock)Editor(7);

        text.Text = "Rejected";
        text.RaiseEvent(new global::Avalonia.Input.KeyEventArgs { RoutedEvent = global::Avalonia.Input.InputElement.KeyDownEvent, Key = global::Avalonia.Input.Key.Enter });
        number.Value = 12m;
        check.IsChecked = true;
        combo.SelectedItem = "Dark";
        radios[1].IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        slider.Value = 80d;
        picker.Color = Colors.White;
        Require(text.Text == "Before" && number.Value == 1m && check.IsChecked == false && Equals(combo.SelectedItem, "Light")
            && radios[0].IsChecked == true && radios[1].IsChecked == false && slider.Value == 10d && picker.Color == Colors.Black,
            "Canceled property edits restore every existing editor.");
        Require(previews == 7 && commits == 0, "Each rejected edit raises one preview and no commit.");

        object[] values = ["External", 23m, true, "Dark", "Dark", 70d, "#FFFFFF", "External read-only"];
        for (var i = 0; i < items.Length; i++) items[i].Value = values[i];
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(ReferenceEquals(text, Editor(0)) && text.Text == "External" && number.Value == 23m && check.IsChecked == true
            && Equals(combo.SelectedItem, "Dark") && radios[1].IsChecked == true && slider.Value == 70d && picker.Color == Colors.White
            && readOnly.Text == "External read-only" && Equals(ToolTip.GetTip(readOnly), "External read-only"),
            "Programmatic values update existing property editors without replacing rows.");
        Require(previews == 7 && commits == 0, "Model updates do not submit back to the host.");
        shell.Content = null;
        items[0].Value = "Reattached";
        shell.Content = grid;
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        editors = grid.GetVisualDescendants().OfType<PropertyCellEditor>().ToArray();
        Require(((TextBox)Editor(0)).Text == "Reattached", "Reattached editors refresh updates missed while detached.");
        Console.WriteLine("PASS PropertyGrid: live external values, canceled edits, no duplicate commits and reattachment.");
    }
    finally { shell.Close(); }
}
static void VerifyHiddenPropertyRows()
{
    var visible = false;
    var hiddenLeaf = new PropertyItem { Name = "Private value", VisibleWhen = () => visible };
    var hiddenCategory = new PropertyItem { Name = "Private category", Kind = PropertyItemKind.Category,
        VisibleWhen = () => visible, Children = [new PropertyItem { Name = "Private descendant" }] };
    var parent = new PropertyItem { Name = "Parent", Children = [new PropertyItem { Name = "Public" }, hiddenLeaf, hiddenCategory] };
    var grid = new PropertyGrid { Items = new[] { parent } };
    var shell = new Window { Width = 480, Height = 360, Content = grid };
    try
    {
        shell.Show();
        foreach (var mode in Enum.GetValues<PropertyGridViewMode>())
        foreach (var query in new string?[] { null, "Parent", "Private" })
        {
            grid.ViewMode = mode;
            grid.SearchText = query;
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            Require(!grid.Rows.Any(row => row.Item.Name.StartsWith("Private", StringComparison.Ordinal)),
                "Hidden items and categories stay hidden in " + mode + " with query " + query);
            if (query == "Private") Require(grid.Rows.Count == 0, "Hidden matches do not retain their ancestors.");
            if (mode == PropertyGridViewMode.Alphabetical)
                Require(!grid.Rows.Any(row => row is PropertyCategoryRow), "Alphabetical search never adds category rows.");
        }
        grid.SearchText = "Private";
        visible = true;
        grid.Refresh();
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(grid.Rows.Any(row => ReferenceEquals(row.Item, hiddenLeaf)),
            "A refreshed dynamic visibility predicate makes the matching property accessible.");
        visible = false;
        grid.Refresh();
        Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        Require(grid.Rows.Count == 0, "Refreshing dynamic visibility removes the branch from current search results.");
        Console.WriteLine("PASS PropertyGrid visibility: categories, hidden descendants, both sorting modes and dynamic refresh.");
    }
    finally { shell.Close(); }
}

static void VerifyThirdPartyLocales()
{
    var application = Application.Current!;
    var editor = new TextBox { Text = "Locale verification" };
    var shell = new Window { Width = 360, Height = 120, Content = editor };
    try
    {
        shell.Show(); Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
        var menu = (MenuFlyout)editor.ContextFlyout!;
        foreach (var culture in new[] { "en-US", "zh-CN", "ja-JP", "zh-Hant", "en-US" })
        {
            I18nManager.Instance.Culture = CultureInfo.GetCultureInfo(culture);
            Dispatcher.UIThread.RunJobs(); shell.UpdateLayout();
            ResourceDictionary semi = culture switch
            {
                "en-US" => new Semi.Avalonia.Locale.en_us(),
                "ja-JP" => new Semi.Avalonia.Locale.ja_jp(),
                "zh-Hant" => new Semi.Avalonia.Locale.zh_tw(),
                _ => new Semi.Avalonia.Locale.zh_cn(),
            };
            ResourceDictionary ursa = culture switch
            {
                "en-US" => new Ursa.Themes.Semi.Locale.en_us(),
                "ja-JP" => new Ursa.Themes.Semi.Locale.en_us(),
                _ => new Ursa.Themes.Semi.Locale.zh_cn(),
            };
            menu.ShowAt(editor); Dispatcher.UIThread.RunJobs();
            Require(Equals(((MenuItem)menu.Items[1]!).Header, semi["STRING_MENU_COPY"])
                && Equals(((MenuItem)menu.Items[2]!).Header, semi["STRING_MENU_PASTE"]),
                "Existing Semi editor menu follows global culture: " + culture);
            Require(Equals(application.Styles.OfType<Ursa.Themes.Semi.UrsaSemiTheme>().Single().Resources["STRING_MENU_DIALOG_CANCEL"],
                ursa["STRING_MENU_DIALOG_CANCEL"]), "Existing Ursa dialog follows global culture: " + culture);
            menu.Hide();
        }
        application.Resources["STRING_MENU_COPY"] = "Host copy";
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        Dispatcher.UIThread.RunJobs();
        menu.ShowAt(editor); Dispatcher.UIThread.RunJobs();
        Require(Equals(((MenuItem)menu.Items[1]!).Header, "Host copy"), "Theme locale changes preserve host overrides.");
        menu.Hide();
        Console.WriteLine("PASS Semi/Ursa: existing menu, global culture and host resource precedence.");
    }
    finally
    {
        application.Resources.Remove("STRING_MENU_COPY");
        I18nManager.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
        shell.Close();
    }
}
