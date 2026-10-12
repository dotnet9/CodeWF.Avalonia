namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>Demo 语言资源键（zh-CN / en-US JSON 均按此层级组织）。</summary>
public static class DemoKeys
{
    // Ribbon 分组 = 左树组节点
    public const string GroupHome = "Demo.Group.Home";
    public const string GroupMarkdown = "Demo.Group.Markdown";
    public const string GroupData = "Demo.Group.Data";
    public const string GroupControls = "Demo.Group.Controls";
    public const string GroupLayout = "Demo.Group.Layout";
    public const string GroupInfrastructure = "Demo.Group.Infrastructure";
    public const string GroupViews = "Demo.Group.Views";

    // 演示项
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
}

/// <summary>演示目录图标（Ribbon 命令按钮 / 导航树节点），与设计原型保持一致。</summary>
public static class DemoIcons
{
    private static readonly Dictionary<string, string> ItemMap = new()
    {
        [DemoKeys.ItemWelcome] = "🏠",
        [DemoKeys.ItemMarkdownViewer] = "📄",
        [DemoKeys.ItemMarkdownLite] = "🪶",
        [DemoKeys.ItemMarkdownEditor] = "✍️",
        [DemoKeys.ItemMarkdownLiveEditor] = "⚡",
        [DemoKeys.ItemMarkdownExport] = "📤",
        [DemoKeys.ItemDataGrid] = "📊",
        [DemoKeys.ItemPropertyGrid] = "🧾",
        [DemoKeys.ItemCodeWFWindow] = "🪟",
        [DemoKeys.ItemSearchListBox] = "🔎",
        [DemoKeys.ItemTransfer] = "🔀",
        [DemoKeys.ItemStatus] = "🏷️",
        [DemoKeys.ItemGuide] = "🧭",
        [DemoKeys.ItemAppGuide] = "🧭",
        [DemoKeys.ItemWindowedStackPanel] = "📋",
        [DemoKeys.ItemColumnDisplayEditor] = "📐",
        [DemoKeys.ItemMarkupExtension] = "🧱",
        [DemoKeys.ItemLog] = "📜",
        [DemoKeys.ItemLang] = "🌐",
        [DemoKeys.ItemDock] = "🪟",
        [DemoKeys.ItemTrapezoidTab] = "🔺",
    };

    public static string ResolveItem(string key) => ItemMap.GetValueOrDefault(key, "◆");

    public static string ResolveGroup(string key) => key switch
    {
        DemoKeys.GroupHome => "🏠",
        DemoKeys.GroupMarkdown => "📄",
        DemoKeys.GroupData => "📊",
        DemoKeys.GroupControls => "🧩",
        DemoKeys.GroupLayout => "📐",
        DemoKeys.GroupInfrastructure => "🧰",
        DemoKeys.GroupViews => "👓",
        _ => "◆",
    };
}
