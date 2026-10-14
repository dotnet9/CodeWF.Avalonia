using L = global::CodeWF.Avalonia.Lang.I18nManager;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>If / Switch 标记扩展演示页 VM。</summary>
public class MarkupExtensionPageViewModel : ReactiveObject
{
    public MarkupExtensionPageViewModel(string extension = "") { ShowIf = extension != "Switch"; ShowSwitch = extension != "If"; }
    public bool ShowIf { get; }
    public bool ShowSwitch { get; }
    private bool _isBusy;
    private string _state = L.Instance.GetResource("Demo.Content.MarkupExtension.state.Running");

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

    public string[] StateOptions { get; } = [L.Instance.GetResource("Demo.Content.MarkupExtension.StateOptions.Running"), L.Instance.GetResource("Demo.Content.MarkupExtension.StateOptions.Paused"), L.Instance.GetResource("Demo.Content.MarkupExtension.StateOptions.Maintenance"), L.Instance.GetResource("Demo.Content.MarkupExtension.StateOptions.Unknown")];
}
