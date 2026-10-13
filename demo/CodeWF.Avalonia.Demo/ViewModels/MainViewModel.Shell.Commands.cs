using CodeWF.Avalonia.Lang;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 外壳快捷键与状态栏入口（Ctrl+D 深浅 / Ctrl+L 语言 / Ctrl+K 命令面板 / 通知演示）。
/// 与 MainViewModel.Shell.cs 同属外壳实体化件。
/// </summary>
public partial class MainViewModel
{
    public ReactiveCommand<RxVoid, RxVoid> ToggleDarkCommand { get; private set; } = null!;

    public ReactiveCommand<RxVoid, RxVoid> ToggleCultureCommand { get; private set; } = null!;

    public ReactiveCommand<RxVoid, RxVoid> NotifyDemoCommand { get; private set; } = null!;

    /// <summary>通知中心的当前卡片数（工具条铃铛徽标）。</summary>
    public int NotificationCount => Notifications.Count;

    private void InitializeShellCommands()
    {
        ToggleDarkCommand = ReactiveCommand.Create(() => { IsDarkMode = !IsDarkMode; });
        ToggleCultureCommand = ReactiveCommand.Create(() =>
        {
            var next = Cultures.FirstOrDefault(c => c != SelectedCulture) ?? SelectedCulture;
            SelectedCulture = next;
        });
        NotifyDemoCommand = ReactiveCommand.Create(() =>
        {
            Notify(DemoNotificationKind.Success, T(DemoKeys.ShellNotifyDemo), T(DemoKeys.ShellNotifyDemoHint));
        });
    }
}
