using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.MindView;
using CodeWF.Avalonia.MindView.Controls;
using CodeWF.Avalonia.MindView.Markdown;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.MindView;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownMindMapAppearanceTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void TitlesRemainReadableInEveryStylePaletteAndMode() => platform.Run(() =>
    {
        var view = new MarkdownMindMapView { Root = MarkdownMindMapDocument.Parse("# Root\n\n## First\n\nSummary"), EnableMotion = false };
        var window = new Window { Width = 1000, Height = 700, Content = view }; window.Show();
        try
        {
            foreach (var palette in StudioPalettes.All) foreach (var dark in new[] { false, true })
            {
                var tokens = StudioPaletteEngine.Derive(palette.Key, palette, dark);
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette.Key, dark);
                view.AccentColor = Color.Parse(dark ? palette.Dark.Accent : palette.Light.Accent);
                view.CardBrush = new SolidColorBrush(tokens["StudioPaperColor"]);
                view.PanelBrush = new SolidColorBrush(tokens["StudioSurfaceColor"]);
                view.TextBrush = new SolidColorBrush(tokens["StudioTextColor"]);
                view.MutedTextBrush = new SolidColorBrush(tokens["StudioMutedColor"]);
                foreach (var style in Enum.GetValues<MindMapStyleKind>())
                {
                    view.StyleKind = style; Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                    foreach (var title in view.GetVisualDescendants().OfType<TextBlock>().Where(t => t.Text is "Root" or "First" or "Summary"))
                    {
                        var card = title.GetVisualAncestors().OfType<Border>().First();
                        var bg = ((ISolidColorBrush)card.Background!).Color;
                        if (bg.A == 0) bg = tokens["StudioPaperColor"];
                        var ink = ((ISolidColorBrush)title.Foreground!).Color;
                        Assert.True(Contrast(ink, bg) >= 4.5, $"{palette.Key}/{dark}/{style}/{title.Text}: {ink} on {bg}");
                    }
                }
            }
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData(MindMapStyleKind.Quiet)]
    [InlineData(MindMapStyleKind.Colorful)]
    [InlineData(MindMapStyleKind.Outline)]
    [InlineData(MindMapStyleKind.Capsule)]
    [InlineData(MindMapStyleKind.Underline)]
    [InlineData(MindMapStyleKind.Mono)]
    public void PointerSelectionPreservesTheOriginalCardTreatment(MindMapStyleKind style) => platform.Run(() =>
    {
        var view = new MarkdownMindMapView { Root = MarkdownMindMapDocument.Parse("# Root\n\n## First\n\n## Second"), StyleKind = style, EnableMotion = false };
        var window = new Window { Width = 1000, Height = 700, Content = view }; window.Show();
        try
        {
            view.Fit(.8); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            Border Card(string title) => view.GetVisualDescendants().OfType<Border>().Single(b => ToolTip.GetTip(b)?.ToString() == title);
            var first = Card("First");
            var thickness = first.BorderThickness;
            var stroke = ((ISolidColorBrush)first.BorderBrush!).Color; var fill = ((ISolidColorBrush)first.Background!).Color;
            void Click(Border card)
            {
                var point = card.TranslatePoint(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2), window)!.Value;
                window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left); Dispatcher.UIThread.RunJobs();
            }
            Click(first); Assert.Equal("First", view.SelectedNode!.Title);
            Assert.Equal(thickness, first.BorderThickness);
            Click(Card("Second")); Assert.Equal("Second", view.SelectedNode!.Title);
            Assert.Equal(thickness, first.BorderThickness);
            Assert.Equal(stroke, ((ISolidColorBrush)first.BorderBrush!).Color);
            Assert.Equal(fill, ((ISolidColorBrush)first.Background!).Color);
            Assert.Equal(default, first.BoxShadow);
        }
        finally { window.Close(); }
    });

    private static double Contrast(Color a, Color b)
    {
        static double L(Color c)
        {
            static double C(byte n) => n / 255d <= .04045 ? n / 255d / 12.92 : Math.Pow((n / 255d + .055) / 1.055, 2.4);
            return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B);
        }
        return (Math.Max(L(a), L(b)) + .05) / (Math.Min(L(a), L(b)) + .05);
    }
}
