using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Presenters;
using global::Avalonia.Headless;
using global::Avalonia.Input;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownInlinePresenterLayoutTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LargeInlineKeepsNaturalLineMetricsWithoutInflatingOrdinaryLines(bool selected) => platform.Run(() =>
    {
        const string text = "<span style=\"font-size:86px\">想</span>法可以很大\n下一行普通文字";
        var presenter = Create(text);
        if (selected) { presenter.SelectionStart = 0; presenter.SelectionEnd = text.Length; }
        presenter.Measure(new Size(600, double.PositiveInfinity));
        presenter.Arrange(new Rect(presenter.DesiredSize));
        var lines = presenter.TextLayout.TextLines;
        Assert.True(lines.Count >= 2);
        // The public span parser clamps authored sizes, including this 86px demo.
        var renderedSize = MarkdownTextRunParser.Parse(text).Max(run => run.FontSize ?? 16);
        Assert.True(lines[0].Height >= renderedSize, $"Large line clipped: {lines[0].Height}");
        Assert.InRange(lines[^1].Height, 24, 32);
        Assert.True(lines[0].Baseline >= renderedSize * .7, $"Large line baseline clipped: {lines[0].Baseline}");
    });

    [Fact]
    public void SelectedColoredTextDoesNotKeepItsUnselectedBackground() => platform.Run(() =>
    {
        const string text = "<span style=\"background-color:#ecf7ec;color:#30953b\">绿底文字</span>";
        var presenter = Create(text);
        presenter.SelectionStart = text.IndexOf('绿');
        presenter.SelectionEnd = text.IndexOf("</span>", StringComparison.Ordinal);
        presenter.Measure(new Size(600, double.PositiveInfinity));
        var selected = presenter.TextLayout.TextLines.SelectMany(line => line.TextRuns)
            .Where(run => run.Properties?.FontRenderingEmSize > 1).ToArray();
        Assert.NotEmpty(selected);
        Assert.All(selected, run =>
        {
            Assert.Same(Brushes.White, run.Properties!.ForegroundBrush);
            Assert.Null(run.Properties.BackgroundBrush);
        });
        presenter.SelectionEnd = presenter.SelectionStart;
        presenter.Measure(new Size(600, double.PositiveInfinity));
        Assert.Contains(presenter.TextLayout.TextLines.SelectMany(line => line.TextRuns),
            run => run.Properties?.BackgroundBrush is SolidColorBrush brush && brush.Color == Color.Parse("#ecf7ec"));
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnteringAndSelectingMixedTextKeepsPreviewAndEditorGeometry(bool dark) => platform.Run(() =>
    {
        const string markdown = "同段 <span style=\"font-size:86px\">想</span>法可以很大，普通小字和 emoji 🧭 都要完整。\n下一行普通文字";
        var view = new MarkdownLiveEditorView { HeaderText = null, ChipText = null };
        view.SetText(markdown);
        var window = new Window { Width = 900, Height = 600, Content = view,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
        window.Show();
        void Layout() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs(); }
        try
        {
            Layout();
            var preview = view.GetVisualDescendants().OfType<SelectableTextBlock>()
                .First(text => text.Inlines?.Text?.Contains("同段") == true);
            var before = preview.TextLayout.TextLines.Select(line => (line.Height, line.Baseline)).ToArray();
            var glyph = preview.TextLayout.HitTestTextPosition(3);
            var point = preview.TranslatePoint(glyph.Center, window)!.Value;
            window.MouseMove(point); window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left); Layout();
            var block = view.GetVisualDescendants().OfType<MarkdownBlockView>().Single(item => item.IsEditing);
            var editor = block.ActiveEditor!;
            var presenter = editor.GetVisualDescendants().OfType<TextPresenter>().Single();
            foreach (var reverse in new[] { false, true })
            {
                editor.SelectionStart = reverse ? markdown.Length : 0;
                editor.SelectionEnd = reverse ? 0 : markdown.Length;
                Layout();
                var after = presenter.TextLayout.TextLines.Select(line => (line.Height, line.Baseline)).ToArray();
                Assert.Equal(before, after);
                Assert.Equal(markdown, view.Text);
                editor.CaretIndex = editor.SelectionStart = editor.SelectionEnd = markdown.IndexOf('小');
                Layout();
                Assert.Equal(before, presenter.TextLayout.TextLines.Select(line => (line.Height, line.Baseline)).ToArray());
            }
            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, "输入");
            Layout();
            Assert.Equal(markdown, view.Text);
            Assert.True(presenter.TextLayout.TextLines[0].Height >= 64);
            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, null);
        }
        finally { window.Close(); }
    });

    private static MarkdownInlinePresenter Create(string text) => new(new MarkdownBlockViewOptions(), false)
    {
        Text = text, FontSize = 16, LineHeight = 24, Foreground = Brushes.Black,
        SelectionBrush = Brushes.DarkBlue, SelectionForegroundBrush = Brushes.White,
        TextWrapping = TextWrapping.Wrap
    };
}
