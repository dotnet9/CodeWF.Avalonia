using System.Collections.Concurrent;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 基于 Semi 控件模板提供可选的 Studio 外观。
/// 令牌由派生引擎按变体运行时填充（规则 §13 字符串 Key 注册表），不再依赖每套配色一份 axaml。
/// </summary>
public sealed class StudioTheme : Styles
{
    private static readonly ConcurrentDictionary<string, ThemeVariant> Variants = new();

    /// <summary>获取配色 Key 与明暗模式对应的主题变体（用户配色运行时创建，内置与用户同轨）。</summary>
    public static ThemeVariant GetVariant(string paletteKey, bool dark)
    {
        var name = $"studio/{paletteKey}/{(dark ? "dark" : "light")}";
        return Variants.GetOrAdd(name, key => new ThemeVariant(key, dark ? ThemeVariant.Dark : ThemeVariant.Light));
    }

    public StudioTheme() => AvaloniaXamlLoader.Load(this);
}
