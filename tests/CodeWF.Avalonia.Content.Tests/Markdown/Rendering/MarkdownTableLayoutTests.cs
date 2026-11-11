using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Themes;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using Markdig;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownTableLayoutTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void CollapsedBordersShareOneEdgeAndRestoreAfterToggle(double thickness) => platform.Run(() =>
    {
        var grid = new MarkdownTableGrid();
        grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition()); grid.RowDefinitions.Add(new RowDefinition());
        for (var row = 0; row < 2; row++)
        for (var column = 0; column < 2; column++)
        {
            var cell = new Border { BorderThickness = new Thickness(thickness), Padding = new Thickness(10), Child = new TextBlock { Text = "Cell" } };
            Grid.SetRow(cell, row); Grid.SetColumn(cell, column); grid.Children.Add(cell);
        }
        void Layout() { grid.Measure(new Size(800, 600)); grid.Arrange(new Rect(grid.DesiredSize)); }
        Layout();
        var separate = grid.DesiredSize;
        MarkdownTableLayout.SetCollapseBorders(grid, true); Layout();
        Assert.Equal(separate.Width - thickness, grid.DesiredSize.Width, 4);
        Assert.Equal(separate.Height - thickness, grid.DesiredSize.Height, 4);
        var first = grid.Children[0]; var right = grid.Children[1]; var below = grid.Children[2];
        Assert.Equal(first.Bounds.Right - thickness, right.Bounds.Left, 4);
        Assert.Equal(first.Bounds.Bottom - thickness, below.Bounds.Top, 4);
        MarkdownTableLayout.SetCollapseBorders(grid, false); Layout();
        Assert.Equal(separate, grid.DesiredSize);
        Assert.All(grid.Children, cell => Assert.Equal(default, cell.Margin));
    });

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
            Assert.InRange(container.Bounds.Width - grid.ColumnDefinitions.Sum(column => column.ActualWidth), 0, 2);

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
    public void FillAvailableWidthRetainsContentProportionsAndFollowsResize() => platform.Run(() =>
    {
        var grid = new MarkdownTableGrid();
        MarkdownTableLayout.SetFillAvailableWidth(grid, true);
        foreach (var text in new[] { "ID", "Longer heading" })
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var cell = new TextBlock { Text = text };
            Grid.SetColumn(cell, grid.Children.Count); grid.Children.Add(cell);
        }
        foreach (var width in new[] { 1000d, 180d, 640d })
        {
            grid.Measure(new Size(width, 400)); grid.Arrange(new Rect(0, 0, width, grid.DesiredSize.Height));
            Assert.InRange(grid.ColumnDefinitions.Sum(column => column.ActualWidth), width - 1, width + 1);
            Assert.True(grid.ColumnDefinitions[1].ActualWidth > grid.ColumnDefinitions[0].ActualWidth * 2);
        }
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

    [Fact]
    public void CellTextMatchesBodyTypographyAndFollowsViewerChanges() => platform.Run(() =>
    {
        var viewer = new MarkdownViewer { ParagraphFontSize = 17, ParagraphLineHeight = 30 };
        const string source = "| Head |\n| --- |\n| Cell |";
        var document = Markdig.Markdown.Parse(source, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
        var container = Assert.IsType<Border>(new TableRenderer().TryRender(document[0], source, viewer));
        var grid = Assert.IsType<MarkdownTableGrid>(container.Child);
        var textBlocks = grid.GetVisualDescendants().OfType<SelectableTextBlock>().ToArray();
        Assert.Equal(2, textBlocks.Length);
        Assert.All(textBlocks, block =>
        {
            Assert.Equal(17, block.FontSize);
            Assert.Equal(30, block.LineHeight);
        });
        viewer.ParagraphFontSize = 15;
        Assert.All(textBlocks, block => Assert.Equal(15, block.FontSize));
        // 表头只保留加粗差异，字号与正文同源。
        Assert.Equal(global::Avalonia.Media.FontWeight.SemiBold, textBlocks[0].FontWeight);
        Assert.Equal(global::Avalonia.Media.FontWeight.Normal, textBlocks[1].FontWeight);
    });

    [Theory]
    [InlineData(MarkdownTypographySizes.Normal)]
    [InlineData(MarkdownTypographySizes.Compact)]
    public void CellTypographyScalesWithBodyTier(string size) => platform.Run(() =>
    {
        // 档位值取排版资源计算结果（文章主题按档位缩放正文），与宿主（如 Vex）把档位值
        // 绑到查看器 ParagraphFontSize 的方式同源；断言单元格与正文完全一致。
        var sized = MarkdownThemes.CreateTypographyResources("Simple", size);
        var bodySize = Assert.IsType<double>(sized[MarkdownStyleKeys.ParagraphFontSizeResource]);
        var viewer = new MarkdownViewer { Markdown = "| 头 |\n| --- |\n| 格 |", ParagraphFontSize = bodySize };
        viewer.Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<MarkdownViewer>((_, scope) =>
        {
            var panel = new StackPanel();
            scope.Register("PART_DocumentHost", panel);
            return panel;
        });
        var window = new Window { Content = viewer, Width = 300, Height = 200 };
        window.Styles.Add(new MarkdownThemes());
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var textBlocks = viewer.GetVisualDescendants()
                .OfType<StackPanel>()
                .Where(panel => panel.Classes.Contains(MarkdownStyleKeys.TableCellContent))
                .SelectMany(panel => panel.Children.OfType<SelectableTextBlock>())
                .ToArray();
            Assert.Equal(2, textBlocks.Length);
            Assert.All(textBlocks, block =>
            {
                Assert.Equal(viewer.ParagraphFontSize, block.FontSize, 4);
                Assert.Equal(viewer.ParagraphLineHeight, block.LineHeight, 4);
            });
        }
        finally { window.Close(); }
    });
}
