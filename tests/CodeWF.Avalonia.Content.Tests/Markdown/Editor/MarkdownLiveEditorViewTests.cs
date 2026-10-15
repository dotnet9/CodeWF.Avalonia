using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.LogicalTree;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

/// <summary>
/// 单栏实时编辑（所见即所得）视图的行为测试（需要 Avalonia 平台）。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownLiveEditorViewTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownLiveEditorViewTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Theory]
    [InlineData("(", "()")]
    [InlineData("[", "[]")]
    [InlineData("\"", "\"\"")]
    public void LiveInputPairsWrapsAndSkipsClosingCharacters(string input, string pair) => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("文字"); view.FocusBlock(0);
        var box = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single().ActiveEditor!;
        box.SelectionStart = 2; box.SelectionEnd = 0;
        var opening = new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = input };
        box.RaiseEvent(opening);
        Assert.True(opening.Handled);
        Assert.Equal(pair[0] + "文字" + pair[1], box.Text);
        box.CaretIndex = box.SelectionStart = box.SelectionEnd = box.Text!.Length - 1;
        var closing = new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = pair[1].ToString() };
        box.RaiseEvent(closing);
        Assert.True(closing.Handled); Assert.Equal(4, box.CaretIndex);
        view.EnableAutoPair = false;
        var disabled = new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = input };
        box.RaiseEvent(disabled); Assert.Equal(pair[0] + "文字" + pair[1] + input, box.Text);
        view.EnableAutoPair = true;
        var unicode = new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = "中文👩‍💻" };
        box.RaiseEvent(unicode); Assert.EndsWith(input + "中文👩‍💻", box.Text);
    });

    [Fact]
    public void TypographyChangesKeepSelectionAndDoNotModifyMarkdown() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("# 标题\n\n正文内容"); view.FocusBlock(1);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        block.ActiveEditor!.SelectionStart = 0; block.ActiveEditor.SelectionEnd = 2;
        var original = view.Text;
        view.TypographyTheme = "OrangeHeart";
        var current = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        Assert.True(current.IsEditing); Assert.Equal(0, current.ActiveEditor!.SelectionStart); Assert.Equal(2, current.ActiveEditor.SelectionEnd);
        Assert.Equal(original, view.Text);
    });

    [Fact]
    public void SourceNavigationScrollsWithoutEditingAndUsesCurrentLineNumbers() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView { DocumentMaxWidth = 810, DocumentPadding = new global::Avalonia.Thickness(44, 34), DocumentBlockSpacing = 6 };
        const string source = "# 标题\r\n\r\n正文\r\n\r\n## 目标\r\n\r\n更多正文";
        view.SetText(source);
        var window = new Window { Width = 1200, Height = 700, Content = view };
        window.Show();
        try
        {
            view.NavigateToSourceLine(5);
            Assert.Equal(-1, view.ActiveBlockIndex);
            Assert.Equal(source, view.Text);
            view.NavigateToSourceLine(5, beginEdit: true);
            Assert.Equal(2, view.ActiveBlockIndex);
            Assert.Equal(source, view.Text);
            Assert.Equal(5, MarkdownBlockParser.Parse(source)[2].SourceLine);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void EditingRendersImmediatelyAndEscapeRestoresOriginalBlock() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView { TypographyTheme = "Simple" };
        view.SetText("# 标题\r\n\r\n原文\r\n");
        var original = view.Text;
        view.FocusBlock(1);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        block.ActiveEditor!.Text = "**实时内容**";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>(),
            preview => preview.Markdown?.Contains("**实时内容**", StringComparison.Ordinal) == true);
        block.ActiveEditor.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(original, view.Text);
        Assert.False(block.IsEditing);
    });

    [Fact]
    public void UndoRedoAndTemporaryDocumentPreserveCrossBlockHistory() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("# 标题\n\n正文");
        var original = view.Text;
        view.FocusBlock(0); view.InsertText("修改一"); Dispatcher.UIThread.RunJobs();
        var first = view.Text;
        view.FocusBlock(1); view.InsertText("修改二"); Dispatcher.UIThread.RunJobs();
        var second = view.Text;
        var saved = view.CaptureState();
        view.SetText("引导临时文档"); view.InsertText("测试");
        view.RestoreState(saved);
        Assert.Equal(second, view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal(first, view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal(original, view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Redo).GetAwaiter().GetResult();
        Assert.Equal(first, view.Text);
    });

    [Fact]
    public void MenuActionsKeepSelectionAfterTheBlockLosesFocus() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("正文内容"); view.FocusBlock(0);
        var block = Assert.Single(view.GetLogicalDescendants().OfType<MarkdownBlockView>());
        block.ActiveEditor!.SelectionStart = 0; block.ActiveEditor.SelectionEnd = 2;
        block.CommitAndRender();
        view.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("**正文**内容", view.Text);
        view.ExecuteAsync(MarkdownEditorAction.OrderedList).GetAwaiter().GetResult();
        Assert.StartsWith("1. ", view.Text);
    });

    [Fact]
    public void MultiParagraphQuote_SharesContainerAndKeepsBlockNavigation() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        var markdown = "> first\n>\n> second\n\n# following";
        view.SetText(markdown);
        Assert.Equal(markdown, view.Text);
        Assert.Equal(3, view.BlockCount);
        var quote = Assert.Single(view.GetLogicalDescendants().OfType<Border>(),
            border => border.Classes.Contains("MdLiveQuote"));
        Assert.Equal(2, Assert.IsType<StackPanel>(quote.Child).Children.Count);
        view.FocusBlock(1);
        Assert.Equal(1, view.ActiveBlockIndex);
        view.InsertText("!");
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("> second!", view.Text);
        Assert.Contains("> first", view.Text);
        Assert.Contains("# following", view.Text);
    });

    [Fact]
    public void SetText_DoesNotRaiseMarkdownChanged() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        var raised = 0;
        view.MarkdownChanged += (_, _) => raised++;

        view.SetText("# Title\n\npara");

        Assert.Equal(0, raised);
        Assert.Equal(2, view.BlockCount);
    });

    [Fact]
    public void Text_RoundTripsIntoBlockModels() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("- [ ] a\n\n# b");

        var text = view.Text;

        Assert.Contains("- [ ] a", text, StringComparison.Ordinal);
        Assert.Contains("# b", text, StringComparison.Ordinal);
    });

    [Fact]
    public void ApplyExternalEdit_SwapsDocumentWithoutRaising() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("first");
        var raised = 0;
        view.MarkdownChanged += (_, _) => raised++;

        view.ApplyExternalEdit("second", 0, 5);

        Assert.Equal(0, raised);
        Assert.Contains("second", view.Text, StringComparison.Ordinal);
    });

    [Fact]
    public void InsertText_RaisesMarkdownChanged() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("start");
        var payloads = new List<string>();
        view.MarkdownChanged += (_, text) => payloads.Add(text);

        view.InsertText("- appended");

        Assert.NotEmpty(payloads);
        Assert.Contains("- appended", payloads[^1], StringComparison.Ordinal);
    });

    [Fact]
    public void TableAndTaskBlocks_ExposeStructure() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("| a |\n| --- |\n| 1 |\n\n- [x] done");

        var text = view.Text;
        Assert.Equal(2, view.BlockCount);
        Assert.True(text.Contains("| a |", StringComparison.Ordinal), $"actual: {text}");
        Assert.True(text.Contains("- [x] done", StringComparison.Ordinal), $"actual: {text}");
    });

    [Theory]
    [InlineData("# title")]
    [InlineData("> quote")]
    [InlineData("```csharp\ncode\n```")]
    public void FocusBlock_DecoratedBlockCanBeEdited(string markdown) => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText(markdown);
        view.FocusBlock(0);
        var block = Assert.Single(view.GetLogicalDescendants().OfType<MarkdownBlockView>());
        Assert.True(block.IsEditing);
        block.ActiveEditor!.Text += " changed";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("changed", view.Text);
    });

    [Fact]
    public void InsertText_ReplacesSelectionAtCaretWithoutRebuilding() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("# abcdef");
        view.FocusBlock(0);
        var block = Assert.Single(view.GetLogicalDescendants().OfType<MarkdownBlockView>());
        var editor = block.ActiveEditor!;
        editor.SelectionStart = 2;
        editor.SelectionEnd = 4;
        view.InsertText("中文");
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("ab中文ef", editor.Text);
        Assert.Equal("# ab中文ef", view.Text);
        Assert.Same(editor, block.ActiveEditor);
    });

    [Fact]
    public void Enter_ContinuesOrderedListWithoutDuplicateNumber() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("1. a\n2. b\n3. c");
        view.FocusBlock(0);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().First();
        block.ActiveEditor!.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter });
        Dispatcher.UIThread.RunJobs();
        var models = MarkdownBlockParser.Parse(view.Text);
        Assert.Equal(new[] { 1, 2, 3, 4 }, models.Select(model => model.OrderedNumber));
        Assert.Contains("3. b", view.Text);
        Assert.Contains("4. c", view.Text);
    });

    [Fact]
    public void Typing_ImmediatelyPublishesAndTaskTogglePersists() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("- [ ] a\n- [ ] b");
        var payloads = new List<string>();
        view.MarkdownChanged += (_, text) => payloads.Add(text);
        view.FocusBlock(0);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().First();
        block.ActiveEditor!.Text = "edited";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("edited", payloads[^1]);
        block.CommitAndRender();
        view.GetLogicalDescendants().OfType<CheckBox>().First().IsChecked = true;
        var models = MarkdownBlockParser.Parse(view.Text);
        Assert.True(models[0].IsChecked);
        Assert.Equal("edited", models[0].Text);
        Assert.False(models[1].IsChecked);
    });

    [Fact]
    public void ShiftEnter_InsertsHardBreakInSameBlock() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("first");
        view.FocusBlock(0);
        var editor = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single().ActiveEditor!;
        editor.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter, KeyModifiers = KeyModifiers.Shift });
        editor.Text += "second";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, view.BlockCount);
        Assert.Equal("first  \nsecond", view.Text);
    });

    [Fact]
    public void Table_CellEditsNavigationAndMenuActionsReachDocument() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("before\n\n| a | b |\n| :--- | ---: |\n| 1 | 2 |\n\nafter");
        var changed = 0;
        view.MarkdownChanged += (_, _) => changed++;
        var table = view.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
        var cells = table.GetLogicalDescendants().OfType<MarkdownBlockView>().ToArray();
        cells[2].BeginEditAtEnd();
        cells[2].ActiveEditor!.Text = "**中文**";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("| **中文** | 2 |", view.Text);
        cells[2].ActiveEditor!.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Tab });
        Assert.False(cells[2].IsEditing);
        Assert.True(cells[3].IsEditing);

        ((MenuItem)table.ContextMenu!.Items[0]!).RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
        ((MenuItem)table.ContextMenu.Items[2]!).RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
        var model = MarkdownBlockParser.Parse(view.Text).Single(block => block.Kind == MarkdownBlockKind.Table);
        Assert.Equal(3, model.Cells.Count);
        Assert.All(model.Cells, row => Assert.Equal(3, row.Count));
        Assert.Equal(TableCellAlignment.Left, model.Alignments[0]);
        Assert.Equal(TableCellAlignment.Right, model.Alignments[1]);
        Assert.StartsWith("before\n\n", view.Text);
        Assert.EndsWith("\n\nafter", view.Text);
        Assert.Equal(3, changed);
    });

    [Fact]
    public async Task Platform_RunFromDifferentWorkersUsesOneUiThread()
    {
        var firstThread = _platform.Run(() => Environment.CurrentManagedThreadId);
        var secondThread = await Task.Run(() => _platform.Run(() => Environment.CurrentManagedThreadId));
        Assert.Equal(firstThread, secondThread);
    }
}
