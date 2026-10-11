using System.Collections;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>RowHeight (double?) to row height: null converts to NaN (auto).</summary>
public sealed class NullableDoubleToHeightConverter : IValueConverter
{
    public static readonly NullableDoubleToHeightConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double height && double.IsFinite(height) ? height : double.NaN;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>Depth (int) to name-cell indent Thickness.</summary>
public sealed class DepthToThicknessConverter : IValueConverter
{
    public static readonly DepthToThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => new Thickness(2 + 16 * (value is int depth ? depth : 0), 0, 8, 0);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>Depth (int) to indent guide indices (0..depth-1); empty when depth is 0.</summary>
public sealed class DepthToGuidesConverter : IValueConverter
{
    public static readonly DepthToGuidesConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int depth && depth > 0 ? Enumerable.Range(0, depth) : Array.Empty<int>();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>bool to chevron glyph (true = ▾ expanded, false = ▸ collapsed).</summary>
public sealed class BoolToChevronConverter : IValueConverter
{
    public static readonly BoolToChevronConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "▾" : "▸";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>null object to true (default instance); <see cref="InverseInstance"/>: null to false.</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public static readonly NullToBoolConverter Instance = new(inverse: false);
    public static readonly NullToBoolConverter InverseInstance = new(inverse: true);

    private readonly bool _inverse;

    private NullToBoolConverter(bool inverse) => _inverse = inverse;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value is null;
        return _inverse ? !isNull : isNull ? (object)true : false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}
