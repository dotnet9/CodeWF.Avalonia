using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Demo.ViewModels;

namespace CodeWF.Avalonia.Demo.Views;

public partial class MainWindow : CodeWFWindow
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        var version = typeof(MainWindow).Assembly.GetName().Version;
        if (version is not null)
        {
            VersionText.Text = "Demo · v" + version.ToString(3);
        }

        if (Environment.GetEnvironmentVariable("NCW_PALETTE_PROBE") == "1")
        {
            DumpPaletteProbe();
            Environment.Exit(0);
        }

        Loaded += OnLoaded;
    }


    // TEMP 探针（NCW_PALETTE_PROBE=1 时执行，验完即删）：转储各变体解析后的关键令牌
    private static void DumpPaletteProbe()
    {
        var app = global::Avalonia.Application.Current!;
        var variants = new (string Name, global::Avalonia.Styling.ThemeVariant Variant)[]
        {
            ("Default", global::Avalonia.Styling.ThemeVariant.Default),
            ("Light", global::Avalonia.Styling.ThemeVariant.Light),
            ("Dark", global::Avalonia.Styling.ThemeVariant.Dark),
            ("Aquatic", Semi.Avalonia.SemiTheme.Aquatic),
            ("Desert", Semi.Avalonia.SemiTheme.Desert),
            ("Dusk", Semi.Avalonia.SemiTheme.Dusk),
            ("NightSky", Semi.Avalonia.SemiTheme.NightSky),
        };
        var keys = new[]
        {
            "CodeWFWindowBackground", "CodeWFWindowForeground", "CodeWFTitleBarForeground",
            "CodeWFDockPanelBackgroundBrush", "CodeWFDockHeaderForegroundBrush",
            "DockSurfaceHeaderBrush", "DockTabForegroundBrush", "DockTabBackgroundBrush",
            "CodeWFLogViewBackground", "TextBlockDefaultForeground"
        };
        var sb = new System.Text.StringBuilder();
        foreach (var (name, variant) in variants)
        {
            app.RequestedThemeVariant = variant;
            sb.AppendLine("=== " + name + " | actual=" + app.ActualThemeVariant + " ===");
            foreach (var key in keys)
            {
                var found = app.TryFindResource(key, out var value);
                var text = value is global::Avalonia.Media.ISolidColorBrush brush
                    ? brush.Color.ToString()
                    : value?.ToString() ?? "null";
                sb.AppendLine("  " + key.PadRight(32) + " found=" + found + "  " + text);
            }
        }
        System.IO.File.WriteAllText(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "palette-probe.txt"), sb.ToString());
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (Program.IsSmokeMode)
        {
            try
            {
                var ok = await _viewModel.RunSmokeAsync(CancellationToken.None);
                Environment.Exit(ok ? 0 : 1);
            }
            catch
            {
                Environment.Exit(1);
            }
        }
    }

    /// <summary>双击 Ribbon 页签折叠/展开命令区。</summary>
    private void OnRibbonTabStripDoubleTapped(object? sender, RoutedEventArgs e)
    {
        _viewModel.RibbonExpanded = !_viewModel.RibbonExpanded;
    }
}
