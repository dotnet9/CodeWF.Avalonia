using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class LogPage : DemoPageBase
{
    public LogPageViewModel ViewModel { get; } = new();
    public LogPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
    public override string TitleKey => "Demo.Page.Log.Title";
    public override string DescriptionKey => "Demo.Page.Log.Desc";
    public override string XamlCode => I18nManager.Instance.GetResource("Demo.Text.LogPageaxaml.XamlSnippet001");
    public override string CSharpCode => I18nManager.Instance.GetResource("Demo.Text.LogPageaxaml.CSharpSnippet002");
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
}
