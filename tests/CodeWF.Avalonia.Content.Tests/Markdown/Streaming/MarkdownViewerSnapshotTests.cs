using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Streaming;

using global::Avalonia.Controls;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Streaming;

/// <summary>
/// 即时编辑快照入口（<see cref="MarkdownViewer.ApplySnapshot"/>）：
/// 整篇赋值时按行级对齐搬运折叠状态——前插与中插都保住，删改则丢弃。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerSnapshotTests(AvaloniaPlatformFixture platform)
{
	private const string Document =
		"# 标题\n\n第一段。\n\n```csharp\nvar a = 1;\nvar b = 2;\n```\n\n结尾。\n";

	private static readonly int CodeBlockOffset = "# 标题\n\n第一段。\n\n".Length;

	[Fact]
	public void ApplySnapshot_KeepsFoldState_WhenLinesAreInsertedAbove() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer(Document);
		try
		{
			WaitRendered(viewer, Document);
			viewer.SetCodeBlockFolded(CodeBlockOffset, true);
			Assert.True(viewer.IsCodeBlockFolded(CodeBlockOffset));

			// 前插一行：代码块整体下移、块键漂移，折叠状态必须跟到新偏移上。
			var edited = "新增一行。\n\n" + Document;
			viewer.ApplySnapshot(edited);
			WaitRendered(viewer, edited);

			Assert.Equal(edited, viewer.Markdown);
			var shiftedOffset = CodeBlockOffset + "新增一行。\n\n".Length;
			Assert.True(viewer.IsCodeBlockFolded(shiftedOffset));
			Assert.False(viewer.IsCodeBlockFolded(CodeBlockOffset));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ApplySnapshot_DropsFoldState_WhenTheBlockIsReplaced() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer(Document);
		try
		{
			WaitRendered(viewer, Document);
			viewer.SetCodeBlockFolded(CodeBlockOffset, true);

			// 代码块内容被换成另一段：同一位置的旧状态不该继承。
			var edited = Document.Replace("var a = 1;", "var c = 3;", StringComparison.Ordinal);
			viewer.ApplySnapshot(edited);
			WaitRendered(viewer, edited);

			Assert.False(viewer.IsCodeBlockFolded(CodeBlockOffset));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ApplySnapshot_IsIdempotentForIdenticalText() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer(Document);
		try
		{
			WaitRendered(viewer, Document);
			viewer.SetCodeBlockFolded(CodeBlockOffset, true);
			viewer.ApplySnapshot(Document);
			WaitRendered(viewer, Document);
			Assert.True(viewer.IsCodeBlockFolded(CodeBlockOffset));
		}
		finally
		{
			window.Close();
		}
	});

	private static (Window Window, MarkdownViewer Viewer) CreateViewer(string markdown)
	{
		// 测试宿主要自带最小模板：无应用级主题资源时默认模板不会注入 PART_DocumentHost。
		var panel = new WindowedStackPanel { Spacing = 8 };
		var presenter = new global::Avalonia.Controls.Presenters.ScrollContentPresenter { Content = panel };
		var scrollViewer = new ScrollViewer
		{
			Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<ScrollViewer>((_, scope) =>
			{
				scope.Register("PART_ScrollContentPresenter", presenter);
				return presenter;
			}),
		};
		panel.ScrollHost = scrollViewer;

		var viewer = new MarkdownViewer
		{
			Markdown = markdown,
			ScrollHost = scrollViewer,
			Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<MarkdownViewer>((_, scope) =>
			{
				scope.Register("PART_DocumentHost", panel);
				return scrollViewer;
			}),
		};

		var window = new Window { Width = 600, Height = 500, Content = viewer };
		window.Show();
		Dispatcher.UIThread.RunJobs();
		return (window, viewer);
	}

	/// <summary>渲染是排队的：等到快照模型确实换成了目标文本再断言。</summary>
	private static void WaitRendered(MarkdownViewer viewer, string expected)
	{
		var deadline = DateTime.UtcNow.AddSeconds(5);
		while (viewer.CurrentModel.Source != expected)
		{
			Dispatcher.UIThread.RunJobs();
			if (DateTime.UtcNow > deadline)
			{
				throw new TimeoutException($"渲染未到达预期文本（当前 {viewer.CurrentModel.Source.Length} 字符，期望 {expected.Length}）");
			}

			Thread.Sleep(10);
		}

		Dispatcher.UIThread.RunJobs();
	}
}
