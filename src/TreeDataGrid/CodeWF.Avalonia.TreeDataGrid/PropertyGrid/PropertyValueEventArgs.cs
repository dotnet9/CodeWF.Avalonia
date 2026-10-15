using Avalonia.Interactivity;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>属性值提交后的变更事件参数。</summary>
public class PropertyValueChangedEventArgs : RoutedEventArgs
{
    public PropertyValueChangedEventArgs(PropertyItem item, object? oldValue, object? newValue)
    {
        Item = item;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>发生变更的属性项。</summary>
    public PropertyItem Item { get; }

    /// <summary>变更前的值。</summary>
    public object? OldValue { get; }

    /// <summary>变更后的值。</summary>
    public object? NewValue { get; }
}

/// <summary>提交前的预览参数；处理程序可设置 <see cref="Cancel"/> 拒绝变更并回退编辑器。</summary>
public sealed class PropertyValuePreviewEventArgs : PropertyValueChangedEventArgs
{
    public PropertyValuePreviewEventArgs(PropertyItem item, object? oldValue, object? newValue) : base(item, oldValue, newValue)
    {
    }

    /// <summary>是否取消本次变更。</summary>
    public bool Cancel { get; set; }
}

/// <summary>绑定 <see cref="PropertyGrid.ValueCommittedCommand"/> 时传入的值变更参数。</summary>
public sealed record PropertyValueChange(PropertyItem Item, object? OldValue, object? NewValue);

/// <summary>路径或按钮编辑器触发浏览操作时的事件参数。</summary>
public sealed class PropertyBrowseRequestedEventArgs : RoutedEventArgs
{
    public PropertyBrowseRequestedEventArgs(PropertyItem item)
    {
        Item = item;
    }

    /// <summary>发起浏览请求的属性项。</summary>
    public PropertyItem Item { get; }
}
