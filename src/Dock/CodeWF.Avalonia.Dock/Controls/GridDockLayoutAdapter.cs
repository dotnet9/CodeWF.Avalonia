using System;
using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace CodeWF.Avalonia.Dock.Controls;

/// <summary>
/// Describes one collapsible Tool region in a GridDock layout.
/// </summary>
public sealed class GridDockRegionDefinition
{
    public GridDockRegionDefinition(
        IEnumerable<int> contentColumns,
        IEnumerable<int> collapseColumns)
    {
        ArgumentNullException.ThrowIfNull(contentColumns);
        ArgumentNullException.ThrowIfNull(collapseColumns);

        ContentColumns = contentColumns.Distinct().Order().ToArray();
        CollapseColumns = collapseColumns.Distinct().Order().ToArray();

        if (ContentColumns.Count == 0)
            throw new ArgumentException("At least one content column is required.", nameof(contentColumns));
        if (CollapseColumns.Count == 0)
            throw new ArgumentException("At least one collapse column is required.", nameof(collapseColumns));
        if (ContentColumns.Any(column => column < 0))
            throw new ArgumentException("Column indexes cannot be negative.", nameof(contentColumns));
        if (CollapseColumns.Any(column => column < 0))
            throw new ArgumentException("Column indexes cannot be negative.", nameof(collapseColumns));
    }

    public IReadOnlyList<int> ContentColumns { get; }

    public IReadOnlyList<int> CollapseColumns { get; }
}

/// <summary>
/// Describes the visual shape and current visibility of a GridDock layout.
/// </summary>
public sealed class GridDockLayoutState
{
    public GridDockLayoutState(
        int columnCount,
        IEnumerable<GridDockRegionState> regions)
    {
        if (columnCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(columnCount));
        ArgumentNullException.ThrowIfNull(regions);

        ColumnCount = columnCount;
        Regions = regions.ToArray();

        if (Regions.Any(region => region is null))
            throw new ArgumentException("Regions cannot contain null values.", nameof(regions));
        if (Regions.Any(region => region.CollapseColumns.Any(column => column < 0 || column >= columnCount)))
            throw new ArgumentException("A region contains a column outside the layout.", nameof(regions));

        var collapseColumns = new HashSet<int>();
        foreach (var region in Regions)
        {
            if (region.CollapseColumns.Any(column => !collapseColumns.Add(column)))
                throw new ArgumentException("Regions cannot collapse the same column.", nameof(regions));
        }
    }

    public int ColumnCount { get; }

    public IReadOnlyList<GridDockRegionState> Regions { get; }
}

/// <summary>
/// Describes one region after the consumer has evaluated its current visibility.
/// </summary>
public sealed class GridDockRegionState
{
    public GridDockRegionState(
        IEnumerable<int> collapseColumns,
        bool hasVisibleContent)
    {
        ArgumentNullException.ThrowIfNull(collapseColumns);
        CollapseColumns = collapseColumns.Distinct().Order().ToArray();
        HasVisibleContent = hasVisibleContent;
    }

    public IReadOnlyList<int> CollapseColumns { get; }

    public bool HasVisibleContent { get; }
}

/// <summary>
/// Supplies the GridDock layout contract to <see cref="FixedToolGridDockBehavior"/>.
/// </summary>
public interface IGridDockLayoutAdapter
{
    GridDockLayoutState? GetLayoutState(IDock rootDock);
}

/// <summary>
/// Standard adapter for layouts whose Tool regions are identified by GridDock columns.
/// </summary>
public sealed class GridDockColumnLayoutAdapter : IGridDockLayoutAdapter
{
    private readonly GridDockRegionDefinition[] _regions;
    private readonly string? _targetDockId;

    public GridDockColumnLayoutAdapter(
        int columnCount,
        params GridDockRegionDefinition[] regions)
        : this(null, columnCount, regions)
    {
    }

    public GridDockColumnLayoutAdapter(
        string? targetDockId,
        int columnCount,
        params GridDockRegionDefinition[] regions)
    {
        if (columnCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(columnCount));
        ArgumentNullException.ThrowIfNull(regions);

        if (regions.Any(region => region is null))
            throw new ArgumentException("Regions cannot contain null values.", nameof(regions));
        if (regions.Any(region =>
                region.ContentColumns.Any(column => column < 0 || column >= columnCount) ||
                region.CollapseColumns.Any(column => column < 0 || column >= columnCount)))
        {
            throw new ArgumentException("A region contains a column outside the layout.", nameof(regions));
        }

        var collapseColumns = new HashSet<int>();
        foreach (var region in regions)
        {
            if (region.CollapseColumns.Any(column => !collapseColumns.Add(column)))
                throw new ArgumentException("Regions cannot collapse the same column.", nameof(regions));
        }

        ColumnCount = columnCount;
        _targetDockId = targetDockId;
        _regions = regions.ToArray();
    }

    public int ColumnCount { get; }

    public GridDockLayoutState? GetLayoutState(IDock rootDock)
    {
        ArgumentNullException.ThrowIfNull(rootDock);

        if (_targetDockId is not null &&
            !string.Equals(rootDock.Id, _targetDockId, StringComparison.Ordinal))
        {
            return null;
        }

        return new GridDockLayoutState(
            ColumnCount,
            _regions.Select(region => new GridDockRegionState(
                region.CollapseColumns,
                HasVisibleContent(rootDock, region.ContentColumns))));
    }

    private static bool HasVisibleContent(
        IDock dock,
        IReadOnlyList<int> contentColumns)
    {
        if (dock.VisibleDockables is not { } visibleDockables)
            return false;

        return visibleDockables.Any(item =>
            contentColumns.Contains(item.Column) && HasVisibleContent(item));
    }

    private static bool HasVisibleContent(IDockable item)
    {
        if (item is ISplitter || item.IsEmpty)
            return false;

        if (item is IDock dock)
            return dock.VisibleDockables is { } children && children.Any(HasVisibleContent);

        return true;
    }
}
