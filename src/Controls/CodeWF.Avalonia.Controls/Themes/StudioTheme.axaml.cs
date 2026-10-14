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
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    };

    public StudioTheme() => AvaloniaXamlLoader.Load(this);
}
