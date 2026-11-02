using CodeWF.Avalonia.Markdown;
using System.Xml.Linq;
using CodeWF.Avalonia.Markdown.Mermaid;
using Mermaider;
using Mermaider.Models;
using Xunit;
using global::Avalonia.Styling;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

public sealed class MermaidSvgCompatibilityTests
{
    [Theory]
    [MemberData(nameof(SankeySources))]
    public void SankeyExternalLabelsRemainInsideTheCanvas(string source)
    {
        var raw = MermaidBlockRenderer.InlineCssVariables(MermaidRenderer.RenderSvg(source, new RenderOptions()), new RenderOptions());
        var original = XDocument.Parse(raw);
        var fitted = XDocument.Parse(MermaidBlockRenderer.FitSankeyLabels(raw));
        var viewBox = fitted.Root!.Attribute("viewBox")!.Value.Split(' ').Select(value => double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var originalWidth = double.Parse(original.Root!.Attribute("viewBox")!.Value.Split(' ')[2], System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(viewBox[2] >= originalWidth, "标签适配不能缩小上游已分配的画布");
        if (source.Length > 100) Assert.True(viewBox[2] > originalWidth, "超长外置标签必须计入画布宽度");
        Assert.Equal(original.Descendants().Where(element => element.Name.LocalName == "text").Select(element => element.Value),
            fitted.Descendants().Where(element => element.Name.LocalName == "text").Select(element => element.Value));
        Assert.Contains("40", fitted.ToString());
    }

    public static IEnumerable<object[]> SankeySources =>
    [
        ["sankey-beta\nIdeas,Draft,40\nDraft,Published,30\nDraft,Revision,10"],
        ["sankey-beta\n非常长的文档来源名称,需要完整展示的最终目标名称,40"],
        [$"sankey-beta\n{new string('长', 100)},需要完整展示的最终目标名称,40"]
    ];

    [Fact]
    public async Task ConcurrentViewsReuseOneLoadedSvgAndPaletteChangesRemainIndependent()
    {
        var source = $"flowchart LR\nA[{Guid.NewGuid():N}] --> B[Done]";
        var options = new RenderOptions { Bg = "#FFFFFF", Fg = "#302C3C", Accent = "#8B7CF6", DataPalette = ["#8B7CF6", "#70A98B"] };
        var key = MermaidBlockRenderer.CreateCacheKey(source, options);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => MermaidBlockRenderer.ResolveSvgSourceAsync(key, source, options)));
        Assert.NotNull(results[0]);
        Assert.All(results, result => Assert.Same(results[0],result));
        var dark = options with { Bg = "#202124", Fg = "#EDE8F5", DataPalette = ["#B6A6FF", "#93CFAC"] };
        var other = await MermaidBlockRenderer.ResolveSvgSourceAsync(MermaidBlockRenderer.CreateCacheKey(source,dark),source,dark);
        Assert.NotNull(other);Assert.NotSame(results[0],other);
    }

    [Fact]
    public void PaletteChangesHaveIndependentCachedLayouts()
    {
        var first = new RenderOptions { DataPalette = ["#8B7CF6", "#72B59A"], Default = "#8B7CF6" };
        var second = first with { DataPalette = ["#E4B95B", "#D78FB6"] };
        Assert.NotEqual(MermaidBlockRenderer.CreateCacheKey("pie\n\"A\":2\n\"B\":1",first),
            MermaidBlockRenderer.CreateCacheKey("pie\n\"A\":2\n\"B\":1",second));
        var raw = MermaidRenderer.RenderSvg("pie\n\"A\":2\n\"B\":1",first);
        Assert.Contains("#8B7CF6",raw,StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#72B59A",raw,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CustomTheme_InheritsItsBasePalette()
    {
        Assert.True(MermaidBlockRenderer.IsDarkTheme(new ThemeVariant("NightSky", ThemeVariant.Dark)));
        Assert.True(MermaidBlockRenderer.IsDarkTheme(new ThemeVariant("NestedNight", new ThemeVariant("Night", ThemeVariant.Dark))));
        Assert.False(MermaidBlockRenderer.IsDarkTheme(new ThemeVariant("Aquatic", ThemeVariant.Light)));
    }

    [Theory]
    [InlineData("#FFFFFF", "#27272A", "#E7EAED")]
    [InlineData("#18181B", "#FAFAFA", "#23252B")]
    public void RectangularNodes_ResolveGradientColorsForSkia(string background, string foreground, string expectedFill)
    {
        var options = new RenderOptions { Bg = background, Fg = foreground, Accent = "#1677FF" };
        var raw = MermaidRenderer.RenderSvg("graph LR\nA[开始] --> B[结束]", options);
        var svg = MermaidBlockRenderer.InlineCssVariables(raw, options);
        var document = XDocument.Parse(svg);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var node = document.Descendants(ns + "g").First(element => element.Attribute("class")?.Value == "node");
        var fill = node.Element(ns + "rect")!.Attribute("fill")!.Value;
        Assert.StartsWith("url(#", fill);
        var gradient = document.Descendants(ns + "linearGradient").Single(element => element.Attribute("id")?.Value == fill[5..^1]);
        Assert.Equal(expectedFill, gradient.Elements(ns + "stop").First().Attribute("stop-color")!.Value);
        Assert.All(document.Descendants().Attributes().Where(attribute => attribute.Name.LocalName is "fill" or "stroke" or "font-size" or "stop-color"),
            attribute =>
            {
                Assert.DoesNotContain("color-mix(", attribute.Value);
                Assert.DoesNotContain("var(", attribute.Value);
            });
    }
}
