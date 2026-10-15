using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
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

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.SearchListBox.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.SearchListBox.Source.CSharp);

    public SearchListBoxPage()
    {
        InitializeComponent();
        DataContext = new SearchListBoxPageViewModel();

        var list = List;
        list.ListItemDoubleTapped += (item, _) =>
        {
            if (DataContext is SearchListBoxPageViewModel vm)
            {
                vm.Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.SearchListBox.Content.DoubleClickFormat), item));
            }
        };
        list.ListSelectionChanged += (_, args) =>
        {
            if (DataContext is SearchListBoxPageViewModel vm)
            {
                vm.Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.SearchListBox.Content.SelectionChangedItemsFormat), (args.Source as SearchListBox)?.SelectedItems?.Count ?? 0));
            }

            UpdateMetrics();
        };
    }



    public override string TitleKey => DemoResources.Page.SearchListBox.Title;

    public override string DescriptionKey => DemoResources.Page.SearchListBox.Desc;

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
            Name = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.Total),
            Kind = PropertyItemKind.ReadOnly,
            Value = List.TotalCount,
            Description = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.NumberOfSourceItemsTotalCount),
        };
        var searchCount = new PropertyItem
        {
            Id = IdSearchCount,
            Name = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.Filtered),
            Kind = PropertyItemKind.ReadOnly,
            Value = List.SearchCount,
            Description = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.MatchingItemsSearchCountWithABuiltIn400ms),
        };
        var selectedCount = new PropertyItem
        {
            Id = IdSelectedCount,
            Name = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.SelectedItems),
            Kind = PropertyItemKind.ReadOnly,
            Value = List.SelectedItems?.Count ?? 0,
            Description = L.Instance.GetResource(DemoResources.Content.SearchListBox.Properties.NumberOfSelectedItemsSupportsCtrlShiftSelection),
        };

        return [total, searchCount, selectedCount];
    }

    private void UpdateMetrics()
    {
        CountText.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.SearchListBox.UpdateMetrics.TotalFilteredSelectedFormat), List.TotalCount, List.SearchCount, List.SelectedItems?.Count ?? 0);

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
