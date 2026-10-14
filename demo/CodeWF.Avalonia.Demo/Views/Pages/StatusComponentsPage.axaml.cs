using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Avalonia.Controls.Models;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class StatusComponentsPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.StatusComponents.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.StatusComponents.Source.CSharp");

    public CodeWF.Avalonia.Demo.ViewModels.Pages.StatusComponentsPageViewModel ViewModel { get; }
    private readonly string _control;

    public StatusComponentsPage() : this("") { }

    public StatusComponentsPage(string control)
    {
        _control = control;
        ViewModel = new(control);
        InitializeComponent();
        DataContext = ViewModel;
    }


    public override string TitleKey => string.IsNullOrEmpty(_control) ? "Demo.Page.Status.Title" : "Demo.Item." + _control;

    public override string DescriptionKey => string.IsNullOrEmpty(_control) ? "Demo.Page.Status.Desc" : "Demo.Api." + _control + ".Desc";

    public override string XamlCode => _control switch
    {
        "StatusBadge" => L.Instance.GetResource("Demo.Api.StatusBadge.Xaml"),
        "StatusCard" => L.Instance.GetResource("Demo.Api.StatusCard.Xaml"),
        _ => XamlSnippet,
    };

    public override string CSharpCode => _control switch
    {
        "StatusBadge" => L.Instance.GetResource("Demo.Api.StatusBadge.CSharp"),
        "StatusCard" => L.Instance.GetResource("Demo.Api.StatusCard.CSharp"),
        _ => CSharpSnippet,
    };

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
}
