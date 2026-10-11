using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// A property panel item: <see cref="Children"/> forms an unlimited-depth tree./// Items with children render as expandable rows (summary in the value cell); leaves render an editor per <see cref="Kind"/>./// Pure data model (no Avalonia types) for easy unit testing and cross-project reuse./// <para>Value type conventions: Number = decimal; Percent = double (0-100); Boolean = bool;/// Color = "#RRGGBB" string; Text/MultilineText/Choice/Radio/Path = string.</para>/// </summary>
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

    /// <summary>Optional unique identifier (for logs, command parameters, editor group names).</summary>
    public string? Id { get; set; }

    /// <summary>Display name (sort key for the alphabetical view).</summary>
    public string Name { get => _name; set => RaiseIfChanged(ref _name, value); }

    /// <summary>Description shown in the bottom description pane.</summary>
    public string? Description { get => _description; set => RaiseIfChanged(ref _description, value); }

    /// <summary>Editor kind (applies to leaf items).</summary>
    public PropertyItemKind Kind { get; set; } = PropertyItemKind.Text;

    /// <summary>Commit timing for text editors (number/boolean/combo always commit immediately).</summary>
    public PropertyCommitMode CommitMode { get; set; } = PropertyCommitMode.FocusLoss;

    /// <summary>Default value: a value differing from it bolds the name and enables reset.</summary>
    public object? DefaultValue { get; set; }

    /// <summary>Current value.</summary>
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

    /// <summary>Candidates for Choice/Radio.</summary>
    public IReadOnlyList<string>? Options { get; set; }

    /// <summary>Number minimum (null = unbounded).</summary>
    public double? Minimum { get; set; }

    /// <summary>Number maximum (null = unbounded).</summary>
    public double? Maximum { get; set; }

    /// <summary>Number step, default 1.</summary>
    public double Increment { get; set; } = 1;

    /// <summary>Number unit suffix (e.g. px, ms, °).</summary>
    public string? Unit { get; set; }

    /// <summary>Number format string (e.g. "0.0").</summary>
    public string? Format { get; set; }

    /// <summary>Watermark for text editors.</summary>
    public string? Watermark { get; set; }

    /// <summary>Read-only (renders as a read-only display).</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>Child items; non-null renders this item as an expandable row. Assignment maintains child Parent references.</summary>
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

    /// <summary>Expanded state (categorized view only; forced open while searching).</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => RaiseIfChanged(ref _isExpanded, value);
    }

    /// <summary>Selection state (maintained by the panel for row highlight and description pane).</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseIfChanged(ref _isSelected, value);
    }

    /// <summary>Per-row height override (px); null uses the panel default. Set by dragging the row divider, double-click resets.</summary>
    public double? RowHeight
    {
        get => _rowHeight;
        set => RaiseIfChanged(ref _rowHeight, value);
    }

    /// <summary>Summary text for expandable rows; with <see cref="SummaryProvider"/> the panel refreshes it on child commits.</summary>
    public string? Summary
    {
        get => _summary;
        set => RaiseIfChanged(ref _summary, value);
    }

    /// <summary>Summary provider: a null result keeps the existing summary. The panel refreshes along the ancestor chain.</summary>
    public Func<string?>? SummaryProvider { get; set; }

    /// <summary>Dynamic visibility: returning false hides the item (call panel Refresh to recompute).</summary>
    public Func<bool>? VisibleWhen { get; set; }

    /// <summary>Parent item (maintained by the panel/builder).</summary>
    public PropertyItem? Parent { get; internal set; }

    /// <summary>Tree depth (maintained by the builder; 0 = top level).</summary>
    public int Depth { get; internal set; }

    /// <summary>Whether this item's value differs from the default.</summary>
    public bool IsModified => DefaultValue is not null && !Equals(Value, DefaultValue);

    /// <summary>Whether this item or any descendant differs from the default (bold name for expandable items).</summary>
    public bool IsModifiedDeep
    {
        get => _isModifiedDeep;
        private set => RaiseIfChanged(ref _isModifiedDeep, value);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Refreshes the summary: invokes <see cref="SummaryProvider"/>; a non-null result updates <see cref="Summary"/>.</summary>
    public void RefreshSummary()
    {
        var summary = SummaryProvider?.Invoke();
        if (summary is not null)
        {
            Summary = summary;
        }
    }

    /// <summary>Resets this item's value to the default.</summary>
    public void Reset()
    {
        if (DefaultValue is not null)
        {
            Value = DefaultValue;
        }
    }

    /// <summary>Resets this item and all descendants.</summary>
    public void ResetSubtree()
    {
        Reset();
        foreach (var descendant in Descendants())
        {
            descendant.Reset();
        }
    }

    /// <summary>Recomputes IsModifiedDeep along the tree (once after data attach; afterwards maintained automatically on value changes).</summary>
    public void RecalculateModifiedDeep()
    {
        foreach (var child in Children ?? [])
        {
            child.RecalculateModifiedDeep();
        }

        IsModifiedDeep = IsModified || (Children?.Any(static child => child.IsModifiedDeep) ?? false);
    }

    /// <summary>Enumerates all descendants (depth-first, excluding self).</summary>
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

    /// <summary>Enumerates self and all descendants.</summary>
    public IEnumerable<PropertyItem> SelfAndDescendants()
    {
        yield return this;
        foreach (var descendant in Descendants())
        {
            yield return descendant;
        }
    }

    /// <summary>Recomputes IsModifiedDeep along the ancestor chain after a value change.</summary>
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
