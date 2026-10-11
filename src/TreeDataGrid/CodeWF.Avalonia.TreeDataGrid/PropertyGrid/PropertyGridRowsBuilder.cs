using System.Globalization;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// Computes visible rows from the model tree + view mode + search text./// Categorized view renders the model tree (category rows + nested items, IsExpanded controls collapse); alphabetical view strips/// groups and depth, sorting every property by name into a flat table. Search filters by name/description, keeping matched ancestors expanded./// </summary>
public static class PropertyGridRowsBuilder
{
    /// <summary>Property name comparer (pinyin order for Chinese).</summary>
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

    /// <summary>Normalizes the search text: trims and lowercases; empty means no search.</summary>
    public static string? NormalizeQuery(string? searchText)
    {
        var query = searchText?.Trim();
        return string.IsNullOrEmpty(query) ? null : query.ToLowerInvariant();
    }

    /// <summary>Whether an item matches the search (name or description contains).</summary>
    public static bool MatchesQuery(PropertyItem item, string? query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        return item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
               || (item.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>Whether the subtree contains a match (including itself).</summary>
    public static bool SubtreeMatches(PropertyItem item, string? query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        return MatchesQuery(item, query) || item.Descendants().Any(descendant => MatchesQuery(descendant, query));
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

            if (item.VisibleWhen?.Invoke() == false)
            {
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
            if (item.Kind == PropertyItemKind.Category)
            {
                CollectFlat(collected, item.Children ?? [], query);
                continue;
            }

            if (item.VisibleWhen?.Invoke() == false)
            {
                continue;
            }

            if (query is null)
            {
                collected.Add(item);
            }
            else if (MatchesQuery(item, query))
            {
                // Matched item: include itself plus all descendants (descendants are standalone rows when flat).
                collected.Add(item);
                collected.AddRange(item.Descendants());
            }
            else if (item.Descendants().Any(descendant => MatchesQuery(descendant, query)))
            {
                // Only descendants matched: keep the ancestor row for context.
                collected.Add(item);
                CollectMatchedDescendants(collected, item.Children ?? [], query);
            }

            if (query is null && item.HasChildren)
            {
                CollectFlat(collected, item.Children ?? [], query);
            }
        }
    }

    private static void CollectMatchedDescendants(List<PropertyItem> collected, IReadOnlyList<PropertyItem> items, string? query)
    {
        foreach (var item in items)
        {
            if (item.VisibleWhen?.Invoke() == false)
            {
                continue;
            }

            if (MatchesQuery(item, query))
            {
                collected.Add(item);
                collected.AddRange(item.Descendants());
            }
            else
            {
                if (item.Descendants().Any(descendant => MatchesQuery(descendant, query)))
                {
                    collected.Add(item);
                }

                CollectMatchedDescendants(collected, item.Children ?? [], query);
            }
        }
    }
}
