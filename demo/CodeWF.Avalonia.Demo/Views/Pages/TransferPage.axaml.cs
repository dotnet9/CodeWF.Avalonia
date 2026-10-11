using System.Collections.Specialized;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TransferPage : DemoPageBase
{
    private const string IdLeftCount = "tr.left";
    private const string IdRightCount = "tr.right";

    public const string XamlSnippet = """
        <!-- 双列穿梭：LeftItemsSource / RightItemsSource 为普通 IList，控件直接在两侧集合间移动 -->
        <codewf:Transfer LeftHeader="功能清单（可选）"
                         RightHeader="本期交付（已选）"
                         LeftItemsSource="{Binding Available}"
                         RightItemsSource="{Binding Selected}" />
        """;

    public const string CSharpSnippet = """
        var transfer = new Transfer
        {
            LeftHeader = "可选",
            RightHeader = "已选",
            LeftItemsSource = available,   // ObservableCollection<FeatureItem>
            RightItemsSource = selected,
        };

        // 移动方式一：中间按钮（有选中项时可用）
        transfer.MoveLeftToRight();
        transfer.MoveRightToLeft();

        // 移动方式二：双击任一侧条目，自动移动到对侧（控件内部处理）

        // 集合即状态：两侧集合的 CollectionChanged 即业务侧的真实变更
        selected.CollectionChanged += (_, e) => Save(selected);
        """;

    public TransferPage()
    {
        InitializeComponent();
        DataContext = new TransferPageViewModel();

        if (DataContext is TransferPageViewModel vm)
        {
            vm.Available.CollectionChanged += OnCollectionChanged;
            vm.Selected.CollectionChanged += OnCollectionChanged;
        }
    }

    public override string TitleKey => "Demo.Page.Transfer.Title";

    public override string DescriptionKey => "Demo.Page.Transfer.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateMetrics();

    public override void OnPropertyCommitted(PropertyValueChange change) => UpdateMetrics();

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var left = new PropertyItem
        {
            Id = IdLeftCount,
            Name = "可选数量",
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = "左列（待选）条目数。",
        };
        var right = new PropertyItem
        {
            Id = IdRightCount,
            Name = "已选数量",
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = "右列（已选）条目数。",
        };

        return [left, right];
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is not TransferPageViewModel vm)
        {
            return;
        }

        var action = e.Action switch
        {
            NotifyCollectionChangedAction.Add => "移入",
            NotifyCollectionChangedAction.Remove => "移出",
            _ => e.Action.ToString(),
        };

        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            vm.Log($"{action}：{DescribeItems(e.NewItems ?? e.OldItems)} · 可选 {vm.Available.Count} / 已选 {vm.Selected.Count}");
        }

        UpdateMetrics();
    }

    private void UpdateMetrics()
    {
        if (DataContext is not TransferPageViewModel vm)
        {
            return;
        }

        foreach (var item in Properties)
        {
            switch (item.Id)
            {
                case IdLeftCount:
                    item.Value = vm.Available.Count;
                    break;
                case IdRightCount:
                    item.Value = vm.Selected.Count;
                    break;
            }

            item.RefreshSummary();
        }
    }

    private static string DescribeItems(System.Collections.IList? items) =>
        items is null ? string.Empty : string.Join("、", items.Cast<object>().Select(i => i.ToString()));
}
