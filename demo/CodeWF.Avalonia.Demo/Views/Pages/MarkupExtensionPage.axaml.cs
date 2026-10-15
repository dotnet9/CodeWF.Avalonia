using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkupExtensionPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.MarkupExtension.Source.Xaml);

    private readonly string _extension;
    public MarkupExtensionPage() : this("") { }

    public MarkupExtensionPage(string extension)
    {
        _extension = extension;
        InitializeComponent();
        DataContext = new MarkupExtensionPageViewModel(extension);
    }

    public override string TitleKey => string.IsNullOrEmpty(_extension) ? DemoResources.Page.MarkupExtension.Title : "Demo.Item." + _extension;

    public override string DescriptionKey => string.IsNullOrEmpty(_extension) ? DemoResources.Page.MarkupExtension.Desc : "Demo.Api." + _extension + ".Desc";

    public override string XamlCode => _extension switch
    {
        "If" => L.Instance.GetResource(DemoResources.Api.If.Xaml),
        "Switch" => L.Instance.GetResource(DemoResources.Api.Switch.Xaml),
        _ => XamlSnippet,
    };

    public override string CSharpCode => L.Instance.GetResource(DemoResources.Content.MarkupExtension.CSharpCode.SwitchBindingIfConverterIfConditionConverterSwitchConverterEnumToBooleanConverterIsCollectionEmptyConverter);

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var note = new PropertyItem
        {
            Id = "mx.note",
            Name = L.Instance.GetResource(DemoResources.Content.MarkupExtension.Properties.UseCases),
            Kind = PropertyItemKind.ReadOnly,
            Value = L.Instance.GetResource(DemoResources.Content.MarkupExtension.Properties.ConditionalDisplayTextAndIcons),
            Description = L.Instance.GetResource(DemoResources.Content.MarkupExtension.Properties.IfSwitchChooseDisplayValuesKeepMultiProperty),
        };

        return [note];
    }
}
