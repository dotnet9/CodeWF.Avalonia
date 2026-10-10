using Avalonia.Controls;

namespace CodeWF.Avalonia.DataGrid;

/// <summary>
/// 大数据量场景的性能预设。Avalonia DataGrid 行/列虚拟化恒开，
/// 无可关闭项，本方法保留调用点语义、不再做额外设置
/// （替代原 ProDataGrid 同名能力，行为由控件本身保证）。
/// </summary>
public static class DataGridPerformanceExtensions
{
    public static void ApplyPerformancePreset(this global::Avalonia.Controls.DataGrid dataGrid)
    {
        _ = dataGrid;
    }
}
