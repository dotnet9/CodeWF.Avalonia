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
    public void CatalogContains12IndependentNativeAndExportStyles()
    {
        platform.Run(() =>
        {
        Assert.Equal(12, ArticleTypographyCatalog.All.Count);
        Assert.Equal(12, ArticleTypographyCatalog.All.Select(t => t.Key).Distinct().Count());
        Assert.Equal(12, ArticleTypographyCatalog.All.Select(t => t.ExportCss).Distinct().Count());
        Assert.Equal(6, ArticleTypographyCatalog.All.Count(t => t.Recommended));
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
    [InlineData(MarkdownTypographySizes.Compact, 13d)]
    [InlineData(MarkdownTypographySizes.Normal, 15d)]
    [InlineData(MarkdownTypographySizes.Relaxed, 17d)]
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
}
