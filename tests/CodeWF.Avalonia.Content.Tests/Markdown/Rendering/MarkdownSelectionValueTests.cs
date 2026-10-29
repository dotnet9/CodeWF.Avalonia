using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 选区只存「块键 + 纯文本偏移」：控件被替换、块列表重建、同文档整篇重绘之后，
/// 选区与选中文本都按键恢复；只有被选中的块本身消失才落空。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownSelectionValueTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void CrossBlockSelection_SurvivesIncrementalRebuild() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer) = CreateViewer(document);
		try
		{
			document.Append("First paragraph.\n\nSecond paragraph.\n");
			WaitForSource(viewer, "First paragraph.\n\nSecond paragraph.\n");

			DragBetweenParagraphs(window, viewer, 0, 1);
			var selected = viewer.SelectedText;
			Assert.True(viewer.HasSelection);
			Assert.Contains("First paragraph.", selected);
			Assert.Contains("Second paragraph.", selected);

			// 追加新块会重建块列表与选中控件的模型，选区坐标不变。
			document.Append("\nThird paragraph.\n");
			WaitForSource(viewer, "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.\n");

			Assert.True(viewer.HasSelection);
			Assert.Equal(selected, viewer.SelectedText);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Selection_SurvivesSameDocumentFullRerender() => platform.Run(() =>
	{
		const string source = "Alpha paragraph.\n\nBeta paragraph.\n";
		var (window, viewer) = CreateStaticViewer(source);
		try
		{
			WaitForSource(viewer, source);

			var oldControls = FindParagraphs(viewer);
			DragBetweenParagraphs(window, viewer, 0, 1);
			var selected = viewer.SelectedText;
			Assert.True(viewer.HasSelection);

			// 同文档重绘（排版切换走整篇重建）：控件实例全换，选区按块键恢复。
			viewer.TypographySize = CodeWF.Avalonia.Markdown.Themes.MarkdownTypographySizes.Compact;
			WaitForRebuild(viewer, oldControls);

			Assert.True(viewer.HasSelection);
			Assert.Equal(selected, viewer.SelectedText);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Selection_ClearsWhenSelectedBlocksDisappear() => platform.Run(() =>
	{
		using var document = new MarkdownStreamingDocument();
		var (window, viewer) = CreateViewer(document);
		try
		{
			document.Append("Alpha paragraph.\n\nBeta paragraph.\n");
			WaitForSource(viewer, "Alpha paragraph.\n\nBeta paragraph.\n");

			DragBetweenParagraphs(window, viewer, 0, 1);
			Assert.True(viewer.HasSelection);

			document.Reset("Completely different text.\n");
			WaitForSource(viewer, "Completely different text.\n");

			Assert.False(viewer.HasSelection);
			Assert.Equal(string.Empty, viewer.SelectedText);
		}
		finally
		{
			window.Close();
		}
	});

	private static SelectableTextBlock[] FindParagraphs(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
			.Where(text => text.Classes.Contains(MarkdownStyleKeys.Paragraph))
			.ToArray();

	/// <summary>
	/// 无头环境下命中测试不会把指针事件路由进查看器子树，这里直接把按下/移动/抬起
	/// 还原成路由事件发到段落上，走的是与真实输入完全相同的隧道处理器。
	/// </summary>
	private static void DragBetweenParagraphs(Window window, MarkdownViewer viewer, int fromIndex, int toIndex)
	{
		window.UpdateLayout();
		Dispatcher.UIThread.RunJobs();
		var paragraphs = FindParagraphs(viewer);
		var source = paragraphs[fromIndex];
		var from = source.TranslatePoint(new Point(4, 6), window)!.Value;
		var to = paragraphs[toIndex].TranslatePoint(new Point(4, 6), window)!.Value;

		var pointer = new TestPointer();
		var pressed = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
		var moved = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other);
		var released = new PointerPointProperties(default, PointerUpdateKind.LeftButtonReleased);

		source.RaiseEvent(new PointerPressedEventArgs(
			source, pointer, window, from, 1, pressed, KeyModifiers.None, 1));
		for (var step = 1; step <= 12; step++)
		{
			var point = from + (to - from) * (step / 12d);
			source.RaiseEvent(new PointerEventArgs(
				InputElement.PointerMovedEvent, source, pointer, window, point, (ulong)(step + 1), moved, KeyModifiers.None));
			Dispatcher.UIThread.RunJobs();
		}

		source.RaiseEvent(new PointerReleasedEventArgs(
			source, pointer, window, to, 20, released, KeyModifiers.None, MouseButton.Left));
		Dispatcher.UIThread.RunJobs();
	}

	// 无头环境没有真实鼠标设备；Pointer 的公开构造函数足以承载路由事件。
	private sealed class TestPointer() : Pointer(1, PointerType.Mouse, true);

	private static (Window Window, MarkdownViewer Viewer) CreateViewer(MarkdownStreamingDocument document) =>
		CreateViewer(viewer => viewer.StreamingSource = document);

	private static (Window Window, MarkdownViewer Viewer) CreateStaticViewer(string source) =>
		CreateViewer(viewer => viewer.Markdown = source);

	private static (Window Window, MarkdownViewer Viewer) CreateViewer(Action<MarkdownViewer> configure)
	{
		var host = new WindowedStackPanel { EstimatedItemHeight = 42, Spacing = 8 };
		var presenter = new ScrollContentPresenter { Content = host };
		var scrollViewer = new ScrollViewer
		{
			Template = new FuncControlTemplate<ScrollViewer>((_, scope) =>
			{
				scope.Register("PART_ScrollContentPresenter", presenter);
				return presenter;
			})
		};
		host.ScrollHost = scrollViewer;

		var viewer = new MarkdownViewer
		{
			ScrollHost = scrollViewer,
			Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
			{
				scope.Register("PART_DocumentHost", host);
				return scrollViewer;
			})
		};
		configure(viewer);

		var window = new Window { Width = 600, Height = 500, Content = viewer };
		window.Show();
		Dispatcher.UIThread.RunJobs();
		return (window, viewer);
	}

	private static void WaitForRebuild(MarkdownViewer viewer, SelectableTextBlock[] oldControls)
	{
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (true)
		{
			Dispatcher.UIThread.RunJobs();
			var paragraphs = FindParagraphs(viewer);
			if (paragraphs.Length == oldControls.Length && !ReferenceEquals(paragraphs[0], oldControls[0]))
			{
				return;
			}

			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException("整篇重绘未重建段落控件");
			}

			Thread.Sleep(10);
		}
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
