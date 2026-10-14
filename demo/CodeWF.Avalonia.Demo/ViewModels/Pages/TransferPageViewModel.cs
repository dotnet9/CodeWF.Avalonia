using L = global::CodeWF.Avalonia.Lang.I18nManager;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed record FeatureItem(string Name, string Stage)
{
    public override string ToString() => $"{Name}（{Stage}）";
}

/// <summary>Transfer 演示页 VM：左列为待选清单，右列为已选清单，控件直接在这两个集合间移动条目。</summary>
public class TransferPageViewModel : ReactiveObject
{
    public ObservableCollection<FeatureItem> Available { get; } = new(
    [
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.IncrementalRendering"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.BlockVirtualization"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.18TypographyThemes"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.PDFExport"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.WordExport"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.RichTextPublishing"), L.Instance.GetResource("Demo.Content.Transfer.Available.Completed")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.CollaborativeAnnotations"), L.Instance.GetResource("Demo.Content.Transfer.Available.Planned")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.VersionComparison"), L.Instance.GetResource("Demo.Content.Transfer.Available.Planned")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.AISummaries"), L.Instance.GetResource("Demo.Content.Transfer.Available.Planned")),
        new FeatureItem(L.Instance.GetResource("Demo.Content.Transfer.Available.HandwrittenFormulaRecognition"), L.Instance.GetResource("Demo.Content.Transfer.Available.Planned")),
    ]);

    public ObservableCollection<FeatureItem> Selected { get; } = new();

    public void Log(string message) => MoveLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");

    public ObservableCollection<string> MoveLog { get; } = new();
}
