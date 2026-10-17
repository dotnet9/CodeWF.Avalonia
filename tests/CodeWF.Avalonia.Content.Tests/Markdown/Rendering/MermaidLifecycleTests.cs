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
}
