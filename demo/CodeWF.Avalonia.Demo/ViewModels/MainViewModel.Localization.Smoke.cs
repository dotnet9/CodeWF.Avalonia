using System.Globalization;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Markdown;
using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.ViewModels;

public partial class MainViewModel
{
    /// <summary>在真实桌面宿主中复用同一个 Markdown 控件，验证 AOT 下的语言刷新与英文回退。</summary>
    private async Task SmokeMarkdownLocalizationAsync(CancellationToken cancellation)
    {
        var host = await AttachSmokeHostAsync(cancellation) ?? throw new InvalidOperationException("Localization smoke needs a native window.");
        var original = global::CodeWF.Toolkit.Lang.I18nManager.Instance.Culture;
        try
        {
            var viewer = new MarkdownViewer { Markdown = "Paragraph\n\n```text\nNative AOT\n```" };
            host.Content = viewer;
            var renderedLabels = new HashSet<string>();
            foreach (var culture in new[] { "en-US", "zh-CN", "ja-JP", "zh-Hant-TW", "fr-FR", "en-US" })
            {
                global::CodeWF.Toolkit.Lang.I18nManager.Instance.Culture = new CultureInfo(culture);
                var expected = MarkdownLocalization.GetResource(MarkdownL.Copy);
                var deadline = DateTime.UtcNow.AddSeconds(8);
                // 12.2.29 起代码块工具条为图标钮（Content=Path），操作文案改挂在 ToolTip 上；
                // 全量重渲染（含框架复用路径）后图标钮的提示应随文化刷新。
                while (!viewer.GetVisualDescendants().OfType<Button>().Any(button =>
                           Equals(ToolTip.GetTip(button) as string, expected) || Equals(button.Content, expected)))
                {
                    await Task.Delay(30, cancellation);
                    if (DateTime.UtcNow >= deadline) throw new TimeoutException("Markdown button did not refresh: " + culture);
                }
                var header = (viewer.ContextMenu!.Items[0] as MenuItem)?.Header as string;
                if (header != MarkdownLocalization.GetResource(MarkdownL.CopyRenderedText) || header == MarkdownL.CopyRenderedText)
                    throw new InvalidOperationException("Markdown context menu did not refresh: " + culture);
                var paragraph = viewer.GetVisualDescendants().OfType<SelectableTextBlock>().First(text => text.ContextMenu?.Items.Count == 2);
                if (!Equals((paragraph.ContextMenu!.Items[0] as MenuItem)?.Header, MarkdownLocalization.GetResource(MarkdownL.CopySelectedText)))
                    throw new InvalidOperationException("Selection context menu did not refresh: " + culture);
                renderedLabels.Add(header);
            }
            if (renderedLabels.Count != 4) throw new InvalidOperationException("Markdown embedded cultures did not produce four translations.");
            Logger.Info("Markdown localization smoke: same live control, menus and code buttons, four translations and fallback passed");
        }
        finally
        {
            host.Content = null;
            if (host.Parent is Panel panel) panel.Children.Remove(host);
            global::CodeWF.Toolkit.Lang.I18nManager.Instance.Culture = original;
        }
    }
}
