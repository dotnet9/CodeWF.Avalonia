using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Layoutable = Avalonia.Layout.Layoutable;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CodeWF.Avalonia.Lang;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>
/// Generic property panel control (modeled after the Visual Studio Properties window):/// categorized/alphabetical views, unlimited-depth property tree, search filtering,/// description pane, multiple editors, modified-bold with reset, per-row height drag, name column resize./// The data model is a <see cref="PropertyItem"/> tree provided through <see cref="Items"/>./// </summary>
public partial class PropertyGrid : UserControl
{
    /// <summary>Minimum width of the name column.</summary>
    public const double MinNameColumnWidth = 90;

    /// <summary>Maximum width of the name column.</summary>
    public const double MaxNameColumnWidth = 340;

    /// <summary>Lower/upper bounds of a single row height (px).</summary>
    public const double MinRowHeight = 20;

    public const double MaxRowHeight = 200;

    /// <summary>Identifies the <see cref="ValueCommitted"/> routed event.</summary>
    public static readonly RoutedEvent<PropertyValueChangedEventArgs> ValueCommittedEvent =
        RoutedEvent.Register<PropertyGrid, PropertyValueChangedEventArgs>(nameof(ValueCommitted), RoutingStrategies.Bubble);

    /// <summary>Identifies the <see cref="ValuePreviewing"/> routed event.</summary>
    public static readonly RoutedEvent<PropertyValuePreviewEventArgs> ValuePreviewingEvent =
        RoutedEvent.Register<PropertyGrid, PropertyValuePreviewEventArgs>(nameof(ValuePreviewing), RoutingStrategies.Bubble);

    /// <summary>Identifies the <see cref="BrowseRequested"/> routed event.</summary>
    public static readonly RoutedEvent<PropertyBrowseRequestedEventArgs> BrowseRequestedEvent =
        RoutedEvent.Register<PropertyGrid, PropertyBrowseRequestedEventArgs>(nameof(BrowseRequested), RoutingStrategies.Bubble);

    /// <summary>Property item tree (root collection; groups are <see cref="PropertyItemKind.Category"/> items).</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<PropertyGrid, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<PropertyGridViewMode> ViewModeProperty =
        AvaloniaProperty.Register<PropertyGrid, PropertyGridViewMode>(nameof(ViewMode), PropertyGridViewMode.Categorized);

    public static readonly StyledProperty<bool> ShowToolBarProperty =
        AvaloniaProperty.Register<PropertyGrid, bool>(nameof(ShowToolBar), true);

    public static readonly StyledProperty<bool> ShowDescriptionPaneProperty =
        AvaloniaProperty.Register<PropertyGrid, bool>(nameof(ShowDescriptionPane), true);

    public static readonly StyledProperty<double> NameColumnWidthProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(NameColumnWidth), 148);

    public static readonly StyledProperty<double> DefaultRowHeightProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(DefaultRowHeight), 26);

    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<PropertyGrid, string?>(nameof(SearchText));

    /// <summary>Custom editor factory: a non-null control replaces the built-in editor.</summary>
    public static readonly StyledProperty<Func<PropertyItem, Control?>?> EditorFactoryProperty =
        AvaloniaProperty.Register<PropertyGrid, Func<PropertyItem, Control?>?>(nameof(EditorFactory));

    /// <summary>Panel background brush; null uses the built-in theme resource (for host palettes).</summary>
    public static readonly StyledProperty<IBrush?> BackgroundBrushProperty =
        AvaloniaProperty.Register<PropertyGrid, IBrush?>(nameof(BackgroundBrush));

    private PropertyItem? _selectedItem;
    private bool _hasVisibleWhen;
    private bool _syncingViewButtons;

    /// <summary>Visible rows (computed by <see cref="PropertyGridRowsBuilder"/>, rebuilt on view mode/search/expansion changes).</summary>
    public ObservableCollection<PropertyGridRow> Rows { get; } = new();

    public PropertyGrid()
    {
        InitializeComponent();
        RowsHost.ItemsSource = Rows;
        PropertyGridLocalization.RegisterResources();
        I18nManager.Instance.CultureChanged += HandleCultureChanged;
        ApplyChromeStrings();
        SyncViewButtons();
        SyncDescriptionMenu();
    }

    public event EventHandler<PropertyValueChangedEventArgs>? ValueCommitted;

    public event EventHandler<PropertyValuePreviewEventArgs>? ValuePreviewing;

    public event EventHandler<PropertyBrowseRequestedEventArgs>? BrowseRequested;

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public PropertyGridViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public bool ShowToolBar
    {
        get => GetValue(ShowToolBarProperty);
        set => SetValue(ShowToolBarProperty, value);
    }

    public bool ShowDescriptionPane
    {
        get => GetValue(ShowDescriptionPaneProperty);
        set => SetValue(ShowDescriptionPaneProperty, value);
    }

    public double NameColumnWidth
    {
        get => GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, Math.Clamp(value, MinNameColumnWidth, MaxNameColumnWidth));
    }

    public double DefaultRowHeight
    {
        get => GetValue(DefaultRowHeightProperty);
        set => SetValue(DefaultRowHeightProperty, Math.Max(16, value));
    }

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public Func<PropertyItem, Control?>? EditorFactory
    {
        get => GetValue(EditorFactoryProperty);
        set => SetValue(EditorFactoryProperty, value);
    }

    public IBrush? BackgroundBrush
    {
        get => GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    /// <summary>Currently selected property (description pane data source).</summary>
    public PropertyItem? SelectedItem
    {
        get => _selectedItem;
        private set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    /// <summary>Read-only StyledProperty for the selected item (for styles/bindings).</summary>
    public static readonly DirectProperty<PropertyGrid, PropertyItem?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<PropertyGrid, PropertyItem?>(
            nameof(SelectedItem), o => o._selectedItem);

    /// <summary>Value commit command (parameter is <see cref="PropertyValueChange"/>, for MVVM).</summary>
    public static readonly StyledProperty<ICommand?> ValueCommittedCommandProperty =
        AvaloniaProperty.Register<PropertyGrid, ICommand?>(nameof(ValueCommittedCommand));

    public static readonly StyledProperty<ICommand?> BrowseRequestedCommandProperty =
        AvaloniaProperty.Register<PropertyGrid, ICommand?>(nameof(BrowseRequestedCommand));

    public ICommand? ValueCommittedCommand
    {
        get => GetValue(ValueCommittedCommandProperty);
        set => SetValue(ValueCommittedCommandProperty, value);
    }

    public ICommand? BrowseRequestedCommand
    {
        get => GetValue(BrowseRequestedCommandProperty);
        set => SetValue(BrowseRequestedCommandProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsProperty)
        {
            OnItemsChanged();
        }
        else if (change.Property == ViewModeProperty)
        {
            SyncViewButtons();
            RebuildRows();
        }
        else if (change.Property == SearchTextProperty)
        {
            RebuildRows();
        }
        else if (change.Property == BackgroundBrushProperty)
        {
            if (RootGrid is not null && change.NewValue is IBrush brush)
            {
                RootGrid.Background = brush;
            }
        }
        else if (change.Property == NameColumnWidthProperty)
        {
            var width = Math.Clamp((double)change.NewValue!, MinNameColumnWidth, MaxNameColumnWidth);
            if (Math.Abs(width - (double)change.NewValue!) > 0.5)
            {
                NameColumnWidth = width;
            }
        }
    }

    /// <summary>Recomputes visible rows (call on view mode/search/expansion/dynamic visibility changes).</summary>
    public void Refresh() => RebuildRows();

    /// <summary>Expands all levels.</summary>
    public void ExpandAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.IsExpanded = true;
        }

        RebuildRows();
    }

    /// <summary>Collapses all levels.</summary>
    public void CollapseAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.IsExpanded = false;
        }

        RebuildRows();
    }

    /// <summary>Resets all modifications (values return to defaults and summaries refresh).</summary>
    public void ResetAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.Reset();
            item.RefreshSummary();
        }

        RebuildRows();
    }

    /// <summary>Selects a property item (updates row highlight and the description pane).</summary>
    public void Select(PropertyItem item)
    {
        if (ReferenceEquals(SelectedItem, item))
        {
            return;
        }

        if (SelectedItem is { } old)
        {
            old.IsSelected = false;
        }

        item.IsSelected = true;
        SelectedItem = item;
    }

    /// <summary>
    /// Commits a value change: raises <see cref="ValuePreviewing"/> (cancelable), writes the model, refreshes the summary chain,    /// raises <see cref="ValueCommitted"/> and the command; refreshes rows when dynamic visibility exists. Returns whether committed.    /// </summary>
    public bool CommitValue(PropertyItem item, object? newValue)
    {
        var oldValue = item.Value;
        if (RaiseValuePreviewing(item, oldValue, newValue))
        {
            return false;
        }

        item.Value = newValue;
        for (var node = item; node is not null; node = node.Parent)
        {
            node.RefreshSummary();
        }

        var args = new PropertyValueChangedEventArgs(item, oldValue, newValue) { RoutedEvent = ValueCommittedEvent };
        ValueCommitted?.Invoke(this, args);
        RaiseEvent(args);

        if (ValueCommittedCommand is { } command)
        {
            var parameter = new PropertyValueChange(item, oldValue, newValue);
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

        if (_hasVisibleWhen)
        {
            Dispatcher.UIThread.Post(RebuildRows);
        }

        return true;
    }

    /// <summary>Raises a value preview (not persisted); returns whether it was canceled.</summary>
    public bool PreviewValue(PropertyItem item, object? newValue)
        => RaiseValuePreviewing(item, item.Value, newValue);

    private bool RaiseValuePreviewing(PropertyItem item, object? oldValue, object? newValue)
    {
        var args = new PropertyValuePreviewEventArgs(item, oldValue, newValue) { RoutedEvent = ValuePreviewingEvent };
        ValuePreviewing?.Invoke(this, args);
        RaiseEvent(args);
        return args.Cancel;
    }

    internal void RaiseBrowseRequested(PropertyItem item)
    {
        var args = new PropertyBrowseRequestedEventArgs(item) { RoutedEvent = BrowseRequestedEvent };
        BrowseRequested?.Invoke(this, args);
        RaiseEvent(args);

        if (BrowseRequestedCommand is { } command)
        {
            if (command.CanExecute(item))
            {
                command.Execute(item);
            }
        }
    }

    private IEnumerable<PropertyItem> EnumerateItems()
    {
        if (Items is IEnumerable<PropertyItem> items)
        {
            foreach (var item in items)
            {
                foreach (var node in item.SelfAndDescendants())
                {
                    yield return node;
                }
            }
        }
    }

    private void OnItemsChanged()
    {
        var roots = (Items as IEnumerable<PropertyItem>)?.ToList() ?? new List<PropertyItem>();
        _hasVisibleWhen = roots.Any(item => item.SelfAndDescendants().Any(x => x.VisibleWhen is not null));
        RebuildRows();

        if (SelectedItem is null && Rows.FirstOrDefault(static row => row is PropertyLeafRow) is { } first)
        {
            Select(first.Item);
        }
    }

    private void RebuildRows()
    {
        var roots = Items as IEnumerable<PropertyItem> ?? [];
        foreach (var root in roots)
        {
            root.RecalculateModifiedDeep();
        }

        var rows = PropertyGridRowsBuilder.Build(roots, ViewMode, SearchText);
        Rows.Clear();
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
    }

    private void SyncViewButtons()
    {
        if (CategorizedButton is null || AlphabetButton is null)
        {
            return;
        }

        _syncingViewButtons = true;
        try
        {
            CategorizedButton.IsChecked = ViewMode == PropertyGridViewMode.Categorized;
            AlphabetButton.IsChecked = ViewMode == PropertyGridViewMode.Alphabetical;
        }
        finally
        {
            _syncingViewButtons = false;
        }
    }

    private void SyncDescriptionMenu()
    {
        if (DescriptionToggleItem is not null)
        {
            var text = PropertyGridLocalization.Get(PropertyGridLanguage.ShowDescriptionPane);
            DescriptionToggleItem.Header = (ShowDescriptionPane ? "✓ " : string.Empty) + text;
        }
    }

    private void ApplyChromeStrings()
    {
        if (SearchBox is not null)
        {
            SearchBox.PlaceholderText = PropertyGridLocalization.Get(PropertyGridLanguage.SearchPlaceholder);
        }

        if (ClearSearchButton is not null)
        {
            ToolTip.SetTip(ClearSearchButton, PropertyGridLocalization.Get(PropertyGridLanguage.ClearSearch));
        }

        ToolTip.SetTip(CategorizedButton, PropertyGridLocalization.Get(PropertyGridLanguage.CategorizedView));
        ToolTip.SetTip(AlphabetButton, PropertyGridLocalization.Get(PropertyGridLanguage.AlphabeticalView));
        ToolTip.SetTip(MoreButton, PropertyGridLocalization.Get(PropertyGridLanguage.MoreOptions));
        if (ExpandAllItem is not null)
        {
            ExpandAllItem.Header = PropertyGridLocalization.Get(PropertyGridLanguage.ExpandAll);
        }

        if (CollapseAllItem is not null)
        {
            CollapseAllItem.Header = PropertyGridLocalization.Get(PropertyGridLanguage.CollapseAll);
        }

        if (ResetAllItem is not null)
        {
            ResetAllItem.Header = PropertyGridLocalization.Get(PropertyGridLanguage.ResetAllChanges);
        }

        if (DescPlaceholderText is not null)
        {
            DescPlaceholderText.Text = PropertyGridLocalization.Get(PropertyGridLanguage.DescriptionPlaceholder);
        }

        PropertyGridChrome.ResetToDefault =
            PropertyGridLocalization.Get(PropertyGridLanguage.ResetToDefaultValue);
        PropertyGridChrome.ToggleChildren =
            PropertyGridLocalization.Get(PropertyGridLanguage.ToggleChildren);
    }
    private void HandleCultureChanged(object? sender, EventArgs e)
    {
        ApplyChromeStrings();
        RebuildRows();
    }

    /* ---------------- Toolbar events ---------------- */
    private void OnCategorizedClick(object? sender, RoutedEventArgs e)
    {
        if (!_syncingViewButtons)
        {
            ViewMode = PropertyGridViewMode.Categorized;
        }
    }

    private void OnAlphabetClick(object? sender, RoutedEventArgs e)
    {
        if (!_syncingViewButtons)
        {
            ViewMode = PropertyGridViewMode.Alphabetical;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (ClearSearchButton is not null)
        {
            ClearSearchButton.IsVisible = !string.IsNullOrEmpty(SearchBox.Text);
        }
    }

    private void OnClearSearchClick(object? sender, RoutedEventArgs e)
    {
        SearchBox.Text = string.Empty;
    }

    private void OnExpandAllClick(object? sender, RoutedEventArgs e) => ExpandAll();

    private void OnCollapseAllClick(object? sender, RoutedEventArgs e) => CollapseAll();

    private void OnDescriptionToggleClick(object? sender, RoutedEventArgs e)
    {
        ShowDescriptionPane = !ShowDescriptionPane;
        SyncDescriptionMenu();
    }

    private void OnResetAllClick(object? sender, RoutedEventArgs e) => ResetAll();

    /* ---------------- Row events ---------------- */
    private void OnRowPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Grid { DataContext: PropertyLeafRow row } && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            Select(row.Item);
        }
    }

    private void OnCategoryPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: PropertyCategoryRow row })
        {
            ToggleExpand(row.Item);
            e.Handled = true;
        }
    }

    private void OnExpanderClick(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBlock { DataContext: PropertyGridRow row })
        {
            ToggleExpand(row.Item);
            e.Handled = true;
        }
    }

    private void ToggleExpand(PropertyItem item)
    {
        item.IsExpanded = !item.IsExpanded;
        RebuildRows();
    }

    private void OnColumnHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border handle || !e.GetCurrentPoint(handle).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var startX = e.GetCurrentPoint(this).Position.X;
        var startWidth = NameColumnWidth;
        handle.Cursor = new Cursor(StandardCursorType.SizeWestEast);
        e.Pointer.Capture(handle);

        void Move(object? _, PointerEventArgs args)
        {
            var dx = args.GetCurrentPoint(this).Position.X - startX;
            NameColumnWidth = startWidth + dx;
        }

        void Finish(IPointer? pointer)
        {
            handle.PointerMoved -= Move;
            handle.PointerReleased -= Up;
            handle.PointerCaptureLost -= Lost;
            pointer?.Capture(null);
        }

        void Up(object? _, PointerReleasedEventArgs args) => Finish(args.Pointer);

        void Lost(object? _, PointerCaptureLostEventArgs args) => Finish(null);

        handle.PointerMoved += Move;
        handle.PointerReleased += Up;
        handle.PointerCaptureLost += Lost;
        e.Handled = true;
    }

    private void OnRowLinePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border line
            || line.Tag is not PropertyItem item
            || !e.GetCurrentPoint(line).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var startHeight = item.RowHeight
                          ?? (line.Parent is Layoutable parent ? parent.Bounds.Height : DefaultRowHeight);
        var startY = e.GetCurrentPoint(this).Position.Y;
        e.Pointer.Capture(line);

        void Move(object? _, PointerEventArgs args)
        {
            var dy = args.GetCurrentPoint(this).Position.Y - startY;
            item.RowHeight = Math.Clamp(Math.Round(startHeight + dy), MinRowHeight, MaxRowHeight);
        }

        void Finish(IPointer? pointer)
        {
            line.PointerMoved -= Move;
            line.PointerReleased -= Up;
            line.PointerCaptureLost -= Lost;
            pointer?.Capture(null);
        }

        void Up(object? _, PointerReleasedEventArgs args) => Finish(args.Pointer);

        void Lost(object? _, PointerCaptureLostEventArgs args) => Finish(null);

        line.PointerMoved += Move;
        line.PointerReleased += Up;
        line.PointerCaptureLost += Lost;
        e.Handled = true;
    }

    private void OnRowLineDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { Tag: PropertyItem item })
        {
            item.RowHeight = null;
        }
    }

    private void OnResetRowClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as MenuItem)?.DataContext is PropertyGridRow row)
        {
            row.Item.ResetSubtree();
            foreach (var node in row.Item.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            RebuildRows();
        }
    }

    private void OnToggleRowClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as MenuItem)?.DataContext is PropertyGridRow row)
        {
            ToggleExpand(row.Item);
        }
    }
}
