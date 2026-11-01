using CodeWF.Avalonia.Markdown.Controls;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownInlineFontLayoutTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void LargerInlineFontExpandsTheContainingLineBox() => platform.Run(() =>
    {
        var panel = new StackPanel();
        var viewer = new MarkdownViewer
        {
            Markdown = "前<span style=\"font-size:86px\">想</span>法，后面的文字仍在同一段。\n\n下一段。",
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", panel);
                return panel;
            })
        };
        var window = new Window { Width = 640, Height = 360, Content = viewer };
        try
        {
            window.Show();
            viewer.Rerender();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var paragraphs = panel.Children.OfType<SelectableTextBlock>().ToArray();
            Assert.Equal(2, paragraphs.Length);
            var paragraph = paragraphs[0];
            var nextParagraph = paragraphs[1];
            Assert.True(paragraph.Bounds.Height > nextParagraph.Bounds.Height * 2,
                $"inline 86px text must get a safe line box, actual heights={paragraph.Bounds.Height}/{nextParagraph.Bounds.Height}");
            Assert.True(paragraph.TextLayout?.Height > nextParagraph.TextLayout?.Height * 2,
                $"text layout must retain the expanded line box, actual heights={paragraph.TextLayout?.Height}/{nextParagraph.TextLayout?.Height}");
        }
        finally
        {
            window.Close();
        }
    });
}
