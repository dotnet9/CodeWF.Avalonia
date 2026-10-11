using Avalonia.Interactivity;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// Arguments of a property value change (raised after commit)./// </summary>
public class PropertyValueChangedEventArgs : RoutedEventArgs
{
    public PropertyValueChangedEventArgs(PropertyItem item, object? oldValue, object? newValue)
    {
        Item = item;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>The changed property item.</summary>
    public PropertyItem Item { get; }

    /// <summary>The value before the change.</summary>
    public object? OldValue { get; }

    /// <summary>The value after the change.</summary>
    public object? NewValue { get; }
}

/// <summary>
/// Preview arguments before a value commit: handlers may set <see cref="Cancel"/> to reject the change (the editor rolls back)./// </summary>
public sealed class PropertyValuePreviewEventArgs : PropertyValueChangedEventArgs
{
    public PropertyValuePreviewEventArgs(PropertyItem item, object? oldValue, object? newValue) : base(item, oldValue, newValue)
    {
    }

    /// <summary>Whether to cancel the change.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Value change command parameter (for binding <see cref="PropertyGrid.ValueCommittedCommand"/>)./// </summary>
public sealed record PropertyValueChange(PropertyItem Item, object? OldValue, object? NewValue);

/// <summary>
/// Browse request arguments (raised when Path/Button editors click "…")./// </summary>
public sealed class PropertyBrowseRequestedEventArgs : RoutedEventArgs
{
    public PropertyBrowseRequestedEventArgs(PropertyItem item)
    {
        Item = item;
    }

    /// <summary>The property item that raised the browse request.</summary>
    public PropertyItem Item { get; }
}
