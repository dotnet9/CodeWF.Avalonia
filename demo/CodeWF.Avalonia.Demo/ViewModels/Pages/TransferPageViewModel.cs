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
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available001"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available002")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available003"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available004")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available005"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available006")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available007"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available008")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available009"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available010")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available011"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available012")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available013"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available014")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available015"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available016")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available017"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available018")),
        new FeatureItem(L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available019"), L.Instance.GetResource("Demo.Text.TransferPageViewModel.Available020")),
    ]);

    public ObservableCollection<FeatureItem> Selected { get; } = new();

    public void Log(string message) => MoveLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");

    public ObservableCollection<string> MoveLog { get; } = new();
}
