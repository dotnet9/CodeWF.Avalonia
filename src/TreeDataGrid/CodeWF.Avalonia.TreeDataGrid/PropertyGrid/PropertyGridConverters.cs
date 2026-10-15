using System.Collections;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>将可空行高转换为布局高度，空值使用 NaN 表示自动高度。</summary>
public sealed class NullableDoubleToHeightConverter : IValueConverter
{
    public static readonly NullableDoubleToHeightConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double height && double.IsFinite(height) ? height : double.NaN;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>将层级转换为名称单元格的缩进。</summary>
public sealed class DepthToThicknessConverter : IValueConverter
{
    public static readonly DepthToThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => new Thickness(2 + 16 * (value is int depth ? depth : 0), 0, 8, 0);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>将层级转换为引导线索引，顶层返回空序列。</summary>
public sealed class DepthToGuidesConverter : IValueConverter
{
    public static readonly DepthToGuidesConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int depth && depth > 0 ? Enumerable.Range(0, depth) : Array.Empty<int>();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>将展开状态转换为对应的展开或折叠箭头。</summary>
public sealed class BoolToChevronConverter : IValueConverter
{
    public static readonly BoolToChevronConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "▾" : "▸";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}

/// <summary>默认实例在值为空时返回 true；<see cref="InverseInstance"/> 在值为空时返回 false。</summary>
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
