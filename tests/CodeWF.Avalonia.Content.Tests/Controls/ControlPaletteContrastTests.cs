using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.Controls.Themes;
using Semi.Avalonia;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class ControlPaletteContrastTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void ExistingStatusAndGuideControlsKeepReadableTextAcrossAllPaletteSwitches() => VerifyControls(true);

    [Fact]
    public void StandaloneControlsKeepReadableTextInNativeSemiThemes() => VerifyControls(false);

    private void VerifyControls(bool studio) => platform.Run(() =>
    {
        var window = new Window { Width = 800, Height = 1100 };
        window.Styles.Add(new SemiTheme());
        window.Styles.Add(new ACSemiTheme());
        if (studio) window.Styles.Add(new StudioTheme());
        var panel = new StackPanel();
        var defaultBadge = new StatusBadge { LeftText = "Default", RightText = "42" };
        panel.Children.Add(defaultBadge);
        var badges = Enum.GetValues<StatusLabelKind>().Select(kind =>
        {
            var badge = new StatusBadge { LeftText = kind.ToString(), RightText = "42" };
            badge.Classes.Add(kind.ToString());
            panel.Children.Add(badge);
            return badge;
        }).Append(defaultBadge).ToArray();
        var cards = Enum.GetValues<StatusLabelKind>().Select(kind =>
        {
            var card = new StatusCard { Kind = kind, TopText = "State", BottomText = kind.ToString() };
            panel.Children.Add(card);
            return card;
        }).ToArray();
        var guideAction = new Button { Content = "Next" };
        var titleBar = new CodeWFTitleBar { Title = "Title" };
        panel.Children.Add(guideAction);
        panel.Children.Add(titleBar);
        window.Content = panel;
        var failures = new List<string>();
        var captureDirectory = Environment.GetEnvironmentVariable("CODEWF_PALETTE_CAPTURE_DIRECTORY");
        if (!string.IsNullOrWhiteSpace(captureDirectory)) Directory.CreateDirectory(captureDirectory);
        try
        {
            window.Show();
                guideAction.Theme = (ControlTheme)window.FindResource("GuidePrimaryActionButton")!;
            var variants = studio
                ? StudioPaletteTestHost.Keys().SelectMany(palette => new[] { false, true }.Select(dark => StudioTheme.GetVariant(palette, dark)))
                : new[] { ThemeVariant.Light, ThemeVariant.Dark, SemiTheme.Aquatic, SemiTheme.Desert, SemiTheme.Dusk, SemiTheme.NightSky };
            foreach (var variant in variants)
            {
                window.RequestedThemeVariant = variant;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.True(window.TryFindResource("CodeWFWindowBackground", window.ActualThemeVariant, out var canvasBrush));
                var canvas = ColorOf((IBrush)canvasBrush!, Colors.White);
                var prefix = variant.ToString();
                foreach (var badge in badges)
                {
                    var outer = badge.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_OuterBorder");
                    var left = badge.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_LeftTextBorder");
                    var leftText = left.GetVisualDescendants().OfType<SelectableTextBlock>().Single();
                    var rightText = badge.GetVisualDescendants().OfType<SelectableTextBlock>().Single(t => t.Name == "PART_RightText");
                    var background = ColorOf(outer.Background!, canvas);
                    Check(leftText.Foreground!, ColorOf(left.Background!, background), prefix + "/Badge/" + badge.LeftText + "/Left");
                    Check(rightText.Foreground!, background, prefix + "/Badge/" + badge.LeftText + "/Right");
                }
                foreach (var card in cards)
                {
                    var border = card.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Root");
                    var background = ColorOf(border.Background!, canvas);
                    foreach (var label in card.GetVisualDescendants().OfType<SelectableTextBlock>())
                        Check(label.Foreground!, background, prefix + "/Card/" + card.Kind + "/" + label.Text);
                }
                Check(guideAction.Foreground!, ColorOf(guideAction.Background!, canvas), prefix + "/Guide/Action");
                Check(titleBar.Foreground!, canvas, prefix + "/TitleBar");
                var location = guideAction.TranslatePoint(new Point(8, 8), window)!.Value;
                window.MouseMove(location);
                Dispatcher.UIThread.RunJobs();
                Check(guideAction.Foreground!, ColorOf(guideAction.Background!, canvas), prefix + "/Guide/Hover");
                window.MouseDown(location, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                Check(guideAction.Foreground!, ColorOf(guideAction.Background!, canvas), prefix + "/Guide/Pressed");
                window.MouseUp(location, MouseButton.Left);
                window.MouseMove(new Point(799, 1099));
                guideAction.IsEnabled = false;
                window.UpdateLayout();
                Assert.True(guideAction.Opacity < 0.6, prefix + "/Guide/Disabled");
                guideAction.IsEnabled = true;
                if (!string.IsNullOrWhiteSpace(captureDirectory))
                {
                    using var bitmap = new RenderTargetBitmap(new PixelSize(800, 1100), new Vector(96, 96));
                    bitmap.Render(window);
                    var fileName = (studio ? "studio-" : "semi-") + System.Text.RegularExpressions.Regex.Replace(prefix, @"[^\w.-]", "_") + ".png";
                    bitmap.Save(Path.Combine(captureDirectory, fileName), new PngBitmapEncoderOptions());
                }
            }
        }
        finally { window.Close(); }
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));

        void Check(IBrush foreground, Color background, string context)
        {
            var text = ColorOf(foreground, background);
            var contrast = Contrast(text, background);
            if (contrast < 4.5) failures.Add($"{context}: {text} on {background} = {contrast:F2}:1");
        }
    });

    // 半透明画刷必须先与实际底色合成，不能仅比较 RGB 而忽略透明度。
    private static Color ColorOf(IBrush brush, Color background)
    {
        var solid = Assert.IsAssignableFrom<ISolidColorBrush>(brush);
        var color = solid.Color;
        var alpha = color.A / 255d * solid.Opacity;
        byte Blend(byte front, byte back) => (byte)Math.Round(front * alpha + back * (1 - alpha));
        return Color.FromRgb(Blend(color.R, background.R), Blend(color.G, background.G), Blend(color.B, background.B));
    }

    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte component)
        {
            var value = component / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        static double Luminance(Color color) => .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
}
