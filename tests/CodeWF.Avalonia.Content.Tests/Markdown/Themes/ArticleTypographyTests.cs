using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Themes;
using Xunit;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Themes;

[Collection("AvaloniaPlatform")]
public sealed class ArticleTypographyTests(AvaloniaPlatformFixture platform)
{
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
