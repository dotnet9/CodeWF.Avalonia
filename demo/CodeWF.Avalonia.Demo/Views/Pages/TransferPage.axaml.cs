using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Collections.Specialized;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TransferPage : DemoPageBase
{
    private const string IdLeftCount = "tr.left";
    private const string IdRightCount = "tr.right";

    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.TransferPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.TransferPageaxaml.CSharpSnippet002");

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
            Name = L.Instance.GetResource("Demo.Text.TransferPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = L.Instance.GetResource("Demo.Text.TransferPageaxaml.BuildProperties004"),
        };
        var right = new PropertyItem
        {
            Id = IdRightCount,
            Name = L.Instance.GetResource("Demo.Text.TransferPageaxaml.BuildProperties005"),
            Kind = PropertyItemKind.ReadOnly,
            Value = 0,
            Description = L.Instance.GetResource("Demo.Text.TransferPageaxaml.BuildProperties006"),
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
            NotifyCollectionChangedAction.Add => L.Instance.GetResource("Demo.Text.TransferPageaxaml.OnCollectionChanged007"),
            NotifyCollectionChangedAction.Remove => L.Instance.GetResource("Demo.Text.TransferPageaxaml.OnCollectionChanged008"),
            _ => e.Action.ToString(),
        };

        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            vm.Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.TransferPageaxaml.OnCollectionChanged009"), action, DescribeItems(e.NewItems ?? e.OldItems), vm.Available.Count, vm.Selected.Count));
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
