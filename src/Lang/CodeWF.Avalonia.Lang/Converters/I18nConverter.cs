using CodeWF.Toolkit.Lang;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CodeWF.Avalonia.Lang.MarkupExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace CodeWF.Avalonia.Lang.Converters;

/// <summary>
/// 结合当前语言、资源键及格式参数生成最终文本。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2
            || values[0] is not CultureInfo currentCulture
            || parameter is not I18nBinding owner
            || IsUnsetValue(values[1]))
        {
            return BindingOperations.DoNothing;
        }

        var value = values[1];
        if (owner.KeyConverter is { } keyConverter)
        {
            value = keyConverter.Convert(value, typeof(string), null, culture);
        }

        if (value is not string key)
        {
            return value;
        }

        value = I18nManager.Instance.GetResource(key, owner.CultureName);

        if (value is string format && owner.Indexes.Count > 0)
        {
            if (owner.Indexes.Any(item => item.IsBinding && item.Index >= values.Count))
            {
                return BindingOperations.DoNothing;
            }

            var args = owner.Indexes
                .Select(item => item.IsBinding ? values[item.Index] : owner.Args[item.Index])
                .ToArray();

            if (args.Any(IsUnsetValue))
            {
                return BindingOperations.DoNothing;
            }

            try
            {
                var formatCulture = currentCulture;
                if (!string.IsNullOrWhiteSpace(owner.CultureName))
                {
                    try { formatCulture = CultureInfo.GetCultureInfo(owner.CultureName); }
                    catch (CultureNotFoundException) { }
                }

                value = string.Format(formatCulture, format, args);
            }
            catch (FormatException)
            {
                value = format;
            }
        }

        return owner.ValueConverter is { } valueConverter
            ? valueConverter.Convert(value, targetType, null, culture)
            : value;
    }

    private static bool IsUnsetValue(object? value)
    {
        return ReferenceEquals(value, AvaloniaProperty.UnsetValue)
            || ReferenceEquals(value, BindingOperations.DoNothing);
    }
}
