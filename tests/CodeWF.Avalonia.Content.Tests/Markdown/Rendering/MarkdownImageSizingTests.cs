using System.Text;
using CodeWF.Avalonia.Markdown.Controls;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownImageSizingTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public Task LoadedImagePreservesRatioAndCanSwitchBetweenSizingPolicies() => platform.RunAsync(async () =>
    {
        var host = new ContentControl();
        var image = new MarkdownImage
        {
            Source = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(
                "<svg xmlns='http://www.w3.org/2000/svg' width='200' height='100'><rect width='200' height='100'/></svg>")),
            Template = new FuncControlTemplate<MarkdownImage>((_, scope) =>
            {
                scope.Register("PART_ContentHost", host);
                return host;
            })
        };
        var window = new Window { Width = 600, Height = 500, Content = image };
        try
        {
            window.Show();
            for (var i = 0; i < 100 && host.Content is not Image; i++) await Task.Delay(10);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var bitmap = Assert.IsType<Image>(host.Content);
            Assert.Equal(200, bitmap.Width, 1); Assert.Equal(100, bitmap.Height, 1);
            image.Sizing = MarkdownImageSizing.FillAvailableWidth;
            window.UpdateLayout();
            Assert.Equal(600, bitmap.Width, 1); Assert.Equal(300, bitmap.Height, 1);
            window.Width = 320; window.UpdateLayout();
            Assert.Equal(320, bitmap.Width, 1); Assert.Equal(160, bitmap.Height, 1);
            image.Sizing = MarkdownImageSizing.Natural; window.UpdateLayout();
            Assert.Equal(200, bitmap.Width, 1); Assert.Equal(100, bitmap.Height, 1);
            Assert.Equal(double.PositiveInfinity, image.MaxWidth);
        }
        finally { window.Close(); }
    });
}
