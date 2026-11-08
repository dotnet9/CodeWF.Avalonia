using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;

using global::Avalonia;
using global::Avalonia.Automation;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 代码块外框：左侧「语言 · 路径#行段」标题，右侧图标工具条（下载/换行/复制/折叠），
/// 折叠后底部展示「展开全部 N 行」。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownCodeBlockFrameTests(AvaloniaPlatformFixture platform)
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void BlogFrontMatter_RendersMetadataInsteadOfCode(bool customSyntax) => platform.Run(() =>
	{
		const string source = "---\ntitle: Avalonia 博文\ndate: 2024-12-05 21:47:17\ncategories:\n  - Avalonia UI\ntags:\n  - \"C#\"\ndraft: false\n---\n\n# 正文\n\n```csharp\nstring? text = null;\n```\n";
		var (window, viewer) = CreateViewer(source, viewer =>
		{
			if (customSyntax) viewer.SyntaxOptions = new MarkdownSyntaxOptions();
		});
		try
		{
			var metadata = Assert.Single(viewer.GetVisualDescendants().OfType<Border>(),
				border => border.Classes.Contains("MdFrontMatter"));
			var text = metadata.GetVisualDescendants().OfType<TextBlock>().Select(block => block.Text).ToArray();
			Assert.Contains("title", text);
			Assert.Contains("Avalonia 博文", text);
			Assert.Contains("  - Avalonia UI", text);
			Assert.Single(viewer.GetVisualDescendants().OfType<Border>(),
				border => border.Classes.Contains(MarkdownStyleKeys.CodeBlock));
			Assert.Equal(source, viewer.Markdown);
		}
		finally { window.Close(); }
	});

	[Theory]
	[InlineData("#FFFFFF", false)]
	[InlineData("#202020", true)]
	[InlineData("#20000000", false)]
	public void DetachedViewer_ResolvesCodeBackgroundWithoutATheme(string background, bool dark) => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer(BuildCodeDocument(3));
		try
		{
			window.Content = null;
			viewer.CodeBackgroundBrush = Brush.Parse(background);
			Assert.Equal(dark, ((IMarkdownRenderContext)viewer).CodeBlockIsDark);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void HostBorderStyle_AppliesAfterEveryDocumentRebuild() => platform.Run(() =>
	{
		var (window, viewer) = CreateViewer(BuildCodeDocument(10), configure: viewer =>
		{
			viewer.Styles.Add(new Style(selector => selector.OfType<Border>().Class(MarkdownStyleKeys.CodeBlock))
			{
				Setters =
				{
					new Setter(Border.MarginProperty, new Thickness(2, 3)),
					new Setter(Border.PaddingProperty, new Thickness(4, 5)),
					new Setter(Border.BorderThicknessProperty, new Thickness(1)),
					new Setter(Border.CornerRadiusProperty, new CornerRadius(6))
				}
			});
		});
		try
		{
			Border? previous = null;
			for (var revision = 0; revision < 3; revision++)
			{
				var source = BuildCodeDocument(11 + revision);
				viewer.Markdown = source;
				WaitForRender(viewer, source);
				window.UpdateLayout();
				var frame = viewer.GetVisualDescendants().OfType<Border>()
					.Single(border => border.Classes.Contains(MarkdownStyleKeys.CodeBlock));
				Assert.NotSame(previous, frame);
				Assert.Equal(new Thickness(2, 3), frame.Margin);
				Assert.Equal(new Thickness(4, 5), frame.Padding);
				Assert.Equal(new Thickness(1), frame.BorderThickness);
				Assert.Equal(new CornerRadius(6), frame.CornerRadius);
				previous = frame;
			}
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Header_ShowsLanguagePathAndLineRange() => platform.Run(() =>
	{
		const string source = "```csharp src/Program.cs#L10-L12\nvar value = 1;\n```\n";
		var (window, viewer) = CreateViewer(source);
		try
		{
			var title = FindTitle(viewer);
			Assert.Equal("csharp · src/Program.cs#L10-12", title.Text);

			var tags = viewer.GetVisualDescendants().OfType<Button>()
				.Select(button => button.Tag as string)
				.ToArray();
			Assert.Contains("CodeCopy", tags);
			Assert.Contains("CodeWrap", tags);
			Assert.Contains("CodeDownload", tags);
			Assert.DoesNotContain("CodeFold", tags, StringComparer.Ordinal);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ShowCodeBlockHeader_HidesTitleAndToolbar() => platform.Run(() =>
	{
		const string source = "```csharp\nvar value = 1;\n```\n";
		var (window, viewer) = CreateViewer(source, configure: viewer => viewer.ShowCodeBlockHeader = false);
		try
		{
			var header = viewer.GetVisualDescendants().OfType<StackPanel>()
				.Single(panel => panel.Classes.Contains(MarkdownStyleKeys.CodeBlockHeader));
			Assert.False(header.IsVisible);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Toolbar_CopyDownloadAndWrapWorkFromTheFrame() => platform.Run(() =>
	{
		const string source = "```csharp src/Program.cs\nvar value = 1;\n```\n";
		CodeBlockSaveEventArgs? saved = null;
		var copied = 0;
		var (window, viewer) = CreateViewer(source, configure: viewer =>
		{
			viewer.CopyClick += (_, _) => copied++;
			viewer.CodeBlockSaveRequested += (_, args) =>
			{
				saved = args;
				args.Handled = true;
			};
		});
		try
		{
			Click(FindButton(viewer, "CodeDownload"), window);
			Assert.NotNull(saved);
			Assert.Equal("var value = 1;", saved!.Code);
			Assert.Equal("csharp", saved.Language);
			Assert.Equal("src/Program.cs", saved.Path);
			Assert.Equal("Program.cs", saved.SuggestedFileName);

			Click(FindButton(viewer, "CodeCopy"), window);
			Assert.Equal(1, copied);

			var code = FindCodeText(viewer);
			Assert.Equal(TextWrapping.NoWrap, code.TextWrapping);
			Click(FindButton(viewer, "CodeWrap"), window);
			Assert.Equal(TextWrapping.Wrap, code.TextWrapping);
			Assert.Equal(MarkdownLocalization.GetResource(MarkdownL.CodeUnwrapLines),
				ToolTip.GetTip(FindButton(viewer, "CodeWrap")));
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void Fold_ShowsExpandFooterAndRestoresContent() => platform.Run(() =>
	{
		var source = BuildCodeDocument(12);
		var (window, viewer) = CreateViewer(source);
		try
		{
			var fold = FindButton(viewer, "CodeFold");
			Assert.Equal("▾", fold.Content);

			Click(fold, window);
			var footer = viewer.GetVisualDescendants().OfType<StackPanel>()
				.Single(panel => panel.Classes.Contains(MarkdownStyleKeys.CodeBlockFooter));
			Assert.True(footer.IsVisible);
			Assert.False(IsCodeContentVisible(viewer));
			var expand = FindButton(viewer, "CodeExpand");
			Assert.Equal(string.Format(MarkdownLocalization.GetResource(MarkdownL.CodeExpandAllLinesFormat), 12),
				expand.Content);

			Click(expand, window);
			Assert.True(IsCodeContentVisible(viewer));
			Assert.False(footer.IsVisible);
			Assert.Equal("▾", FindButton(viewer, "CodeFold").Content);
		}
		finally
		{
			window.Close();
		}
	});

	[Fact]
	public void ShortCodeBlock_HasNoFoldButton() => platform.Run(() =>
	{
		var source = BuildCodeDocument(3);
		var (window, viewer) = CreateViewer(source);
		try
		{
			Assert.Null(viewer.GetVisualDescendants().OfType<Button>()
				.FirstOrDefault(button => Equals(button.Tag, "CodeFold")));
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

	private static (Window Window, MarkdownViewer Viewer) CreateViewer(
		string source,
		Action<MarkdownViewer>? configure = null)
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
		configure?.Invoke(viewer);

		var window = new Window { Width = 600, Height = 500, Content = viewer };
		window.Show();
		Dispatcher.UIThread.RunJobs();
		WaitForRender(viewer, source);
		return (window, viewer);
	}

	private static SelectableTextBlock FindTitle(MarkdownViewer viewer) =>
		viewer.GetVisualDescendants().OfType<SelectableTextBlock>()
			.Single(text => text.Classes.Contains(MarkdownStyleKeys.CodeLanguage));

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
