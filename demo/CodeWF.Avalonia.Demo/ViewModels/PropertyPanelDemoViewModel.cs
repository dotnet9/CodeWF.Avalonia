using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// PropertyGrid 演示：构建四级属性树（分组/可展开子树/动态摘要/动态可见性），记录值提交日志；
/// 同时持有本页嵌入 PropertyGrid 的界面状态（视图模式 / 工具栏 / 描述面板 / 搜索 / 列宽）。
/// </summary>
public class PropertyPanelDemoViewModel : ReactiveObject
{
    private const string IdViewMode = "pg.viewMode";
    private const string IdToolBar = "pg.toolbar";
    private const string IdDescriptionPane = "pg.descriptionPane";
    private const string IdNameWidth = "pg.nameWidth";


    private PropertyItem? _retryStrategy;
    private PropertyItem? _backoff;
    private PropertyItem? _backoffFactor;
    private PropertyGridViewMode _viewMode = PropertyGridViewMode.Categorized;
    private bool _showToolBar = true;
    private bool _showDescriptionPane = true;
    private string? _searchText;
    private double _nameColumnWidth = 148d;

    public ObservableCollection<PropertyItem> Items { get; } = new();

    public ObservableCollection<string> ChangeLog { get; } = new();

    public ObservableCollection<string> BrowseLog { get; } = new();

    public PropertyGridViewMode ViewMode
    {
        get => _viewMode;
        set => this.RaiseAndSetIfChanged(ref _viewMode, value);
    }

    public bool ShowToolBar
    {
        get => _showToolBar;
        set => this.RaiseAndSetIfChanged(ref _showToolBar, value);
    }

    public bool ShowDescriptionPane
    {
        get => _showDescriptionPane;
        set => this.RaiseAndSetIfChanged(ref _showDescriptionPane, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public double NameColumnWidth
    {
        get => _nameColumnWidth;
        set => this.RaiseAndSetIfChanged(ref _nameColumnWidth, value);
    }

    public ICommand ValueCommittedCommand { get; }

    public ICommand BrowseRequestedCommand { get; }

    public PropertyPanelDemoViewModel()
    {
        ValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(change =>
        {
            ChangeLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {change.Item.Name}：{Describe(change.OldValue)} → {Describe(change.NewValue)}");
        });

        BrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(item =>
        {
            BrowseLog.Insert(0, string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Content.BrowseFormat), DateTime.Now, item.Name));
        });

        Build();
    }

    private static string Describe(object? value) => value switch
    {
        null => L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Describe.Empty),
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? string.Empty,
    };

    private void Build()
    {
        var name = new PropertyItem { Id = "p01", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Name), Kind = PropertyItemKind.Text, Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Valve01), DefaultValue = "", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.DisplayNameUsedOnTheCanvasAndIn) };
        var visible = new PropertyItem { Id = "p02", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Visible), Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = true, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.HideTheNodeOnTheCanvasWhileKeeping) };
        var fill = new PropertyItem { Id = "p03", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FillColor), Kind = PropertyItemKind.Color, Value = "#3B82F6", DefaultValue = "#3B82F6", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeFillColorInRRGGBBFormat) };
        var opacity = new PropertyItem { Id = "p04", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Opacity), Kind = PropertyItemKind.Percent, Value = 72d, DefaultValue = 100d, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeOpacityDragToPreviewAndReleaseTo) };

        var fontFamily = new PropertyItem { Id = "p05a", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FontFamily), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MicrosoftYaHei), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.SimSun), "Consolas", "Arial"], Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MicrosoftYaHei), DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MicrosoftYaHei), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FontFamilyUsedToRenderText) };
        var fontSize = new PropertyItem { Id = "p05b", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FontSize), Kind = PropertyItemKind.Number, Value = 12m, DefaultValue = 12m, Minimum = 8, Maximum = 72, Increment = 1, Unit = "px", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.TextFontSize) };
        var fontWeight = new PropertyItem { Id = "p05c", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Weight), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Light), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Regular), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Bold)], Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Regular), DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Regular), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FontWeight) };
        var italic = new PropertyItem { Id = "p05d", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Italic), Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RenderTextInItalic) };
        var font = new PropertyItem
        {
            Id = "p05",
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Font),
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.GlobalTextFontSettings),
            Children = [fontFamily, fontSize, fontWeight, italic],
        };
        font.SummaryProvider = () => $"{fontFamily.Value}, {fontSize.Value}px, {fontWeight.Value}{(Equals(italic.Value, true) ? L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ItalicSummary) : string.Empty)}";

        var appearance = new PropertyItem { Id = "g1", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Appearance), Kind = PropertyItemKind.Category, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeAppearanceProperties), Children = [name, visible, fill, opacity, font] };

        var posX = new PropertyItem { Id = "p06a", Name = "X", Kind = PropertyItemKind.Number, Value = 120m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.HorizontalNodeCoordinateOnTheCanvas) };
        var posY = new PropertyItem { Id = "p06b", Name = "Y", Kind = PropertyItemKind.Number, Value = 86m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.VerticalNodeCoordinateOnTheCanvas) };
        var position = new PropertyItem { Id = "p06", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Position), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodePositionOnTheCanvas), Children = [posX, posY] };
        position.SummaryProvider = () => $"{posX.Value}, {posY.Value}";

        var width = new PropertyItem { Id = "p07a", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Width), Kind = PropertyItemKind.Number, Value = 160m, DefaultValue = 160m, Minimum = 40, Maximum = 960, Increment = 2, Unit = "px", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeWidth) };
        var height = new PropertyItem { Id = "p07b", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.High), Kind = PropertyItemKind.Number, Value = 48m, DefaultValue = 48m, Minimum = 24, Maximum = 720, Increment = 2, Unit = "px", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeHeight) };
        var keepRatio = new PropertyItem { Id = "p07c", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.KeepAspectRatio), Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.LockTheAspectRatioWhileResizing) };
        var size = new PropertyItem { Id = "p07", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Size), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodeDimensionsAndConstraints), Children = [width, height, keepRatio] };
        size.SummaryProvider = () => $"{width.Value} × {height.Value}";

        var rotate = new PropertyItem { Id = "p08", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Rotation), Kind = PropertyItemKind.Number, Value = 0m, DefaultValue = 0m, Minimum = -360, Maximum = 360, Increment = 5, Unit = "°", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RotationAroundTheAnchorPositiveValuesAreCounterclockwise) };
        var centerX = new PropertyItem { Id = "p09a", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.CenterX), Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.HorizontalRotationAnchorAsAPercentageOfWidth) };
        var centerY = new PropertyItem { Id = "p09b", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.CenterY), Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.VerticalRotationAnchorAsAPercentageOfHeight) };
        var rotationCenter = new PropertyItem { Id = "p09", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RotationAnchor), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RotationAnchorInPercentageCoordinates), Children = [centerX, centerY] };
        rotationCenter.SummaryProvider = () => $"{centerX.Value}%, {centerY.Value}%";

        var layout = new PropertyItem { Id = "g2", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Layout), Kind = PropertyItemKind.Category, Children = [position, size, rotate, rotationCenter] };

        var joinMode = new PropertyItem { Id = "p10", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.IncludeInComputation), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.IncludeInComputation), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.PreviewOnly), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Exclude)], Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.PreviewOnly), DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.IncludeInComputation), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.IncludeTheNodeInSimulationAndResultExport) };
        var trigger = new PropertyItem { Id = "p11", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Trigger), Kind = PropertyItemKind.Radio, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Manual), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Automatic), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Scheduled)], Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Automatic), DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Manual), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.TriggerThatStartsTheNode) };
        var priority = new PropertyItem { Id = "p12", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Priority), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.High), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Medium), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Low)], Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Medium), DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Medium), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ExecutionOrderWithinASchedulingCycle) };

        var retryCount = new PropertyItem { Id = "p13a", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MaximumRetries), Kind = PropertyItemKind.Number, Value = 3m, DefaultValue = 3m, Minimum = 0, Maximum = 99, Unit = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Retries), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MaximumAutomaticRetriesAfterFailure) };
        var retryInterval = new PropertyItem { Id = "p13b", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RetryInterval), Kind = PropertyItemKind.Number, Value = 500m, DefaultValue = 500m, Minimum = 0, Maximum = 60000, Increment = 100, Unit = "ms", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.DelayBetweenRetries) };
        _backoff = new PropertyItem { Id = "p13c", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ExponentialBackoff), Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = false, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MultiplyRetryDelaysByThisFactorToAvoid) };
        _backoffFactor = new PropertyItem
        {
            Id = "p13ca",
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.BackoffFactor),
            Kind = PropertyItemKind.Number,
            Value = 1.6m,
            DefaultValue = 1.5m,
            Minimum = 1,
            Maximum = 5,
            Increment = 0.1,
            Format = "0.0",
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.MultiplierAppliedToEachDelayWithExponentialBackoff),
        };
        _backoffFactor.VisibleWhen = () => Equals(_backoff?.Value, true);
        _retryStrategy = new PropertyItem { Id = "p13", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RetryPolicy), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.AutomaticRetryPolicyAfterExecutionFailure), Children = [retryCount, retryInterval, _backoff, _backoffFactor] };
        _retryStrategy.SummaryProvider = () => string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.RetriesMsFormat), retryCount.Value, retryInterval.Value);

        var note = new PropertyItem { Id = "p14", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Note), Kind = PropertyItemKind.MultilineText, Value = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.PeriodicallyCheckThatInletPressureMeetsOperatingRequirements), DefaultValue = "", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.AnnotationSavedWithTheModelExcludedFromComputation) };

        var behavior = new PropertyItem { Id = "g3", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Behavior), Kind = PropertyItemKind.Category, Children = [joinMode, trigger, priority, _retryStrategy, note] };

        var fullPath = new PropertyItem { Id = "p15", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.FullPath), Kind = PropertyItemKind.ReadOnly, Value = "D:\\models\\plant-v2\\pump_station.sfc", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.AbsoluteModelFilePathReadOnly) };
        var encoding = new PropertyItem { Id = "p16", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Encoding), Kind = PropertyItemKind.Choice, Options = ["Unicode (UTF-8)", "Unicode (UTF-16 LE)", "GB2312", "ASCII"], Value = "Unicode (UTF-8)", DefaultValue = "Unicode (UTF-8)", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.DocumentCharacterEncoding) };
        var outputDir = new PropertyItem { Id = "p17", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.OutputDirectory), Kind = PropertyItemKind.Path, Value = "D:\\exports\\plant-v2", DefaultValue = "D:\\exports\\plant-v2", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.SimulationResultDirectory) };
        var stylesheet = new PropertyItem { Id = "p18", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.StyleSheet), Kind = PropertyItemKind.Button, Watermark = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.BrowseStyleSheet), Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.StyleSheetUsedInTheNodeReportClick) };

        PropertyItem MakePort(string id, string name, string portId, string portType, decimal diameter, string summaryType)
        {
            var portNumber = new PropertyItem { Id = id + "a", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ID), Kind = PropertyItemKind.Text, Value = portId, DefaultValue = portId, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.UniquePortIdentifier) };
            var type = new PropertyItem { Id = id + "b", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Type), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Inlet), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Outlet), L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Drain)], Value = portType, DefaultValue = portType, Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.DirectionOfFlowThroughThePort) };
            var dia = new PropertyItem { Id = id + "c", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Diameter), Kind = PropertyItemKind.Number, Value = diameter, DefaultValue = diameter, Minimum = 50, Maximum = 2000, Increment = 10, Unit = "mm", Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NominalPortDiameter) };
            var port = new PropertyItem { Id = id, Name = name, Description = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ConfigurationForFormat), name), Children = [portNumber, type, dia] };
            port.SummaryProvider = () => $"DN{dia.Value} · {type.Value}";
            return port;
        }

        var ports = new PropertyItem
        {
            Id = "p19",
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Ports),
            Kind = PropertyItemKind.ReadOnly,
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.NodePortsExpandToConfigureEachPort),
            Children = [MakePort("p19a", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Inlet1), "IN-01", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Inlet), 200m, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Inlet)), MakePort("p19b", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Outlet1), "OUT-01", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Outlet), 150m, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Outlet)), MakePort("p19c", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.DrainPort), "DW-01", L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Drain), 80m, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.Drain))],
        };
        ports.SummaryProvider = () => string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.ItemsFormat), ports.Children?.Count ?? 0);
        ports.Summary = ports.SummaryProvider();

        var file = new PropertyItem { Id = "g4", Name = L.Instance.GetResource(DemoResources.Content.PropertyGridSample.Sample.File), Kind = PropertyItemKind.Category, Children = [fullPath, encoding, outputDir, stylesheet, ports] };

        foreach (var item in new[] { appearance, layout, behavior, file })
        {
            Items.Add(item);
        }

        // 初始化摘要
        foreach (var root in Items)
        {
            foreach (var node in root.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            root.RecalculateModifiedDeep();
        }
    }
    public void Commit(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdViewMode when change.NewValue is string mode:
                ViewMode = mode == L.Instance.GetResource(DemoResources.Content.PropertyGrid.OnPropertyCommitted.Alphabetical) ? PropertyGridViewMode.Alphabetical : PropertyGridViewMode.Categorized;
                break;
            case IdToolBar when change.NewValue is bool toolBar:
                ShowToolBar = toolBar;
                break;
            case IdDescriptionPane when change.NewValue is bool pane:
                ShowDescriptionPane = pane;
                break;
            case IdNameWidth when change.NewValue is decimal width:
                NameColumnWidth = (double)width;
                break;
        }
    }

    public IReadOnlyList<PropertyItem> BuildProperties()
    {
        var viewMode = new PropertyItem
        {
            Id = IdViewMode,
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.ViewMode),
            Kind = PropertyItemKind.Choice,
            Options = [L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.Categorized), L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.Alphabetical)],
            Value = L.Instance.GetResource(ViewMode == PropertyGridViewMode.Categorized ? DemoResources.Content.PropertyGrid.Properties.Categorized : DemoResources.Content.PropertyGrid.Properties.Alphabetical),
            DefaultValue = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.Categorized),
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.CategorizedViewGroupsByCategoryAlphabeticalViewFlattens),
        };
        var toolBar = new PropertyItem
        {
            Id = IdToolBar,
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.ShowToolbar),
            Kind = PropertyItemKind.Boolean,
            Value = ShowToolBar,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.ToolbarForViewSwitchingExpandCollapseAndReset),
        };
        var descriptionPane = new PropertyItem
        {
            Id = IdDescriptionPane,
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.ShowDescriptions),
            Kind = PropertyItemKind.Boolean,
            Value = ShowDescriptionPane,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.ShowTheSelectedPropertySDescriptionBelowThe),
        };
        var nameWidth = new PropertyItem
        {
            Id = IdNameWidth,
            Name = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.NameColumnWidth),
            Kind = PropertyItemKind.Number,
            Value = (decimal)NameColumnWidth,
            DefaultValue = 148m,
            Minimum = 90,
            Maximum = 340,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.PropertyGrid.Properties.InitialNameColumnWidthDragTheSeparatorTo),
        };

        return [viewMode, toolBar, descriptionPane, nameWidth];
    }

}
