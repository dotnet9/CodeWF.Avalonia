namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// Row view models for the property panel: a visible row = model item + display depth./// </summary>
public abstract class PropertyGridRow
{
    protected PropertyGridRow(PropertyItem item, int depth)
    {
        Item = item;
        Depth = depth;
    }

    /// <summary>The property item behind the row.</summary>
    public PropertyItem Item { get; }

    /// <summary>Display depth (0 = top level); drives indent and level guides.</summary>
    public int Depth { get; }
}

/// <summary>Category header row (full width; click collapses/expands children).</summary>
public sealed class PropertyCategoryRow : PropertyGridRow
{
    public PropertyCategoryRow(PropertyItem item, int depth) : base(item, depth)
    {
    }
}

/// <summary>Property row (name cell + editor value cell).</summary>
public sealed class PropertyLeafRow : PropertyGridRow
{
    public PropertyLeafRow(PropertyItem item, int depth, bool isFlat) : base(item, depth)
    {
        IsFlat = isFlat;
    }

    /// <summary>Whether this is a flat alphabetical row (no expander, no indentation).</summary>
    public bool IsFlat { get; }

    /// <summary>Whether the expander is shown (has children and not flat).</summary>
    public bool ShowExpander => Item.HasChildren && !IsFlat;
}
