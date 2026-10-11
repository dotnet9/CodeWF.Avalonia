using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;
using CodeWF.Avalonia.Markdown.Controls;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>
/// Dock 演示页 VM：用 Dock.Model.ReactiveUI 构建工作区布局（左工具栏 + 文档区），
/// DocumentTemplate 用完整包 MarkdownViewer 呈现文档内容。
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
            Content = ToolOverviewMarkdown,
        };
        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            VisibleDockables = CreateList<IDockable>(overviewTool),
            ActiveDockable = overviewTool,
            ToolTemplate = ToolTemplate,
            Dock = DockMode.Left,
            Proportion = 0.28,
        };

        var designDocument = new Document { Id = "Doc.Design", Title = "设计说明", Content = DocDesignMarkdown };
        var apiDocument = new Document { Id = "Doc.Api", Title = "接口草稿", Content = DocApiMarkdown };
        var docDock = new DocumentDock
        {
            Id = "DocDock",
            VisibleDockables = CreateList<IDockable>(designDocument, apiDocument),
            ActiveDockable = designDocument,
            DocumentTemplate = DocumentTemplate,
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

        本页内容由 DocumentTemplate 呈现为 MarkdownViewer。
        """;

    private const string DocApiMarkdown = """
        # 接口草稿

        ```csharp
        var factory = new Dock.Model.ReactiveUI.Factory();
        var layout = new RootDock { VisibleDockables = [mainDock] };
        factory.InitLayout(layout);
        ```
        """;

    /// <summary>文档内容模板：兼容 Document 与裸字符串两种呈现对象。</summary>
    public static IDataTemplate DocumentTemplate { get; } =
        new FuncDataTemplate<object?>((data, _) =>
        {
            var markdown = data switch
            {
                Document { Content: string text } => text,
                string text => text,
                _ => string.Empty,
            };

            return new MarkdownViewer { Markdown = markdown, Margin = new Thickness(8) };
        });

    /// <summary>工具内容模板。</summary>
    public static IDataTemplate ToolTemplate { get; } =
        new FuncDataTemplate<object?>((data, _) =>
        {
            if (data is Tool { Content: string markdown })
            {
                return new MarkdownViewer { Markdown = markdown, Margin = new Thickness(8) };
            }

            return new TextBlock { Text = data?.ToString() ?? string.Empty, Margin = new Thickness(8) };
        });
}
