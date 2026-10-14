using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.Views.Pages;

namespace CodeWF.Avalonia.Demo.Views.Scenes;

/// <summary>
/// 场景一：Markdown 写作全流程。
/// 步骤 = 打开文档 / 编辑 / 实时预览 / 导出复制 / 任务回写，每一步复用组件库里的同一批页面。
/// </summary>
public class MarkdownFlowPage : ScenePageBase
{
    public MarkdownFlowPage()
    {
        AddStep("Demo.Scene.Step.Open", new MarkdownViewerPage());
        AddStep("Demo.Scene.Step.Edit", new MarkdownEditorPage());
        AddStep("Demo.Scene.Step.Preview", new MarkdownLiveEditorPage());
        AddStep("Demo.Scene.Step.Export", new MarkdownExportPage());
        AddStep("Demo.Scene.Step.WriteBack", new MarkdownLitePage());
        AttachSceneShell();
    }

    public override string TitleKey => "Demo.Page.MarkdownFlow.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownFlow.Desc";

    protected override string SummaryKey => "Demo.Scene.Summary.Markdown";

    public override string XamlCode => L.Instance.GetResource("Demo.Content.MarkdownFlow.XamlCode.CodewfMarkdownViewerMarkdownTypographyThemeCodewfMarkdownEditorViewMarkdownShowLineNumbers");

    public override string CSharpCode => L.Instance.GetResource("Demo.Content.MarkdownFlow.CSharpCode.AddStepDemoSceneStepOpenNewMarkdownViewerPageAddStep");
}
