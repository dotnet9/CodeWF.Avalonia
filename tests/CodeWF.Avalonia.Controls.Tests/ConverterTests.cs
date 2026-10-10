using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.Controls.Themes.Converters;

namespace CodeWF.Avalonia.Controls.Tests;

public sealed class ConverterTests
{
    [Fact]
    public void StatusBackgroundConverterFallsBackForUnknownKind()
    {
        var converter = new StatusLabelKindBackgroundConverter();

        var result = Assert.IsType<SolidColorBrush>(converter.Convert((StatusLabelKind)999, typeof(IBrush), null, null!));

        Assert.Equal(Color.Parse("#E6F7FF"), result.Color);
    }

    [Fact]
    public void OneWayConvertersReturnUnsetValueFromConvertBack()
    {
        var converters = new IValueConverter[]
        {
            new StatusBadgeLeftTextBorderCornerRadiusConverter(),
            new StatusCardKindForegroundConverter(),
            new StatusLabelKindBackgroundConverter(),
            new TrapezoidShapedTabItemPaddingConverter(),
            new CornerRadiusInsetConverter()
        };

        foreach (var converter in converters)
        {
            var result = converter.ConvertBack(null, typeof(object), null, null!);

            Assert.Same(AvaloniaProperty.UnsetValue, result);
        }
    }
}
