using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Data;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Shared.Rendering;
using Markdig.Syntax;
using CodeWF.Avalonia.Markdown.Themes;

namespace CodeWF.Avalonia.Markdown.Rendering;

/// <summary>
/// 标题渲染器：下边框强调 + 按级别取字号，内容走内联转换与链接交互。
/// </summary>
internal sealed class HeadingRenderer : IMarkdownBlockRenderer
{
    public Control? TryRender(Block block, string? sourceMarkdown, IMarkdownRenderContext context)
    {
        if (block is not HeadingBlock heading)
        {
            return null;
        }

        var border = new Border();
        context.AddMarkdownClass(
            border,
            MarkdownStyleKeys.HeadingBorder,
            MarkdownStyleKeys.GetHeadingBorderClass(heading.Level));
        context.BindTheme(border, Border.BorderBrushProperty, MarkdownViewer.AccentBrushProperty, BindingPriority.Style);
        context.BindTheme(border, Layoutable.MarginProperty, MarkdownViewer.HeadingMarginProperty, BindingPriority.Style);

        var textBlock = context.CreateSelectableText(MarkdownStyleKeys.Heading, MarkdownStyleKeys.GetHeadingClass(heading.Level));
        textBlock.FontWeight = FontWeight.Bold;
        context.BindTheme(textBlock, SelectableTextBlock.ForegroundProperty, MarkdownViewer.TextBrushProperty, BindingPriority.Style);
        context.BindTheme(textBlock, SelectableTextBlock.FontFamilyProperty, MarkdownViewer.ContentFontFamilyProperty);
        context.BindTheme(textBlock, SelectableTextBlock.FontSizeProperty, GetHeadingFontSizeProperty(heading.Level));

        foreach (var inline in context.ConvertInlines(heading.Inline, false))
        {
            textBlock.Inlines?.Add(inline);
        }

        context.AttachLinkInteraction(textBlock, heading.Inline, false);

        if (context is MarkdownViewer viewer && ArticleTypographyCatalog.Find(viewer.TypographyTheme) is { } article)
        {
        var prefix = "CodeWFMarkdownHeading" + heading.Level;
        border.Bind(Border.BorderThicknessProperty, border.GetResourceObservable(prefix + "BorderThickness"));
        border.Bind(Border.PaddingProperty, border.GetResourceObservable(prefix + "Padding"));
        border.Bind(Border.CornerRadiusProperty, border.GetResourceObservable(prefix + "CornerRadius"));
        border.Bind(Border.BackgroundProperty, border.GetResourceObservable(prefix + "Background"));
        border.Bind(Border.BorderBrushProperty, border.GetResourceObservable(prefix + "BorderBrush"));
        textBlock.Bind(SelectableTextBlock.ForegroundProperty, textBlock.GetResourceObservable(prefix + "Foreground"));
        textBlock.Bind(SelectableTextBlock.FontWeightProperty, textBlock.GetResourceObservable(prefix + "FontWeight"));
        textBlock.Bind(SelectableTextBlock.TextAlignmentProperty, textBlock.GetResourceObservable(prefix + "TextAlignment"));
        var font = article.Headings[heading.Level - 1].Font == "system" ? article.Font : article.Headings[heading.Level - 1].Font;
        if (font != "system") textBlock.FontFamily = ArticleTypography.ResolveFont(font);
        }

        border.Child = textBlock;
        return border;
    }

    private static StyledProperty<double> GetHeadingFontSizeProperty(int level)
    {
        return level switch
        {
            1 => MarkdownViewer.Heading1FontSizeProperty,
            2 => MarkdownViewer.Heading2FontSizeProperty,
            3 => MarkdownViewer.Heading3FontSizeProperty,
            4 => MarkdownViewer.Heading4FontSizeProperty,
            5 => MarkdownViewer.Heading5FontSizeProperty,
            _ => MarkdownViewer.Heading6FontSizeProperty
        };
    }
}
