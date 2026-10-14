using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownViewerPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Source.CSharp);

    public MarkdownViewerPageViewModel ViewModel { get; } = new();

    public MarkdownViewerPage()
    {
        InitializeComponent();
        DataContext = ViewModel;

        Viewer.TaskWriteBackRequested += (_, e) =>
        {
            ViewModel.ApplyExternalEdit(e.Markdown);
            UpdateMetrics();
        };
        Viewer.LayoutUpdated += (_, _) => UpdateMetrics();
    }


    public override string TitleKey => DemoResources.Page.MarkdownViewer.Title;

    public override string DescriptionKey => DemoResources.Page.MarkdownViewer.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateMetrics();

    public override void OnPropertyCommitted(PropertyValueChange change) => ViewModel.Commit(change);
    protected override IReadOnlyList<PropertyItem> BuildProperties() => ViewModel.BuildProperties();

    private void UpdateMetrics()
    {
        ViewModel.UpdateMetrics(Viewer.LastRenderMode.ToString(), Viewer.LastParseDuration.TotalMilliseconds, Viewer.RealizedBlockCount);
    }
}
