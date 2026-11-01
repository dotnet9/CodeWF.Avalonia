using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Toolkit.Lang;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Headless;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Markdown.Rendering;

[Collection("AvaloniaPlatform")]
public sealed class MarkdownViewerContextMenuTests(AvaloniaPlatformFixture platform)
{
    [Fact]
    public void DefaultMenusCanBeDisabledAndCustomMenuSurvivesCultureAndRemount() => platform.Run(() =>
    {
        var viewer = new MarkdownViewer
        {
            Markdown = "Paragraph",
            Template = new global::Avalonia.Controls.Templates.FuncControlTemplate<MarkdownViewer>((_, scope) =>
            {
                var host = new StackPanel();
                scope.Register("PART_DocumentHost", host);
                return host;
            })
        };
        var window = new Window { Width = 640, Height = 420, Content = viewer };
        window.Styles.Add(new global::Semi.Avalonia.SemiTheme());
        window.Styles.Add(new MarkdownFullThemes());
        var manager = I18nManager.Instance;
        var originalCulture = manager.Culture;
        void Render()
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            do
            {
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
                if (viewer.GetVisualDescendants().OfType<SelectableTextBlock>().Any()) return;
                Thread.Sleep(10);
            } while (DateTime.UtcNow < deadline);
            throw new TimeoutException("Viewer paragraph did not render");
        }
        SelectableTextBlock Text() => viewer.GetVisualDescendants().OfType<SelectableTextBlock>().Single();
        void RightClick()
        {
            var point = Text().TranslatePoint(new Point(12, Text().Bounds.Height / 2), window)!.Value;
            window.MouseMove(point); window.MouseDown(point, global::Avalonia.Input.MouseButton.Right);
            window.MouseUp(point, global::Avalonia.Input.MouseButton.Right); Render();
        }
        window.Show();
        try
        {
            Render();
            RightClick();
            Assert.True(Text().ContextMenu?.IsOpen);
            Text().ContextMenu!.Close();
            viewer.IsDefaultContextMenuEnabled = false; Render();
            Assert.Null(viewer.ContextMenu);
            Assert.Null(Text().ContextMenu);
            Assert.Null(Text().ContextFlyout);
            var custom = new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "Custom action" } } };
            viewer.ContextMenu = custom;
            manager.Culture = new System.Globalization.CultureInfo("en-US"); Render();
            window.Content = null; window.Content = viewer; Render();
            Assert.Same(custom, viewer.ContextMenu);
            RightClick(); Assert.True(custom.IsOpen); custom.Close();
            viewer.ContextMenu = null;
            viewer.IsDefaultContextMenuEnabled = true; Render();
            RightClick(); Assert.True(Text().ContextMenu?.IsOpen); Text().ContextMenu!.Close();
        }
        finally { window.Close(); manager.Culture = originalCulture; }
    });
}
