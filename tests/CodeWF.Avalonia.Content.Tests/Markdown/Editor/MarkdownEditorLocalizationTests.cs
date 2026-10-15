using System.Globalization;
using System.Text.Json;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Controls.Wysiwyg;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using global::Avalonia.Controls;
using global::Avalonia.LogicalTree;
using global::Avalonia.Threading;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Editor;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownEditorLocalizationTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void ExistingEditorsAndTableMenusFollowTheSharedCulture() => platform.Run(() =>
    {
        var manager = I18nManager.Instance;
        manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
        var live = new MarkdownLiveEditorView();
        live.SetText("| A | B |\n| --- | --- |\n| 1 | 2 |");
        var source = new MarkdownEditorView();
        var insertion = new MarkdownLiveEditorView();
        var host = new StackPanel { Children = { live, source, insertion } };
        var window = new Window { Content = host };
        try
        {
            window.Show();
            var table = live.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
            var originalText = live.Text;
            foreach (var (culture, bold, menu) in new[]
            {
                ("en-US", "bold text", "Add row"),
                ("zh-CN", "粗体", "添加行"),
                ("zh-Hant", "粗體", "新增列"),
                ("ja-JP", "太字", "行を追加"),
                ("fr-FR", "bold text", "Add row")
            })
            {
                manager.Culture = new CultureInfo(culture);
                Dispatcher.UIThread.RunJobs();
                Assert.Same(table, live.GetLogicalDescendants().OfType<MarkdownTableView>().Single());
                Assert.Equal(menu, Assert.IsType<MenuItem>(table.ContextMenu!.Items[0]).Header);
                Assert.Equal(bold, source.Localizer.Get(MarkdownEditorText.BoldPlaceholder));
                source.SetText(string.Empty);
                source.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();
                Assert.Contains(bold, source.Text, StringComparison.Ordinal);
                insertion.SetText(string.Empty);
                insertion.FocusBlock(0);
                insertion.ExecuteAsync(MarkdownEditorAction.Bold).GetAwaiter().GetResult();
                Assert.Contains(bold, insertion.Text, StringComparison.Ordinal);
                Assert.Equal(originalText, live.Text);
            }
            host.Children.Remove(live);
            manager.Culture = new CultureInfo("zh-CN");
            host.Children.Add(live);
            Dispatcher.UIThread.RunJobs();
            var attachedTable = live.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
            Assert.Equal("添加行", Assert.IsType<MenuItem>(attachedTable.ContextMenu!.Items[0]).Header);
        }
        finally
        {
            window.Close();
            manager.Register(new JsonLangPlugin { ScanResourceFolder = false }, new CultureInfo("en-US"));
        }
    });

    [Fact]
    public void HostOverridesRefreshExistingMenusAndKeepEmbeddedFallbacks() => platform.Run(() =>
    {
        var folder = Path.Combine(Path.GetTempPath(), "MarkdownEditorLocalizationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var manager = I18nManager.Instance;
        var plugin = new JsonLangPlugin { ResourceFolder = folder };
        manager.Register(plugin, new CultureInfo("en-US"));
        var live = new MarkdownLiveEditorView();
        live.SetText("| A | B |\n| --- | --- |\n| 1 | 2 |");
        var window = new Window { Content = live };
        try
        {
            window.Show();
            var table = live.GetLogicalDescendants().OfType<MarkdownTableView>().Single();
            foreach (var value in new[] { "Host row", "Changed row" })
            {
                File.WriteAllText(Path.Combine(folder, "en-US.json"), JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["language"] = "English", ["description"] = "Host", ["cultureName"] = "en-US",
                    [MarkdownL.EditorTableAddRow] = value,
                    [MarkdownL.EditorBoldPlaceholder] = value
                }));
                plugin.Load(new CultureInfo("en-US"));
                manager.AddResource();
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(value, Assert.IsType<MenuItem>(table.ContextMenu!.Items[0]).Header);
                Assert.Equal("Remove last row", Assert.IsType<MenuItem>(table.ContextMenu.Items[1]).Header);
                Assert.Equal(value, new MarkdownEditorLocalizer().Get(MarkdownEditorText.BoldPlaceholder));
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
