using Avalonia.Media;
using Avalonia.Styling;
using CodeWF.Avalonia.Controls.Themes;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class StudioThemeTests
{
    public static TheoryData<ThemeVariant> Variants => new()
    {
        StudioTheme.DotNetLight, StudioTheme.DotNetDark,
        StudioTheme.AuroraLight, StudioTheme.AuroraDark,
        StudioTheme.EmberLight, StudioTheme.EmberDark,
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public void PaletteHasReadableTextAndCompleteWorkspaceResources(ThemeVariant variant)
    {
        var theme = new StudioTheme();
        Color Get(string key)
        {
            Assert.True(theme.TryGetResource(key, variant, out var value), key);
            return Assert.IsType<SolidColorBrush>(value).Color;
        }

        var background = Get("StudioSurface");
        Assert.True(Contrast(Get("StudioText"), background) >= 4.5);
        Assert.True(Contrast(Get("StudioAccent"), background) >= 4.5);
        Assert.True(Contrast(Get("StudioMuted"), background) >= 4.5);
        Assert.True(theme.TryGetResource("StudioPropertyGridTheme", variant, out var propertyTheme));
        var propertyPalette = Assert.IsType<global::Avalonia.Controls.ResourceDictionary>(propertyTheme);
        Assert.True(Contrast(Assert.IsAssignableFrom<ISolidColorBrush>(propertyPalette["PgSelectedText"]).Color,
            Assert.IsAssignableFrom<ISolidColorBrush>(propertyPalette["PgSelected"]).Color) >= 4.5);
        foreach (var key in new[] { "DockSurfaceEditorBrush", "DockSurfacePanelBrush", "DockTabActiveIndicatorBrush", "CodeWFWindowTitleBarForeground", "ButtonSolidPrimaryBackground" })
            Get(key);
    }

    [Fact]
    public void ExistingPalettesKeepTheirSemiTemplateColors()
    {
        var theme = new StudioTheme();
        Assert.False(theme.TryGetResource("SemiColorPrimary", ThemeVariant.Light, out _));
        Assert.False(theme.TryGetResource("SemiColorPrimary", ThemeVariant.Dark, out _));
        Assert.True(theme.TryGetResource("StudioSurface", ThemeVariant.Dark, out _));
    }

    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte component)
        {
            var value = component / 255d;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        static double Luminance(Color color) => 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }
}
