using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Controls;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 宿主集成 API（能力部分）：大纲提取与块/源码偏移跳转。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerNavigationTests(AvaloniaPlatformFixture platform)
{
	private static readonly string Document =
		"# 第一章\n\n" + new string('甲', 400) + "\n\n" +
		"## 第二章\n\n" + new string('乙', 400) + "\n\n" +
		"### 第三章\n\n" + new string('丙', 400) + "\n\n" +
		"## 第四章\n\n" + new string('丁', 400) + "\n\n" +
		"### 第五章\n\n" + new string('戊', 400) + "\n";

	[Fact]
	public void GetOutline_ReturnsHeadingsWithSourceOffsets() => platform.Run(() =>
	{
		var (window, viewer, _) = CreateViewer();
		try
		{
			var outline = viewer.GetOutline();

			Assert.Equal(5, outline.Count);
			Assert.Equal("第一章", outline[0].Title);
			Assert.Equal(1, outline[0].Level);
			Assert.Equal(1, outline[0].Line);
			Assert.Equal(0, outline[0].SourceOffset);
			Assert.True(outline[4].SourceOffset > outline[0].SourceOffset);
			Assert.Equal(3, outline[4].Level);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ScrollToBlock_RealizesLaterBlock_AndReturnsTrue() => platform.Run(() =>
	{
		var (window, viewer, scrollViewer) = CreateViewer();
		try
		{
			Dispatcher.UIThread.RunJobs();
			Assert.True(scrollViewer.Extent.Height > scrollViewer.Viewport.Height, "测试文档应可滚动");

			var outline = viewer.GetOutline();
			var targetIndex = IndexOfBlockAtOffset(viewer, outline[4].SourceOffset);
			Assert.True(targetIndex > 0);

			Assert.True(viewer.ScrollToBlock(targetIndex));
			Assert.True(scrollViewer.Offset.Y > 0, "跳转后应离开文档顶部");

			// 视口顶部应停在目标块附近（允许一行的收敛误差）。
			Assert.True(viewer.ScrollToSourceOffset(outline[0].SourceOffset));
			Assert.True(scrollViewer.Offset.Y < 40, "跳回首个标题应接近文档顶部");
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ScrollToSourceOffset_ClampsOutOfRange_ToNearestBlock() => platform.Run(() =>
	{
		var (window, viewer, _) = CreateViewer();
		try
		{
			// 越界偏移钳制到最近块（与阅读锚定定位语义一致）。
			Assert.True(viewer.ScrollToSourceOffset(999_999));
			Assert.True(viewer.ScrollToSourceOffset(-5));

			// 块序号越界返回 false。
			Assert.False(viewer.ScrollToBlock(-1));
			Assert.False(viewer.ScrollToBlock(999));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void TryGetBlockAt_ReturnsBlockUnderPoint() => platform.Run(() =>
	{
		var source = Document;
		var thirdHeadingOffset = source.IndexOf("### 第三章", StringComparison.Ordinal);
		var thirdHeadingLine = 1 + source.AsSpan(0, thirdHeadingOffset).Count('\n');
		var (window, viewer, _) = CreateViewer();
		try
		{
			Dispatcher.UIThread.RunJobs();

			// 视口内逐点扫描应命中已渲染块；跳到第三章后命中点在其标题附近。
			Assert.True(viewer.ScrollToSourceOffset(thirdHeadingOffset));

			var matched = false;
			for (var y = 8.0; y < viewer.Bounds.Height; y += 16)
			{
				if (!viewer.TryGetBlockAt(new Point(viewer.Bounds.Width / 2, y), out var probe))
				{
					continue;
				}

				matched = true;
				Assert.True(probe.SourceLine >= thirdHeadingLine - 1, $"命中行 {probe.SourceLine} 应在第三章（{thirdHeadingLine}）之后");
				break;
			}

			Assert.True(matched, "视口内应至少命中一个已渲染块");

			// 远离内容的点不命中。
			Assert.False(viewer.TryGetBlockAt(new Point(-500, -500), out _));
		}
		finally
		{
			window.Close();
		}
	});

	private static int IndexOfBlockAtOffset(MarkdownViewer viewer, int offset)
	{
		for (var i = viewer.CurrentModel.Blocks.Count - 1; i >= 0; i--)
		{
			if (viewer.CurrentModel.Blocks[i].SourceSpan.Start <= offset)
			{
				return i;
			}
		}

		return 0;
	}

	private static (Window Window, MarkdownViewer Viewer, ScrollViewer ScrollViewer) CreateViewer()
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
			Markdown = Document,
			ScrollHost = scrollViewer,
			Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
			{
				scope.Register("PART_DocumentHost", panel);
				return scrollViewer;
			})
		};

		var window = new Window { Width = 600, Height = 400, Content = viewer };
		window.Show();
		Dispatcher.UIThread.RunJobs();
		return (window, viewer, scrollViewer);
	}
}
