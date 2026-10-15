using System.Globalization;
using Avalonia.Data;
using Avalonia.Styling;
using CodeWF.Avalonia.Controls.Converters;
using CodeWF.Avalonia.Controls.Themes;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class StudioThemeModeConverterTests
{
    [Theory]
    [InlineData(StudioThemeMode.System)]
    [InlineData(StudioThemeMode.Light)]
    [InlineData(StudioThemeMode.Dark)]
    public void RoundTripsEnumAndIndexWithoutUsingPaletteVariants(StudioThemeMode mode)
    {
        var converter = StudioThemeModeConverter.Instance;
        var variant = converter.Convert(mode, typeof(ThemeVariant), null, CultureInfo.InvariantCulture);
        Assert.Equal(mode, converter.ConvertBack(variant, typeof(StudioThemeMode), null, CultureInfo.InvariantCulture));
        Assert.Equal((int)mode, converter.ConvertBack(variant, typeof(int), null, CultureInfo.InvariantCulture));
        Assert.Equal(variant, converter.Convert((int)mode, typeof(ThemeVariant), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InvalidInputDoesNotOverwriteTheBoundMode()
    {
        var converter = StudioThemeModeConverter.Instance;
        Assert.Same(BindingOperations.DoNothing, converter.Convert(99, typeof(ThemeVariant), null, CultureInfo.InvariantCulture));
        Assert.Same(BindingOperations.DoNothing, converter.ConvertBack(StudioTheme.DotNetDark, typeof(StudioThemeMode), null, CultureInfo.InvariantCulture));
    }
}
