using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace CodeWF.Avalonia.Dock.Controls;

/// <summary>
/// Collapses configured GridDock Tool regions when they have no visible content.
/// The host supplies an <see cref="IGridDockLayoutAdapter"/> because the common
/// library cannot infer the meaning of application-specific Dock columns.
/// </summary>
public static class FixedToolGridDockBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<DockControl, Control, bool>("IsEnabled", false);

    public static readonly AttachedProperty<IGridDockLayoutAdapter?> LayoutAdapterProperty =
        AvaloniaProperty.RegisterAttached<DockControl, Control, IGridDockLayoutAdapter?>("LayoutAdapter");

    private static readonly AttachedProperty<Subscription?> SubscriptionProperty =
        AvaloniaProperty.RegisterAttached<PropertyOwner, GridDockControl, Subscription?>("Subscription");

    static FixedToolGridDockBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>((control, change) =>
        {
            if (change.NewValue is true)
            {
                control.AttachedToVisualTree += OnAttachedToVisualTree;
                control.DetachedFromVisualTree += OnDetachedFromVisualTree;
                StartOwnerRefresh(control);
            }
            else
            {
                control.AttachedToVisualTree -= OnAttachedToVisualTree;
                control.DetachedFromVisualTree -= OnDetachedFromVisualTree;
                StopOwnerRefresh(control);
                UnsubscribeAll(control);
            }
        });

        LayoutAdapterProperty.Changed.AddClassHandler<Control>((control, _) =>
        {
            if (GetIsEnabled(control))
            {
                UnsubscribeAll(control);
                StartOwnerRefresh(control);
            }
        });
    }

    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static IGridDockLayoutAdapter? GetLayoutAdapter(AvaloniaObject element) =>
        element.GetValue(LayoutAdapterProperty);

    public static void SetLayoutAdapter(AvaloniaObject element, IGridDockLayoutAdapter? value) =>
        element.SetValue(LayoutAdapterProperty, value);

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
            StartOwnerRefresh(control);
    }

    private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;
        StopOwnerRefresh(control);
        UnsubscribeAll(control);
    }

    private static void StartOwnerRefresh(Control owner)
    {
        StopOwnerRefresh(owner);

        if (!owner.IsAttachedToVisualTree())
            return;

        if (Refresh(owner))
            return;

        owner.LayoutUpdated += OnOwnerLayoutUpdated;
        Dispatcher.UIThread.Post(() => RefreshOwnerAndStopWhenReady(owner), DispatcherPriority.Background);
    }

    private static void RefreshOwnerAndStopWhenReady(Control owner)
    {
        if (!owner.IsAttachedToVisualTree() || !GetIsEnabled(owner))
            return;

        if (Refresh(owner))
            StopOwnerRefresh(owner);
    }

    private static void StopOwnerRefresh(Control owner) => owner.LayoutUpdated -= OnOwnerLayoutUpdated;

    private static void OnOwnerLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is Control control)
            RefreshOwnerAndStopWhenReady(control);
    }

    private static bool Refresh(Control owner)
    {
        var adapter = GetLayoutAdapter(owner);
        if (adapter is null)
            return true;

        var controls = owner is GridDockControl direct
            ? new[] { direct }
            : owner.GetVisualDescendants().OfType<GridDockControl>();
        var found = false;
        var ready = true;

        foreach (var gridDock in controls)
        {
            if (gridDock.DataContext is not IDock dock)
                continue;

            found = true;
            var subscription = Subscribe(gridDock, dock, adapter);
            ready &= subscription.Apply();
        }

        return found && ready;
    }

    private static Subscription Subscribe(
        GridDockControl control,
        IDock dock,
        IGridDockLayoutAdapter adapter)
    {
        if (control.GetValue(SubscriptionProperty) is { } existing &&
            ReferenceEquals(existing.Adapter, adapter) &&
            ReferenceEquals(existing.Dock, dock))
        {
            return existing;
        }

        Unsubscribe(control);
        var subscription = new Subscription(control, dock, adapter);
        control.SetValue(SubscriptionProperty, subscription);
        subscription.SubscribeDockTree();
        return subscription;
    }

    private static void Unsubscribe(GridDockControl control)
    {
        if (control.GetValue(SubscriptionProperty) is not { } subscription)
            return;

        subscription.Dispose();
        control.ClearValue(SubscriptionProperty);
    }

    private static void UnsubscribeAll(Control owner)
    {
        if (owner is GridDockControl direct)
            Unsubscribe(direct);

        foreach (var gridDock in owner.GetVisualDescendants().OfType<GridDockControl>())
            Unsubscribe(gridDock);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly GridDockControl _control;
        private readonly Dictionary<int, GridLength> _savedWidths = new();
        private readonly HashSet<INotifyPropertyChanged> _propertySources = new();
        private readonly HashSet<INotifyCollectionChanged> _collectionSources = new();
        private Grid? _layoutGrid;
        private bool[]? _lastVisibility;
        private int[][]? _regionColumns;
        private bool _refreshPending;
        private bool _disposed;

        public Subscription(
            GridDockControl control,
            IDock dock,
            IGridDockLayoutAdapter adapter)
        {
            _control = control;
            Dock = dock;
            Adapter = adapter;
        }

        public IDock Dock { get; }

        public IGridDockLayoutAdapter Adapter { get; }

        public void SubscribeDockTree()
        {
            foreach (var source in _propertySources)
                source.PropertyChanged -= OnDockChanged;
            foreach (var source in _collectionSources)
                source.CollectionChanged -= OnCollectionChanged;

            _propertySources.Clear();
            _collectionSources.Clear();
            SubscribeDock(Dock);
        }

        private void SubscribeDock(IDock dock)
        {
            if (dock is INotifyPropertyChanged notify)
                SubscribePropertySource(notify);
            if (dock.VisibleDockables is not INotifyCollectionChanged collection)
                return;

            _collectionSources.Add(collection);
            collection.CollectionChanged += OnCollectionChanged;
            foreach (var child in dock.VisibleDockables ?? Array.Empty<IDockable>())
            {
                if (child is INotifyPropertyChanged childNotify)
                    SubscribePropertySource(childNotify);
                if (child is IDock childDock)
                    SubscribeDock(childDock);
            }
        }

        private void SubscribePropertySource(INotifyPropertyChanged source)
        {
            if (_propertySources.Add(source))
                source.PropertyChanged += OnDockChanged;
        }

        private void OnDockChanged(object? sender, PropertyChangedEventArgs e) => ScheduleRefresh();

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SubscribeDockTree();
            ScheduleRefresh();
        }

        private void ScheduleRefresh()
        {
            if (_disposed || _refreshPending)
                return;

            _refreshPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _refreshPending = false;
                if (_disposed || !_control.IsAttachedToVisualTree())
                    return;

                if (!Apply())
                {
                    _control.LayoutUpdated -= OnLayoutUpdated;
                    _control.LayoutUpdated += OnLayoutUpdated;
                }
            }, DispatcherPriority.Background);
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (Apply())
                _control.LayoutUpdated -= OnLayoutUpdated;
        }

        public bool Apply()
        {
            var state = Adapter.GetLayoutState(Dock);
            if (state is null)
                return true;

            var grid = FindLayoutGrid(state.ColumnCount);
            if (grid is null)
                return false;

            if (!ReferenceEquals(_layoutGrid, grid))
            {
                _layoutGrid = grid;
                _savedWidths.Clear();
                _lastVisibility = null;
                _regionColumns = null;
            }

            var regionColumns = state.Regions.Select(region => region.CollapseColumns.ToArray()).ToArray();
            if (!AreSameShape(_regionColumns, regionColumns))
            {
                _savedWidths.Clear();
                _lastVisibility = null;
                _regionColumns = regionColumns;
            }

            if (_lastVisibility is null || _lastVisibility.Length != state.Regions.Count)
            {
                _lastVisibility = new bool[state.Regions.Count];
                for (var regionIndex = 0; regionIndex < state.Regions.Count; regionIndex++)
                {
                    var region = state.Regions[regionIndex];
                    SaveCurrentWidths(region.CollapseColumns);
                    if (!region.HasVisibleContent)
                        Collapse(region.CollapseColumns);
                    _lastVisibility[regionIndex] = region.HasVisibleContent;
                }

                return true;
            }

            for (var regionIndex = 0; regionIndex < state.Regions.Count; regionIndex++)
            {
                var region = state.Regions[regionIndex];
                var wasVisible = _lastVisibility[regionIndex];

                if (region.HasVisibleContent)
                {
                    if (!wasVisible)
                        Restore(region.CollapseColumns);
                    else
                        SaveCurrentWidths(region.CollapseColumns);
                }
                else
                {
                    if (wasVisible)
                        SaveCurrentWidths(region.CollapseColumns);
                    Collapse(region.CollapseColumns);
                }

                _lastVisibility[regionIndex] = region.HasVisibleContent;
            }

            return true;
        }

        private Grid? FindLayoutGrid(int columnCount)
        {
            var gridDockItems = _control.GetVisualDescendants()
                .OfType<ItemsControl>()
                .Where(items => items.Classes.Contains("GridDock"));

            var matchingItems = gridDockItems.FirstOrDefault(items =>
                ReferenceEquals(items.DataContext, Dock));
            var grid = matchingItems?.ItemsPanelRoot as Grid;

            return grid is { } layoutGrid &&
                   layoutGrid.ColumnDefinitions.Count == columnCount
                ? layoutGrid
                : null;
        }

        private void SaveCurrentWidths(IEnumerable<int> columns)
        {
            foreach (var column in columns)
            {
                if (column < 0 || column >= _layoutGrid!.ColumnDefinitions.Count)
                    continue;

                var width = _layoutGrid.ColumnDefinitions[column].Width;
                if (width.Value > 0 || width.IsStar)
                    _savedWidths[column] = width;
            }
        }

        private void Collapse(IEnumerable<int> columns)
        {
            var grid = _layoutGrid;
            if (grid is null)
                return;

            foreach (var column in columns)
            {
                if (column < 0 || column >= grid.ColumnDefinitions.Count)
                    continue;
                grid.ColumnDefinitions[column].Width = new GridLength(0);
            }

            grid.InvalidateMeasure();
            grid.InvalidateArrange();
        }

        private void Restore(IEnumerable<int> columns)
        {
            var grid = _layoutGrid;
            if (grid is null)
                return;

            foreach (var column in columns)
            {
                if (column < 0 || column >= grid.ColumnDefinitions.Count ||
                    !_savedWidths.TryGetValue(column, out var width))
                    continue;

                grid.ColumnDefinitions[column].Width = width;
            }

            grid.InvalidateMeasure();
            grid.InvalidateArrange();
        }

        private static bool AreSameShape(int[][]? current, int[][] next)
        {
            if (current is null || current.Length != next.Length)
                return false;

            return current.Zip(next).All(pair => pair.First.SequenceEqual(pair.Second));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _control.LayoutUpdated -= OnLayoutUpdated;

            RestoreSavedWidths();

            foreach (var source in _propertySources)
                source.PropertyChanged -= OnDockChanged;
            foreach (var source in _collectionSources)
                source.CollectionChanged -= OnCollectionChanged;

            _propertySources.Clear();
            _collectionSources.Clear();
            _savedWidths.Clear();
        }

        private void RestoreSavedWidths()
        {
            if (_layoutGrid is null)
                return;

            foreach (var (column, width) in _savedWidths)
            {
                if (column >= 0 && column < _layoutGrid.ColumnDefinitions.Count)
                    _layoutGrid.ColumnDefinitions[column].Width = width;
            }

            _layoutGrid.InvalidateMeasure();
            _layoutGrid.InvalidateArrange();
        }
    }

    private sealed class PropertyOwner : AvaloniaObject
    {
    }
}
