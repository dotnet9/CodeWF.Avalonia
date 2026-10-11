using CodeWF.Avalonia.Content.Tests.Markdown.Rendering;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.TreeDataGrid;

/// <summary>
/// PropertyGrid 无头实例化回归：控件 XAML 运行时加载、可见行重建、选中与提交流程。
/// </summary>
[Collection("AvaloniaPlatform")]
public sealed class PropertyGridHeadlessTests
{
    private readonly AvaloniaPlatformFixture _platform;

    public PropertyGridHeadlessTests(AvaloniaPlatformFixture platform) => _platform = platform;

    [Fact]
    public void PropertyGrid_Instantiates_Rebuilds_Selects_AndCommits()
    {
        _platform.Run(() =>
        {
            var grid = new PropertyGrid();
            var name = new PropertyItem { Name = "名称", Kind = PropertyItemKind.Text, Value = "阀门-01", DefaultValue = "" };
            var appearance = new PropertyItem
            {
                Name = "外观",
                Kind = PropertyItemKind.Category,
                Children = [name],
            };

            grid.Items = new PropertyItem[] { appearance };

            // 分类视图：分组行 + 属性行
            Assert.Equal(2, grid.Rows.Count);
            Assert.Equal(name, grid.SelectedItem); // 自动选中首个属性行

            // 选中与描述面板数据源
            grid.Select(name);
            Assert.Equal(name, grid.SelectedItem);
            Assert.True(name.IsSelected);

            // 提交：预览事件可取消
            PropertyValuePreviewEventArgs? previewArgs = null;
            EventHandler<PropertyValuePreviewEventArgs> cancelHandler = (_, e) =>
            {
                previewArgs = e;
                e.Cancel = true;
            };
            grid.ValuePreviewing += cancelHandler;
            Assert.False(grid.CommitValue(name, "被拒绝的值"));
            Assert.NotNull(previewArgs);
            Assert.NotEqual("被拒绝的值", name.Value);

            grid.ValuePreviewing -= cancelHandler;
            var committed = new List<PropertyValueChangedEventArgs>();
            grid.ValueCommitted += (_, e) => committed.Add(e);
            Assert.True(grid.CommitValue(name, "新名称"));
            Assert.Equal("新名称", name.Value);
            Assert.True(name.IsModified);
            Assert.Single(committed);
            Assert.Equal("阀门-01", committed[0].OldValue);

            // 字母视图：摊平为单行
            grid.ViewMode = PropertyGridViewMode.Alphabetical;
            Assert.Single(grid.Rows);

            // 搜索过滤
            grid.SearchText = "不存在的属性";
            Assert.Empty(grid.Rows);
            grid.SearchText = null;

            // 展开折叠与重置
            grid.ViewMode = PropertyGridViewMode.Categorized;
            grid.CollapseAll();
            Assert.Single(grid.Rows);
            grid.ExpandAll();
            Assert.Equal(2, grid.Rows.Count);
            grid.ResetAll();
            Assert.Equal(string.Empty, name.Value); // 默认值为空串，重置后回到默认
            Assert.False(name.IsModified);
        });
    }

    [Fact]
    public void PropertyGrid_DynamicVisibility_RowsOnRefresh()
    {
        _platform.Run(() =>
        {
            var hidden = new PropertyItem { Name = "退避系数", Kind = PropertyItemKind.Number, Value = 1.6m, DefaultValue = 1.5m };
            var toggle = new PropertyItem { Name = "指数退避", Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false };
            hidden.VisibleWhen = () => Equals(toggle.Value, true);

            var grid = new PropertyGrid
            {
                Items = new PropertyItem[] { hidden, toggle },
            };

            Assert.Single(grid.Rows); // 退避系数被隐藏
            toggle.Value = true;
            grid.Refresh();
            Assert.Equal(2, grid.Rows.Count);
        });
    }
}
