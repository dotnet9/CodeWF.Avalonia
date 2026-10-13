using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Demo.ViewModels;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Lang.Json;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class LangPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource("Demo.Text.LangPageaxaml.XamlSnippet001");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Text.LangPageaxaml.CSharpSnippet002");

    public CodeWF.Avalonia.Demo.ViewModels.Pages.LangPageViewModel ViewModel { get; } = new();

    public LangPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.CultureRequested = name => (TopLevel.GetTopLevel(this)?.DataContext as MainViewModel)?.SelectCulture(name);
        AttachedToVisualTree += (_, _) => ViewModel.Activate();
        DetachedFromVisualTree += (_, _) => ViewModel.Deactivate();
    }

    public override string TitleKey => "Demo.Page.Lang.Title";

    public override string DescriptionKey => "Demo.Page.Lang.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => ViewModel.Activate();

}
