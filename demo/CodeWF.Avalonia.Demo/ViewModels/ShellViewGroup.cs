using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Demo.Views;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Ribbon "View" command group: panel visibility toggles (navigation tree / log) with CheckBox semantics.
/// Checked state = panel visible. Matches the design prototype; shell commands stay out of the tree and API catalog.
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

    /// <summary>Syncs current panel visibility into check states (after layout switch and layout restore).</summary>
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

    /// <summary>Shell command item: click runs a shell action and never enters the document-open flow.</summary>
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

        /// <summary>Shell commands render as check items (.selected style + check glyph).</summary>
        public override bool IsShellCommand => true;

        public void ExecuteAndRefresh()
        {
            Execute();
            _owner.ShellViewGroup?.Refresh();
        }

        private static DemoPageBase ThrowFactory() =>
            throw new InvalidOperationException("Shell command must not open a document (defensive guard; unreachable path).");
    }
}
