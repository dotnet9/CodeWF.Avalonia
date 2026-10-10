using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodeWF.Avalonia.TreeDataGrid;

public static class TreeDataGridExtension
{
    private static readonly ConditionalWeakTable<global::Avalonia.Controls.TreeDataGrid, TreeDataGridSortingState> SortingRegistrations = new();
    private static readonly ConditionalWeakTable<global::Avalonia.Controls.TreeDataGrid, TreeDataGridSelectAllState> SelectAllRegistrations = new();
    private static readonly ConditionalWeakTable<global::Avalonia.Controls.TreeDataGrid, TreeDataGridSmartTooltipsState> SmartTooltipsRegistrations = new();
    private static readonly ConditionalWeakTable<TextBlock, SmartToolTipRegistration> SmartToolTipRegistrations = new();
    private static readonly ConditionalWeakTable<TextBlock, ThemeAwareToolTipTextBlock> SmartToolTipContents = new();

    /// <summary>
    /// 为 global::Avalonia.Controls.TreeDataGrid 一次性启用默认增强：三态排序、Ctrl+A 全选和智能 ToolTip。
    /// </summary>
    public static void EnableDefaults<T>(
        this global::Avalonia.Controls.TreeDataGrid treeDataGrid,
        FlatTreeDataGridSource<T> itemSource,
        bool enableSorting = true,
        bool enableSelectAll = true,
        bool enableSmartTooltips = true)
        where T : class
    {
        if (enableSorting)
        {
            treeDataGrid.AddSorting(itemSource);
        }

        if (enableSelectAll)
        {
            treeDataGrid.AddSelectAll(itemSource);
        }

        if (enableSmartTooltips)
        {
            treeDataGrid.EnableSmartTooltips();
        }
    }

    /// <summary>
    /// 为 global::Avalonia.Controls.TreeDataGrid 添加三态排序：升序、降序、取消排序。
    /// </summary>
    public static void AddSorting<T>(this global::Avalonia.Controls.TreeDataGrid treeDataGrid, FlatTreeDataGridSource<T> itemSource)
        where T : class
    {
        if (SortingRegistrations.TryGetValue(treeDataGrid, out _))
        {
            return;
        }

        var state = new TreeDataGridSortingState();
        SortingRegistrations.Add(treeDataGrid, state);

        treeDataGrid.CanUserSortColumns = false;
        treeDataGrid.AddHandler(Button.ClickEvent, (_, e) =>
        {
            if (e.Source is not TreeDataGridColumnHeader header ||
                treeDataGrid.Source is not FlatTreeDataGridSource<T> source ||
                source.Columns is not IList columns ||
                header.ColumnIndex < 0 ||
                header.ColumnIndex >= columns.Count ||
                columns[header.ColumnIndex] is not IColumn column)
            {
                return;
            }

            e.Handled = true;
            treeDataGrid.CanUserSortColumns = false;

            var nextDirection = state.GetNextDirection(column, source);
            if (nextDirection is null)
            {
                state.Clear();
                ClearSorting(treeDataGrid, source, columns);
                return;
            }

            if (((ITreeDataGridSource)source).SortBy(column, nextDirection.Value))
            {
                state.Set(column, nextDirection.Value);
            }
            else
            {
                state.Clear();
            }
        }, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private sealed class TreeDataGridSortingState
    {
        private ITreeDataGridSource? _source;
        private IColumn? _column;
        private ListSortDirection? _direction;

        public ListSortDirection? GetNextDirection(IColumn column, ITreeDataGridSource source)
        {
            if (!ReferenceEquals(_source, source) || !ReferenceEquals(_column, column))
            {
                _source = source;
                _column = column;
                _direction = ListSortDirection.Ascending;
                return ListSortDirection.Ascending;
            }

            return _direction switch
            {
                null => ListSortDirection.Ascending,
                ListSortDirection.Ascending => ListSortDirection.Descending,
                ListSortDirection.Descending => null,
                _ => ListSortDirection.Ascending
            };
        }

        public void Set(IColumn column, ListSortDirection direction)
        {
            _column = column;
            _direction = direction;
        }

        public void Clear()
        {
            _source = null;
            _column = null;
            _direction = null;
        }
    }

    /// <summary>
    /// 为 global::Avalonia.Controls.TreeDataGrid 添加 Ctrl+A 全选，以及 Ctrl+Shift+A 取消全选。
    /// </summary>
    public static void AddSelectAll<T>(this global::Avalonia.Controls.TreeDataGrid treeDataGrid, FlatTreeDataGridSource<T> itemSource)
        where T : class
    {
        _ = itemSource;
        if (SelectAllRegistrations.TryGetValue(treeDataGrid, out _))
        {
            return;
        }

        var state = new TreeDataGridSelectAllState(
            treeDataGrid,
            TreeDataGridSelectAllState.SelectAllRows<T>);
        SelectAllRegistrations.Add(treeDataGrid, state);
        state.Enable();
    }

    private sealed class TreeDataGridSelectAllState
    {
        private readonly global::Avalonia.Controls.TreeDataGrid _treeDataGrid;
        private readonly Action<ITreeDataGridSource, bool> _selectionHandler;

        public TreeDataGridSelectAllState(
            global::Avalonia.Controls.TreeDataGrid treeDataGrid,
            Action<ITreeDataGridSource, bool> selectionHandler)
        {
            _treeDataGrid = treeDataGrid;
            _selectionHandler = selectionHandler;
        }

        public void Enable()
        {
            _treeDataGrid.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Source is TextBox ||
                (e.Source as Control)?.FindAncestorOfType<TextBox>() is not null ||
                e.Key != Key.A ||
                e.KeyModifiers is not (KeyModifiers.Control or (KeyModifiers.Control | KeyModifiers.Shift)) ||
                _treeDataGrid.Source is not ITreeDataGridSource source)
            {
                return;
            }

            if (e.KeyModifiers == KeyModifiers.Control)
            {
                _selectionHandler(source, true);
            }
            else
            {
                _selectionHandler(source, false);
            }

            e.Handled = true;
        }

        public static void SelectAllRows<T>(ITreeDataGridSource source, bool selectAll)
            where T : class
        {
            if (source is not FlatTreeDataGridSource<T> flatSource ||
                flatSource.RowSelection is not { } rowSelection)
            {
                return;
            }

            rowSelection.Clear();
            if (!selectAll)
            {
                return;
            }

            rowSelection.BeginBatchUpdate();

            try
            {
                for (var i = 0; i < flatSource.Rows.Count; i++)
                {
                    rowSelection.Select(new IndexPath(i));
                }
            }
            finally
            {
                rowSelection.EndBatchUpdate();
            }
        }
    }

    /// <summary>
    /// 为 global::Avalonia.Controls.TreeDataGrid 启用智能 ToolTip，只在内容显示不全时显示。
    /// </summary>
    public static void EnableSmartTooltips(this global::Avalonia.Controls.TreeDataGrid treeDataGrid)
    {
        var state = SmartTooltipsRegistrations.GetValue(
            treeDataGrid,
            grid => new TreeDataGridSmartTooltipsState(grid));
        state.Enable();
    }

    /// <summary>
    /// 为 global::Avalonia.Controls.TreeDataGrid 启用智能 ToolTip，可指定要处理的列索引。
    /// </summary>
    public static void EnableSmartTooltips(this global::Avalonia.Controls.TreeDataGrid treeDataGrid, params int[] targetColumnIndexes)
    {
        var state = SmartTooltipsRegistrations.GetValue(
            treeDataGrid,
            grid => new TreeDataGridSmartTooltipsState(grid));
        state.Enable(targetColumnIndexes);
    }

    private sealed class TreeDataGridSmartTooltipsState
    {
        private readonly global::Avalonia.Controls.TreeDataGrid _treeDataGrid;
        private int[]? _targetColumnIndexes;
        private bool _enabled;

        public TreeDataGridSmartTooltipsState(global::Avalonia.Controls.TreeDataGrid treeDataGrid)
        {
            _treeDataGrid = treeDataGrid;
        }

        public void Enable(params int[]? targetColumnIndexes)
        {
            if (_enabled)
            {
                return;
            }

            _enabled = true;
            _targetColumnIndexes = targetColumnIndexes is { Length: > 0 }
                ? targetColumnIndexes.Distinct().ToArray()
                : null;
            _treeDataGrid.RowPrepared += OnRowPrepared;
        }

        private void OnRowPrepared(object? sender, TreeDataGridRowEventArgs e)
        {
            var row = e.Row;
            if (row is null)
            {
                return;
            }

            DispatcherTimer.RunOnce(
                () =>
                {
                    if (row.IsAttachedToVisualTree())
                    {
                        ProcessTreeDataGridRow(row, _targetColumnIndexes);
                    }
                },
                TimeSpan.FromMilliseconds(1000));
        }
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(global::Avalonia.Controls.TreeDataGrid))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(FlatTreeDataGridSource<>))]
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicFields,
        "Avalonia.Controls.Models.TreeDataGrid.AnonymousSortableRows`1",
        "Avalonia.Controls.TreeDataGrid")]
    private static void ClearSorting<T>(global::Avalonia.Controls.TreeDataGrid treeDataGrid, FlatTreeDataGridSource<T> source, IList columns)
        where T : class
    {
        foreach (var item in columns)
        {
            if (item is IColumn column)
            {
                column.SortDirection = null;
            }
        }

        ClearPrivateField(treeDataGrid, "_userSortColumn");
        SetPrivateField(treeDataGrid, "_userSortDirection", ListSortDirection.Ascending);
        ClearPrivateField(source, "_comparer");
        ClearPrivateField(source, "_comparison");

        var rows = source.Rows;
        ClearPrivateField(rows, "_sortedIndexes");
        ClearPrivateField(rows, "_comparer");
        ClearPrivateField(rows, "_comparison");

        treeDataGrid.Source = null;
        treeDataGrid.Source = source;
    }

    private static void ClearPrivateField(object target, string fieldName)
    {
        SetPrivateField(target, fieldName, null);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2075",
        Justification = "ClearSorting preserves the private fields of the fixed legacy global::Avalonia.Controls.TreeDataGrid types with DynamicDependency.")]
    private static void SetPrivateField(object target, string fieldName, object? value)
    {
        var type = target.GetType();
        while (type is not null)
        {
            var field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new MissingFieldException(target.GetType().FullName, fieldName);
    }

    private static void ProcessTreeDataGridRow(TreeDataGridRow? row, int[]? targetColumnIndexes = default)
    {
        if (row is null)
        {
            return;
        }

        var cells = new List<(Visual Visual, int ColumnIndex)>();
        FindTreeDataGridCells(row, cells);

        if (targetColumnIndexes?.Any() != true)
        {
            if (cells.Count == 0)
            {
                ProcessCell(row);
                return;
            }

            cells.ForEach(cell => ProcessCell(cell.Visual));
            return;
        }

        var targetColumns = new HashSet<int>(targetColumnIndexes.Where(x => x >= 0));
        foreach (var cell in cells)
        {
            if (targetColumns.Contains(cell.ColumnIndex))
            {
                ProcessCell(cell.Visual);
            }
        }
    }

    private static void ProcessCell(Visual? visual)
    {
        if (visual is null)
        {
            return;
        }

        var textBlocks = new List<TextBlock>();
        FindVisualChildren(visual, textBlocks);

        textBlocks.ForEach(SetupSmartTooltip);
    }

    private static void FindTreeDataGridCells(Visual? visual, List<(Visual Visual, int ColumnIndex)> cells)
    {
        if (visual is null)
        {
            return;
        }

        foreach (var child in visual.GetVisualChildren())
        {
            if (child is Visual visualChild)
            {
                var columnIndex = TryGetTreeDataGridCellColumnIndex(visualChild);
                if (columnIndex is not null)
                {
                    cells.Add((visualChild, columnIndex.Value));
                }

                FindTreeDataGridCells(visualChild, cells);
            }
        }
    }

    private static int? TryGetTreeDataGridCellColumnIndex(Visual visual)
    {
        return visual is TreeDataGridCell cell ? cell.ColumnIndex : null;
    }

    private static void FindVisualChildren<T>(Visual? visual, List<T> array) where T : Visual
    {
        if (visual is null)
        {
            return;
        }

        foreach (var child in visual.GetVisualChildren())
        {
            if (child is T t)
            {
                array.Add(t);
            }

            if (child is Visual visualChild)
            {
                FindVisualChildren(visualChild, array);
            }
        }
    }

    private static void SetupSmartTooltip(TextBlock textBlock)
    {
        if (SmartToolTipRegistrations.TryGetValue(textBlock, out _))
        {
            return;
        }

        textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        SmartToolTipRegistrations.Add(
            textBlock,
            new SmartToolTipRegistration(
                textBlock.GetObservable(TextBlock.TextProperty)
                    .Subscribe(new AnonymousObserver<string?>(_ => UpdateToolTip(textBlock))),
                textBlock.GetObservable(Visual.BoundsProperty)
                    .Subscribe(new AnonymousObserver<Rect>(_ => UpdateToolTip(textBlock)))));

        UpdateToolTip(textBlock);
    }

    private static void UpdateToolTip(TextBlock textBlock)
    {
        try
        {
            if (string.IsNullOrEmpty(textBlock.Text) || textBlock.Bounds.Width <= 0)
            {
                ToolTip.SetTip(textBlock, null);
                return;
            }

            var formattedText = new FormattedText(
                textBlock.Text!,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight),
                textBlock.FontSize,
                textBlock.Foreground);

            ToolTip.SetTip(
                textBlock,
                formattedText.Width > textBlock.Bounds.Width ? GetSmartToolTipContent(textBlock) : null);
        }
        catch (ArgumentException)
        {
            ToolTip.SetTip(textBlock, null);
        }
    }

    private static TextBlock GetSmartToolTipContent(TextBlock owner)
    {
        var content = SmartToolTipContents.GetValue(owner, static _ => new ThemeAwareToolTipTextBlock());

        content.Text = owner.Text;
        return content;
    }

    private sealed class SmartToolTipRegistration
    {
        private readonly IDisposable _textSubscription;
        private readonly IDisposable _boundsSubscription;

        public SmartToolTipRegistration(IDisposable textSubscription, IDisposable boundsSubscription)
        {
            _textSubscription = textSubscription;
            _boundsSubscription = boundsSubscription;
        }
    }

    private sealed class ThemeAwareToolTipTextBlock : TextBlock
    {
        private IDisposable? _foregroundBinding;

        public ThemeAwareToolTipTextBlock()
        {
            TextWrapping = TextWrapping.Wrap;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _foregroundBinding?.Dispose();
            if (this.FindAncestorOfType<ToolTip>() is { } toolTip)
            {
                _foregroundBinding = Bind(ForegroundProperty, toolTip.GetObservable(ToolTip.ForegroundProperty));
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _foregroundBinding?.Dispose();
            _foregroundBinding = null;
            base.OnDetachedFromVisualTree(e);
        }
    }
}
