using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Themes;
using Semi.Avalonia;
using Xunit;

namespace CodeWF.Avalonia.Content.Tests.Controls;

[Collection("AvaloniaPlatform")]
public sealed class TrapezoidTabThemeTests(AvaloniaPlatformFixture platform)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitTabContainersShowTheSelectedShapeAndReadableHeaders(bool strip) => platform.Run(() =>
    {
        var window = new Window { Width = 600, Height = 300 };
        window.Styles.Add(new SemiTheme());
        window.Styles.Add(new ACSemiTheme());
        var tabs = strip ? (SelectingItemsControl)new TabStrip() : new TabControl();
        tabs.Theme = (ControlTheme)window.FindResource(strip ? "TrapezoidShapedTabStrip" : "TrapezoidShapedTabControl")!;
        for (var index = 0; index < 3; index++)
            tabs.Items.Add(strip ? new TabStripItem { Content = "Header " + index }
                : new TabItem { Header = "Header " + index, Content = new TextBlock { Text = "Body " + index } });
        tabs.SelectedIndex = 1;
        window.Content = tabs;
        try
        {
            window.Show();
            window.UpdateLayout();
            var shapes = tabs.GetVisualDescendants().OfType<TrapezoidShapedTabItemBorder>().ToArray();
            Assert.Equal(3, shapes.Length);
            var selected = Assert.Single(shapes, shape => shape.IsVisible);
            Assert.True(selected.Bounds.Height >= 24);
            Assert.True(selected.Bounds.Width >= 60);
            tabs.SelectedIndex = 2;
            window.UpdateLayout();
            Assert.False(selected.IsVisible);
            Assert.Single(shapes, shape => shape.IsVisible);
        }
        finally { window.Close(); }
    });
}
