using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Media;
using global::Avalonia.Media.Immutable;
using global::Avalonia.Media.TextFormatting;
using global::Avalonia.Utilities;
using global::Avalonia.VisualTree;
using CodeWF.Toolkit.Lang;
using System.Globalization;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using AvaloniaFontStyle = global::Avalonia.Media.FontStyle;
using TextMateFontStyle = TextMateSharp.Themes.FontStyle;

namespace CodeWF.Avalonia.Markdown.Rendering;

internal static class CodeHighlighter
{
    private const int MaxCacheSize = 32;
    private static readonly Dictionary<(string Language, ThemeName Theme), (IGrammar? Grammar, Theme Theme)> Cache = new();
    private static readonly Queue<(string Language, ThemeName Theme)> CacheOrder = new();
    private static readonly Dictionary<HighlightCacheKey, HighlightedLine[]> HighlightCache = new();
    private static readonly Queue<HighlightCacheKey> HighlightCacheOrder = new();

    public static Control Render(
        string code,
        string language,
        bool isDark,
        FontFamily fontFamily,
        double fontSize,
        double lineHeight,
        Func<bool>? hasGlobalSelection = null,
        Func<Task>? copyGlobalSelectionAsync = null)
    {
        var themeName = isDark ? ThemeName.DarkPlus : ThemeName.LightPlus;
        var (grammar, theme) = GetOrCreateGrammar(NormalizeLanguage(language), themeName);
        var lines = grammar is null ? null : GetOrCreateHighlightedLines(code, NormalizeLanguage(language), themeName, grammar, theme, isDark);
        var normalized = code.Replace("\r\n", "\n");

        // 长代码以文本样式区间排版，避免为每个 token 建立参与主题匹配的逻辑树节点。
        SelectableTextBlock textBlock = lines is { Length: >= 64 }
            ? new HighlightedCodeTextBlock(normalized, lines)
            : new SelectableTextBlock();
        textBlock.TextWrapping = TextWrapping.NoWrap;
        textBlock.FontFamily = fontFamily;
        textBlock.FontSize = fontSize;
        textBlock.LineHeight = lineHeight;
            // 选区使用不透明底色，避免与浅色代码背景合成后降低白字对比度。
        textBlock.SelectionBrush = new ImmutableSolidColorBrush(Color.FromRgb(0x24, 0x5F, 0xC0));
        textBlock.SelectionForegroundBrush = Brushes.White;
        textBlock.Classes.Add(MarkdownStyleKeys.CodeBlockText);
        TextOptions.SetBaselinePixelAlignment(textBlock, BaselinePixelAlignment.Aligned);
        textBlock.ContextMenu = CreateCopyContextMenu(textBlock, hasGlobalSelection, copyGlobalSelectionAsync);
        var lineCount = normalized.Count(character => character == '\n') + 1;
        if (textBlock is HighlightedCodeTextBlock) return Wrap(textBlock, isDark, lineCount);
        // SelectableTextBlock 每次收到 Inlines 变化都会重新拼接全文来校正选区。
        // 先在未绑定的集合内完成构造，避免长代码的 token 数量带来平方级复制。
        var inlines = new InlineCollection();

        if (grammar is null)
        {
            inlines.Add(new Run(code) { Foreground = isDark ? Brushes.White : Brushes.Black });
            textBlock.Inlines = inlines;
            return Wrap(textBlock, isDark, lineCount);
        }

        for (var index = 0; index < lines!.Length; index++)
        {
            if (index > 0) inlines.Add(new LineBreak());
            foreach (var token in lines[index].Tokens)
            {
                inlines.Add(token.CreateRun());
            }

        }

        textBlock.Inlines = inlines;
        return Wrap(textBlock, isDark, lineCount);
    }

    private static Control Wrap(Control content, bool isDark, int lineCount)
    {
        if (content is SelectableTextBlock textBlock)
        {
            var lineNumbers = new SelectableTextBlock
            {
                Text = string.Join(Environment.NewLine, Enumerable.Range(1, lineCount).Select(i => i.ToString())),
                TextWrapping = TextWrapping.NoWrap,
                FontFamily = textBlock.FontFamily,
                FontSize = textBlock.FontSize,
                LineHeight = textBlock.LineHeight,
                // 依据实际代码背景选择行号颜色，不随窗口明暗误判，也不额外衰减透明度。
                Foreground = new ImmutableSolidColorBrush(isDark ? Color.FromRgb(0xAD, 0xB7, 0xC5) : Color.FromRgb(0x52, 0x60, 0x75)),
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 12, 0),
                IsHitTestVisible = false
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                }
            };
            grid.Children.Add(lineNumbers);
            Grid.SetColumn(content, 1);
            grid.Children.Add(content);
            content = grid;
        }

        var scrollViewer = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        scrollViewer.Classes.Add(MarkdownStyleKeys.CodeBlockScrollViewer);
        scrollViewer.ScrollChanged += (_, _) =>
        {
            // Semi 的横向滚动条覆盖内容；溢出时预留实际轨道高度，最后一行仍可见。
            var height = scrollViewer.Extent.Width > scrollViewer.Viewport.Width + .5
                ? Math.Max(16, scrollViewer.GetVisualDescendants().OfType<ScrollBar>()
                    .FirstOrDefault(bar => bar.Orientation == global::Avalonia.Layout.Orientation.Horizontal)?.Bounds.Height ?? 0)
                : 0;
            var margin = content.Margin;
            if (Math.Abs(margin.Bottom - height) > .5) content.Margin = new Thickness(margin.Left, margin.Top, margin.Right, height);
        };
        return scrollViewer;
    }

    private static ContextMenu CreateCopyContextMenu(
        SelectableTextBlock textBlock,
        Func<bool>? hasGlobalSelection,
        Func<Task>? copyGlobalSelectionAsync)
    {
        var copySelectionItem = new MenuItem
        {
            Header = MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText)
        };
        copySelectionItem.Click += async (_, _) =>
        {
            if (hasGlobalSelection?.Invoke() == true && copyGlobalSelectionAsync is not null)
            {
                await copyGlobalSelectionAsync();
            }
            else
            {
                textBlock.Copy();
            }
        };
        return new ContextMenu { ItemsSource = new[] { copySelectionItem } };
    }

    private static (IGrammar? Grammar, Theme Theme) GetOrCreateGrammar(string language, ThemeName themeName)
    {
        var key = (language, themeName);
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (Cache.Count >= MaxCacheSize)
        {
            var oldest = CacheOrder.Dequeue();
            Cache.Remove(oldest);
        }

        var options = new RegistryOptions(themeName);
        var registry = new Registry(options);
        var theme = registry.GetTheme();
        var scope = options.GetScopeByLanguageId(language);
        var grammar = string.IsNullOrWhiteSpace(scope) ? null : registry.LoadGrammar(scope);
        grammar ??= registry.LoadGrammar(options.GetScopeByLanguageId("log"));

        var value = (grammar, theme);
        Cache[key] = value;
        CacheOrder.Enqueue(key);
        return value;
    }

    private static HighlightedLine[] GetOrCreateHighlightedLines(
        string code,
        string language,
        ThemeName themeName,
        IGrammar grammar,
        Theme theme,
        bool isDark)
    {
        var key = new HighlightCacheKey(code, language, themeName);
        if (HighlightCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (HighlightCache.Count >= MaxCacheSize)
        {
            var oldest = HighlightCacheOrder.Dequeue();
            HighlightCache.Remove(oldest);
        }

        var highlightedLines = Tokenize(code, grammar, theme, isDark);
        HighlightCache[key] = highlightedLines;
        HighlightCacheOrder.Enqueue(key);
        return highlightedLines;
    }

    private static HighlightedLine[] Tokenize(string code, IGrammar grammar, Theme theme, bool isDark)
    {
        var lines = new List<HighlightedLine>();
        IStateStack? ruleStack = null;
        foreach (var line in code.Replace("\r\n", "\n").Split('\n'))
        {
            var result = grammar.TokenizeLine(line, ruleStack, TimeSpan.FromSeconds(2));
            ruleStack = result.RuleStack;
            var tokens = new List<HighlightedToken>();

            foreach (var token in result.Tokens)
            {
                var start = Math.Min(token.StartIndex, line.Length);
                var end = Math.Min(token.EndIndex, line.Length);
                if (end <= start)
                {
                    continue;
                }

                tokens.Add(CreateHighlightedToken(line[start..end], token.Scopes, theme, isDark));
            }

            lines.Add(new HighlightedLine(tokens));
        }

        return lines.ToArray();
    }

    private static Run CreateRun(string text, IEnumerable<string> scopes, Theme theme, bool isDark)
    {
        return CreateHighlightedToken(text, scopes, theme, isDark).CreateRun();
    }

    private static HighlightedToken CreateHighlightedToken(string text, IEnumerable<string> scopes, Theme theme, bool isDark)
    {
        var foregroundId = -1;
        var backgroundId = -1;
        var fontStyle = TextMateFontStyle.None;

        foreach (var rule in theme.Match(scopes.ToList()))
        {
            if (foregroundId == -1 && rule.foreground > 0)
            {
                foregroundId = rule.foreground;
            }

            if (backgroundId == -1 && rule.background > 0)
            {
                backgroundId = rule.background;
            }

            if (rule.fontStyle > 0)
            {
                fontStyle |= (TextMateFontStyle)rule.fontStyle;
            }
        }

        return new HighlightedToken(
            text,
            foregroundId == -1
                ? (isDark ? Brushes.White : Brushes.Black)
                : new ImmutableSolidColorBrush(ParseColor(theme.GetColor(foregroundId))),
            backgroundId == -1
                ? null
                : new ImmutableSolidColorBrush(ParseColor(theme.GetColor(backgroundId))),
            (fontStyle & TextMateFontStyle.Bold) != 0 ? FontWeight.Bold : FontWeight.Normal,
            (fontStyle & TextMateFontStyle.Italic) != 0 ? AvaloniaFontStyle.Italic : AvaloniaFontStyle.Normal);
    }

    private static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "text";
        }

        return language.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant() switch
        {
            "c#" or "cs" => "csharp",
            "js" => "javascript",
            "ts" => "typescript",
            "ps" => "powershell",
            "pwsh" => "powershell",
            var value => value
        };
    }

    private static Color ParseColor(string hex)
    {
        var value = hex.TrimStart('#');
        if (value.Length == 8)
        {
            return Color.FromArgb(
                byte.Parse(value[..2], NumberStyles.HexNumber),
                byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber),
                byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber));
        }

        return Color.FromRgb(
            byte.Parse(value[..2], NumberStyles.HexNumber),
            byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
            byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber));
    }

    private sealed record HighlightedLine(IReadOnlyList<HighlightedToken> Tokens);

    private sealed class HighlightedCodeTextBlock : SelectableTextBlock
    {
        private readonly IReadOnlyList<HighlightedLine> _lines;
        protected override Type StyleKeyOverride => typeof(SelectableTextBlock);
        public HighlightedCodeTextBlock(string text, IReadOnlyList<HighlightedLine> lines)
        { _lines = lines; Text = text; }

        protected override TextLayout CreateTextLayout(string? text)
        {
            var styles = new List<ValueSpan<TextRunProperties>>();
            var start = 0;
            var selectionStart = Math.Min(SelectionStart, SelectionEnd);
            var selectionEnd = Math.Max(SelectionStart, SelectionEnd);
            foreach (var line in _lines)
            {
                foreach (var token in line.Tokens)
                {
                    var end = start + token.Text.Length;
                    var cutStart = Math.Clamp(selectionStart, start, end);
                    var cutEnd = Math.Clamp(selectionEnd, start, end);
                    Add(start, cutStart, token.Foreground);
                    Add(cutStart, cutEnd, SelectionForegroundBrush ?? token.Foreground);
                    Add(cutEnd, end, token.Foreground);
                    start = end;
                    void Add(int from, int to, IBrush? foreground)
                    {
                        if (to <= from) return;
                        styles.Add(new(from, to - from, new GenericTextRunProperties(
                            new Typeface(FontFamily, token.FontStyle, token.FontWeight, FontStretch), FontSize,
                            foregroundBrush: foreground, backgroundBrush: token.Background)));
                    }
                }
                start++; // 源文本使用 LF；每行末尾的换行沿用默认样式。
            }
            return new TextLayout(text ?? string.Empty, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize, Foreground, TextAlignment, TextWrapping, lineHeight: LineHeight,
                letterSpacing: LetterSpacing, textStyleOverrides: styles);
        }
    }

    private sealed record HighlightedToken(
        string Text,
        IBrush? Foreground,
        IBrush? Background,
        FontWeight FontWeight,
        AvaloniaFontStyle FontStyle)
    {
        public Run CreateRun()
        {
            return new Run
            {
                Text = Text,
                Foreground = Foreground,
                Background = Background,
                FontWeight = FontWeight,
                FontStyle = FontStyle
            };
        }
    }

    private readonly record struct HighlightCacheKey(string Code, string Language, ThemeName Theme);
}
