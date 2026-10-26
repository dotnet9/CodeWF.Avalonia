using global::Avalonia.Controls;
using global::Avalonia.Media;

namespace CodeWF.Avalonia.Markdown.Rendering;

/// <summary>
/// 代码高亮能力委托：输入代码与排版参数，返回高亮内容控件；
/// 返回 null 表示无法高亮（调用方应使用 Core 的单色等宽降级渲染）。
/// <paramref name="session"/> 为宿主提供的行级缓存会话（可 null）：流式增长场景
/// 下未变行复用缓存 token，只重算新增/变更行。
/// </summary>
public delegate Control? MarkdownCodeHighlighter(
    string code,
    string language,
    bool isDark,
    FontFamily fontFamily,
    double fontSize,
    double lineHeight,
    MarkdownCodeHighlightSession? session,
    Func<bool>? hasGlobalSelection,
    Func<Task>? copySelectionAsync);
