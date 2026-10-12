using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Threading;
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
        CreateViewer(string source)
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
        var presenter = new ScrollContentPresenter { Content = panel };
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
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", panel);
                return scrollViewer;
            })
        };

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
    /// 不用 <c>ScrollViewer.Extent</c> 直接跳跃，因为虚拟化文档的总高度量测后才收敛。
    /// </summary>
    private static void ScrollToBottom(WindowedStackPanel panel, ScrollViewer scrollViewer)
    {
        for (var i = 0; i < 80; i++)
        {
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + 480);
            Dispatcher.UIThread.RunJobs();
        }

        if (panel.GetRealizedItem(panel.ItemCount - 1) is null)
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
