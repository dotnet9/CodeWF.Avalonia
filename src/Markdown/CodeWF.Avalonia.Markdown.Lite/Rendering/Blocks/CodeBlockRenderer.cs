using global::Avalonia.Automation;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.LogicalTree;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Toolkit.Lang;
using Markdig.Syntax;

namespace CodeWF.Avalonia.Markdown.Rendering;

/// <summary>
/// 代码块渲染器：语言标头 + 复制按钮 + TextMate 语法高亮，
/// 受理 FencedCodeBlock 与缩进 CodeBlock。
/// </summary>
internal sealed class CodeBlockRenderer : IMarkdownBlockRenderer
{
    /// <summary>行数达到该阈值的代码块才显示折叠按钮（短代码折叠无收益）。</summary>
    internal const int FoldableLineThreshold = 8;

    public Control? TryRender(Block block, string? sourceMarkdown, IMarkdownRenderContext context)
    {
        if (block is not CodeBlock codeBlock)
        {
            return null;
        }

        var code = codeBlock.Lines.ToString();
        var language = codeBlock is FencedCodeBlock fenced ? fenced.Info ?? "text" : "text";
        var blockKey = Math.Max(codeBlock.Span.Start, 0);
        var isFolded = context.IsCodeBlockFolded(blockKey);

        var border = new Border();
        context.AddMarkdownClass(border, MarkdownStyleKeys.CodeBlock);
        context.BindTheme(border, Border.BackgroundProperty, MarkdownViewer.CodeBackgroundBrushProperty);
        context.BindTheme(border, Border.BorderBrushProperty, MarkdownViewer.BorderLineBrushProperty);

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        context.AddMarkdownClass(stack, MarkdownStyleKeys.CodeBlockContent);
        stack.IsVisible = !isFolded;
        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        context.AddMarkdownClass(header, MarkdownStyleKeys.CodeBlockHeader);
        context.BindTheme(header, Control.IsVisibleProperty, MarkdownViewer.ShowCodeBlockHeaderProperty);

        if (codeBlock.Lines.Count >= FoldableLineThreshold)
        {
            var foldButton = new Button
            {
                Content = isFolded ? "▸" : "▾",
                Tag = "CodeFold",
                MinHeight = 0,
                Background = Brushes.Transparent
            };
            global::Avalonia.Automation.AutomationProperties.SetName(foldButton,
                $"{language} {(isFolded ? "▸" : "▾")}");
            foldButton.Click += (_, _) => context.SetCodeBlockFolded(blockKey, !isFolded);
            header.Children.Add(foldButton);
        }

        var languageText = context.CreateSelectableText(MarkdownStyleKeys.CodeLanguage);
        languageText.Text = string.IsNullOrWhiteSpace(language) ? "text" : language;
        languageText.VerticalAlignment = VerticalAlignment.Center;
        context.BindTheme(languageText, SelectableTextBlock.ForegroundProperty, MarkdownViewer.MutedTextBrushProperty);
        context.BindTheme(languageText, SelectableTextBlock.FontSizeProperty, MarkdownViewer.CodeLanguageFontSizeProperty);

        var copyButton = new Button
        {
            Content = MarkdownLocalization.GetResource(MarkdownL.Copy),
            Tag = code,
            MinHeight = 0,
            Background = Brushes.Transparent
        };
        // 多代码块场景下读屏器需要可区分的按钮名称："语言 复制"。
        AutomationProperties.SetName(copyButton,
            $"{(string.IsNullOrWhiteSpace(language) ? "text" : language)} {MarkdownLocalization.GetResource(MarkdownL.Copy)}");
        context.AddMarkdownClass(copyButton, MarkdownStyleKeys.CopyButton);
        context.BindTheme(copyButton, Button.ForegroundProperty, MarkdownViewer.TextBrushProperty);
        copyButton.Click += (_, _) =>
        {
            context.CopyCodeToClipboard(code);
        };

        header.Children.Add(languageText);
        header.Children.Add(copyButton);
        stack.Children.Add(header);
        var codeView = context.CodeHighlighter?.Invoke(
                code,
                language,
                context.CodeBlockIsDark,
                context.CodeFontFamily,
                context.CodeBlockFontSize,
                context.CodeBlockLineHeight,
                context.GetCodeHighlightSession(blockKey),
                () => context.HasSelection,
                context.CopySelectionAsync)
            ?? CreatePlainCodeFallback(code, context);
        if (context.CodeBlockWrap)
        {
            foreach (var text in codeView.GetLogicalDescendants().OfType<SelectableTextBlock>())
            {
                if (text.Tag as string == "CodeLineNumbers") text.IsVisible = false;
                else text.TextWrapping = TextWrapping.Wrap;
            }
            foreach (var scroll in codeView.GetLogicalDescendants().OfType<ScrollViewer>().Prepend(codeView as ScrollViewer).OfType<ScrollViewer>())
                scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
        stack.Children.Add(codeView);

        context.RaiseCodeBlockToolRender(header, stack, codeBlock);

        border.Child = stack;
        return border;
    }

    /// <summary>
    /// 无高亮能力时的降级渲染：单色等宽文本（Core 零高亮依赖）。
    /// </summary>
    private static Control CreatePlainCodeFallback(string code, IMarkdownRenderContext context)
    {
        var textBlock = context.CreateSelectableText("MdCodeBlockPlain");
        textBlock.FontFamily = context.CodeFontFamily;
        textBlock.FontSize = context.CodeBlockFontSize;
        textBlock.LineHeight = context.CodeBlockLineHeight;
        textBlock.TextWrapping = TextWrapping.NoWrap;
        context.BindTheme(textBlock, SelectableTextBlock.ForegroundProperty, MarkdownViewer.TextBrushProperty);

        var lines = code.TrimEnd().Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            textBlock.Inlines?.Add(new Run(lines[i]));
            if (i < lines.Length - 1)
            {
                textBlock.Inlines?.Add(new LineBreak());
            }
        }

        var scrollViewer = new ScrollViewer
        {
            Content = textBlock,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        return scrollViewer;
    }
}
