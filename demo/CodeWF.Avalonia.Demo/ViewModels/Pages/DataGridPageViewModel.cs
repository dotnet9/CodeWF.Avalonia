using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public record DataGridRowModel(string Name, int Age, string City, string Comment);

/// <summary>DataGrid 演示页 VM：增强版与原生版对照（三态排序 / 自然排序 / 智能 ToolTip）。</summary>
public class DataGridPageViewModel : ReactiveObject
{
    public ObservableCollection<DataGridRowModel> Rows { get; } = CreateRows();

    private static ObservableCollection<DataGridRowModel> CreateRows()
    {
        var names = new[]
        {
            ("item2", 28, "杭州"), ("item10", 35, "北京"), ("item1", 41, "西雅图"),
            ("item22", 19, "成都"), ("item3", 33, "柏林"), ("alpha", 25, "上海"),
            ("beta", 30, "深圳"), ("gamma", 45, "广州"), ("delta", 22, "南京"),
            ("item100", 38, "东京"),
        };

        var rows = new ObservableCollection<DataGridRowModel>();
        for (var i = 0; i < names.Length; i++)
        {
            var (name, age, city) = names[i];
            rows.Add(new DataGridRowModel(
                name,
                age,
                city,
                $"{name} 的备注信息第 {i + 1} 行——这段文字足够长，会在列宽不足时被截断，" +
                "启用智能 ToolTip 后悬停自动显示完整内容，未截断的单元格不弹提示。"));
        }

        return rows;
    }
}
