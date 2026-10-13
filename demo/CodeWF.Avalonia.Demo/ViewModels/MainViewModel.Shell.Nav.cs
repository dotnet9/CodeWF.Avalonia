using CodeWF.Avalonia.Lang;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 导航树工具面板的实体化件：过滤框 + 就地重命名（与 design 原型 tree 区一致）。
/// 就地重命名的状态与提交逻辑在 <see cref="MainViewModel"/> 的 Shell partial 上。
/// </summary>
public partial class MainViewModel
{
    private string _navFilter = string.Empty;

    /// <summary>导航树的过滤关键字（空串显示全部）。</summary>
    public string NavFilter
    {
        get => _navFilter;
        set
        {
            if (Equals(_navFilter, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _navFilter, value);
            ApplyNavFilter();
        }
    }

    public string NavFilterPlaceholder => T(DemoKeys.ShellNavFilter);

    public string NavEmptyHint => T(DemoKeys.ShellNavEmpty);

    public string RenameHint => T(DemoKeys.ShellRenameHint);

    public string ResetRenamesHint => T(DemoKeys.ShellResetRenames);

    /// <summary>过滤后是否一个节点都不剩（显示空态文案）。</summary>
    public bool IsNavEmpty => Groups.All(g => g.IsFilteredOut);

    /// <summary>导航树数据源（过滤以 <c>IsFilteredOut</c> 标记实现，不重建集合）。</summary>
    public IReadOnlyList<DemoGroupViewModel> FilteredGroups => Groups;

    public ReactiveCommand<DemoItemViewModel, RxVoid> SelectNavItemCommand { get; private set; } = null!;

    public ReactiveCommand<RxVoid, RxVoid> ResetRenamesCommand { get; private set; } = null!;

    public ReactiveCommand<RxVoid, RxVoid> EscapeShellCommand { get; private set; } = null!;

    private void InitializeNavServices()
    {
        SelectNavItemCommand = ReactiveCommand.Create<DemoItemViewModel>(OpenItem);
        ResetRenamesCommand = ReactiveCommand.Create(ResetRenames);
        EscapeShellCommand = ReactiveCommand.Create(EscapeShellOverlay);
    }

    /// <summary>按关键字逐个叶子打标；命中时自动展开分组，方便直接看到结果。</summary>
    private void ApplyNavFilter()
    {
        var query = _navFilter.Trim();
        foreach (var group in Groups)
        {
            var matched = 0;
            foreach (var item in group.Items)
            {
                var hit = query.Length == 0
                          || item.DisplayTitle.Contains(query, StringComparison.OrdinalIgnoreCase)
                          || group.Title.Contains(query, StringComparison.OrdinalIgnoreCase);
                item.IsFilteredOut = !hit;
                if (hit)
                {
                    matched++;
                }
            }

            group.IsFilteredOut = matched == 0;
            if (query.Length > 0)
            {
                group.IsExpanded = matched > 0;
            }
        }

        this.RaisePropertyChanged(nameof(IsNavEmpty));
    }
}
