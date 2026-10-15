using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.Controls.Models;
using ReactiveUI;
using ReactiveUI.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>ColumnDisplayEditor 演示页 VM：列模型集合 + 变更审计（拖拽排序 / 可见性）。</summary>
public class ColumnDisplayEditorPageViewModel : ReactiveObject
{
    public ObservableCollection<ColumnDisplayItem> Columns { get; }

    public ObservableCollection<string> ChangeLog { get; } = new();

    public ReactiveCommand<RxVoid, RxVoid> ResetCommand { get; }

    public ColumnDisplayEditorPageViewModel()
    {
        Columns = CreateDefaultColumns();
        foreach (var column in Columns)
        {
            column.PropertyChanged += OnColumnPropertyChanged;
        }

        Columns.CollectionChanged += OnColumnsCollectionChanged;
        ResetCommand = ReactiveCommand.Create(Reset);
    }

    public void Reset()
    {
        foreach (var column in Columns)
        {
            column.PropertyChanged -= OnColumnPropertyChanged;
        }

        Columns.Clear();
        foreach (var column in CreateDefaultColumns())
        {
            Columns.Add(column);
            column.PropertyChanged += OnColumnPropertyChanged;
        }

        ChangeLog.Insert(0, string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.Reset.RestoredDefaultColumnsFormat), DateTime.Now));
    }

    private static ObservableCollection<ColumnDisplayItem> CreateDefaultColumns() =>
        new(
        [
            new ColumnDisplayItem("name", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.Name)),
            new ColumnDisplayItem("kind", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.Type)),
            new ColumnDisplayItem("status", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.Status)),
            new ColumnDisplayItem("owner", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.Owner)),
            new ColumnDisplayItem("updated", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.UpdatedAt)),
            new ColumnDisplayItem("remark", L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.CreateDefaultColumns.Notes), visible: false),
        ]);

    private void OnColumnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColumnDisplayItem.Visible)
            && sender is ColumnDisplayItem column)
        {
            ChangeLog.Insert(0, string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.OnColumnPropertyChanged.VisibilityOfFormat), DateTime.Now, column.DisplayText, (column.Visible ? L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.OnColumnPropertyChanged.Visible) : L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.OnColumnPropertyChanged.Hidden))));
        }
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move && e.NewStartingIndex >= 0)
        {
            var moved = e.NewItems?.Cast<ColumnDisplayItem>().FirstOrDefault();
            ChangeLog.Insert(0,
                string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.ColumnDisplayEditor.OnColumnsCollectionChanged.MovedToPositionFormat), DateTime.Now, moved?.DisplayText, e.NewStartingIndex + 1));
        }
    }
}
