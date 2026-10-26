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
    private MarkdownTypewriterStream? _typewriter;
    private DispatcherTimer? _streamingArrivalTimer;
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

        // LLM 打字机接线（推荐）：到达侧只喂 delta，节奏与帧泵、排空终局全部
        // 由 MarkdownTypewriterStream 托管；模拟真实网络的随机抖动照旧。
        _typewriter = MarkdownTypewriterStream.Attach(_streamingDocument);
        _streamingDocument.SnapshotReady += (_, _) => UpdateMetrics();

        _streamingArrivalTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Background,
            (_, _) => ArriveStreamingChunk());
        _streamingArrivalTimer.Start();
    }

    private void OnCompleteStreamingClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        _streamingArrivalTimer?.Stop();
        _typewriter?.Finish();
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

    private void ArriveStreamingChunk()
    {
        if (_typewriter is null)
        {
            return;
        }

        var size = Random.Shared.Next(8, 120);
        var end = Math.Min(_streamingPosition + size, _streamingSourceText.Length);
        _typewriter.AppendDelta(_streamingSourceText[_streamingPosition..end]);
        _streamingPosition = end;
        if (_streamingPosition >= _streamingSourceText.Length)
        {
            _streamingArrivalTimer?.Stop();
            _typewriter.Finish(); // 排空后自动终局
        }
    }

    private void StopStreaming()
    {
        _streamingArrivalTimer?.Stop();
        _streamingArrivalTimer = null;
        _typewriter?.Dispose(); // 先停打字机（内含帧泵），再释放目标文档
        _typewriter = null;
        _streamingDocument?.Dispose();
        _streamingDocument = null;
    }
}
