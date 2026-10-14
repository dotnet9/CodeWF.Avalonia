using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
namespace CodeWF.Avalonia.Demo.Views.Pages;
public sealed class AboutPage : DemoPageBase
{
    public AboutPage() => Content = Ui.Page(
        Ui.Heading("@Demo.Catalog.About"), Ui.Sub("@Demo.Catalog.AboutDesc"),
        Ui.Card("@Demo.Catalog.Project", Ui.Code("dotnet add package CodeWF.Avalonia.Controls"), Ui.Key("https://github.com/dotnet9/CodeWF.Avalonia")),
        Ui.Hint("@Demo.Catalog.License"));
    public override string TitleKey => DemoResources.Item.About;
    public override string DescriptionKey => DemoResources.Catalog.AboutDesc;
    public override string XamlCode => "";
}
