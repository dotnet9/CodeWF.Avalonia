using CodeWF.Avalonia.Controls.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

/// <summary>ColumnDisplayEditor 演示页 VM：列模型集合 + 变更审计（拖拽排序 / 可见性）。</summary>
public class ColumnDisplayEditorPageViewModel : ReactiveObject
{
    public ObservableCollection<ColumnDisplayItem> Columns { get; }

    public ObservableCollection<string> ChangeLog { get; } = new();

    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

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

        ChangeLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 重置为默认列配置");
    }

    private static ObservableCollection<ColumnDisplayItem> CreateDefaultColumns() =>
        new(
        [
            new ColumnDisplayItem("name", "名称"),
            new ColumnDisplayItem("kind", "类型"),
            new ColumnDisplayItem("status", "状态"),
            new ColumnDisplayItem("owner", "负责人"),
            new ColumnDisplayItem("updated", "更新时间"),
            new ColumnDisplayItem("remark", "备注", visible: false),
        ]);

    private void OnColumnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColumnDisplayItem.Visible)
            && sender is ColumnDisplayItem column)
        {
            ChangeLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 「{column.DisplayText}」可见性 → {(column.Visible ? "显示" : "隐藏")}");
        }
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move && e.NewStartingIndex >= 0)
        {
            var moved = e.NewItems?.Cast<ColumnDisplayItem>().FirstOrDefault();
            ChangeLog.Insert(0,
                $"[{DateTime.Now:HH:mm:ss}] 「{moved?.DisplayText}」移动到位置 {e.NewStartingIndex + 1}");
        }
    }
}
