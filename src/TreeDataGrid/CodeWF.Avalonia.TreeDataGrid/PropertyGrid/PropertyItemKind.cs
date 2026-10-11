namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// Editor kinds of a property item. Leaves render the matching editor in the value cell./// </summary>
public enum PropertyItemKind
{
    /// <summary>Category header row (full width, collapses its children).</summary>
    Category,

    /// <summary>Single-line text (default).</summary>
    Text,

    /// <summary>Multi-line text (commits on focus loss, Esc reverts).</summary>
    MultilineText,

    /// <summary>Number (NumericUpDown); value should be decimal.</summary>
    Number,

    /// <summary>Boolean (CheckBox); commits on toggle.</summary>
    Boolean,

    /// <summary>Dropdown (ComboBox); value comes from <see cref="PropertyItem.Options"/>.</summary>
    Choice,

    /// <summary>Radio group (horizontal RadioButtons).</summary>
    Radio,

    /// <summary>Percent slider (live preview while dragging, commits on release); value is double (0-100).</summary>
    Percent,

    /// <summary>Color; value is a "#RRGGBB" string.</summary>
    Color,

    /// <summary>Path (text + "…" browse button); browsing is handled via BrowseRequested.</summary>
    Path,

    /// <summary>Full-row button (e.g. a "…" browse row); clicks raise BrowseRequested.</summary>
    Button,

    /// <summary>Date/time (text edited as "yyyy/MM/dd HH:mm"); value is DateTime.</summary>
    DateTime,

    /// <summary>Read-only display.</summary>
    ReadOnly,
}
