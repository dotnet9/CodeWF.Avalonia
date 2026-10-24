using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Demo.Views;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Ribbon「视图」外壳命令组：Dock 面板显隐（导航树 / 日志 / 最大化 / 重置）。
/// 与 design 原型一致——外壳命令不进导航树与 API 目录，点击直接驱动外壳而不打开文档。
/// </summary>
public sealed class ShellViewGroup : DemoGroupViewModel
{
    public ShellViewGroup(MainViewModel owner) : base(
        DemoKeys.ShellViewGroup,
        "▤",
        new DemoItemViewModel[]
        {
            Shell(owner, DemoKeys.ToolNavigation, "SemiIconSidebar", () => owner.TogglePanel("nav")),
            Shell(owner, DemoKeys.ToolLog, "SemiIconList", () => owner.TogglePanel("log")),
            Shell(owner, DemoKeys.ShellMaximize, "SemiIconFullScreenStroked", owner.MaximizeDocumentArea),
            Shell(owner, DemoKeys.ShellReset, "SemiIconRefresh", owner.ResetLayout),
        },
        isShellGroup: true)
    {
    }

    private static DemoItemViewModel Shell(MainViewModel owner, string titleKey, string iconKey, Action execute)
    {
        var icon = Application.Current?.FindResource(iconKey) as Geometry
                   ?? throw new InvalidOperationException("Missing shell icon: " + iconKey);
        return new ShellItemViewModel(titleKey, execute, icon);
    }

    /// <summary>外壳命令项：点击执行外壳动作，永不进入文档打开流程。</summary>
    public sealed class ShellItemViewModel : DemoItemViewModel
    {
        public ShellItemViewModel(string titleKey, Action execute, Geometry ribbonIcon)
            : base(titleKey, ThrowFactory, ribbonIcon: ribbonIcon)
        {
            Execute = execute;
        }

        public Action Execute { get; }

        private static DemoPageBase ThrowFactory() => throw new InvalidOperationException("外壳命令不打开文档。");
    }
}
