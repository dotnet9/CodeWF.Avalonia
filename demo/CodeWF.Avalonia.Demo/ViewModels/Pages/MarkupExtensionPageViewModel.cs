using L = global::CodeWF.Avalonia.Lang.I18nManager;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>If / Switch 标记扩展演示页 VM。</summary>
public class MarkupExtensionPageViewModel : ReactiveObject
{
    private bool _isBusy;
    private string _state = L.Instance.GetResource("Demo.Text.MarkupExtensionPageViewModel.state001");

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

    public string[] StateOptions { get; } = [L.Instance.GetResource("Demo.Text.MarkupExtensionPageViewModel.StateOptions002"), L.Instance.GetResource("Demo.Text.MarkupExtensionPageViewModel.StateOptions003"), L.Instance.GetResource("Demo.Text.MarkupExtensionPageViewModel.StateOptions004"), L.Instance.GetResource("Demo.Text.MarkupExtensionPageViewModel.StateOptions005")];
}
