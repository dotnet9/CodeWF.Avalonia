using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.Demo.Views.Pages;
using CodeWF.Avalonia.Demo.Views.Scenes;
using System.Collections.ObjectModel;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 演示目录：分组与页签一一镜像（组节点 = Ribbon 页签名，叶子 = 演示命令）。
/// 信息架构与设计原型一致 ——「场景优先（3 条工作流）+ 组件库分区（3 个页签）」，原页面一页不删。
/// Prism 只做 Shell 解析，页面与 VM 不进容器，按目录工厂直接构造。
/// </summary>
public static class DemoCatalog
{
    public static ObservableCollection<DemoGroupViewModel> CreateGroups()
    {
        return
        [
            new(DemoKeys.GroupHome,
            [
                new DemoItemViewModel(DemoKeys.ItemWelcome, () => new WelcomePage()),
            ]),
            // ── 场景页：完整工作流，内部按步骤复用下面的组件页 ──
            new(DemoKeys.GroupMarkdown,
            [
                new DemoItemViewModel(DemoKeys.ItemMarkdownFlow, () => new MarkdownFlowPage()),
                new DemoItemViewModel(DemoKeys.ItemMarkdownViewer, () => new MarkdownViewerPage()),
                new DemoItemViewModel(DemoKeys.ItemMarkdownLite, () => new MarkdownLitePage()),
                new DemoItemViewModel(DemoKeys.ItemMarkdownEditor, () => new MarkdownEditorPage()),
                new DemoItemViewModel(DemoKeys.ItemMarkdownLiveEditor, () => new MarkdownLiveEditorPage()),
                new DemoItemViewModel(DemoKeys.ItemMarkdownExport, () => new MarkdownExportPage()),
            ]),
            new(DemoKeys.GroupData,
            [
                new DemoItemViewModel(DemoKeys.ItemDataFlow, () => new DataFlowPage()),
                new DemoItemViewModel(DemoKeys.ItemDataGrid, () => new DataGridPage()),
                new DemoItemViewModel(DemoKeys.ItemPropertyGrid, () => new PropertyGridPage()),
            ]),
            new(DemoKeys.GroupDocs,
            [
                new DemoItemViewModel(DemoKeys.ItemDocsFlow, () => new DocsFlowPage()),
                new DemoItemViewModel(DemoKeys.ItemDock, () => new DockPage()),
                new DemoItemViewModel(DemoKeys.ItemTrapezoidTab, () => new TrapezoidTabPage()),
            ]),
            // ── 组件库分区 ──
            new(DemoKeys.GroupInput,
            [
                new DemoItemViewModel(DemoKeys.ItemSearchListBox, () => new SearchListBoxPage()),
                new DemoItemViewModel(DemoKeys.ItemTransfer, () => new TransferPage()),
                new DemoItemViewModel(DemoKeys.ItemMarkupExtension, () => new MarkupExtensionPage()),
            ]),
            new(DemoKeys.GroupPresent,
            [
                new DemoItemViewModel(DemoKeys.ItemStatus, () => new StatusComponentsPage()),
                new DemoItemViewModel(DemoKeys.ItemGuide, () => new GuidePage()),
                new DemoItemViewModel(DemoKeys.ItemAppGuide, () => new AppGuidePage()),
                new DemoItemViewModel(DemoKeys.ItemWindowedStackPanel, () => new WindowedStackPanelPage()),
                new DemoItemViewModel(DemoKeys.ItemColumnDisplayEditor, () => new ColumnDisplayEditorPage()),
            ]),
            new(DemoKeys.GroupInfrastructure,
            [
                new DemoItemViewModel(DemoKeys.ItemLog, () => new LogPage()),
                new DemoItemViewModel(DemoKeys.ItemLang, () => new LangPage()),
                new DemoItemViewModel(DemoKeys.ItemCodeWFWindow, () => new CodeWFWindowPage()),
            ]),
        ];
    }
}
