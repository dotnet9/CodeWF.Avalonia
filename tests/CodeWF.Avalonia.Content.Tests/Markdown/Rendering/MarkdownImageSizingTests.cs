using System.Text;
using Markdig;
using CodeWF.Avalonia.Markdown.Controls;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownImageSizingTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task TableImagesReserveTheirHeightAndFollowColumnWidth(bool liveEditing) => platform.RunAsync(async () =>
    {
        var images = new List<(MarkdownImage Image, ContentControl Host)>();
        MarkdownViewer.RegisterImageControlFactory((_, _, _) =>
        {
            var host = new ContentControl();
            var image = new MarkdownImage
            {
                Source = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    "<svg xmlns='http://www.w3.org/2000/svg' width='240' height='120'><rect width='240' height='120'/></svg>")),
                Sizing = MarkdownImageSizing.FillAvailableWidth,
                Template = new FuncControlTemplate<MarkdownImage>((_, scope) =>
                { scope.Register("PART_ContentHost", host); return host; })
            };
            images.Add((image, host));
            return image;
        });
        const string source = "| Light | Dark |\n| --- | --- |\n| ![left](left.svg) | ![right](right.svg) |";
        var document = Markdig.Markdown.Parse(source, new Markdig.MarkdownPipelineBuilder().UsePipeTables().Build());
        var table = new CodeWF.Avalonia.Markdown.Rendering.TableRenderer().TryRender(document[0], source, new MarkdownViewer());
        if (liveEditing)
        {
            var editor = new CodeWF.Avalonia.Markdown.Editor.Controls.MarkdownLiveEditorView
                { HeaderText = null, ChipText = null, DocumentPadding = new global::Avalonia.Thickness(0) };
            editor.SetText(source);
            table = editor;
            images.Clear();
        }
        var window = new Window { Width = 600, Height = 500, Content = table };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new CodeWF.Avalonia.Markdown.Themes.MarkdownFullThemes());
        try
        {
            window.Show();
            for (var i = 0; i < 100 && (images.Count < 2 || images.Any(item => item.Host.Content is not Image)); i++) await Task.Delay(10);
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Equal(2, images.Count);
            var wide = images[0].Image.Bounds.Width;
            foreach (var item in images)
            {
                Assert.IsType<Image>(item.Host.Content);
                Assert.InRange(item.Image.Bounds.Width, 100, 301);
                Assert.True(item.Image.Bounds.Height > 50);
                Assert.True(table!.Bounds.Height >= item.Image.Bounds.Bottom, "The table must reserve space for its image row.");
            }
            window.Width = 300; window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            Assert.True(images[0].Image.Bounds.Width < wide);
            Assert.True(table!.Bounds.Width <= 301);
        }
        finally { window.Close(); MarkdownViewer.RegisterImageControlFactory(null); }
    });

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
