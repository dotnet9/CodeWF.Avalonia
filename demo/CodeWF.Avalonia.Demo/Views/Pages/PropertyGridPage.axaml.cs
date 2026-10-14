using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class PropertyGridPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.PropertyGrid.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.PropertyGrid.Source.CSharp");

    public PropertyPanelDemoViewModel ViewModel { get; } = new();

    public PropertyGridPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private PropertyGrid Grid => DemoGrid;

    public override string TitleKey => "Demo.Page.PropertyGrid.Title";

    public override string DescriptionKey => "Demo.Page.PropertyGrid.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();

    private void OnExpandAll(object? sender, RoutedEventArgs e) => Grid.ExpandAll();

    private void OnCollapseAll(object? sender, RoutedEventArgs e) => Grid.CollapseAll();

    private void OnResetAll(object? sender, RoutedEventArgs e) => Grid.ResetAll();
}
