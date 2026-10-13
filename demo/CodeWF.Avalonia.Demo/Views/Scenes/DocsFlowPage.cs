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
        AddStep("Demo.Scene.Step.Dock", new DockPage());
        AddStep("Demo.Scene.Step.Tabs", new TrapezoidTabPage());
        AddStep("Demo.Scene.Step.Panels", new LogPage());
        AddStep("Demo.Scene.Step.Status", new StatusComponentsPage());
        AttachSceneShell();
    }

    public override string TitleKey => "Demo.Page.DocsFlow.Title";

    public override string DescriptionKey => "Demo.Page.DocsFlow.Desc";

    protected override string SummaryKey => "Demo.Scene.Summary.Docs";

    public override string XamlCode => L.Instance.GetResource("Demo.Text.DocsFlowPage.XamlCode001");

    public override string CSharpCode => L.Instance.GetResource("Demo.Text.DocsFlowPage.CSharpCode002");
}
