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

    public override string XamlCode => """
        <!-- 场景页外壳由 ScenePageBase 构建：工作流步骤条 + 内嵌组件页 -->
        <codewf:MarkdownViewer Markdown="{Binding Content}"
                               TypographyTheme="{Binding Theme}" />
        <codewf:MarkdownEditorView Markdown="{Binding Source}" ShowLineNumbers="True" />
        <codewf:MarkdownLiveEditorView Markdown="{Binding Doc}" />
        """;

    public override string CSharpCode => """
        // 场景页把「打开 → 编辑 → 预览 → 导出 → 回写」编成一条工作流，
        // 每一步直接复用组件库里的同一个页面实例（不重复实现）：
        AddStep("Demo.Scene.Step.Open", new MarkdownViewerPage());
        AddStep("Demo.Scene.Step.Edit", new MarkdownEditorPage());
        AddStep("Demo.Scene.Step.Preview", new MarkdownLiveEditorPage());
        AddStep("Demo.Scene.Step.Export", new MarkdownExportPage());
        AddStep("Demo.Scene.Step.WriteBack", new MarkdownLitePage());
        AttachSceneShell();

        // 任务勾选回写源文由 MarkdownViewer 发起：
        viewer.TaskWriteBackRequested += (_, e) => e.Result.Markdown = e.Rendered;
        """;
}
