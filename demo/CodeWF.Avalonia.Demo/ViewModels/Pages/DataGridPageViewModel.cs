using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public record DataGridRowModel(string Name, int Age, string City, string Comment);

/// <summary>DataGrid 演示页 VM：增强版与原生版对照（三态排序 / 自然排序 / 智能 ToolTip）。</summary>
public class DataGridPageViewModel : ReactiveObject
{
    private const string IdUseDefaults = "dg.useDefaults";
    private const string IdRowHeight = "dg.rowHeight";

    private bool _useDefaults = true;
    private double _rowHeight = 32;
    public bool UseDefaults { get => _useDefaults; private set => this.RaiseAndSetIfChanged(ref _useDefaults, value); }
    public double RowHeight { get => _rowHeight; private set => this.RaiseAndSetIfChanged(ref _rowHeight, value); }

    public ObservableCollection<DataGridRowModel> Rows { get; } = CreateRows();

    private static ObservableCollection<DataGridRowModel> CreateRows()
    {
        var names = new[]
        {
            ("item2", 28, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Hangzhou")), ("item10", 35, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Beijing")), ("item1", 41, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Seattle")),
            ("item22", 19, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Chengdu")), ("item3", 33, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Berlin")), ("alpha", 25, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Shanghai")),
            ("beta", 30, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Shenzhen")), ("gamma", 45, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Guangzhou")), ("delta", 22, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Nanjing")),
            ("item100", 38, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.Tokyo")),
        };

        var rows = new ObservableCollection<DataGridRowModel>();
        for (var i = 0; i < names.Length; i++)
        {
            var (name, age, city) = names[i];
            rows.Add(new DataGridRowModel(
                name,
                age,
                city,
                string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.NoteThisTextIsDeliberatelyLongAndClipsFormat"), name, i + 1) +
                L.Instance.GetResource("Demo.Content.DataGrid.CreateRows.SmartToolTipRevealsClippedContentOnHoverFully")));
        }

        return rows;
    }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdUseDefaults when change.NewValue is bool useDefaults:
                UseDefaults = useDefaults;
                break;
            case IdRowHeight when change.NewValue is decimal rowHeight:
                RowHeight = (double)rowHeight;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var useDefaults = new PropertyItem
        {
            Id = IdUseDefaults,
            Name = L.Instance.GetResource("Demo.Content.DataGrid.Properties.EnableDefaults"),
            Kind = PropertyItemKind.Boolean,
            Value = UseDefaults,
            DefaultValue = true,
            Description = L.Instance.GetResource("Demo.Content.DataGrid.Properties.UseDefaultsEnablesThreeStateAndNaturalSortingSmart"),
        };
        var rowHeight = new PropertyItem
        {
            Id = IdRowHeight,
            Name = L.Instance.GetResource("Demo.Content.DataGrid.Properties.RowHeight"),
            Kind = PropertyItemKind.Number,
            Value = (decimal)RowHeight,
            DefaultValue = 32m,
            Minimum = 24,
            Maximum = 64,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Content.DataGrid.Properties.RowHeightOfTheEnhancedGrid"),
        };

        return [useDefaults, rowHeight];
    }

}
