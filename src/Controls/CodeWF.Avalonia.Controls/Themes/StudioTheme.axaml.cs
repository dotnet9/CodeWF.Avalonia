using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// Optional Studio appearance using Semi control templates.
/// Palette resources live in AXAML theme dictionaries, keyed by explicit variants.
/// </summary>
public sealed class StudioTheme : Styles
{
    public static ThemeVariant DotNetLight { get; } = new(nameof(DotNetLight), ThemeVariant.Light);
    public static ThemeVariant DotNetDark { get; } = new(nameof(DotNetDark), ThemeVariant.Dark);
    public static ThemeVariant AuroraLight { get; } = new(nameof(AuroraLight), ThemeVariant.Light);
    public static ThemeVariant AuroraDark { get; } = new(nameof(AuroraDark), ThemeVariant.Dark);
    public static ThemeVariant EmberLight { get; } = new(nameof(EmberLight), ThemeVariant.Light);
    public static ThemeVariant EmberDark { get; } = new(nameof(EmberDark), ThemeVariant.Dark);

    public StudioTheme() => AvaloniaXamlLoader.Load(this);
}
