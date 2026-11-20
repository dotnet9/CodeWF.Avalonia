using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia.Collections;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>文档区上下文：DataTemplate 按 Context 类型匹配正文视图。</summary>
public record DocumentContext(string Text);

/// <summary>工具区上下文：同上。</summary>
public record ToolContext(string Text);

/// <summary>
/// Dock 演示页 VM：用 Dock.Model.ReactiveUI 构建工作区布局（左工具栏 + 文档区）。
/// 12.x 无 Content/DocumentTemplate：内容放进 Context，由 DockPage.axaml 的
/// DataTemplates 按 Context 类型解析为正文视图。
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
            Title = L.Instance.GetResource(DemoResources.Content.Dock.overviewTool.WorkspaceOverview),
            Context = new ToolContext(ToolOverviewText),
        };
        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            VisibleDockables = CreateList<IDockable>(overviewTool),
            ActiveDockable = overviewTool,
            Dock = DockMode.Left,
            Proportion = 0.28,
        };

        var designDocument = new Document { Id = "Doc.Design", Title = L.Instance.GetResource(DemoResources.Content.Dock.designDocument.DesignNotes), Context = new DocumentContext(DocDesignText) };
        var apiDocument = new Document { Id = "Doc.Api", Title = L.Instance.GetResource(DemoResources.Content.Dock.apiDocument.APIDraft), Context = new DocumentContext(DocApiText) };
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
            Title = L.Instance.GetResource(DemoResources.Content.Dock.Content.DockWorkspace),
            VisibleDockables = CreateList<IDockable>(mainDock),
            ActiveDockable = mainDock,
            IsCollapsable = false,
        };

        factory.InitLayout(Layout);
    }

    private static AvaloniaList<T> CreateList<T>(params T[] items) => new(items);

    private static string ToolOverviewText => L.Instance.GetResource(DemoResources.Content.Dock.ToolOverviewMarkdown.WorkspaceOverviewLeftToolPanelRightDraggableDocuments);

    private static string DocDesignText => L.Instance.GetResource(DemoResources.Content.Dock.DocDesignMarkdown.DesignNotesDockLayoutRootDockProportionalDockToolDockDocumentDock);

    private static string DocApiText => L.Instance.GetResource(DemoResources.Content.Dock.DocApiMarkdown.APIDraftCsharpVarFactoryNewDockModel);
}
