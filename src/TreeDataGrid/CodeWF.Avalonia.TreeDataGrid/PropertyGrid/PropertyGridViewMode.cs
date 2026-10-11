namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// View modes of the property panel./// </summary>
public enum PropertyGridViewMode
{
    /// <summary>Categorized view: renders the model tree (category rows + unlimited-depth children).</summary>
    Categorized,

    /// <summary>Alphabetical view: strips groups and depth; all properties sorted by name in a flat table.</summary>
    Alphabetical,
}
