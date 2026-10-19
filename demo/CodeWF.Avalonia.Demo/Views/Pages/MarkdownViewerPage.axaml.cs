using global::Avalonia.Threading;

using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using CodeWF.Avalonia.Markdown.Streaming;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownViewerPage : DemoPageBase
{
    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownViewer.Source.CSharp);

    public MarkdownViewerPageViewModel ViewModel { get; } = new();

    private MarkdownStreamingDocument? _streamingDocument;
    private DispatcherTimer? _streamingTimer;
    private string _streamingSourceText = string.Empty;
    private int _streamingPosition;

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
        DetachedFromVisualTree += (_, _) => StopStreaming();
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

    private void OnStartStreamingClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        StopStreaming();
        Viewer.StreamingSource = null;
        _streamingSourceText = MarkdownViewerPageViewModel.SampleMarkdown;
        _streamingDocument = new MarkdownStreamingDocument();
        _streamingPosition = 0;
        Viewer.StreamingSource = _streamingDocument;

        _streamingTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background,
            (_, _) => AppendStreamingChunk());
        _streamingTimer.Start();
    }

    private void OnCompleteStreamingClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _streamingTimer?.Stop();
        _streamingDocument?.Complete();
        UpdateMetrics();
    }

    private void OnExitStreamingClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        StopStreaming();
        Viewer.StreamingSource = null;
        // 同值赋值不触发属性变更，先清空再恢复以回到整串渲染。
        Viewer.Markdown = string.Empty;
        Viewer.Markdown = ViewModel.AppliedMarkdown;
        UpdateMetrics();
    }

    private void AppendStreamingChunk()
    {
        if (_streamingDocument is null)
        {
            return;
        }

        var end = Math.Min(_streamingPosition + 8, _streamingSourceText.Length);
        _streamingDocument.Append(_streamingSourceText[_streamingPosition..end]);
        _streamingPosition = end;
        if (_streamingPosition >= _streamingSourceText.Length)
        {
            _streamingTimer?.Stop();
            _streamingDocument.Complete();
        }

        UpdateMetrics();
    }

    private void StopStreaming()
    {
        _streamingTimer?.Stop();
        _streamingTimer = null;
        _streamingDocument?.Dispose();
        _streamingDocument = null;
    }
}
