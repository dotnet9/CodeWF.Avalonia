using Avalonia;
using Avalonia.Platform;
using Avalonia.Threading;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 统一应用的八套配色与浅色、深色、跟随系统模式。
/// 宿主加载 Semi 与 StudioTheme 后绑定本对象，退出时释放系统主题订阅。
/// </summary>
public sealed class StudioThemeController : AvaloniaObject, IDisposable
{
    public static readonly StyledProperty<StudioPalette> PaletteProperty =
        AvaloniaProperty.Register<StudioThemeController, StudioPalette>(nameof(Palette), StudioPalette.DotNet);
    public static readonly StyledProperty<StudioThemeMode> ModeProperty =
        AvaloniaProperty.Register<StudioThemeController, StudioThemeMode>(nameof(Mode), StudioThemeMode.System);

    private readonly Application _application;
    private readonly IPlatformSettings? _platformSettings;
    private bool _systemDark;
    private bool _disposed;

    public StudioThemeController(Application application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _platformSettings = application.PlatformSettings;
        _systemDark = _platformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark;
        if (_platformSettings is not null) _platformSettings.ColorValuesChanged += OnColorValuesChanged;
        Apply();
    }

    public StudioPalette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public StudioThemeMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PaletteProperty || change.Property == ModeProperty) Apply();
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
        _application.RequestedThemeVariant = StudioTheme.GetVariant(Palette, dark);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_platformSettings is not null) _platformSettings.ColorValuesChanged -= OnColorValuesChanged;
    }
}
