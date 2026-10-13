using L = global::CodeWF.Avalonia.Lang.I18nManager;
using System.Windows.Input;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class LogPageViewModel : ReactiveObject
{
    private const string IdMinimumLevel = "log.minimumLevel";
    private const string IdMaxDisplay = "log.maxDisplay";
    private LogLevel _minimumLevel = LogLevel.Information;
    private int _maxDisplayCount = 2000;
    public LogLevel MinimumLevel { get => _minimumLevel; private set => this.RaiseAndSetIfChanged(ref _minimumLevel, value); }
    public int MaxDisplayCount { get => _maxDisplayCount; private set => this.RaiseAndSetIfChanged(ref _maxDisplayCount, value); }
    public ICommand WriteCommand { get; }
    public ICommand BurstCommand { get; }

    public LogPageViewModel()
    {
        WriteCommand = ReactiveCommand.Create<string>(Write);
        BurstCommand = ReactiveCommand.CreateFromTask(WriteBurstAsync);
    }

    private void Write(string level)
    {
        switch (level)
        {
            case "Trace": Logger.Trace(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteTrace018")); break;
            case "Debug": Logger.Debug(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteDebug019")); break;
            case "Info": Logger.Info(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteInfo020") + DateTime.Now.ToString("HH:mm:ss.fff")); break;
            case "Warning": Logger.Warn(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteWarning021")); break;
            case "Error": Logger.Error(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteError022")); break;
            case "Critical": Logger.Fatal(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteCritical023")); break;
        }
    }

    private static async Task WriteBurstAsync()
    {
        for (var i = 1; i <= 200; i++)
        {
            Logger.Info(string.Format(L.Instance.GetResource("Demo.Text.LogPageaxaml.OnWriteBurst024"), i));
            if (i % 50 == 0) await Task.Delay(50);
        }
    }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdMinimumLevel when change.NewValue is string level:
                MinimumLevel = level switch
                {
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.LogPageaxaml.OnPropertyCommitted003") => LogLevel.Trace,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.LogPageaxaml.OnPropertyCommitted004") => LogLevel.Debug,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.LogPageaxaml.OnPropertyCommitted005") => LogLevel.Warning,
                    var localizedValue when localizedValue == L.Instance.GetResource("Demo.Text.LogPageaxaml.OnPropertyCommitted006") => LogLevel.Error,
                    _ => LogLevel.Information,
                };
                break;
            case IdMaxDisplay when change.NewValue is decimal max:
                MaxDisplayCount = (int)max;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var minimumLevel = new PropertyItem
        {
            Id = IdMinimumLevel,
            Name = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties007"),
            Kind = PropertyItemKind.Choice,
            Options = [L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties008"), L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties009"), L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties010"), L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties011"), L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties012")],
            Value = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties013"),
            DefaultValue = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties014"),
            Description = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties015"),
        };
        var maxDisplay = new PropertyItem
        {
            Id = IdMaxDisplay,
            Name = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties016"),
            Kind = PropertyItemKind.Number,
            Value = 2000m,
            DefaultValue = 1000m,
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            Description = L.Instance.GetResource("Demo.Text.LogPageaxaml.BuildProperties017"),
        };

        return [minimumLevel, maxDisplay];
    }

}
