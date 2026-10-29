using Avalonia.Automation.Peers;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Automation;
using CodeWF.Avalonia.Markdown.Controls;

using global::Avalonia.Automation;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Automation;

/// <summary>
/// 无障碍基线：Viewer 根 peer 暴露文档角色与纯文本名称，
/// 代码块复制按钮名称可区分（能力边界见 MarkdownViewerAutomationPeer）。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerAutomationTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void ViewerPeer_ExposesDocumentRoleAndPlainTextName() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer("# 标题\n\n正文内容甲\n\n正文内容乙\n");
		try
		{
			var peer = new MarkdownViewerAutomationPeer(viewer);

			Assert.Equal(AutomationControlType.Document, peer.GetAutomationControlType());
			Assert.Equal(nameof(MarkdownViewer), peer.GetClassName());
			var name = peer.GetName();
			Assert.Contains("正文内容甲", name);
			Assert.Contains("正文内容乙", name);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ViewerPeer_TruncatesLongPlainTextName() => platform.Run(() =>
	{
		var longParagraph = new string('字', 3000);
		var (window, viewer) = CreateViewer(longParagraph);
		try
		{
			var peer = new MarkdownViewerAutomationPeer(viewer);

			var name = peer.GetName();
			Assert.NotNull(name);
			Assert.True(name.Length <= 2001, $"名称长度 {name.Length} 超过截断上限");
			Assert.EndsWith("…", name);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Headings_ExposeAutomationLevel() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer("# 一级\n\n正文\n\n### 三级\n");
		try
		{
			var headings = viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
				.Where(text => text.Classes.Contains(MarkdownStyleKeys.Heading))
				.ToArray();

			Assert.Equal(2, headings.Length);
			Assert.Equal(1, AutomationProperties.GetHeadingLevel(headings[0]));
			Assert.Equal(3, AutomationProperties.GetHeadingLevel(headings[1]));
			Assert.Equal(0, AutomationProperties.GetHeadingLevel(
				viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
					.Single(text => text.Classes.Contains(MarkdownStyleKeys.Paragraph))));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void CopyButton_NameIncludesLanguage() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer("```csharp\nvar value = 1;\n```\n");
		try
		{
			var button = viewer.GetVisualDescendants().OfType<Button>().FirstOrDefault();
			Assert.NotNull(button);

			var name = AutomationProperties.GetName(button);
			Assert.False(string.IsNullOrWhiteSpace(name));
			Assert.Contains("csharp", name);
		}
		finally
		{
			window.Close();
		}
	});

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
