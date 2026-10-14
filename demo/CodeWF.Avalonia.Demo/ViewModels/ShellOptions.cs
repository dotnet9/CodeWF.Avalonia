using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Log;
using Semi.Avalonia;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>9 色板（默认/水生/沙漠/黄昏/夜空/高对比），叠加浅/深模式映射为应用主题变体。</summary>
public sealed class AppearanceOption : global::ReactiveUI.ReactiveObject
{
    public delegate ThemeVariant VariantResolver(bool isDarkMode);

    public AppearanceOption(string titleKey, VariantResolver resolveVariant, string swatchKey)
    {
        TitleKey = titleKey;
        ResolveVariant = resolveVariant;
        SwatchKey = swatchKey;
    }

    /// <summary>名称的 Lang 键。</summary>
    public string TitleKey { get; }

    public VariantResolver ResolveVariant { get; }
    public string SwatchKey { get; }
    public IBrush Swatch => global::Avalonia.Application.Current is { } app
        && app.TryFindResource(SwatchKey, out var resource) && resource is IBrush brush
            ? brush : Brushes.Transparent;

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));

    public override string ToString() => Title;

    public static AppearanceOption DotNet() =>
        new(DemoResources.Palette.DotNet, dark => dark ? StudioTheme.DotNetDark : StudioTheme.DotNetLight, "StudioSwatchDotNet");

    public static AppearanceOption Aurora() =>
        new(DemoResources.Palette.Aurora, dark => dark ? StudioTheme.AuroraDark : StudioTheme.AuroraLight, "StudioSwatchAurora");

    public static AppearanceOption Ember() =>
        new(DemoResources.Palette.Ember, dark => dark ? StudioTheme.EmberDark : StudioTheme.EmberLight, "StudioSwatchEmber");

    public static AppearanceOption Default() =>
        new(DemoResources.Palette.Default, isDark => isDark ? ThemeVariant.Dark : ThemeVariant.Light, "StudioSwatchDefault");

    public static AppearanceOption Aquatic() =>
        new(DemoResources.Palette.Aquatic, _ => SemiTheme.Aquatic, "StudioSwatchAquatic");

    public static AppearanceOption Desert() =>
        new(DemoResources.Palette.Desert, _ => SemiTheme.Desert, "StudioSwatchDesert");

    public static AppearanceOption Dusk() =>
        new(DemoResources.Palette.Dusk, _ => SemiTheme.Dusk, "StudioSwatchDusk");

    public static AppearanceOption NightSky() =>
        new(DemoResources.Palette.NightSky, _ => SemiTheme.NightSky, "StudioSwatchNightSky");

    public static AppearanceOption HighContrast() =>
        new(DemoResources.Palette.HighContrast, _ => CodeWFLogTheme.HighContrast, "StudioSwatchHighContrast");
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
