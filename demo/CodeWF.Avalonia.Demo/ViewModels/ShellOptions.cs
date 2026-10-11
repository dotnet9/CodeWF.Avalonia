using Avalonia.Styling;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Log;
using Semi.Avalonia;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>6 色板（默认/水生/沙漠/黄昏/夜空/高对比），叠加浅/深模式映射为应用主题变体。</summary>
public sealed class AppearanceOption
{
    public delegate ThemeVariant VariantResolver(bool isDarkMode);

    public AppearanceOption(string titleKey, VariantResolver resolveVariant)
    {
        TitleKey = titleKey;
        ResolveVariant = resolveVariant;
    }

    /// <summary>名称的 Lang 键。</summary>
    public string TitleKey { get; }

    public VariantResolver ResolveVariant { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public override string ToString() => Title;

    public static AppearanceOption Default() =>
        new("Demo.Palette.Default", isDark => isDark ? ThemeVariant.Dark : ThemeVariant.Light);

    public static AppearanceOption Aquatic() =>
        new("Demo.Palette.Aquatic", _ => SemiTheme.Aquatic);

    public static AppearanceOption Desert() =>
        new("Demo.Palette.Desert", _ => SemiTheme.Desert);

    public static AppearanceOption Dusk() =>
        new("Demo.Palette.Dusk", _ => SemiTheme.Dusk);

    public static AppearanceOption NightSky() =>
        new("Demo.Palette.NightSky", _ => SemiTheme.NightSky);

    public static AppearanceOption HighContrast() =>
        new("Demo.Palette.HighContrast", _ => CodeWFLogTheme.HighContrast);
}

/// <summary>可选语言。</summary>
public sealed class CultureOption
{
    public CultureOption(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }

    /// <summary>语言名以其自身语言展示（不随界面语言切换）。</summary>
    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
