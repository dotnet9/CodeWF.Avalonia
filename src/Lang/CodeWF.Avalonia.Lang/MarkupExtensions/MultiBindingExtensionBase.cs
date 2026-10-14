using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace CodeWF.Avalonia.Lang.MarkupExtensions;

/// <summary>
/// 封装 MultiBinding，供派生标记扩展统一初始化转换器状态。
/// </summary>
public abstract class MultiBindingExtensionBase : MarkupExtension
{
    private readonly MultiBinding binding = new();
    private bool converterInitialized;
    private bool converterParameterInitialized;

    /// <summary>
    /// 内部 MultiBinding 使用的子绑定集合。
    /// </summary>
    internal IList<BindingBase> Bindings => binding.Bindings;

    /// <summary>
    /// 内部 MultiBinding 使用的绑定模式。
    /// </summary>
    protected BindingMode Mode
    {
        get => binding.Mode;
        set => binding.Mode = value;
    }

    /// <summary>
    /// 多值转换器，派生类可在初始化时设置。
    /// </summary>
    protected IMultiValueConverter? Converter
    {
        get => binding.Converter;
        set
        {
            if (converterInitialized)
            {
                throw new InvalidOperationException($"The {GetType().Name}.Converter property is readonly.");
            }

            binding.Converter = value;
            converterInitialized = true;
        }
    }

    /// <summary>
    /// 转换器参数，派生类可在初始化时设置。
    /// </summary>
    protected object? ConverterParameter
    {
        get => binding.ConverterParameter;
        set
        {
            if (converterParameterInitialized)
            {
                throw new InvalidOperationException($"The {GetType().Name}.ConverterParameter property is readonly.");
            }

            binding.ConverterParameter = value;
            converterParameterInitialized = true;
        }
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => binding;
}
