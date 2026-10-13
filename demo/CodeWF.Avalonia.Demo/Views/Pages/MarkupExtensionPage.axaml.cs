using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkupExtensionPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.MarkupExtensionPageaxaml.XamlSnippet001");

    private readonly string _extension;
    public MarkupExtensionPage() : this("") { }

    public MarkupExtensionPage(string extension)
    {
        _extension = extension;
        InitializeComponent();
        DataContext = new MarkupExtensionPageViewModel(extension);
    }

    public override string TitleKey => string.IsNullOrEmpty(_extension) ? "Demo.Page.MarkupExtension.Title" : "Demo.Item." + _extension;

    public override string DescriptionKey => string.IsNullOrEmpty(_extension) ? "Demo.Page.MarkupExtension.Desc" : "Demo.Api." + _extension + ".Desc";

    public override string XamlCode => _extension switch
    {
        "If" => L.Instance.GetResource("Demo.Api.If.Xaml"),
        "Switch" => L.Instance.GetResource("Demo.Api.Switch.Xaml"),
        _ => XamlSnippet,
    };

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
