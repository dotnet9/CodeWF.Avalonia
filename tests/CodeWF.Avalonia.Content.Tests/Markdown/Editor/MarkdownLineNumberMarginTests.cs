using System.Globalization;
using Avalonia;
using AvaloniaEdit.Rendering;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownLineNumberMarginTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(12, 21)]
    [InlineData(15, 26.25)]
    [InlineData(24, 42)]
    public void NumbersFollowFirstWrappedTextBaselineAtEveryScrollOffset(double fontSize, double lineHeight) =>
        platform.Run(() =>
        {
            var editor = new MarkdownEditorView
            {
                EditorFontSize = fontSize,
                EditorLineHeight = lineHeight,
                Text = string.Join('\n', Enumerable.Range(1, 60)
                    .Select(number => $"Line {number}: " + new string('W', 60)))
            };
            var window = new Window { Width = 380, Height = 260 };
            window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
            window.Styles.Add(new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
            window.Content = editor;
            window.RequestedThemeVariant = global::Avalonia.Styling.ThemeVariant.Light;
            window.Show();
            try
            {
                editor.Measure(new Size(380, 260));
                editor.Arrange(new Rect(0, 0, 380, 260));
                var textView = editor.Editor.TextArea.TextView;
                var margin = Assert.Single(editor.Editor.TextArea.LeftMargins.OfType<MarkdownLineNumberMargin>());
                foreach (var offset in new[] { 0d, lineHeight * 7 + 3 })
                {
                    editor.Editor.ScrollToVerticalOffset(offset);
                    Dispatcher.UIThread.RunJobs();
                    window.UpdateLayout();
                    textView.EnsureVisualLines();
                    Assert.True(textView.VisualLines.Count > 0,
                        $"TextView={textView.Bounds}, area={editor.Editor.TextArea.Bounds}, editor={editor.Bounds}, template={editor.Editor.Template is not null}, document={textView.Document?.TextLength}");
                    Assert.Contains(textView.VisualLines, line => line.TextLines.Count > 1);
                    foreach (var visualLine in textView.VisualLines)
                    {
                        var number = new FormattedText(
                            visualLine.FirstDocumentLine.LineNumber.ToString(CultureInfo.InvariantCulture),
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Cascadia Mono, Consolas, Microsoft YaHei UI")),
                            margin.FontSize, Brushes.Gray);
                        var origin = margin.GetTextOrigin(visualLine, number);
                        var expected = visualLine.GetTextLineVisualYPosition(
                            visualLine.TextLines[0], VisualYPosition.Baseline) - textView.VerticalOffset;
                        Assert.Equal(expected, origin.Y + number.Baseline, 6);
                        Assert.Equal(margin.Bounds.Width - margin.RightPadding, origin.X + number.Width, 6);
                    }
                }
            }
            finally { window.Close(); }
        });
}
