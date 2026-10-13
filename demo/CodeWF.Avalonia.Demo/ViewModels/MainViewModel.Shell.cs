using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia;
using Avalonia.Threading;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Logging;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using ReactiveUI.Primitives;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>通知等级（对应原型 ncard 的 s / w / e / i 四类）。</summary>
public enum DemoNotificationKind
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>一条通知（通知中心的卡片）。</summary>
public sealed class DemoNotification
{
    public DemoNotification(DemoNotificationKind kind, string title, string? body)
    {
        Kind = kind;
        Title = title;
        Body = body ?? string.Empty;
        CreatedAt = DateTimeOffset.Now;
    }

    public DemoNotificationKind Kind { get; }

    public string Title { get; }

    public string Body { get; }

    public DateTimeOffset CreatedAt { get; }

    /// <summary>卡片左侧色带（对应原型 ok / warn / err / info 四色）。</summary>
    public string Accent => Kind switch
    {
        DemoNotificationKind.Success => "#26a346",
        DemoNotificationKind.Warning => "#e8a103",
        DemoNotificationKind.Error => "#e34d4d",
        _ => "#0064fa",
    };

    public string Glyph => Kind switch
    {
        DemoNotificationKind.Success => "✓",
        DemoNotificationKind.Warning => "!",
        DemoNotificationKind.Error => "✕",
        _ => "i",
    };

    public string TimeText => CreatedAt.ToString("HH:mm:ss");
}

/// <summary>界面密度（对应原型 data-density 的 cozy / compact）。</summary>
public enum DemoDensity
{
    Comfort,
    Compact,
}

/// <summary>
/// 演示台外壳的实体化件（与 design 原型对齐）：
/// 通知中心 / 命令面板（Ctrl+K）/ 密度切换 / 布局记忆 / 导航树就地重命名。
/// 状态集中在这个 partial 上，视图只做呈现与输入转发。
/// </summary>
public partial class MainViewModel
{
    private const int MaxNotifications = 4;
    private static readonly TimeSpan NotificationLifetime = TimeSpan.FromSeconds(6);

    private DispatcherTimer? _notificationTimer;

    private DemoDensity _density = DemoDensity.Comfort;
    private bool _commandPaletteOpen;
    private string _commandQuery = string.Empty;
    private int _commandIndex;
    private DemoItemViewModel? _renameTarget;
    private string _renameText = string.Empty;

    // ───────────────────────── 通知中心 ─────────────────────────

    /// <summary>通知中心可见卡片（最多 4 张，超出淘汰最早的）。</summary>
    public ObservableCollection<DemoNotification> Notifications { get; } = [];

    public bool HasNotifications => Notifications.Count > 0;

    public ReactiveCommand<DemoNotification, RxVoid> DismissNotificationCommand { get; private set; } = null!;

    public ReactiveCommand<RxVoid, RxVoid> ClearNotificationsCommand { get; private set; } = null!;

    /// <summary>推入一条通知（同时写日志与状态栏，与原型 notify 一致）。</summary>
    public void Notify(DemoNotificationKind kind, string title, string? body = null)
    {
        var notification = new DemoNotification(kind, title, body);
        Notifications.Add(notification);
        while (Notifications.Count > MaxNotifications)
        {
            Notifications.RemoveAt(0);
        }

        RaiseNotificationState();
        ScheduleNotificationSweep();

        var line = string.IsNullOrWhiteSpace(body) ? title : $"{title}：{body}";
        Logger.Info(line);
        StatusText = line;
    }

    private void DismissNotification(DemoNotification notification)
    {
        if (Notifications.Remove(notification))
        {
            RaiseNotificationState();
        }
    }

    private void ClearNotifications()
    {
        Notifications.Clear();
        RaiseNotificationState();
    }

    /// <summary>到期卡片自动消失（对应原型 6 秒定时）。</summary>
    private void SweepNotifications()
    {
        var expired = Notifications.Where(n => DateTimeOffset.Now - n.CreatedAt >= NotificationLifetime).ToArray();
        if (expired.Length == 0)
        {
            return;
        }

        foreach (var notification in expired)
        {
            Notifications.Remove(notification);
        }

        RaiseNotificationState();
    }

    private void ScheduleNotificationSweep()
    {
        if (_notificationTimer is { IsEnabled: false })
        {
            _notificationTimer.Start();
        }
    }

    private void RaiseNotificationState()
    {
        this.RaisePropertyChanged(nameof(HasNotifications));
        this.RaisePropertyChanged(nameof(NotificationCount));
    }

    // ───────────────────────── 密度切换 ─────────────────────────

    /// <summary>界面密度：紧凑档收窄 Ribbon 命令按钮与树行高（原型的 data-density）。</summary>
    public DemoDensity Density
    {
        get => _density;
        set
        {
            if (Equals(_density, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _density, value);
            this.RaisePropertyChanged(nameof(IsCompactDensity));
            this.RaisePropertyChanged(nameof(DensityLabel));
            SyncGlobalItems();
            Notify(DemoNotificationKind.Info, T(DemoKeys.ShellDensity), DensityLabel);
        }
    }

    public bool IsCompactDensity => Density == DemoDensity.Compact;

    public string DensityLabel => T(Density == DemoDensity.Compact
        ? DemoKeys.ShellDensityCompact
        : DemoKeys.ShellDensityComfort);

    public ReactiveCommand<RxVoid, RxVoid> ToggleDensityCommand { get; private set; } = null!;

    private void ToggleDensity() =>
        Density = Density == DemoDensity.Compact ? DemoDensity.Comfort : DemoDensity.Compact;

    // ───────────────────────── 命令面板（Ctrl+K） ─────────────────────────

    public bool CommandPaletteOpen
    {
        get => _commandPaletteOpen;
        set
        {
            if (Equals(_commandPaletteOpen, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _commandPaletteOpen, value);
            if (value)
            {
                CommandQuery = string.Empty;
                _commandIndex = 0;
            }

            RaiseCommandState();
        }
    }

    public string CommandQuery
    {
        get => _commandQuery;
        set
        {
            if (Equals(_commandQuery, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _commandQuery, value);
            _commandIndex = 0;
            RaiseCommandState();
        }
    }

    /// <summary>过滤后的命令列表（演示文档 + 外壳动作）。</summary>
    public IReadOnlyList<DemoCommandEntry> FilteredCommands => BuildCommandEntries();

    public int CommandIndex => _commandIndex;

    public string CommandPlaceholder => T(DemoKeys.ShellCommandPlaceholder);

    public string CommandHint => T(DemoKeys.ShellCommandHint);

    public string NotificationDismissHint => T(DemoKeys.ShellNotificationDismiss);

    public ReactiveCommand<RxVoid, RxVoid> ToggleCommandPaletteCommand { get; private set; } = null!;

    public ReactiveCommand<DemoCommandEntry, RxVoid> RunCommandEntryCommand { get; private set; } = null!;

    /// <summary>↑↓ 在命令列表里移动选择（原型 cpmMove）。</summary>
    public void MoveCommandSelection(int delta)
    {
        var count = FilteredCommands.Count;
        if (count == 0)
        {
            return;
        }

        _commandIndex = ((_commandIndex + delta) % count + count) % count;
        this.RaisePropertyChanged(nameof(CommandIndex));
    }

    /// <summary>Enter：执行当前选中命令。</summary>
    public void RunSelectedCommand()
    {
        var entries = FilteredCommands;
        if (entries.Count == 0)
        {
            return;
        }

        RunCommandEntry(entries[Math.Clamp(_commandIndex, 0, entries.Count - 1)]);
    }

    /// <summary>Esc：先关命令面板，否则清空通知中心。</summary>
    public void EscapeShellOverlay()
    {
        if (CommandPaletteOpen)
        {
            CommandPaletteOpen = false;
            return;
        }

        if (HasNotifications)
        {
            ClearNotifications();
        }
    }

    private void RunCommandEntry(DemoCommandEntry entry)
    {
        CommandPaletteOpen = false;
        entry.Run();
    }

    private IReadOnlyList<DemoCommandEntry> BuildCommandEntries()
    {
        var entries = new List<DemoCommandEntry>();

        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                entries.Add(new DemoCommandEntry(item.Icon, item.DisplayTitle, group.Title, true, () => OpenItem(item)));
            }
        }

        entries.Add(new DemoCommandEntry("◐", T(DemoKeys.ShellDark), T(DemoKeys.ShellGroup), false,
            () => IsDarkMode = !IsDarkMode));
        entries.Add(new DemoCommandEntry("▦", T(DemoKeys.ShellDensity), T(DemoKeys.ShellGroup), false, ToggleDensity));
        entries.Add(new DemoCommandEntry("▣", T(DemoKeys.ShellQuickSave), T(DemoKeys.ShellGroup), false, SaveLayout));
        entries.Add(new DemoCommandEntry("↺", T(DemoKeys.ShellResetRenames), T(DemoKeys.ShellGroup), false, ResetRenames));
        entries.Add(new DemoCommandEntry("⌂", T(DemoKeys.ShellReset), T(DemoKeys.ShellGroup), false, ResetLayout));

        var query = _commandQuery.Trim();
        if (query.Length == 0)
        {
            return entries;
        }

        return entries
            .Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || e.Group.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private void RaiseCommandState()
    {
        this.RaisePropertyChanged(nameof(FilteredCommands));
        this.RaisePropertyChanged(nameof(CommandIndex));
    }

    // ───────────────────────── 导航树就地重命名 ─────────────────────────

    /// <summary>正在重命名的叶子（null 表示没有）。</summary>
    public DemoItemViewModel? RenameTarget
    {
        get => _renameTarget;
        private set
        {
            if (Equals(_renameTarget, value))
            {
                return;
            }

            var previous = _renameTarget;
            this.RaiseAndSetIfChanged(ref _renameTarget, value);
            if (previous is not null)
            {
                previous.IsRenaming = false;
            }

            if (value is not null)
            {
                RenameText = value.DisplayTitle;
                value.IsRenaming = true;
            }

            this.RaisePropertyChanged(nameof(IsRenaming));
        }
    }

    public bool IsRenaming => RenameTarget is not null;

    public string RenameText
    {
        get => _renameText;
        set => this.RaiseAndSetIfChanged(ref _renameText, value);
    }

    public bool HasRenames => Groups.SelectMany(g => g.Items).Any(i => i.HasCustomTitle);

    public void BeginRename(DemoItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        RenameTarget = item;
    }

    /// <summary>Enter / 失焦提交（空串视为清除自定义名）。</summary>
    public void CommitRename()
    {
        if (RenameTarget is not { } target)
        {
            return;
        }

        var text = RenameText.Trim();
        target.CustomTitle = text.Length == 0 || text == target.Title ? null : text;
        Logger.Info(string.IsNullOrEmpty(target.CustomTitle)
            ? string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.MainViewModelShell.CommitRename001"), target.Title)
            : string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Text.MainViewModelShell.CommitRename002"), target.Title, target.CustomTitle));
        RenameTarget = null;
        this.RaisePropertyChanged(nameof(HasRenames));
        RefreshDocumentTitles();
    }

    /// <summary>Esc 取消。</summary>
    public void CancelRename() => RenameTarget = null;

    /// <summary>一键恢复全部原名（原型 tree.reset）。</summary>
    public void ResetRenames()
    {
        var changed = false;
        foreach (var item in Groups.SelectMany(g => g.Items))
        {
            if (item.CustomTitle is null)
            {
                continue;
            }

            item.CustomTitle = null;
            changed = true;
        }

        RenameTarget = null;
        this.RaisePropertyChanged(nameof(HasRenames));
        if (!changed)
        {
            return;
        }

        RefreshDocumentTitles();
        Notify(DemoNotificationKind.Warning, T(DemoKeys.ShellResetRenames), T(DemoKeys.ShellResetRenamesHint));
    }

    /// <summary>重命名后同步已打开文档与 Dock 页签标题。</summary>
    private void RefreshDocumentTitles()
    {
        foreach (var document in Documents)
        {
            document.RefreshTitle();
            if (document.DockDocument is not null)
            {
                document.DockDocument.Title = document.DisplayTitle;
            }
        }
    }

    // ───────────────────────── 布局记忆（对应原型 localStorage） ─────────────────────────

    /// <summary>布局存档路径（`%LOCALAPPDATA%\CodeWF.Avalonia.Demo\layout.json`）。</summary>
    public static string LayoutPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CodeWF.Avalonia.Demo",
        "layout.json");

    /// <summary>保存布局：色板 / 深浅 / 密度 / 语言 / 面板显隐 / 文档与激活项 / 重命名。</summary>
    public void SaveLayout()
    {
        if (Program.IsSmokeMode)
        {
            return;
        }

        var ok = SaveLayoutTo(LayoutPath);
        Notify(ok ? DemoNotificationKind.Success : DemoNotificationKind.Warning,
            T(ok ? DemoKeys.ShellLayoutSaved : DemoKeys.ShellLayoutSaveFailed),
            LayoutPath);
    }

    /// <summary>启动时恢复上次布局；无存档或读取失败时保持默认界面。</summary>
    public bool LoadLayout() => !Program.IsSmokeMode && Program.CaptureDirectory is null && LoadLayoutFrom(LayoutPath);

    /// <summary>把当前布局写入指定文件（冒烟用临时路径，避免污染用户存档）。</summary>
    public bool SaveLayoutTo(string path)
    {
        var state = new LayoutState
        {
            Palette = SelectedPalette.TitleKey,
            Dark = IsDarkMode,
            Density = Density.ToString(),
            Culture = SelectedCulture.Name,
            NavVisible = IsPanelVisible(_navTool),
            PropsVisible = IsPanelVisible(_propTool),
            LogVisible = IsPanelVisible(_logTool),
            Documents = Documents.Select(d => d.TitleKey).ToArray(),
            ActiveDocument = ActiveDocument?.TitleKey,
            Renames = Groups.SelectMany(g => g.Items)
                .Where(i => i.CustomTitle is not null)
                .ToDictionary(i => i.TitleKey, i => i.CustomTitle!),
        };

        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(state, LayoutJsonContext.Instance.LayoutState));
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(L.Instance.GetResource("Demo.Text.MainViewModelShell.SaveLayoutTo003") + ex.Message);
            return false;
        }
    }

    /// <summary>从指定文件恢复布局。</summary>
    public bool LoadLayoutFrom(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var state = JsonSerializer.Deserialize(File.ReadAllText(path), LayoutJsonContext.Instance.LayoutState);
            if (state is null)
            {
                return false;
            }

            if (state.Palette is not null
                && Palettes.FirstOrDefault(p => p.TitleKey == state.Palette) is { } palette)
            {
                _selectedPalette = palette;
                this.RaisePropertyChanged(nameof(SelectedPalette));
            }

            _isDarkMode = state.Dark;
            this.RaisePropertyChanged(nameof(IsDarkMode));
            ApplyAppearance();

            if (Enum.TryParse<DemoDensity>(state.Density, out var density))
            {
                Density = density;
            }

            if (state.Culture is not null
                && Cultures.FirstOrDefault(c => c.Name == state.Culture) is { } culture)
            {
                _selectedCulture = culture;
                this.RaisePropertyChanged(nameof(SelectedCulture));
                I18nManager.Instance.Culture = new System.Globalization.CultureInfo(culture.Name);
            }

            foreach (var (titleKey, customTitle) in state.Renames ?? new Dictionary<string, string>())
            {
                var item = Groups.SelectMany(g => g.Items).FirstOrDefault(i => i.TitleKey == titleKey);
                if (item is not null)
                {
                    item.CustomTitle = customTitle;
                }
            }

            if (state.Documents is { Length: > 0 })
            {
                foreach (var titleKey in state.Documents)
                {
                    var item = Groups.SelectMany(g => g.Items).FirstOrDefault(i => i.TitleKey == titleKey);
                    if (item is not null)
                    {
                        OpenItem(item);
                    }
                }

                if (Documents.FirstOrDefault(d => d.TitleKey == state.ActiveDocument) is { } active)
                {
                    ActivateDocument(active);
                }
            }

            SetPanelVisible(_navTool, state.NavVisible);
            SetPanelVisible(_propTool, state.PropsVisible);
            SetPanelVisible(_logTool, state.LogVisible);

            this.RaisePropertyChanged(nameof(HasRenames));
            Logger.Info(L.Instance.GetResource("Demo.Text.MainViewModelShell.LoadLayoutFrom004") + path);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(L.Instance.GetResource("Demo.Text.MainViewModelShell.LoadLayoutFrom005") + ex.Message);
            return false;
        }
    }

    private bool IsPanelVisible(Tool? tool) =>
        tool is not null && _layout?.HiddenDockables?.Contains(tool) != true;

    /// <summary>按目标状态设置面板显隐（与 TogglePanel 互补，供布局恢复用）。</summary>
    private void SetPanelVisible(Tool? tool, bool visible)
    {
        if (tool is null || _layout is null || IsPanelVisible(tool) == visible)
        {
            return;
        }

        TogglePanel(tool.Id switch
        {
            "Tool.Nav" => "nav",
            "Tool.Props" => "props",
            _ => "log",
        });
    }


    // ───────────────────────── 初始化 ─────────────────────────

    /// <summary>外壳实体化件的命令与定时器接线（构造函数末尾调用）。</summary>
    private void InitializeShellServices()
    {
        DismissNotificationCommand = ReactiveCommand.Create<DemoNotification>(DismissNotification);
        ClearNotificationsCommand = ReactiveCommand.Create(ClearNotifications);
        ToggleDensityCommand = ReactiveCommand.Create(ToggleDensity);
        ToggleCommandPaletteCommand = ReactiveCommand.Create(() => { CommandPaletteOpen = !CommandPaletteOpen; });
        RunCommandEntryCommand = ReactiveCommand.Create<DemoCommandEntry>(RunCommandEntry);

        if (!Program.IsSmokeMode)
        {
            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _notificationTimer.Tick += (_, _) => SweepNotifications();
        }
    }
}

/// <summary>命令面板条目：图标 + 标题 + 所属分组 + 执行动作。</summary>
public sealed class DemoCommandEntry(string icon, string title, string group, bool document, Action run)
{
    public string Icon { get; } = icon;

    public string Title { get; } = title;

    public string Group { get; } = group;

    /// <summary>是否指向一个演示文档（用于「文档 / 命令」标记）。</summary>
    public bool Document { get; } = document;

    public string KindLabel => Document ? L.Instance.GetResource("Demo.Text.MainViewModelShell.KindLabel006") : L.Instance.GetResource("Demo.Text.MainViewModelShell.KindLabel007");

    public void Run() => run();
}

/// <summary>
/// 布局存档的序列化上下文：源码生成器产出元数据，NativeAOT 下不依赖反射。
/// 中文不转义，存档文件与原型 localStorage 里的一样可读，便于人工排障。
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LayoutState))]
internal partial class LayoutJsonContext : JsonSerializerContext
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>带自定义选项的上下文实例（不转义非 ASCII）。</summary>
    public static LayoutJsonContext Instance { get; } = new(Options);
}

/// <summary>布局存档的持久化模型（色板 / 深浅 / 密度 / 语言 / 面板 / 文档 / 重命名）。</summary>
internal sealed class LayoutState
{
    public string? Palette { get; set; }

    public bool Dark { get; set; }

    public string? Density { get; set; }

    public string? Culture { get; set; }

    public bool NavVisible { get; set; } = true;

    public bool PropsVisible { get; set; } = true;

    public bool LogVisible { get; set; } = true;

    public string[]? Documents { get; set; }

    public string? ActiveDocument { get; set; }

    public Dictionary<string, string>? Renames { get; set; }
}