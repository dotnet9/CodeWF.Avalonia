using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CodeWF.Avalonia.DataGrid;

public interface IDataGridSortDirectionAwareComparer : IComparer
{
    ListSortDirection SortDirection { get; set; }
}

public static class DataGridExtension
{
    private static readonly ConditionalWeakTable<global::Avalonia.Controls.DataGrid, DataGridSortingState> SortingRegistrations = new();
    private static readonly ConditionalWeakTable<global::Avalonia.Controls.DataGrid, DataGridDefaultState> DefaultRegistrations = new();
    private static readonly ConditionalWeakTable<TextBlock, SmartToolTipRegistration> SmartToolTipRegistrations = new();
    private static readonly ConditionalWeakTable<TextBlock, ThemeAwareToolTipTextBlock> SmartToolTipContents = new();

    /// <summary>
    /// 为 DataGrid 一次性启用默认增强：三态排序、自然排序和智能 ToolTip。
    /// </summary>
    public static void EnableDefaults(
        this global::Avalonia.Controls.DataGrid dataGrid,
        bool enableSorting = true,
        bool enableNaturalSorting = true,
        bool enableSmartTooltips = true)
    {
        var state = DefaultRegistrations.GetValue(dataGrid, grid => new DataGridDefaultState(grid));

        if (enableSorting && !state.SortingEnabled)
        {
            dataGrid.AddSorting();
            state.SortingEnabled = true;
        }

        if (enableNaturalSorting)
        {
            state.EnableNaturalSorting();
        }

        if (enableSmartTooltips && !state.SmartTooltipsEnabled)
        {
            state.EnableSmartTooltips();
        }
    }

    /// <summary>
    /// 为 DataGrid 添加三态排序：升序、降序、取消排序。
    /// </summary>
    public static void AddSorting(this global::Avalonia.Controls.DataGrid dataGrid)
    {
        if (SortingRegistrations.TryGetValue(dataGrid, out _))
        {
            return;
        }

        var state = new DataGridSortingState();
        SortingRegistrations.Add(dataGrid, state);

        dataGrid.Sorting += (s, e) =>
        {
            e.Handled = true;

            if (s is not global::Avalonia.Controls.DataGrid grid)
            {
                return;
            }

            var view = GetOrCreateCollectionView(grid);
            if (view is null)
            {
                state.Clear();
                return;
            }

            var nextDirection = state.GetNextDirection(e.Column, view);
            view.SortDescriptions.Clear();

            if (nextDirection is not null)
            {
                var sortDescription = CreateSortDescription(e.Column, nextDirection.Value, view.Culture);
                if (sortDescription is null)
                {
                    state.Clear();
                }
                else
                {
                    view.SortDescriptions.Add(sortDescription);
                }
            }

            view.Refresh();
        };
    }

    /// <summary>
    /// 为所有可排序绑定列补充自然排序比较器。
    /// </summary>
    public static void AddNaturalSorting(this global::Avalonia.Controls.DataGrid dataGrid)
    {
        var state = DefaultRegistrations.GetValue(dataGrid, grid => new DataGridDefaultState(grid));
        state.EnableNaturalSorting();
    }

    private static DataGridCollectionView? GetOrCreateCollectionView(global::Avalonia.Controls.DataGrid dataGrid)
    {
        if (dataGrid.ItemsSource is DataGridCollectionView view)
        {
            return view;
        }

        if (dataGrid.ItemsSource is not IEnumerable source)
        {
            return null;
        }

        view = new DataGridCollectionView(source);
        dataGrid.ItemsSource = view;
        return view;
    }

    private static DataGridSortDescription? CreateSortDescription(
        DataGridColumn column,
        ListSortDirection direction,
        CultureInfo culture)
    {
        if (column.CustomSortComparer is { } comparer)
        {
            if (comparer is IDataGridSortDirectionAwareComparer directionAwareComparer)
            {
                directionAwareComparer.SortDirection = direction;
            }

            return DataGridSortDescription.FromComparer(comparer, direction);
        }

        var memberPath = GetSortPropertyPath(column);
        return string.IsNullOrWhiteSpace(memberPath)
            ? null
            : DataGridSortDescription.FromPath(memberPath, direction, culture);
    }

    private static string? GetSortPropertyPath(DataGridColumn column)
    {
        if (!string.IsNullOrWhiteSpace(column.SortMemberPath))
        {
            return column.SortMemberPath;
        }

        return column is DataGridBoundColumn { Binding: Binding binding }
            ? binding.Path
            : null;
    }

    private sealed class DataGridSortingState
    {
        private DataGridCollectionView? _view;
        private DataGridColumn? _column;
        private ListSortDirection? _direction;

        public ListSortDirection? GetNextDirection(DataGridColumn column, DataGridCollectionView view)
        {
            if (!ReferenceEquals(_view, view) || !ReferenceEquals(_column, column))
            {
                _view = view;
                _column = column;
                _direction = ListSortDirection.Ascending;
                return _direction;
            }

            _direction = _direction switch
            {
                null => ListSortDirection.Ascending,
                ListSortDirection.Ascending => ListSortDirection.Descending,
                ListSortDirection.Descending => null,
                _ => ListSortDirection.Ascending
            };

            if (_direction is null)
            {
                _column = null;
            }

            return _direction;
        }

        public void Clear()
        {
            _view = null;
            _column = null;
            _direction = null;
        }
    }

    private sealed class DataGridDefaultState
    {
        private readonly global::Avalonia.Controls.DataGrid _dataGrid;
        private bool _naturalSortingEnabled;

        public DataGridDefaultState(global::Avalonia.Controls.DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
        }

        public bool SortingEnabled { get; set; }

        public bool SmartTooltipsEnabled => _loadingRowHandler is not null;

        private EventHandler<DataGridRowEventArgs>? _loadingRowHandler;

        public void EnableNaturalSorting()
        {
            if (_naturalSortingEnabled)
            {
                ApplyNaturalSortComparers();
                return;
            }

            _naturalSortingEnabled = true;
            if (_dataGrid.Columns is INotifyCollectionChanged columns)
            {
                columns.CollectionChanged += OnColumnsChanged;
            }

            ApplyNaturalSortComparers();
        }

        public void EnableSmartTooltips(params int[]? targetColumnIndexes)
        {
            if (_loadingRowHandler is not null)
            {
                return;
            }

            var indexes = targetColumnIndexes is { Length: > 0 }
                ? targetColumnIndexes.Distinct().ToArray()
                : null;
            _loadingRowHandler = (_, e) =>
                DispatcherTimer.RunOnce(
                    () =>
                    {
                        if (e.Row.IsAttachedToVisualTree())
                        {
                            ProcessDataGridRow(e.Row, indexes);
                        }
                    },
                    TimeSpan.FromMilliseconds(1000));
            _dataGrid.LoadingRow += _loadingRowHandler;
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ApplyNaturalSortComparers();
        }

        private void ApplyNaturalSortComparers()
        {
            foreach (var column in _dataGrid.Columns)
            {
                ApplyNaturalSortComparer(column);
            }
        }

        private static void ApplyNaturalSortComparer(DataGridColumn column)
        {
            if (!column.CanUserSort || column.CustomSortComparer is not null)
            {
                return;
            }

            var sortPath = GetColumnSortPath(column);
            if (string.IsNullOrWhiteSpace(sortPath))
            {
                return;
            }

            column.CustomSortComparer = new DataGridNaturalSortComparer(sortPath);
        }

        private static string? GetColumnSortPath(DataGridColumn column)
        {
            if (!string.IsNullOrWhiteSpace(column.SortMemberPath))
            {
                return column.SortMemberPath;
            }

            if (column is DataGridBoundColumn { Binding: Binding binding })
            {
                return binding.Path;
            }

            return GetSortPropertyPath(column);
        }
    }

    /// <summary>
    /// 判断是否双击了 DataGrid 行。
    /// </summary>
    public static bool IsDoubleClickRow(TappedEventArgs? e)
    {
        if (e is null)
        {
            return true;
        }

        if (e.Source is not Control control)
        {
            return false;
        }

        var row = control.FindAncestorOfType<DataGridRow>();
        return row is not null;
    }

    /// <summary>
    /// 为 DataGrid 启用智能 ToolTip，只在内容显示不全时显示。
    /// </summary>
    public static void EnableSmartTooltips(this global::Avalonia.Controls.DataGrid dataGrid)
    {
        var state = DefaultRegistrations.GetValue(dataGrid, grid => new DataGridDefaultState(grid));
        state.EnableSmartTooltips();
    }

    /// <summary>
    /// 为 DataGrid 启用智能 ToolTip，可指定要处理的列索引。
    /// </summary>
    public static void EnableSmartTooltips(this global::Avalonia.Controls.DataGrid dataGrid, params int[] targetColumnIndexes)
    {
        var state = DefaultRegistrations.GetValue(dataGrid, grid => new DataGridDefaultState(grid));
        state.EnableSmartTooltips(targetColumnIndexes);
    }

    private static void ProcessDataGridRow(DataGridRow row, int[]? targetColumnIndexes = default)
    {
        var cells = new List<DataGridCell>();
        FindVisualChildren(row, cells);

        if (targetColumnIndexes?.Any() != true)
        {
            cells.ForEach(ProcessCell);
            return;
        }

        for (var i = 0; i < targetColumnIndexes.Length; i++)
        {
            var columnIndex = targetColumnIndexes[i];
            if (columnIndex < 0 || columnIndex >= cells.Count)
            {
                continue;
            }

            ProcessCell(cells[columnIndex]);
        }
    }

    private static void ProcessCell(DataGridCell cell)
    {
        var textBlocks = new List<TextBlock>();
        FindVisualChildren(cell, textBlocks);

        textBlocks.ForEach(SetupSmartTooltip);
    }

    private static void FindVisualChildren<T>(Visual visual, List<T> array)
        where T : Visual
    {
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
