using System.Linq;
using AvaloniaEdit.Highlighting;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorHighlightingTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownEditorHighlightingTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void EditorLoadsMarkerSplitHighlightingAndTintsMarkers()
    {
        _platform.Run(() =>
        {
            var view = new MarkdownEditorView
            {
                Text = "# 标题\n\n**加粗** 与 [链接](https://codewf.com)\n\n```cs\nvar x = 1;\n```\n"
            };
            var definition = view.Editor.SyntaxHighlighting;
            Assert.NotNull(definition);
            Assert.NotNull(definition.NamedHighlightingColors.FirstOrDefault(color => color.Name == "Marker"));

            // 运行时注入：内置兜底调色板的 Marker 灰应已落到命名色上。
            var marker = definition.NamedHighlightingColors.First(color => color.Name == "Marker");
            var fallback = Assert.IsAssignableFrom<global::Avalonia.Media.ISolidColorBrush>(
                MarkdownEditorPalette.Resolve(MarkdownEditorPalette.MarkerKey, view.ActualThemeVariant));
            Assert.True(fallback.Color == marker.Foreground?.GetColor(null),
                $"Marker foreground should use the palette fallback: {marker.Foreground?.GetColor(null)}");
        });
    }

    [Theory]
    [InlineData("- [ ] 带上相机")]
    [InlineData("- [x] 收拾行李")]
    [InlineData("1. [X] 收拾行李")]
    public void TaskMarkersDoNotColorTheFollowingTextAsALink(string text) => _platform.Run(() =>
    {
        var view = new MarkdownEditorView { Text = text };
        using var highlighter = new DocumentHighlighter(view.Editor.Document, view.Editor.SyntaxHighlighting);
        var sections = highlighter.HighlightLine(1).Sections;
        Assert.DoesNotContain(sections, section => section.Color.Name == "Link");
        Assert.Contains(sections, section => section.Color.Name == "Marker"
            && section.Offset <= text.IndexOf('[') && section.Offset + section.Length >= text.IndexOf(']') + 1);
    });

    [Theory]
    [InlineData("[链接](https://codewf.com) 后续正文")]
    [InlineData("[链接] 后续正文")]
    [InlineData("![图片](image.png) 后续正文")]
    public void LinksAndImagesDoNotColorTrailingText(string text) => _platform.Run(() =>
    {
        var view = new MarkdownEditorView { Text = text };
        using var highlighter = new DocumentHighlighter(view.Editor.Document, view.Editor.SyntaxHighlighting);
        var sections = highlighter.HighlightLine(1).Sections;
        Assert.Contains(sections, section => section.Color.Name == "Link");
        Assert.DoesNotContain(sections, section => section.Offset + section.Length > text.IndexOf(" 后续正文", StringComparison.Ordinal));
        if (text.Contains('(')) Assert.Contains(sections, section => section.Color.Name == "Url");
    });
}
