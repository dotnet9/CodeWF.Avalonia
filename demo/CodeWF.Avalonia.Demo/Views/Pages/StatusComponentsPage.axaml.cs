using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class StatusComponentsPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.StatusComponentsPageaxaml.CSharpSnippet002");

    public CodeWF.Avalonia.Demo.ViewModels.Pages.StatusComponentsPageViewModel ViewModel { get; } = new();

    public StatusComponentsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }


    public override string TitleKey => "Demo.Page.Status.Title";

    public override string DescriptionKey => "Demo.Page.Status.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
}
