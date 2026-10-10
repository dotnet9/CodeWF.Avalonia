using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace CodeWF.Avalonia.DataGrid;

public sealed class DataGridNaturalSortComparer : IDataGridSortDirectionAwareComparer
{
    private static readonly CompareInfo CompareInfo = CultureInfo.CurrentCulture.CompareInfo;
    private readonly ConcurrentDictionary<Type, PropertyInfo?[]> _propertyPathCache = new();
    private readonly string[] _pathSegments;

    public DataGridNaturalSortComparer(string propertyPath)
    {
        _pathSegments = propertyPath.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;

    public int Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        return CompareValues(GetValue(x), GetValue(y));
    }

    private object? GetValue(object source)
    {
        if (_pathSegments.Length == 0)
        {
            return source;
        }

        object? value = source;
        foreach (var property in GetPropertyPath(source.GetType()))
        {
            if (value is null || property is null)
            {
                return null;
            }

            value = property.GetValue(value);
        }

        return value;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "DataGrid sort paths are runtime view-model property paths.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "DataGrid sort paths are runtime view-model property paths.")]
    private PropertyInfo?[] GetPropertyPath(Type sourceType)
    {
        return _propertyPathCache.GetOrAdd(sourceType, static (type, segments) =>
        {
            var properties = new PropertyInfo?[segments.Length];
            var currentType = type;

            for (var i = 0; i < segments.Length; i++)
            {
                var property = currentType.GetProperty(
                    segments[i],
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                properties[i] = property;
                if (property is null)
                {
                    break;
                }

                currentType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            }

            return properties;
        }, _pathSegments);
    }

    private static int CompareValues(object? x, object? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is string xText && y is string yText)
        {
            return CompareText(xText, yText);
        }

        if (x.GetType() == y.GetType() && x is IComparable comparable)
        {
            return comparable.CompareTo(y);
        }

        if (x is IComparable xComparable)
        {
            try
            {
                return xComparable.CompareTo(y);
            }
            catch (ArgumentException)
            {
            }
        }

        return CompareText(
            Convert.ToString(x, CultureInfo.CurrentCulture) ?? string.Empty,
            Convert.ToString(y, CultureInfo.CurrentCulture) ?? string.Empty);
    }

    public static int CompareText(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xIndex = 0;
        var yIndex = 0;
        while (xIndex < x.Length && yIndex < y.Length)
        {
            var xIsDigit = char.IsDigit(x[xIndex]);
            var yIsDigit = char.IsDigit(y[yIndex]);
            if (xIsDigit && yIsDigit)
            {
                var comparison = CompareNumberRun(x, ref xIndex, y, ref yIndex);
                if (comparison != 0)
                {
                    return comparison;
                }

                continue;
            }

            var textComparison = CompareTextRun(x, ref xIndex, y, ref yIndex);
            if (textComparison != 0)
            {
                return textComparison;
            }
        }

        return x.Length.CompareTo(y.Length);
    }

    private static int CompareTextRun(string x, ref int xIndex, string y, ref int yIndex)
    {
        var xStart = xIndex;
        var yStart = yIndex;

        while (xIndex < x.Length && !char.IsDigit(x[xIndex]))
        {
            xIndex++;
        }

        while (yIndex < y.Length && !char.IsDigit(y[yIndex]))
        {
            yIndex++;
        }

        if (xStart == xIndex && xIndex < x.Length)
        {
            xIndex++;
        }

        if (yStart == yIndex && yIndex < y.Length)
        {
            yIndex++;
        }

        return CompareInfo.Compare(x[xStart..xIndex], y[yStart..yIndex], CompareOptions.StringSort);
    }

    private static int CompareNumberRun(string x, ref int xIndex, string y, ref int yIndex)
    {
        var xStart = xIndex;
        var yStart = yIndex;

        while (xIndex < x.Length && char.IsDigit(x[xIndex]))
        {
            xIndex++;
        }

        while (yIndex < y.Length && char.IsDigit(y[yIndex]))
        {
            yIndex++;
        }

        var xSignificant = SkipLeadingZeros(x, xStart, xIndex);
        var ySignificant = SkipLeadingZeros(y, yStart, yIndex);
        var xSignificantLength = xIndex - xSignificant;
        var ySignificantLength = yIndex - ySignificant;

        if (xSignificantLength != ySignificantLength)
        {
            return xSignificantLength.CompareTo(ySignificantLength);
        }

        for (var i = 0; i < xSignificantLength; i++)
        {
            var comparison = x[xSignificant + i].CompareTo(y[ySignificant + i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return (xIndex - xStart).CompareTo(yIndex - yStart);
    }

    private static int SkipLeadingZeros(string value, int start, int end)
    {
        while (start < end - 1 && value[start] == '0')
        {
            start++;
        }

        return start;
    }
}
