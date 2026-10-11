using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>If / Switch 标记扩展演示页 VM。</summary>
public class MarkupExtensionPageViewModel : ReactiveObject
{
    private bool _isBusy;
    private string _state = "运行中";

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    public string[] StateOptions { get; } = ["运行中", "已暂停", "维护中", "未知"];
}
