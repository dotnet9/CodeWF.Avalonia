using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Toolkit.Lang.I18nManager;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Toolkit.Lang;
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
public partial class MainViewModel : ReactiveObject, IDisposable
{
    private DemoDocumentViewModel? _activeDocument;
    private DemoItemViewModel? _selectedNavItem;
    private string _statusText = string.Empty;
    private bool _ribbonExpanded = true;
    private bool _logPanelVisible = true;
    private AppearanceOption _selectedPalette;
    private StudioThemeMode _themeMode = StudioThemeMode.System;
    private StudioThemeController? _themeController;
    private CultureOption _selectedCulture;
    private DemoGroupViewModel? _selectedGroup;
    private PropertyItem? _densityItem;
    private PropertyItem? _paletteItem;
    private PropertyItem? _modeItem;
    private PropertyItem? _langItem;
    private DemoDockFactory? _dockFactory;
    private RootDock? _layout;
    private Tool? _navTool;
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
        QuickSaveCommand = ReactiveCommand.Create(SaveLayout);
        QuickResetCommand = ReactiveCommand.Create(ResetLayout);
        QuickSettingsCommand = ReactiveCommand.Create(() => OpenItem(Groups[0].Items[0]));
        QuickGuideCommand = ReactiveCommand.Create(OpenGuidanceDemo);
        WriteLogsCommand = ReactiveCommand.Create(WriteSampleLogs);

        Palettes = new ObservableCollection<AppearanceOption>
        {
            AppearanceOption.DotNet(),
            AppearanceOption.Aurora(),
            AppearanceOption.Ember(),
            AppearanceOption.Default(),
            AppearanceOption.Aquatic(),
            AppearanceOption.Desert(),
            AppearanceOption.Dusk(),
            AppearanceOption.NightSky(),
        };
        _selectedPalette = Palettes[0];
        CyclePaletteCommand = ReactiveCommand.Create(() => { SelectedPalette = Palettes[(Palettes.IndexOf(SelectedPalette) + 1) % Palettes.Count]; });

        Cultures = new ObservableCollection<CultureOption>
        {
            new("zh-CN", L.Instance.GetResource(DemoResources.Shell.LanguageChinese)),
            new("en-US", "English"),
        };
        _selectedCulture = Cultures[0];

        GlobalValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(OnGlobalPropertyCommitted);
        GlobalBrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        GlobalProperties = BuildGlobalProperties();
        ApplyAppearance();

        I18nManager.Instance.CultureChanged += OnCultureChanged;

        // Dock 工作区（导航树 / 属性 / 日志为可停靠工具面板，文档区为 DocumentDock）。
        BuildLayout();

        // 启动即打开欢迎文档，避免空文档区。
        OpenItem(Groups[0].Items[0]);

        // 初始化通知、命令、密度与持久化布局。
        InitializeShellServices();
        InitializeShellCommands();
        InitializeNavServices();
        if (LoadLayout())
        {
            Notify(DemoNotificationKind.Info, T(DemoKeys.ShellLayoutLoaded), LayoutPath);
        }
    }

    /// <summary>Ribbon 分组（左树镜像同一集合）。</summary>
    public IReadOnlyList<DemoRibbonSection> RibbonSections => SelectedGroup?.RibbonSections ?? [];

    public IReadOnlyList<DemoItemViewModel> RibbonItems => SelectedGroup?.Items.ToArray() ?? [];


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
        set
        {
            if (ReferenceEquals(_selectedGroup, value)) return;
            this.RaiseAndSetIfChanged(ref _selectedGroup, value);
            if (value is not null) value.IsExpanded = true;
            this.RaisePropertyChanged(nameof(RibbonItems));
            this.RaisePropertyChanged(nameof(RibbonSections));
        }
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
            foreach (var group in Groups)
            {
                foreach (var item in group.Items)
                    item.IsSelected = item.TitleKey == value?.TitleKey;
            }
            var selected = Groups.SelectMany(group => group.Items).FirstOrDefault(item => item.IsSelected);
            _selectedNavItem = selected;
            this.RaisePropertyChanged(nameof(SelectedNavItem));
            if (selected is not null)
                SelectedGroup = Groups.First(group => group.Items.Contains(selected));
            this.RaisePropertyChanged(nameof(ActivePropertyItems));
            this.RaisePropertyChanged(nameof(ActiveValueCommittedCommand));
            this.RaisePropertyChanged(nameof(ActiveBrowseRequestedCommand));
        }
    }

    private bool UsesGlobalProperties => ActiveDocument?.Page is Views.Pages.WelcomePage
        || ActiveDocument?.Page.Properties.Count is not > 0;

    public IReadOnlyList<PropertyItem> ActivePropertyItems =>
        UsesGlobalProperties ? GlobalProperties : ActiveDocument?.Page.Properties ?? GlobalProperties;

    public ICommand ActiveValueCommittedCommand =>
        UsesGlobalProperties ? GlobalValueCommittedCommand : ActiveDocument?.ValueCommittedCommand ?? GlobalValueCommittedCommand;

    public ICommand ActiveBrowseRequestedCommand =>
        UsesGlobalProperties ? GlobalBrowseRequestedCommand : ActiveDocument?.BrowseRequestedCommand ?? GlobalBrowseRequestedCommand;

    /// <summary>无文档打开时的全局设置属性（外观 / 语言），由 PropertyGrid 直接驱动 Shell。</summary>
    public IReadOnlyList<PropertyItem> GlobalProperties { get; private set; }

    public ReactiveCommand<PropertyValueChange, RxVoid> GlobalValueCommittedCommand { get; }

    public ReactiveCommand<PropertyItem, RxVoid> GlobalBrowseRequestedCommand { get; }

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
        get => ActiveDocument?.PropertiesVisible ?? true;
        set { if (ActiveDocument is { } document) document.PropertiesVisible = value; }
    }

    /// <summary>日志数据源：Logger 的 UI 事件流。</summary>
    public LogEventFeed LogSource => Logger.Events;

    public string LogFolder => DemoBootstrapper.LogFolder;

    public ReactiveCommand<RxVoid, RxVoid> CyclePaletteCommand { get; }

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

    public IReadOnlyList<ThemeModeOption> ThemeModeOptions { get; } =
    [
        new(DemoResources.Shell.GlobalSettings.SystemMode),
        new(DemoResources.Shell.GlobalSettings.LightMode),
        new(DemoResources.Shell.GlobalSettings.DarkMode),
    ];

    /// <summary>标题栏与属性面板共用的三态主题模式。</summary>
    public IReadOnlyList<string> ThemeModes =>
    [
        T(DemoResources.Shell.GlobalSettings.SystemMode),
        T(DemoResources.Shell.GlobalSettings.LightMode),
        T(DemoResources.Shell.GlobalSettings.DarkMode),
    ];

    public int ThemeModeIndex
    {
        get => (int)_themeMode;
        set
        {
            if (value is < 0 or > 2 || value == (int)_themeMode) return;
            _themeMode = (StudioThemeMode)value;
            this.RaisePropertyChanged();
            ApplyAppearance();
        }
    }

    /// <summary>快捷键与回归脚本使用的显式浅深切换，退出跟随系统模式。</summary>
    public bool IsDarkMode
    {
        get => Application.Current is { } app
            ? app.ActualThemeVariant == SelectedPalette.ResolveVariant(true)
            : _themeMode == StudioThemeMode.Dark;
        set => ThemeModeIndex = (int)(value ? StudioThemeMode.Dark : StudioThemeMode.Light);
    }

    public ObservableCollection<CultureOption> Cultures { get; }

    public void SelectCulture(string name)
    {
        if (Cultures.FirstOrDefault(culture => culture.Name == name) is { } culture) SelectedCulture = culture;
    }

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
            this.RaisePropertyChanged(nameof(RibbonSections));
            SyncToolTitles();
            foreach (var palette in Palettes) palette.RefreshTitle();
            foreach (var mode in ThemeModeOptions) mode.RefreshTitle();
            this.RaisePropertyChanged(nameof(ThemeModes));
            this.RaisePropertyChanged(nameof(ThemeModeIndex));
            GlobalProperties = BuildGlobalProperties();
            SyncGlobalItems();
            foreach (var document in Documents)
            {
                document.RefreshProperties();
                document.RefreshTitle();
                if (document.Page is Views.Pages.WelcomePage overview) overview.Overview.RefreshCulture();
            }
            this.RaisePropertyChanged(nameof(ActivePropertyItems));
            this.RaisePropertyChanged(nameof(NavFilterPlaceholder));
            this.RaisePropertyChanged(nameof(NavEmptyHint));
            this.RaisePropertyChanged(nameof(RenameHint));
            this.RaisePropertyChanged(nameof(ResetRenamesHint));
            this.RaisePropertyChanged(nameof(DensityLabel));
            this.RaisePropertyChanged(nameof(CommandPlaceholder));
            this.RaisePropertyChanged(nameof(CommandHint));
            this.RaisePropertyChanged(nameof(NotificationDismissHint));
            RaiseCommandState();
            StatusText = I18nManager.Instance.GetResource(DemoResources.Shell.Ready);
            Logger.Info(L.Instance.GetResource(DemoResources.Shell.LanguageChanged) + value.Name);
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
        if (page is Views.Pages.WelcomePage overview)
        {
            overview.Navigate = key =>
            {
                var destination = Groups.SelectMany(g => g.Items).FirstOrDefault(i => i.TitleKey == key);
                if (destination is not null) OpenItem(destination);
            };
            overview.TogglePanel = TogglePanel;
        }
        var document = new DemoDocumentViewModel(item, page, this);
        var dockDocument = _dockFactory?.CreateDocument() ?? new Document();
        dockDocument.Id = "Doc." + item.TitleKey;
        dockDocument.Title = document.DisplayTitle;
        dockDocument.Context = new Views.DemoDocumentView { DataContext = document };
        document.DockDocument = dockDocument;

        Documents.Add(document);
        if (_docDock is not null && _dockFactory is not null)
        {
            _dockFactory.AddDockable(_docDock, dockDocument);
            _dockFactory.SetActiveDockable(dockDocument);
            _dockFactory.SetFocusedDockable(_docDock, dockDocument);
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
        Logger.Info(L.Instance.GetResource(DemoResources.Shell.DocumentOpened) + item.Title);
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
        if (_dockFactory is not null && document.DockDocument is { } dockDocument)
        {
            _dockFactory.SetActiveDockable(dockDocument);
            if (dockDocument.Owner is IDock owner)
                _dockFactory.SetFocusedDockable(owner, dockDocument);
        }

        ActiveDocument = document;
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Any(i => i.TitleKey == document.TitleKey));
    }

    /// <summary>Dock 侧关闭（页签 ✕）后的状态同步入口，仅由 <see cref="DemoDockFactory"/> 回调。</summary>
    private void SyncDocumentClosed(DemoDocumentViewModel document)
    {
        document.Dispose();
        Documents.Remove(document);
        if (ActiveDocument == document)
        {
            ActiveDocument = Documents.LastOrDefault();
        }

        StatusText = I18nManager.Instance.GetResource(DemoKeys.ShellReady);
        Logger.Info(L.Instance.GetResource(DemoResources.Shell.DocumentClosed) + document.Title);
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
                if (ActiveDocument is { } document) document.PropertiesVisible = !document.PropertiesVisible;
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
            Logger.Info(L.Instance.GetResource(DemoResources.Shell.PanelShown) + title);
        }
        else
        {
            _dockFactory.HideDockable(tool);
            Logger.Info(L.Instance.GetResource(DemoResources.Shell.PanelHidden) + title);
        }
    }

    /// <summary>重置工作区：整体重建 Dock 布局并回到欢迎文档。</summary>
    public void ResetLayout()
    {
        foreach (var document in Documents) document.Dispose();
        Documents.Clear();
        ActiveDocument = null;
        _selectedNavItem = null;
        BuildLayout();
        OpenItem(Groups[0].Items[0]);
        Logger.Info(L.Instance.GetResource(DemoResources.Shell.LayoutReset));
    }

    /// <summary>
    /// 构建 Dock 工作区：左导航树 / 下日志为 ToolDock，属性面板属于各自文档，
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
            Proportion = 0.16,
        };
        var logDock = new ToolDock
        {
            Id = "Dock.Log",
            MinHeight = 84,
            ActiveDockable = _logTool,
            VisibleDockables = _dockFactory.CreateList<IDockable>(_logTool),
            Dock = DockMode.Bottom,
            Proportion = 0.14,
        };

        _docDock = new DocumentDock
        {
            Id = "Dock.Docs",
            VisibleDockables = _dockFactory.CreateList<IDockable>(),
            CanCreateDocument = false,
            Proportion = 0.84,
            IsCollapsable = false,
        };
        _docDock.PropertyChanged += OnDocDockPropertyChanged;

        var mainHorizontal = new ProportionalDock
        {
            Id = "Dock.Main",
            Orientation = global::Dock.Model.Core.Orientation.Horizontal,
            Proportion = 0.86,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                navDock,
                new ProportionalDockSplitter(),
                _docDock),
            ActiveDockable = _docDock,
        };
        var workspace = new ProportionalDock
        {
            Id = "Dock.Workspace",
            Orientation = global::Dock.Model.Core.Orientation.Vertical,
            VisibleDockables = _dockFactory.CreateList<IDockable>(
                mainHorizontal,
                new ProportionalDockSplitter(),
                logDock),
            ActiveDockable = mainHorizontal,
        };

        _layout = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = _dockFactory.CreateList<IDockable>(workspace),
            ActiveDockable = workspace,
            IsCollapsable = false,
        };
        _dockFactory.InitLayout(_layout);
        this.RaisePropertyChanged(nameof(Layout));
    }

    private void OnDocDockPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDock.ActiveDockable) && _docDock is not null)
        {
            ActiveDocument = Documents.FirstOrDefault(document => ReferenceEquals(document.DockDocument, _docDock.ActiveDockable));
        }
    }

    private void SyncToolTitles()
    {
        if (_navTool is not null) _navTool.Title = T(DemoKeys.ToolNavigation);
        if (_logTool is not null) _logTool.Title = T(DemoKeys.ToolLog);
    }

    private string T(string key) => I18nManager.Instance.GetResource(key);

    /// <summary>接管 Dock 侧文档关闭（页签 ✕），与 Documents/ActiveDocument 双向同步。</summary>
    private sealed class DemoDockFactory(MainViewModel owner) : Factory
    {
        public override void InitLayout(IDockable layout)
        {
            // 为拖出文档提供原生浮动窗口，内容与文档内属性面板共同承载。
            HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = () => new global::Dock.Avalonia.Controls.HostWindow(),
            };
            base.InitLayout(layout);
        }

        public override void CloseDockable(IDockable dockable)
        {
            base.CloseDockable(dockable);
            if (dockable is Document { Context: Views.DemoDocumentView { DataContext: DemoDocumentViewModel document } })
            {
                owner.SyncDocumentClosed(document);
            }
        }
    }

    public void WriteSampleLogs()
    {
        Logger.Info(L.Instance.GetResource(DemoResources.Shell.SampleInformationLog));
        Logger.Warn(L.Instance.GetResource(DemoResources.Shell.SampleWarningLog));
        Logger.Error(L.Instance.GetResource(DemoResources.Shell.SampleErrorLog));
    }

    public async Task<bool> RunSmokeAsync(CancellationToken cancellation)
    {
        // 冒烟：先写三类日志验证日志通道，再按信息架构逐页构建（含场景页步骤切换）。
        DemoBootstrapper.Initialize();
        Logger.Info(L.Instance.GetResource(DemoResources.Content.Shell.RunSmokeAsync.SmokeStarted));
        WriteSampleLogs();
        var overviewOk = await SmokeOverviewAsync(cancellation);
        var ok = await SmokeWalkCatalogAsync(cancellation);
        Logger.Info(ok ? L.Instance.GetResource(DemoResources.Content.Shell.RunSmokeAsync.SmokePassed) : L.Instance.GetResource(DemoResources.Content.Shell.RunSmokeAsync.SmokeFailed));
        await Logger.ShutdownAsync();
        return ok && overviewOk;
    }

    private void ApplyAppearance()
    {
        if (Application.Current is null)
        {
            return;
        }

        if (_themeController is null)
        {
            _themeController = new StudioThemeController(Application.Current);
            Application.Current.PropertyChanged += OnApplicationPropertyChanged;
        }
        _themeController.Palette = SelectedPalette.Palette;
        _themeController.Mode = _themeMode;
        this.RaisePropertyChanged(nameof(IsDarkMode));
        SyncGlobalItems();
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.Shell.ApplyAppearance.AppearanceFormat), SelectedPalette.Title, ThemeModes[ThemeModeIndex]));
    }

    private IReadOnlyList<PropertyItem> BuildGlobalProperties()
    {
        _densityItem = new PropertyItem
        {
            Id = "shell.density",
            Name = T(DemoKeys.ShellDensity),
            Kind = PropertyItemKind.Choice,
            Options = [T(DemoKeys.ShellDensityComfort), T(DemoKeys.ShellDensityCompact)],
            Value = DensityLabel,
            DefaultValue = T(DemoKeys.ShellDensityComfort),
            Description = T(DemoResources.Studio.DensityDescription),
        };
        _paletteItem = new PropertyItem
        {
            Id = "shell.palette",
            Name = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.PaletteLabel),
            Kind = PropertyItemKind.Choice,
            Options = Palettes.Select(p => p.Title).ToArray(),
            Value = Palettes[0].Title,
            DefaultValue = Palettes[0].Title,
            Description = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.PaletteDescription),
        };
        _modeItem = new PropertyItem
        {
            Id = "shell.mode",
            Name = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.ModeLabel),
            Kind = PropertyItemKind.Choice,
            Options = ThemeModes,
            Value = ThemeModes[ThemeModeIndex],
            DefaultValue = ThemeModes[0],
            Description = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.ModeDescription),
        };
        _langItem = new PropertyItem
        {
            Id = "shell.lang",
            Name = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.LanguageLabel),
            Kind = PropertyItemKind.Choice,
            Options = Cultures.Select(c => c.DisplayName).ToArray(),
            Value = Cultures[0].DisplayName,
            DefaultValue = Cultures[0].DisplayName,
            Description = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.LanguageDescription),
        };

        var appearance = new PropertyItem
        {
            Id = "shell.appearance",
            Name = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.AppearanceGroup),
            Kind = PropertyItemKind.Category,
            Description = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.AppearanceDescription),
            Children = [_densityItem, _paletteItem, _modeItem],
        };
        var language = new PropertyItem
        {
            Id = "shell.language",
            Name = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.LanguageGroup),
            Kind = PropertyItemKind.Category,
            Description = L.Instance.GetResource(DemoResources.Shell.GlobalSettings.LanguageGroupDescription),
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
            case "shell.density" when value is not null:
                Density = value == T(DemoKeys.ShellDensityCompact) ? DemoDensity.Compact : DemoDensity.Comfort;
                break;
            case "shell.palette" when value is not null:
                SelectedPalette = Palettes.FirstOrDefault(p => p.Title == value) ?? SelectedPalette;
                break;
            case "shell.mode" when value is not null:
                ThemeModeIndex = Array.IndexOf(ThemeModes.ToArray(), value);
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

        Sync(_densityItem, DensityLabel);
        Sync(_paletteItem, SelectedPalette.Title);
        Sync(_modeItem, ThemeModes[ThemeModeIndex]);
        Sync(_langItem, SelectedCulture.DisplayName);
    }

    private void OnCultureChanged(object? sender, EventArgs args) => this.RaisePropertyChanged(nameof(StatusText));

    private void OnApplicationPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == Application.ActualThemeVariantProperty) this.RaisePropertyChanged(nameof(IsDarkMode));
    }

    public void Dispose()
    {
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        if (Application.Current is { } app) app.PropertyChanged -= OnApplicationPropertyChanged;
        _themeController?.Dispose();
        foreach (var document in Documents) document.Dispose();
    }

}
