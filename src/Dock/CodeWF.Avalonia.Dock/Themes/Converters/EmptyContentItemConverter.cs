using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CodeWF.Avalonia.Dock.Themes.Converters;

public sealed class EmptyContentItemConverter : IValueConverter
{
    private readonly int _index;

    private EmptyContentItemConverter(int index)
    {
        _index = index;
    }

    public static IValueConverter First { get; } = new EmptyContentItemConverter(0);

    public static IValueConverter Second { get; } = new EmptyContentItemConverter(1);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IList items && _index < items.Count
            ? items[_index]
            : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
