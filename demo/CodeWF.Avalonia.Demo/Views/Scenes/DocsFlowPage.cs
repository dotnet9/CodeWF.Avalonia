using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.Views.Pages;

namespace CodeWF.Avalonia.Demo.Views.Scenes;

/// <summary>
/// 场景三：多文档工作流。
/// 步骤 = 停靠布局 / 多文档页签 / 面板联动 / 状态反馈，演示 Dock 工作区与配套面板的协作。
/// </summary>
public class DocsFlowPage : ScenePageBase
{
    public DocsFlowPage()
    {
        AddStep(DemoResources.Scene.Step.Dock, new DockPage());
        AddStep(DemoResources.Scene.Step.Tabs, new TrapezoidTabPage());
        AddStep(DemoResources.Scene.Step.Panels, new LogPage());
        AddStep(DemoResources.Scene.Step.Status, new StatusComponentsPage());
        AttachSceneShell();
    }

    public override string TitleKey => DemoResources.Page.DocsFlow.Title;

    public override string DescriptionKey => DemoResources.Page.DocsFlow.Desc;

    protected override string SummaryKey => DemoResources.Scene.Summary.Docs;

    public override string XamlCode => L.Instance.GetResource(DemoResources.Content.DocsFlow.XamlCode.DockDockControlLayoutDockControlDataTemplatesDataTemplateDataTypeVm);

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.DocsFlow.CSharpCode.VarDocNewDocumentFactoryHideDockableToolFactory);
}
