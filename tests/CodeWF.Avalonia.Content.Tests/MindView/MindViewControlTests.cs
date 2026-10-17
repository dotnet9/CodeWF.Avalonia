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
            view.Root = null;
            view.Root = MarkdownMindMapDocument.Parse(source);
            Assert.Equal(.9, view.Zoom, 4);
            Assert.True(view.Root.Children[0].IsCollapsed);
            Assert.Equal("正文", view.Root.Children[0].Children[0].FullText);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void OutlineRevealExpandsAncestorsCentersSelectionAndKeepsThemeTokens() => platform.Run(() =>
    {
        var root = MarkdownMindMapDocument.Parse("# 文档\n\n## 一节\n\n### 小节\n\n正文\n\n## 二节\n\n正文");
        var target = root.Children[0].Children[0];
        var view = new MarkdownMindMapView { Root = root, CanvasBrush = Brushes.Beige, CardBrush = Brushes.AliceBlue, TextBrush = Brushes.DarkBlue, AccentColor = Colors.Crimson };
        var window = new Window { Width = 1000, Height = 700, Content = view };
        window.Show();
        try
        {
            view.CollapseAll();
            var collapsedCount = view.VisibleNodeCount;
            Assert.True(view.RevealNode(target.Id));
            Assert.Same(target, view.SelectedNode);
            Assert.False(root.Children[0].IsCollapsed);
            Assert.True(view.VisibleNodeCount > collapsedCount);
            Assert.Equal(3, target.HeadingLevel);
            window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            var card = view.GetVisualDescendants().OfType<Border>().Single(b => ToolTip.GetTip(b)?.ToString() == "小节");
            var center = card.TranslatePoint(new global::Avalonia.Point(card.Bounds.Width / 2, card.Bounds.Height / 2), view)!.Value;
            Assert.InRange(center.X, view.Bounds.Width / 2 - 1, view.Bounds.Width / 2 + 1);
            Assert.Contains(view.GetVisualDescendants().OfType<Panel>(), panel => panel.Background == Brushes.Beige);
            view.ClearSelection(); Assert.Null(view.SelectedNode);
            Assert.Equal("正文", target.Children[0].FullText);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void LongDocumentSupportsFoldRevealAndResizeWithoutLosingTheFocusedNode() => platform.Run(() =>
    {
        var markdown = "# 长文档\n\n" + string.Join("\n\n", Enumerable.Range(1, 120).Select(i => $"## 章节 {i}\n\n这是一段需要在导图中截取摘要、并在点击后保留完整正文的内容 {i}。\n\n- 列表项 {i}\n- 第二项 {i}"));
        var root = MarkdownMindMapDocument.Parse(markdown);
        var view = new MarkdownMindMapView { Root = root, EnableMotion = false };
        var window = new Window { Width = 1280, Height = 820, Content = view }; window.Show();
        try
        {
            Assert.Equal(481, view.VisibleNodeCount);
            Assert.True(view.RealizedNodeCount < 100);
            view.CollapseAll(); Assert.Equal(121, view.VisibleNodeCount);
            var last = root.Children.Last(); view.RevealNode(last.Id); window.UpdateLayout();
            window.Width = 980; window.Height = 640; window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            var card = view.GetVisualDescendants().OfType<Border>().Single(b => ToolTip.GetTip(b)?.ToString() == last.FullText);
            var center = card.TranslatePoint(new global::Avalonia.Point(card.Bounds.Width / 2, card.Bounds.Height / 2), view)!.Value;
            Assert.InRange(center.X, view.Bounds.Width / 2 - 1, view.Bounds.Width / 2 + 1);
            Assert.InRange(center.Y, view.Bounds.Height / 2 - 1, view.Bounds.Height / 2 + 1);
            Assert.Same(last, view.SelectedNode);
            Assert.Equal(120, root.Children.Count);
        }
        finally { window.Close(); }
    });

    [Fact]
    public void AutomaticFitKeepsRootVisibleWhenWindowNarrows() => platform.Run(() =>
    {
        var view = new MarkdownMindMapView { Root = MarkdownMindMapDocument.Parse("# 文档\n\n## 一\n\n### 二\n\n正文"), EnableMotion = false };
        var window = new Window { Width = 1280, Height = 820, Content = view }; window.Show();
        try
        {
            view.Fit(.75); window.Width = 780; window.Height = 500; window.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            var root = view.GetVisualDescendants().OfType<Border>().Single(b => ToolTip.GetTip(b)?.ToString() == view.Root!.FullText);
            var point = root.TranslatePoint(default, view)!.Value;
            Assert.True(point.X >= 0 && point.Y >= 0);
            Assert.Equal(60, root.Bounds.Height);
        }
        finally { window.Close(); }
    });

    [Theory]
    [InlineData("zh-CN", "中心主题")]
    [InlineData("en-US", "Center topic")]
    public void LocalizedResourcesAreEmbeddedInThePackage(string culture, string expected) =>
        Assert.Equal(expected, MindViewLocalization.GetResource(MindViewL.CenterTopicPlaceholder, culture));
}
