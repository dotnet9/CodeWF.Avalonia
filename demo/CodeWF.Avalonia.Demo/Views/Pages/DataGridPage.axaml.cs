using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.DataGrid;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class DataGridPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.DataGrid.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.DataGrid.Source.CSharp);

    public DataGridPageViewModel ViewModel { get; } = new();

    public DataGridPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public override string TitleKey => DemoResources.Page.DataGrid.Title;

    public override string DescriptionKey => DemoResources.Page.DataGrid.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();

}
