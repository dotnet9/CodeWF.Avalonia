using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TrapezoidTabPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.TrapezoidTab.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.TrapezoidTab.Source.CSharp");

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
            Name = L.Instance.GetResource("Demo.Content.TrapezoidTab.Properties.ThemeKeys"),
            Kind = PropertyItemKind.ReadOnly,
            Value = "TrapezoidShapedTabControl / TrapezoidShapedTabStrip / TrapezoidShapedTabItem",
            Description = L.Instance.GetResource("Demo.Content.TrapezoidTab.Properties.ACSemiThemeInjectsTheResourceKeysForUseWith"),
        };

        return [note];
    }
}
