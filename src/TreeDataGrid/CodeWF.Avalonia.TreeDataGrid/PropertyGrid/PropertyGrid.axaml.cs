using PropertyGridLanguage = CodeWF.Avalonia.TreeDataGrid.PropertyGrid.I18n.ResourceKeys;
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
using CodeWF.Toolkit.Lang;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

/// <summary>通用属性面板，支持分类和字母排序、不限层级的属性树、搜索、说明、多种值编辑器、修改标记和重置，以及行高与名称列宽拖动。通过 <see cref="Items"/> 接收 <see cref="PropertyItem"/> 树。</summary>
public partial class PropertyGrid : UserControl
{
    /// <summary>名称列的最小宽度。</summary>
    public const double MinNameColumnWidth = 90;

    /// <summary>名称列的最大宽度。</summary>
    public const double MaxNameColumnWidth = 340;

    /// <summary>单行高度的上下限，单位为像素。</summary>
    public const double MinRowHeight = 20;

    public const double MaxRowHeight = 200;

    /// <summary><see cref="ValueCommitted"/> 路由事件的标识。</summary>
    public static readonly RoutedEvent<PropertyValueChangedEventArgs> ValueCommittedEvent =
        RoutedEvent.Register<PropertyGrid, PropertyValueChangedEventArgs>(nameof(ValueCommitted), RoutingStrategies.Bubble);

    /// <summary><see cref="ValuePreviewing"/> 路由事件的标识。</summary>
    public static readonly RoutedEvent<PropertyValuePreviewEventArgs> ValuePreviewingEvent =
        RoutedEvent.Register<PropertyGrid, PropertyValuePreviewEventArgs>(nameof(ValuePreviewing), RoutingStrategies.Bubble);

    /// <summary><see cref="BrowseRequested"/> 路由事件的标识。</summary>
    public static readonly RoutedEvent<PropertyBrowseRequestedEventArgs> BrowseRequestedEvent =
        RoutedEvent.Register<PropertyGrid, PropertyBrowseRequestedEventArgs>(nameof(BrowseRequested), RoutingStrategies.Bubble);

    /// <summary>属性树的根集合，分组使用 <see cref="PropertyItemKind.Category"/> 类型。</summary>
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

    /// <summary>自定义编辑器工厂，返回非空控件时替换内置编辑器。</summary>
    public static readonly StyledProperty<Func<PropertyItem, Control?>?> EditorFactoryProperty =
        AvaloniaProperty.Register<PropertyGrid, Func<PropertyItem, Control?>?>(nameof(EditorFactory));

    /// <summary>面板背景画刷，空值使用内置主题资源。</summary>
    public static readonly StyledProperty<IBrush?> BackgroundBrushProperty =
        AvaloniaProperty.Register<PropertyGrid, IBrush?>(nameof(BackgroundBrush));

    /// <summary>可选的 Pg 资源覆盖，空值恢复内置主题。</summary>
    public static readonly StyledProperty<ResourceDictionary?> ThemeOverridesProperty =
        AvaloniaProperty.Register<PropertyGrid, ResourceDictionary?>(nameof(ThemeOverrides));

    private readonly ResourceDictionary _hostTheme = new();

    private PropertyItem? _selectedItem;
    private bool _hasVisibleWhen;
    private bool _syncingViewButtons;

    /// <summary>当前可见行，由 <see cref="PropertyGridRowsBuilder"/> 计算，在模式、搜索或展开状态变化时重建。</summary>
    public ObservableCollection<PropertyGridRow> Rows { get; } = new();

    public PropertyGrid()
    {
        InitializeComponent();
        // 默认资源作为回退，宿主可覆盖全部编辑器和行样式。
        var defaults = new ResourceDictionary();
        var themeDefaults = Resources.ThemeDictionaries.ToArray();
        Resources.ThemeDictionaries.Clear();
        foreach (var entry in themeDefaults)
            defaults.ThemeDictionaries[entry.Key] = entry.Value;
        Resources.MergedDictionaries.Add(defaults);
        Resources.MergedDictionaries.Add(_hostTheme);
        RowsHost.ItemsSource = Rows;
        ApplyChromeStrings();
        SyncViewButtons();
        SyncDescriptionMenu();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        I18nManager.Instance.CultureChanged += HandleCultureChanged;
        I18nManager.Instance.ResourcesChanged += HandleCultureChanged;
        ApplyChromeStrings();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        I18nManager.Instance.CultureChanged -= HandleCultureChanged;
        I18nManager.Instance.ResourcesChanged -= HandleCultureChanged;
        base.OnDetachedFromVisualTree(e);
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

    public ResourceDictionary? ThemeOverrides
    {
        get => GetValue(ThemeOverridesProperty);
        set => SetValue(ThemeOverridesProperty, value);
    }

    /// <summary>当前选中的属性，也是底部说明区域的数据源。</summary>
    public PropertyItem? SelectedItem
    {
        get => _selectedItem;
        private set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    /// <summary>选中项的只读样式属性，供样式和绑定使用。</summary>
    public static readonly DirectProperty<PropertyGrid, PropertyItem?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<PropertyGrid, PropertyItem?>(
            nameof(SelectedItem), o => o._selectedItem);

    /// <summary>值提交命令，参数为 <see cref="PropertyValueChange"/>，用于 MVVM 绑定。</summary>
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

        if (change.Property == ThemeOverridesProperty)
        {
            _hostTheme.Clear();
            if (ThemeOverrides is { } overrides)
                foreach (var entry in overrides)
                    _hostTheme[entry.Key] = entry.Value;
        }
        else if (change.Property == ItemsProperty)
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

    /// <summary>重新计算可见行，在模式、搜索、展开或动态可见性变化时调用。</summary>
    public void Refresh() => RebuildRows();

    /// <summary>展开所有层级。</summary>
    public void ExpandAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.IsExpanded = true;
        }

        RebuildRows();
    }

    /// <summary>折叠所有层级。</summary>
    public void CollapseAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.IsExpanded = false;
        }

        RebuildRows();
    }

    /// <summary>重置所有修改，将值恢复为默认值并刷新汇总。</summary>
    public void ResetAll()
    {
        foreach (var item in EnumerateItems())
        {
            item.Reset();
            item.RefreshSummary();
        }

        RebuildRows();
    }

    /// <summary>选择属性项，同时更新行高亮和底部说明。</summary>
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

    /// <summary>提交值变更：先触发可取消的 <see cref="ValuePreviewing"/>，再写入模型、刷新汇总链并触发 <see cref="ValueCommitted"/> 和命令。存在动态可见性时刷新行，返回是否提交成功。</summary>
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

    /// <summary>触发不写入模型的值预览，返回是否已取消。</summary>
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

        var selection = Rows.FirstOrDefault(row => row is PropertyLeafRow && row.Item.Id is not null && row.Item.Id == SelectedItem?.Id)
            ?? Rows.FirstOrDefault(static row => row is PropertyLeafRow);
        if (selection is not null) Select(selection.Item);
        else SelectedItem = null;
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
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => HandleCultureChanged(sender, e));
            return;
        }
        ApplyChromeStrings();
        RebuildRows();
    }

    /* ---------------- 工具栏事件 ---------------- */
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

    /* ---------------- 属性行事件 ---------------- */
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

    /// <summary>行尾展开按钮：按下即切换展开状态并拦截冒泡，避免分组行或属性行的按下处理重复切换。</summary>
    private void OnExpanderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: PropertyGridRow row } && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
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
