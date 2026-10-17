using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.LogicalTree;
using Markdig;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownHtmlTableTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void PandocTableRendersCellsAndRichContentWithoutChangingSource() => platform.Run(() =>
    {
        const string source = """
            <table>
            <colgroup><col style="width: 10%" /><col style="width: 90%" /></colgroup>
            <thead><tr><th><strong>编号</strong></th><th>说明</th></tr></thead>
            <tbody><tr><td>MOD-01</td><td><ol><li><p>创建 &amp; 验证</p></li><li><p><code>a | b</code></p></li></ol></td></tr></tbody>
            </table>
            """;
        var parsed = Markdig.Markdown.Parse(source);
        var result = Assert.IsType<Border>(new HtmlTableBlockRenderer().TryRender(parsed[0], source, new MarkdownViewer()));
        var grid = Assert.IsType<MarkdownTableGrid>(result.Child);
        Assert.Equal(2, grid.ColumnDefinitions.Count);
        Assert.Equal(2, grid.RowDefinitions.Count);
        Assert.Equal(4, grid.Children.Count);
        var texts = grid.GetLogicalDescendants().OfType<SelectableTextBlock>().Select(text => text.Text ?? text.Inlines?.Text ?? string.Empty).ToArray();
        Assert.Contains(texts, text => text.Contains("MOD-01"));
        Assert.Contains(texts, text => text.Contains("创建 & 验证") && text.Contains("a | b"));
        Assert.DoesNotContain(texts, text => text.Contains("<td>") || text.Contains("<col"));
        Assert.Contains(grid.GetLogicalDescendants().OfType<SelectableTextBlock>(), text => text.Inlines?.Any(inline => inline is global::Avalonia.Controls.Documents.LineBreak) == true);
        Assert.All(grid.Children.Take(2), cell => Assert.Contains(MarkdownStyleKeys.TableHeaderCell, cell.Classes));

        result.Measure(new Size(280, double.PositiveInfinity));
        result.Arrange(new Rect(result.DesiredSize));
        Assert.InRange(grid.ColumnDefinitions.Sum(column => column.ActualWidth), 1, 281);
        Assert.Equal(source, Assert.IsType<Markdig.Syntax.HtmlBlock>(parsed[0]).Lines.ToString());
    });

    [Fact]
    public void TableWithoutThDoesNotInventAHeader() => platform.Run(() =>
    {
        const string source = "<table><tr><td>A</td><td>B</td></tr><tr><td>C</td><td>D</td></tr></table>";
        var result = Assert.IsType<Border>(new HtmlTableBlockRenderer().TryRender(Markdig.Markdown.Parse(source)[0], source, new MarkdownViewer()));
        var grid = Assert.IsType<MarkdownTableGrid>(result.Child);
        Assert.All(grid.Children, cell => Assert.DoesNotContain(MarkdownStyleKeys.TableHeaderCell, cell.Classes));
    });

    [Theory]
    [InlineData("<table><tr><td colspan=\"2\">Merged</td></tr></table>")]
    [InlineData("<table><tr><td><table><tr><td>Nested</td></tr></table></td></tr></table>")]
    [InlineData("<div>Other HTML</div>")]
    public void UnsupportedStructureKeepsTheExistingFallback(string source) => platform.Run(() =>
        Assert.Null(new HtmlTableBlockRenderer().TryRender(Markdig.Markdown.Parse(source)[0], source, new MarkdownViewer())));
}
