using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Themes;
using Xunit;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Runtime.InteropServices;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Themes;

[Collection("AvaloniaPlatform")]
public sealed class ArticleTypographyTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false, MarkdownTypographySizes.Normal)]
    [InlineData(true, MarkdownTypographySizes.Normal)]
    [InlineData(false, MarkdownTypographySizes.Compact)]
    [InlineData(true, MarkdownTypographySizes.Compact)]
    public void RenderedHeadingKeepsDashedBorderAndCanReturnToSolid(bool dark, string size) => platform.Run(() =>
    {
        var viewer = new MarkdownViewer { Markdown = "## Heading", TypographySize = size };
        viewer.Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<MarkdownViewer>((_, scope) =>
        {
            var panel = new StackPanel();
            scope.Register("PART_DocumentHost", panel);
            return panel;
        });
        var window = new Window
        {
            Content = viewer, Width = 300, Height = 200,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light
        };
        window.Styles.Add(new MarkdownThemes());
        window.Show();
        try
        {
            foreach (var theme in new[] { "GeekBlack", "Basic", "GeekBlack" })
            {
                viewer.TypographyTheme = theme;
                viewer.Rerender();
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                var heading = viewer.GetVisualDescendants().OfType<Border>()
                    .Single(border => border.Classes.Contains(MarkdownStyleKeys.Heading2Border));
                Assert.IsAssignableFrom<SelectableTextBlock>(heading.Child);
                var expectedStroke = Thickness.Parse(ArticleTypographyCatalog.Find(theme)!.Headings[1].Border).Bottom
                    * (size == MarkdownTypographySizes.Compact ? .87 : 1);
                Assert.Equal(expectedStroke, heading.BorderThickness.Bottom, 4);
                // 检查真实绘制的下边框：虚线有透明间隙，切回普通主题必须恢复实线。
                var paintedHeading = theme == "GeekBlack" ? (Control)heading.Parent! : heading;
                paintedHeading.Margin = default;
                heading.Margin = default;
                paintedHeading.Measure(new Size(240, 80));
                paintedHeading.Arrange(new Rect(0, 0, 240, 80));
                using var bitmap = new RenderTargetBitmap(new PixelSize(240, 80), new Vector(96, 96));
                bitmap.Render(paintedHeading);
                var pixels = new byte[240 * 80 * 4];
                var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                try { bitmap.CopyPixels(new PixelRect(0, 0, 240, 80), handle.AddrOfPinnedObject(), pixels.Length, 240 * 4); }
                finally { handle.Free(); }
                var painted = Enumerable.Range(0, 240).Count(x => pixels[(79 * 240 + x) * 4 + 3] > 128);
                if (theme == "GeekBlack") Assert.InRange(painted, 80, 200);
                else Assert.Equal(240, painted);
                Assert.Contains("Heading", viewer.GetRenderedText());
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void NativeSpacingPreservesTheDocumentRules()
    {
        var simple = ArticleTypographyCatalog.Find("Simple")!;
        Assert.Equal("0,0,0,17.6", simple.Headings[0].Margin);
        Assert.Equal(46.4, simple.Headings[0].LineHeight, 4);
        Assert.Equal("3,0,0,0", simple.Quote.Border);
        Assert.Equal(.06, ArticleTypographyCatalog.Find("InkBlack")!.Quote.Tint);
        Assert.Equal("Center", ArticleTypographyCatalog.Find("WeChatFormat")!.Quote.Alignment);
    }
    [Fact]
    public void CatalogContains13IndependentNativeAndExportStyles()
    {
        platform.Run(() =>
        {
        Assert.Equal(13, ArticleTypographyCatalog.All.Count);
        Assert.Equal(13, ArticleTypographyCatalog.All.Select(t => t.Key).Distinct().Count());
        Assert.Equal(13, ArticleTypographyCatalog.All.Select(t => t.ExportCss).Distinct().Count());
        Assert.Equal(7, ArticleTypographyCatalog.All.Count(t => t.Recommended));
        foreach (var type in ArticleTypographyCatalog.All)
        {
            Assert.True(MarkdownTypographyThemeRegistry.Contains(type.Key), type.Key);
            Assert.InRange(type.BodySize, 15d, 17d);
            Assert.Contains("color-scheme:light", type.ExportCss);
            var resources = type.CreateResources();
            foreach (var mode in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                var palette = Assert.IsType<ResourceDictionary>(resources.ThemeDictionaries[mode]);
                for (var level = 1; level <= 6; level++)
                {
                    var prefix = "CodeWFMarkdownHeading" + level;
                    Assert.IsType<double>(resources[prefix + "FontSize"]);
                    Assert.IsAssignableFrom<IBrush>(palette[prefix + "Foreground"]);
                    Assert.IsAssignableFrom<IBrush>(palette[prefix + "Background"]);
                }
            }
        }
        });
    }

    [Fact]
    public void CatalogRetainsBorderPaintCornersAndParagraphIndent()
    {
        platform.Run(() =>
        {
            var basic = ArticleTypographyCatalog.Find("Basic")!;
            var sage = ArticleTypographyCatalog.Find("SoftSage")!;
            var technology = ArticleTypographyCatalog.Find("TechnologyBlue")!;
            Assert.Equal("var(--border)", basic.Headings[1].BorderColor);
            Assert.True(ArticleTypographyCatalog.Find("GeekBlack")!.Headings[1].DashedBorder);
            Assert.Equal(new CornerRadius(0, 4, 4, 0), technology.Headings[1].Corners);
            Assert.Equal(.09, technology.Headings[1].Tint);
            Assert.Equal(new CornerRadius(0, 8, 8, 0), sage.Quote.Corners);
            Assert.Equal("var(--typography-accent)", sage.Quote.BorderColor);
            Assert.Equal(34, ArticleTypographyCatalog.Find("InkBlack")!.ParagraphIndent);
            foreach (var mode in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                var basicColors = Assert.IsType<ResourceDictionary>(basic.CreateResources().ThemeDictionaries[mode]);
                var sageColors = Assert.IsType<ResourceDictionary>(sage.CreateResources().ThemeDictionaries[mode]);
                Assert.Equal(((ISolidColorBrush)basicColors[MarkdownStyleKeys.BorderBrushResource]!).Color,
                    ((ISolidColorBrush)basicColors["CodeWFMarkdownHeading2BorderBrush"]!).Color);
                Assert.Equal(((ISolidColorBrush)sageColors[MarkdownStyleKeys.AccentBrushResource]!).Color,
                    ((ISolidColorBrush)sageColors["CodeWFMarkdownQuoteBorderBrush"]!).Color);
                Assert.Equal(new CornerRadius(0, 8, 8, 0), sage.CreateResources()["CodeWFMarkdownQuoteCornerRadius"]);
            }
        });
    }

    [Theory]
    [InlineData("leftBar", "4,0,0,0", "15.68,6.72,15.68,6.72", 0d, 0d)]
    [InlineData("underline", "0,0,0,2", "0,0,0,7.84", 0d, 0d)]
    [InlineData("tint", "0,0,0,0", "15.68,8.96,15.68,8.96", .1d, 4d)]
    [InlineData("none", "0,0,0,0", "0,0,0,0", 0d, 0d)]
    public void CustomTypographyRewritesOnlyTheH2Decoration(string decoration, string border, string padding,
        double tint, double radius)
    {
        platform.Run(() =>
        {
            var key = "user:typography-test";
            var custom = ArticleTypographyResolver.CreateCustom(key, "测试", "#24766B", "#4CBFA8",
                "#C4704A", "#D89A6E", "serif", 1.7d, decoration);
            var simple = ArticleTypographyCatalog.Find("Simple")!;

            Assert.Equal(border, custom.Headings[1].Border);
            Assert.Equal(padding, custom.Headings[1].Padding);
            Assert.Equal(tint, custom.Headings[1].Tint);
            Assert.Equal(radius, custom.Headings[1].Radius);
            // 装饰只动 H2：其余级别与字号、行距、字体沿用模板。
            Assert.Equal(simple.Headings[0], custom.Headings[0]);
            Assert.Equal(simple.Headings[2], custom.Headings[2]);
            Assert.Equal(simple.BodySize, custom.BodySize);
            Assert.Equal(1.7d, custom.Leading);
            Assert.Equal("serif", custom.Font);

            var resources = custom.CreateResources();
            Assert.Equal(Thickness.Parse(border), Assert.IsType<Thickness>(resources["CodeWFMarkdownHeading2BorderThickness"]));
            Assert.Equal(Thickness.Parse(padding), Assert.IsType<Thickness>(resources["CodeWFMarkdownHeading2Padding"]));
            Assert.Equal(new CornerRadius(radius), Assert.IsType<CornerRadius>(resources["CodeWFMarkdownHeading2CornerRadius"]));
            var light = Assert.IsType<ResourceDictionary>(resources.ThemeDictionaries[ThemeVariant.Light]);
            Assert.IsAssignableFrom<IBrush>(light["CodeWFMarkdownHeading2Background"]);
            Assert.IsAssignableFrom<IBrush>(light["CodeWFMarkdownHeading2BorderBrush"]);
        });
    }

    [Theory]
    [InlineData(MarkdownTypographySizes.Compact, 16d * .87)]
    [InlineData(MarkdownTypographySizes.Normal, 16d)]
    [InlineData(MarkdownTypographySizes.Relaxed, 16d * 1.13)]
    public void SizeTierInjectsBodyBaselineAndScalesHeadings(string size, double expectedBodySize)
    {
        platform.Run(() =>
        {
            var typography = ArticleTypographyCatalog.Find("Simple")!;
            var resources = MarkdownThemes.CreateTypographyResources(typography.Key, size);

            Assert.Equal(expectedBodySize, Assert.IsType<double>(resources[MarkdownStyleKeys.ParagraphFontSizeResource]));
            Assert.Equal(expectedBodySize * typography.Leading,
                Assert.IsType<double>(resources[MarkdownStyleKeys.ParagraphLineHeightResource]), 4);
            var h1 = Assert.IsType<double>(resources["CodeWFMarkdownHeading1FontSize"]);
            Assert.Equal(typography.Headings[0].Size * expectedBodySize / typography.BodySize, h1, 4);
        });
    }
    [Fact]
    public void RegisteredFactoryOverridesSurviveSizeScaling()
    {
        platform.Run(() =>
        {
            var article = ArticleTypographyCatalog.Find("Simple")!;
            try
            {
                MarkdownTypographyThemeRegistry.Register(article.Key, () =>
                {
                    var resources = article.CreateResources();
                    resources["CodeWFMarkdownHeading1Padding"] = new Thickness(0, 0, 0, 14);
                    resources["CodeWFMarkdownTableCellPadding"] = new Thickness(13, 10);
                    return resources;
                });
                var normal = MarkdownThemes.CreateTypographyResources(article.Key, MarkdownTypographySizes.Normal);
                Assert.Equal(new Thickness(0, 0, 0, 14), normal["CodeWFMarkdownHeading1Padding"]);
                Assert.Equal(new Thickness(13, 10), normal["CodeWFMarkdownTableCellPadding"]);
                var compact = MarkdownThemes.CreateTypographyResources(article.Key, MarkdownTypographySizes.Compact);
                Assert.Equal(new Thickness(0, 0, 0, 14 * .87), compact["CodeWFMarkdownHeading1Padding"]);
                Assert.Equal(16 * .87, compact[MarkdownStyleKeys.ParagraphFontSizeResource]);
            }
            finally { MarkdownTypographyThemeRegistry.Register(article.Key, article.CreateResources); }
        });
    }

}
