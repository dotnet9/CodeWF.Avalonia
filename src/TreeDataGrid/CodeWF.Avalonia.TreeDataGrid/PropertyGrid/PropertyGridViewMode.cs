namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性面板的显示模式。</summary>
public enum PropertyGridViewMode
{
    /// <summary>分类模式：显示分组和不限层级的属性树。</summary>
    Categorized,

    /// <summary>字母模式：去除分组和缩进，将属性按名称排序。</summary>
    Alphabetical,
}
