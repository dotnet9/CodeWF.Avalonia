using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.Styling;
using Markdig;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownTableToolsTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void ToolsReceiveRichCellDataBeforeLayoutAndStayInsideTheirDocumentBlock() => platform.Run(() =>
    {
        var panel = new StackPanel();
        var viewer = new MarkdownViewer
        {
            TableToolsFactory = (_, data) => new Button { Content = data.Rows[1][0] },
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", panel);
                return panel;
            })
        };
        var window = new Window { Width = 400, Height = 300, Content = viewer };
        viewer.Resources["CodeWFMarkdownTableCellBorderThickness"] = new global::Avalonia.Thickness(2);
        window.Styles.Add(new global::Avalonia.Styling.Style(selector => selector.OfType<Grid>().Class(MarkdownStyleKeys.TableBlock))
        {
            Setters = { new global::Avalonia.Styling.Setter(Control.MarginProperty, new global::Avalonia.Thickness(0, 12)) }
        });
        try
        {
            window.Show();
            for (var i = 0; i < 12; i++)
            {
                viewer.Markdown = $"| **Header** | Link |\n| --- | --- |\n| row {i} | [label](https://example.com) |\n\nAfter table.";
                WaitForSource(viewer);
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                var table = Assert.Single(panel.Children.OfType<MarkdownTableBlock>());
                Assert.Equal(new global::Avalonia.Thickness(0, 12), table.Margin);
                Assert.Equal("Header", table.Data.Rows[0][0]);
                Assert.Equal("label", table.Data.Rows[1][1]);
                Assert.True(table.Data.HasHeader);
                Assert.Equal($"row {i}", Assert.IsType<Button>(table.Children[0]).Content);
                var tableFrame = Assert.IsType<Border>(table.Children[1]);
                Assert.All(Assert.IsAssignableFrom<Grid>(tableFrame.Child).Children.OfType<Border>(),
                    cell => Assert.Equal(new global::Avalonia.Thickness(2), cell.BorderThickness));
                Assert.DoesNotContain(panel.Children, child => child is Button);
            }
            viewer.Markdown = "Only text.";
            WaitForSource(viewer);
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain(panel.Children, child => child is MarkdownTableBlock or Button);
        }
        finally { window.Close(); }
    });

    private static void WaitForSource(MarkdownViewer viewer)
    {
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (viewer.CurrentModel.Source != viewer.Markdown && DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(viewer.Markdown, viewer.CurrentModel.Source);
    }

    [Fact]
    public void HtmlTablesUseTheSameHookWithoutInventingAHeader() => platform.Run(() =>
    {
        const string source = "<table><tr><td>A</td><td><b>B</b></td></tr><tr><td>C</td><td>D</td></tr></table>";
        var viewer = new MarkdownViewer { TableToolsFactory = (_, _) => new Button() };
        var table = Assert.IsType<MarkdownTableBlock>(new HtmlTableBlockRenderer().TryRender(Markdig.Markdown.Parse(source)[0], source, viewer));
        Assert.False(table.Data.HasHeader);
        Assert.Equal("B", table.Data.Rows[0][1]);
        Assert.Equal("D", table.Data.Rows[1][1]);
    });

    [Fact]
    public void DataCopiesCallerRows()
    {
        string[][] source = [["A"]];
        var data = new MarkdownTableData(source, true);
        source[0][0] = "changed";
        Assert.Equal("A", data.Rows[0][0]);
    }
}
