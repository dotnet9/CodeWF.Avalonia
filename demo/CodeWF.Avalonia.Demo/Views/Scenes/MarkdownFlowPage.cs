using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
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
        AddStep(DemoResources.Scene.Step.Open, new MarkdownViewerPage());
        AddStep(DemoResources.Scene.Step.Edit, new MarkdownEditorPage());
        AddStep(DemoResources.Scene.Step.Preview, new MarkdownLiveEditorPage());
        AddStep(DemoResources.Scene.Step.Export, new MarkdownExportPage());
        AddStep(DemoResources.Scene.Step.WriteBack, new MarkdownLitePage());
        AttachSceneShell();
    }

    public override string TitleKey => DemoResources.Page.MarkdownFlow.Title;

    public override string DescriptionKey => DemoResources.Page.MarkdownFlow.Desc;

    protected override string SummaryKey => DemoResources.Scene.Summary.Markdown;

    public override string XamlCode => L.Instance.GetResource(DemoResources.Content.MarkdownFlow.XamlCode.CodewfMarkdownViewerMarkdownTypographyThemeCodewfMarkdownEditorViewMarkdownShowLineNumbers);

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.MarkdownFlow.CSharpCode.AddStepDemoSceneStepOpenNewMarkdownViewerPageAddStep);
}
