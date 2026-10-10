using System.Collections;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using global::CodeWF.Avalonia.TreeDataGrid;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.TreeDataGrid;

/// <summary>
/// TreeDataGrid 三态排序的取消排序依赖上游内部字段回写（反射 + DynamicDependency），
/// 换用社区版 TreeDataGrid.Avalonia 后必须验证字段名与类型仍然存在。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class TreeDataGridSortingRegressionTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public TreeDataGridSortingRegressionTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void ClearSorting_ResetsPrivateState_WithoutMissingField()
    {
        var (treeDataGrid, source, columns) = _platform.Run(() =>
        {
            var grid = new global::Avalonia.Controls.TreeDataGrid();
            var items = new[] { new Item("b"), new Item("a"), new Item("c") };
            var textColumn = new TextColumn<Item, string>(nameof(Item.Name), x => x.Name);
            var src = new FlatTreeDataGridSource<Item>(items);
            src.Columns.Add(textColumn);
            grid.Source = src;

            // 启用增强（注册排序处理器），然后模拟用户升序排序，
            // 让上游进入匿名可排序行状态，这正是取消排序要回写的场景。
            grid.AddSorting(src);
            src.SortBy(textColumn, System.ComponentModel.ListSortDirection.Ascending);

            return (grid, src, (IList)src.Columns);
        });

        // 反射调用内部 ClearSorting：字段名漂移时会抛 MissingFieldException，测试即失败。
        var clearSorting = typeof(TreeDataGridExtension).GetMethod(
            "ClearSorting",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(clearSorting);
        var closed = clearSorting!.MakeGenericMethod(typeof(Item));

        var (sortDirection, sourceRestored) = _platform.Run(() =>
        {
            closed.Invoke(null, new[] { (object)treeDataGrid, source, columns });
            var textColumn = (TextColumn<Item, string>)columns[0]!;
            var current = (FlatTreeDataGridSource<Item>)treeDataGrid.Source!;
            return (textColumn.SortDirection, ReferenceEquals(current, source));
        });

        Assert.Null(sortDirection);
        Assert.True(sourceRestored);
    }

    private sealed record Item(string Name);
}
