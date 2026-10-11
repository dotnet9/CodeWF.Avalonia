using Avalonia.Controls;
using CodeWF.Avalonia.DataGrid;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class DataGridPage : DemoPageBase
{
    private const string IdUseDefaults = "dg.useDefaults";
    private const string IdRowHeight = "dg.rowHeight";

    public const string XamlSnippet = """
        <!-- 附加属性一键启用：三态排序 + 自然排序 + 智能 ToolTip + Ctrl+A 全选 -->
        <DataGrid codewf:DataGridEnhancement.UseDefaults="True"
                  ItemsSource="{Binding Rows}"
                  AutoGenerateColumns="False" CanUserSortColumns="True"
                  IsReadOnly="True" HeadersVisibility="Column">
          <DataGrid.Columns>
            <DataGridTextColumn Header="名称" Binding="{Binding Name}" Width="140" />
            <DataGridTextColumn Header="备注" Binding="{Binding Comment}" Width="*" />
          </DataGrid.Columns>
        </DataGrid>
        """;

    public const string CSharpSnippet = """
        // 等价的扩展方法形态，可按需开关单项能力
        dataGrid.EnableDefaults(dataGrid.ItemsSource as IEnumerable);
        // 等价于：
        dataGrid.AddSorting();            // 升 → 降 → 取消，取消时恢复原始顺序
        dataGrid.AddNaturalSorting();     // item2 排在 item10 之前
        dataGrid.EnableSmartTooltips();   // 仅对被截断的单元格显示 ToolTip，可传列索引

        // 附加属性 / 扩展方法二选一
        DataGridEnhancement.SetUseDefaults(dataGrid, true);

        // 性能预设（大数据量）
        dataGrid.ApplyPerformancePreset();
        """;

    public DataGridPage()
    {
        InitializeComponent();
        DataContext = new DataGridPageViewModel();
    }

    public override string TitleKey => "Demo.Page.DataGrid.Title";

    public override string DescriptionKey => "Demo.Page.DataGrid.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdUseDefaults when change.NewValue is bool useDefaults:
                DataGridEnhancement.SetUseDefaults(EnhancedGrid, useDefaults);
                break;
            case IdRowHeight when change.NewValue is decimal rowHeight:
                EnhancedGrid.RowHeight = (double)rowHeight;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var useDefaults = new PropertyItem
        {
            Id = IdUseDefaults,
            Name = "一键增强",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "上方网格的 UseDefaults 开关（三态排序 / 自然排序 / 智能 ToolTip / 全选）。",
        };
        var rowHeight = new PropertyItem
        {
            Id = IdRowHeight,
            Name = "行高",
            Kind = PropertyItemKind.Number,
            Value = 32m,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = "增强版网格行高。",
        };

        return [useDefaults, rowHeight];
    }

}
