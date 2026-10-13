using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TrapezoidTabPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.TrapezoidTabPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.TrapezoidTabPageaxaml.CSharpSnippet002");

    public TrapezoidTabPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => "Demo.Page.TrapezoidTab.Title";

    public override string DescriptionKey => "Demo.Page.TrapezoidTab.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "tt.note",
            Name = L.Instance.GetResource("Demo.Text.TrapezoidTabPageaxaml.BuildProperties003"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "TrapezoidShapedTabControl / TrapezoidShapedTabStrip / TrapezoidShapedTabItem",
            Description = L.Instance.GetResource("Demo.Text.TrapezoidTabPageaxaml.BuildProperties004"),
        };

        return [note];
    }
}
