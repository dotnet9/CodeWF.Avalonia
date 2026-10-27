using CodeWF.Avalonia.Markdown.Mermaid;
using global::Avalonia;
using global::Avalonia.Media;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MermaidThemeOptionsTests(AvaloniaPlatformFixture platform)
{
    /// <summary>宿主可以逐档覆盖图表配色；漏读某一档时，主题里那条资源就是死配置。</summary>
    [Fact]
    public void HostMutedResourceReachesTheOptionsAndTheCacheKey() => platform.Run(() =>
    {
        var app = Application.Current!;
        const string source = "flowchart LR\nA[开始] --> B[结束]";
        var without = MermaidBlockRenderer.GetRenderOptions(source);
        app.Resources["CodeWFMarkdownMermaidMuted"] = new SolidColorBrush(Colors.Red);
        try
        {
            var with = MermaidBlockRenderer.GetRenderOptions(source);
            Assert.Null(without.Muted);
            Assert.Equal("#FF0000", with.Muted);
            Assert.NotEqual(MermaidBlockRenderer.CreateCacheKey(source, without),
                MermaidBlockRenderer.CreateCacheKey(source, with));
        }
        finally
        {
            app.Resources.Remove("CodeWFMarkdownMermaidMuted");
        }
    });
}
