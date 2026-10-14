using Avalonia.Markup.Xaml;
using Avalonia.Data;
using System;
using System.Collections.Generic;

namespace CodeWF.Avalonia.Lang.MarkupExtensions;

/// <summary>
/// 用于查询国际化资源的 XAML 标记扩展。
/// </summary>
public class I18nExtension : MarkupExtension
{
    /// <summary>
    /// 根据资源键创建标记扩展。
    /// </summary>
    public I18nExtension(object key)
    {
        Key = key;
    }

    /// <summary>
    /// 创建包含一个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0) : this(key)
    {
        Args.Add(arg0);
    }

    /// <summary>
    /// 创建包含两个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
    }

    /// <summary>
    /// 创建包含三个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
    }

    /// <summary>
    /// 创建包含四个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
    }

    /// <summary>
    /// 创建包含五个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
    }

    /// <summary>
    /// 创建包含六个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5) :
        this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
    }

    /// <summary>
    /// 创建包含七个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5,
        object arg6) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
        Args.Add(arg6);
    }

    /// <summary>
    /// 创建包含八个格式参数的标记扩展。
    /// </summary>
    public I18nExtension(object key, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5,
        object arg6, object arg7) : this(key)
    {
        Args.Add(arg0);
        Args.Add(arg1);
        Args.Add(arg2);
        Args.Add(arg3);
        Args.Add(arg4);
        Args.Add(arg5);
        Args.Add(arg6);
        Args.Add(arg7);
    }

    /// <summary>
    /// 资源键、常量、枚举值或绑定。
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// 格式参数，可使用常量或 Avalonia 绑定。
    /// </summary>
    public List<object> Args { get; } = new();

    /// <summary>
    /// 固定语言名称；未设置时跟随 I18nManager.Culture。
    /// </summary>
    public string? CultureName { get; set; }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => CreateBinding();

    /// <summary>为使用 C# 构建的视图创建相同的动态国际化绑定。</summary>
    public MultiBinding CreateBinding() =>
        (MultiBinding)new I18nBinding(Key, CultureName, Args).ProvideValue(null!);
}
