using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class DockPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.Dock.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.Dock.Source.CSharp);

    public DockPage()
    {
        InitializeComponent();
        DataContext = new DockPageViewModel();
    }

    public override string TitleKey => DemoResources.Page.Dock.Title;

    public override string DescriptionKey => DemoResources.Page.Dock.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "dock.note",
            Name = L.Instance.GetResource(DemoResources.Content.Dock.Properties.Composition),
            Kind = PropertyItemKind.ReadOnly,
            Value = "Dock.Avalonia 12.1 + Dock.Model.ReactiveUI + CodeWF DockSemiTheme",
            Description = L.Instance.GetResource(DemoResources.Content.Dock.Properties.CodeWFAvaloniaDockSuppliesTheSemiAppearanceTrapezoid),
        };

        return [note];
    }
}
