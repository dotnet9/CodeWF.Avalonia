using Avalonia.Collections;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>文档区上下文：DataTemplate 按 Context 类型匹配 MarkdownViewer。</summary>
public record MarkdownDocumentContext(string Markdown);

/// <summary>工具区上下文：同上。</summary>
public record MarkdownToolContext(string Markdown);

/// <summary>
/// Dock 演示页 VM：用 Dock.Model.ReactiveUI 构建工作区布局（左工具栏 + 文档区）。
/// 12.x 无 Content/DocumentTemplate：内容放进 Context，由 DockPage.axaml 的
/// DataTemplates 按 Context 类型解析为 MarkdownViewer。
/// </summary>
public class DockPageViewModel
{
    public RootDock Layout { get; }

    public DockPageViewModel()
    {
        var factory = new Factory();

        var overviewTool = new Tool
        {
            Id = "Tool.Overview",
            Title = "工作区概览",
            Context = new MarkdownToolContext(ToolOverviewMarkdown),
        };
        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            VisibleDockables = CreateList<IDockable>(overviewTool),
            ActiveDockable = overviewTool,
            Dock = DockMode.Left,
            Proportion = 0.28,
        };

        var designDocument = new Document { Id = "Doc.Design", Title = "设计说明", Context = new MarkdownDocumentContext(DocDesignMarkdown) };
        var apiDocument = new Document { Id = "Doc.Api", Title = "接口草稿", Context = new MarkdownDocumentContext(DocApiMarkdown) };
        var docDock = new DocumentDock
        {
            Id = "DocDock",
            VisibleDockables = CreateList<IDockable>(designDocument, apiDocument),
            ActiveDockable = designDocument,
            CanCreateDocument = false,
        };

        var mainDock = new ProportionalDock
        {
            Id = "MainDock",
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                toolDock,
                new ProportionalDockSplitter(),
                docDock),
            ActiveDockable = docDock,
        };

        Layout = new RootDock
        {
            Id = "Root",
            Title = "Dock 工作区",
            VisibleDockables = CreateList<IDockable>(mainDock),
            ActiveDockable = mainDock,
            IsCollapsable = false,
        };

        factory.InitLayout(Layout);
    }

    private static AvaloniaList<T> CreateList<T>(params T[] items) => new(items);

    private const string ToolOverviewMarkdown = """
        ### 工作区概览

        - 左侧：工具面板（本面板）
        - 右侧：文档区（可拖拽 / 停靠 / 浮动）
        - 页签：双击可最大化工作区

        试试拖动文档页签重新停靠。
        """;

    private const string DocDesignMarkdown = """
        # 设计说明

        Dock.Avalonia 的布局模型：**RootDock → ProportionalDock → ToolDock / DocumentDock**。

        | 概念 | 说明 |
        | --- | --- |
        | RootDock | 布局根，含隐藏停靠物集合 |
        | ProportionalDock | 比例分割容器 |
        | ToolDock | 工具停靠区（左 / 右 / 上 / 下） |
        | DocumentDock | 文档页签区 |

        本页内容存放在 Document.Context，由页面级 DataTemplate 解析为 MarkdownViewer。
        """;

    private const string DocApiMarkdown = """
        # 接口草稿

        ```csharp
        var factory = new Dock.Model.ReactiveUI.Factory();
        var layout = new RootDock { VisibleDockables = [mainDock] };
        factory.InitLayout(layout);
        ```
        """;
}
