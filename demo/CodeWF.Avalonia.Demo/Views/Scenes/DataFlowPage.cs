using CodeWF.Avalonia.Demo.Views.Pages;

namespace CodeWF.Avalonia.Demo.Views.Scenes;

/// <summary>
/// 场景二：表数据工作流。
/// 步骤 = 导入数据 / 列配置 / 排序过滤 / 导出，内嵌 DataGrid、列配置编辑器与属性面板页。
/// </summary>
public class DataFlowPage : ScenePageBase
{
    public DataFlowPage()
    {
        AddStep("Demo.Scene.Step.Import", new DataGridPage());
        AddStep("Demo.Scene.Step.Columns", new ColumnDisplayEditorPage());
        AddStep("Demo.Scene.Step.SortFilter", new DataGridPage());
        AddStep("Demo.Scene.Step.ExportData", new PropertyGridPage());
        AttachSceneShell();
    }

    public override string TitleKey => "Demo.Page.DataFlow.Title";

    public override string DescriptionKey => "Demo.Page.DataFlow.Desc";

    protected override string SummaryKey => "Demo.Scene.Summary.Data";

    public override string XamlCode => """
        <!-- 表数据工作流：DataGrid 增强 + 列配置 + 导出 -->
        <DataGrid codewf:DataGridEnhancement.UseDefaults="True"
                  ItemsSource="{Binding Rows}" />

        <codewf:ColumnDisplayEditor Items="{Binding Columns}" />
        """;

    public override string CSharpCode => """
        // 1) 导入：一键打开三态排序 / 自然排序 / 智能 ToolTip
        dataGrid.EnableDefaults(itemsSource);

        // 2) 列配置：拖拽排序直接在源集合上移动
        Columns.Move(oldIndex, newIndex);

        // 3) 排序 / 过滤：三态循环，取消时恢复原始顺序
        dataGrid.AddNaturalSorting();

        // 4) 导出：DataTableExtensions 负责 Csv / Excel 落盘
        dataTable.ToExcel("order-2026.xlsx");
        """;
}
