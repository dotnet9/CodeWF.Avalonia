using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.Demo.Views;
using CodeWF.Avalonia.Demo.Views.Pages;
using CodeWF.Avalonia.Demo.Views.Scenes;
using CodeWF.Avalonia.Log;
using CodeWF.Avalonia.Log.Notifications.Views;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Toolkit.Logging;

/// <summary>离屏检查真实 Demo 的动态配色与交互态，可按目录输出各页截图。</summary>
internal static class DemoPaletteVerification
{
    public static int Run(string[] args)
    {
        var output = Path.GetFullPath(args.FirstOrDefault() is { } firstArgument && !firstArgument.StartsWith("--", StringComparison.Ordinal)
            ? firstArgument : "E:/github/temp/demo-palette-audit");
        Directory.CreateDirectory(output);
        var failures = new List<string>();
        var measurements = new List<string> { "mode\tstate\tforeground\tbackground\tcontrast" };
        var modes = Enum.GetValues<StudioPalette>().SelectMany(palette => new[] { false, true }
            .Select(dark => (Palette: palette, Dark: dark, Name: palette + (dark ? "-dark" : "-light")))).ToArray();
        var shell = new MainWindow();
        var model = (MainViewModel)shell.DataContext!;
        var scene = new MarkdownFlowPage();
        scene.GoToStep(1);
        var sceneWindow = new Window { Width = 1100, Height = 720, Content = scene };
        var blocks = new WindowedStackPanelPage();
        var blocksWindow = new Window { Width = 1100, Height = 720, Content = blocks };
        var samplePage = new CodeWFWindowPage();
        var sampleOwner = new Window { Width = 1100, Height = 720, Content = samplePage };
        try
        {
            shell.Show();
            model.ResetLayout();
            sceneWindow.Show();
            blocksWindow.Show();
            sampleOwner.Show();
            Pump(shell, sceneWindow, blocksWindow, sampleOwner);
            blocks.GetVisualDescendants().OfType<Button>().First().RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var sampleButtons = samplePage.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("demo-action")).ToArray();
            sampleButtons[1].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            sampleButtons[2].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Pump(blocksWindow, sampleOwner);
            var samples = sampleOwner.OwnedWindows.OfType<CodeWFWindow>().ToArray();
            Require(samples.Length == 2, "Window examples open the two real owned windows.");
            var custom = samples.Single(window => window.LeftContent is TextBlock);
            var extended = samples.Single(window => window.LeftContent is not TextBlock);
            var generated = blocks.FindControl<WindowedStackPanel>("BlocksPanel")!;
            Require(generated.ItemCount == 5000, "WindowedStackPanel generates the real example blocks.");
            var firstBlock = generated.Children.OfType<Border>().First();

            foreach (var mode in modes)
            {
                var previousFailures = failures.Count;
                model.SelectedPalette = model.Palettes.Single(palette => palette.Palette == mode.Palette);
                model.IsDarkMode = mode.Dark;
                var variant = StudioTheme.GetVariant(mode.Palette, mode.Dark);
                foreach (var window in new Window[] { shell, sceneWindow, blocksWindow, sampleOwner, custom, extended })
                    window.RequestedThemeVariant = variant;
                Pump(shell, sceneWindow, blocksWindow, sampleOwner, custom, extended);
                var canvas = Resource(shell, "StudioBackground");
                var raised = Resource(shell, "StudioRaised");
                var accent = Resource(shell, "StudioAccent");
                model.SelectedGroup = model.Groups[0];
                model.CommandPaletteOpen = false;
                model.Notifications.Clear();
                Pump(shell);
                var commands = shell.FindControl<ScrollViewer>("RibbonCommands")!.GetVisualDescendants().OfType<Button>().ToArray();
                var selected = commands.Single(button => button.Classes.Contains("selected"));
                var ordinary = commands.First(button => !button.Classes.Contains("selected"));
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                Pump(shell);
                CheckText(selected, canvas, mode.Name, "Ribbon/selected");
                CheckText(ordinary, canvas, mode.Name, "Ribbon/normal");
                Require(ColorOf(Presenter(selected).Foreground!, canvas) == accent, mode.Name + "/Ribbon selected accent");
                var center = ordinary.TranslatePoint(new Point(ordinary.Bounds.Width / 2, ordinary.Bounds.Height / 2), shell)!.Value;
                shell.MouseMove(center);
                Pump(shell);
                CheckText(ordinary, canvas, mode.Name, "Ribbon/hover");
                Require(ColorOf(Presenter(ordinary).Background!, canvas) == raised, mode.Name + "/Ribbon hover surface");
                shell.MouseDown(center, MouseButton.Left);
                Pump(shell);
                CheckText(ordinary, canvas, mode.Name, "Ribbon/pressed");
                Require(ColorOf(Presenter(ordinary).Background!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Ribbon pressed surface");
                // 在按钮外抬起，避免改变待检查文档。
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                shell.MouseUp(new Point(2, shell.Bounds.Height - 2), MouseButton.Left);
                ordinary.IsEnabled = false;
                Pump(shell);
                Require(!ordinary.IsEffectivelyEnabled, mode.Name + "/Ribbon disabled is effective");
                Require(Presenter(ordinary).Foreground is ISolidColorBrush, mode.Name + "/Ribbon disabled has a resolved foreground");
                Require(ColorOf(Presenter(ordinary).Foreground!, canvas) != Resource(shell, "StudioText"),
                    mode.Name + "/Ribbon disabled text differs from normal text");
                var disabledIcon = ordinary.GetVisualDescendants().OfType<PathIcon>().Single(icon => icon.Classes.Contains("ribbon-icon"));
                Require(ColorOf(disabledIcon.Foreground!, canvas) != accent, mode.Name + "/Ribbon disabled icon differs from normal accent");
                Save(shell, Path.Combine(output, mode.Name + "-ribbon.png"));
                ordinary.ClearValue(InputElement.IsEnabledProperty);

                model.CommandQuery = string.Empty;
                model.CommandPaletteOpen = true;
                foreach (var kind in Enum.GetValues<DemoNotificationKind>())
                    model.Notify(kind, kind.ToString(), "CodeWF.Avalonia");
                Pump(shell);
                var overlay = shell.GetVisualDescendants().OfType<ShellOverlay>().Single();
                var countBadge = shell.FindControl<Border>("NotificationCountBadge")!;
                var countText = shell.FindControl<TextBlock>("NotificationCountText")!;
                Require(countBadge.IsEffectivelyVisible && countText.Text == model.NotificationCount.ToString(CultureInfo.CurrentCulture),
                    mode.Name + $"/Notification count reflects the visible notifications; visible={countBadge.IsEffectivelyVisible}, text={countText.Text}, count={model.NotificationCount}");
                Check(countText.Foreground!, ColorOf(countBadge.Background!, canvas), mode.Name, "Notification/count");
                var commandBox = overlay.FindControl<Border>("CommandBox")!;
                Require(ColorOf(commandBox.Background!, canvas) == Resource(shell, "StudioSurface"), mode.Name + "/Command popup surface");
                var results = overlay.FindControl<ListBox>("CommandResults")!;
                Require(results.SelectedIndex == 0, mode.Name + "/Command initial visual selection");
                model.MoveCommandSelection(1);
                Pump(shell);
                Require(results.SelectedIndex == 1 && results.ContainerFromIndex(1) is ListBoxItem { IsSelected: true },
                    mode.Name + "/Command keyboard index updates the visible selection");
                var selectedResult = (ListBoxItem)results.ContainerFromIndex(1)!;
                CheckText(selectedResult, Resource(shell, "StudioSurface"), mode.Name, "Command/selected");
                var hint = overlay.FindControl<TextBlock>("CommandHintText")!;
                Check(hint.Foreground!, Resource(shell, "StudioSurface"), mode.Name, "Command/hint");
                var command = results.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("cpm-item")).ElementAt(2);
                var commandPoint = command.TranslatePoint(new Point(command.Bounds.Width / 2, command.Bounds.Height / 2), shell)!.Value;
                shell.MouseMove(commandPoint);
                Pump(shell);
                CheckText(command, Resource(shell, "StudioSurface"), mode.Name, "Command/hover");
                shell.MouseDown(commandPoint, MouseButton.Left);
                Pump(shell);
                CheckText(command, Resource(shell, "StudioSurface"), mode.Name, "Command/pressed");
                Require(ColorOf(Presenter(command).Background!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Command pressed surface");
                shell.MouseMove(new Point(2, shell.Bounds.Height - 2));
                shell.MouseUp(new Point(2, shell.Bounds.Height - 2), MouseButton.Left);
                Pump(shell);
                foreach (var card in overlay.GetVisualDescendants().OfType<Border>().Where(border => border.Classes.Contains("notification-card")))
                {
                    var notification = (DemoNotification)card.DataContext!;
                    var key = notification.Kind switch
                    {
                        DemoNotificationKind.Success => "SemiColorSuccess",
                        DemoNotificationKind.Warning => "SemiColorWarning",
                        DemoNotificationKind.Error => "SemiColorDanger",
                        _ => "SemiColorInformation",
                    };
                    var glyph = card.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("notification-glyph"));
                    var background = ColorOf(card.Background!, canvas);
                    Require(ColorOf(glyph.Foreground!, background) == Resource(shell, key), mode.Name + "/Notification/" + notification.Kind + "/dynamic semantic color");
                    Check(glyph.Foreground!, background, mode.Name, "Notification/" + notification.Kind);
                    foreach (var label in card.GetVisualDescendants().OfType<TextBlock>().Where(label => !label.Classes.Contains("notification-glyph")))
                        Check(label.Foreground!, background, mode.Name, "Notification/" + notification.Kind + "/text-" + label.FontSize, label.Opacity);
                }
                Save(shell, Path.Combine(output, mode.Name + "-overlay.png"));
                model.CommandPaletteOpen = false;

                var steps = scene.GetVisualDescendants().OfType<Button>().Where(button => button.Classes.Contains("scene-step")).ToArray();
                Require(steps[1].Classes.Contains("active"), mode.Name + "/Scene active state");
                Require(ColorOf(steps[1].BorderBrush!, canvas) == accent, mode.Name + "/Existing scene active border follows palette");
                Require(ColorOf(steps[2].BorderBrush!, canvas) == Resource(shell, "StudioBorder"), mode.Name + "/Existing scene normal border follows palette");
                var doneMarker = steps[0].GetVisualDescendants().OfType<Border>().Single(border => border.Classes.Contains("step-no"));
                var doneText = (TextBlock)doneMarker.Child!;
                Require(doneMarker.Classes.Contains("done") && ColorOf(doneText.Foreground!, raised) == Resource(shell, "SemiColorSuccess"),
                    mode.Name + "/Existing scene completed marker follows semantic palette");
                Check(doneText.Foreground!, ColorOf(doneMarker.Background!, canvas), mode.Name, "Scene/completed");
                CheckText(steps[1], canvas, mode.Name, "Scene/active");
                Require(ColorOf(firstBlock.Background!, canvas) == raised, mode.Name + "/Existing generated block follows palette");
                Require(ColorOf(((TextBlock)custom.LeftContent!).Foreground!, canvas) == accent,
                    mode.Name + "/Existing custom window title icon follows palette");
                var hero = extended.GetVisualDescendants().OfType<Border>().Single(border => border.Height == 72);
                Require(ColorOf(hero.Background!, canvas) == Resource(shell, "StudioPrimary"), mode.Name + "/Existing extended window follows palette");
                Check(((TextBlock)hero.Child!).Foreground!, ColorOf(hero.Background!, canvas), mode.Name, "Window/extended");
                Save(sceneWindow, Path.Combine(output, mode.Name + "-scene.png"));
                Console.WriteLine($"{(failures.Count == previousFailures ? "PASS" : "FAIL")} Demo palette states: {mode.Name}, failures={failures.Count - previousFailures}");
            }

            VerifyDesktopNotification();
            if (args.Contains("--catalog", StringComparer.Ordinal))
                CaptureCatalog();
            File.WriteAllLines(Path.Combine(output, "contrast.tsv"), measurements);
            File.WriteAllLines(Path.Combine(output, "failures.txt"), failures);
            Console.WriteLine($"Demo palette audit: {modes.Length} modes, {measurements.Count - 1} contrast measurements, {failures.Count} failures; {output}");
            if (failures.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, failures));
            return 0;

            void VerifyDesktopNotification()
            {
                var previousVariant = Application.Current!.RequestedThemeVariant;
                var manager = I18nManager.Instance;
                var localizer = new JsonLocalizer(typeof(NotificationWindow).Assembly);
                var previousCulture = manager.Culture;
                var notification = new NotificationWindow();
                notification.Configure("Native palette verification", TimeSpan.Zero, null, null, DesktopNotificationAttentionMode.None);
                notification.AddLogs([(new CodeWFLogEvent { Sequence = 1, Timestamp = DateTimeOffset.UtcNow,
                    Level = Microsoft.Extensions.Logging.LogLevel.Error, CategoryName = "Theme verification",
                    Message = "Open notification retains its content while the theme and language change.", RequestNotification = true },
                    "Open notification retains its content while the theme and language change.")]);
                var initialFailureCount = failures.Count;
                try
                {
                    notification.Show();
                    var confirm = notification.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("confirm"));
                    foreach (var mode in modes)
                    {
                        Application.Current.RequestedThemeVariant = StudioTheme.GetVariant(mode.Palette, mode.Dark);
                        Pump(notification);
                        var canvas = Resource(notification, "StudioBackground");
                        Require(ColorOf(confirm.Background!, canvas) == Resource(notification, "StudioPrimary"), mode.Name + "/Desktop notification follows brand");
                        CheckText(confirm, canvas, mode.Name, "desktop-notification-normal");
                        var point = confirm.TranslatePoint(new Point(8, 8), notification)!.Value;
                        notification.MouseMove(point);
                        Pump(notification);
                        Require(ColorOf(confirm.Background!, canvas) == Resource(notification, "StudioPrimaryHover"), mode.Name + "/Desktop notification hover follows brand");
                        CheckText(confirm, canvas, mode.Name, "desktop-notification-hover");
                        notification.MouseDown(point, MouseButton.Left);
                        Pump(notification);
                        Require(ColorOf(confirm.Background!, canvas) == Resource(notification, "StudioPrimaryPressed"), mode.Name + "/Desktop notification pressed follows brand");
                        CheckText(confirm, canvas, mode.Name, "desktop-notification-pressed");
                        notification.MouseMove(new Point(1, 1));
                        notification.MouseUp(new Point(1, 1), MouseButton.Left);
                        Pump(notification);
                        var labels = new HashSet<string>();
                        foreach (var culture in new[] { "zh-CN", "en-US" })
                        {
                            manager.Culture = new CultureInfo(culture);
                            Pump(notification);
                            Require(Equals(confirm.Content, localizer.GetResource(CodeWF.Avalonia.Log.I18n.ResourceKeys.Notification.Confirm)), mode.Name + "/Open desktop notification changes language: " + culture);
                            labels.Add(Convert.ToString(confirm.Content, CultureInfo.InvariantCulture) ?? string.Empty);
                        }
                        Require(labels.Count == 2, mode.Name + "/Open desktop notification renders distinct translated labels");
                        Save(notification, Path.Combine(output, mode.Name + "-desktop-notification.png"));
                    }
                }
                finally
                {
                    notification.Close();
                    Application.Current.RequestedThemeVariant = previousVariant;
                    manager.Culture = previousCulture;
                }
                Console.WriteLine($"{(failures.Count == initialFailureCount ? "PASS" : "FAIL")} live desktop notifications: 16 palette/mode combinations, normal/hover/pressed and open-window language changes");
            }

            void CaptureCatalog()
            {
                var pageFilter = Option("--page");
                var cultures = args.Contains("--all-cultures", StringComparer.Ordinal)
                    ? new[] { "zh-CN", "en-US" } : new[] { Option("--culture") ?? "zh-CN" };
                var items = model.Groups.SelectMany(group => group.Items)
                    .Where(item => pageFilter is null || item.PrototypeId == pageFilter || item.Api == pageFilter).ToArray();
                Require(items.Length > 0, "Catalog filter matches at least one page: " + pageFilter);
                foreach (var item in items)
                {
                    model.OpenItem(item);
                    foreach (var culture in cultures)
                    {
                        model.SelectedCulture = model.Cultures.Single(option => option.Name == culture);
                        foreach (var mode in modes)
                        {
                            model.SelectedPalette = model.Palettes.Single(palette => palette.Palette == mode.Palette);
                            model.IsDarkMode = mode.Dark;
                            shell.RequestedThemeVariant = StudioTheme.GetVariant(mode.Palette, mode.Dark);
                            Pump(shell);
                            Require(model.ActiveDocument!.Page.GetVisualAncestors().Contains(shell), "Catalog page is attached: " + item.Api);
                            Save(shell, Path.Combine(output, "catalog", culture, item.PrototypeId, mode.Name + ".png"));
                        }
                    }
                    Console.WriteLine("CAPTURE catalog page: " + item.PrototypeId);
                }
            }
        }
        finally
        {
            File.WriteAllLines(Path.Combine(output, "contrast.tsv"), measurements);
            File.WriteAllLines(Path.Combine(output, "failures.txt"), failures);
            foreach (var window in sampleOwner.OwnedWindows.ToArray()) window.Close();
            sampleOwner.Close();
            blocksWindow.Close();
            sceneWindow.Close();
            shell.Close();
        }

        string? Option(string key) => args.SkipWhile(argument => argument != key).Skip(1).FirstOrDefault();
        void Require(bool condition, string context) { if (!condition) failures.Add(context); }
        void Check(IBrush foreground, Color background, string mode, string state, double opacity = 1)
        {
            var text = ColorOf(foreground, background, opacity);
            var contrast = Contrast(text, background);
            measurements.Add(string.Join('\t', mode, state, text, background, contrast.ToString("F3", CultureInfo.InvariantCulture)));
            Require(contrast >= 4.5, $"{mode}/{state}: {text} on {background} = {contrast:F2}:1");
        }
        void CheckText(Control control, Color behind, string mode, string state)
        {
            var presenter = control.GetVisualDescendants().OfType<ContentPresenter>().First(item => item.Name == "PART_ContentPresenter");
            var background = ColorOf(presenter.Background ?? Brushes.Transparent, behind);
            Check(presenter.Foreground!, background, mode, state);
            foreach (var label in control.GetVisualDescendants().OfType<TextBlock>())
                Check(label.Foreground!, background, mode, state + "/label-" + label.FontSize, label.Opacity);
        }
    }

    private static ContentPresenter Presenter(Button button) => button.GetVisualDescendants().OfType<ContentPresenter>()
        .Single(presenter => presenter.Name == "PART_ContentPresenter");

    private static Color Resource(Control control, string key)
    {
        if (!control.TryFindResource(key, control.ActualThemeVariant, out var resource) || resource is not ISolidColorBrush brush)
            throw new InvalidOperationException("Missing solid theme resource: " + key);
        return brush.Color;
    }

    private static void Pump(params Window[] windows)
    {
        for (var pass = 0; pass < 3; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            foreach (var window in windows) window.UpdateLayout();
        }
    }

    private static void Save(Window window, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var bitmap = new RenderTargetBitmap(new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height));
        bitmap.Render(window);
        using var stream = File.Create(path);
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
    }

    // 透明画刷先合成到底色，再计算相对亮度与文本对比度。
    private static Color ColorOf(IBrush brush, Color background, double opacity = 1)
    {
        if (brush is not ISolidColorBrush solid) throw new InvalidOperationException("Expected resolved solid brush: " + brush);
        var alpha = solid.Color.A / 255d * solid.Opacity * opacity;
        byte Blend(byte front, byte back) => (byte)Math.Round(front * alpha + back * (1 - alpha));
        return Color.FromRgb(Blend(solid.Color.R, background.R), Blend(solid.Color.G, background.G), Blend(solid.Color.B, background.B));
    }

    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte component)
        {
            var value = component / 255d;
            return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4);
        }
        static double Light(Color color) => .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        return (Math.Max(Light(first), Light(second)) + .05) / (Math.Min(Light(first), Light(second)) + .05);
    }
}
