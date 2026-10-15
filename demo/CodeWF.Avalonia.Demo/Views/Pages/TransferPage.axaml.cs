using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using System.Collections.Specialized;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TransferPage : DemoPageBase
{
    private const string IdLeftCount = "tr.left";
    private const string IdRightCount = "tr.right";

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.Transfer.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.Transfer.Source.CSharp);

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

    public override string TitleKey => DemoResources.Page.Transfer.Title;

    public override string DescriptionKey => DemoResources.Page.Transfer.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateMetrics();

    public override void OnPropertyCommitted(PropertyValueChange change) => UpdateMetrics();

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var left = new PropertyItem
        {
            Id = IdLeftCount,
            Name = L.Instance.GetResource(DemoResources.Content.Transfer.Properties.AvailableCount),
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = L.Instance.GetResource(DemoResources.Content.Transfer.Properties.NumberOfAvailableItemsOnTheLeft),
        };
        var right = new PropertyItem
        {
            Id = IdRightCount,
            Name = L.Instance.GetResource(DemoResources.Content.Transfer.Properties.SelectedCount),
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = L.Instance.GetResource(DemoResources.Content.Transfer.Properties.NumberOfSelectedItemsOnTheRight),
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
            NotifyCollectionChangedAction.Add => L.Instance.GetResource(DemoResources.Content.Transfer.OnCollectionChanged.MoveIn),
            NotifyCollectionChangedAction.Remove => L.Instance.GetResource(DemoResources.Content.Transfer.OnCollectionChanged.MoveOut),
            _ => e.Action.ToString(),
        };

        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            vm.Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.Transfer.OnCollectionChanged.AvailableSelectedFormat), action, DescribeItems(e.NewItems ?? e.OldItems), vm.Available.Count, vm.Selected.Count));
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
