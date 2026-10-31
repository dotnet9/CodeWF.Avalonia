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
        Assert.True(Contrast(Get("StudioText"), background) >= 4.5, variant + "/Text");
        // 强调色是「状态色」不是「正文字色」：原型 design/assets 里有 183 处 color:var(--accent)，
        // 多为链接、选中标签、图标与描边。九套 × 明暗对三面（bg/surface/raised）最差 3.005:1（鼠尾草浅底
        // accent 对 --raised），落在 WCAG 1.4.11（UI 组件与非文本 3:1）一档；正文两档守住 4.5（Muted 最差实测 4.63）。
        Assert.True(Contrast(Get("StudioAccent"), background) >= 3.0, variant + "/Accent");
        Assert.True(Contrast(Get("StudioMuted"), background) >= 4.5, variant + "/Muted");
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
                // Accent 是状态/强调色，用在链接、选中态、图标这类非正文位置（原型 183 处 color:var(--accent)），
                // 按 WCAG 1.4.11 的 3:1 一档守；Text/Muted 是正文角色，保持 4.5。
                // 九套 × 明暗对三面最差 3.005（鼠尾草浅底 accent 对 --raised），次差 4.483（琥珀浅底），暗色最差 8.34。
                var threshold = foreground == "StudioAccent" ? 3.0 : 4.5;
                var contrast = Contrast(ColorOf(foreground), ColorOf(surface));
                Assert.True(contrast >= threshold, $"{variant}/{foreground}/{surface} = {contrast:F3}:1");
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

    /// <summary>用户配色走运行时 Fill：状态色与静态 axaml 同源，且作为文字在 paper/surface 上达标（规则 §3/§5）。</summary>
    [Theory]
    [MemberData(nameof(Variants))]
    public void RuntimeFillCarriesReadableStatusColors(ThemeVariant variant)
    {
        var palette = StudioPalettes.All.Single(item => StudioTheme.GetVariant(item.Key, true) == variant || StudioTheme.GetVariant(item.Key, false) == variant);
        var dark = StudioTheme.GetVariant(palette.Key, true) == variant;
        var derived = StudioPaletteEngine.Derive(palette.Key, palette, dark);
        foreach (var pair in StudioPaletteEngine.DeriveExtras(palette, dark)) derived[pair.Key] = pair.Value;
        var dictionary = new global::Avalonia.Controls.ResourceDictionary();
        StudioTokens.Fill(dictionary, derived);

        foreach (var key in new[] { "SemiColorSuccess", "SemiColorWarning", "SemiColorDanger", "SemiColorDangerActive" })
        {
            var brush = Assert.IsType<SolidColorBrush>(dictionary[key]);
            Assert.Equal(derived[key], brush.Color);
        }

        foreach (var key in new[] { "SemiColorSuccess", "SemiColorWarning", "SemiColorDanger" })
        {
            var color = Assert.IsType<SolidColorBrush>(dictionary[key]).Color;
            var paper = derived["StudioPaperColor"];
            var surface = derived["StudioSurfaceColor"];
            Assert.True(Contrast(color, paper) >= 4.5, $"{variant}/{key}/paper = {Contrast(color, paper):F3}:1");
            Assert.True(Contrast(color, surface) >= 4.5, $"{variant}/{key}/surface = {Contrast(color, surface):F3}:1");
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
