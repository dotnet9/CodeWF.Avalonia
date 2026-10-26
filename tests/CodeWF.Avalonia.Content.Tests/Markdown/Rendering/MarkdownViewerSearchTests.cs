using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Markdown.Controls;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 文档内查找：命中计数、高亮拆分、FindNext/Previous 循环定位、大小写与
/// 整词、清除还原、渲染后重放。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerSearchTests(AvaloniaPlatformFixture platform)
{
	private const string Document =
		"# 搜索标题\n\n正文包含 Alpha 与 alpha 混排。\n\n第二段提到 AlphaDot 边界。\n\n结尾段落。\n";

	[Fact]
	public void Find_CountsMatches_AppliesHighlight_AndClearRestores() => platform.Run(() =>
	{
		var (window, viewer, _) = CreateViewer();
		try
		{
			Dispatcher.UIThread.RunJobs();

			Assert.Equal(3, viewer.Find("alpha"));
			Assert.Equal(3, viewer.MatchCount);
			Assert.Equal(-1, viewer.CurrentMatchIndex);

			var highlighted = CountHighlightedRuns(viewer);
			Assert.Equal(3, highlighted);

			// 大小写敏感：只剩两个 Alpha。
			Assert.Equal(2, viewer.Find("Alpha", caseSensitive: true));

			// 整词：AlphaDot 不计入。
			Assert.Equal(1, viewer.Find("Alpha", caseSensitive: true, wholeWord: true));

			viewer.ClearSearch();
			Assert.Equal(0, viewer.MatchCount);
			Assert.Equal(-1, viewer.CurrentMatchIndex);
			Assert.Equal(0, CountHighlightedRuns(viewer));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void FindNext_CyclesAndMovesCurrentBrush() => platform.Run(() =>
	{
		var (window, viewer, _) = CreateViewer();
		try
		{
			Dispatcher.UIThread.RunJobs();

			Assert.Equal(3, viewer.Find("alpha"));
			Assert.True(viewer.FindNext());

			var first = viewer.CurrentMatchIndex;
			viewer.FindNext();
			var second = viewer.CurrentMatchIndex;
			Assert.NotEqual(first, second);
			Assert.Equal(1, second); // 第二个命中按阅读序为 1 号

			// 循环：再两次回到起点。
			viewer.FindNext();
			viewer.FindNext();
			Assert.Equal(first, viewer.CurrentMatchIndex);

			Assert.True(viewer.FindPrevious());
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Search_SurvivesRerender_AndClearsOnEmptyQuery() => platform.Run(() =>
	{
		var (window, viewer, _) = CreateViewer();
		try
		{
			Dispatcher.UIThread.RunJobs();
			Assert.Equal(2, viewer.Find("Alpha", caseSensitive: true));

			// 同文档重渲染（字号变化走全量）：高亮重放（大小写敏感 2 处命中）。
			viewer.TypographySize = CodeWF.Avalonia.Markdown.Themes.MarkdownTypographySizes.Compact;
			Assert.Equal(2, viewer.Find("Alpha", caseSensitive: true));
			WaitForRender(viewer);
			Assert.Equal(2, CountHighlightedRuns(viewer));

			// 空查询等同清除。
			Assert.Equal(0, viewer.Find("   "));
			Assert.Equal(0, CountHighlightedRuns(viewer));
		}
		finally
		{
			window.Close();
		}
	});

	private static int CountHighlightedRuns(MarkdownViewer viewer)
	{
		return viewer.GetVisualDescendants()
			.OfType<SelectableTextBlock>()
			.SelectMany(block => block.Inlines ?? Enumerable.Empty<global::Avalonia.Controls.Documents.Inline>())
			.OfType<global::Avalonia.Controls.Documents.Run>()
			.Count(run => run.Background is not null);
	}

	private static void WaitForRender(MarkdownViewer viewer)
	{
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (DateTime.UtcNow < deadline)
		{
			Dispatcher.UIThread.RunJobs();
			Thread.Sleep(10);
		}
	}

	private static (Window Window, MarkdownViewer Viewer, ScrollViewer ScrollViewer) CreateViewer()
	{
		var panel = new WindowedStackPanel { EstimatedItemHeight = 42, Spacing = 8 };
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
			Markdown = Document,
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
		return (window, viewer, scrollViewer);
	}
}
