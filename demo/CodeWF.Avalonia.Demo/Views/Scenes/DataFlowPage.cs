using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
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
        AddStep(DemoResources.Scene.Step.Import, new DataGridPage());
        AddStep(DemoResources.Scene.Step.Columns, new ColumnDisplayEditorPage());
        AddStep(DemoResources.Scene.Step.SortFilter, new DataGridPage());
        AddStep(DemoResources.Scene.Step.ExportData, new PropertyGridPage());
        AttachSceneShell();
    }

    public override string TitleKey => DemoResources.Page.DataFlow.Title;

    public override string DescriptionKey => DemoResources.Page.DataFlow.Desc;

    protected override string SummaryKey => DemoResources.Scene.Summary.Data;

    public override string XamlCode => L.Instance.GetResource(DemoResources.Content.DataFlow.XamlCode.DataGridCodewfDataGridEnhancementUseDefaultsTrueItemsSourceCodewfColumnDisplayEditor);

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.DataFlow.CSharpCode.DataGridEnableDefaultsItemsSourceColumnsMoveOldIndexNewIndexDataGrid);
}
