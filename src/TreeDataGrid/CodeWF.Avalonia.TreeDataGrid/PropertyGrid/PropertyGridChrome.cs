// 通过 x:Static 提供界面文本，在语言切换后、重建行之前刷新。
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

public static class PropertyGridChrome
{
    public static string ResetToDefault { get; set; } = "Reset to default value";

    public static string ToggleChildren { get; set; } = "Expand/collapse children";
}
