using Avalonia;
using Avalonia.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownNonTextSelectionTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClickAndDragUseModelEndpointsWhenBlockHasNoMatchingText(bool hasCaption) => platform.Run(() =>
    {
        const string source = "flowchart LR\nA --> B";
        var diagram = new Border
        {
            Child = hasCaption ? new SelectableTextBlock { Text = "Diagram preview" } : new Image()
        };
        diagram.Arrange(new Rect(0, 0, 200, 120));
        var selection = new MarkdownSelectionController();
        selection.SetBlocks([new MarkdownSelectionBlock(diagram, 0, new MarkdownTextSpan(0, source.Length), source)]);

        Assert.True(selection.Begin(new Point(100, 20)));
        selection.CommitClickWithoutDrag();
        Assert.False(selection.HasSelection);
        Assert.Equal(string.Empty, selection.SelectedText);

        Assert.True(selection.Begin(new Point(100, 20)));
        Assert.True(selection.Extend(new Point(100, 100)));
        Assert.True(selection.HasSelection);
        Assert.Equal(source, selection.SelectedText);
        selection.Clear();
        Assert.False(selection.HasSelection);
    });
}
