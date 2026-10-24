using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Demo.Views;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Ribbon「视图」外壳命令组：面板可见性开关（导航树 / 日志），CheckBox 语义——
/// 勾选态 = 面板显示。与 design 原型一致——外壳命令不进导航树与 API 目录。
/// </summary>
public sealed class ShellViewGroup : DemoGroupViewModel
{
    private readonly MainViewModel _owner;

    public ShellItemViewModel NavItem { get; }
    public ShellItemViewModel LogItem { get; }

    public ShellViewGroup(MainViewModel owner) : base(
        DemoKeys.ShellViewGroup,
        "▤",
        CreateItems(owner, out var nav, out var log),
        isShellGroup: true)
    {
        _owner = owner;
        NavItem = nav;
        LogItem = log;
        Refresh();
    }

    private static IEnumerable<DemoItemViewModel> CreateItems(MainViewModel owner, out ShellItemViewModel nav, out ShellItemViewModel log)
    {
        nav = Shell(owner, DemoKeys.ToolNavigation, "SemiIconSidebar", () => owner.TogglePanel("nav"));
        log = Shell(owner, DemoKeys.ToolLog, "SemiIconList", () => owner.TogglePanel("log"));
        return new DemoItemViewModel[] { nav, log };
    }

    /// <summary>把当前面板可见性同步到勾选态（切换后与布局恢复后调用）。</summary>
    public void Refresh()
    {
        NavItem.IsSelected = _owner.IsNavVisible;
        LogItem.IsSelected = _owner.IsLogVisible;
    }

    private static ShellItemViewModel Shell(MainViewModel owner, string titleKey, string iconKey, Action execute)
    {
        var icon = Application.Current?.FindResource(iconKey) as Geometry
                   ?? throw new InvalidOperationException("Missing shell icon: " + iconKey);
        return new ShellItemViewModel(titleKey, execute, icon, owner);
    }

    /// <summary>外壳命令项：点击执行外壳动作，永不进入文档打开流程。</summary>
    public sealed class ShellItemViewModel : DemoItemViewModel
    {
        private readonly MainViewModel _owner;

        public ShellItemViewModel(string titleKey, Action execute, Geometry ribbonIcon, MainViewModel owner)
            : base(titleKey, ThrowFactory, ribbonIcon: ribbonIcon)
        {
            _owner = owner;
            Execute = execute;
        }

        public Action Execute { get; }

        /// <summary>外壳命令以勾选态呈现（.selected 样式 + 对勾图标）。</summary>
        public override bool IsShellCommand => true;

        public void ExecuteAndRefresh()
        {
            Execute();
            _owner.ShellViewGroup?.Refresh();
        }

        private static DemoPageBase ThrowFactory() => throw new InvalidOperationException("外壳命令不打开文档。");
    }
}
