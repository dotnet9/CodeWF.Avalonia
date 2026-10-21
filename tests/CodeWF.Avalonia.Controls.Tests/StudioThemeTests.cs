using Avalonia.Media;
using Avalonia.Styling;
using CodeWF.Avalonia.Controls.Themes;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class StudioThemeTests
{
    public static TheoryData<ThemeVariant> Variants
    {
        get
        {
            var data = new TheoryData<ThemeVariant>();
            foreach (var palette in StudioPalettes.All)
            foreach (var dark in new[] { false, true }) data.Add(StudioTheme.GetVariant(palette.Key, dark));
            return data;
        }
    }

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
    public void AllPalettesProvideNativeControlColors()
    {
        var theme = new StudioTheme();
        Assert.True(theme.TryGetResource("SemiColorPrimary", ThemeVariant.Light, out _));
        Assert.True(theme.TryGetResource("SemiColorPrimary", ThemeVariant.Dark, out _));
        Assert.True(theme.TryGetResource("StudioSurface", ThemeVariant.Dark, out _));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void SemiControlAliasesUseTheSelectedPalette(ThemeVariant variant)
    {
        var theme = new StudioTheme();
        ISolidColorBrush Get(string key)
        {
            Assert.True(theme.TryGetResource(key, variant, out var value), key);
            return Assert.IsAssignableFrom<ISolidColorBrush>(value);
        }

        foreach (var key in new[] { "SliderTrackForeground", "SliderThumbBorderBrush", "SliderThumbPointeroverBorderBrush",
                     "TextBoxFocusBorderBrush", "ComboBoxSelectorFocusBorderBrush", "CalendarDatePickerFocusBorderBrush",
                     "CheckBoxPointeroverBorderBrush", "RadioButtonUncheckIconPointeroverBorderBrush", "TabItemIconSelectedForeground" })
            Assert.Equal(Get("StudioAccent").Color, Get(key).Color);
        foreach (var key in new[] { "CheckBoxCheckedDefaultBackground", "CheckBoxCheckedPointeroverBackground",
                     "ButtonSolidPrimaryBackground", "ButtonSolidPrimaryBorderBrush", "CalendarItemCalendarDayButtonSelectedBackground" })
        {
            Assert.Equal(Get("StudioPrimary").Color, Get(key).Color);
            Assert.True(Contrast(Get("StudioOnPrimary").Color, Get(key).Color) >= 4.5, key);
        }
        Assert.Equal(0.4, Get("CheckBoxCheckedDisabledBackground").Opacity);
    }

    [Fact]
    public void TextRolesAndActionGradientsStayReadableOnEveryWorkspaceSurface()
    {
        var theme = new StudioTheme();
        foreach (var palette in StudioPalettes.All.Select(item => item.Key))
        foreach (var dark in new[] { false, true })
        {
            var variant = StudioTheme.GetVariant(palette, dark);
            object Get(string key)
            {
                Assert.True(theme.TryGetResource(key, variant, out var value), variant + "/" + key);
                return value!;
            }
            Color ColorOf(string key) => Assert.IsAssignableFrom<ISolidColorBrush>(Get(key)).Color;
            foreach (var surface in new[] { "StudioBackground", "StudioSurface", "StudioRaised" })
            foreach (var foreground in new[] { "StudioText", "StudioMuted", "StudioAccent" })
            {
                var contrast = Contrast(ColorOf(foreground), ColorOf(surface));
                Assert.True(contrast >= 4.5, $"{variant}/{foreground}/{surface} = {contrast:F3}:1");
            }
            var hover = ColorOf("StudioPrimaryHover");
            var pressed = ColorOf("StudioPrimaryPressed");
            Assert.NotEqual(ColorOf("StudioPrimary"), hover);
            Assert.NotEqual(hover, pressed);
            Assert.True(Contrast(ColorOf("StudioOnPrimary"), hover) >= 4.5, variant + "/Primary/Hover");
            Assert.True(Contrast(ColorOf("StudioOnPrimary"), pressed) >= 4.5, variant + "/Primary/Pressed");
            var gradient = Assert.IsType<LinearGradientBrush>(Get("StudioActionGradient"));
            var first = gradient.GradientStops[0].Color;
            var last = gradient.GradientStops[^1].Color;
            // 除渐变端点外，也覆盖插值中的颜色，防止明亮副色使白字失去对比。
            for (var step = 0; step <= 100; step++)
            {
                var amount = step / 100d;
                byte Blend(byte a, byte b) => (byte)Math.Round(a + (b - a) * amount);
                var background = Color.FromRgb(Blend(first.R, last.R), Blend(first.G, last.G), Blend(first.B, last.B));
                Assert.True(Contrast(ColorOf("StudioOnPrimary"), background) >= 4.5, variant + "/ActionGradient/" + step);
            }
        }
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
