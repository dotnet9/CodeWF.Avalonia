using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Styling;
using global::Avalonia.Threading;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

/// <summary>
/// <see cref="MarkdownEditorView"/> 的同步语义回归测试。
/// <para>
/// 背景：宿主（Vex）切换文件时会把新正文写入编辑器，若这次写入被当成「用户编辑」回抛，
/// Shell 就会把刚打开的文档标记为已修改并弹出保存确认。这里断言
/// 「<see cref="MarkdownEditorView.SetText"/> 与 <see cref="MarkdownEditorView.ApplyExternalEdit"/>
/// 的整篇同步不触发 <see cref="MarkdownEditorView.MarkdownChanged"/>」，
/// 而真实输入（<see cref="MarkdownEditorView.InsertText"/>）必须触发。
/// </para>
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorViewSyncTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public MarkdownEditorViewSyncTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void FullThemesRenderEditorsWithSemiWithoutSimpleOrFluent() => _platform.Run(() =>
    {
        var window = new Window { Width = 480, Height = 320 };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new global::CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
        var editor = new MarkdownEditorView { Text = "# Visible source" };
        window.Content = editor;
        window.Show();
        try
        {
            foreach (var theme in new[] { ThemeVariant.Light, new ThemeVariant("StudioDark", ThemeVariant.Dark) })
            {
                window.RequestedThemeVariant = theme;
                Dispatcher.UIThread.RunJobs();
                Assert.NotNull(editor.Editor.Template);
                Assert.NotNull(editor.Editor.TextArea.Template);
                Assert.True(editor.Editor.TextArea.TextView.Bounds.Height > 0);
                Assert.Equal("# Visible source", editor.Editor.Text);
            }
        }
        finally { window.Close(); }
    });

    [Fact]
    public void DerivedDarkThemesUseTheDarkFallbackPalette()
    {
        var theme = new ThemeVariant("Studio", new ThemeVariant("CustomDark", ThemeVariant.Dark));
        var expected = MarkdownEditorPalette.Resolve(MarkdownEditorPalette.BackgroundKey, ThemeVariant.Dark);
        var actual = MarkdownEditorPalette.Resolve(MarkdownEditorPalette.BackgroundKey, theme);
        Assert.Equal(Assert.IsAssignableFrom<global::Avalonia.Media.ISolidColorBrush>(expected).Color,
            Assert.IsAssignableFrom<global::Avalonia.Media.ISolidColorBrush>(actual).Color);
    }

    [Fact]
    public void TextBindingSynchronizesProgrammaticAndUserEdits() => _platform.Run(() =>
    {
        var source = new TextBox { Text = "# Initial" };
        var view = new MarkdownEditorView();
        using var binding = view.Bind(MarkdownEditorView.TextProperty, new global::Avalonia.Data.Binding
        {
            Source = source, Path = nameof(TextBox.Text), Mode = global::Avalonia.Data.BindingMode.TwoWay
        });
        Assert.Equal("# Initial", view.Text);
        source.Text = "# Updated";
        Assert.Equal("# Updated", view.Text);
        view.Editor.Text = "# Typed";
        Assert.Equal("# Typed", source.Text);
        view.SetText("# Loaded");
        Assert.Equal("# Loaded", source.Text);
    });

    [Fact]
    public void SetText_DoesNotRaiseMarkdownChanged() => _platform.Run(() =>
    {
        var view = new MarkdownEditorView();
        var raised = 0;
        view.MarkdownChanged += (_, _) => raised++;

        view.SetText("# first document");
        view.SetText("# second document");

        Assert.Equal("# second document", view.Text);
        Assert.Equal(0, raised);
    });

    [Fact]
    public void InsertText_RaisesMarkdownChanged() => _platform.Run(() =>
    {
        var view = new MarkdownEditorView();
        var payloads = new List<string>();
        view.MarkdownChanged += (_, text) => payloads.Add(text);

        // SetText 把光标复位到 0，因此插入发生在文档头部。
        view.SetText("abc");
        view.InsertText("XY");

        Assert.Equal("XYabc", view.Text);
        Assert.Contains("XYabc", payloads);
    });

    [Fact]
    public void ApplyExternalEdit_ReplaceInPlace_RaisesMarkdownChangedOnce() => _platform.Run(() =>
    {
        var view = new MarkdownEditorView();
        view.SetText("- [ ] task");

        var raised = 0;
        view.MarkdownChanged += (_, _) => raised++;

        // 长度一致：原地替换任务勾选状态，应只回抛一次。
        view.ApplyExternalEdit("- [x] task", 3, 1);

        Assert.Equal("- [x] task", view.Text);
        Assert.Equal(1, raised);
    });

    [Fact]
    public void ExecuteAsync_Bold_WrapsSelectionAndRaisesChanged() => _platform.Run(() =>
    {
        var view = new MarkdownEditorView();
        view.SetText("word");

        view.Editor.SelectionStart = 0;
        view.Editor.SelectionLength = 4;

        var raised = 0;
        view.MarkdownChanged += (_, _) => raised++;

        view.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();

        Assert.Equal("**word**", view.Text);
        Assert.Equal(1, raised);
    });

    [Fact]
    public void SimultaneousLightAndDarkEditors_KeepIndependentHighlighting() => _platform.Run(() =>
    {
        var light = new MarkdownEditorView();
        var dark = new MarkdownEditorView();
        var window = new Window
        {
            Content = new StackPanel
            {
                Children =
                {
                    new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Light, Child = light },
                    new ThemeVariantScope { RequestedThemeVariant = ThemeVariant.Dark, Child = dark }
                }
            }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var lightColor = light.Editor.SyntaxHighlighting.GetNamedColor("Heading").Foreground!.GetColor(null);
            var darkColor = dark.Editor.SyntaxHighlighting.GetNamedColor("Heading").Foreground!.GetColor(null);
            Assert.NotEqual(lightColor, darkColor);
            Assert.NotSame(light.Editor.SyntaxHighlighting, dark.Editor.SyntaxHighlighting);
            foreach (var editor in new[] { light, dark })
            {
                var definition = editor.Editor.SyntaxHighlighting;
                var visibility = definition.MainRuleSet.Spans.Select(span => span.RuleSet)
                    .Where(rules => rules is not null).SelectMany(rules => rules!.Rules)
                    .First(rule => rule.Color?.Name == "Visibility").Color!;
                Assert.Equal(definition.GetNamedColor("Link").Foreground!.GetColor(null), visibility.Foreground!.GetColor(null));
                Assert.Equal(definition.GetNamedColor("Link").Foreground!.GetColor(null),
                    Assert.IsAssignableFrom<global::Avalonia.Media.ISolidColorBrush>(editor.Editor.TextArea.TextView.LinkTextForegroundBrush).Color);
            }
        }
        finally { window.Close(); }
    });
}
