using Avalonia.Controls;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class SearchListBoxPage : DemoPageBase
{
    private const string IdTotal = "slb.total";
    private const string IdSearchCount = "slb.searchCount";
    private const string IdSelectedCount = "slb.selectedCount";

    public const string XamlSnippet = """
        <!-- 内置搜索框（400ms 防抖）、多选、计数条；条目文本按 item.ToString() 匹配 -->
        <codewf:SearchListBox ItemsSource="{Binding Items}" />
        """;

    public const string CSharpSnippet = """
        var list = new SearchListBox { ItemsSource = items };

        // 计数条指标
        var total = list.TotalCount;        // 数据源总数
        var visible = list.SearchCount;     // 当前过滤结果数

        // 多选（Ctrl / Shift）与事件
        var selected = list.SelectedItems;  // List<object?>
        list.ListSelectionChanged += (item, args) => Refresh(args.Source.SelectedItems);
        list.ListItemDoubleTapped += (item, args) => Open(item);

        // 批量增删（直接作用到源集合）
        list.AddRange(newItems);
        list.RemoveRange(selected);
        """;

    public SearchListBoxPage()
    {
        InitializeComponent();
        DataContext = new SearchListBoxPageViewModel();

        var list = List;
        list.ListItemDoubleTapped += (item, _) =>
        {
            if (DataContext is SearchListBoxPageViewModel vm)
            {
                vm.Log($"双击：{item}");
            }
        };
        list.ListSelectionChanged += (_, args) =>
        {
            if (DataContext is SearchListBoxPageViewModel vm)
            {
                vm.Log($"选区变化：{(args.Source as SearchListBox)?.SelectedItems?.Count ?? 0} 项");
            }

            UpdateMetrics();
        };
    }



    public override string TitleKey => "Demo.Page.SearchListBox.Title";

    public override string DescriptionKey => "Demo.Page.SearchListBox.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateMetrics();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        // 指标类只读属性在打开 / 选区变化时刷新。
        UpdateMetrics();
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var total = new PropertyItem
        {
            Id = IdTotal,
            Name = "总数",
            Kind = PropertyItemKind.ReadOnly,
            Value = List.TotalCount,
            Description = "数据源总条目数（TotalCount）。",
        };
        var searchCount = new PropertyItem
        {
            Id = IdSearchCount,
            Name = "过滤结果",
            Kind = PropertyItemKind.ReadOnly,
            Value = List.SearchCount,
            Description = "当前搜索过滤后的条目数（SearchCount），控件内置 400ms 防抖。",
        };
        var selectedCount = new PropertyItem
        {
            Id = IdSelectedCount,
            Name = "选中项",
            Kind = PropertyItemKind.ReadOnly,
            Value = List.SelectedItems?.Count ?? 0,
            Description = "当前多选选中数量（Ctrl / Shift 多选）。",
        };

        return [total, searchCount, selectedCount];
    }

    private void UpdateMetrics()
    {
        CountText.Text = $"总数 {List.TotalCount} · 过滤结果 {List.SearchCount} · 选中 {List.SelectedItems?.Count ?? 0}";

        foreach (var item in Properties)
        {
            switch (item.Id)
            {
                case IdTotal:
                    item.Value = List.TotalCount;
                    break;
                case IdSearchCount:
                    item.Value = List.SearchCount;
                    break;
                case IdSelectedCount:
                    item.Value = List.SelectedItems?.Count ?? 0;
                    break;
            }

            item.RefreshSummary();
        }
    }
}
