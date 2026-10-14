using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Windows.Input;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>The overview uses the same Markdown renderer and grid enhancement as the full demos.</summary>
public sealed class WelcomeViewModel : ReactiveObject
{
    private string _markdown = L.Instance.GetResource(DemoResources.Content.Welcome.markdown.GiveInterfacesExpressionBuildDesktopExperiencesWithCodeWF);
    private string _copyStatus = string.Empty;
    private int _labIndex;
    private bool _experimentsExpanded;

    public WelcomeViewModel(Action<string> navigate, Action<string> togglePanel)
    {
        NavigateCommand = ReactiveCommand.Create<string>(navigate);
        CopyInstallCommand = ReactiveCommand.CreateFromTask(CopyInstallAsync);
        TogglePanelCommand = ReactiveCommand.Create<string>(togglePanel);
    }

    public IReadOnlyList<DemoGroupViewModel> CatalogGroups { get; } = DemoCatalog.CreateGroups()
        .Where(group => group.TitleKey is not DemoResources.Group.Home and not DemoResources.Group.Examples).ToArray();

    public string Markdown { get => _markdown; set => this.RaiseAndSetIfChanged(ref _markdown, value); }
    public string CopyStatus { get => _copyStatus; set => this.RaiseAndSetIfChanged(ref _copyStatus, value); }
    public bool ExperimentsExpanded { get => _experimentsExpanded; set => this.RaiseAndSetIfChanged(ref _experimentsExpanded, value); }
    public int LabIndex { get => _labIndex; set => this.RaiseAndSetIfChanged(ref _labIndex, value); }
    public System.Collections.ObjectModel.ObservableCollection<WelcomeSortRow> Rows { get; } =
        [new("item10", "Markdown"), new("item2", "DataGrid"), new("item1", "Dock"), new("item100", "PropertyGrid")];

    public void RefreshCulture()
    {
        foreach (var row in Rows) row.RefreshCulture();
    }
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
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoResources.Studio.Copied);
        }
        catch
        {
            CopyStatus = global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(DemoResources.Studio.CopyFailed);
        }
    }
    public ICommand TogglePanelCommand { get; }
}

public sealed class WelcomeSortRow(string name, string module) : ReactiveObject
{
    public string Name { get; } = name;
    public string Module { get; } = module;
    public string Status => L.Instance.GetResource(DemoResources.Studio.SortReady);
    public void RefreshCulture() => this.RaisePropertyChanged(nameof(Status));
}
