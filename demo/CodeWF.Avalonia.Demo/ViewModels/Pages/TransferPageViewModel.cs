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
        new FeatureItem("增量渲染", "已完成"),
        new FeatureItem("块级虚拟化", "已完成"),
        new FeatureItem("18 套排版主题", "已完成"),
        new FeatureItem("PDF 导出", "已完成"),
        new FeatureItem("Word 导出", "已完成"),
        new FeatureItem("公众号复制", "已完成"),
        new FeatureItem("协作批注", "规划中"),
        new FeatureItem("版本对比", "规划中"),
        new FeatureItem("AI 摘要", "规划中"),
        new FeatureItem("公式手写识别", "规划中"),
    ]);

    public ObservableCollection<FeatureItem> Selected { get; } = new();

    public void Log(string message) => MoveLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");

    public ObservableCollection<string> MoveLog { get; } = new();
}
