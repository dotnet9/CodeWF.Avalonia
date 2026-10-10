using Avalonia;
using Avalonia.Controls;

namespace CodeWF.Avalonia.DataGrid;

public sealed class DataGridEnhancement
{
    public static readonly AttachedProperty<bool> UseDefaultsProperty =
        AvaloniaProperty.RegisterAttached<DataGridEnhancement, global::Avalonia.Controls.DataGrid, bool>("UseDefaults");

    static DataGridEnhancement()
    {
        UseDefaultsProperty.Changed.AddClassHandler<global::Avalonia.Controls.DataGrid>(OnUseDefaultsChanged);
    }

    private DataGridEnhancement()
    {
    }

    public static bool GetUseDefaults(global::Avalonia.Controls.DataGrid dataGrid)
    {
        return dataGrid.GetValue(UseDefaultsProperty);
    }

    public static void SetUseDefaults(global::Avalonia.Controls.DataGrid dataGrid, bool value)
    {
        dataGrid.SetValue(UseDefaultsProperty, value);
    }

    private static void OnUseDefaultsChanged(global::Avalonia.Controls.DataGrid dataGrid, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.GetNewValue<bool>())
        {
            dataGrid.EnableDefaults();
        }
    }
}
