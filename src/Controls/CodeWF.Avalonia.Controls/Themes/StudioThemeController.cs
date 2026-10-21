using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 统一应用的配色（字符串 Key 注册表，规则 §13）与浅色、深色、跟随系统模式。
/// 令牌字典由派生引擎按变体运行时填充：不依赖每套配色一份 axaml。
/// 宿主加载 Semi 与 Studio 主题后绑定本对象，退出时释放系统主题订阅。
/// </summary>
public sealed class StudioThemeController : AvaloniaObject, IDisposable
{
    public static readonly StyledProperty<string> PaletteKeyProperty =
        AvaloniaProperty.Register<StudioThemeController, string>(nameof(PaletteKey), StudioPalettes.DefaultKey);
    public static readonly StyledProperty<StudioThemeMode> ModeProperty =
        AvaloniaProperty.Register<StudioThemeController, StudioThemeMode>(nameof(Mode), StudioThemeMode.System);

    private readonly Application _application;
    private readonly StudioPaletteRegistry _registry = new();
    private readonly IPlatformSettings? _platformSettings;
    private bool _systemDark;
    private bool _disposed;

    public StudioThemeController(Application application)
    {
        _application = application;
        _platformSettings = application.PlatformSettings;
        _systemDark = _platformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark;
        if (_platformSettings is not null) _platformSettings.ColorValuesChanged += OnColorValuesChanged;
        Apply();
    }

    public string PaletteKey
    {
        get => GetValue(PaletteKeyProperty);
        set => SetValue(PaletteKeyProperty, value);
    }

    public StudioThemeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>注册表（宿主可注册用户配色后调用 SetPalette 应用）。</summary>
    public StudioPaletteRegistry Registry => _registry;

    /// <summary>配色色卡（派生强调色），供画廊/下拉展示。</summary>
    public IBrush GetSwatch(string key)
    {
        var palette = _registry.Find(key);
        if (palette is null) return Brushes.Transparent;
        var derived = StudioPaletteEngine.Derive(key, palette, false);
        return new SolidColorBrush(derived["StudioAccentColor"]);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PaletteKeyProperty || change.Property == ModeProperty) Apply();
    }

    private void OnColorValuesChanged(object? sender, PlatformColorValues values)
    {
        void Refresh()
        {
            if (_disposed) return;
            _systemDark = values.ThemeVariant == PlatformThemeVariant.Dark;
            Apply();
        }
        if (Dispatcher.UIThread.CheckAccess()) Refresh();
        else Dispatcher.UIThread.Post(Refresh);
    }

    private void Apply()
    {
        if (_disposed) return;
        var dark = Mode switch
        {
            StudioThemeMode.Dark => true,
            StudioThemeMode.Light => false,
            _ => _systemDark,
        };
        var palette = _registry.Find(PaletteKey) ?? _registry.Find(StudioPalettes.DefaultKey)!;
        var variant = StudioTheme.GetVariant(palette.Key, dark);
        EnsureUserPaletteResources(palette, dark, variant);
        _application.RequestedThemeVariant = variant;
    }

    /// <summary>引擎派生 33+ 基色 → StudioTokens.Fill 填充全部令牌键（含 Semi/Dock/Log 等间接引用）。</summary>
    private static ResourceDictionary BuildDictionary(StudioPaletteDefinition palette, bool dark)
    {
        var derived = StudioPaletteEngine.Derive(palette.Key, palette, dark);
        var extras = StudioPaletteEngine.DeriveExtras(palette, dark);
        foreach (var pair in extras) derived[pair.Key] = pair.Value;
        var dictionary = new ResourceDictionary();
        StudioTokens.Fill(dictionary, derived);
        if (dictionary["StudioActionGradient"] is LinearGradientBrush action && action.GradientStops.Count == 0)
        {
            action.GradientStops.Add(new GradientStop(derived["StudioPrimaryColor"], 0));
            action.GradientStops.Add(new GradientStop(derived["StudioGradientEndColor"], 1));
        }
        dictionary["StudioPropertyGridTheme"] = dark ? "Dark" : "Light";
        dictionary["SystemAccentColor"] = derived["StudioAccentColor"];
        return dictionary;
    }

    /// <summary>用户配色：派生字典合并进应用级资源（内置九套走静态 axaml，无需合并）。</summary>
    private void EnsureUserPaletteResources(StudioPaletteDefinition palette, bool dark, ThemeVariant variant)
    {
        if (!palette.Key.StartsWith("user:")) return;
        _application.Resources.ThemeDictionaries[variant] = BuildDictionary(palette, dark);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_platformSettings is not null) _platformSettings.ColorValuesChanged -= OnColorValuesChanged;
    }
}
