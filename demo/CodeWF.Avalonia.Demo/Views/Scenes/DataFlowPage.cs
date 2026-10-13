using L = global::CodeWF.Avalonia.Lang.I18nManager;
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

    public override string XamlCode => L.Instance.GetResource("Demo.Text.DataFlowPage.XamlCode001");

    public override string CSharpCode => L.Instance.GetResource("Demo.Text.DataFlowPage.CSharpCode002");
}
