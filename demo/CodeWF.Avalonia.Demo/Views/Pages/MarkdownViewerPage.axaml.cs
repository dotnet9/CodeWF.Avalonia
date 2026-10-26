using global::Avalonia.Threading;
using System.Diagnostics;

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
    private MarkdownStreamPacer? _streamingPacer;
    private DispatcherTimer? _streamingArrivalTimer;
    private DispatcherTimer? _streamingPaceTimer;
    private Stopwatch _streamingClock = Stopwatch.StartNew();
    private string _streamingSourceText = string.Empty;
    private int _streamingPosition;
    private bool _streamingArrivalDone;
    private bool _streamingCompleted;

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
        _streamingPacer = new MarkdownStreamPacer(MarkdownStreamPacer.PacingPreset.Balanced);
        _streamingClock = Stopwatch.StartNew();
        _streamingPosition = 0;
        _streamingArrivalDone = false;
        _streamingCompleted = false;
        Viewer.StreamingSource = _streamingDocument;

        // 模拟真实网络：分片大小与到达间隔随机抖动；节奏器按帧平滑释放，
        // 避免 60ms 一片的机械感，也展示宿主侧推荐的接线方式。
        _streamingArrivalTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Background,
            (_, _) => ArriveStreamingChunk());
        _streamingPaceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Background,
            (_, _) => TickStreamingPacer());
        _streamingArrivalTimer.Start();
        _streamingPaceTimer.Start();
    }

    private void OnCompleteStreamingClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!_streamingArrivalDone)
        {
            _streamingArrivalTimer?.Stop();
            _streamingArrivalDone = true;
            _streamingPacer?.Finish();
        }

        TryCompleteStreaming();
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
        if (_streamingDocument is null || _streamingPacer is null)
        {
            return;
        }

        var size = Random.Shared.Next(8, 120);
        var end = Math.Min(_streamingPosition + size, _streamingSourceText.Length);
        _streamingPacer.Push(_streamingSourceText[_streamingPosition..end], _streamingClock.Elapsed);
        _streamingPosition = end;
        if (_streamingPosition >= _streamingSourceText.Length)
        {
            _streamingArrivalTimer?.Stop();
            _streamingArrivalDone = true;
            _streamingPacer.Finish();
        }
    }

    private void TickStreamingPacer()
    {
        if (_streamingDocument is null || _streamingPacer is null)
        {
            return;
        }

        var released = _streamingPacer.Tick(_streamingClock.Elapsed);
        if (released.Length > 0)
        {
            _streamingDocument.Append(released);
            UpdateMetrics();
        }

        TryCompleteStreaming();
    }

    private void TryCompleteStreaming()
    {
        if (_streamingCompleted)
        {
            return;
        }

        if (_streamingPacer is { IsFinished: true, IsDrained: true })
        {
            _streamingPaceTimer?.Stop();
            _streamingDocument?.Complete();
            _streamingCompleted = true;
        }
    }

    private void StopStreaming()
    {
        _streamingArrivalTimer?.Stop();
        _streamingPaceTimer?.Stop();
        _streamingArrivalTimer = null;
        _streamingPaceTimer = null;
        _streamingDocument?.Dispose();
        _streamingDocument = null;
        _streamingPacer = null;
    }
}
