using System.Globalization;
using System.Text.Json;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Templates;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownLocalizationTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void ViewerRefreshesMenuWhenCultureChangesBeforeAttachment() => platform.Run(() =>
    {
        var manager = I18nManager.Instance;
        manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("zh-CN"));
        var viewer = new MarkdownViewer();
        manager.Culture = new CultureInfo("en-US");
        var window = new Window { Content = viewer };
        try
        {
            window.Show();
            Assert.Equal("Copy rendered text", Assert.IsType<MenuItem>(viewer.ContextMenu!.Items[0]).Header);
        }
        finally
        {
            window.Close();
            manager.Culture = new CultureInfo("en-US");
        }
    });

    [Fact]
    public void ExistingViewerUpdatesMenusAndCodeButtonsAcrossCultureChanges() => platform.Run(() =>
    {
        var manager = I18nManager.Instance;
        manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
        var host = new StackPanel();
        var viewer = new MarkdownViewer
        {
            Markdown = "Paragraph\n\n```text\nSample\n```",
            Template = new FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                scope.Register("PART_DocumentHost", host);
                return host;
            })
        };
        var window = new Window { Content = viewer };
        try
        {
            window.Show();
            foreach (var (culture, copy, rendered, selected) in new[]
            {
                ("en-US", "Copy", "Copy rendered text", "Copy selected text"),
                ("zh-CN", "复制", "复制渲染文本", "复制选中文本"),
                ("ja-JP", "コピー", "レンダリング済みテキストをコピー", "選択したテキストをコピー"),
                ("fr-FR", "Copy", "Copy rendered text", "Copy selected text"),
                ("en-US", "Copy", "Copy rendered text", "Copy selected text")
            })
            {
                manager.Culture = new CultureInfo(culture);
                var deadline = DateTime.UtcNow.AddSeconds(5);
                while (!viewer.GetVisualDescendants().OfType<Button>().Any(button => Equals(ToolTip.GetTip(button), copy)))
                {
                    Dispatcher.UIThread.RunJobs();
                    Assert.True(DateTime.UtcNow < deadline, "Code block translation did not refresh: " + culture);
                    Thread.Sleep(10);
                }
                Assert.Equal(rendered, Assert.IsType<MenuItem>(viewer.ContextMenu!.Items[0]).Header);
                Assert.Contains(viewer.GetVisualDescendants().OfType<Button>(), button => Equals(ToolTip.GetTip(button), copy));
                var paragraph = viewer.GetVisualDescendants().OfType<SelectableTextBlock>().First(text => text.ContextMenu?.Items.Count == 2);
                Assert.Equal(selected, Assert.IsType<MenuItem>(paragraph.ContextMenu!.Items[0]).Header);
            }
        }
        finally
        {
            window.Close();
            manager.Culture = new CultureInfo("en-US");
        }
    });

    [Fact]
    public void ExistingViewerRefreshesAfterHostOverridesAreReloaded() => platform.Run(() =>
    {
        var folder = Path.Combine(Path.GetTempPath(), "MarkdownLocalizationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var manager = I18nManager.Instance;
        var plugin = new JsonLangPlugin { ResourceFolder = folder };
        manager.Register(plugin, new CultureInfo("en-US"));
        var viewer = new MarkdownViewer();
        var window = new Window { Content = viewer };
        try
        {
            window.Show();
            foreach (var value in new[] { "Host copy", "Changed copy" })
            {
                // 模拟宿主重新读取自己的输出资源，不注册或复制库的默认 JSON。
                File.WriteAllText(Path.Combine(folder, "en-US.json"), JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["language"] = "English", ["description"] = "Host", ["cultureName"] = "en-US",
                    [MarkdownL.CopyRenderedText] = value
                }));
                plugin.Load(new CultureInfo("en-US"));
                manager.AddResource();
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(value, Assert.IsType<MenuItem>(viewer.ContextMenu!.Items[0]).Header);
                Assert.Equal("Copy selected text", MarkdownLocalization.GetResource(MarkdownL.CopySelectedText));
            }
        }
        finally
        {
            window.Close();
            manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
            Directory.Delete(folder, true);
        }
    });
}
