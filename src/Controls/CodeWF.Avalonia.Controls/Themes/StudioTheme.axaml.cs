using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// An optional studio appearance layered after Semi and CodeWF themes.
/// Uses existing control templates; the six variants inherit Semi Light/Dark.
/// </summary>
public sealed class StudioTheme : Styles
{
    public static ThemeVariant DotNetLight { get; } = new("DotNetLight", ThemeVariant.Light);
    public static ThemeVariant DotNetDark { get; } = new("DotNetDark", ThemeVariant.Dark);
    public static ThemeVariant AuroraLight { get; } = new("AuroraLight", ThemeVariant.Light);
    public static ThemeVariant AuroraDark { get; } = new("AuroraDark", ThemeVariant.Dark);
    public static ThemeVariant EmberLight { get; } = new("EmberLight", ThemeVariant.Light);
    public static ThemeVariant EmberDark { get; } = new("EmberDark", ThemeVariant.Dark);

    public StudioTheme()
    {
        AvaloniaXamlLoader.Load(this);
        // Default resources also support the existing Semi palettes.
        Resources.ThemeDictionaries[ThemeVariant.Light] = Palette(false, "#512BD4", "#7355DE", false, false);
        Resources.ThemeDictionaries[ThemeVariant.Dark] = Palette(true, "#A78BFA", "#7654EA", false, false);
        Resources.ThemeDictionaries[DotNetLight] = Palette(false, "#512BD4", "#7355DE", false);
        Resources.ThemeDictionaries[DotNetDark] = Palette(true, "#A78BFA", "#7654EA", false);
        Resources.ThemeDictionaries[AuroraLight] = Palette(false, "#6745D6", "#1AAFA9", false);
        Resources.ThemeDictionaries[AuroraDark] = Palette(true, "#AE96FF", "#26D4CC", false);
        Resources.ThemeDictionaries[EmberLight] = Palette(false, "#A94F36", "#D09566", true);
        Resources.ThemeDictionaries[EmberDark] = Palette(true, "#E6A58F", "#E9BD87", true);
    }

    private static ResourceDictionary Palette(bool dark, string accent, string secondary, bool warm, bool overrideTemplates = true)
    {
        var background = dark ? (warm ? "#191512" : "#10111C") : (warm ? "#F6F0E9" : "#F4F3FA");
        var surface = dark ? (warm ? "#221D19" : "#191A2B") : "#FFFFFF";
        var raised = dark ? (warm ? "#2D251F" : "#22243A") : (warm ? "#EEE5DB" : "#EEECF8");
        var border = dark ? (warm ? "#49382E" : "#343650") : (warm ? "#DDCABC" : "#DDD9ED");
        var text = dark ? "#F3F0FC" : "#242036";
        var muted = dark ? "#AAA5C0" : "#6E6684";
        var dictionary = new ResourceDictionary();
        void Brush(string key, string color) => dictionary[key] = new SolidColorBrush(Color.Parse(color));
        Brush("StudioBackground", background);
        Brush("StudioSurface", surface);
        Brush("StudioRaised", raised);
        Brush("StudioBorder", border);
        Brush("StudioText", text);
        Brush("StudioMuted", muted);
        Brush("StudioAccent", accent);
        Brush("StudioSecondary", secondary);
        // Flat overrides are copied by PropertyGrid; no dependency on its assembly is needed.
        dictionary["StudioPropertyGridTheme"] = new ResourceDictionary
        {
            ["PgBackground"] = dictionary["StudioSurface"],
            ["PgToolbarBackground"] = dictionary["StudioSurface"],
            ["PgGroupBackground"] = dictionary["StudioRaised"],
            ["PgForeground"] = dictionary["StudioText"],
            ["PgSecondaryForeground"] = dictionary["StudioMuted"],
            ["PgBorder"] = dictionary["StudioBorder"],
            ["PgLine"] = dictionary["StudioBorder"],
            ["PgHover"] = dictionary["StudioRaised"],
            ["PgAccent"] = dictionary["StudioAccent"],
            ["PgSelected"] = new SolidColorBrush(Color.Parse(warm ? "#A94F36" : "#512BD4")),
            ["PgSelectedSoft"] = dictionary["StudioRaised"],
            ["PgSelectedText"] = Brushes.White,
            ["PgEditorBackground"] = dictionary["StudioSurface"],
        };
        dictionary["StudioGradient"] = new LinearGradientBrush
        {
            StartPoint = new global::Avalonia.RelativePoint(0, 0, global::Avalonia.RelativeUnit.Relative),
            EndPoint = new global::Avalonia.RelativePoint(1, 1, global::Avalonia.RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new(Color.Parse(warm ? "#BD674D" : secondary is "#26D4CC" or "#1AAFA9" ? "#8968FF" : "#512BD4"), 0),
                new(Color.Parse(secondary), 1),
            },
        };
        if (!overrideTemplates) return dictionary;
        Brush("CodeWFWindowBackground", background);
        Brush("CodeWFWindowForeground", text);
        Brush("CodeWFWindowTitleBarBackground", background);
        Brush("CodeWFWindowTitleBarForeground", text);
        foreach (var key in new[] { "DockSurfaceWorkbenchBrush", "DockSurfaceEditorBrush" }) Brush(key, background);
        foreach (var key in new[] { "DockSurfaceSidebarBrush", "DockSurfacePanelBrush", "DockSurfaceHeaderBrush", "DockTabActiveBackgroundBrush", "DockDocumentTabStripBackgroundBrush", "DockDocumentTabStripMaskBrush", "CodeWFDockPanelBackgroundBrush" }) Brush(key, surface);
        foreach (var key in new[] { "DockTabHoverBackgroundBrush", "DockTabBackgroundBrush", "DockTabCloseHoverBackgroundBrush", "DockSplitterIdleBrush" }) Brush(key, raised);
        foreach (var key in new[] { "DockBorderSubtleBrush", "DockBorderStrongBrush", "DockSeparatorBrush", "DockDocumentContentBorderBrush", "CodeWFDockPanelBorderBrush", "CodeWFDockInactiveBorderBrush" }) Brush(key, border);
        foreach (var key in new[] { "DockTabActiveIndicatorBrush", "DockTargetIndicatorBrush", "DockSplitterHoverBrush", "DockSplitterDragBrush", "DockSurfaceHeaderActiveBrush" }) Brush(key, accent);
        foreach (var key in new[] { "DockTabSelectedForegroundBrush", "DockTabActiveForegroundBrush", "DockDocumentTabSelectedForegroundBrush", "DockDocumentTabPointerOverForegroundBrush" }) Brush(key, text);
        Brush("DockTabForegroundBrush", muted);
        // Override the template-facing keys as well: Semi aliases use StaticResource.
        foreach (var key in new[] { "WindowDefaultBackground", "SemiColorBg0", "SemiColorBg1" }) Brush(key, background);
        foreach (var key in new[] { "SemiColorBg2", "SemiColorBg3", "SemiColorFill0", "ButtonDefaultBackground" }) Brush(key, surface);
        foreach (var key in new[] { "SemiColorFill1", "SemiColorFill2", "ButtonDefaultPointeroverBackground", "ButtonDefaultPressedBackground" }) Brush(key, raised);
        foreach (var key in new[] { "SemiColorBorder", "TabControlSeparatorBorderBrush", "ButtonOutlineBorderBrush" }) Brush(key, border);
        foreach (var key in new[] { "WindowDefaultForeground", "SemiColorText0", "SemiColorText1" }) Brush(key, text);
        foreach (var key in new[] { "SemiColorText2", "SemiColorText3" }) Brush(key, muted);
        foreach (var key in new[] { "SemiColorPrimary", "SemiColorInformation", "ButtonDefaultPrimaryForeground" }) Brush(key, accent);
        foreach (var key in new[] { "RadioButtonCheckIconDefaultBackground", "RadioButtonCheckIconPointeroverBackground", "RadioButtonCheckIconPressedBackground", "RadioButtonCheckIconDefaultBorderBrush", "RadioButtonCheckIconPointeroverBorderBrush", "RadioButtonCheckIconPressedBorderBrush", "RadioButtonButtonCheckedForeground", "TabItemButtonHeaderSelectedForeground", "TabItemLinePipeSelectedBackground", "ProgressBarIndicatorBrush" }) Brush(key, accent);
        Brush("ProgressBarPrimaryForeground", accent);
        foreach (var key in new[] { "SemiColorPrimaryLight", "SemiColorInformationLight" }) Brush(key, raised);
        foreach (var key in new[] { "ButtonSolidPrimaryBackground", "ButtonSolidPrimaryPointeroverBackground", "ButtonSolidPrimaryPressedBackground" }) Brush(key, warm ? "#A94F36" : "#512BD4");
        dictionary["SystemAccentColor"] = Color.Parse(accent);
        return dictionary;
    }
}
