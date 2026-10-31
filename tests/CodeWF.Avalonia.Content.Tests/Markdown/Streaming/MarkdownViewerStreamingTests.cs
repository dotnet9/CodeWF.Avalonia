using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// Viewer 流式入口（StreamingSource）：快照驱动渲染、冻结前缀增量复用、
/// 与 Markdown 整串赋值的互斥关系。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerStreamingTests(AvaloniaPlatformFixture platform)
{
	private const string Sample =
		"# 流式标题\n\n第一段内容。\n\n- 列表项甲\n- 列表项乙\n\n```csharp\nvar value = 42;\n```\n\n结尾段落。\n";

	[Fact]
	public void StreamingSource_RendersGrowingSnapshots() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer, _, _) = CreateViewer(document);
		try
		{
			var appended = 0;
			for (var step = 0; step < Sample.Length; step += 6)
			{
				var end = Math.Min(step + 6, Sample.Length);
				document.Append(Sample[appended..end]);
				appended = end;
				WaitForSource(viewer, Sample[..end]);
			}

			Assert.Equal(Sample, viewer.CurrentModel.Source);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void StreamingSource_AfterFirstFullRender_AppliesIncrementally() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer, _, _) = CreateViewer(document);
		try
		{
			document.Append("# 标题\n\n第一段。\n\n");
			WaitForSource(viewer, "# 标题\n\n第一段。\n\n");
			Assert.Equal(MarkdownRenderMode.Full, viewer.LastRenderMode);

			// 追加内容后冻结前缀哈希不变，应走增量替换而非整篇重建。
			document.Append("第二段。\n");
			WaitForSource(viewer, "# 标题\n\n第一段。\n\n第二段。\n");
			Assert.Equal(MarkdownRenderMode.Incremental, viewer.LastRenderMode);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void MarkdownAssignment_DetachesStreamingSource() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer, _, _) = CreateViewer(document);
		try
		{
			document.Append("流式内容\n");
			WaitForSource(viewer, "流式内容\n");
			Assert.NotNull(viewer.StreamingSource);

			viewer.Markdown = "# 整串内容\n";
			WaitForSource(viewer, "# 整串内容\n");

			Assert.Null(viewer.StreamingSource);
		}
		finally
		{
			window.Close();
		}
	});

	[Theory]
	[InlineData(16)]
	[InlineData(80)]
	public void StreamingReveal_FadesInOnlyNewBlocks(int durationMilliseconds) => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer, panel, _) = CreateViewer(document);
		try
		{
			viewer.StreamingReveal = true;
			viewer.StreamingRevealDuration = TimeSpan.FromMilliseconds(durationMilliseconds);
			document.Append("# 标题\n\n第一段。\n\n");
			WaitForSource(viewer, "# 标题\n\n第一段。\n\n");
			Assert.Equal(0, viewer.RevealedBlockCount); // 首屏整篇渲染不做渐显
			var original = panel.Children.OfType<Control>().ToArray();

			document.Append("第二段。\n\n");
			WaitForSource(viewer, "# 标题\n\n第一段。\n\n第二段。\n\n");
			Assert.True(viewer.RevealedBlockCount >= 1, "追加的新块应触发渐显");
			Assert.All(original, control => Assert.Equal(1, control.Opacity));

			// 渐显结束（含掉帧）后所有块都应回到完全不透明。
			var deadline = DateTime.UtcNow.AddSeconds(5);
			while (DateTime.UtcNow < deadline
				&& panel.Children.OfType<Control>().Any(control => control.Opacity != 1))
			{
				global::Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
				Dispatcher.UIThread.RunJobs();
				Thread.Sleep(16);
			}

			Assert.All(panel.Children.OfType<Control>(), control => Assert.Equal(1, control.Opacity));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Complete_PublishesFinalBlocks() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer, _, _) = CreateViewer(document);
		try
		{
			document.Append(Sample);
			WaitForSource(viewer, Sample);
			document.Complete();

			var deadline = DateTime.UtcNow.AddSeconds(5);
			while (!document.Latest.IsComplete)
			{
				Dispatcher.UIThread.RunJobs();
				if (DateTime.UtcNow > deadline)
				{
					throw new TimeoutException("流未完成");
				}

				Thread.Sleep(10);
			}

			Dispatcher.UIThread.RunJobs();
			Assert.Equal(document.Latest.Model.Blocks.Count, viewer.CurrentModel.Blocks.Count);
		}
		finally
		{
			window.Close();
		}
	});

	private static (Window Window, MarkdownViewer Viewer, WindowedStackPanel Panel, ScrollViewer ScrollViewer)
		CreateViewer(MarkdownStreamingDocument document)
	{
		var panel = new WindowedStackPanel
		{
			EstimatedItemHeight = 42,
			KeepAliveBudget = 3000,
			KeepAliveMinItemHeight = 72,
			KeepAliveUnknownCount = 6,
			Spacing = 8
		};

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
			StreamingSource = document,
			ScrollHost = scrollViewer,
			Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
			{
				scope.Register("PART_DocumentHost", panel);
				return scrollViewer;
			})
		};

		var window = new Window { Width = 600, Height = 500, Content = viewer };
		window.Show();
		Dispatcher.UIThread.RunJobs();
		return (window, viewer, panel, scrollViewer);
	}

	private static void WaitForSource(MarkdownViewer viewer, string source)
	{
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (viewer.CurrentModel.Source != source)
		{
			Dispatcher.UIThread.RunJobs();
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException($"流式渲染未到达预期源码（当前 {viewer.CurrentModel.Source.Length} 字符，期望 {source.Length}）");
			}

			Thread.Sleep(10);
		}

		Dispatcher.UIThread.RunJobs();
	}
}
