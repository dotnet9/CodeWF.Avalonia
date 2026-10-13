using L = global::CodeWF.Avalonia.Lang.I18nManager;
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
            BrowseLog.Insert(0, string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Content001"), DateTime.Now, item.Name));
        });

        Build();
    }

    private static string Describe(object? value) => value switch
    {
        null => L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Describe002"),
        bool b => b ? "true" : "false",
        _ => value.ToString() ?? string.Empty,
    };

    private void Build()
    {
        var name = new PropertyItem { Id = "p01", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build003"), Kind = PropertyItemKind.Text, Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build004"), DefaultValue = "", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build005") };
        var visible = new PropertyItem { Id = "p02", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build006"), Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = true, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build007") };
        var fill = new PropertyItem { Id = "p03", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build008"), Kind = PropertyItemKind.Color, Value = "#3B82F6", DefaultValue = "#3B82F6", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build009") };
        var opacity = new PropertyItem { Id = "p04", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build010"), Kind = PropertyItemKind.Percent, Value = 72d, DefaultValue = 100d, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build011") };

        var fontFamily = new PropertyItem { Id = "p05a", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build012"), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build013"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build014"), "Consolas", "Arial"], Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build015"), DefaultValue = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build016"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build017") };
        var fontSize = new PropertyItem { Id = "p05b", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build018"), Kind = PropertyItemKind.Number, Value = 12m, DefaultValue = 12m, Minimum = 8, Maximum = 72, Increment = 1, Unit = "px", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build019") };
        var fontWeight = new PropertyItem { Id = "p05c", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build020"), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build021"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build022"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build023")], Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build024"), DefaultValue = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build025"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build026") };
        var italic = new PropertyItem { Id = "p05d", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build027"), Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build028") };
        var font = new PropertyItem
        {
            Id = "p05",
            Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build029"),
            Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build030"),
            Children = [fontFamily, fontSize, fontWeight, italic],
        };
        font.SummaryProvider = () => $"{fontFamily.Value}, {fontSize.Value}px, {fontWeight.Value}{(Equals(italic.Value, true) ? L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build031") : string.Empty)}";

        var appearance = new PropertyItem { Id = "g1", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build032"), Kind = PropertyItemKind.Category, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build033"), Children = [name, visible, fill, opacity, font] };

        var posX = new PropertyItem { Id = "p06a", Name = "X", Kind = PropertyItemKind.Number, Value = 120m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build034") };
        var posY = new PropertyItem { Id = "p06b", Name = "Y", Kind = PropertyItemKind.Number, Value = 86m, DefaultValue = 0m, Minimum = -9999, Maximum = 9999, Unit = "px", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build035") };
        var position = new PropertyItem { Id = "p06", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build036"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build037"), Children = [posX, posY] };
        position.SummaryProvider = () => $"{posX.Value}, {posY.Value}";

        var width = new PropertyItem { Id = "p07a", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build038"), Kind = PropertyItemKind.Number, Value = 160m, DefaultValue = 160m, Minimum = 40, Maximum = 960, Increment = 2, Unit = "px", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build039") };
        var height = new PropertyItem { Id = "p07b", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build040"), Kind = PropertyItemKind.Number, Value = 48m, DefaultValue = 48m, Minimum = 24, Maximum = 720, Increment = 2, Unit = "px", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build041") };
        var keepRatio = new PropertyItem { Id = "p07c", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build042"), Kind = PropertyItemKind.Boolean, Value = false, DefaultValue = false, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build043") };
        var size = new PropertyItem { Id = "p07", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build044"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build045"), Children = [width, height, keepRatio] };
        size.SummaryProvider = () => $"{width.Value} × {height.Value}";

        var rotate = new PropertyItem { Id = "p08", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build046"), Kind = PropertyItemKind.Number, Value = 0m, DefaultValue = 0m, Minimum = -360, Maximum = 360, Increment = 5, Unit = "°", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build047") };
        var centerX = new PropertyItem { Id = "p09a", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build048"), Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build049") };
        var centerY = new PropertyItem { Id = "p09b", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build050"), Kind = PropertyItemKind.Percent, Value = 50d, DefaultValue = 50d, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build051") };
        var rotationCenter = new PropertyItem { Id = "p09", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build052"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build053"), Children = [centerX, centerY] };
        rotationCenter.SummaryProvider = () => $"{centerX.Value}%, {centerY.Value}%";

        var layout = new PropertyItem { Id = "g2", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build054"), Kind = PropertyItemKind.Category, Children = [position, size, rotate, rotationCenter] };

        var joinMode = new PropertyItem { Id = "p10", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build055"), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build056"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build057"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build058")], Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build059"), DefaultValue = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build060"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build061") };
        var trigger = new PropertyItem { Id = "p11", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build062"), Kind = PropertyItemKind.Radio, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build063"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build064"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build065")], Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build066"), DefaultValue = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build067"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build068") };
        var priority = new PropertyItem { Id = "p12", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build069"), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build070"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build071"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build072")], Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build073"), DefaultValue = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build074"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build075") };

        var retryCount = new PropertyItem { Id = "p13a", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build076"), Kind = PropertyItemKind.Number, Value = 3m, DefaultValue = 3m, Minimum = 0, Maximum = 99, Unit = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build077"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build078") };
        var retryInterval = new PropertyItem { Id = "p13b", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build079"), Kind = PropertyItemKind.Number, Value = 500m, DefaultValue = 500m, Minimum = 0, Maximum = 60000, Increment = 100, Unit = "ms", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build080") };
        _backoff = new PropertyItem { Id = "p13c", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build081"), Kind = PropertyItemKind.Boolean, Value = true, DefaultValue = false, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build082") };
        _backoffFactor = new PropertyItem
        {
            Id = "p13ca",
            Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build083"),
            Kind = PropertyItemKind.Number,
            Value = 1.6m,
            DefaultValue = 1.5m,
            Minimum = 1,
            Maximum = 5,
            Increment = 0.1,
            Format = "0.0",
            Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build084"),
        };
        _backoffFactor.VisibleWhen = () => Equals(_backoff?.Value, true);
        _retryStrategy = new PropertyItem { Id = "p13", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build085"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build086"), Children = [retryCount, retryInterval, _backoff, _backoffFactor] };
        _retryStrategy.SummaryProvider = () => string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build087"), retryCount.Value, retryInterval.Value);

        var note = new PropertyItem { Id = "p14", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build088"), Kind = PropertyItemKind.MultilineText, Value = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build089"), DefaultValue = "", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build090") };

        var behavior = new PropertyItem { Id = "g3", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build091"), Kind = PropertyItemKind.Category, Children = [joinMode, trigger, priority, _retryStrategy, note] };

        var fullPath = new PropertyItem { Id = "p15", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build092"), Kind = PropertyItemKind.ReadOnly, Value = "D:\\models\\plant-v2\\pump_station.sfc", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build093") };
        var encoding = new PropertyItem { Id = "p16", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build094"), Kind = PropertyItemKind.Choice, Options = ["Unicode (UTF-8)", "Unicode (UTF-16 LE)", "GB2312", "ASCII"], Value = "Unicode (UTF-8)", DefaultValue = "Unicode (UTF-8)", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build095") };
        var outputDir = new PropertyItem { Id = "p17", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build096"), Kind = PropertyItemKind.Path, Value = "D:\\exports\\plant-v2", DefaultValue = "D:\\exports\\plant-v2", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build097") };
        var stylesheet = new PropertyItem { Id = "p18", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build098"), Kind = PropertyItemKind.Button, Watermark = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build099"), Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build100") };

        PropertyItem MakePort(string id, string name, string portId, string portType, decimal diameter, string summaryType)
        {
            var portNumber = new PropertyItem { Id = id + "a", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build101"), Kind = PropertyItemKind.Text, Value = portId, DefaultValue = portId, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build102") };
            var type = new PropertyItem { Id = id + "b", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build103"), Kind = PropertyItemKind.Choice, Options = [L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build104"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build105"), L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build106")], Value = portType, DefaultValue = portType, Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build107") };
            var dia = new PropertyItem { Id = id + "c", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build108"), Kind = PropertyItemKind.Number, Value = diameter, DefaultValue = diameter, Minimum = 50, Maximum = 2000, Increment = 10, Unit = "mm", Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build109") };
            var port = new PropertyItem { Id = id, Name = name, Description = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build110"), name), Children = [portNumber, type, dia] };
            port.SummaryProvider = () => $"DN{dia.Value} · {type.Value}";
            return port;
        }

        var ports = new PropertyItem
        {
            Id = "p19",
            Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build111"),
            Kind = PropertyItemKind.ReadOnly,
            Description = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build112"),
            Children = [MakePort("p19a", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build113"), "IN-01", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build114"), 200m, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build115")), MakePort("p19b", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build116"), "OUT-01", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build117"), 150m, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build118")), MakePort("p19c", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build119"), "DW-01", L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build120"), 80m, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build121"))],
        };
        ports.SummaryProvider = () => string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build122"), ports.Children?.Count ?? 0);
        ports.Summary = ports.SummaryProvider();

        var file = new PropertyItem { Id = "g4", Name = L.Instance.GetResource("Demo.Text.PropertyPanelDemoViewModel.Build123"), Kind = PropertyItemKind.Category, Children = [fullPath, encoding, outputDir, stylesheet, ports] };

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
                ViewMode = mode == L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.OnPropertyCommitted003") ? PropertyGridViewMode.Alphabetical : PropertyGridViewMode.Categorized;
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
            Name = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties004"),
            Kind = PropertyItemKind.Choice,
            Options = [L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties005"), L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties006")],
            Value = L.Instance.GetResource(ViewMode == PropertyGridViewMode.Categorized ? "Demo.Text.PropertyGridPageaxaml.BuildProperties005" : "Demo.Text.PropertyGridPageaxaml.BuildProperties006"),
            DefaultValue = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties008"),
            Description = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties009"),
        };
        var toolBar = new PropertyItem
        {
            Id = IdToolBar,
            Name = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties010"),
            Kind = PropertyItemKind.Boolean,
            Value = ShowToolBar,
            DefaultValue = true,
            Description = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties011"),
        };
        var descriptionPane = new PropertyItem
        {
            Id = IdDescriptionPane,
            Name = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties012"),
            Kind = PropertyItemKind.Boolean,
            Value = ShowDescriptionPane,
            DefaultValue = true,
            Description = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties013"),
        };
        var nameWidth = new PropertyItem
        {
            Id = IdNameWidth,
            Name = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties014"),
            Kind = PropertyItemKind.Number,
            Value = (decimal)NameColumnWidth,
            DefaultValue = 148m,
            Minimum = 90,
            Maximum = 340,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Text.PropertyGridPageaxaml.BuildProperties015"),
        };

        return [viewMode, toolBar, descriptionPane, nameWidth];
    }

}
