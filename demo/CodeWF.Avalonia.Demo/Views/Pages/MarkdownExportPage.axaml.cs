using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Toolkit.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownExportPage : DemoPageBase
{
    public MarkdownExportPageViewModel ViewModel { get; } = new();

    public MarkdownExportPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.SelectOutput = SelectOutputAsync;
    }

    public override string TitleKey => DemoResources.Page.MarkdownExport.Title;
    public override string DescriptionKey => DemoResources.Page.MarkdownExport.Desc;
    public override string XamlCode => I18nManager.Instance.GetResource(DemoResources.Content.MarkdownExport.Source.Xaml);
    public override string CSharpCode => I18nManager.Instance.GetResource(DemoResources.Content.MarkdownExport.Source.CSharp);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();
    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);

    private async Task<string?> SelectOutputAsync(string title, string extension)
    {
        if (TopLevel.GetTopLevel(this) is not { } topLevel) return null;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = "markdown-export",
            DefaultExtension = extension,
        });
        return file?.TryGetLocalPath();
    }
}
