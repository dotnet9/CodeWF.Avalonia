using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using System.Text;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 文档块虚拟化的「离屏保留」策略：重块（代码块、图片、表格、图表）彻底滚出视口后按高度预算保留控件，
/// 轻量段落照常解体，避免滚动时反复重建（重新高亮、重新解码、重新光栅化）。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerVirtualizationTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void ReturningToPreviousTextAfterFullRenderStillRefreshesTheDocument() => platform.Run(() =>
    {
        const string first = "# First\n\nOpening paragraph.";
        const string second = "# Second\n\n```cs\nvar n = 42;\n```";
        var (window, viewer, _, _) = CreateViewer(first);
        try
        {
            viewer.Markdown = second;
            WaitForRender(viewer, second);
            viewer.Markdown = first;
            viewer.CodeBlockWrap = !viewer.CodeBlockWrap;
            Dispatcher.UIThread.RunJobs();
            WaitForRender(viewer, first);
            viewer.Markdown = second;
            WaitForRender(viewer, second);
            Assert.Equal(second, viewer.CurrentModel.Source);
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void LargeDocumentsCreateTextControlsOnlyInsideRealizedBlocks() => platform.Run(() =>
    {
        var created = 0;
        var source = string.Join("\n\n", Enumerable.Range(0, 600).Select(i => $"```text\ncode {i}\n```"));
        var (window, viewer, panel, scroll) = CreateViewer(source, viewer => viewer.CodeBlockToolRender += (_, _) => created++);
        try
        {
            Assert.InRange(created, 1, 99);
            Assert.True(viewer.GetVisualDescendants().OfType<SelectableTextBlock>().Count() < 100);
            ScrollToBottom(panel, scroll);
            Assert.True(viewer.GetVisualDescendants().OfType<SelectableTextBlock>().Count() < 100);
            scroll.Offset = default; Dispatcher.UIThread.RunJobs();
            Assert.NotNull(panel.GetRealizedItem(0));
            Assert.True(viewer.GetVisualDescendants().OfType<SelectableTextBlock>().Count() < 100);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void SwitchingFromShortSectionToLargeDocumentDefersOffscreenContent() => platform.Run(() =>
    {
        var created = 0;
        var shortSection = "# Section\n\nOpening paragraph.";
        var largeDocument = shortSection + "\n\n" + string.Join("\n\n", Enumerable.Range(0, 600).Select(i => $"```text\ncode {i}\n```"));
        var (window, viewer, panel, _) = CreateViewer(shortSection, viewer => viewer.CodeBlockToolRender += (_, _) => created++);
        try
        {
            for (var pass = 0; pass < 3; pass++)
            {
                var before = created;
                viewer.Markdown = largeDocument; WaitForRender(viewer, largeDocument);
                Assert.InRange(created - before, 1, 99);
                Assert.True(panel.IsVirtualizing);
                Assert.True(panel.RealizedItemCount < 100);
                viewer.Markdown = shortSection; WaitForRender(viewer, shortSection);
            }
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData(1)]
    [InlineData(1.5)]
    public void ReadingPositionRoundTripsAfterScrolling(double scale) => platform.Run(() =>
    {
        var source = BuildDocument(codeBlockFirst: false, blocks: 100);
        var (window, viewer, _, scroll) = CreateViewer(source, scale: scale);
        try
        {
            var sampledGap = false;
            for (var offset = 1200; offset < 1300; offset += 2)
            {
                scroll.Offset = new Vector(0, offset); Dispatcher.UIThread.RunJobs();
                var before = scroll.Offset.Y;
                var position = viewer.SaveReadingPosition();
                Assert.NotNull(position);
                sampledGap |= position.Value.BlockRatio < 0;
                scroll.Offset = default; Dispatcher.UIThread.RunJobs();
                Assert.True(viewer.RestoreReadingPosition(position)); Dispatcher.UIThread.RunJobs();
                Assert.InRange(Math.Abs(scroll.Offset.Y - before), 0, .75);
            }
            Assert.True(sampledGap, "Round trips must include a viewport center in inter-block whitespace.");
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData(1)]
    [InlineData(1.5)]
    public void ReadingPositionSurvivesASectionPresentationInALargeMixedDocument(double scale) => platform.Run(() =>
    {
        var source = string.Join("\n\n", Enumerable.Range(0, 500).Select(i =>
            $"## Chapter {i}\n\nParagraph {i}. " + new string('x', i % 7 * 80) +
            $"\n\n```text\n{string.Join('\n', Enumerable.Range(0, i % 12 + 1).Select(n => $"code {i} line {n}"))}\n```"));
        var (window, viewer, panel, scroll) = CreateViewer(source, scale: scale);
        try
        {
            for (var i = 0; i < 45; i++) { scroll.Offset = new Vector(0, scroll.Offset.Y + 480); Dispatcher.UIThread.RunJobs(); }
            var position = viewer.SaveReadingPosition();
            Assert.NotNull(position);
            Assert.InRange(position.Value.BlockIndex, 30, 1400);
            viewer.Markdown = "# Presentation\n\nA short section."; WaitForRender(viewer, viewer.Markdown);
            viewer.Markdown = source; WaitForRender(viewer, source);
            Assert.True(viewer.RestoreReadingPosition(position)); Dispatcher.UIThread.RunJobs();
            var restored = viewer.SaveReadingPosition();
            Assert.NotNull(restored);
            Assert.Equal(position.Value.BlockIndex, restored.Value.BlockIndex);
            Assert.InRange(Math.Abs(position.Value.BlockRatio - restored.Value.BlockRatio), 0, .05);
            Assert.True(panel.RealizedItemCount < 100);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void SourceNavigationCanLocateAnOffscreenBlockBeforeScrolling() => platform.Run(() =>
    {
        var source = string.Join("\n\n", Enumerable.Range(0, 600).Select(i => $"Paragraph {i}."));
        var (window, viewer, panel, scroll) = CreateViewer(source);
        try
        {
            Assert.False(viewer.IsBlockRealized(450));
            Assert.True(viewer.TryGetSourceLineBounds(901, out var bounds));
            Assert.True(bounds.Y > scroll.Viewport.Height);
            Assert.True(viewer.IsBlockRealized(450));
            scroll.Offset = new Vector(0, bounds.Y); Dispatcher.UIThread.RunJobs();
            Assert.Contains(viewer.GetVisualDescendants().OfType<SelectableTextBlock>(), text => (text.Text ?? text.Inlines?.Text) == "Paragraph 450.");
            Assert.True(panel.RealizedItemCount < 100);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void RecycledBlocksKeepMeasuredHeightIncludingMargins() => platform.Run(() =>
    {
        var panel = new WindowedStackPanel { EstimatedItemHeight = 56, KeepAliveBudget = 0, KeepAliveUnknownCount = 0 };
        for (var i = 0; i < 100; i++) panel.AddItem(new Border { Height = 40, Margin = new Thickness(0, 8) });
        var presenter = new ScrollContentPresenter { Content = panel };
        var scroll = new ScrollViewer { Template = new FuncControlTemplate<ScrollViewer>((_, scope) =>
        {
            scope.Register("PART_ScrollContentPresenter", presenter); return presenter;
        }) };
        panel.ScrollHost = scroll;
        var window = new Window { Width = 600, Height = 400, Content = scroll };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(56, panel.GetItemHeight(0));
            for (var pass = 0; pass < 4; pass++)
            {
                for (var offset = 0; offset <= 5600; offset += 200) { scroll.Offset = new Vector(0, offset); Dispatcher.UIThread.RunJobs(); }
                Assert.Null(panel.GetRealizedItem(0));
                Assert.Equal(56, panel.GetItemHeight(0));
                for (var offset = 5600; offset >= 0; offset -= 200) { scroll.Offset = new Vector(0, offset); Dispatcher.UIThread.RunJobs(); }
                Assert.Equal(5600, scroll.Extent.Height);
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void HeavyBlockScrolledOutOfView_StaysRealized() => platform.Run(() =>
    {
        var source = BuildDocument(codeBlockFirst: true, blocks: 60);
        var (window, viewer, panel, scrollViewer) = CreateViewer(source);
        try
        {
            ScrollToBottom(panel, scrollViewer);

            Assert.True(panel.ItemCount >= 60);
            Assert.True(panel.IsItemMeasured(0));
            Assert.NotNull(panel.GetRealizedItem(0));
        }
        finally
        {
            window.Close();
        }
    });

    [Fact]
    public void LightBlockScrolledOutOfView_IsReleased() => platform.Run(() =>
    {
        var source = BuildDocument(codeBlockFirst: false, blocks: 60);
        var (window, viewer, panel, scrollViewer) = CreateViewer(source);
        try
        {
            ScrollToBottom(panel, scrollViewer);

            Assert.True(panel.ItemCount >= 60);
            Assert.Null(panel.GetRealizedItem(0));
        }
        finally
        {
            window.Close();
        }
    });

    /// <summary>目标块之外的可见块全部释放，验证窗口裁剪确实按视口收紧（而不是整篇物化）。</summary>
    [Fact]
    public void VirtualizingDocument_RealizesOnlyViewportBlocks() => platform.Run(() =>
    {
        var source = BuildDocument(codeBlockFirst: true, blocks: 60);
        var (window, viewer, panel, _) = CreateViewer(source);
        try
        {
            Assert.True(panel.IsVirtualizing);
            Assert.True(panel.RealizedItemCount < panel.ItemCount / 2);
        }
        finally
        {
            window.Close();
        }
    });

    private static (Window Window, MarkdownViewer Viewer, WindowedStackPanel Panel, ScrollViewer ScrollViewer)
        CreateViewer(string source, Action<MarkdownViewer>? configure = null, double scale = 1)
    {
        var panel = new WindowedStackPanel
        {
            EstimatedItemHeight = 42,
            KeepAliveBudget = 3000,
            KeepAliveMinItemHeight = 72,
            KeepAliveUnknownCount = 6,
            Spacing = 8
        };

        // 测试宿主不带控件主题，这里直接给出 ScrollViewer 的最小模板（内容呈现器），
        // 让其正常计算 Extent/Viewport 并支持 Offset 滚动。
        var presenter = new ScrollContentPresenter { Content = scale == 1 ? panel : new LayoutTransformControl { Child = panel, LayoutTransform = new global::Avalonia.Media.ScaleTransform(scale, scale), Margin = new Thickness(20, 40) } };
        var scrollViewer = new ScrollViewer
        {
            Template = new FuncControlTemplate<ScrollViewer>((_, scope) =>
            {
                scope.Register("PART_ScrollContentPresenter", presenter);
                return presenter;
            })
        };
        panel.ScrollHost = scrollViewer;

        var viewer = new MarkdownViewer
        {
            Markdown = source,
            ScrollHost = scrollViewer,
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", panel);
                return scrollViewer;
            })
        };

        configure?.Invoke(viewer);
        var window = new Window { Width = 600, Height = 500, Content = viewer };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.CaptureRenderedFrame()?.Dispose();
        Dispatcher.UIThread.RunJobs();
        WaitForRender(viewer, source);
        window.CaptureRenderedFrame()?.Dispose();
        Dispatcher.UIThread.RunJobs();
        return (window, viewer, panel, scrollViewer);
    }

    /// <summary>
    /// 像用户那样逐步向下滚动到文档底部：视口内只剩末尾块，最前面的块彻底离开窗口。
    /// 不用 <c>ScrollViewer.Extent</c> 直接跳跃，因为虚拟化文档的总高度量测后才收敛；步进按视口高留 20px 交叠，
    /// 让掠过边界的块也被实现。终止条件是「末尾块出现了」而不是「滚了 N 步」：
    /// 曾写死 80 步 = 38400px，版式行高或块数一涨就滚不到底，报成查看器滚不动，实为测试自己截断了行程。
    /// </summary>
    private static void ScrollToBottom(WindowedStackPanel panel, ScrollViewer scrollViewer)
    {
        var last = panel.ItemCount - 1;
        for (var i = 0; i < 400 && panel.GetRealizedItem(last) is null; i++)
        {
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + Math.Max(1d, scrollViewer.Viewport.Height - 20));
            Dispatcher.UIThread.RunJobs();
        }

        if (panel.GetRealizedItem(last) is null)
        {
            throw new InvalidOperationException(
                $"Scroll did not reach the document end: offset={scrollViewer.Offset.Y}, extent={scrollViewer.Extent.Height}, viewport={scrollViewer.Viewport.Height}");
        }
    }

    private static string BuildDocument(bool codeBlockFirst, int blocks)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < blocks; i++)
        {
            if (codeBlockFirst && i == 0)
            {
                builder.AppendLine("```csharp");
                for (var line = 0; line < 10; line++)
                {
                    builder.AppendLine($"var value{line} = {line};");
                }

                builder.AppendLine("```");
                builder.AppendLine();
                continue;
            }

            builder.AppendLine($"Paragraph {i} with enough text to occupy one line.");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static void WaitForRender(MarkdownViewer viewer, string source)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (viewer.CurrentModel.Source != source)
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("Document rendering did not finish");
            }

            Thread.Sleep(10);
        }

        Dispatcher.UIThread.RunJobs();
    }
}
