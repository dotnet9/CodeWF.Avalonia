namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>Demo 语言资源键（zh-CN / en-US JSON 均按此层级组织）。</summary>
public static class DemoKeys
{
    // Ribbon 分组 = 左树组节点（场景优先 + 组件库分区）
    public const string GroupHome = "Demo.Group.Home";
    public const string GroupMarkdown = "Demo.Group.Markdown";
    public const string GroupData = "Demo.Group.Data";
    public const string GroupDocs = "Demo.Group.Docs";
    public const string GroupInput = "Demo.Group.Input";
    public const string GroupPresent = "Demo.Group.Present";
    public const string GroupInfrastructure = "Demo.Group.Infrastructure";

    // 场景页（工作流）
    public const string ItemMarkdownFlow = "Demo.Item.MarkdownFlow";
    public const string ItemDataFlow = "Demo.Item.DataFlow";
    public const string ItemDocsFlow = "Demo.Item.DocsFlow";

    // 组件页
    public const string ItemWelcome = "Demo.Item.Welcome";
    public const string ItemMarkdownViewer = "Demo.Item.MarkdownViewer";
    public const string ItemMarkdownLite = "Demo.Item.MarkdownLite";
    public const string ItemMarkdownEditor = "Demo.Item.MarkdownEditor";
    public const string ItemMarkdownLiveEditor = "Demo.Item.MarkdownLiveEditor";
    public const string ItemMarkdownExport = "Demo.Item.MarkdownExport";
    public const string ItemDataGrid = "Demo.Item.DataGrid";
    public const string ItemPropertyGrid = "Demo.Item.PropertyGrid";
    public const string ItemCodeWFWindow = "Demo.Item.CodeWFWindow";
    public const string ItemSearchListBox = "Demo.Item.SearchListBox";
    public const string ItemTransfer = "Demo.Item.Transfer";
    public const string ItemStatus = "Demo.Item.Status";
    public const string ItemGuide = "Demo.Item.Guide";
    public const string ItemAppGuide = "Demo.Item.AppGuide";
    public const string ItemWindowedStackPanel = "Demo.Item.WindowedStackPanel";
    public const string ItemColumnDisplayEditor = "Demo.Item.ColumnDisplayEditor";
    public const string ItemMarkupExtension = "Demo.Item.MarkupExtension";
    public const string ItemLog = "Demo.Item.Log";
    public const string ItemLang = "Demo.Item.Lang";
    public const string ItemDock = "Demo.Item.Dock";
    public const string ItemTrapezoidTab = "Demo.Item.TrapezoidTab";

    // 外壳
    public const string ShellReady = "Demo.Shell.Ready";
    public const string ToolNavigation = "Demo.Tool.Navigation";
    public const string ToolProperties = "Demo.Tool.Properties";
    public const string ToolLog = "Demo.Tool.Log";

    // 外壳实体化件：通知中心 / 命令面板 / 密度 / 布局记忆 / 就地重命名
    public const string ShellGroup = "Demo.Shell.Group";
    public const string ShellDark = "Demo.Shell.Dark";
    public const string ShellQuickSave = "Demo.Shell.QuickSave";
    public const string ShellReset = "Demo.Shell.Reset";
    public const string ShellCommand = "Demo.Shell.Command";
    public const string ShellCommandPlaceholder = "Demo.Shell.CommandPlaceholder";
    public const string ShellCommandHint = "Demo.Shell.CommandHint";
    public const string ShellCommandEmpty = "Demo.Shell.CommandEmpty";
    public const string ShellDensity = "Demo.Shell.Density";
    public const string ShellDensityLabel = "Demo.Shell.DensityLabel";
    public const string ShellDensityCompact = "Demo.Shell.DensityCompact";
    public const string ShellDensityComfort = "Demo.Shell.DensityComfort";
    public const string ShellLayoutSaved = "Demo.Shell.LayoutSaved";
    public const string ShellLayoutSaveFailed = "Demo.Shell.LayoutSaveFailed";
    public const string ShellLayoutLoaded = "Demo.Shell.LayoutLoaded";
    public const string ShellResetRenames = "Demo.Shell.ResetRenames";
    public const string ShellResetRenamesHint = "Demo.Shell.ResetRenamesHint";
    public const string ShellRenameHint = "Demo.Shell.RenameHint";
    public const string ShellNotificationDismiss = "Demo.Shell.NotificationDismiss";
    public const string ShellNotifyDemo = "Demo.Shell.NotifyDemo";
    public const string ShellNotifyDemoHint = "Demo.Shell.NotifyDemoHint";
    public const string ShellNavFilter = "Demo.Shell.NavFilter";
    public const string ShellNavEmpty = "Demo.Shell.NavEmpty";
}

/// <summary>
/// 演示目录图标（Ribbon 命令按钮 / 导航树节点），与设计原型保持一致。
/// 图标为几何符号；实现期若要换成矢量图标，可整体替换为 Semi / Ursa / Iconica 的图标控件。
/// </summary>
public static class DemoIcons
{
    private static readonly Dictionary<string, string> ItemMap = new()
    {
        [DemoKeys.ItemWelcome] = "⌂",
        [DemoKeys.ItemMarkdownFlow] = "▶",
        [DemoKeys.ItemDataFlow] = "▤",
        [DemoKeys.ItemDocsFlow] = "❐",
        [DemoKeys.ItemMarkdownViewer] = "▤",
        [DemoKeys.ItemMarkdownLite] = "▭",
        [DemoKeys.ItemMarkdownEditor] = "✎",
        [DemoKeys.ItemMarkdownLiveEditor] = "⚡",
        [DemoKeys.ItemMarkdownExport] = "⇪",
        [DemoKeys.ItemDataGrid] = "▦",
        [DemoKeys.ItemPropertyGrid] = "☰",
        [DemoKeys.ItemCodeWFWindow] = "▣",
        [DemoKeys.ItemSearchListBox] = "⌕",
        [DemoKeys.ItemTransfer] = "⇄",
        [DemoKeys.ItemStatus] = "◈",
        [DemoKeys.ItemGuide] = "◉",
        [DemoKeys.ItemAppGuide] = "◎",
        [DemoKeys.ItemWindowedStackPanel] = "▥",
        [DemoKeys.ItemColumnDisplayEditor] = "▥",
        [DemoKeys.ItemMarkupExtension] = "⟨⟩",
        [DemoKeys.ItemLog] = "☰",
        [DemoKeys.ItemLang] = "⊕",
        [DemoKeys.ItemDock] = "▣",
        [DemoKeys.ItemTrapezoidTab] = "◤",
    };

    public static string ResolveItem(string key) => ItemMap.GetValueOrDefault(key, "◆");

    public static string ResolveGroup(string key) => key switch
    {
        DemoKeys.GroupHome => "⌂",
        DemoKeys.GroupMarkdown => "▤",
        DemoKeys.GroupData => "▦",
        DemoKeys.GroupDocs => "❐",
        DemoKeys.GroupInput => "⌕",
        DemoKeys.GroupPresent => "◈",
        DemoKeys.GroupInfrastructure => "☰",
        _ => "◆",
    };
}
