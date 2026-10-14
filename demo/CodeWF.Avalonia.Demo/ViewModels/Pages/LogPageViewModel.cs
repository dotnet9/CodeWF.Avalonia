using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
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
            case "Trace": Logger.Trace(L.Instance.GetResource(DemoResources.Content.Log.OnWriteTrace.DemoTraceLogHiddenAtTheDefaultLevel)); break;
            case "Debug": Logger.Debug(L.Instance.GetResource(DemoResources.Content.Log.OnWriteDebug.DemoDebugLogHiddenAtTheDefaultLevel)); break;
            case "Info": Logger.Info(L.Instance.GetResource(DemoResources.Content.Log.OnWriteInfo.DemoInformationLog) + DateTime.Now.ToString("HH:mm:ss.fff")); break;
            case "Warning": Logger.Warn(L.Instance.GetResource(DemoResources.Content.Log.OnWriteWarning.DemoWarningLogLessThan10FreeDisk)); break;
            case "Error": Logger.Error(L.Instance.GetResource(DemoResources.Content.Log.OnWriteError.DemoErrorLogDownstreamTimeoutOf3000ms)); break;
            case "Critical": Logger.Fatal(L.Instance.GetResource(DemoResources.Content.Log.OnWriteCritical.DemoCriticalLogDatabaseConnectionFailed)); break;
        }
    }

    private static async Task WriteBurstAsync()
    {
        for (var i = 1; i <= 200; i++)
        {
            Logger.Info(string.Format(L.Instance.GetResource(DemoResources.Content.Log.OnWriteBurst.LoadTestLogEventThroughputAndDisplayThrottlingFormat), i));
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
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.Log.OnPropertyCommitted.Trace) => LogLevel.Trace,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.Log.OnPropertyCommitted.Debug) => LogLevel.Debug,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.Log.OnPropertyCommitted.Warning) => LogLevel.Warning,
                    var localizedValue when localizedValue == L.Instance.GetResource(DemoResources.Content.Log.OnPropertyCommitted.Error) => LogLevel.Error,
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
            Name = L.Instance.GetResource(DemoResources.Content.Log.Properties.MinimumLevel),
            Kind = PropertyItemKind.Choice,
            Options = [L.Instance.GetResource(DemoResources.Content.Log.Properties.Trace), L.Instance.GetResource(DemoResources.Content.Log.Properties.Debug), L.Instance.GetResource(DemoResources.Content.Log.Properties.Information), L.Instance.GetResource(DemoResources.Content.Log.Properties.Warning), L.Instance.GetResource(DemoResources.Content.Log.Properties.Error)],
            Value = L.Instance.GetResource(DemoResources.Content.Log.Properties.Information),
            DefaultValue = L.Instance.GetResource(DemoResources.Content.Log.Properties.Information),
            Description = L.Instance.GetResource(DemoResources.Content.Log.Properties.HideLogsBelowThisLevelMinimumLevel),
        };
        var maxDisplay = new PropertyItem
        {
            Id = IdMaxDisplay,
            Name = L.Instance.GetResource(DemoResources.Content.Log.Properties.MaximumEntries),
            Kind = PropertyItemKind.Number,
            Value = 2000m,
            DefaultValue = 1000m,
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            Description = L.Instance.GetResource(DemoResources.Content.Log.Properties.MaximumRetainedEntriesMaxDisplayCountOlderEntriesAreDropped),
        };

        return [minimumLevel, maxDisplay];
    }

}
