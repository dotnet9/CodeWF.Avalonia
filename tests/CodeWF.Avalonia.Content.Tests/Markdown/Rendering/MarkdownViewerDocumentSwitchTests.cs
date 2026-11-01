using CodeWF.Avalonia.Markdown;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using CodeWF.Avalonia.Markdown.Controls;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerDocumentSwitchTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void SwitchFromCustomContainerToImages_PreservesRenderedBlockIndices() => platform.Run(() =>
    {
        var (window, viewer, host) = CreateViewer("# Initial\n\nInitial paragraph");
        try
        {
            var container = "# HTML sample\n\n::: warning\nUnsupported container content\n:::\n\nFollowing paragraph";
            var images = "# Images and links\n\n![Missing image](not-found.png)\n\n[Link](https://example.com)";
            foreach (var source in new[] { container, images, container, images })
            {
                SwitchDocument(viewer, source);
                Assert.Equal(viewer.CurrentModel.Blocks.Count, host.Children.Count);
                Assert.Equal(source, viewer.CurrentModel.Source);
                if (source == container)
                {
                    Assert.Equal("::: warning\nUnsupported container content\n:::",
                        Assert.IsAssignableFrom<SelectableTextBlock>(host.Children[1]).Text);
                }
            }
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void EditBeforeCustomContainer_ReusesFollowingControlsWithCorrectSourceBounds() => platform.Run(() =>
    {
        var (window, viewer, host) = CreateViewer("# Initial\n\nInitial paragraph");
        try
        {
            var source = "# Title\n\n::: warning\nContainer text\n:::\n\nFollowing paragraph";
            SwitchDocument(viewer, source);
            // 首次后台解析建立快照；之后应按模型索引增量复用未变化的后缀。
            var container = host.Children[1];
            var following = host.Children[2];
            var changed = source.Replace("# Title", "# A longer title");
            SwitchDocument(viewer, changed);

            Assert.Equal(MarkdownRenderMode.Incremental, viewer.LastRenderMode);
            Assert.Equal(3, host.Children.Count);
            Assert.Same(container, host.Children[1]);
            Assert.Same(following, host.Children[2]);
            Assert.True(viewer.TryGetSourceOffsetBounds(changed.IndexOf("Following paragraph", StringComparison.Ordinal), out var bounds));
            Assert.True(bounds.Height > 0);
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void BeforeHeadingContextUpdatesOnReusedBlocksAndFullRerenders() => platform.Run(() =>
    {
        const string source = "> Quote\n\n## Heading\n\nFollowing paragraph";
        var (window, viewer, host) = CreateViewer(source);
        try
        {
            SwitchDocument(viewer, source);
            var quote = host.Children[0];
            Assert.Contains("MdBeforeHeading", quote.Classes);
            foreach (var prefix in new[] { "#####", "####", "######", "#" })
            {
                var changed = source.Replace("## Heading", prefix + " Heading");
                SwitchDocument(viewer, changed);
                Assert.Equal(MarkdownRenderMode.Incremental, viewer.LastRenderMode);
                Assert.Same(quote, host.Children[0]);
                Assert.Equal(prefix.Length <= 4, quote.Classes.Contains("MdBeforeHeading"));
            }
            viewer.CodeBlockWrap = !viewer.CodeBlockWrap;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("MdBeforeHeading", host.Children[0].Classes);
            SwitchDocument(viewer, "> Quote\n\nOrdinary paragraph");
            Assert.DoesNotContain("MdBeforeHeading", host.Children[0].Classes);
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void AfterQuoteContextUpdatesWhenThePreviousBlockChanges(int level) => platform.Run(() =>
    {
        var source = $"> Quote\n\n{new string('#', level)} Heading\n\nFollowing paragraph";
        var (window, viewer, host) = CreateViewer(source);
        try
        {
            SwitchDocument(viewer, source);
            var heading = host.Children[1];
            Assert.Contains("MdAfterQuote", heading.Classes);
            foreach (var preceding in new[] { "Paragraph", "> Quote", "Paragraph" })
            {
                SwitchDocument(viewer, source.Replace("> Quote", preceding));
                Assert.Equal(MarkdownRenderMode.Incremental, viewer.LastRenderMode);
                Assert.Same(heading, host.Children[1]);
                Assert.Equal(preceding.StartsWith('>'), heading.Classes.Contains("MdAfterQuote"));
            }
            viewer.CodeBlockWrap = !viewer.CodeBlockWrap;
            Dispatcher.UIThread.RunJobs();
            Assert.DoesNotContain("MdAfterQuote", host.Children[1].Classes);
        }
        finally { window.Close(); }
    });

    private static (Window Window, MarkdownViewer Viewer, StackPanel Host) CreateViewer(string source)
    {
        var host = new StackPanel();
        var viewer = new MarkdownViewer
        {
            Markdown = source,
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", host);
                return host;
            })
        };
        var window = new Window { Width = 600, Height = 500, Content = viewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        SwitchDocument(viewer, source + "\n\nPrime background rendering");
        return (window, viewer, host);
    }

    private static void SwitchDocument(MarkdownViewer viewer, string source)
    {
        viewer.Markdown = source;
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (viewer.CurrentModel.Source != source)
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow > deadline) throw new TimeoutException("Document rendering did not finish");
            Thread.Sleep(10);
        }
        Dispatcher.UIThread.RunJobs();
    }
}
