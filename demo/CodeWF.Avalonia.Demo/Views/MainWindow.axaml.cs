using CodeWF.Avalonia.Demo.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CodeWF.Avalonia.Demo.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
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
}
