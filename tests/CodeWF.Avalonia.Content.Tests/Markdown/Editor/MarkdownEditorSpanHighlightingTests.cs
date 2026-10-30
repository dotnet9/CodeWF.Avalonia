using System.Linq;

using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Document;

using CodeWF.Avalonia.Markdown.Editor.Controls;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorSpanHighlightingTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownEditorSpanHighlightingTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void EditorHighlightsSpanTagsAsMarker()
    {
        _platform.Run(() =>
        {
            var view = new MarkdownEditorView
            {
                Text = "选中文字调整<span style=\"color:#D52515\">颜色</span>、<span style=\"font-size:20px\">字号</span>。\n"
            };
            var definition = view.Editor.SyntaxHighlighting;
            Assert.NotNull(definition);
            var marker = definition.NamedHighlightingColors.FirstOrDefault(color => color.Name == "Marker");
            Assert.NotNull(marker);

            // span 标签 Rule 是否注册进主规则集，且正则能命中样本文本。
            var spanRules = definition.MainRuleSet.Rules
                .OfType<AvaloniaEdit.Highlighting.HighlightingRule>()
                .Where(rule => rule.Color?.Name == "Marker")
                .ToList();
            Assert.Contains(spanRules, rule => rule.Regex.IsMatch("<span style=\"color:#D52515\">"));
            Assert.Contains(spanRules, rule => rule.Regex.IsMatch("</span>"));
        });
    }
}
