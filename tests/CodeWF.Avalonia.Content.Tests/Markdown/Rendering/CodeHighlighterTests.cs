using CodeWF.Avalonia.Markdown;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.LogicalTree;
using global::Avalonia.Media;
using CodeWF.Avalonia.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Highlighting;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.Controls.Templates;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class CodeHighlighterTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void LongHighlightedCodeIsChunkedWithoutRebuildingTheFullText() => platform.Run(() =>
    {
        const string line = "public int Compute(int value) => value * 42; // comment";
        CodeHighlighter.Render(line, "csharp", false, FontFamily.Default, 13, 22, null);
        var source = string.Join('\n', Enumerable.Repeat(line, 400));
        var before = GC.GetAllocatedBytesForCurrentThread();
        var control = CodeHighlighter.Render(source, "csharp", false, FontFamily.Default, 13, 22, null);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        var codeBlocks = control.GetLogicalDescendants().OfType<SelectableTextBlock>()
            .Where(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText)).ToArray();
        // 400 行按 128 行切块：128 + 128 + 128 + 16。
        Assert.Equal(4, codeBlocks.Length);
        Assert.Equal(source, string.Join('\n', codeBlocks.Select(block => block.Text)));
        Assert.True(allocated < 64L * 1024 * 1024, $"400 code lines allocated {allocated / 1048576d:F1} MiB");
        foreach (var block in codeBlocks)
        {
            block.Measure(new global::Avalonia.Size(800, double.PositiveInfinity));
        }

        Assert.Equal(400 * 22, codeBlocks.Sum(block => block.DesiredSize.Height), 1);
        var first = codeBlocks[0];
        first.SelectionStart = first.Text!.IndexOf("Compute", StringComparison.Ordinal);
        first.SelectionEnd = first.SelectionStart + 7;
        Assert.Equal("Compute", first.SelectedText);
        Assert.True(control.GetLogicalDescendants().Count() < 24, "Long code should not create a logical element for every token");
    });

    [Theory]
    [InlineData("var value = 1;", 1)]
    [InlineData("first\nsecond", 2)]
    [InlineData("first\n\n", 3)]
    public void CodeHeightAndLineNumbersMatchTheActualLines(string source, int lineCount) => platform.Run(() =>
    {
        var control = CodeHighlighter.Render(source, "csharp", false, FontFamily.Default, 13, 22, null);
        var blocks = control.GetLogicalDescendants().OfType<SelectableTextBlock>().ToArray();
        var code = blocks.Single(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        var numbers = blocks.Single(block => !block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        code.Measure(new global::Avalonia.Size(800, double.PositiveInfinity));
        Assert.Equal(lineCount * 22, code.DesiredSize.Height, 1);
        Assert.Equal(string.Join(Environment.NewLine, Enumerable.Range(1, lineCount)), numbers.Text);
    });

    [Theory]
    [InlineData("#0D000000", "#FFFFFF", false)]
    [InlineData("#14FFFFFF", "#14161A", true)]
    [InlineData("#14FFFFFF", "#EFEAF8", false)]
    [InlineData("#202020", "#FFFFFF", true)]
    public void ActualCodeBackgroundComposesTransparencyBeforeChoosingTokens(string codeColor, string pageColor, bool dark) => platform.Run(() =>
    {
        var viewer = new MarkdownViewer
        {
            CodeBackgroundBrush = Brush.Parse(codeColor),
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                var host = new StackPanel();
                scope.Register("PART_DocumentHost", host);
                return host;
            }),
            Markdown = "```text\nproject/\n  src/\n```"
        }.UseHighlighting();
        var window = new Window { Width = 800, Height = 500, RequestedThemeVariant = new ThemeVariant("Custom", dark ? ThemeVariant.Dark : ThemeVariant.Light),
            Content = new Border { Background = Brush.Parse(pageColor), Child = viewer } };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var document = Markdig.Markdown.Parse(viewer.Markdown!);
            var rendered = new CodeBlockRenderer().TryRender(document[0], viewer.Markdown, viewer)!;
            var text = rendered.GetLogicalDescendants().OfType<SelectableTextBlock>()
                .Single(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
            var front = Color.Parse(codeColor); var back = Color.Parse(pageColor);
            byte Blend(byte f, byte b) => (byte)Math.Round(f * front.A / 255d + b * (1 - front.A / 255d));
            var visibleBackground = Color.FromRgb(Blend(front.R, back.R), Blend(front.G, back.G), Blend(front.B, back.B));
            foreach (var run in text.Inlines!.OfType<Run>()) AssertReadable(run.Foreground!, visibleBackground, 1);
            var numbers = rendered.GetLogicalDescendants().OfType<SelectableTextBlock>().Single(block => block.Text?.StartsWith("1" + Environment.NewLine) == true);
            AssertReadable(numbers.Foreground!, visibleBackground, 1);
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData(false, "#F6F8FA")]
    [InlineData(false, "#F7F7F7")]
    [InlineData(true, "#0D1117")]
    [InlineData(true, "#202124")]
    [InlineData(true, "#282C34")]
    public void CodeLineNumbersAndSelectedTextRemainReadable(bool dark, string codeBackground) => platform.Run(() =>
    {
        var control = CodeHighlighter.Render("first\nsecond", "text", dark, FontFamily.Default, 13, 22, null);
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
            FontFamily.Default, 13, 22, null);
        var text = control.GetLogicalDescendants().OfType<SelectableTextBlock>().Single(block => block.Classes.Contains(MarkdownStyleKeys.CodeBlockText));
        var runs = text.Inlines!.OfType<Run>().ToArray();
        Assert.True(runs.Select(run => run.Foreground).OfType<ISolidColorBrush>().Select(brush => brush.Color).Distinct().Count() >= 3);
        Assert.Contains(runs, run => run.FontStyle == FontStyle.Normal && run.FontWeight == FontWeight.Normal);
    });
}
