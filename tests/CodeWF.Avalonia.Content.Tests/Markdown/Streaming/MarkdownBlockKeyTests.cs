using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 块键坐标系统一（规格：块键 = 原始源码起始偏移）：流式尾块、冻结段、
/// 终局整篇解析三种坐标下，折叠状态与高亮会话的键必须一致。
/// 回归背景：渲染器此前用解析输入内的相对 Span.Start 做键，第二个及之后
/// 冻结段 / 尾块与绝对偏移错位，折叠状态跨迁移丢失。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownBlockKeyTests(AvaloniaPlatformFixture platform)
{
	private static readonly string CodeBody =
		string.Join("\n", Enumerable.Range(1, 10).Select(i => $"var value{i} = {i};")) + "\n";

	private static readonly string Document = "前言内容。\n\n```csharp\n" + CodeBody + "```\n\n后记内容。\n";

	[Fact]
	public void FoldState_SurvivesTailToFrozenTransition() => platform.Run(() =>
	{
		var fenceStart = Document.IndexOf("```csharp", StringComparison.Ordinal);
		var midFence = fenceStart + 20;

		using var streaming = new MarkdownStreamingDocument();
		var (window, viewer, _) = CreateViewer(streaming);
		try
		{
			// 流到代码块中部（尾块形态），以绝对偏移折叠。
			streaming.Append(Document[..midFence]);
			WaitForSource(viewer, Document[..midFence]);

			var streamingStart = LocateCodeBlockStart(viewer);
			Assert.Equal(fenceStart, streamingStart);
			viewer.SetCodeBlockFolded(fenceStart, true);
			Assert.True(viewer.IsCodeBlockFolded(fenceStart));

			// 流完（围栏闭合 → 块冻结 → 键坐标系从尾部迁到冻结段 → 终局整篇解析）。
			streaming.Append(Document[midFence..]);
			WaitForComplete(viewer, Document, streaming);

			Assert.True(viewer.IsCodeBlockFolded(fenceStart), "折叠状态应跨尾→冻结迁移保留");

			// 触发整篇重渲染：渲染器按绝对块键读取折叠状态，内容面板应保持折叠。
			viewer.RefreshRemoteImages();
			Dispatcher.UIThread.RunJobs();

			var contentPanel = viewer.GetVisualDescendants()
				.OfType<StackPanel>()
				.FirstOrDefault(panel => panel.Classes.Contains("MdCodeBlockContent"));
			Assert.NotNull(contentPanel);
			Assert.False(contentPanel.IsVisible, "重渲染后代码块内容应保持折叠");
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void FoldKey_IsStable_AcrossSecondFrozenSegment() => platform.Run(() =>
	{
		// 代码块位于第二个冻结段（前面已有内容冻结，解析输入不从 0 开始）：
		// 整篇渲染与流式渲染的折叠键都必须是同一绝对偏移。
		using var streaming = new MarkdownStreamingDocument();
		var (window, viewer, _) = CreateViewer(streaming);
		try
		{
			var fenceStart = Document.IndexOf("```csharp", StringComparison.Ordinal);
			streaming.Append(Document);
			WaitForComplete(viewer, Document, streaming);

			viewer.SetCodeBlockFolded(fenceStart, true);
			Assert.True(viewer.IsCodeBlockFolded(fenceStart));

			// 同文档重渲染（内容面板重建）后键仍命中。
			viewer.RefreshRemoteImages();
			Dispatcher.UIThread.RunJobs();
			Assert.True(viewer.IsCodeBlockFolded(fenceStart));
		}
		finally
		{
			window.Close();
		}
	});

	private static int LocateCodeBlockStart(MarkdownViewer viewer)
	{
		foreach (var block in viewer.CurrentModel.Blocks)
		{
			if (block.Kind == MarkdownBlockKind.Code)
			{
				return block.SourceSpan.Start;
			}
		}

		throw new InvalidOperationException("流式快照中未找到代码块");
	}

	private static void WaitForSource(MarkdownViewer viewer, string source)
	{
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (viewer.CurrentModel.Source != source)
		{
			Dispatcher.UIThread.RunJobs();
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException($"流式渲染未到达预期源码（当前 {viewer.CurrentModel.Source.Length} 字符）");
			}

			Thread.Sleep(10);
		}

		Dispatcher.UIThread.RunJobs();
	}

	private static void WaitForComplete(MarkdownViewer viewer, string source, MarkdownStreamingDocument streaming)
	{
		streaming.Complete();
		WaitForSource(viewer, source);
		// 终局整篇解析快照与前一快照同源码，再泵几轮让 Complete 快照落地。
		for (var i = 0; i < 5; i++)
		{
			Thread.Sleep(20);
			Dispatcher.UIThread.RunJobs();
		}
	}

	private static (Window Window, MarkdownViewer Viewer, WindowedStackPanel Panel) CreateViewer(
		MarkdownStreamingDocument document)
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
		return (window, viewer, panel);
	}
}
