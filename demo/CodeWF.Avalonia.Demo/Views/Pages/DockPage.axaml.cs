using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class DockPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.DockPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.DockPageaxaml.CSharpSnippet002");

    public DockPage()
    {
        InitializeComponent();
        DataContext = new DockPageViewModel();
    }

    public override string TitleKey => "Demo.Page.Dock.Title";

    public override string DescriptionKey => "Demo.Page.Dock.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "dock.note",
            Name = L.Instance.GetResource("Demo.Text.DockPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "Dock.Avalonia 12.1 + Dock.Model.ReactiveUI + CodeWF DockSemiTheme",
            Description = L.Instance.GetResource("Demo.Text.DockPageaxaml.BuildProperties004"),
        };

        return [note];
    }
}
