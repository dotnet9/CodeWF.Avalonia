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

        Loaded += OnLoaded;
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
