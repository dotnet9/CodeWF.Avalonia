using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.LogicalTree;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

/// <summary>
/// Wysiwyg 使用标准 TextBox 模板语义：输入框驱动呈现部件，呈现部件移动光标不抢写选区。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownBlockViewPresenterBindingTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownBlockViewPresenterBindingTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void EditorDrivesPresenterWithoutReverseCaretSelectionReset() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("第一段文字\n\n第二段文字");
        var window = new Window { Width = 900, Height = 600, Content = view };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            view.FocusBlock(0);
            Dispatcher.UIThread.RunJobs();

            var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().First(b => b.IsEditing);
            var box = block.ActiveEditor!;
            var presenter = Assert.IsAssignableFrom<TextPresenter>(box.GetVisualDescendants()
                .OfType<TextPresenter>().FirstOrDefault(p => p.Name == "PART_TextPresenter"));

            // box → presenter：文本与排版属性正向同步。
            box.Text = "第一段文字已修改";
            Assert.Equal("第一段文字已修改", presenter.Text);
            box.FontSize = 22;
            Assert.Equal(22, presenter.GetValue(TextElement.FontSizeProperty));
            box.FontWeight = FontWeight.Bold;
            Assert.Equal(FontWeight.Bold, presenter.GetValue(TextElement.FontWeightProperty));
            box.LineHeight = 31;
            Assert.Equal(31, presenter.LineHeight);

            box.CaretIndex = 4;
            box.SelectionStart = 1;
            box.SelectionEnd = 4;
            Assert.Equal(1, presenter.SelectionStart);
            Assert.Equal(4, presenter.SelectionEnd);
            presenter.MoveCaretToTextPosition(3);
            Assert.Equal(1, box.SelectionStart);
            Assert.Equal(4, box.SelectionEnd);
        }
        finally { window.Close(); }
    });
}
