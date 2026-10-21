using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Log;
using CodeWF.Avalonia.Log.Notifications.Views;
using CodeWF.Toolkit.Logging;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class NotificationStudioPaletteTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void OpenDesktopNotificationFollowsBrandStatesAndHostResourceOverrides() => platform.Run(() =>
    {
        var application = Application.Current!;
        var originalVariant = application.RequestedThemeVariant;
        IStyle[] styles = [new global::Semi.Avalonia.SemiTheme(), new CodeWFLogTheme(), new StudioTheme()];
        foreach (var style in styles) application.Styles.Add(style);
        using var themeController = new StudioThemeController(application);
        var window = new NotificationWindow();
        window.Configure("Brand palette", TimeSpan.Zero, null, null, DesktopNotificationAttentionMode.None);
        var key = LogNotificationResourceKeys.ConfirmBackground;
        var hadOverride = application.Resources.ContainsKey(key);
        var originalOverride = hadOverride ? application.Resources[key] : null;
        try
        {
            window.Show();
            var button = window.GetVisualDescendants().OfType<Button>().Single(control => control.Classes.Contains("confirm"));
            foreach (var palette in StudioPaletteTestHost.Keys())
            foreach (var dark in new[] { false, true })
            {
                themeController.PaletteKey = palette;
                themeController.Mode = dark ? StudioThemeMode.Dark : StudioThemeMode.Light;
                application.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.Equal(Resource("StudioPrimary"), Solid(button.Background));
                Assert.Equal(Resource("StudioOnPrimary"), Solid(button.Foreground));
                var position = button.TranslatePoint(new Point(8, 8), window)!.Value;
                window.MouseMove(position);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(Resource("StudioPrimaryHover"), Solid(button.Background));
                window.MouseDown(position, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(Resource("StudioPrimaryPressed"), Solid(button.Background));
                // 移出后释放，避免触发确认命令而关闭正在重复换色的窗口。
                window.MouseMove(new Point(1, 1));
                window.MouseUp(new Point(1, 1), MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                var folder = Environment.GetEnvironmentVariable("CODEWF_PALETTE_CAPTURE_DIRECTORY");
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    Directory.CreateDirectory(folder);
                    using var bitmap = new RenderTargetBitmap(PixelSize.FromSize(window.Bounds.Size, 1), new Vector(96, 96));
                    bitmap.Render(window);
                    bitmap.Save(Path.Combine(folder, $"notification-{palette}-{(dark ? "dark" : "light")}.png"), new PngBitmapEncoderOptions());
                }
            }
            application.Resources[key] = new SolidColorBrush(Color.Parse("#184881"));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Color.Parse("#184881"), Solid(button.Background));
        }
        finally
        {
            window.Close();
            if (hadOverride) application.Resources[key] = originalOverride;
            else application.Resources.Remove(key);
            foreach (var style in styles) application.Styles.Remove(style);
            themeController.Dispose();
            application.RequestedThemeVariant = originalVariant;
        }

        Color Resource(string resourceKey)
        {
            Assert.True(window.TryFindResource(resourceKey, window.ActualThemeVariant, out var resource));
            return Solid(Assert.IsAssignableFrom<IBrush>(resource));
        }
        static Color Solid(IBrush? brush) => Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;
    });
}
