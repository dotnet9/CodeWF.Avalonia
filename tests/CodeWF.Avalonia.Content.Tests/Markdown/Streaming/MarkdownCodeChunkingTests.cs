using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 长代码分块（无高亮器路径）：超过阈值按 128 行切块；流式追加只重建首个变化行
/// 所在的块及其后续块，前面块的控件实例与排版结果保持不变。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownCodeChunkingTests(AvaloniaPlatformFixture platform)
{
    private const int Threshold = 200;
    private const int LinesPerChunk = 128;

    [Fact]
    public void LongCode_SplitsIntoChunks_AndKeepsEarlierChunksWhenAppending() => platform.Run(() =>
    {
        using var document = new MarkdownStreamingDocument();
        var (window, viewer) = CreateViewer(document);
        try
        {
            var builder = new System.Text.StringBuilder("```text\n");
            AppendLines(builder, 0, Threshold + 10);
            document.Append(builder.ToString());
            WaitForSource(viewer, builder.ToString());

            var chunks = FindCodeChunks(viewer);
            Assert.Equal(2, chunks.Count);
            Assert.Contains("line0", chunks[0].Inlines!.Text);
            Assert.Contains($"line{Threshold + 9}", chunks[1].Inlines!.Text);
            var first = chunks[0];

            // 尾行仍在围栏内：只增长最后一个块，前面的块保持同一控件实例。
            builder.Append($"line{Threshold + 10}\n");
            document.Append($"line{Threshold + 10}\n");
            WaitForSource(viewer, builder.ToString());

            chunks = FindCodeChunks(viewer);
            Assert.Equal(2, chunks.Count);
            Assert.Same(first, chunks[0]);
            Assert.Contains($"line{Threshold + 10}", chunks[1].Inlines!.Text);
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void CodeAtThreshold_StaysSingleBlock() => platform.Run(() =>
    {
        using var document = new MarkdownStreamingDocument();
        var (window, viewer) = CreateViewer(document);
        try
        {
            var builder = new System.Text.StringBuilder("```text\n");
            AppendLines(builder, 0, Threshold);
            document.Append(builder.ToString());
            WaitForSource(viewer, builder.ToString());

            Assert.Single(FindCodeChunks(viewer));
        }
        finally
        {
            window.Close();
        }
    });

    private static void AppendLines(System.Text.StringBuilder builder, int start, int count)
    {
        for (var line = start; line < start + count; line++)
        {
            builder.Append("line").Append(line).Append('\n');
        }
    }

    private static List<SelectableTextBlock> FindCodeChunks(MarkdownViewer viewer) =>
        viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
            .Where(text => text.Classes.Contains("MdCodeBlockPlain"))
            .ToList();

    private static (Window Window, MarkdownViewer Viewer) CreateViewer(MarkdownStreamingDocument document)
    {
        var host = new StackPanel();
        var viewer = new MarkdownViewer
        {
            StreamingSource = document,
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", host);
                return host;
            })
        };

        var window = new Window { Width = 600, Height = 500, Content = viewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, viewer);
    }

    private static void WaitForSource(MarkdownViewer viewer, string source)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (viewer.CurrentModel.Source != source)
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException(
                    $"流式渲染未到达预期源码（当前 {viewer.CurrentModel.Source.Length} 字符，期望 {source.Length}）");
            }

            Thread.Sleep(10);
        }

        Dispatcher.UIThread.RunJobs();
    }
}
