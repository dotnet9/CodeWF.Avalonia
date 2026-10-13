using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CodeWF.Avalonia.Demo.ViewModels;

namespace CodeWF.Avalonia.Demo.Views.Tools;

/// <summary>
/// 导航树工具面板：就地重命名的输入转发（双击进入、回车提交、Esc 取消、失焦提交）。
/// </summary>
public partial class NavToolView : UserControl
{
    public NavToolView()
    {
        InitializeComponent();
    }

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
