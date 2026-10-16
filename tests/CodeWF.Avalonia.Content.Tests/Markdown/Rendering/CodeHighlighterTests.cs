using CodeWF.Avalonia.Markdown;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.LogicalTree;
using global::Avalonia.Media;
using CodeWF.Avalonia.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class CodeHighlighterTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false, "#F6F8FA")]
    [InlineData(false, "#F7F7F7")]
    [InlineData(true, "#0D1117")]
    [InlineData(true, "#202124")]
    [InlineData(true, "#282C34")]
    public void CodeLineNumbersAndSelectedTextRemainReadable(bool dark, string codeBackground) => platform.Run(() =>
    {
        var control = CodeHighlighter.Render("first\nsecond", "text", dark, FontFamily.Default, 13, 22);
        var blocks = control.GetLogicalDescendants().OfType<SelectableTextBlock>().ToArray();
        var code = blocks.Single(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        var numbers = blocks.Single(block => !block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        AssertReadable(numbers.Foreground!, Color.Parse(codeBackground), numbers.Opacity);
        var selection = Assert.IsAssignableFrom<ISolidColorBrush>(code.SelectionBrush);
        Assert.Equal(255, selection.Color.A);
        Assert.Equal(1, selection.Opacity);
        AssertReadable(code.SelectionForegroundBrush!, selection.Color, 1);
    });

    private static void AssertReadable(IBrush foreground, Color background, double opacity)
    {
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(foreground);
        var alpha = brush.Opacity * brush.Color.A / 255d * opacity;
        byte Blend(byte front, byte back) => (byte)Math.Round(front * alpha + back * (1 - alpha));
        var color = Color.FromRgb(Blend(brush.Color.R, background.R), Blend(brush.Color.G, background.G), Blend(brush.Color.B, background.B));
        static double Linear(byte value) => value / 255d <= .04045 ? value / 255d / 12.92 : Math.Pow((value / 255d + .055) / 1.055, 2.4);
        static double Luminance(Color value) => .2126 * Linear(value.R) + .7152 * Linear(value.G) + .0722 * Linear(value.B);
        var a = Luminance(color);
        var b = Luminance(background);
        var contrast = (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
        Assert.True(contrast >= 4.5, $"{color} on {background}: {contrast:F2}:1");
    }

    [Theory]
    [InlineData("csharp", false)]
    [InlineData("cs", true)]
    [InlineData("c#", true)]
    public void CSharpAliases_ProduceColoredTokensWithoutDefaultEmphasis(string language, bool dark) => platform.Run(() =>
    {
        var control = CodeHighlighter.Render("public int value = 42; // comment", language, dark,
            FontFamily.Default, 13, 22);
        var text = control.GetLogicalDescendants().OfType<SelectableTextBlock>().Single(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        var runs = text.Inlines!.OfType<Run>().ToArray();
        Assert.True(runs.Select(run => run.Foreground).OfType<ISolidColorBrush>().Select(brush => brush.Color).Distinct().Count() >= 3);
        Assert.Contains(runs, run => run.FontStyle == FontStyle.Normal && run.FontWeight == FontWeight.Normal);
    });
}
