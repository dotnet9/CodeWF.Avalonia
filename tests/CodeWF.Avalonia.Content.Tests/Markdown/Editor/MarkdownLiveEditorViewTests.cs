using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.LogicalTree;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

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
    [InlineData(MarkdownEditorAction.Strikethrough, "~~")]
    [InlineData(MarkdownEditorAction.Highlight, "==")]
    [InlineData(MarkdownEditorAction.Superscript, "^")]
    [InlineData(MarkdownEditorAction.Subscript, "~")]
    public void ExtendedInlineActionsPreserveHistory(MarkdownEditorAction action, string marker) => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("文字"); view.FocusBlock(0);
        var box = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single().ActiveEditor!;
        box.SelectionStart = 0; box.SelectionEnd = 2;
        view.ExecuteAsync(action).GetAwaiter().GetResult();
        Assert.Contains(marker + "文字" + marker, view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal("文字", view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Redo).GetAwaiter().GetResult();
        Assert.Contains(marker + "文字" + marker, view.Text);
    });

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
    public void SourceNavigationRevealsDistantBlocksAndTheStartOfTallTables() => _platform.Run(() =>
    {
        var sections = Enumerable.Range(0, 240).Select(i => $"## Chapter {i}\n\nParagraph {i}. " + new string('x', i % 5 * 60));
        var prefix = string.Join("\n\n", sections) + "\n\n";
        var table = "| Target | Value |\n| --- | --- |\n" + string.Join('\n', Enumerable.Range(0, 80).Select(i => $"| Row {i} | Cell {i} |"));
        var source = prefix + table + "\n\n## End\n\nLast paragraph.";
        var view = new MarkdownLiveEditorView { DocumentMaxWidth = 810 };
        view.SetText(source);
        var panel = view.GetLogicalDescendants().OfType<CodeWF.Avalonia.Controls.Controls.WindowedStackPanel>().Single();
        var scroll = view.GetLogicalDescendants().OfType<ScrollViewer>().Single();
        scroll.Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<ScrollViewer>((_, scope) =>
        {
            var presenter = new global::Avalonia.Controls.Presenters.ScrollContentPresenter { Content = scroll.Content };
            scope.Register("PART_ScrollContentPresenter", presenter);
            return presenter;
        });
        var window = new Window { Width = 1000, Height = 600, Content = view };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            // 此夹具没有主题，给普通块保留行高，避免零高预览使整个文档落入同一视口。
            for (var i = 0; i < panel.ItemCount; i++)
            {
                panel.RealizeItem(i);
                panel.GetRealizedItem(i)!.MinHeight = 40;
            }
            window.UpdateLayout();
            Assert.True(scroll.Viewport.Height > 0);
            foreach (var line in new[] { prefix.Count(c => c == '\n') + 1, 1, 701, 101 })
            {
                var index = MarkdownBlockParser.Parse(source).FindLastIndex(model => model.SourceLine <= line);
                view.NavigateToSourceLine(line);
                Dispatcher.UIThread.RunJobs();
                var target = panel.GetRealizedItem(index);
                Assert.NotNull(target);
                var origin = target.TranslatePoint(default, scroll)!.Value;
                Assert.InRange(origin.Y, -.5, scroll.Viewport.Height - 1);
                Assert.True(panel.RealizedItemCount < 100, $"line={line}, realized={panel.RealizedItemCount}, offset={scroll.Offset.Y}, viewport={scroll.Viewport}, extent={scroll.Extent}");
                Assert.Equal(-1, view.ActiveBlockIndex);
                Assert.Equal(source, view.Text);
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void EditingKeepsRichPreviewAndEscapeRestoresOriginalBlock() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView { TypographyTheme = "Simple" };
        view.SetText("# 标题\r\n\r\n原文\r\n");
        var original = view.Text;
        view.FocusBlock(1);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        block.ActiveEditor!.Text = "**实时内容**";
        Dispatcher.UIThread.RunJobs();
        Assert.Single(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        Assert.Contains("**实时内容**", view.Text);
        Assert.Single(view.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        block.ActiveEditor.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(original, view.Text);
        Assert.False(block.IsEditing);
    });

    [Fact]
    public void InlineEditingKeepsThePreviewAndUpdatesItsGeometryWhileTyping() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView { TypographyTheme = "OrangeHeart", TypographySize = "Small" };
        view.SetText("original **content**", "C:/documents/first.md");
        view.FocusBlock(0);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single();
        var readOnlyPreview = block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single();

        view.FocusBlock(0);
        Assert.Same(readOnlyPreview, block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single());
        Assert.Equal("original **content**", block.ActiveEditor!.Text);
        block.ActiveEditor!.Text = "updated *content*";
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("updated *content*", view.Text);
        Assert.Contains("updated *content*", readOnlyPreview.Markdown);

        block.CommitAndRender();
        Assert.Same(readOnlyPreview, block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single());
        Assert.Contains("updated *content*", readOnlyPreview.Markdown);
        Assert.Equal("C:/documents/first.md", readOnlyPreview.ImageBasePath);
        Assert.Equal("OrangeHeart", readOnlyPreview.TypographyTheme);
        Assert.Equal("Small", readOnlyPreview.TypographySize);
        view.FocusBlock(0);
        Assert.Equal("updated *content*", block.ActiveEditor!.Text);
        Assert.Same(readOnlyPreview, block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single());
    });

    [Fact]
    public void DocumentAndImageBasePathLoadTogetherAndStandalonePathChangePreservesSelection() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        var changed = 0;
        view.MarkdownChanged += (_, _) => changed++;
        const string source = "# Heading\r\n\r\n![Local](image.png)\r\n";
        view.SetText(source, "C:/documents/first.md");
        Assert.Equal(source, view.Text);
        Assert.Equal("C:/documents/first.md", view.ImageBasePath);
        Assert.All(view.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>(),
            preview => Assert.Equal(view.ImageBasePath, preview.ImageBasePath));
        view.FocusBlock(1);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        block.ActiveEditor!.SelectionStart = 2;
        block.ActiveEditor.SelectionEnd = 5;

        view.ImageBasePath = "C:/other/second.md";
        block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Last();
        Assert.True(block.IsEditing);
        Assert.Equal(2, block.ActiveEditor!.SelectionStart);
        Assert.Equal(5, block.ActiveEditor.SelectionEnd);
        Assert.Equal(source, view.Text);
        Assert.All(view.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>(),
            preview => Assert.Equal("C:/other/second.md", preview.ImageBasePath));

        view.SetText("replacement", null);
        Assert.Equal("replacement", view.Text);
        Assert.Null(view.ImageBasePath);
        Assert.Equal(-1, view.ActiveBlockIndex);
        Assert.Equal(0, changed);
    });

    [Fact]
    public void PlainTableCellCreatesRichPreviewOnlyWhenItsContentNeedsIt() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView();
        view.SetText("| A | B |\n| --- | --- |\n| plain | cell |");
        view.FocusBlock(0);
        var table = view.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
        var block = table.GetLogicalDescendants().OfType<MarkdownBlockView>().ElementAt(2);
        Assert.Empty(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        block.BeginEditAtEnd();
        Assert.Empty(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        block.ActiveEditor!.Text = "$x$";
        Dispatcher.UIThread.RunJobs();
        Assert.Single(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        block.CommitAndRender();
        var preview = block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single();
        Assert.True(preview.IsVisible);
        Assert.Contains("$x$", preview.Markdown);
        block.BeginEditAtEnd();
        block.ActiveEditor!.Text = "plain again";
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        block.CommitAndRender();
        Assert.Empty(block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>());
        Assert.Contains("plain again", view.Text);
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
        view.FocusBlock(1);
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
    public void ParagraphBoundaryKeysNavigateAndMergeWithoutLosingMarkdown() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("first **bold**\n\nsecond");
        view.FocusBlock(0);
        var first = ActiveBox(view);
        first.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Right });
        Assert.Equal(1, view.ActiveBlockIndex); Assert.Equal(0, ActiveBox(view).CaretIndex);
        ActiveBox(view).RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
        Assert.Equal(0, view.ActiveBlockIndex); Assert.Equal(first.Text!.Length, ActiveBox(view).CaretIndex);
        ActiveBox(view).RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Delete });
        Assert.Equal(1, view.BlockCount); Assert.Equal("first **bold**second", view.Text);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal("first **bold**\n\nsecond", view.Text);
    });

    [Fact]
    public void BackspaceAtAHeadingStartRemovesFormattingBeforeJoiningParagraphs() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("before\n\n## title"); view.FocusBlock(1);
        var box = ActiveBox(view); box.CaretIndex = box.SelectionStart = box.SelectionEnd = 0;
        box.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Back });
        Assert.Equal("before\n\ntitle", view.Text); Assert.Equal(2, view.BlockCount);
        ActiveBox(view).RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Back });
        Assert.Equal("beforetitle", view.Text); Assert.Equal(6, ActiveBox(view).CaretIndex);
    });

    [Fact]
    public void FormattingToolbarUsesTheCurrentlyEditedTableCell() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("| A | B |\n| --- | --- |\n| one | two |"); view.FocusBlock(0);
        var table = view.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
        var cell = table.GetLogicalDescendants().OfType<MarkdownBlockView>().Last(); cell.BeginEditAtEnd();
        Assert.Single(table.GetLogicalDescendants().OfType<MarkdownBlockView>(), item => item.IsEditing);
        cell.ActiveEditor!.SelectAll();
        cell.CommitAndRender();
        view.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("**two**", view.Text); Assert.DoesNotContain("**one**", view.Text);
    });

    [Fact]
    public void MeasuredBlocksKeepTheFirstScreenAndTheScrolledAnchorStable() => _platform.Run(() =>
    {
        var panel = new CodeWF.Avalonia.Controls.Controls.WindowedStackPanel { EnableVirtualization = false };
        var items = Enumerable.Range(0, 30).Select(_ => new Border { Height = 40 }).ToArray();
        foreach (var item in items) panel.AddItem(item);
        var presenter = new global::Avalonia.Controls.Presenters.ScrollContentPresenter { Content = panel };
        var scroll = new ScrollViewer
        {
            Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<ScrollViewer>((_, scope) =>
            {
                scope.Register("PART_ScrollContentPresenter", presenter);
                return presenter;
            })
        };
        panel.ScrollHost = scroll;
        var window = new Window { Width = 800, Height = 500, Content = scroll };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            items[5].Height = 120;
            window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, scroll.Offset.Y, 1);
            scroll.Offset = new global::Avalonia.Vector(0, 400);
            window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            items[0].Height = 80;
            window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            Assert.Equal(440, scroll.Offset.Y, 1);
        }
        finally { window.Close(); }
    });

    private static TextBox ActiveBox(MarkdownLiveEditorView view) =>
        view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single(block => block.IsEditing).ActiveEditor!;

    [Fact]
    public void CodeLanguageAppearsBelowOnlyDuringEditingAndEscapeRestoresIt() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("```csharp\nvar value = 1;\n```"); view.FocusBlock(0);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single();
        var language = block.GetLogicalDescendants().OfType<TextBox>().Single(input => input.Classes.Contains("MdCodeLanguageInput"));
        var preview = block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single();
        Assert.False(preview.ShowCodeBlockHeader); Assert.Equal("csharp", language.Text);
        language.Text = "javascript"; Dispatcher.UIThread.RunJobs();
        Assert.StartsWith("```javascript", view.Text);
        language.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
        Assert.StartsWith("```csharp", view.Text); Assert.False(block.IsEditing);
        Assert.False(language.IsEffectivelyVisible);
    });

    [Fact]
    public async Task Platform_RunFromDifferentWorkersUsesOneUiThread()
    {
        var firstThread = _platform.Run(() => Environment.CurrentManagedThreadId);
        var secondThread = await Task.Run(() => _platform.Run(() => Environment.CurrentManagedThreadId));
        Assert.Equal(firstThread, secondThread);
    }

    [Fact]
    public void SelectionSurvivesCommittingForAMenuOrDialog() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView(); view.SetText("# Heading\n\nA paragraph here."); view.FocusBlock(1);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single(item => item.IsEditing);
        block.ActiveEditor!.SelectionStart = 2; block.ActiveEditor.SelectionEnd = 11;
        var selection = view.Selection;
        block.CommitAndRender();
        Assert.Equal(selection, view.Selection);
        var saved = view.CaptureState(); view.RestoreState(saved);
        Assert.Equal(selection, view.Selection);
    });

    [Fact]
    public void FontPreferencesPreserveCodeEditingAndUndo() => _platform.Run(() =>
    {
        var view = new MarkdownLiveEditorView { EditorFontSize = 18, CodeFontSize = 16 };
        view.SetText("```text\nhello\n```"); view.FocusBlock(0);
        view.InsertText("x");
        var text = view.Text;
        view.CodeFontSize = 19;
        Assert.Equal(text, view.Text);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single();
        Assert.Equal(19, block.ActiveEditor!.FontSize);
        Assert.Equal(19, block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single().CodeBlockFontSize);
        view.ExecuteAsync(MarkdownEditorAction.Undo).GetAwaiter().GetResult();
        Assert.Equal("```text\nhello\n```", view.Text.TrimEnd());
    });

    [Fact]
    public void HostCodeFontIsSharedByPreviewAndInlineEditing() => _platform.Run(() =>
    {
        var font = new global::Avalonia.Media.FontFamily("Arial");
        var view = new MarkdownLiveEditorView { CodeFontFamily = font };
        view.SetText("```text\nhello\n```"); view.FocusBlock(0);
        var block = view.GetLogicalDescendants().OfType<MarkdownBlockView>().Single();
        var preview = block.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single();
        Assert.Equal(font, preview.CodeFontFamily);
        Assert.Equal(font, block.ActiveEditor!.FontFamily);
        view.CodeFontFamily = new global::Avalonia.Media.FontFamily("Consolas");
        Assert.Equal("Consolas", view.GetLogicalDescendants().OfType<CodeWF.Avalonia.Markdown.Controls.MarkdownViewer>().Single().CodeFontFamily.Name);
    });
}
