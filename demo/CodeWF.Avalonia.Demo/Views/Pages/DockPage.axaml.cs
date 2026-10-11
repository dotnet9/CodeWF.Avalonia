using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class DockPage : DemoPageBase
{
    public const string XamlSnippet = """
        <!-- 布局（RootDock 树）由 VM 构建，DockControl 只负责呈现与交互 -->
        <dock:DockControl xmlns:dock="using:Dock.Avalonia.Controls"
                          Layout="{Binding Layout}" />
        """;

    public const string CSharpSnippet = """
        // App.Styles 中引入 Semi 皮肤（含 Dock 内置文案的 zh-cn / en-us 本地化）：
        //   <codewf:DockSemiTheme Locale="zh-CN" />

        var factory = new Dock.Model.ReactiveUI.Factory();

        var document = new Document { Id = "Doc.1", Title = "设计说明", Content = markdown };
        var docDock = new DocumentDock
        {
            VisibleDockables = AvaloniaList<IDockable> [document],
            ActiveDockable = document,
            DocumentTemplate = markdownTemplate,   // 文档内容模板
            CanCreateDocument = false,
        };
        var mainDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = AvaloniaList<IDockable>
            [
                toolDock, new ProportionalDockSplitter(), docDock
            ],
        };
        var layout = new RootDock { VisibleDockables = AvaloniaList<IDockable> [mainDock] };
        factory.InitLayout(layout);
        """;

    public DockPage()
    {
        InitializeComponent();
        DataContext = new DockPageViewModel();
    }

    public override string TitleKey => "Demo.Page.Dock.Title";

    public override string DescriptionKey => "Demo.Page.Dock.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "dock.note",
            Name = "构成",
            Kind = PropertyItemKind.ReadOnly,
            Value = "Dock.Avalonia 12.1 + Dock.Model.ReactiveUI + CodeWF DockSemiTheme",
            Description = "CodeWF.Avalonia.Dock 提供 Semi 皮肤、梯形文档页签边框与固定工具区行为；布局引擎来自 Dock.Avalonia。",
        };

        return [note];
    }
}
