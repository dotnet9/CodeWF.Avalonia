using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class ColumnDisplayEditorPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.ColumnDisplayEditorPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.ColumnDisplayEditorPageaxaml.CSharpSnippet002");

    public ColumnDisplayEditorPage()
    {
        InitializeComponent();
        DataContext = new ColumnDisplayEditorPageViewModel();
    }

    public override string TitleKey => "Demo.Page.ColumnDisplayEditor.Title";

    public override string DescriptionKey => "Demo.Page.ColumnDisplayEditor.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "cde.note",
            Name = L.Instance.GetResource("Demo.Text.ColumnDisplayEditorPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "IList<ColumnDisplayItem>（Key / DisplayText / Visible）",
            Description = L.Instance.GetResource("Demo.Text.ColumnDisplayEditorPageaxaml.BuildProperties004"),
        };

        return [note];
    }
}
