using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class PropertyGridPage : DemoPageBase
{
    private const string IdViewMode = "pg.viewMode";
    private const string IdToolBar = "pg.toolbar";
    private const string IdDescriptionPane = "pg.descriptionPane";
    private const string IdNameWidth = "pg.nameWidth";

    public const string XamlSnippet = """
        <!-- 分类/字母视图、搜索过滤、描述面板、修改加粗、列宽拖拽 -->
        <codewf:PropertyGrid Items="{Binding Items}"
                             ViewMode="Categorized"
                             ShowToolBar="True"
                             ShowDescriptionPane="True"
                             SearchText="{Binding SearchText}"
                             ValueCommittedCommand="{Binding ValueCommittedCommand}"
                             BrowseRequestedCommand="{Binding BrowseRequestedCommand}" />
        """;

    public const string CSharpSnippet = """
        // 数据模型是纯 POCO：PropertyItem 树（Category → 子项/子分组）
        var name = new PropertyItem { Id = "n", Name = "名称", Kind = PropertyItemKind.Text, Value = "阀门-01" };
        var visible = new PropertyItem { Id = "v", Name = "可见", Kind = PropertyItemKind.Boolean, Value = true };
        var appearance = new PropertyItem
        {
            Id = "g1", Name = "外观", Kind = PropertyItemKind.Category,
            Children = [name, visible],
        };

        // 摘要链与动态可见性
        position.SummaryProvider = () => $"{posX.Value}, {posY.Value}";
        backoffFactor.VisibleWhen = () => Equals(backoff.Value, true);

        // 控件能力
        grid.ExpandAll();
        grid.CollapseAll();
        grid.ResetAll();
        grid.Select(item);
        grid.CommitValue(item, newValue);      // 程序化提交（触发加粗与摘要刷新）
        grid.ValueCommitted += (_, e) => Log(e.Item.Name, e.OldValue, e.NewValue);

        // 11 种编辑器：Text/MultilineText/Number/Boolean/Choice/Radio/Percent/Color/Path/Button/DateTime/ReadOnly
        """;

    public PropertyGridPage()
    {
        InitializeComponent();
        DataContext = new PropertyPanelDemoViewModel();
    }

    private PropertyGrid Grid => DemoGrid;

    public override string TitleKey => "Demo.Page.PropertyGrid.Title";

    public override string DescriptionKey => "Demo.Page.PropertyGrid.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        if (DataContext is not PropertyPanelDemoViewModel vm)
        {
            return;
        }

        switch (change.Item.Id)
        {
            case IdViewMode when change.NewValue is string mode:
                vm.ViewMode = mode == "字母" ? PropertyGridViewMode.Alphabetical : PropertyGridViewMode.Categorized;
                break;
            case IdToolBar when change.NewValue is bool toolBar:
                vm.ShowToolBar = toolBar;
                break;
            case IdDescriptionPane when change.NewValue is bool pane:
                vm.ShowDescriptionPane = pane;
                break;
            case IdNameWidth when change.NewValue is decimal width:
                vm.NameColumnWidth = (double)width;
                break;
        }
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var viewMode = new PropertyItem
        {
            Id = IdViewMode,
            Name = "视图模式",
            Kind = PropertyItemKind.Choice,
            Options = ["分类", "字母"],
            Value = "分类",
            DefaultValue = "分类",
            Description = "分类视图按 Category 分组；字母视图平铺并按名称排序。",
        };
        var toolBar = new PropertyItem
        {
            Id = IdToolBar,
            Name = "显示工具栏",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "顶部工具栏（视图切换 / 展开 / 折叠 / 重置）。",
        };
        var descriptionPane = new PropertyItem
        {
            Id = IdDescriptionPane,
            Name = "显示描述面板",
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = "底部描述面板，显示选中项的说明。",
        };
        var nameWidth = new PropertyItem
        {
            Id = IdNameWidth,
            Name = "名称列宽",
            Kind = PropertyItemKind.Number,
            Value = 148m,
            DefaultValue = 148m,
            Minimum = 90,
            Maximum = 340,
            Unit = "px",
            Description = "名称列初始宽度，运行中也可直接拖拽分隔条。",
        };

        return [viewMode, toolBar, descriptionPane, nameWidth];
    }

    private void OnExpandAll(object? sender, RoutedEventArgs e) => Grid.ExpandAll();

    private void OnCollapseAll(object? sender, RoutedEventArgs e) => Grid.CollapseAll();

    private void OnResetAll(object? sender, RoutedEventArgs e) => Grid.ResetAll();
}
