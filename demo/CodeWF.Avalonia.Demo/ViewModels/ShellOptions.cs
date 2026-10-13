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

    public AppearanceOption(string titleKey, VariantResolver resolveVariant)
    {
        TitleKey = titleKey;
        ResolveVariant = resolveVariant;
        Swatch = titleKey.Split('.').Last() switch
        {
            "DotNet" => Brush.Parse("#512BD4"),
            "Aurora" => new LinearGradientBrush
            {
                GradientStops = new GradientStops { new(Color.Parse("#8968FF"), 0), new(Color.Parse("#26D4CC"), 1) }
            },
            "Ember" => Brush.Parse("#BD674D"),
            "Default" => Brush.Parse("#0064FA"),
            "Aquatic" => Brush.Parse("#008D9E"),
            "Desert" => Brush.Parse("#B57532"),
            "Dusk" => Brush.Parse("#7A5CC4"),
            "NightSky" => Brush.Parse("#2B6CB0"),
            _ => Brush.Parse("#777777")
        };
    }

    /// <summary>名称的 Lang 键。</summary>
    public string TitleKey { get; }

    public VariantResolver ResolveVariant { get; }
    public IBrush Swatch { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));

    public override string ToString() => Title;

    public static AppearanceOption DotNet() =>
        new("Demo.Palette.DotNet", dark => dark ? StudioTheme.DotNetDark : StudioTheme.DotNetLight);

    public static AppearanceOption Aurora() =>
        new("Demo.Palette.Aurora", dark => dark ? StudioTheme.AuroraDark : StudioTheme.AuroraLight);

    public static AppearanceOption Ember() =>
        new("Demo.Palette.Ember", dark => dark ? StudioTheme.EmberDark : StudioTheme.EmberLight);

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
