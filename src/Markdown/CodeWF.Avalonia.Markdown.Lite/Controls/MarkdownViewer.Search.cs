using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Media;
using global::Avalonia.Media.Immutable;
using global::Avalonia.VisualTree;

namespace CodeWF.Avalonia.Markdown.Controls;

/// <summary>
/// MarkdownViewer 的文档内查找部分：Find/FindNext/FindPrevious/ClearSearch、
/// 命中计数与就地高亮。对照宿主集成 API 面清单（源码视图用自己的编辑器查找）。
///
/// v1 语义：命中扫描覆盖已渲染块的 Inlines 文本（段落/标题/列表/引用/表格等），
/// 代码块正文（Text 模式自绘高亮）暂不参与；跨行内元素边界（如跨粗体边界）的
/// 匹配会计入定位但不高亮。任何重渲染后自动重新扫描并重放高亮。
/// </summary>
public partial class MarkdownViewer
{
    /// <summary>搜索命中高亮画刷（默认半透明琥珀，宿主可按主题覆盖）。</summary>
    public static readonly StyledProperty<IBrush?> SearchHighlightBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(
            nameof(SearchHighlightBrush),
            new ImmutableSolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xD5, 0x4F)));

    /// <summary>当前命中画刷（较深，FindNext 循环时随命中移动）。</summary>
    public static readonly StyledProperty<IBrush?> SearchCurrentBrushProperty =
        AvaloniaProperty.Register<MarkdownViewer, IBrush?>(
            nameof(SearchCurrentBrush),
            new ImmutableSolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0x98, 0x00)));

    public IBrush? SearchHighlightBrush
    {
        get => GetValue(SearchHighlightBrushProperty);
        set => SetValue(SearchHighlightBrushProperty, value);
    }

    public IBrush? SearchCurrentBrush
    {
        get => GetValue(SearchCurrentBrushProperty);
        set => SetValue(SearchCurrentBrushProperty, value);
    }

    /// <summary>搜索状态变化（新搜索、命中移动、清除）时触发。</summary>
    public event EventHandler? SearchChanged;

    private string? _searchText;
    private bool _searchCaseSensitive;
    private bool _searchWholeWord;
    private readonly List<SearchMatch> _searchMatches = [];
    private int _searchCurrentIndex = -1;
    private readonly List<(SelectableTextBlock TextBlock, Run Source, string OriginalText, List<Run> Inserted)> _searchModified = [];

    /// <summary>当前搜索的命中总数（未搜索或无命中为 0）。</summary>
    public int MatchCount => _searchMatches.Count;

    /// <summary>当前命中序号（0 起始；无命中为 -1）。</summary>
    public int CurrentMatchIndex => _searchCurrentIndex;

    /// <summary>
    /// 在当前已渲染文档中查找：<paramref name="searchText"/> 为空白时等同
    /// <see cref="ClearSearch"/>。返回命中总数。
    /// </summary>
    public int Find(string searchText, bool caseSensitive = false, bool wholeWord = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ClearSearch();
            return 0;
        }

        _searchText = searchText;
        _searchCaseSensitive = caseSensitive;
        _searchWholeWord = wholeWord;
        ReapplySearch();
        SearchChanged?.Invoke(this, EventArgs.Empty);
        return _searchMatches.Count;
    }

    /// <summary>跳到下一个命中（循环）并滚动定位；无命中返回 false。</summary>
    public bool FindNext() => MoveSearchMatch(1);

    /// <summary>跳到上一个命中（循环）并滚动定位；无命中返回 false。</summary>
    public bool FindPrevious() => MoveSearchMatch(-1);

    /// <summary>清除搜索：还原被高亮拆分的文本，重置命中状态。</summary>
    public void ClearSearch()
    {
        if (_searchText is null && _searchMatches.Count == 0 && _searchModified.Count == 0)
        {
            return;
        }

        RestoreSearchInlines();
        _searchText = null;
        _searchMatches.Clear();
        _searchCurrentIndex = -1;
        SearchChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>渲染完成后调用：搜索激活时重新扫描并重放高亮（命中位置按新内容重算）。</summary>
    private void ReapplySearchIfActive()
    {
        if (_searchText is not null)
        {
            ReapplySearch();
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool MoveSearchMatch(int direction)
    {
        if (_searchMatches.Count == 0)
        {
            return false;
        }

        _searchCurrentIndex = (_searchCurrentIndex + direction + _searchMatches.Count) % _searchMatches.Count;
        ApplyCurrentMatchBrush();
        var match = _searchMatches[_searchCurrentIndex];
        ScrollToBlock(match.BlockIndex);
        SearchChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// 重扫已渲染块的 Inlines 文本并应用高亮：先还原上一轮拆分，再对每个
    /// Run 内的完整匹配做三段拆分（中段上色）。命中按阅读序（块序 × Run 序 ×
    /// Run 内位置）排序，跨行内元素边界的匹配跳过高亮。
    /// </summary>
    private void ReapplySearch()
    {
        RestoreSearchInlines();
        _searchMatches.Clear();
        _searchCurrentIndex = -1;
        if (_searchText is null || _documentHost is null)
        {
            return;
        }

        var comparison = _searchCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var highlight = SearchHighlightBrush;
        for (var blockIndex = 0; blockIndex < _renderedBlocks.Count; blockIndex++)
        {
            // 块根控件本身可能就是文本块（段落直接以 SelectableTextBlock 渲染）。
            var root = _renderedBlocks[blockIndex].Control;
            var textBlocks = root is SelectableTextBlock rootText
                ? new[] { rootText }.Concat(root.GetVisualDescendants().OfType<SelectableTextBlock>())
                : root.GetVisualDescendants().OfType<SelectableTextBlock>();
            foreach (var textBlock in textBlocks.Where(block => block.Tag as string != "CodeLineNumbers"))
            {
                CollectMatchesInTextBlock(blockIndex, textBlock, comparison, highlight);
            }
        }
    }

    private void CollectMatchesInTextBlock(int blockIndex, SelectableTextBlock textBlock, StringComparison comparison, IBrush? highlight)
    {
        var inlines = textBlock.Inlines;
        if (inlines is null || inlines.Count == 0)
        {
            return;
        }

        var modified = false;
        // 就地收集拆分计划后统一改集合，避免遍历时修改。
        var insertions = new List<(int Index, Run Source, string OriginalText, List<Run> Inserted)>();
        for (var i = 0; i < inlines.Count; i++)
        {
            if (inlines[i] is not Run { Text: { } text } run || text.Length == 0)
            {
                continue;
            }

            var offsets = FindAllMatchOffsets(text, comparison);
            if (offsets.Count == 0)
            {
                continue;
            }

            var (originalText, afterRuns, matchRuns) = SplitRunWithHighlights(run, text, offsets, highlight);
            insertions.Add((i, run, originalText, afterRuns));
            modified = true;
            foreach (var matchRun in matchRuns)
            {
                _searchMatches.Add(new SearchMatch(blockIndex, matchRun));
            }
        }

        if (!modified)
        {
            return;
        }

        // 从后往前替换，保持前面的索引有效。源 Run 留在原位（文本截为首个前缀）。
        for (var i = insertions.Count - 1; i >= 0; i--)
        {
            var (index, source, originalText, inserted) = insertions[i];
            source.Text = originalText[..source.Text!.Length];
            for (var r = inserted.Count - 1; r >= 0; r--)
            {
                inlines.Insert(index + 1, inserted[r]);
            }

            _searchModified.Add((textBlock, source, originalText, inserted));
        }
    }

    /// <summary>
    /// 把命中位置拆成 前/命中(高亮)/后 段：返回（完整原文、源 Run 之后的追加段、
    /// 命中段列表）。源 Run 保留在原位只承载首个前缀，还原时删追加段并复位文本。
    /// </summary>
    private (string OriginalText, List<Run> AfterRuns, List<Run> MatchRuns) SplitRunWithHighlights(
        Run source, string text, IReadOnlyList<int> offsets, IBrush? highlight)
    {
        var originalText = text;
        var afterRuns = new List<Run>();
        var matchRuns = new List<Run>();
        var cursor = 0;
        // 从最后一个命中往前拆：把 source.Text 截短为当前前缀，其后的段落生成新 Run 前插在 source 之后。
        for (var i = offsets.Count - 1; i >= 0; i--)
        {
            var offset = offsets[i];
            var matchEnd = offset + _searchText!.Length;
            if (matchEnd < text.Length)
            {
                afterRuns.Insert(0, new Run(text[matchEnd..]));
                text = text[..matchEnd];
            }

            var matchRun = new Run(text[offset..matchEnd]) { Background = highlight };
            matchRuns.Insert(0, matchRun);
            afterRuns.Insert(0, matchRun);
            if (offset > 0)
            {
                afterRuns.Insert(0, new Run(text[..offset]));
                text = text[..offset];
            }
        }

        source.Text = text;
        return (originalText, afterRuns, matchRuns);
    }

    private IReadOnlyList<int> FindAllMatchOffsets(string text, StringComparison comparison)
    {
        var offsets = new List<int>();
        var needle = _searchText!;
        var start = 0;
        while ((start = text.IndexOf(needle, start, comparison)) >= 0)
        {
            if (!_searchWholeWord || IsWholeWordBoundary(text, start, needle.Length))
            {
                offsets.Add(start);
            }

            start += needle.Length;
        }

        return offsets;
    }

    private static bool IsWholeWordBoundary(string text, int start, int length)
    {
        var beforeOk = start == 0 || !char.IsLetterOrDigit(text[start - 1]);
        var after = start + length;
        var afterOk = after >= text.Length || !char.IsLetterOrDigit(text[after]);
        return beforeOk && afterOk;
    }

    private void ApplyCurrentMatchBrush()
    {
        for (var i = 0; i < _searchMatches.Count; i++)
        {
            _searchMatches[i].Run.Background = i == _searchCurrentIndex
                ? SearchCurrentBrush
                : SearchHighlightBrush;
        }
    }

    private void RestoreSearchInlines()
    {
        foreach (var (textBlock, source, originalText, inserted) in _searchModified)
        {
            var inlines = textBlock.Inlines;
            if (inlines is not null)
            {
                foreach (var run in inserted)
                {
                    inlines.Remove(run);
                }
            }

            source.Text = originalText;
        }

        _searchModified.Clear();
    }

    private sealed record SearchMatch(int BlockIndex, Run Run);
}
