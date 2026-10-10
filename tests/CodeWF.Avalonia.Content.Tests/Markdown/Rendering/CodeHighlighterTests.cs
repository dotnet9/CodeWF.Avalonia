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
