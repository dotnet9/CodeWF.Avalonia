using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;

using global::Avalonia.Controls;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

/// <summary>
/// 编辑器查找的双向定位回归：上一个/下一个各自的起点语义、越界后的回绕，
/// 以及大小写、整词与正则在向上方向上同样生效。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorSearchServiceTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void FindNext_ThenFindPrevious_ReturnsToThePreviousMatch() => platform.Run(() =>
    {
        using var find = Open("alpha one\nalpha two\nalpha three\n");

        find.Search(MarkdownEditorSearchAction.FindNext, "alpha");
        Assert.Equal((0, 5), find.Selection);
        Assert.Equal((1, 3), (find.Current, find.Total));

        find.Search(MarkdownEditorSearchAction.FindNext, "alpha");
        Assert.Equal((10, 5), find.Selection);
        Assert.Equal(2, find.Current);

        // 停在第 2 处时按「上一个」必须回第 1 处，而不是原地不动。
        find.Search(MarkdownEditorSearchAction.FindPrevious, "alpha");
        Assert.Equal((0, 5), find.Selection);
        Assert.Equal((1, 3), (find.Current, find.Total));
    });

    [Fact]
    public void FindPrevious_WrapsToTheLastMatch_AndFindNextWrapsForward() => platform.Run(() =>
    {
        using var find = Open("alpha one\nalpha two\nalpha three\n");

        find.Search(MarkdownEditorSearchAction.FindPrevious, "alpha");
        Assert.Equal((20, 5), find.Selection);
        Assert.Equal((3, 3), (find.Current, find.Total));

        find.Search(MarkdownEditorSearchAction.FindNext, "alpha");
        Assert.Equal((0, 5), find.Selection);
        Assert.Equal(1, find.Current);
    });

    [Fact]
    public void FindPrevious_HonoursWholeWordAndMatchCase() => platform.Run(() =>
    {
        using var find = Open("alpha Alpha alphabet Alpha\n");

        find.Search(MarkdownEditorSearchAction.FindPrevious, "alpha");
        Assert.Equal((21, 5), find.Selection);
        Assert.Equal((4, 4), (find.Current, find.Total));

        // 整词排除 alphabet 后共 3 处，第 4 处向前应落到第 2 处而不是第 3 处。
        find.Search(MarkdownEditorSearchAction.FindPrevious, "alpha", isWholeWord: true);
        Assert.Equal((6, 5), find.Selection);
        Assert.Equal((2, 3), (find.Current, find.Total));

        // 区分大小写后只剩两处大写；此刻停在首处，向前必然回绕到末处。
        find.Search(MarkdownEditorSearchAction.FindPrevious, "Alpha", isMatchCase: true);
        Assert.Equal((21, 5), find.Selection);
        Assert.Equal((2, 2), (find.Current, find.Total));

        // 再向前一次落到首处大写，小写 alpha 与 alphabet 都不参与。
        find.Search(MarkdownEditorSearchAction.FindPrevious, "Alpha", isMatchCase: true);
        Assert.Equal((6, 5), find.Selection);
        Assert.Equal((1, 2), (find.Current, find.Total));
    });

    [Fact]
    public void FindPrevious_WithoutMatches_ReportsZeroAndKeepsSelection() => platform.Run(() =>
    {
        using var find = Open("alpha one\nalpha two\n");
        find.Search(MarkdownEditorSearchAction.FindNext, "alpha");
        Assert.Equal((0, 5), find.Selection);

        find.Search(MarkdownEditorSearchAction.FindPrevious, "zzz");
        Assert.Equal((0, 0), (find.Current, find.Total));
        Assert.Equal((0, 5), find.Selection);

        // 空查询不参与定位，交给宿主提示先输入文本。
        find.Search(MarkdownEditorSearchAction.FindPrevious, string.Empty);
        Assert.Equal((0, 5), find.Selection);
    });

    [Fact]
    public void FindPrevious_StepsThroughRegexMatchesInReverse() => platform.Run(() =>
    {
        using var find = Open("a1 b22 c333\n");

        find.Search(MarkdownEditorSearchAction.FindPrevious, @"(\d+)", isRegex: true);
        Assert.Equal((8, 3), find.Selection);
        Assert.Equal((3, 3), (find.Current, find.Total));

        find.Search(MarkdownEditorSearchAction.FindPrevious, @"(\d+)", isRegex: true);
        Assert.Equal((4, 2), find.Selection);
        Assert.Equal(2, find.Current);

        find.Search(MarkdownEditorSearchAction.FindPrevious, @"(\d+)", isRegex: true);
        Assert.Equal((1, 1), find.Selection);
        Assert.Equal(1, find.Current);
    });

    [Fact]
    public void SteppingThroughMatches_ReportsNoTextChange() => platform.Run(() =>
    {
        using var find = Open("alpha one\nalpha two\n");

        // 定位只移动选区；回报正文变化会让宿主把一次查找当成编辑（标脏并写进撤销栈）。
        find.Search(MarkdownEditorSearchAction.FindNext, "alpha")
            .Search(MarkdownEditorSearchAction.FindPrevious, "alpha")
            .Search(MarkdownEditorSearchAction.Count, "alpha");
        Assert.Equal(0, find.TextChanges);

        // 替换确实改写正文，必须回报，宿主才能同步文档与历史。
        find.Search(MarkdownEditorSearchAction.ReplaceAll, "alpha", replacement: "beta");
        Assert.Equal(1, find.TextChanges);
    });

    private FindHarness Open(string document)
    {
        var window = new Window { Width = 480, Height = 320 };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
        var editor = new MarkdownEditorView { Text = document };
        window.Content = editor;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var harness = new FindHarness(window, editor);
        editor.SearchResultProduced += (_, args) => harness.Report(args);
        editor.MarkdownChanged += (_, _) => harness.TextChanges++;
        return harness;
    }

    private sealed class FindHarness(Window window, MarkdownEditorView editor) : IDisposable
    {
        public int Current { get; private set; } = -1;

        public int Total { get; private set; } = -1;

        public int TextChanges { get; set; }

        public (int Start, int Length) Selection => editor.Selection;

        public void Report(MarkdownEditorSearchResultEventArgs args)
        {
            Current = args.CurrentIndex;
            Total = args.TotalCount;
        }

        public FindHarness Search(
            MarkdownEditorSearchAction action,
            string searchText,
            string? replacement = null,
            bool isMatchCase = false,
            bool isWholeWord = false,
            bool isRegex = false)
        {
            Current = -1;
            Total = -1;
            editor.Search(new MarkdownEditorSearchCommand(action, searchText, replacement, isMatchCase, isWholeWord, isRegex));
            return this;
        }

        public void Dispose() => window.Close();
    }
}
