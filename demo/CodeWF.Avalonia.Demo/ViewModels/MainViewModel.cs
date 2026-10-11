using Avalonia;
using Avalonia.Styling;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Logging;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// Shell 视图模型：Ribbon 分组、导航树镜像、文档区、右侧 PropertyGrid 与底部日志的调度中枢。
/// 页面/子 VM 不走容器注册，按演示目录工厂直接构造。
/// </summary>
public class MainViewModel : ReactiveObject
{
    private DemoDocumentViewModel? _activeDocument;
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

    public MainViewModel()
    {
        Groups = DemoCatalog.CreateGroups();
        Documents = new ObservableCollection<DemoDocumentViewModel>();

        OpenItemCommand = ReactiveCommand.Create<DemoItemViewModel>(OpenItem);
        CloseDocumentCommand = ReactiveCommand.Create<DemoDocumentViewModel>(CloseDocument);
        ToggleRibbonCommand = ReactiveCommand.Create(() => RibbonExpanded = !RibbonExpanded);
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

        // 启动即打开欢迎文档，避免空文档区。
        OpenItem(Groups[0].Items[0]);
    }

    /// <summary>Ribbon 分组（左树镜像同一集合）。</summary>
    public ObservableCollection<DemoGroupViewModel> Groups { get; }

    /// <summary>已打开的演示文档。</summary>
    public ObservableCollection<DemoDocumentViewModel> Documents { get; }

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
            if (this.RaiseAndSetIfChanged(ref _activeDocument, value))
            {
                this.RaisePropertyChanged(nameof(ActivePropertyItems));
                this.RaisePropertyChanged(nameof(ActiveValueCommittedCommand));
                this.RaisePropertyChanged(nameof(ActiveBrowseRequestedCommand));
            }
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

    public ReactiveCommand<PropertyValueChange, Unit> GlobalValueCommittedCommand { get; private set; } = null!;

    public ReactiveCommand<PropertyItem, Unit> GlobalBrowseRequestedCommand { get; private set; } = null!;

    public ReactiveCommand<DemoItemViewModel, Unit> OpenItemCommand { get; }

    public ReactiveCommand<DemoDocumentViewModel, Unit> CloseDocumentCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleRibbonCommand { get; }

    public ReactiveCommand<Unit, Unit> WriteLogsCommand { get; }

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
            if (this.RaiseAndSetIfChanged(ref _selectedPalette, value))
            {
                ApplyAppearance();
            }
        }
    }

    /// <summary>浅/深模式；对固定配色的色板（水生等）无效果，切回默认色板时生效。</summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _isDarkMode, value))
            {
                ApplyAppearance();
            }
        }
    }

    public ObservableCollection<CultureOption> Cultures { get; }

    public CultureOption SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (this.RaiseAndSetIfChanged(ref _selectedCulture, value))
            {
                I18nManager.Instance.Culture = new CultureInfo(value.Name);
                Logger.Info("界面语言切换为 " + value.Name);
            }
        }
    }

    /// <summary>单击导航叶子或 Ribbon 命令：打开（或激活）对应演示文档。</summary>
    public void OpenItem(DemoItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var existing = Documents.FirstOrDefault(d => d.TitleKey == item.TitleKey);
        if (existing is not null)
        {
            ActiveDocument = existing;
            return;
        }

        var page = item.PageFactory();
        var document = new DemoDocumentViewModel(item, page);
        Documents.Add(document);
        ActiveDocument = document;
        page.OnDocumentOpened();
        SelectedGroup = Groups.FirstOrDefault(g => g.Items.Contains(item));
    }

    public void CloseDocument(DemoDocumentViewModel document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var index = Documents.IndexOf(document);
        Documents.Remove(document);
        if (ActiveDocument == document)
        {
            ActiveDocument = index > 0 ? Documents[index - 1] : Documents.FirstOrDefault();
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
        // 冒烟：写入三类日志，验证日志渲染数据源与命令通道。
        DemoBootstrapper.Initialize();
        Logger.Info("冒烟开始");
        WriteSampleLogs();
        await Task.Delay(500, cancellation);
        Logger.Info("冒烟结束");
        await Logger.ShutdownAsync();
        return true;
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
