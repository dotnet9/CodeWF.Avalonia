using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
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
        Assert.Equal("0,1,0,1", ArticleTypographyCatalog.Find("Column")!.Quote.Border);
        Assert.Equal("Center", ArticleTypographyCatalog.Find("WeChatFormat")!.Quote.Alignment);
    }
    [Fact]
    public void CatalogContains27IndependentNativeAndExportStyles()
    {
        platform.Run(() =>
        {
        Assert.Equal(27, ArticleTypographyCatalog.All.Count);
        Assert.Equal(27, ArticleTypographyCatalog.All.Select(t => t.Key).Distinct().Count());
        Assert.Equal(27, ArticleTypographyCatalog.All.Select(t => t.ExportCss).Distinct().Count());
        Assert.Equal(6, ArticleTypographyCatalog.All.Count(t => t.Recommended));
        foreach (var type in ArticleTypographyCatalog.All)
        {
            Assert.True(MarkdownTypographyThemeRegistry.Contains(type.Key), type.Key);
            Assert.True(type.BodySize >= 16, type.Key);
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
}
