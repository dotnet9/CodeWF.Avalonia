using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 基于 Semi 控件模板提供可选的 Studio 外观。
/// 配色资源位于 AXAML 主题字典，通过显式主题变体加载。
/// </summary>
public sealed class StudioTheme : Styles
{
    public static ThemeVariant DotNetLight { get; } = new(nameof(DotNetLight), ThemeVariant.Light);
    public static ThemeVariant DotNetDark { get; } = new(nameof(DotNetDark), ThemeVariant.Dark);
    public static ThemeVariant AuroraLight { get; } = new(nameof(AuroraLight), ThemeVariant.Light);
    public static ThemeVariant AuroraDark { get; } = new(nameof(AuroraDark), ThemeVariant.Dark);
    public static ThemeVariant EmberLight { get; } = new(nameof(EmberLight), ThemeVariant.Light);
    public static ThemeVariant EmberDark { get; } = new(nameof(EmberDark), ThemeVariant.Dark);
    public static ThemeVariant AquaticLight { get; } = new(nameof(AquaticLight), ThemeVariant.Light);
    public static ThemeVariant AquaticDark { get; } = new(nameof(AquaticDark), ThemeVariant.Dark);
    public static ThemeVariant DesertLight { get; } = new(nameof(DesertLight), ThemeVariant.Light);
    public static ThemeVariant DesertDark { get; } = new(nameof(DesertDark), ThemeVariant.Dark);
    public static ThemeVariant DuskLight { get; } = new(nameof(DuskLight), ThemeVariant.Light);
    public static ThemeVariant DuskDark { get; } = new(nameof(DuskDark), ThemeVariant.Dark);
    public static ThemeVariant NightSkyLight { get; } = new(nameof(NightSkyLight), ThemeVariant.Light);
    public static ThemeVariant NightSkyDark { get; } = new(nameof(NightSkyDark), ThemeVariant.Dark);
    public static ThemeVariant WeftLight { get; } = new(nameof(WeftLight), ThemeVariant.Light);
    public static ThemeVariant WeftDark { get; } = new(nameof(WeftDark), ThemeVariant.Dark);
    public static ThemeVariant GraphiteLight { get; } = new(nameof(GraphiteLight), ThemeVariant.Light);
    public static ThemeVariant GraphiteDark { get; } = new(nameof(GraphiteDark), ThemeVariant.Dark);
    public static ThemeVariant OceanLight { get; } = new(nameof(OceanLight), ThemeVariant.Light);
    public static ThemeVariant OceanDark { get; } = new(nameof(OceanDark), ThemeVariant.Dark);
    public static ThemeVariant SandLight { get; } = new(nameof(SandLight), ThemeVariant.Light);
    public static ThemeVariant SandDark { get; } = new(nameof(SandDark), ThemeVariant.Dark);
    public static ThemeVariant RoseLight { get; } = new(nameof(RoseLight), ThemeVariant.Light);
    public static ThemeVariant RoseDark { get; } = new(nameof(RoseDark), ThemeVariant.Dark);
    public static ThemeVariant IrisLight { get; } = new(nameof(IrisLight), ThemeVariant.Light);
    public static ThemeVariant IrisDark { get; } = new(nameof(IrisDark), ThemeVariant.Dark);
    public static ThemeVariant OliveLight { get; } = new(nameof(OliveLight), ThemeVariant.Light);
    public static ThemeVariant OliveDark { get; } = new(nameof(OliveDark), ThemeVariant.Dark);
    public static ThemeVariant TerracottaLight { get; } = new(nameof(TerracottaLight), ThemeVariant.Light);
    public static ThemeVariant TerracottaDark { get; } = new(nameof(TerracottaDark), ThemeVariant.Dark);

    /// <summary>获取配色与明暗模式对应的主题变体，不依赖颜色字符串判断。</summary>
    public static ThemeVariant GetVariant(StudioPalette palette, bool dark) => palette switch
    {
        StudioPalette.DotNet => dark ? DotNetDark : DotNetLight,
        StudioPalette.Aurora => dark ? AuroraDark : AuroraLight,
        StudioPalette.Ember => dark ? EmberDark : EmberLight,
        StudioPalette.Default => dark ? ThemeVariant.Dark : ThemeVariant.Light,
        StudioPalette.Aquatic => dark ? AquaticDark : AquaticLight,
        StudioPalette.Desert => dark ? DesertDark : DesertLight,
        StudioPalette.Dusk => dark ? DuskDark : DuskLight,
        StudioPalette.NightSky => dark ? NightSkyDark : NightSkyLight,
        StudioPalette.Weft => dark ? WeftDark : WeftLight,
        StudioPalette.Graphite => dark ? GraphiteDark : GraphiteLight,
        StudioPalette.Ocean => dark ? OceanDark : OceanLight,
        StudioPalette.Sand => dark ? SandDark : SandLight,
        StudioPalette.Rose => dark ? RoseDark : RoseLight,
        StudioPalette.Iris => dark ? IrisDark : IrisLight,
        StudioPalette.Olive => dark ? OliveDark : OliveLight,
        StudioPalette.Terracotta => dark ? TerracottaDark : TerracottaLight,
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    public StudioTheme() => AvaloniaXamlLoader.Load(this);
}
