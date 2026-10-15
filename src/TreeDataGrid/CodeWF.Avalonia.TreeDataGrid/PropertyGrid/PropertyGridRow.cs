namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性面板的可见行模型，由属性项和显示层级组成。</summary>
public abstract class PropertyGridRow
{
    protected PropertyGridRow(PropertyItem item, int depth)
    {
        Item = item;
        Depth = depth;
    }

    /// <summary>当前行对应的属性项。</summary>
    public PropertyItem Item { get; }

    /// <summary>显示层级，顶层为 0，用于缩进和层级引导线。</summary>
    public int Depth { get; }
}

/// <summary>占满整行的分组标题，单击可展开或折叠子项。</summary>
public sealed class PropertyCategoryRow : PropertyGridRow
{
    public PropertyCategoryRow(PropertyItem item, int depth) : base(item, depth)
    {
    }
}

/// <summary>由名称单元格和值编辑单元格组成的属性行。</summary>
public sealed class PropertyLeafRow : PropertyGridRow
{
    public PropertyLeafRow(PropertyItem item, int depth, bool isFlat) : base(item, depth)
    {
        IsFlat = isFlat;
    }

    /// <summary>是否为无展开按钮和缩进的字母排序行。</summary>
    public bool IsFlat { get; }

    /// <summary>是否显示展开按钮，仅有子项且非平铺时显示。</summary>
    public bool ShowExpander => Item.HasChildren && !IsFlat;
}
