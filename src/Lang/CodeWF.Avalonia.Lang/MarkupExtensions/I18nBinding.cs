using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using CodeWF.Avalonia.Lang.Converters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CodeWF.Avalonia.Lang.MarkupExtensions;

/// <summary>
/// I18nExtension 使用的多值绑定实现。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class I18nBinding : MultiBindingExtensionBase
{
    /// <summary>
    /// 根据资源键或资源键绑定创建国际化绑定。
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "I18nBinding intentionally accepts runtime Avalonia bindings for dynamic localization keys.")]
    public I18nBinding(object key)
    {
        Mode = BindingMode.OneWay;
        Converter = new I18nConverter();
        ConverterParameter = this;
        KeyConverter = new I18nKeyConverter();

        var cultureBinding = new CompiledBindingExtension
        {
            Source = I18nManager.Instance,
            Mode = BindingMode.OneWay,
            Path = new CompiledBindingPathBuilder()
                .Property(
                    new ClrPropertyInfo(
                        nameof(I18nManager.Culture),
                        target => ((I18nManager)target).Culture,
                        (target, value) => ((I18nManager)target).Culture = value as CultureInfo,
                        typeof(CultureInfo)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        };

        Bindings.Add(cultureBinding);

        Key = key;
        if (Key is not BindingBase keyBinding)
        {
            keyBinding = new CompiledBindingExtension { Source = key };
        }

        Bindings.Add(keyBinding);

        var resourceVersionBinding = new CompiledBindingExtension
        {
            Source = I18nManager.Instance,
            Mode = BindingMode.OneWay,
            Path = new CompiledBindingPathBuilder()
                .Property(
                    new ClrPropertyInfo(
                        nameof(I18nManager.ResourceVersion),
                        target => ((I18nManager)target).ResourceVersion,
                        null,
                        typeof(int)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        };

        Bindings.Add(resourceVersionBinding);
    }

    /// <summary>
    /// 创建包含固定语言和格式参数的国际化绑定。
    /// </summary>
    public I18nBinding(object key, string? cultureName, IReadOnlyCollection<object> args) : this(key)
    {
        CultureName = cultureName;

        foreach (var arg in args)
        {
            if (arg is BindingBase binding)
            {
                Indexes.Add((true, Bindings.Count));
                Bindings.Add(binding);
            }
            else
            {
                Indexes.Add((false, Args.Count));
                Args.Add(arg);
            }
        }
    }

    /// <summary>
    /// 资源键、常量、枚举值或 Avalonia 绑定。
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// 固定语言名称；未设置时跟随 I18nManager.Culture。
    /// </summary>
    public string? CultureName { get; set; }

    /// <summary>
    /// 参数索引：绑定参数指向 MultiBinding 项，常量参数指向 Args 项。
    /// </summary>
    internal List<(bool IsBinding, int Index)> Indexes { get; } = [];

    /// <summary>
    /// 传递给 string.Format 的常量参数。
    /// </summary>
    internal List<object> Args { get; } = [];

    /// <summary>
    /// 将资源键转换为查询使用的字符串。
    /// </summary>
    public IValueConverter? KeyConverter { get; set; }

    /// <summary>
    /// 可选的最终结果转换器。
    /// </summary>
    public IValueConverter? ValueConverter { get; set; }
}
