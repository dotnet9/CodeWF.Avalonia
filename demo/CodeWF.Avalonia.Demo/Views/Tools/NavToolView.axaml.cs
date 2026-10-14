using Avalonia.Controls;
using Avalonia.Input;
using System.ComponentModel;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodeWF.Avalonia.Demo.ViewModels;

namespace CodeWF.Avalonia.Demo.Views.Tools;

/// <summary>
/// 导航树工具面板：就地重命名的输入转发（双击进入、回车提交、Esc 取消、失焦提交）。
/// </summary>
public partial class NavToolView : UserControl
{
    private MainViewModel? _observedViewModel;

    public NavToolView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => ObserveSelection();
        DetachedFromVisualTree += (_, _) => StopObservingSelection();
        DataContextChanged += (_, _) => { if (IsLoaded) ObserveSelection(); };
    }

    private void ObserveSelection()
    {
        StopObservingSelection();
        _observedViewModel = ViewModel;
        if (_observedViewModel is not null) _observedViewModel.PropertyChanged += OnSelectionChanged;
        ScrollToSelection();
    }

    private void StopObservingSelection()
    {
        if (_observedViewModel is not null) _observedViewModel.PropertyChanged -= OnSelectionChanged;
        _observedViewModel = null;
    }

    private void OnSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedNavItem)) ScrollToSelection();
    }

    private void ScrollToSelection() => Dispatcher.UIThread.Post(() =>
    {
        if (!IsLoaded) return;
        UpdateLayout();
        this.GetVisualDescendants().OfType<Button>()
            .FirstOrDefault(button => button.Classes.Contains("nav-leaf") && button.DataContext is DemoItemViewModel { IsSelected: true })
            ?.BringIntoView();
    }, DispatcherPriority.Background);

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnRenameClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is Button { DataContext: DemoItemViewModel item })
        {
            ViewModel?.BeginRename(item);
        }
    }

    private void OnLeafDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Button { DataContext: DemoItemViewModel item })
        {
            ViewModel?.BeginRename(item);
            e.Handled = true;
        }
    }

    private void OnRenameBoxLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        // 输入框出现即全选，双击改名不需要再手动清理文本。
        Dispatcher.UIThread.Post(() =>
        {
            box.Focus();
            box.SelectAll();
        }, DispatcherPriority.Input);
    }

    private void OnRenameBoxKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                ViewModel?.CommitRename();
                e.Handled = true;
                break;
            case Key.Escape:
                ViewModel?.CancelRename();
                e.Handled = true;
                break;
        }
    }

    private void OnRenameBoxLostFocus(object? sender, RoutedEventArgs e) => ViewModel?.CommitRename();
}
