using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性面板的数据项，<see cref="Children"/> 组成不限层级的树。有子项时显示可展开的汇总行，叶子项按 <see cref="Kind"/> 显示编辑器。模型不依赖 Avalonia 类型。数值使用 decimal，百分比使用 double，布尔值使用 bool，颜色及文本类使用字符串。</summary>
public class PropertyItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string? _description;
    private object? _value;
    private bool _isExpanded = true;
    private bool _isSelected;
    private double? _rowHeight;
    private string? _summary;
    private bool _isModifiedDeep;
    private IReadOnlyList<PropertyItem>? _children;

    /// <summary>可选的唯一标识，用于日志、命令参数和编辑器分组。</summary>
    public string? Id { get; set; }

    /// <summary>显示名称，也是字母模式的排序依据。</summary>
    public string Name { get => _name; set => RaiseIfChanged(ref _name, value); }

    /// <summary>在底部说明区域显示的描述。</summary>
    public string? Description { get => _description; set => RaiseIfChanged(ref _description, value); }

    /// <summary>叶子项使用的编辑器类型。</summary>
    public PropertyItemKind Kind { get; set; } = PropertyItemKind.Text;

    /// <summary>文本编辑器的提交时机，数值、布尔值和下拉框始终立即提交。</summary>
    public PropertyCommitMode CommitMode { get; set; } = PropertyCommitMode.FocusLoss;

    /// <summary>默认值，当前值与其不同时名称加粗并允许重置。</summary>
    public object? DefaultValue { get; set; }

    /// <summary>当前值。</summary>
    public object? Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value))
            {
                return;
            }

            _value = value;
            OnPropertyChanged();
            UpdateModifiedChain();
        }
    }

    /// <summary>下拉框和单选按钮的候选项。</summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>数值下限，空值表示不限制。</summary>
    public double? Minimum { get; set; }

    /// <summary>数值上限，空值表示不限制。</summary>
    public double? Maximum { get; set; }

    /// <summary>数值步长，默认为 1。</summary>
    public double Increment { get; set; } = 1;

    /// <summary>数值单位后缀，例如 px、ms。</summary>
    public string? Unit { get; set; }

    /// <summary>数值格式字符串。</summary>
    public string? Format { get; set; }

    /// <summary>文本编辑器的占位提示。</summary>
    public string? Watermark { get; set; }

    /// <summary>是否只读，只读时仅显示值。</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>子项集合，有子项时显示可展开行；赋值时同步子项的 Parent 引用。</summary>
    public IReadOnlyList<PropertyItem>? Children
    {
        get => _children;
        set
        {
            if (ReferenceEquals(_children, value))
            {
                return;
            }

            _children = value;
            if (_children is not null)
            {
                foreach (var child in _children)
                {
                    child.Parent = this;
                }
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasChildren));
        }
    }

    public bool HasChildren => Children is { Count: > 0 };

    /// <summary>展开状态，仅分类模式生效，搜索时自动展开匹配分支。</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => RaiseIfChanged(ref _isExpanded, value);
    }

    /// <summary>选择状态，由面板维护，用于行高亮和说明区域。</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseIfChanged(ref _isSelected, value);
    }

    /// <summary>当前行的自定义高度，单位为像素；空值使用默认高度，拖动行分隔线调整，双击重置。</summary>
    public double? RowHeight
    {
        get => _rowHeight;
        set => RaiseIfChanged(ref _rowHeight, value);
    }

    /// <summary>可展开行的汇总文本；设置 <see cref="SummaryProvider"/> 后，子项提交时自动刷新。</summary>
    public string? Summary
    {
        get => _summary;
        set => RaiseIfChanged(ref _summary, value);
    }

    /// <summary>汇总值提供函数，返回空值时保留现有汇总；面板沿祖先链刷新。</summary>
    public Func<string?>? SummaryProvider { get; set; }

    /// <summary>动态可见性，返回 false 时隐藏当前项及子树，调用面板 Refresh 重新计算。</summary>
    public Func<bool>? VisibleWhen { get; set; }

    /// <summary>父属性项，由面板和行构建器维护。</summary>
    public PropertyItem? Parent { get; internal set; }

    /// <summary>树层级，顶层为 0，由行构建器维护。</summary>
    public int Depth { get; internal set; }

    /// <summary>当前值是否与默认值不同。</summary>
    public bool IsModified => DefaultValue is not null && !Equals(Value, DefaultValue);

    /// <summary>当前项或后代是否存在修改，用于可展开行的名称加粗。</summary>
    public bool IsModifiedDeep
    {
        get => _isModifiedDeep;
        private set => RaiseIfChanged(ref _isModifiedDeep, value);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>调用 <see cref="SummaryProvider"/> 刷新汇总，返回非空值时更新 <see cref="Summary"/>。</summary>
    public void RefreshSummary()
    {
        var summary = SummaryProvider?.Invoke();
        if (summary is not null)
        {
            Summary = summary;
        }
    }

    /// <summary>将当前项恢复为默认值。</summary>
    public void Reset()
    {
        if (DefaultValue is not null)
        {
            Value = DefaultValue;
        }
    }

    /// <summary>重置当前项及其所有后代。</summary>
    public void ResetSubtree()
    {
        Reset();
        foreach (var descendant in Descendants())
        {
            descendant.Reset();
        }
    }

    /// <summary>沿属性树重新计算 IsModifiedDeep，数据首次挂载时调用，后续由值变更自动维护。</summary>
    public void RecalculateModifiedDeep()
    {
        foreach (var child in Children ?? [])
        {
            child.RecalculateModifiedDeep();
        }

        IsModifiedDeep = IsModified || (Children?.Any(static child => child.IsModifiedDeep) ?? false);
    }

    /// <summary>按深度优先顺序枚举所有后代，不含当前项。</summary>
    public IEnumerable<PropertyItem> Descendants()
    {
        if (Children is null)
        {
            yield break;
        }

        foreach (var child in Children)
        {
            yield return child;
            foreach (var grandchild in child.Descendants())
            {
                yield return grandchild;
            }
        }
    }

    /// <summary>枚举当前项及其所有后代。</summary>
    public IEnumerable<PropertyItem> SelfAndDescendants()
    {
        yield return this;
        foreach (var descendant in Descendants())
        {
            yield return descendant;
        }
    }

    /// <summary>值变更后沿祖先链重新计算 IsModifiedDeep。</summary>
    private void UpdateModifiedChain()
    {
        for (var node = this; node is not null; node = node.Parent)
        {
            var deep = node.IsModified || node.Children?.Any(static child => child.IsModifiedDeep) == true;
            node.IsModifiedDeep = deep;
        }
    }

    private void RaiseIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
