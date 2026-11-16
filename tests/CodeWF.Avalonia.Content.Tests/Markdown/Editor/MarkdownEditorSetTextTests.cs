using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Editor.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

/// <summary>
/// <see cref="MarkdownEditorView.SetText"/> 整体替换回归：无论新文档比旧文档长还是短，
/// 都必须一次性整体替换（AvaloniaEdit 全量 Rope 重建），不允许出现
/// "增量替换把长度差误算成负的删除长度" 的路径——宿主外部修改静默载入正走这条链路。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorSetTextTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownEditorSetTextTests(AvaloniaPlatformFixture platform) => _platform = platform;

    private const string LongDocument =
        """
        # 长文档

        第一段：用于构造比新文档明显更长的初始内容，覆盖多行与中英文混排。

        第二段：AvaloniaEdit 的 TextDocument.Text 走 Rope Clear + Insert 整体重建，
        本次回归守住这条路径不被改成按长度差增量替换。

        第三段：后续段落继续撑长初始文本，确保旧文档长度显著大于新文档。

        第四段：再补一段。

        第五段：最后一长段。

        ## 小节

        - 列表项一
        - 列表项二
        - 列表项三

        ```csharp
        var answer = 42;
        ```

        结尾段落。
        """;

    private const string ShortDocument = "# 短文档\n\n短正文。";

    [Fact]
    public void SetText_ReplacesShorterDocumentWholeAndResetsCaret() => _platform.Run(() =>
    {
        Application.Current!.Styles.Add(new Semi.Avalonia.SemiTheme());
        var view = new MarkdownEditorView();
        var window = new Window { Width = 480, Height = 360, Content = view };
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var changedCount = 0;
            view.MarkdownChanged += (_, _) => changedCount++;

            view.SetText(LongDocument);
            Assert.Equal(LongDocument, view.Text);

            // 关键场景：新文档更短。走整体替换不得抛异常、不得残留旧文本尾部。
            view.SetText(ShortDocument);
            Assert.Equal(ShortDocument, view.Text);

            var textEditor = view.GetVisualDescendants().OfType<AvaloniaEdit.TextEditor>().Single();
            Assert.Equal(ShortDocument, textEditor.Text);
            Assert.Equal(0, textEditor.CaretOffset);

            // SetText 约定：不触发 MarkdownChanged（载入中的中间文本不能回抛成用户编辑）。
            Assert.Equal(0, changedCount);

            // 反方向（新文档更长）同样整体替换成功。
            view.SetText(LongDocument);
            Assert.Equal(LongDocument, view.Text);
            Assert.Equal(0, changedCount);
        }
        finally
        {
            window.Close();
        }
    });
}
