using System.Collections.Concurrent;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 基于 Semi 控件模板提供可选的 Studio 外观。
/// 内置九套配色的令牌由生成器产出静态 axaml（StudioThemeVariants.g.cs 提供变体）；
/// 用户配色经控制器注册后在应用级合并字典生效。
/// </summary>
public sealed partial class StudioTheme : Styles
{
    private static readonly ConcurrentDictionary<string, ThemeVariant> Variants = new();

    public StudioTheme() => AvaloniaXamlLoader.Load(this);

    /// <summary>内置键查静态变体；用户配色（未注册键）运行时创建。</summary>
    public static ThemeVariant GetVariant(string paletteKey, bool dark)
    {
        if (BuiltinVariants.TryGetValue((paletteKey, dark), out var builtin)) return builtin;
        var name = $"studio/{paletteKey}/{(dark ? "dark" : "light")}";
        return Variants.GetOrAdd(name, key => new ThemeVariant(key, dark ? ThemeVariant.Dark : ThemeVariant.Light));
    }
}
