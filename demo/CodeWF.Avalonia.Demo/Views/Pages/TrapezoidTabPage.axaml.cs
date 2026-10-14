using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class TrapezoidTabPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.TrapezoidTab.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.TrapezoidTab.Source.CSharp);

    public TrapezoidTabPage()
    {
        InitializeComponent();
    }

    public override string TitleKey => DemoResources.Page.TrapezoidTab.Title;

    public override string DescriptionKey => DemoResources.Page.TrapezoidTab.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "tt.note",
            Name = L.Instance.GetResource(DemoResources.Content.TrapezoidTab.Properties.ThemeKeys),
            Kind = PropertyItemKind.ReadOnly,
            Value = "TrapezoidShapedTabControl / TrapezoidShapedTabStrip / TrapezoidShapedTabItem",
            Description = L.Instance.GetResource(DemoResources.Content.TrapezoidTab.Properties.ACSemiThemeInjectsTheResourceKeysForUseWith),
        };

        return [note];
    }
}
