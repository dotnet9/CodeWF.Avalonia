using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using CodeWF.Avalonia.Controls.Themes;

namespace CodeWF.Avalonia.Controls.Converters;

/// <summary>
/// 在应用的三态主题模式与标准主题变体之间转换。
/// 将 Ursa 的主题按钮放入独立的 ThemeVariantScope 后使用，避免按钮覆盖应用的配色变体。
/// </summary>
public sealed class StudioThemeModeConverter : IValueConverter
{
    public static StudioThemeModeConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StudioThemeMode.Light or (int)StudioThemeMode.Light => ThemeVariant.Light,
        StudioThemeMode.Dark or (int)StudioThemeMode.Dark => ThemeVariant.Dark,
        StudioThemeMode.System or (int)StudioThemeMode.System => ThemeVariant.Default,
        _ => BindingOperations.DoNothing,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var mode = value switch
        {
            ThemeVariant variant when variant == ThemeVariant.Light => StudioThemeMode.Light,
            ThemeVariant variant when variant == ThemeVariant.Dark => StudioThemeMode.Dark,
            ThemeVariant variant when variant == ThemeVariant.Default => StudioThemeMode.System,
            _ => (StudioThemeMode?)null,
        };
        if (mode is null) return BindingOperations.DoNothing;
        return targetType == typeof(int) ? (object)(int)mode.Value : mode.Value;
    }
}
