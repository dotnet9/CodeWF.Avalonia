using Avalonia;
using Avalonia.Data.Converters;
using Dock.Avalonia.Controls;
using System;
using System.Globalization;

namespace CodeWF.Avalonia.Dock.Converters;

[Obsolete("The built-in themes do not use this converter. Use a dedicated application converter instead.")]
public class TrapezoidShapedDocumentTabStripItemPaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentTabStripItem tabItem)
        {
            return new Thickness(0);
        }

        var padding = tabItem.Padding;
        return new Thickness(padding.Left * 2, padding.Top, padding.Right * 2, padding.Bottom);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter only supports one-way conversion.");
    }
}
