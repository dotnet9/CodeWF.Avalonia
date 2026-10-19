using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Security;

/// <summary>
/// 链接点击事件（防钓鱼）：链接被点击时 LinkClicked 在打开之前触发，
/// 文字与目标不符的链接带 RequiresConfirmation 标记，宿主置 Cancel 阻止打开。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownLinkClickTests(AvaloniaPlatformFixture platform)
{
	[Fact]
	public void ClickingMisleadingLink_RaisesEventWithConfirmationFlag() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer("[https://github.com](https://evil.example)\n");
		var events = new List<MarkdownLinkClickEventArgs>();
		viewer.LinkClicked += (_, args) =>
		{
			events.Add(args);
			args.Cancel = true;
		};

		try
		{
			Assert.True(ClickLinkCenter(window, viewer), "未能在链接上完成点击");

			var args = Assert.Single(events);
			Assert.Equal("https://evil.example", args.Url);
			Assert.Equal("https://github.com", args.LinkText);
			Assert.True(args.RequiresConfirmation);
			Assert.True(args.Cancel);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ClickingNormalLink_DoesNotRequireConfirmation() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer("[项目主页](https://github.com/dotnet9)\n");
		var events = new List<MarkdownLinkClickEventArgs>();
		viewer.LinkClicked += (_, args) =>
		{
			events.Add(args);
			// 置 Cancel 避免测试环境真的拉起浏览器。
			args.Cancel = true;
		};

		try
		{
			Assert.True(ClickLinkCenter(window, viewer), "未能在链接上完成点击");

			var args = Assert.Single(events);
			Assert.Equal("https://github.com/dotnet9", args.Url);
			Assert.Equal("项目主页", args.LinkText);
			Assert.False(args.RequiresConfirmation);
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
		return (window, viewer);
	}

	/// <summary>找到挂接链接交互的文本块，模拟点击链接文字中心。</summary>
	private static bool ClickLinkCenter(Window window, MarkdownViewer viewer)
	{
		var textBlock = viewer.GetVisualDescendants()
			.OfType<SelectableTextBlock>()
			.FirstOrDefault(block => block.Tag is System.Collections.Generic.IReadOnlyList<global::CodeWF.Avalonia.Markdown.Rendering.MarkdownLinkSpan> { Count: > 0 });
		if (textBlock?.Inlines is not { } inlines)
		{
			return false;
		}

		// 内容经由 Inlines 构建，Text 属性为空，递归提取纯文本定位链接中点。
		var text = ExtractText(inlines);
		if (text.Length == 0)
		{
			return false;
		}

		var textLayout = textBlock.TextLayout;
		var hit = textLayout.HitTestTextPosition(text.Length / 2);
		if (textBlock.TranslatePoint(hit.Center, window) is not { } windowPoint)
		{
			return false;
		}

		window.MouseMove(windowPoint);
		window.MouseDown(windowPoint, MouseButton.Left);
		window.MouseUp(windowPoint, MouseButton.Left);
		Dispatcher.UIThread.RunJobs();
		return true;
	}

	private static string ExtractText(global::Avalonia.Controls.Documents.InlineCollection inlines)
	{
		var builder = new System.Text.StringBuilder();
		foreach (var inline in inlines)
		{
			switch (inline)
			{
				case global::Avalonia.Controls.Documents.Run run:
					builder.Append(run.Text);
					break;
				case global::Avalonia.Controls.Documents.Span span:
					builder.Append(ExtractText(span.Inlines));
					break;
				case global::Avalonia.Controls.Documents.LineBreak:
					builder.Append('\n');
					break;
			}
		}

		return builder.ToString();
	}
}
