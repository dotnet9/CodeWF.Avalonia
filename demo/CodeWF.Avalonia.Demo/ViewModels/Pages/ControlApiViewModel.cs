using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Log;
using CodeWF.Toolkit.Logging;
using ReactiveUI;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class ControlApiViewModel : ReactiveObject
{
    private string _title = "CodeWFTitleBar";
    private double _thickness = 3;

    public ControlApiViewModel()
    {
        Source = new FlatTreeDataGridSource<ControlRow>(Rows)
        {
            Columns =
            {
                new TextColumn<ControlRow, string>("Name", row => row.Name),
                new TextColumn<ControlRow, int>("Value", row => row.Value),
                new TextColumn<ControlRow, string>("Description", row => row.Description, new GridLength(180)),
            }
        };
        Source.Selection = new TreeDataGridRowSelectionModel<ControlRow>(Source) { SingleSelect = false };
        Source.RowSelection!.SelectionChanged += (_, _) => this.RaisePropertyChanged(nameof(SelectionSummary));
        NotifyCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current is not { } app) return;
            LogNotifications.SetMode(app, LogNotificationMode.InApp);
            LogNotifications.SetMinimumLevel(app, Microsoft.Extensions.Logging.LogLevel.Warning);
            Logger.Warn(I18nManager.Instance.GetResource("Demo.Catalog.Notification"));
        });
    }

    public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
    public double Thickness { get => _thickness; set => this.RaiseAndSetIfChanged(ref _thickness, value); }
    public ObservableCollection<ControlRow> Rows { get; } = new(
        new[] { 10, 2, 1, 20, 3, 11 }.Select(value => new ControlRow("item" + value, value,
            I18nManager.Instance.GetResource("Demo.Catalog.TreeLongText"))));
    public string SelectionSummary => string.Format(I18nManager.Instance.GetResource("Demo.Catalog.TreeSelection"), Source.RowSelection?.SelectedIndexes.Count ?? 0, Rows.Count);
    public FlatTreeDataGridSource<ControlRow> Source { get; }
    public ICommand NotifyCommand { get; }
}

public sealed record ControlRow(string Name, int Value, string Description);
