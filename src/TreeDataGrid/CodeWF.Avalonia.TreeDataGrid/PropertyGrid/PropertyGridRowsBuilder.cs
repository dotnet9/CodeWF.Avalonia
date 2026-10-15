using System.Globalization;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>根据模型树、显示模式和搜索词计算可见行；分类模式保留层级及展开状态，字母模式按名称平铺。搜索保留匹配的可见祖先。</summary>
public static class PropertyGridRowsBuilder
{
    /// <summary>属性名称比较器，中文按拼音排序。</summary>
    public static StringComparer NameComparer { get; } =
        StringComparer.Create(CultureInfo.GetCultureInfo("zh-Hans-CN"), ignoreCase: true);

    public static IReadOnlyList<PropertyGridRow> Build(
        IEnumerable<PropertyItem> items,
        PropertyGridViewMode mode,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(items);

        var roots = items as IReadOnlyList<PropertyItem> ?? items.ToList();
        Normalize(roots, parent: null, depth: 0);

        var query = NormalizeQuery(searchText);
        return mode == PropertyGridViewMode.Alphabetical
            ? BuildFlat(roots, query)
            : BuildCategorized(roots, query);
    }

    /// <summary>规范搜索词：去除两端空白并转为小写，空值表示不过滤。</summary>
    public static string? NormalizeQuery(string? searchText)
    {
        var query = searchText?.Trim();
        return string.IsNullOrEmpty(query) ? null : query.ToLowerInvariant();
    }

    /// <summary>判断属性名称或说明是否包含搜索词。</summary>
    public static bool MatchesQuery(PropertyItem item, string? query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        return item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
               || (item.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>判断可见子树是否包含匹配项，包括当前项。</summary>
    public static bool SubtreeMatches(PropertyItem item, string? query)
    {
        if (item.VisibleWhen?.Invoke() == false)
        {
            return false;
        }

        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        return MatchesQuery(item, query) || (item.Children?.Any(child => SubtreeMatches(child, query)) ?? false);
    }

    private static void Normalize(IReadOnlyList<PropertyItem> items, PropertyItem? parent, int depth)
    {
        foreach (var item in items)
        {
            item.Parent = parent;
            item.Depth = depth;
            if (item.Children is { Count: > 0 } children)
            {
                Normalize(children, item, depth + 1);
            }
        }
    }

    private static List<PropertyGridRow> BuildCategorized(IReadOnlyList<PropertyItem> items, string? query)
    {
        var rows = new List<PropertyGridRow>();
        AppendCategorized(rows, items, depth: 0, query);
        return rows;
    }

    private static void AppendCategorized(List<PropertyGridRow> rows, IReadOnlyList<PropertyItem> items, int depth, string? query)
    {
        foreach (var item in items)
        {
            if (item.VisibleWhen?.Invoke() == false)
            {
                continue;
            }

            if (item.Kind == PropertyItemKind.Category)
            {
                if (query is not null && !SubtreeMatches(item, query))
                {
                    continue;
                }

                rows.Add(new PropertyCategoryRow(item, depth));
                if (query is not null || item.IsExpanded)
                {
                    AppendCategorized(rows, item.Children ?? [], depth + 1, query);
                }

                continue;
            }

            if (query is not null && !SubtreeMatches(item, query))
            {
                continue;
            }

            rows.Add(new PropertyLeafRow(item, depth, isFlat: false));
            if (item.HasChildren && (query is not null || item.IsExpanded))
            {
                AppendCategorized(rows, item.Children ?? [], depth + 1, query);
            }
        }
    }

    private static List<PropertyGridRow> BuildFlat(IReadOnlyList<PropertyItem> items, string? query)
    {
        var flat = new List<PropertyItem>();
        CollectFlat(flat, items, query);
        flat.Sort(static (a, b) => NameComparer.Compare(a.Name, b.Name));
        return flat.ConvertAll(item => (PropertyGridRow)new PropertyLeafRow(item, depth: 0, isFlat: true));
    }

    private static void CollectFlat(List<PropertyItem> collected, IReadOnlyList<PropertyItem> items, string? query)
    {
        foreach (var item in items)
        {
            if (item.VisibleWhen?.Invoke() == false)
            {
                continue;
            }

            if (item.Kind == PropertyItemKind.Category)
            {
                CollectFlat(collected, item.Children ?? [], query);
                continue;
            }

            if (query is null || MatchesQuery(item, query))
            {
                collected.Add(item);
                CollectFlat(collected, item.Children ?? [], query: null);
            }
            else if (item.Children?.Any(child => SubtreeMatches(child, query)) == true)
            {
                // 仅可见后代命中时，保留祖先并继续筛选可见子树。
                collected.Add(item);
                CollectFlat(collected, item.Children ?? [], query);
            }
        }
    }

}
