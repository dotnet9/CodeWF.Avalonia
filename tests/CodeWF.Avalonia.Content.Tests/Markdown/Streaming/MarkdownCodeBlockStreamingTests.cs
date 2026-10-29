using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 流式代码块原地更新：追加仍在围栏内时复用同一外框与滚动容器，
/// 只替换代码正文，块内选区、换行与折叠状态跨快照保留；
/// 围栏闭合后新增的块接在尾块之后插入。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownCodeBlockStreamingTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void GrowingFence_KeepsFrameScrollViewerAndSelection() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer) = CreateViewer(document);
		try
		{
			document.Append("```csharp\nvar first = 1;\n");
			WaitForSource(viewer, "```csharp\nvar first = 1;\n");
			var scroll = FindCodeScroll(viewer);
			var text = FindCodeText(viewer);
			Assert.Contains("var first = 1;", text.Inlines!.Text);
			text.SelectionStart = 4;
			text.SelectionEnd = 9;

			document.Append("var second = 2;\n");
			WaitForSource(viewer, "```csharp\nvar first = 1;\nvar second = 2;\n");

			Assert.Equal(MarkdownRenderMode.Incremental, viewer.LastRenderMode);
			Assert.Same(scroll, FindCodeScroll(viewer));
			var grown = FindCodeText(viewer);
			Assert.Contains("var second = 2;", grown.Inlines!.Text);
			Assert.Equal(4, grown.SelectionStart);
			Assert.Equal(9, grown.SelectionEnd);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ClosingFence_InsertsFollowingBlocksAfterTheSameFrame() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer) = CreateViewer(document);
		try
		{
			document.Append("```csharp\nvar first = 1;\n");
			WaitForSource(viewer, "```csharp\nvar first = 1;\n");
			var scroll = FindCodeScroll(viewer);

			// 末行仍在围栏内时追加：先增长围栏，再闭合并带出后续段落。
			document.Append("var second = 2;\n```\n\nAfter paragraph.\n");
			WaitForSource(viewer, "```csharp\nvar first = 1;\nvar second = 2;\n```\n\nAfter paragraph.\n");

			Assert.Same(scroll, FindCodeScroll(viewer));
			Assert.Equal(2, viewer.CurrentModel.Blocks.Count);
			Assert.Contains("var second = 2;", FindCodeText(viewer).Inlines!.Text);
			Assert.Contains(viewer.GetVisualDescendants().OfType<SelectableTextBlock>(),
				block => (block.Inlines?.Text ?? block.Text)?.Contains("After paragraph.", StringComparison.Ordinal) == true);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void WrapAndFoldStates_SurviveInPlaceGrowth() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer) = CreateViewer(document);
		try
		{
			var builder = new System.Text.StringBuilder("```csharp\n");
			document.Append(builder.ToString());
			WaitForSource(viewer, builder.ToString());

			for (var i = 0; i < 9; i++)
			{
				builder.Append($"var value{i} = {i};\n");
				document.Append($"var value{i} = {i};\n");
				WaitForSource(viewer, builder.ToString());
			}

			Click(FindButton(viewer, "CodeWrap"), window);
			Click(FindButton(viewer, "CodeFold"), window);
			Assert.False(IsCodeContentVisible(viewer));

			builder.Append("var tail = 10;\n");
			document.Append("var tail = 10;\n");
			WaitForSource(viewer, builder.ToString());

			Assert.False(IsCodeContentVisible(viewer), "流式追加不应展开已折叠的代码块");
			Assert.Equal(TextWrapping.Wrap, FindCodeText(viewer).TextWrapping);
			Assert.Equal("▸", FindButton(viewer, "CodeFold").Content);
		}
		finally
		{
			window.Close();
		}
	});

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

	private static ScrollViewer FindCodeScroll(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<ScrollViewer>()
			.Single(scroll => scroll.Classes.Contains(MarkdownStyleKeys.CodeBlockScrollViewer));

	private static SelectableTextBlock FindCodeText(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
			.Single(text => text.Classes.Contains("MdCodeBlockPlain"));

	private static Button FindButton(MarkdownViewer viewer, string tag) =>
		viewer.GetVisualDescendants().OfType<Button>().Single(button => Equals(button.Tag, tag));

	private static bool IsCodeContentVisible(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<StackPanel>()
			.Single(panel => panel.Classes.Contains(MarkdownStyleKeys.CodeBlockContent)).IsVisible;

	private static void Click(Button button, Window window)
	{
		var center = new Rect(0, 0, Math.Max(button.Bounds.Width, 1), Math.Max(button.Bounds.Height, 1)).Center;
		if (button.TranslatePoint(center, window) is not { } windowPoint)
		{
			throw new InvalidOperationException("按钮不在窗口可视化树内");
		}

		window.MouseMove(windowPoint);
		window.MouseDown(windowPoint, MouseButton.Left);
		window.MouseUp(windowPoint, MouseButton.Left);
		Dispatcher.UIThread.RunJobs();
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
