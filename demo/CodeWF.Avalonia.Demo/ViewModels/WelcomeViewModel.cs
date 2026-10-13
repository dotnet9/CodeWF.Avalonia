using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Windows.Input;
using CodeWF.Avalonia.Demo.ViewModels.Pages;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>The overview uses the same Markdown renderer and grid enhancement as the full demos.</summary>
public sealed class WelcomeViewModel : ReactiveObject
{
    private string _markdown = L.Instance.GetResource("Demo.Text.WelcomeViewModel.markdown001");
    private string _copyStatus = string.Empty;
    private int _labIndex;

    public WelcomeViewModel(Action<string> navigate, Action<string> togglePanel)
    {
        NavigateCommand = ReactiveCommand.Create<string>(navigate);
        CopyInstallCommand = ReactiveCommand.CreateFromTask(CopyInstallAsync);
        TogglePanelCommand = ReactiveCommand.Create<string>(togglePanel);
    }

    public string Markdown { get => _markdown; set => this.RaiseAndSetIfChanged(ref _markdown, value); }
    public string CopyStatus { get => _copyStatus; set => this.RaiseAndSetIfChanged(ref _copyStatus, value); }
    public int LabIndex { get => _labIndex; set => this.RaiseAndSetIfChanged(ref _labIndex, value); }
    public System.Collections.ObjectModel.ObservableCollection<DataGridRowModel> Rows { get; } = new DataGridPageViewModel().Rows;
    public string InstallCommand => "dotnet add package CodeWF.Avalonia.Controls";
    public ICommand NavigateCommand { get; }
    public ICommand CopyInstallCommand { get; }
    public Func<string, Task>? CopyToClipboard { private get; set; }

    private async Task CopyInstallAsync()
    {
        try
        {
            if (CopyToClipboard is null) throw new InvalidOperationException("Clipboard unavailable");
            await CopyToClipboard(InstallCommand);
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Studio.Copied");
        }
        catch
        {
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Studio.CopyFailed");
        }
    }
    public ICommand TogglePanelCommand { get; }
}
