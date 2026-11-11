using System.Text;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;
using CodeWF.Avalonia.Markdown.Images;
using CodeWF.Avalonia.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Mermaid;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

/// <summary>
/// 图片源码交互（Typora 式「图上方源码框」）与消失 bug 的回归测试：
/// 即时编辑里点图片不再整块换成原文（图片消失、失焦又回来），而是与图表一致的媒体布局；
/// 阅读视图仅编辑态 MarkdownViewer 展开源码框，纯阅读点击弹出放大查看对话框（纯查看）。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownImageSourceEditTests(AvaloniaPlatformFixture platform)
{
    private static string CreateSvgSource() => "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(
        "<svg xmlns='http://www.w3.org/2000/svg' width='140' height='70'><rect width='140' height='70'/></svg>"));

    /// <summary>测试宿主没有查看器控件主题：直接给出最小模板（文档宿主），与渲染管线测试同做法。</summary>
    private static MarkdownViewer CreateViewer(string markdown, bool editingEnabled = true)
    {
        var documentHost = new StackPanel();
        return new MarkdownViewer
        {
            Markdown = markdown,
            Width = 640,
            IsDiagramEditingEnabled = editingEnabled,
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", documentHost);
                return new ScrollViewer { Content = documentHost };
            })
        };
    }

    private static void AddTestStyles(Window window)
    {
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
    }

    /// <summary>等待正文里出现已加载（有位图源）的渲染图片；超时抛出诊断信息。</summary>
    private static Image WaitForRenderedImage(Control host, Window window)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            var rendered = host.GetVisualDescendants().OfType<Image>().FirstOrDefault(item => item.Source is not null);
            if (rendered is not null && rendered.Bounds.Height > 0) return rendered;
            Thread.Sleep(10);
        }

        var contents = host.GetVisualDescendants().OfType<MarkdownImage>()
            .Select(item => $"{item.AltText}:{item.Source}:{item.GetVisualDescendants().OfType<Image>().Count()}")
            .ToList();
        throw new Xunit.Sdk.XunitException("Rendered image did not appear. MarkdownImage contents: " + string.Join(", ", contents));
    }

    private static void ClickAt(Window window, Control target) =>
        ClickAt(window, target, new global::Avalonia.Point(target.Bounds.Width / 2, target.Bounds.Height / 2));

    private static void ClickAt(Window window, Control target, global::Avalonia.Point targetPoint)
    {
        var point = target.TranslatePoint(targetPoint, window)!.Value;
        window.MouseMove(point); window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void StandaloneImageRecordsSyntaxSpanAndInlineImageDoesNot() => platform.Run(() =>
    {
        CodeWFMarkdownImagesExtensions.UseImages();
        var svgSource = CreateSvgSource();
        const string inlineText = "前文 ";
        var markdown = "# Title\n\n![logo](" + svgSource + ")\n\n" + inlineText + "![inline](" + svgSource + ")";
        var viewer = CreateViewer(markdown);
        var window = new Window { Width = 640, Height = 800, Content = viewer };
        AddTestStyles(window);
        window.Show();
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (viewer.GetVisualDescendants().OfType<MarkdownImage>().Count() < 2 && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(10); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            }

            var images = viewer.GetVisualDescendants().OfType<MarkdownImage>().ToList();
            Assert.Equal(2, images.Count);

            // 单图段落：记录语法区间，切片必须精确还原原文（含 data URI 括号内的特殊字符）。
            var standalone = images.Single(item => item.AltText == "logo");
            var span = MarkdownImageEdit.GetSourceSpan(standalone);
            Assert.NotNull(span);
            Assert.Equal("![logo](" + svgSource + ")", markdown.Substring(span!.Start, span.Length));

            // 行内图片：不记录区间，保持点击放大，不给源码编辑入口。
            var inline = images.Single(item => item.AltText == "inline");
            Assert.Null(MarkdownImageEdit.GetSourceSpan(inline));
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });

    [Fact]
    public void EditingViewerImageClickOpensSourceAboveImageAndCommitsViaEditCommitted() => platform.Run(() =>
    {
        CodeWFMarkdownImagesExtensions.UseImages();
        var svgSource = CreateSvgSource();
        var imageSyntax = "![logo](" + svgSource + ")";
        var markdown = "# Title\n\n" + imageSyntax + "\n\nAfter.";
        var viewer = CreateViewer(markdown);
        var outside = new Button { Content = "Outside" };
        var window = new Window { Width = 640, Height = 800, Content = new StackPanel { Children = { viewer, outside } } };
        AddTestStyles(window);

        MermaidEditEventArgs? received = null;
        MermaidEditing.AddEditCommittedHandler(viewer, (_, args) => received = args);
        window.Show();
        try
        {
            var rendered = WaitForRenderedImage(viewer, window);

            // 点击图片：源码框在图片上方展开，图片本体保留（消失 bug 回归点）。
            ClickAt(window, rendered);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();

            var input = viewer.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(item => item.Name == "PART_SourceInput");
            Assert.NotNull(input);
            Assert.True(input!.IsVisible);
            Assert.True(input.IsKeyboardFocusWithin);
            Assert.Equal(imageSyntax, input.Text);
            var imageOrigin = rendered.TranslatePoint(default, viewer)!.Value;
            var inputOrigin = input.TranslatePoint(default, viewer)!.Value;
            Assert.True(inputOrigin.Y + input.Bounds.Height <= imageOrigin.Y + 6,
                $"Source box must sit above the image: input={inputOrigin.Y}+{input.Bounds.Height}, image={imageOrigin.Y}");
            Assert.True(rendered.Bounds.Height > 0, "The image must stay visible while its source is being edited.");

            // 修改源码后点击外部收起：经 MermaidEditing.EditCommitted 同一通道回写。
            input.SelectAll();
            const string replaced = "![logo](replaced-image.svg)";
            window.KeyTextInput(replaced);
            Dispatcher.UIThread.RunJobs();
            Assert.Null(received);
            ClickAt(window, outside, new global::Avalonia.Point(4, 4));
            Dispatcher.UIThread.RunJobs();

            Assert.NotNull(received);
            Assert.Equal(markdown, received!.OriginalMarkdown);
            Assert.Equal(markdown.Replace(imageSyntax, replaced), received.Markdown);
            Assert.Equal(markdown.IndexOf(imageSyntax, StringComparison.Ordinal), received.Start);
            Assert.False(input.IsVisible);
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });

    [Fact]
    public void EscapeCancelsImageSourceEditWithoutWriteBack() => platform.Run(() =>
    {
        CodeWFMarkdownImagesExtensions.UseImages();
        var svgSource = CreateSvgSource();
        var imageSyntax = "![logo](" + svgSource + ")";
        var markdown = "# Title\n\n" + imageSyntax + "\n\nAfter.";
        var viewer = CreateViewer(markdown);
        var window = new Window { Width = 640, Height = 800, Content = viewer };
        AddTestStyles(window);

        var commitCount = 0;
        MermaidEditing.AddEditCommittedHandler(viewer, (_, _) => commitCount++);
        window.Show();
        try
        {
            var rendered = WaitForRenderedImage(viewer, window);
            ClickAt(window, rendered);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();

            var input = viewer.GetVisualDescendants().OfType<TextBox>().First(item => item.Name == "PART_SourceInput");
            input.SelectAll();
            window.KeyTextInput("![logo](changed.svg)");
            Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();

            Assert.False(input.IsVisible);
            Assert.Equal(0, commitCount);
            // 取消后图片源恢复原文，图区重新加载原文图片。
            Assert.Equal(svgSource, viewer.GetVisualDescendants().OfType<MarkdownImage>().First().Source);
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });

    [Fact]
    public void ReadingViewerImageClickOpensZoomDialogAndEscapeCloses() => platform.Run(() =>
    {
        CodeWFMarkdownImagesExtensions.UseImages();
        var svgSource = CreateSvgSource();
        var markdown = "# Title\n\n![logo](" + svgSource + ")\n\nAfter.";
        var viewer = CreateViewer(markdown, editingEnabled: false);
        var window = new Window { Width = 640, Height = 800, Content = viewer };
        AddTestStyles(window);

        var commitCount = 0;
        MermaidEditing.AddEditCommittedHandler(viewer, (_, _) => commitCount++);
        window.Show();
        try
        {
            var rendered = WaitForRenderedImage(viewer, window);
            var image = viewer.GetVisualDescendants().OfType<MarkdownImage>().Single();

            ClickAt(window, rendered);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();

            // 纯阅读：点击弹放大查看对话框（遮罩层挂在窗口内、可直接枚举），无源码框、无回写。
            var dialog = window.GetVisualDescendants().OfType<MarkdownMediaZoomDialog>().Single();
            Assert.DoesNotContain(viewer.GetVisualDescendants().OfType<TextBox>(),
                item => item.Name == "PART_SourceInput" && item.IsVisible);
            Assert.Equal(0, commitCount);
            Assert.True(rendered.Bounds.Height > 0);
            Assert.Equal(svgSource, image.Source);

            // Esc 关闭对话框，遮罩层移除；再次点击可重新打开。
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.DoesNotContain(window.GetVisualDescendants(), item => item is MarkdownMediaZoomDialog);

            ClickAt(window, rendered);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Contains(window.GetVisualDescendants(), item => item is MarkdownMediaZoomDialog);
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });

    [Fact]
    public void ReadingViewerMermaidClickOpensZoomDialog() => platform.Run(() =>
    {
        CodeWF.Avalonia.Markdown.Mermaid.CodeWFMarkdownMermaidExtensions.EnsureRegistered();
        const string source = "flowchart LR\nA[Input] --> B[Output]";
        var viewer = CreateViewer("# Title\n\n```mermaid\n" + source + "\n```\n\nAfter.", editingEnabled: false);
        var window = new Window { Width = 640, Height = 800, Content = viewer };
        AddTestStyles(window);

        var commitCount = 0;
        MermaidEditing.AddEditCommittedHandler(viewer, (_, _) => commitCount++);
        window.Show();
        try
        {
            // 等阅读态的图表交互壳出现（渲染器把图表包进 MermaidDiagramView 的可命中按钮）。
            Button? previewButton = null;
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline)
            {
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
                previewButton = viewer.GetVisualDescendants().OfType<Button>()
                    .FirstOrDefault(item => item.Name == "PreviewButton" && item.Bounds.Height > 0);
                if (previewButton is not null) break;
                Thread.Sleep(10);
            }

            Assert.NotNull(previewButton);
            Assert.True(previewButton!.IsHitTestVisible, "阅读态图表必须可命中（点击 = 放大查看）");

            // 点击图表：弹放大查看对话框（独立渲染管线只读重画），无源码框、无回写。
            ClickAt(window, previewButton);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();

            var dialog = window.GetVisualDescendants().OfType<MarkdownMediaZoomDialog>().Single();
            Assert.Contains(dialog.GetVisualDescendants().OfType<Border>(),
                item => item.Classes.Contains("MdMermaidPreview"));
            // 只读渲染：对话框内没有 mermaid 语言徽标。
            Assert.DoesNotContain(dialog.GetVisualDescendants().OfType<Button>(),
                item => item.Content as string == "mermaid");
            Assert.Equal(0, commitCount);

            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.DoesNotContain(window.GetVisualDescendants(), item => item is MarkdownMediaZoomDialog);
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });

    [Fact]
    public void LiveEditorImageBlockShowsSourceAbovePreviewAndEnterCommits() => platform.Run(() =>
    {
        CodeWFMarkdownImagesExtensions.UseImages();
        var svgSource = CreateSvgSource();
        var imageSyntax = "![logo](" + svgSource + ")";
        var view = new MarkdownLiveEditorView { HeaderText = null, ChipText = null };
        view.SetText(imageSyntax + "\n\nAfter image.");
        var window = new Window { Width = 640, Height = 800, Content = view };
        AddTestStyles(window);
        window.Show();
        try
        {
            var rendered = WaitForRenderedImage(view, window);
            ClickAt(window, rendered);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); Dispatcher.UIThread.RunJobs();

            // 与图表一致：块进入编辑态、源码框在上、渲染预览（图片）在下且仍然可见——不再整块换成原文。
            var block = view.GetVisualDescendants().OfType<MarkdownBlockView>().Single(item => item.IsEditing);
            var editor = block.ActiveEditor!;
            Assert.True(editor.IsKeyboardFocusWithin);
            Assert.Equal(imageSyntax, editor.Text);
            var liveImage = block.GetVisualDescendants().OfType<Image>().First(item => item.Source is not null);
            var editorOrigin = editor.TranslatePoint(default, block)!.Value;
            var imageOrigin = liveImage.TranslatePoint(default, block)!.Value;
            Assert.True(imageOrigin.Y >= editorOrigin.Y + editor.Bounds.Height,
                $"Source must sit above the image: editor={editorOrigin.Y}+{editor.Bounds.Height}, image={imageOrigin.Y}");
            Assert.True(liveImage.Bounds.Height > 0, "The image must stay visible while editing its source.");

            // 回车即提交（与阅读视图图片源码框一致），写回文档全文。
            editor.SelectAll();
            const string replaced = "![logo](live-replaced.svg)";
            window.KeyTextInput(replaced);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(block.IsEditing);
            Assert.Contains(replaced, view.Text);
            Assert.DoesNotContain(imageSyntax, view.Text);
        }
        finally
        {
            window.Close();
            MarkdownViewer.RegisterImageControlFactory(null);
        }
    });
}
