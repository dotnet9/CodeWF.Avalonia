using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Lang;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>八套配色与明暗模式独立，颜色令牌由 StudioTheme 维护。</summary>
public sealed class AppearanceOption(string titleKey, StudioPalette palette) : global::ReactiveUI.ReactiveObject
{
    public string TitleKey { get; } = titleKey;
    public StudioPalette Palette { get; } = palette;
    public ThemeVariant ResolveVariant(bool dark) => StudioTheme.GetVariant(Palette, dark);
    public string SwatchKey => $"StudioSwatch{Palette}";
    public IBrush Swatch => global::Avalonia.Application.Current is { } app
        && app.TryFindResource(SwatchKey, out var resource) && resource is IBrush brush
            ? brush : Brushes.Transparent;
    public string Title => I18nManager.Instance.GetResource(TitleKey);
    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));
    public override string ToString() => Title;

    public static AppearanceOption DotNet() => new(DemoResources.Palette.DotNet, StudioPalette.DotNet);

    public static AppearanceOption Aurora() => new(DemoResources.Palette.Aurora, StudioPalette.Aurora);

    public static AppearanceOption Ember() => new(DemoResources.Palette.Ember, StudioPalette.Ember);

    public static AppearanceOption Default() => new(DemoResources.Palette.Default, StudioPalette.Default);

    public static AppearanceOption Aquatic() => new(DemoResources.Palette.Aquatic, StudioPalette.Aquatic);

    public static AppearanceOption Desert() => new(DemoResources.Palette.Desert, StudioPalette.Desert);

    public static AppearanceOption Dusk() => new(DemoResources.Palette.Dusk, StudioPalette.Dusk);

    public static AppearanceOption NightSky() => new(DemoResources.Palette.NightSky, StudioPalette.NightSky);
}

/// <summary>保留对象身份的主题模式选项，切换语言只刷新显示标题。</summary>
public sealed class ThemeModeOption(string titleKey) : global::ReactiveUI.ReactiveObject
{
    public string Title => I18nManager.Instance.GetResource(titleKey);
    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));
    public override string ToString() => Title;
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
