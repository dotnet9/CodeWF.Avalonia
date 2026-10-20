using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using global::Avalonia.LogicalTree;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

public sealed class MarkdownSyntaxOptionsTests
{
    [Fact]
    public void OptionsAreIndependentAndKeepUnrelatedExtensions()
    {
        const string source = "==重点== ~低~ ^高^ ~~删除~~ https://codewf.com\n下一行\n\n| A | B |\n| - | - |\n| 1 | 2 |";
        var enabled = new MarkdownSyntaxOptions(SoftBreaks: true).CreatePipeline();
        var disabled = new MarkdownSyntaxOptions(Subscript: false, Highlight: false, AutoLinks: false).CreatePipeline();
        var first = Markdig.Markdown.ToHtml(source, enabled);
        var second = Markdig.Markdown.ToHtml(source, disabled);
        Assert.Contains("<mark>", first); Assert.Contains("<sub>", first); Assert.Contains("<sup>", first);
        Assert.Contains("<a href=", first); Assert.Contains("<br", first);
        Assert.DoesNotContain("<mark>", second); Assert.DoesNotContain("<sub>", second); Assert.DoesNotContain("<sup>", second);
        Assert.DoesNotContain("<a href=", second);
        Assert.Contains("<del>", second); Assert.Contains("<table>", second);
        Assert.Equal(first, Markdig.Markdown.ToHtml(source, enabled));
    }
}

[Collection("AvaloniaPlatform")]
public sealed class MarkdownSyntaxControlTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void CodeWrappingAndFontChangesUpdateExistingViewer() => platform.Run(() =>
    {
        var viewer = new MarkdownViewer { Markdown = "```text\nlong code line\n```" };
        viewer.Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
        {
            var panel = new StackPanel(); scope.Register("PART_DocumentHost", panel); return panel;
        });
        var window = new Window { Content = viewer, Width = 300, Height = 240 };
        window.Show();
        try
        {
            viewer.CodeBlockWrap = true; viewer.CodeBlockFontSize = 19; viewer.Rerender(); Dispatcher.UIThread.RunJobs();
            var code = viewer.GetVisualDescendants().OfType<ScrollViewer>()
                .SelectMany(scroll => scroll.GetLogicalDescendants()).OfType<SelectableTextBlock>()
                .First(x => x.Classes.Contains("MdCodeBlockPlain") || x.Classes.Contains("MdCodeBlockText"));
            Assert.Equal(global::Avalonia.Media.TextWrapping.Wrap, code.TextWrapping);
            Assert.Equal(19, code.FontSize);
            Assert.Equal("```text\nlong code line\n```", viewer.Markdown);
        }
        finally { window.Close(); }
    });


    [Fact]
    public void ChangingOneViewerReparsesOnlyThatViewerWithoutChangingSource() => platform.Run(() =>
    {
        const string source = "==重点== ~低~ ^高^\n下一行";
        MarkdownViewer Create(MarkdownSyntaxOptions options) => new()
        {
            SyntaxOptions = options, Markdown = source,
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                var panel = new StackPanel(); scope.Register("PART_DocumentHost", panel); return panel;
            })
        };
        var first = Create(new MarkdownSyntaxOptions(SoftBreaks: true));
        var second = Create(new MarkdownSyntaxOptions(Subscript: false, Highlight: false));
        var window = new Window { Content = new StackPanel { Children = { first, second } } };
        window.Show();
        try
        {
            first.Rerender(); second.Rerender(); Dispatcher.UIThread.RunJobs();
            Assert.Equal("重点 低 高\n下一行", first.GetRenderedText().Replace("\r\n", "\n"));
            Assert.Contains("==重点==", second.GetRenderedText());
            var spans = first.GetVisualDescendants().OfType<TextBlock>()
                .SelectMany(text => text.Inlines?.OfType<Span>() ?? []).ToArray();
            Assert.Contains(spans, span => span.Background is not null);
            Assert.Contains(spans, span => span.BaselineAlignment == global::Avalonia.Media.BaselineAlignment.Subscript);
            first.SyntaxOptions = new MarkdownSyntaxOptions(Subscript: false, Highlight: false);
            first.Rerender(); Dispatcher.UIThread.RunJobs();
            Assert.Contains("==重点==", first.GetRenderedText());
            Assert.Equal(source, first.Markdown); Assert.Equal(source, second.Markdown);
            second.SyntaxOptions = new MarkdownSyntaxOptions(); second.Rerender();
            Assert.DoesNotContain("==", second.GetRenderedText());
        }
        finally { window.Close(); }
    });

    [Fact]
    public void LiveSyntaxChangePreservesEditsAndUndo() => platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("正文"); view.FocusBlock(0);
        view.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();
        var edited = view.Text;
        view.SyntaxOptions = new MarkdownSyntaxOptions(Math: false, Diagrams: false, Highlight: false);
        Assert.Equal(edited, view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal("正文", view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Redo).GetAwaiter().GetResult();
        Assert.Equal(edited, view.Text);
    });
}
