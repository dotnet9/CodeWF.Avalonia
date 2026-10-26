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
        _viewModel.PaletteDialogHandler = draft => new PaletteEditDialog(draft).ShowDialog<bool>(this).GetAwaiter().GetResult();
        Closed += (_, _) => _viewModel.Dispose();
        var version = typeof(MainWindow).Assembly.GetName().Version;
        if (version is not null)
        {
            VersionText.Text = "Demo · v" + version.ToString(4);
        }

        // 模式 pill 的 ItemsSource 随语言换新数组会重置选择，绑定重推与换源求值顺序又不保证，换源后按模型对齐。
        if (this.FindControl<global::Avalonia.Controls.ListBox>("ThemeModePills") is { } modePills)
        {
            modePills.PropertyChanged += (_, args) =>
            {
                if (args.Property == global::Avalonia.Controls.ListBox.ItemsSourceProperty
                    && DataContext is MainViewModel model
                    && modePills.SelectedIndex != model.ThemeModeIndex)
                {
                    modePills.SelectedIndex = model.ThemeModeIndex;
                }
            };
        }

        Loaded += OnLoaded;
    }

    /// <summary>配色卡片点击 = 选中并应用（用户卡 ✎/× 走各自命令）。</summary>
    private void OnPaletteCardClick(object? sender, RoutedEventArgs e)
    {
        if (sender is global::Avalonia.Controls.Button { Tag: AppearanceOption option } && !Equals(_viewModel.SelectedPalette, option))
        {
            _viewModel.SelectedPalette = option;
        }
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (Program.CaptureDirectory is { } directory)
        {
            try
            {
                await _viewModel.CaptureStudioAsync(this, directory);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.Exit(1);
            }
        }

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
