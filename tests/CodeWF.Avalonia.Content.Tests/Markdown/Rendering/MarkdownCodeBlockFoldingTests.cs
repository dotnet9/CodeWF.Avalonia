using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 代码块折叠：长代码块头部出现折叠按钮，点击后就地收起；
/// 折叠状态按块键保留（同文档重渲染不丢，换文档重置）。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownCodeBlockFoldingTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void LongCodeBlock_ShowsFoldButtonAndCollapsesOnClick() => platform.Run(() =>
	{
		var source = BuildCodeDocument(12);
		var (window, viewer) = CreateViewer(source);
		try
		{
			var foldButton = FindFoldButton(viewer);
			Assert.NotNull(foldButton);
			Assert.Equal("▾", foldButton.Content);
			Assert.True(IsCodeContentVisible(viewer));

			Click(foldButton, window);
			Dispatcher.UIThread.RunJobs();

			Assert.False(IsCodeContentVisible(viewer), "点击折叠按钮后代码内容应收起");
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void FoldedState_SurvivesSameDocumentRerender_AndResetsOnDocumentChange() => platform.Run(() =>
	{
		var source = BuildCodeDocument(12);
		var (window, viewer) = CreateViewer(source);
		try
		{
			Click(FindFoldButton(viewer)!, window);
			Dispatcher.UIThread.RunJobs();
			Assert.False(IsCodeContentVisible(viewer));

			// 同文档重渲染（主题/字号变化走全量路径）：折叠状态按块键保留。
			viewer.TypographySize = CodeWF.Avalonia.Markdown.Themes.MarkdownTypographySizes.Small;
			WaitForRender(viewer, source);
			Assert.False(IsCodeContentVisible(viewer), "同文档重渲染不应丢失折叠状态");

			var appendedDocument = source + "\nA new paragraph.\n";
			viewer.Markdown = appendedDocument;
			WaitForRender(viewer, appendedDocument);
			Assert.False(IsCodeContentVisible(viewer), "其他段落变化不应展开未改动的代码块");

			// 换文档：状态重置为展开。
			var otherDocument = BuildCodeDocument(10);
			viewer.Markdown = otherDocument;
			WaitForRender(viewer, otherDocument);
			Assert.True(IsCodeContentVisible(viewer), "更换文档后折叠状态应重置");
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ShortCodeBlock_DoesNotShowFoldButton() => platform.Run(() =>
	{
		var source = BuildCodeDocument(3);
		var (window, viewer) = CreateViewer(source);
		try
		{
			Assert.Null(FindFoldButton(viewer));
			Assert.True(IsCodeContentVisible(viewer));
		}
		finally
		{
			window.Close();
		}
	});

	private static string BuildCodeDocument(int lines)
	{
		var codeLines = string.Concat(Enumerable.Range(0, lines).Select(i => $"var value{i} = {i};\n"));
		return $"```csharp\n{codeLines}```\n";
	}

	private static (Window Window, MarkdownViewer Viewer) CreateViewer(string source)
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
			Markdown = source,
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
		WaitForRender(viewer, source);
		return (window, viewer);
	}

	private static Button? FindFoldButton(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => Equals(button.Tag, "CodeFold"));

	private static bool IsCodeContentVisible(MarkdownViewer viewer)
	{
		// 折叠的判定：代码内容 StackPanel（挂 MdCodeBlockContent 类）是否可见。
		var content = viewer.GetVisualDescendants()
			.OfType<StackPanel>()
			.FirstOrDefault(panel => panel.Classes.Contains(global::CodeWF.Avalonia.Markdown.MarkdownStyleKeys.CodeBlockContent));
		return content?.IsVisible ?? false;
	}

	private static void Click(Button button, Window window)
	{
		var center = new Rect(0, 0, Math.Max(button.Bounds.Width, 1), Math.Max(button.Bounds.Height, 1)).Center;
		if (button.TranslatePoint(center, window) is not { } windowPoint)
		{
			throw new InvalidOperationException("折叠按钮不在窗口可视化树内");
		}

		window.MouseMove(windowPoint);
		window.MouseDown(windowPoint, MouseButton.Left);
		window.MouseUp(windowPoint, MouseButton.Left);
		Dispatcher.UIThread.RunJobs();
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
