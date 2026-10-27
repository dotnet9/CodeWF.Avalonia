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
/// Wysiwyg 块编辑器模板内 TextPresenter 双向绑定回归：
/// box→presenter（文本/排版正向）与 presenter→box（光标/选区回写）两个方向都必须保持同步。
/// 该绑定此前是字符串反射 Binding（AOT 门禁 IL2026/IL3050），替换绑定机制后由本测试兜底。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownBlockViewPresenterBindingTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownBlockViewPresenterBindingTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void PresenterStaysTwoWaySyncedWithEditorBox() => _platform.Run(() =>
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

            // presenter → box：光标与选区反向回写（反射 Binding 的 TwoWay 语义）。
            presenter.CaretIndex = 3;
            Assert.Equal(3, box.CaretIndex);
            presenter.SelectionStart = 1;
            presenter.SelectionEnd = 4;
            Assert.Equal(1, box.SelectionStart);
            Assert.Equal(4, box.SelectionEnd);
        }
        finally { window.Close(); }
    });
}
