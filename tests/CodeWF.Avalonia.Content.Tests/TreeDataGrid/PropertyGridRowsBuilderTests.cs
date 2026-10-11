using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

using Xunit;

namespace CodeWF.Avalonia.Content.Tests.TreeDataGrid;

/// <summary>
/// PropertyGrid 可见行构建器：分类/字母双视图、搜索过滤、动态可见性的纯逻辑回归。
/// </summary>
public sealed class PropertyGridRowsBuilderTests
{
    private static PropertyItem Category(string name, params PropertyItem[] children)
        => new() { Name = name, Kind = PropertyItemKind.Category, Children = children };

    private static PropertyItem Text(string name, string value = "", string? description = null)
        => new() { Name = name, Kind = PropertyItemKind.Text, Value = value, DefaultValue = value, Description = description };

    private static IReadOnlyList<PropertyGridRow> Build(
        IReadOnlyList<PropertyItem> items,
        PropertyGridViewMode mode = PropertyGridViewMode.Categorized,
        string? search = null)
        => PropertyGridRowsBuilder.Build(items, mode, search);

    [Fact]
    public void Categorized_BuildsCategoryRowsAndDepths()
    {
        var items = new PropertyItem[]
        {
            Category("外观", Text("名称"), new PropertyItem { Name = "字体", Children = [Text("字号")] }),
            Category("文件", Text("编码")),
        };

        var rows = Build(items);

        Assert.Equal(6, rows.Count);
        Assert.IsType<PropertyCategoryRow>(rows[0]);
        Assert.Equal("外观", rows[0].Item.Name);
        Assert.Equal(0, rows[0].Depth);
        Assert.IsType<PropertyLeafRow>(rows[1]);
        Assert.Equal(1, rows[1].Depth);
        Assert.Equal(1, rows[2].Depth); // 字体（可展开行，与名称同层）
        Assert.Equal(2, rows[3].Depth); // 字号
        Assert.Equal(0, rows[4].Depth); // 文件
        Assert.Equal(1, rows[5].Depth); // 编码
    }

    [Fact]
    public void Categorized_CollapsedGroupHidesChildren()
    {
        var group = new PropertyItem
        {
            Name = "外观",
            Kind = PropertyItemKind.Category,
            Children = [Text("名称"), Text("可见")],
            IsExpanded = false,
        };
        var items = new PropertyItem[] { group, Category("文件", Text("编码")) };

        var rows = Build(items);

        Assert.Equal(3, rows.Count); // 外观（已折叠）+ 文件 + 编码
        Assert.IsType<PropertyCategoryRow>(rows[0]);
        Assert.IsType<PropertyCategoryRow>(rows[1]);
        Assert.IsType<PropertyLeafRow>(rows[2]);
    }

    [Fact]
    public void Categorized_SearchKeepsAncestorChainAndForcesExpand()
    {
        var items = new PropertyItem[]
        {
            Category("外观", Text("名称"), new PropertyItem { Name = "字体", Children = [Text("字号")] }),
            Category("文件", Text("编码")),
        };

        var rows = Build(items, PropertyGridViewMode.Categorized, search: "字号");

        Assert.Equal(3, rows.Count); // 外观 → 字体 → 字号
        Assert.Equal("外观", rows[0].Item.Name);
        Assert.Equal("字体", rows[1].Item.Name);
        Assert.Equal("字号", rows[2].Item.Name);
        Assert.IsType<PropertyLeafRow>(rows[2]);
        Assert.True(((PropertyLeafRow)rows[2]).Item.Name == "字号");
    }

    [Fact]
    public void Alphabetical_FlattensGroupsAndSortsByPinyin()
    {
        var items = new PropertyItem[]
        {
            Category("外观", Text("不透明度"), new PropertyItem { Name = "字体", Children = [Text("字号")] }),
            Category("文件", Text("编码"), Text("便签")),
        };

        var rows = Build(items, PropertyGridViewMode.Alphabetical);

        var names = rows.Select(static row => row.Item.Name).ToArray();
        // 全部摊平（含深层子项）、无分组行、按拼音排序
        Assert.Equal(new[] { "编码", "便签", "不透明度", "字号", "字体" }, names);
        Assert.All(rows, static row => Assert.Equal(0, row.Depth));
        Assert.All(rows, static row => Assert.True(((PropertyLeafRow)row).IsFlat));
    }

    [Fact]
    public void Alphabetical_SearchIncludesMatchedSubtree()
    {
        var items = new PropertyItem[]
        {
            Category("文件", new PropertyItem { Name = "端口列表", Children = [Text("编号"), Text("直径")] }, Text("编码")),
        };

        var rows = Build(items, PropertyGridViewMode.Alphabetical, search: "直径");

        var names = rows.Select(static row => row.Item.Name).ToArray();
        Assert.Equal(new[] { "端口列表", "直径" }, names);
    }

    [Fact]
    public void VisibleWhen_SkipsHiddenItemsUntilVisible()
    {
        var extra = Text("退避系数");
        var toggle = new PropertyItem { Name = "指数退避", Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false };
        extra.VisibleWhen = () => Equals(toggle.Value, true);

        var items = new PropertyItem[] { Category("行为", toggle, extra) };

        Assert.Empty(Build(items).Where(static row => row.Item.Name == "退避系数"));

        toggle.Value = true;
        Assert.Single(Build(items).Where(static row => row.Item.Name == "退避系数"));
    }

    [Fact]
    public void ModifiedDeep_FollowsDescendantChanges()
    {
        var child = new PropertyItem
        {
            Name = "字号",
            Kind = PropertyItemKind.Number,
            DefaultValue = 12m,
            Value = 12m,
        };
        var parent = new PropertyItem { Name = "字体", Children = [child] };

        Assert.False(parent.IsModifiedDeep);

        child.Value = 14m;
        Assert.True(child.IsModified);
        Assert.True(parent.IsModifiedDeep);

        child.Reset();
        Assert.False(parent.IsModifiedDeep);
    }
}
