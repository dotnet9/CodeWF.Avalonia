using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CodeWF.Avalonia.Content.Tests.Controls;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Log;
using CodeWF.Avalonia.Log.Notifications.Views;
using CodeWF.Avalonia.Log.Notifications.ViewModels;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.MindView;
using CodeWF.Avalonia.MindView.Controls;
using CodeWF.Avalonia.MindView.Themes;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Toolkit.Logging;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using Xunit;
using Xunit.Abstractions;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Themes;

[Collection("AvaloniaPlatform")]
public sealed class ContentPaletteRegressionTests(AvaloniaPlatformFixture platform, ITestOutputHelper output)
{
    [Fact]
    public void EditorsRespectWindowScopedResourcesDuringThemeSwitches() => platform.Run(() =>
    {
        var dark = new ThemeVariant("ContentDark", ThemeVariant.Dark);
        var source = new MarkdownEditorView { Text = "# Source" };
        var live = new MarkdownLiveEditorView();
        live.SetText("# Preview");
        var host = new Grid { RowDefinitions = new RowDefinitions("*,*"), Children = { source, live } };
        Grid.SetRow(live, 1);
        var window = new Window { Width = 700, Height = 500, Content = host };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new MarkdownFullThemes());
        window.Resources.ThemeDictionaries[ThemeVariant.Light] = EditorResources("#FFF9F0", "#25354A");
        window.Resources.ThemeDictionaries[dark] = EditorResources("#26364A", "#F8FAFC");
        window.Show();
        try
        {
            foreach (var variant in new[] { ThemeVariant.Light, dark, ThemeVariant.Light })
            {
                window.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();
                var expected = variant == dark ? "#26364A" : "#FFF9F0";
                Assert.Equal(Color.Parse(expected), Solid(source.Editor.Background));
                Assert.Equal(Solid(source.Editor.Foreground), Solid(live.TextBrush));
                Assert.Equal("# Source", source.Text);
                Assert.Equal("# Preview", live.Text);
                CaptureIfRequested(window, variant == dark ? "editors-dark" : "editors-light");
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void MindMapUsesWindowAndDerivedVariantResources() => platform.Run(() =>
    {
        var dark = new ThemeVariant("MindCustomDark", ThemeVariant.Dark);
        var editor = new MindMapEditor { Roots = new ObservableCollection<MindMapNode> { new("Root", new MindMapNode("Child")) } };
        var window = new Window { Width = 900, Height = 600, Content = editor };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new MindViewThemes());
        window.Resources.ThemeDictionaries[dark] = new ResourceDictionary
        {
            [MindViewStyleKeys.CanvasBackgroundBrushResource] = Brush.Parse("#283645"),
            [MindViewStyleKeys.PrimaryTextBrushResource] = Brush.Parse("#F8FAFC")
        };
        window.RequestedThemeVariant = dark;
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.True(editor.IsDarkTheme);
            Assert.Equal(Color.Parse("#283645"), Solid(editor.GetVisualDescendants().OfType<Canvas>().Single().Background));
            Assert.Equal(Color.Parse("#F8FAFC"), Solid(editor.GetVisualDescendants().OfType<TextBox>().Single(box => box.Text == "Child").Foreground));
        }
        finally { window.Close(); }
    });

    [Fact]
    public void MindMapRecognizesDarknessThroughMoreThanOneInheritedVariant() => platform.Run(() =>
    {
        var editor = new MindMapEditor();
        var window = new Window { Content = editor, RequestedThemeVariant = new ThemeVariant("NestedMind", new ThemeVariant("MindBase", ThemeVariant.Dark)) };
        window.Show();
        try { Assert.True(editor.IsDarkTheme); }
        finally { window.Close(); }
    });

    [Fact]
    public void LogTextRetainsReadableSemanticColorsInEveryStudioVariant() => platform.Run(() =>
    {
        var theme = new CodeWFLogTheme();
        var keys = new[]
        {
            LogViewResourceKeys.TimestampForeground, LogViewResourceKeys.ContentForeground,
            LogViewResourceKeys.TraceForeground, LogViewResourceKeys.InformationForeground,
            LogViewResourceKeys.WarningForeground, LogViewResourceKeys.ErrorForeground
        };
                foreach (var palette in StudioPaletteTestHost.Keys())
        foreach (var dark in new[] { false, true })
        {
            var variant = StudioTheme.GetVariant(palette, dark);
            var background = Color.Parse(dark ? "#1B1B1B" : "#FFFFFF");
            foreach (var key in keys)
            {
                Assert.True(theme.TryGetResource(key, variant, out var value));
                var color = Solid(Assert.IsAssignableFrom<IBrush>(value));
                var ratio = Contrast(color, background);
                output.WriteLine($"{palette}/{dark}/{key}: {color} against {background}: {ratio:F2}:1");
                Assert.True(ratio >= 4.5, $"{variant.Key}/{key}: {ratio:F2}:1");
            }
        }
    });

    [Fact]
    public void EditorFallbackSyntaxRemainsReadableOnTheCurrentLine()
    {
        foreach (var dark in new[] { false, true })
        {
            var variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            foreach (var backgroundKey in new[] { MarkdownEditorPalette.BackgroundKey, MarkdownEditorPalette.CurrentLineBackgroundKey })
            foreach (var key in new[] { MarkdownEditorPalette.ForegroundKey, MarkdownEditorPalette.AccentKey, MarkdownEditorPalette.LinkKey,
                MarkdownEditorPalette.QuoteKey, MarkdownEditorPalette.CodeKey, MarkdownEditorPalette.ImageKey,
                MarkdownEditorPalette.WarningKey, MarkdownEditorPalette.DangerKey })
            {
                var ratio = Contrast(Solid(MarkdownEditorPalette.Resolve(key, variant)), Solid(MarkdownEditorPalette.Resolve(backgroundKey, variant)));
                Assert.True(ratio >= 4.5, $"{variant.Key}/{key}/{backgroundKey}: {ratio:F2}:1");
            }
        }
    }

    [Fact]
    public void DefaultMindMapRootTextAndCaretRemainReadableAcrossThemeSwitches() => platform.Run(() =>
    {
        var root = new MindMapNode("Root") { Note = "Details" };
        var editor = new MindMapEditor { Roots = new ObservableCollection<MindMapNode> { root } };
        var window = new Window { Width = 900, Height = 600, Content = editor };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new MindViewThemes());
        window.Show();
        try
        {
            foreach (var dark in new[] { false, true, false })
            {
                window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                Dispatcher.UIThread.RunJobs();
                var card = editor.GetVisualDescendants().OfType<Border>().Single(border => ReferenceEquals(border.DataContext, root) && border.Child is StackPanel);
                foreach (var box in card.GetVisualDescendants().OfType<TextBox>())
                {
                    Assert.Equal(Solid(box.Foreground), Solid(box.CaretBrush));
                    var ratio = Contrast(Solid(box.Foreground), Solid(card.Background));
                    Assert.True(ratio >= 4.5, $"MindMap/{dark}/{box.Text}: {ratio:F2}:1");
                }
                CaptureIfRequested(window, dark ? "mindmap-dark" : "mindmap-light");
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void LogViewRecolorsAlreadyRenderedEntriesAcrossThemeSwitches() => platform.Run(() =>
    {
        var ownsLogger = Logger.TryInitialize(new LoggerOptions { EnableConsole = false, LineTemplate = "{Level:u3}|{UserMessage}{NewLine}" });
        Logger.Warn("palette warning");
        Logger.FlushAsync().GetAwaiter().GetResult();
        var view = new LogView { Source = Logger.Events };
        var window = new Window { Content = view };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new CodeWFLogTheme());
        window.Show();
        view.MinimumLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
        try
        {
            foreach (var dark in new[] { false, true, false })
            {
                window.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
                Dispatcher.UIThread.RunJobs();
                var text = view.FindControl<SelectableTextBlock>("LogTextView")!;
                Assert.Contains("palette warning", text.Inlines!.Text);
                var warning = text.Inlines.OfType<Run>().Single(run => run.Text == "WAR");
                Assert.Equal(Solid(view.WarningForeground), Solid(warning.Foreground));
                var ratio = Contrast(Solid(warning.Foreground), Color.Parse(dark ? "#1B1B1B" : "#FFFFFF"));
                Assert.True(ratio >= 4.5, $"Existing warning/{dark}: {ratio:F2}:1");
            }
        }
        finally
        {
            window.Close();
            if (ownsLogger) Logger.ShutdownAsync().GetAwaiter().GetResult();
        }
    });

    [Fact]
    public void OpenNotificationSwitchesLanguageWithoutRecreatingItsWindow() => platform.Run(() =>
    {
        var manager = I18nManager.Instance;
        var folder = Path.Combine(Path.GetTempPath(), "CodeWFNotificationLocalization", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
        var window = new NotificationWindow();
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Configure("Palette test", TimeSpan.Zero, null, null, DesktopNotificationAttentionMode.None);
        window.AddLogs([(NotificationEvent(1), "First")]);
        window.Show();
        try
        {
            window.AddLogs([(NotificationEvent(2), "Second")]);
            var viewModel = Assert.IsType<NotificationWindowViewModel>(window.DataContext);
            foreach (var (culture, confirm, folderLabel, newLogs) in new[]
            {
                ("en-US", "OK", "Open log folder", "1 new log"),
                ("zh-CN", "确定", "打开日志目录", "1条新日志"),
                ("fr-FR", "OK", "Open log folder", "1 new log"),
                ("en-US", "OK", "Open log folder", "1 new log")
            })
            {
                manager.Culture = new CultureInfo(culture);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(confirm, window.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("confirm")).Content);
                Assert.Equal(folderLabel, viewModel.OpenLogFolderButtonText);
                Assert.Equal(newLogs, viewModel.NewLogText);
                Assert.Same(viewModel, window.DataContext);
                Assert.Equal("Second", viewModel.LogContent);
                window.RequestedThemeVariant = culture == "zh-CN" ? ThemeVariant.Dark : ThemeVariant.Light;
                Dispatcher.UIThread.RunJobs();
                CaptureIfRequested(window, $"notification-{culture}-{window.ActualThemeVariant.Key}");
            }
            var plugin = new JsonLangPlugin { ResourceFolder = folder };
            foreach (var caption in new[] { "Dismiss", "Close now" })
            {
                File.WriteAllText(Path.Combine(folder, "en-US.json"), JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["language"] = "English", ["description"] = "Host", ["cultureName"] = "en-US",
                    [CodeWF.Avalonia.Log.I18n.ResourceKeys.Notification.Confirm] = caption
                }));
                if (caption == "Dismiss") manager.Register(plugin, new CultureInfo("en-US"));
                else { plugin.Load(new CultureInfo("en-US")); manager.AddResource(); }
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(caption, window.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("confirm")).Content);
                Assert.Equal("Open log folder", viewModel.OpenLogFolderButtonText);
            }
        }
        finally
        {
            window.Close();
            manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
            Directory.Delete(folder, true);
        }
    });

    [Fact]
    public void NotificationResourcesSwitchWithTheHostAndAllowLocalOverrides() => platform.Run(() =>
    {
        var application = global::Avalonia.Application.Current!;
        LogNotifications.SetMode(application, LogNotificationMode.DesktopWindow);
        var card = new Border();
        var window = new Window { Content = card };
        using var binding = card.Bind(Border.BackgroundProperty, card.GetResourceObservable(LogNotificationResourceKeys.WindowBackground));
        window.Show();
        try
        {
                    foreach (var palette in StudioPaletteTestHost.Keys())
            foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
                Dispatcher.UIThread.RunJobs();
                var background = Solid(card.Background);
                Assert.Equal(dark, Luminance(background) < .2);
                foreach (var key in new[] { LogNotificationResourceKeys.ContentForeground, LogNotificationResourceKeys.TitleForeground, LogNotificationResourceKeys.MutedForeground })
                {
                    Assert.True(card.TryFindResource(key, card.ActualThemeVariant, out var value));
                    var ratio = Contrast(Solid(Assert.IsAssignableFrom<IBrush>(value)), background);
                    output.WriteLine($"Notification/{palette}/{dark}/{key}: {ratio:F2}:1");
                    Assert.True(ratio >= 4.5, $"{key}: {ratio:F2}:1");
                }
                var pairs = new[]
                {
                    (LogNotificationResourceKeys.ConfirmForeground, LogNotificationResourceKeys.ConfirmBackground),
                    (LogNotificationResourceKeys.ConfirmForeground, LogNotificationResourceKeys.ConfirmPointerOverBackground),
                    (LogNotificationResourceKeys.ConfirmForeground, LogNotificationResourceKeys.ConfirmPressedBackground),
                    (LogNotificationResourceKeys.OpenLogFolderForeground, LogNotificationResourceKeys.OpenLogFolderBackground),
                    (LogNotificationResourceKeys.OpenLogFolderForeground, LogNotificationResourceKeys.OpenLogFolderPointerOverBackground),
                    (LogNotificationResourceKeys.OpenLogFolderForeground, LogNotificationResourceKeys.OpenLogFolderPressedBackground),
                    (LogNotificationResourceKeys.NewLogForeground, LogNotificationResourceKeys.NewLogLabelBackground),
                    (LogNotificationResourceKeys.DebugForeground, LogNotificationResourceKeys.DebugBackground),
                    (LogNotificationResourceKeys.InfoForeground, LogNotificationResourceKeys.InfoBackground),
                    (LogNotificationResourceKeys.WarnForeground, LogNotificationResourceKeys.WarnBackground),
                    (LogNotificationResourceKeys.ErrorForeground, LogNotificationResourceKeys.ErrorBackground),
                    (LogNotificationResourceKeys.FatalForeground, LogNotificationResourceKeys.FatalBackground)
                };
                foreach (var (foregroundKey, backgroundKey) in pairs)
                {
                    Assert.True(card.TryFindResource(foregroundKey, card.ActualThemeVariant, out var fg));
                    Assert.True(card.TryFindResource(backgroundKey, card.ActualThemeVariant, out var bg));
                    foreach (var sample in BackgroundSamples(Assert.IsAssignableFrom<IBrush>(bg), background))
                    {
                        var ratio = Contrast(Solid(Assert.IsAssignableFrom<IBrush>(fg)), sample);
                        Assert.True(ratio >= 4.5, $"{palette}/{dark}/{foregroundKey}/{backgroundKey}: {ratio:F2}:1");
                    }
                }
            }
            window.Resources[LogNotificationResourceKeys.WindowBackground] = Brushes.MidnightBlue;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Colors.MidnightBlue, Solid(card.Background));
        }
        finally
        {
            window.Close();
            LogNotifications.SetMode(application, LogNotificationMode.None);
        }
    });

    private static ResourceDictionary EditorResources(string background, string foreground) => new()
    {
        [MarkdownEditorPalette.BackgroundKey] = Brush.Parse(background),
        [MarkdownEditorPalette.ForegroundKey] = Brush.Parse(foreground)
    };

    private static CodeWFLogEvent NotificationEvent(long sequence) => new()
    {
        Sequence = sequence, Timestamp = DateTimeOffset.UtcNow, Level = Microsoft.Extensions.Logging.LogLevel.Error,
        CategoryName = "Palette test", Message = sequence == 1 ? "First" : "Second", RequestNotification = true
    };

    private static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;

    private static IEnumerable<Color> BackgroundSamples(IBrush brush, Color canvas)
    {
        static Color Composite(Color color, double opacity, Color background)
        {
            var alpha = color.A / 255d * opacity;
            return Color.FromRgb((byte)Math.Round(color.R * alpha + background.R * (1 - alpha)),
                (byte)Math.Round(color.G * alpha + background.G * (1 - alpha)),
                (byte)Math.Round(color.B * alpha + background.B * (1 - alpha)));
        }
        return brush switch
        {
            ISolidColorBrush solid => new[] { Composite(solid.Color, solid.Opacity, canvas) },
            IGradientBrush gradient => gradient.GradientStops.Select(stop => Composite(stop.Color, gradient.Opacity, canvas)),
            _ => throw new InvalidOperationException($"Unsupported brush: {brush.GetType().Name}")
        };
    }

    private static void CaptureIfRequested(Window window, string name)
    {
        var folder = Environment.GetEnvironmentVariable("CODEWF_CONTENT_THEME_CAPTURE");
        if (string.IsNullOrWhiteSpace(folder)) return;
        Directory.CreateDirectory(folder);
        window.UpdateLayout();
        using var bitmap = new RenderTargetBitmap(new global::Avalonia.PixelSize((int)Math.Ceiling(window.Bounds.Width), (int)Math.Ceiling(window.Bounds.Height)), new global::Avalonia.Vector(96, 96));
        bitmap.Render(window);
        using var stream = File.Create(Path.Combine(folder, name + ".png"));
        bitmap.Save(stream, new PngBitmapEncoderOptions());
    }

    private static double Contrast(Color foreground, Color background)
    {
        var first = Luminance(foreground);
        var second = Luminance(background);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }

    private static double Luminance(Color color)
    {
        static double Linear(byte channel)
        {
            var value = channel / 255d;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }
        return .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
    }
}
