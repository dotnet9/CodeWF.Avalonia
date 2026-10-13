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

    public override string XamlCode => """
        <!-- 多文档工作区：文档内容走 Context + DataTemplate -->
        <dock:DockControl Layout="{Binding Layout}">
          <DockControl.DataTemplates>
            <DataTemplate DataType="vm:DocContext">
              <md:MarkdownViewer Markdown="{Binding Markdown}" />
            </DataTemplate>
          </DockControl.DataTemplates>
        </dock:DockControl>
        """;

    public override string CSharpCode => """
        // 12.x 文档内容放 Context，由 DataTemplate 解析成视图
        var doc = new Document
        {
            Title = "设计说明",
            Context = new DocContext(markdown),
        };

        // 面板显隐 / 恢复走同一套工厂方法，布局重置整体重建
        factory.HideDockable(tool);
        factory.RestoreDockable(tool.Id);
        """;
}
