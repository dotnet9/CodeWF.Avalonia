using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.ViewModels;

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

    /// <summary>导航树单击叶子即打开对应演示文档。</summary>
    private void OnNavTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavTree.SelectedItem is DemoItemViewModel item)
        {
            _viewModel.OpenItem(item);
        }
    }

    /// <summary>双击 Ribbon 页签折叠/展开命令区。</summary>
    private void OnRibbonTabStripDoubleTapped(object? sender, RoutedEventArgs e)
    {
        _viewModel.RibbonExpanded = !_viewModel.RibbonExpanded;
    }
}
