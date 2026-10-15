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

// 离屏运行真实 Demo 主题与控件，锁屏时也可验证 NativeAOT 动态语言切换。
AppBuilder.Configure<CodeWF.Avalonia.Demo.App>().UseSkia().WithInterFont().UseReactiveUI(_ => { })
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
DemoBootstrapper.Initialize();
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
    foreach (var variant in new[] { StudioTheme.DotNetLight, StudioTheme.DotNetDark,
                 StudioTheme.AuroraLight, StudioTheme.AuroraDark, StudioTheme.EmberLight, StudioTheme.EmberDark,
                 StudioTheme.AquaticLight, StudioTheme.AquaticDark, StudioTheme.DesertLight, StudioTheme.DesertDark,
                 StudioTheme.DuskLight, StudioTheme.DuskDark, StudioTheme.NightSkyLight, StudioTheme.NightSkyDark,
                 StudioTheme.DotNetLight })
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
            foreach (var palette in Enum.GetValues<StudioPalette>())
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
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
static string MissingRepositoryMessage()
{
    try { new UpdateServiceOptions().Validate(); }
    catch (InvalidOperationException error) { return error.Message; }
    throw new InvalidOperationException("Expected a repository validation message.");
}
