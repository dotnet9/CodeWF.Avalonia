using CodeWF.Avalonia.Markdown.Mermaid;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Styling;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MermaidLifecycleTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData("flowchart LR\nA[Input] --> B[Output]")]
    [InlineData("not valid mermaid")]
    public void RepeatedAttachDetachAndThemeChangesAreSafe(string source) => platform.Run(() =>
    {
        var block = MermaidBlockRenderer.CreateMermaidBlock(source);
        var scroll = new ScrollViewer { Content = block };
        var window = new Window { Width = 600, Height = 400 };
        var app = Application.Current!; var originalTheme = app.RequestedThemeVariant;
        window.Show();
        try
        {
            for (var pass = 0; pass < 5; pass++)
            {
                window.Content = scroll; Dispatcher.UIThread.RunJobs();
                app.RequestedThemeVariant = pass % 2 == 0 ? ThemeVariant.Dark : ThemeVariant.Light;
                Dispatcher.UIThread.RunJobs();
                window.Content = null; Dispatcher.UIThread.RunJobs();
            }
        }
        finally { window.Close(); app.RequestedThemeVariant = originalTheme; }
    });

    /// <summary>图表宽度只受视口约束：宿主栏宽超过 800 时不该再被一档固定上限压小。</summary>
    [Fact]
    public void DiagramCeilingFollowsTheViewportInsteadOfAFixedWidth() => platform.Run(() =>
    {
        var block = MermaidBlockRenderer.CreateMermaidBlock("flowchart LR\nA[Input] --> B[Output]");
        var image = ((Border)block).Child!;
        var scroll = new ScrollViewer { Content = block };
        // 此夹具没有主题，裸 ScrollViewer 取不到模板、Viewport 恒为 0；按本仓既有做法塞一个最小 presenter 模板。
        scroll.Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<ScrollViewer>((_, scope) =>
        {
            var presenter = new global::Avalonia.Controls.Presenters.ScrollContentPresenter { Content = scroll.Content };
            scope.Register("PART_ScrollContentPresenter", presenter);
            return presenter;
        });
        var window = new Window { Width = 1100, Height = 600, Content = scroll };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.True(scroll.Viewport.Width > 900, $"视口没铺开：viewport={scroll.Viewport} max={image.MaxWidth}");
            Assert.Equal(scroll.Viewport.Width, image.MaxWidth);
        }
        finally { window.Close(); }
    });
}
