using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class ColumnDisplayEditorPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.Source.CSharp);

    public ColumnDisplayEditorPage()
    {
        InitializeComponent();
        DataContext = new ColumnDisplayEditorPageViewModel();
    }

    public override string TitleKey => DemoResources.Page.ColumnDisplayEditor.Title;

    public override string DescriptionKey => DemoResources.Page.ColumnDisplayEditor.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "cde.note",
            Name = L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.Properties.DataContract),
            Kind = PropertyItemKind.ReadOnly,
            Value = "IList<ColumnDisplayItem>（Key / DisplayText / Visible）",
            Description = L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.Properties.TheEditorRequiresIListItemsSourceDraggingReordersIt),
        };

        return [note];
    }
}
