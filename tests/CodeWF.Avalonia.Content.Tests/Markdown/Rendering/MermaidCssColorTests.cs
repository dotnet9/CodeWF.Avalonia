using System.Xml.Linq;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Mermaid;
using Mermaider;
using Mermaider.Models;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// Skia 不实现 CSS 级联：fill 上留着 var()/color-mix() 会被当成无效颜色直接画黑。
/// 象限图四格这类「带回退的嵌套表达式」是旧的字面量替换覆盖不到的形态，这里把它钉住。
/// </summary>
public sealed class MermaidCssColorTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private const string Quadrant = """
        quadrantChart
            title 覆盖率
            x-axis 低 --> 高
            y-axis 差 --> 好
            quadrant-1 重点
            quadrant-2 保持
            quadrant-3 观察
            quadrant-4 调整
            甲: [0.3, 0.6]
            乙: [0.75, 0.8]
        """;

    [Theory]
    [InlineData("#FFFFFF", "#27272A")]
    [InlineData("#18181B", "#FAFAFA")]
    public void QuadrantCellsResolveToFourDistinctNonBlackFills(string background, string foreground)
    {
        var options = new RenderOptions { Bg = background, Fg = foreground, Accent = "#8B7CF6" };
        var svg = MermaidBlockRenderer.InlineCssVariables(MermaidRenderer.RenderSvg(Quadrant, options), options);
        var document = XDocument.Parse(svg);

        // 四格是带字面量 fill 的方块，外框走 fill="none"，因此按 fill 的形态把它们挑出来即可。
        var cells = document.Descendants(Svg + "rect")
            .Select(rect => rect.Attribute("fill")?.Value)
            .Where(fill => fill is { Length: 7 } && fill![0] == '#')
            .Select(fill => fill!.ToUpperInvariant())
            .ToArray();

        Assert.Equal(4, cells.Length);
        Assert.DoesNotContain("#000000", cells);
        Assert.Equal(4, cells.Distinct().Count());
    }

    [Theory]
    [InlineData("quadrantChart\n    x-axis 低 --> 高\n    y-axis 差 --> 好\n    quadrant-1 A\n    甲: [0.3, 0.6]")]
    [InlineData("gantt\n    title 里程碑\n    dateFormat  YYYY-MM-DD\n    section 一期\n    设计 :a1, 2026-01-01, 7d\n    开发 :after a1, 10d")]
    [InlineData("C4Context\n    title 系统\n    Person(p, 用户, \"使用\")\n    System(s, 服务, \"核心\")\n    Rel(p, s, \"调用\")")]
    [InlineData("flowchart LR\n    A[开始] --> B{判断}\n    B -->|是| C[完成]\n    B -->|否| A")]
    public void NoCssFunctionSurvivesInAnyPaintAttribute(string source)
    {
        var options = new RenderOptions { Bg = "#18181B", Fg = "#FAFAFA", Accent = "#8B7CF6" };
        var svg = MermaidBlockRenderer.InlineCssVariables(MermaidRenderer.RenderSvg(source, options), options);

        Assert.All(XDocument.Parse(svg).Descendants().Attributes()
            .Where(attribute => attribute.Name.LocalName is "fill" or "stroke" or "font-size"),
            attribute =>
            {
                Assert.DoesNotContain("var(", attribute.Value);
                Assert.DoesNotContain("color-mix(", attribute.Value);
            });
    }

    [Fact]
    public void FallbackChainPrefersTheTokenAndFallsBackWhenItIsAbsent()
    {
        Assert.Equal("#FAFAFA", CssColorResolver.Flatten("var(--accent, var(--fg))",
            new Dictionary<string, string> { ["--fg"] = "#FAFAFA" }));
        Assert.Equal("#3B82F6", CssColorResolver.Flatten("var(--accent, var(--fg))",
            new Dictionary<string, string> { ["--accent"] = "#3B82F6", ["--fg"] = "#FAFAFA" }));
    }

    /// <summary>只写一侧百分比时另一侧取补数；把它当成 50% 会把 16% 算成 16/66，整张图的底色一起偏深。</summary>
    [Theory]
    [InlineData("color-mix(in srgb, #4E79A7 16%, var(--bg))", "#E2E9F0")]
    [InlineData("color-mix(in srgb, #4E79A7, var(--bg) 84%)", "#E2E9F0")]
    [InlineData("color-mix(in srgb, var(--bg) 50%, #4E79A7 50%)", "#A6BCD3")]
    [InlineData("color-mix(in srgb, var(--bg) 10%, #4E79A7 30%)", "#7A9ABD")]
    public void MixUsesTheCssPercentageRule(string expression, string expected)
        => Assert.Equal(expected, CssColorResolver.Flatten(expression,
            new Dictionary<string, string> { ["--bg"] = "#FFFFFF" }));

    [Fact]
    public void TransparentContributesNoColourToTheVisibleSide()
    {
        // 预乘插值下透明一侧不贡献通道值；按不透明通道直接混合会把白色拽向黑。
        Assert.StartsWith("rgba(255, 255, 255,", CssColorResolver.Flatten(
            "color-mix(in srgb, transparent 50%, #FFFFFF 50%)", new Dictionary<string, string>()));
    }

    [Fact]
    public void UnresolvableFunctionsAreLeftVerbatimInsteadOfCorrupted()
    {
        const string markup = "<rect fill=\"color-mix(in oklab, var(--nope), #000)\"/>";
        Assert.Equal(markup, CssColorResolver.Flatten(markup, new Dictionary<string, string>()));
    }

    /// <summary>宿主显式给出的 Muted 色要压过镜像公式，否则这一档主题色永远读不到。</summary>
    [Fact]
    public void HostMutedOverridesTheDerivedTint()
    {
        const string markup = "fill=\"var(--muted, #666666)\"";
        var options = new RenderOptions { Bg = "#18181B", Fg = "#FAFAFA", Muted = "#FF0000" };
        Assert.Equal("fill=\"#FF0000\"", MermaidBlockRenderer.InlineCssVariables(markup, options));

        // 没给 Muted 时才回到 fg 55% 的镜像公式，保持既有主题下的既有色值。
        Assert.Equal("fill=\"#949495\"", MermaidBlockRenderer.InlineCssVariables(markup, options with { Muted = null }));
    }
}
