using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using CodeWF.Toolkit.Logging;
using ReactiveUI;
using ReactiveUI.Primitives;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Shell 视图模型：Ribbon 分组、导航树镜像、文档区、右侧 PropertyGrid 与底部日志的调度中枢。
/// 页面/子 VM 不走容器注册，按演示目录工厂直接构造。
/// </summary>
public partial class MainViewModel : ReactiveObject
{
    private DemoDocumentViewModel? _activeDocument;
    private DemoItemViewModel? _selectedNavItem;
    private string _statusText = string.Empty;
    private bool _ribbonExpanded = true;
    private bool _logPanelVisible = true;
    private bool _propertiesPanelVisible = true;
    private AppearanceOption _selectedPalette;
    private bool _isDarkMode;
    private CultureOption _selectedCulture;
    private DemoGroupViewModel? _selectedGroup;
    private PropertyItem? _paletteItem;
    private PropertyItem? _modeItem;
    private PropertyItem? _langItem;
    private DemoDockFactory? _dockFactory;
    private RootDock? _layout;
    private Tool? _navTool;
    private Tool? _propTool;
    private Tool? _logTool;
    private DocumentDock? _docDock;

    public MainViewModel()
    {
        Groups = DemoCatalog.CreateGroups();
        Documents = new ObservableCollection<DemoDocumentViewModel>();

        OpenItemCommand = ReactiveCommand.Create<DemoItemViewModel>(OpenItem);
        CloseDocumentCommand = ReactiveCommand.Create<DemoDocumentViewModel>(CloseDocument);
        ToggleRibbonCommand = ReactiveCommand.Create(() => { RibbonExpanded = !RibbonExpanded; });
        TogglePanelCommand = ReactiveCommand.Create<string>(TogglePanel);
        QuickSaveCommand = ReactiveCommand.Create(() => { Logger.Info("保存工作区布局（演示）"); });
        QuickResetCommand = ReactiveCommand.Create(ResetLayout);
        QuickSettingsCommand = ReactiveCommand.Create(() => { Logger.Info("打开设置（演示）"); });
        QuickGuideCommand = ReactiveCommand.Create(OpenGuidanceDemo);
        WriteLogsCommand = ReactiveCommand.Create(WriteSampleLogs);

        Palettes = new ObservableCollection<AppearanceOption>
        {
            AppearanceOption.Default(),
            AppearanceOption.Aquatic(),
            AppearanceOption.Desert(),
            AppearanceOption.Dusk(),
            AppearanceOption.NightSky(),
            AppearanceOption.HighContrast(),
        };
        _selectedPalette = Palettes[0];

        Cultures = new ObservableCollection<CultureOption>
        {
            new("zh-CN", "简体中文"),
            new("en-US", "English"),
        };
        _selectedCulture = Cultures[0];

        GlobalProperties = BuildGlobalProperties();

        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(StatusText));

        // Dock 工作区（导航树 / 属性 / 日志为可停靠工具面板，文档区为 DocumentDock）。
        BuildLayout();

        // 启动即打开欢迎文档，避免空文档区。
        OpenItem(Groups[0].Items[0]);
    }

    /// <summary>Ribbon 分组（左树镜像同一集合）。</summary>
    public ObservableCollection<DemoGroupViewModel> Groups { get; }

    /// <summary>已打开的演示文档。</summary>
    public ObservableCollection<DemoDocumentViewModel> Documents { get; }

    /// <summary>Dock 工作区布局（DockControl 绑定目标；重置布局时整体重建）。</summary>
    public RootDock? Layout => _layout;

    /// <summary>导航树当前选中叶子（TwoWay；程序侧同步时经 OpenItem 打开文档）。</summary>
    public DemoItemViewModel? SelectedNavItem
    {
        get => _selectedNavItem;
        set
        {
            if (Equals(_selectedNavItem, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedNavItem, value);
            if (value is not null)
            {
                OpenItem(value);
            }
        }
    }

    /// <summary>状态栏文本（打开/关闭文档、切换语言时刷新）。</summary>
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    /// <summary>Ribbon 当前页签（命令区随之切换；导航树点击时同步高亮）。</summary>
    public DemoGroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set => this.RaiseAndSetIfChanged(ref _selectedGroup, value);
    }

    /// <summary>当前活动文档；为空时右侧面板展示全局外观/语言属性（PropertyGrid 自我演示）。</summary>
    public DemoDocumentViewModel? ActiveDocument
    {
        get => _activeDocument;
        set
        {
            if (Equals(_activeDocument, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _activeDocument, value);
            this.RaisePropertyChanged(nameof(ActivePropertyItems));
            this.RaisePropertyChanged(nameof(ActiveValueCommittedCommand));
            this.RaisePropertyChanged(nameof(ActiveBrowseRequestedCommand));
        }
    }

    public IReadOnlyList<PropertyItem> ActivePropertyItems =>
        ActiveDocument?.Page.Properties ?? GlobalProperties;

    public ICommand ActiveValueCommittedCommand =>
        ActiveDocument?.ValueCommittedCommand ?? GlobalValueCommittedCommand;

    public ICommand ActiveBrowseRequestedCommand =>
        ActiveDocument?.BrowseRequestedCommand ?? GlobalBrowseRequestedCommand;

    /// <summary>无文档打开时的全局设置属性（外观 / 语言），由 PropertyGrid 直接驱动 Shell。</summary>
    public IReadOnlyList<PropertyItem> GlobalProperties { get; }

    public ReactiveCommand<PropertyValueChange, RxVoid> GlobalValueCommittedCommand { get; private set; } = null!;

    public ReactiveCommand<PropertyItem, RxVoid> GlobalBrowseRequestedCommand { get; private set; } = null!;

    public ReactiveCommand<DemoItemViewModel, RxVoid> OpenItemCommand { get; }

    public ReactiveCommand<DemoDocumentViewModel, RxVoid> CloseDocumentCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> ToggleRibbonCommand { get; }

    /// <summary>面板显隐切换（参数 nav / props / log）。</summary>
    public ReactiveCommand<string, RxVoid> TogglePanelCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickSaveCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickResetCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> QuickSettingsCommand { get; }

    /// <summary>标签栏的引导入口：切换到「应用引导」页并说明用法。</summary>
    public ReactiveCommand<RxVoid, RxVoid> QuickGuideCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> WriteLogsCommand { get; }

    /// <summary>Ribbon 命令区折叠状态（双击页签切换）。</summary>
    public bool RibbonExpanded
    {
        get => _ribbonExpanded;
        set => this.RaiseAndSetIfChanged(ref _ribbonExpanded, value);
    }

    public bool LogPanelVisible
    {
        get => _logPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _logPanelVisible, value);
    }

    public bool PropertiesPanelVisible
    {
        get => _propertiesPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _propertiesPanelVisible, value);
    }

    /// <summary>日志数据源：Logger 的 UI 事件流。</summary>
    public LogEventFeed LogSource => Logger.Events;

    public string LogFolder => DemoBootstrapper.LogFolder;

    public ObservableCollection<AppearanceOption> Palettes { get; }

    public AppearanceOption SelectedPalette
    {
        get => _selectedPalette;
        set
        {
            if (Equals(_selectedPalette, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedPalette, value);
            ApplyAppearance();
        }
    }

    /// <summary>浅/深模式；对固定配色的色板（水生等）无效果，切回默认色板时生效。</summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (Equals(_isDarkMode, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _isDarkMode, value);
            ApplyAppearance();
        }
    }

    public ObservableCollection<CultureOption> Cultures { get; }

    public CultureOption SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (Equals(_selectedCulture, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _selectedCulture, value);
            I18nManager.Instance.Culture = new CultureInfo(value.Name);
            SyncToolTitles();
            Logger.Info("界面语言切换为 " + value.Name);
        }
    }

    /// <summary>单击导航叶子或 Ribbon 命令：打开（或激活）对应演示文档。</summary>
    public void OpenItem(DemoItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var existing = Documents.FirstOrDefault(d => d.TitleKey == item.TitleKey);
        if (existing is not null)
        {
            ActivateDocument(existing);
            return;
        }

        var page = item.PageFactory();
        var document = new DemoDocumentViewModel(item, page);
        var dockDocument = _dockFactory?.CreateDocument() ?? new Document();
        dockDocument.Id = "Doc." + item.TitleKey;
        dockDocument.Title = item.Title;
        dockDocument.Context = new Views.DemoDocumentView { DataContext = document };
        document.DockDocument = dockDocument;

        Documents.Add(document);
        if (_docDock is not null)
        {
            _docDock.VisibleDockables ??= _dockFactory?.CreateList<IDockable>() ?? new AvaloniaList<IDockable>();
            _docDock.VisibleDockables.Add(dockDocument);
            _docDock.ActiveDockable = dockDocument;
        }

        ActiveDocument = document;
        page.OnDocumentOpened();
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Contains(item));

        // 同步导航树高亮（直接改字段，避免 TwoWay 回环再次触发 OpenItem）。
        if (!Equals(_selectedNavItem, item))
        {
            _selectedNavItem = item;
            this.RaisePropertyChanged(nameof(SelectedNavItem));
        }

        StatusText = I18nManager.Instance.GetResource(DemoKeys.ShellReady);
        Logger.Info("打开文档：" + item.Title);
    }

    public void CloseDocument(DemoDocumentViewModel document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_dockFactory is not null && document.DockDocument is not null)
        {
            // 经工厂关闭：DemoDockFactory.CloseDockable 回调 SyncDocumentClosed 保持状态一致。
            _dockFactory.CloseDockable(document.DockDocument);
        }
        else
        {
            SyncDocumentClosed(document);
        }
    }

    /// <summary>激活已打开文档（Dock 页签点击 / 重复打开命令）。</summary>
    private void ActivateDocument(DemoDocumentViewModel document)
    {
        if (_docDock is not null && document.DockDocument is not null)
        {
            _docDock.ActiveDockable = document.DockDocument;
        }

        ActiveDocument = document;
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Any(i => i.TitleKey == document.TitleKey));
    }

    /// <summary>Dock 侧关闭（页签 ✕）后的状态同步入口，仅由 <see cref="DemoDockFactory"/> 回调。</summary>
    private void SyncDocumentClosed(DemoDocumentViewModel document)
    {
        Documents.Remove(document);
        if (ActiveDocument == document)
        {
            ActiveDocument = Documents.LastOrDefault();
        }

        StatusText = I18nManager.Instance.GetResource(DemoKeys.ShellReady);
        Logger.Info("关闭文档：" + document.Title);
    }

    /// <summary>面板显隐切换：隐藏 = 移入 RootDock 隐藏集合，恢复 = 按 Id 取回。</summary>
    public void TogglePanel(string panel)
    {
        if (_dockFactory is null)
        {
            return;
        }

        switch (panel)
        {
            case "nav":
                ToggleTool(_navTool, I18nManager.Instance.GetResource(DemoKeys.ToolNavigation));
                break;
            case "props":
                ToggleTool(_propTool, I18nManager.Instance.GetResource(DemoKeys.ToolProperties));
                break;
            case "log":
                ToggleTool(_logTool, I18nManager.Instance.GetResource(DemoKeys.ToolLog));
                break;
        }
    }

    private void ToggleTool(Tool? tool, string title)
    {
        if (tool is null || _dockFactory is null || _layout is null)
        {
            return;
        }

        var hidden = _layout.HiddenDockables?.Contains(tool) == true;
        if (hidden)
        {
            _dockFactory.RestoreDockable(tool.Id);
            Logger.Info("显示面板：" + title);
        }
        else
        {
            _dockFactory.HideDockable(tool);
            Logger.Info("隐藏面板：" + title);
        }
    }

    /// <summary>重置工作区：整体重建 Dock 布局并回到欢迎文档。</summary>
    public void ResetLayout()
    {
        Documents.Clear();
        ActiveDocument = null;
        _selectedNavItem = null;
        BuildLayout();
        OpenItem(Groups[0].Items[0]);
        Logger.Info("重置工作区布局");
    }

    /// <summary>
    /// 构建 Dock 工作区：左导航树 / 右属性面板 / 下日志为 ToolDock，
    /// 中央为 DocumentDock（演示文档页签），比例容器组合后挂到 RootDock。
    /// </summary>
    private void BuildLayout()
    {
        _dockFactory = new DemoDockFactory(this);

        // 12.x 内容承载：Context 直接放控件实例（Control 无需模板解析，面板/浮动均直接渲染）。
        _navTool = new Tool
        {
            Id = "Tool.Nav",
            Title = T(DemoKeys.ToolNavigation),
            Context = new Views.Tools.NavToolView { DataContext = this },
        };
        _propTool = new Tool
        {
            Id = "Tool.Props",
            Title = T(DemoKeys.ToolProperties),
            Context = new Views.Tools.PropertyToolView { DataContext = this },
        };
        _logTool = new Tool
        {
            Id = "Tool.Log",
            Title = T(DemoKeys.ToolLog),
            Context = new Views.Tools.LogToolView { DataContext = this },
        };
        var navDock = new ToolDock
        {
            Id = "Dock.Nav",
            ActiveDockable = _navTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_navTool),
            Dock = DockMode.Left,
            Proportion = 0.18,
        };
        var propDock = new ToolDock
        {
            Id = "Dock.Props",
            ActiveDockable = _propTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_propTool),
            Dock = DockMode.Right,
            Proportion = 0.2,
        };
        var logDock = new ToolDock
        {
            Id = "Dock.Log",
            ActiveDockable = _logTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_logTool),
            Dock = DockMode.Bottom,
            Proportion = 0.24,
        };

        _docDock = new DocumentDock
        {
            Id = "Dock.Docs",
            VisibleDockables = _dockFactory.CreateList<IDockable>(),
            CanCreateDocument = false,
            Proportion = 0.76,
            IsCollapsable = false,
        };
        _docDock.PropertyChanged += OnDocDockPropertyChanged;

        var centerVertical = new ProportionalDock
        {
            Id = "Dock.Center",
            Orientation = global::Dock.Model.Core.Orientation.Vertical,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                _docDock,
                new ProportionalDockSplitter(),
                logDock),
            ActiveDockable = _docDock,
        };
        var mainHorizontal = new ProportionalDock
        {
            Id = "Dock.Main",
            Orientation = global::Dock.Model.Core.Orientation.Horizontal,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                navDock,
                new ProportionalDockSplitter(),
                centerVertical,
                new ProportionalDockSplitter(),
                propDock),
            ActiveDockable = centerVertical,
        };

        _layout = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = _dockFactory.CreateList<IDockable>(mainHorizontal),
            ActiveDockable = mainHorizontal,
            IsCollapsable = false,
        };
        _dockFactory.InitLayout(_layout);
        this.RaisePropertyChanged(nameof(Layout));
    }

    private void OnDocDockPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDock.ActiveDockable) && _docDock is not null)
        {
            ActiveDocument = (_docDock.ActiveDockable as Document)?.Context as DemoDocumentViewModel;
        }
    }

    private void SyncToolTitles()
    {
        if (_navTool is not null) _navTool.Title = T(DemoKeys.ToolNavigation);
        if (_propTool is not null) _propTool.Title = T(DemoKeys.ToolProperties);
        if (_logTool is not null) _logTool.Title = T(DemoKeys.ToolLog);
    }

    private string T(string key) => I18nManager.Instance.GetResource(key);

    /// <summary>接管 Dock 侧文档关闭（页签 ✕），与 Documents/ActiveDocument 双向同步。</summary>
    private sealed class DemoDockFactory(MainViewModel owner) : Factory
    {
        public override void CloseDockable(IDockable dockable)
        {
            base.CloseDockable(dockable);
            if (dockable is Document { Context: Views.DemoDocumentView view }
                && view.DataContext is DemoDocumentViewModel document)
            {
                owner.SyncDocumentClosed(document);
            }
        }
    }

    public void WriteSampleLogs()
    {
        Logger.Info("演示：一条信息日志");
        Logger.Warn("演示：一条警告日志");
        Logger.Error("演示：一条错误日志");
    }

    public async Task<bool> RunSmokeAsync(CancellationToken cancellation)
    {
        // 冒烟：先写三类日志验证日志通道，再按信息架构逐页构建（含场景页步骤切换）。
        DemoBootstrapper.Initialize();
        Logger.Info("冒烟开始");
        WriteSampleLogs();
        var ok = await SmokeWalkCatalogAsync(cancellation);
        Logger.Info(ok ? "冒烟结束：全部页面构建 + 逐色板渲染通过" : "冒烟结束：存在失败页面，详见上方告警");
        await Logger.ShutdownAsync();
        return ok;
    }

    private void ApplyAppearance()
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = SelectedPalette.ResolveVariant(IsDarkMode);
        SyncGlobalItems();
        Logger.Info($"外观切换：{SelectedPalette.Title} / {(IsDarkMode ? "深色" : "浅色")}");
    }

    private IReadOnlyList<PropertyItem> BuildGlobalProperties()
    {
        GlobalValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(OnGlobalPropertyCommitted);
        GlobalBrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        _paletteItem = new PropertyItem
        {
            Id = "shell.palette",
            Name = "色板",
            Kind = PropertyItemKind.Choice,
            Options = Palettes.Select(p => p.Title).ToArray(),
            Value = Palettes[0].Title,
            DefaultValue = Palettes[0].Title,
            Description = "应用配色：默认 / 水生 / 沙漠 / 黄昏 / 夜空 / 高对比。",
        };
        _modeItem = new PropertyItem
        {
            Id = "shell.mode",
            Name = "界面模式",
            Kind = PropertyItemKind.Radio,
            Options = ["浅色", "深色"],
            Value = "浅色",
            DefaultValue = "浅色",
            Description = "浅 / 深模式；固定配色的色板不随该模式变化。",
        };
        _langItem = new PropertyItem
        {
            Id = "shell.lang",
            Name = "语言",
            Kind = PropertyItemKind.Choice,
            Options = Cultures.Select(c => c.DisplayName).ToArray(),
            Value = Cultures[0].DisplayName,
            DefaultValue = Cultures[0].DisplayName,
            Description = "界面语言，切换即时生效（I18nManager.Culture）。",
        };

        var appearance = new PropertyItem
        {
            Id = "shell.appearance",
            Name = "外观",
            Kind = PropertyItemKind.Category,
            Description = "演示台外观设置（这里的修改即右侧 PropertyGrid 的自我演示）。",
            Children = [_paletteItem, _modeItem],
        };
        var language = new PropertyItem
        {
            Id = "shell.language",
            Name = "语言",
            Kind = PropertyItemKind.Category,
            Description = "演示台语言设置。",
            Children = [_langItem],
        };

        foreach (var root in new[] { appearance, language })
        {
            foreach (var node in root.SelfAndDescendants())
            {
                node.RefreshSummary();
            }

            root.RecalculateModifiedDeep();
        }

        return [appearance, language];
    }

    private void OnGlobalPropertyCommitted(PropertyValueChange change)
    {
        var value = change.NewValue?.ToString();
        switch (change.Item.Id)
        {
            case "shell.palette" when value is not null:
                SelectedPalette = Palettes.FirstOrDefault(p => p.Title == value) ?? SelectedPalette;
                break;
            case "shell.mode" when value is not null:
                IsDarkMode = value == "深色";
                break;
            case "shell.lang" when value is not null:
                var culture = Cultures.FirstOrDefault(c => c.DisplayName == value);
                if (culture is not null)
                {
                    SelectedCulture = culture;
                }

                break;
        }
    }

    private void SyncGlobalItems()
    {
        void Sync(PropertyItem? item, object? value)
        {
            if (item is null || Equals(item.Value, value))
            {
                return;
            }

            item.Value = value;
            item.RefreshSummary();
        }

        Sync(_paletteItem, SelectedPalette.Title);
        Sync(_modeItem, IsDarkMode ? "深色" : "浅色");
        Sync(_langItem, SelectedCulture.DisplayName);
    }
}
