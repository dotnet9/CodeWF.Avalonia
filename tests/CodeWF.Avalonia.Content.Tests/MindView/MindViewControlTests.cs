using System.Collections.ObjectModel;
using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.MindView;
using CodeWF.Avalonia.MindView.Controls;
using CodeWF.Avalonia.MindView.I18n;
using CodeWF.Avalonia.MindView.Markdown;
using CodeWF.Avalonia.MindView.Themes;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.MindView;

[Collection("AvaloniaPlatform")]
public class MindViewControlTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void EveryPaletteSupportsBothModesAndStructuralEditing() => platform.Run(() =>
    {
        var editor = new MindMapEditor { Roots = new ObservableCollection<MindMapNode> { new("中心", new MindMapNode("分支")) } };
        var window = new Window { Width = 1000, Height = 700, Content = editor };
        window.Styles.Add(new MindViewThemes());
        window.Show();
        try
        {
            foreach (var palette in Enum.GetValues<StudioPalette>()) foreach (var dark in new[] { false, true })
            {
                window.RequestedThemeVariant = StudioTheme.GetVariant(palette, dark);
                editor.AccentColor = Color.Parse(dark ? "#AE96FF" : "#6745D6");
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(dark, editor.IsDarkTheme);
                Assert.True(editor.Bounds.Width > 900);
                Assert.Contains(editor.GetVisualDescendants().OfType<Border>(), border =>
                    border.Background is ISolidColorBrush brush && brush.Color == editor.AccentColor);
                var title = editor.GetVisualDescendants().OfType<TextBox>().Single(box => box.Text == "中心");
                Assert.Equal(dark ? Colors.Black : Colors.White, Assert.IsAssignableFrom<ISolidColorBrush>(title.Foreground).Color);
            }
            var child = editor.AddChild(editor.Roots[0], "新增");
            Assert.Contains(child, editor.Roots[0].Children);
            Assert.Equal("新增", child.Title);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void ViewportRestoreKeepsZoomAndCollapsedBranchesWithoutMutatingMarkdown() => platform.Run(() =>
    {
        const string source = "# 计划\n\n## 第一节\n\n正文\n\n## 第二节\n\n另一段";
        var view = new MarkdownMindMapView { Root = MarkdownMindMapDocument.Parse(source) };
        var window = new Window { Width = 1000, Height = 700, Content = view };
        window.Show();
        try
        {
            view.SetZoom(.9);
            view.ToggleCollapse(view.Root!.Children[0]);
            var state = view.CaptureViewport();
            view.Root = MarkdownMindMapDocument.Parse("# 演示\n\n正文");
            view.RestoreViewport(state, onNextRoot: true);
            view.Root = MarkdownMindMapDocument.Parse(source);
            Assert.Equal(.9, view.Zoom, 4);
            Assert.True(view.Root.Children[0].IsCollapsed);
            Assert.Equal("正文", view.Root.Children[0].Children[0].FullText);
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData("zh-CN", "中心主题")]
    [InlineData("en-US", "Center topic")]
    public void LocalizedResourcesAreEmbeddedInThePackage(string culture, string expected) =>
        Assert.Equal(expected, MindViewLocalization.GetResource(MindViewL.CenterTopicPlaceholder, culture));
}
