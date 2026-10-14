using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Threading;
using Markdig;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownTableLayoutTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData("ID", "Description", "A long description that must receive more room than the identifier column.")]
    [InlineData("编号", "说明", "内容较多的一列应当获得更多空间，短标签列保持紧凑，窄窗口中自动换行。")]
    [InlineData("Key", "URL", "https://example.test/" + "averylongunbrokentokenaverylongunbrokentokenaverylongunbrokentoken")]
    public void ContentWidthsRemainCompactAndReflowAfterResize(string heading, string description, string content) => platform.Run(() =>
    {
        var source = $"| {heading} | {description} |\n| --- | --- |\n| 1 | {content} |";
        var document = Markdig.Markdown.Parse(source, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
        var container = Assert.IsType<Border>(new TableRenderer().TryRender(document[0], source, new MarkdownViewer()));
        var grid = Assert.IsType<MarkdownTableGrid>(container.Child);
        foreach (var cell in grid.Children.OfType<Border>()) cell.Padding = new Thickness(10, 8);
        var window = new Window { Width = 640, Height = 400, Content = container };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var labelWidth = grid.ColumnDefinitions[0].ActualWidth;
            var wideHeight = grid.DesiredSize.Height;
            Assert.True(labelWidth < grid.ColumnDefinitions[1].ActualWidth / 2);
            Assert.InRange(grid.ColumnDefinitions.Sum(column => column.ActualWidth), 1, 641);

            window.Width = 240;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.InRange(grid.ColumnDefinitions.Sum(column => column.ActualWidth), 1, 241);
            Assert.True(grid.DesiredSize.Height > wideHeight, "Narrow tables wrap their text instead of clipping it.");
            Assert.True(grid.ColumnDefinitions[0].ActualWidth <= labelWidth + 1);

            window.Width = 640;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Assert.InRange(grid.ColumnDefinitions[0].ActualWidth, labelWidth - 1, labelWidth + 1);
            Assert.InRange(grid.DesiredSize.Height, wideHeight - 1, wideHeight + 1);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void SeveralShortColumnsDoNotExpandToEqualFractionsOfTheWindow() => platform.Run(() =>
    {
        var grid = new MarkdownTableGrid();
        foreach (var text in new[] { "A", "Longer heading", "B" })
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var cell = new TextBlock { Text = text };
            Grid.SetColumn(cell, grid.Children.Count);
            grid.Children.Add(cell);
        }
        grid.Measure(new Size(1000, 400));
        grid.Arrange(new Rect(0, 0, 1000, grid.DesiredSize.Height));
        Assert.True(grid.ColumnDefinitions[1].ActualWidth > grid.ColumnDefinitions[0].ActualWidth * 2);
        Assert.True(grid.ColumnDefinitions.Sum(column => column.ActualWidth) < 500);
    });
}
