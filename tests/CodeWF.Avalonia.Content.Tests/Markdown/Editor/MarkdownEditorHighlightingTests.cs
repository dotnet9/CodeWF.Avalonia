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
}
