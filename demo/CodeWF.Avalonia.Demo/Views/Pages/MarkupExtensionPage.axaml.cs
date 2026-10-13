using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkupExtensionPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.XamlSnippet001");

    public MarkupExtensionPage()
    {
        InitializeComponent();
        DataContext = new MarkupExtensionPageViewModel();
    }

    public override string TitleKey => "Demo.Page.MarkupExtension.Title";

    public override string DescriptionKey => "Demo.Page.MarkupExtension.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.CSharpCode002");

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "mx.note",
            Name = L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.ReadOnly,
            Value = L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.BuildProperties004"),
            Description = L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.BuildProperties005"),
        };

        return [note];
    }
}
