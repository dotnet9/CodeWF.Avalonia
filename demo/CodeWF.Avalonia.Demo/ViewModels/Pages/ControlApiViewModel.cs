using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using CodeWF.Toolkit.Lang;
using CodeWF.Avalonia.Log;
using CodeWF.Toolkit.Logging;
using ReactiveUI;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed class ControlApiViewModel : ReactiveObject
{
    private string _title = "CodeWFTitleBar";
    private double _thickness = 3;
    private PropertyItem? _titleProperty;
    private PropertyItem? _thicknessProperty;

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
            Logger.Warn(NotificationMessage);
        });
    }

    public string Title { get => _title; set { this.RaiseAndSetIfChanged(ref _title, value); if (_titleProperty is not null) _titleProperty.Value = value; } }
    public double Thickness { get => _thickness; set { this.RaiseAndSetIfChanged(ref _thickness, value); if (_thicknessProperty is not null) _thicknessProperty.Value = (decimal)value; } }
    public ObservableCollection<ControlRow> Rows { get; } = new(
        new[] { 10, 2, 1, 20, 3, 11 }.Select(value => new ControlRow("item" + value, value,
            I18nManager.Instance.GetResource(DemoResources.Catalog.TreeLongText))));
    public string SelectionSummary => string.Format(I18nManager.Instance.GetResource(DemoResources.Catalog.TreeSelection), Source.RowSelection?.SelectedIndexes.Count ?? 0, Rows.Count);
    public FlatTreeDataGridSource<ControlRow> Source { get; }
    public ICommand NotifyCommand { get; }

    public IReadOnlyList<PropertyItem> BuildProperties(string api)
    {
        if (api == "TitleBar")
        {
            _titleProperty = new PropertyItem { Id = nameof(Title), Name = "Title", Value = Title, DefaultValue = "CodeWFTitleBar" };
            return [_titleProperty];
        }
        if (api == "DiagonalLine")
        {
            _thicknessProperty = new PropertyItem { Id = nameof(Thickness), Name = "BorderThickness", Kind = PropertyItemKind.Number,
                Value = (decimal)Thickness, DefaultValue = 3m, Minimum = 1, Maximum = 12, Increment = 1 };
            return [_thicknessProperty];
        }
        if (api == "TreeDataGrid")
            return [new PropertyItem { Id = "SingleSelect", Name = "SingleSelect", Kind = PropertyItemKind.Boolean,
                Value = Source.RowSelection!.SingleSelect, DefaultValue = false }];
        return [new PropertyItem { Id = "Message", Name = "Message", Value = NotificationMessage,
            DefaultValue = I18nManager.Instance.GetResource(DemoResources.Catalog.Notification) }];
    }

    public string NotificationMessage { get; private set; } = I18nManager.Instance.GetResource(DemoResources.Catalog.Notification);

    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case nameof(Title): Title = change.NewValue?.ToString() ?? ""; break;
            case nameof(Thickness): Thickness = Convert.ToDouble(change.NewValue); break;
            case "SingleSelect": Source.RowSelection!.SingleSelect = change.NewValue is true; break;
            case "Message": NotificationMessage = change.NewValue?.ToString() ?? ""; break;
        }
    }
}

public sealed record ControlRow(string Name, int Value, string Description);
