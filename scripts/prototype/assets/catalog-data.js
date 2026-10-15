"use strict";
window.CodeWFDemoCatalog = [
  {
    "titleKey": "Demo.Group.Home",
    "titleZh": "首页",
    "titleEn": "Home",
    "id": "home",
    "items": [
      {
        "titleKey": "Demo.Item.Welcome",
        "page": "WelcomePage",
        "api": "",
        "package": "",
        "prototypeId": "welcome",
        "titleZh": "介绍",
        "titleEn": "Introduction",
        "description": "",
        "icon": "⌂",
        "prototypeIcon": "<path d=\"M2 11.45a1 1 0 0 1 .33-.75l9-8.1a1 1 0 0 1 1.34 0l9 8.1a1 1 0 0 1 .33.75V20a2 2 0 0 1-2 2h-4a1 1 0 0 1-1-1v-4a3 3 0 0 0-6 0v4a1 1 0 0 1-1 1H4a2 2 0 0 1-2-2v-8.55Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconHome"
      },
      {
        "titleKey": "Demo.Item.About",
        "page": "AboutPage",
        "api": "",
        "package": "",
        "prototypeId": "about",
        "titleZh": "关于",
        "titleEn": "About",
        "description": "",
        "icon": "ⓘ",
        "prototypeIcon": "<path d=\"M12 23a11 11 0 1 0 0-22 11 11 0 0 0 0 22Zm2-16a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-5 3.75c0-.41.34-.75.75-.75h2.75a1 1 0 0 1 1 1v5.5h.75a.75.75 0 0 1 0 1.5h-4.5a.75.75 0 0 1 0-1.5h.75v-5h-.75a.75.75 0 0 1-.75-.75Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconInfoCircle"
      }
    ],
    "icon": "⌂",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Input",
    "titleZh": "输入与选择",
    "titleEn": "Inputs & Selection",
    "id": "input",
    "items": [
      {
        "titleKey": "Demo.Item.SearchListBox",
        "page": "SearchListBoxPage",
        "api": "SearchListBox",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "searchlist",
        "titleZh": "SearchListBox",
        "titleEn": "SearchListBox",
        "description": "SearchListBox",
        "icon": "⌕",
        "prototypeIcon": "<path d=\"M10.5 2a8.5 8.5 0 1 0 4.86 15.48l4.08 4.08a1.5 1.5 0 1 0 2.12-2.12l-4.08-4.08A8.5 8.5 0 0 0 10.5 2ZM5 10.5a5.5 5.5 0 1 1 11 0 5.5 5.5 0 0 1-11 0Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconSearch"
      },
      {
        "titleKey": "Demo.Item.Transfer",
        "page": "TransferPage",
        "api": "Transfer",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "transfer",
        "titleZh": "Transfer",
        "titleEn": "Transfer",
        "description": "Transfer",
        "icon": "⇄",
        "prototypeIcon": "<path d=\"M1 12c0-.83.67-1.5 1.5-1.5h15.38l-6.44-6.44a1.5 1.5 0 0 1 2.12-2.12l9 9a1.5 1.5 0 0 1 0 2.12l-9 9a1.5 1.5 0 0 1-2.12-2.12l6.44-6.44H2.5A1.5 1.5 0 0 1 1 12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconArrowRight"
      }
    ],
    "icon": "⌕",
    "prototypeIcon": "<rect x=\"3.5\" y=\"4.5\" width=\"17\" height=\"15\" rx=\"1.7\"/><path d=\"M6.5 9.5h11M6.5 13.5h7\"/><circle cx=\"16.5\" cy=\"15.8\" r=\"2.3\"/><path d=\"M18.3 17.6l2.2 2.2\"/>"
  },
  {
    "titleKey": "Demo.Group.Feedback",
    "titleZh": "反馈与引导",
    "titleEn": "Feedback & Guidance",
    "id": "feedback",
    "items": [
      {
        "titleKey": "Demo.Item.StatusBadge",
        "page": "StatusComponentsPage(\"StatusBadge\")",
        "api": "StatusBadge",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "status-badge",
        "titleZh": "StatusBadge",
        "titleEn": "StatusBadge",
        "description": "StatusBadge",
        "sectionKey": "Demo.Catalog.SectionStatuses",
        "icon": "◈",
        "prototypeIcon": "<path d=\"M12 23a11 11 0 1 0 0-22 11 11 0 0 0 0 22Zm5.88-13.18-6.2 7.6a1.5 1.5 0 0 1-2.37 0l-3.5-4a1.5 1.5 0 1 1 2.37-1.84l2.3 2.46L15.5 8a1.5 1.5 0 1 1 2.38 1.82Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconTickCircle"
      },
      {
        "titleKey": "Demo.Item.StatusCard",
        "page": "StatusComponentsPage(\"StatusCard\")",
        "api": "StatusCard",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "status-card",
        "titleZh": "StatusCard",
        "titleEn": "StatusCard",
        "description": "StatusCard",
        "sectionKey": "Demo.Catalog.SectionStatuses",
        "icon": "▣",
        "prototypeIcon": "<path d=\"M4 1a3 3 0 0 0-3 3v16a3 3 0 0 0 3 3h16a3 3 0 0 0 3-3V4a3 3 0 0 0-3-3H4Zm15 3a1 1 0 0 1 1 1v5.5h-6.5V4H19Zm-8.5 0H5a1 1 0 0 0-1 1v5.5h6.5V4ZM4 13.5V19a1 1 0 0 0 1 1h5.5v-6.5H4Zm9.5 6.5H19a1 1 0 0 0 1-1v-5.5h-6.5V20Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridSquare"
      },
      {
        "titleKey": "Demo.Item.Guide",
        "page": "GuidePage",
        "api": "Guide",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "guide",
        "titleZh": "Guide",
        "titleEn": "Guide",
        "description": "Guide",
        "sectionKey": "Demo.Catalog.SectionGuidance",
        "icon": "◉",
        "prototypeIcon": "<path d=\"M12 23a11 11 0 1 0 0-22 11 11 0 0 0 0 22Zm-.17-8.4c-.86 0-1.3-.53-1.3-1.23 0-1.05.53-1.72 1.74-2.64l.06-.05c.83-.62 1.4-1.06 1.4-1.85 0-.9-.83-1.4-1.76-1.4-.76 0-1.34.28-1.8.87-.33.34-.6.53-1.05.53C8.38 8.83 8 8.3 8 7.7c0-.6.34-1.22.88-1.68A5.15 5.15 0 0 1 12.28 5c2.71 0 4.62 1.34 4.62 3.64 0 1.68-1 2.5-2.2 3.31-.8.59-1.17.93-1.48 1.62-.33.6-.58 1.04-1.4 1.04Zm-.02 4.17c-.94 0-1.71-.6-1.71-1.54 0-.94.77-1.55 1.7-1.55.95 0 1.7.6 1.7 1.55 0 .93-.75 1.54-1.7 1.54Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconHelpCircle"
      },
      {
        "titleKey": "Demo.Item.Notifications",
        "page": "ControlApiPage(\"LogNotifications\")",
        "api": "LogNotifications",
        "package": "CodeWF.Avalonia.Log",
        "prototypeId": "notifications",
        "titleZh": "LogNotifications",
        "titleEn": "LogNotifications",
        "description": "LogNotifications",
        "sectionKey": "Demo.Catalog.SectionNotifications",
        "icon": "◉",
        "prototypeIcon": "<path d=\"M18 9a6 6 0 0 0-3-5.2 3 3 0 0 0-6 0A6 6 0 0 0 6 9s0 2-.5 4c-.28 1.13-1.69 2.9-2.86 4.23-.58.67-.12 1.77.77 1.77H20.6c.89 0 1.35-1.1.77-1.77-1.17-1.32-2.58-3.1-2.86-4.23-.5-2-.5-4-.5-4Z\n        M15 20a3 3 0 1 1-6 0h6Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBell"
      }
    ],
    "icon": "◈",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Data",
    "titleZh": "数据展示",
    "titleEn": "Data Display",
    "id": "data",
    "items": [
      {
        "titleKey": "Demo.Item.DataGrid",
        "page": "DataGridPage",
        "api": "DataGridEnhancement",
        "package": "CodeWF.Avalonia.DataGrid",
        "prototypeId": "datagrid",
        "titleZh": "DataGridEnhancement",
        "titleEn": "DataGridEnhancement",
        "description": "DataGridEnhancement",
        "sectionKey": "Demo.Catalog.SectionGrids",
        "icon": "▦",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-5Z\n        M13 15c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2v-5Z\n        M13 4c0-1.1.9-2 2-2h5a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-5a2 2 0 0 1-2-2V4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconGridView"
      },
      {
        "titleKey": "Demo.Item.ColumnDisplayEditor",
        "page": "ColumnDisplayEditorPage",
        "api": "ColumnDisplayEditor",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "colde",
        "titleZh": "ColumnDisplayEditor",
        "titleEn": "ColumnDisplayEditor",
        "description": "ColumnDisplayEditor",
        "sectionKey": "Demo.Catalog.SectionGrids",
        "icon": "▥",
        "prototypeIcon": "<path d=\"M8 5.07c-.8.47-1.8.46-2.6 0l-.24-.15a1.4 1.4 0 0 0-1.82.29c-.64.82-1.16 1.73-1.55 2.7a1.4 1.4 0 0 0 .66 1.71l.24.14a2.57 2.57 0 0 1 0 4.48l-.24.14a1.4 1.4 0 0 0-.66 1.72 11.1 11.1 0 0 0 1.55 2.69 1.4 1.4 0 0 0 1.82.3l.25-.15a2.57 2.57 0 0 1 3.88 2.24v.28c-.01.7.46 1.33 1.16 1.43a11 11 0 0 0 3.1 0 1.4 1.4 0 0 0 1.17-1.43v-.28a2.57 2.57 0 0 1 3.87-2.24l.25.14c.6.36 1.38.26 1.82-.29.64-.82 1.16-1.73 1.55-2.7a1.4 1.4 0 0 0-.66-1.71l-.24-.14a2.57 2.57 0 0 1 0-4.48l.24-.14a1.4 1.4 0 0 0 .66-1.72 11.08 11.08 0 0 0-1.55-2.69 1.4 1.4 0 0 0-1.82-.3l-.25.15a2.57 2.57 0 0 1-3.88-2.24v-.28c.01-.7-.46-1.33-1.16-1.43a10.99 10.99 0 0 0-3.1 0c-.7.1-1.17.72-1.17 1.43v.28C9.3 3.75 8.8 4.61 8 5.07Zm6 10.4a4 4 0 1 0-4-6.93 4 4 0 0 0 4 6.92Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconSetting"
      },
      {
        "titleKey": "Demo.Item.TreeDataGrid",
        "page": "ControlApiPage(\"TreeDataGridExtension\")",
        "api": "TreeDataGridExtension",
        "package": "CodeWF.Avalonia.TreeDataGrid",
        "prototypeId": "treegrid",
        "titleZh": "TreeDataGridExtension",
        "titleEn": "TreeDataGridExtension",
        "description": "TreeDataGridExtension",
        "sectionKey": "Demo.Catalog.SectionGrids",
        "icon": "▦",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      },
      {
        "titleKey": "Demo.Item.PropertyGrid",
        "page": "PropertyGridPage",
        "api": "PropertyGrid",
        "package": "CodeWF.Avalonia.TreeDataGrid",
        "prototypeId": "propgrid",
        "titleZh": "PropertyGrid",
        "titleEn": "PropertyGrid",
        "description": "PropertyGrid",
        "sectionKey": "Demo.Catalog.SectionProperties",
        "icon": "☰",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      },
      {
        "titleKey": "Demo.Item.Log",
        "page": "LogPage",
        "api": "LogView",
        "package": "CodeWF.Avalonia.Log",
        "prototypeId": "log",
        "titleZh": "LogView",
        "titleEn": "LogView",
        "description": "LogView",
        "sectionKey": "Demo.Catalog.SectionLogs",
        "icon": "☰",
        "prototypeIcon": "<path d=\"M2 4c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V4Z\n        M2 11.5c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\n        M2 19c0-1.1.9-2 2-2h16a2 2 0 0 1 2 2v1a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconListView"
      }
    ],
    "icon": "▦",
    "prototypeIcon": "<path d=\"M4 19.5V4.5M4 19.5h15.5\"/><rect x=\"7\" y=\"11\" width=\"3\" height=\"6\"/><rect x=\"12\" y=\"7.5\" width=\"3\" height=\"9.5\"/><rect x=\"17\" y=\"14\" width=\"3\" height=\"3\"/>"
  },
  {
    "titleKey": "Demo.Group.Layout",
    "titleZh": "布局与导航",
    "titleEn": "Layout & Navigation",
    "id": "layout",
    "items": [
      {
        "titleKey": "Demo.Item.CodeWFWindow",
        "page": "CodeWFWindowPage",
        "api": "CodeWFWindow",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "codewfwindow",
        "titleZh": "CodeWFWindow",
        "titleEn": "CodeWFWindow",
        "description": "CodeWFWindow",
        "sectionKey": "Demo.Catalog.SectionWindows",
        "icon": "▣",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      },
      {
        "titleKey": "Demo.Item.TitleBar",
        "page": "ControlApiPage(\"CodeWFTitleBar\")",
        "api": "CodeWFTitleBar",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "titlebar",
        "titleZh": "CodeWFTitleBar",
        "titleEn": "CodeWFTitleBar",
        "description": "CodeWFTitleBar",
        "sectionKey": "Demo.Catalog.SectionWindows",
        "icon": "▤",
        "prototypeIcon": "<path d=\"M5 2a3 3 0 0 0-3 3v14a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3V5a3 3 0 0 0-3-3H5Zm2 4h10a1 1 0 0 1 1 1v2a1 1 0 1 1-2 0V8h-3v8h1a1 1 0 1 1 0 2h-4a1 1 0 1 1 0-2h1V8H8v1a1 1 0 0 1-2 0V7a1 1 0 0 1 1-1Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconTextRectangle"
      },
      {
        "titleKey": "Demo.Item.Dock",
        "page": "DockPage",
        "api": "DockControl",
        "package": "CodeWF.Avalonia.Dock",
        "prototypeId": "dock",
        "titleZh": "DockControl",
        "titleEn": "DockControl",
        "description": "DockControl",
        "sectionKey": "Demo.Catalog.SectionNavigation",
        "icon": "▣",
        "prototypeIcon": "<path d=\"m21.21 15.05.26.14c.51.28.68.9.38 1.39-.1.15-.23.27-.38.36l-8.92 4.92c-.34.19-.76.19-1.1 0l-8.92-4.92a.98.98 0 0 1-.38-1.39c.1-.15.23-.27.38-.36l.26-.14 8.66 4.78c.32.17.72.18 1.05.02l.05-.02 8.66-4.78Zm0-4.07.26.14c.51.29.68.91.38 1.4-.1.14-.23.27-.38.36l-8.92 4.92c-.34.18-.76.18-1.1 0l-8.92-4.92a.98.98 0 0 1-.38-1.4c.1-.14.23-.27.38-.36l.26-.14 8.66 4.78c.32.18.72.19 1.05.03l.05-.03 8.66-4.78Zm-8.66-8.84 8.92 4.92c.51.28.68.9.38 1.39-.1.15-.23.27-.38.36l-8.92 4.92c-.34.19-.76.19-1.1 0L2.53 8.81a.98.98 0 0 1-.38-1.39c.1-.15.23-.27.38-.36l8.92-4.92c.34-.19.76-.19 1.1 0Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconLayers"
      },
      {
        "titleKey": "Demo.Item.TrapezoidTab",
        "page": "TrapezoidTabPage",
        "api": "TrapezoidShapedTabControl",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "trapezoid",
        "titleZh": "TrapezoidShapedTabControl",
        "titleEn": "TrapezoidShapedTabControl",
        "description": "TrapezoidShapedTabControl",
        "sectionKey": "Demo.Catalog.SectionNavigation",
        "icon": "◤",
        "prototypeIcon": "<path d=\"M2 5a3 3 0 0 1 3-3h3.3a2 2 0 0 1 1.25.44l1.13.9a3 3 0 0 0 1.87.66H18c2 0 3 2 3 4H7.53A2 2 0 0 0 5.6 9.48L2.57 20.76C2.21 20.26 2 19.66 2 19V5Zm20.17 5H9.5a2 2 0 0 0-1.92 1.44l-2.7 9.28A1 1 0 0 0 5.82 22H18.5a2 2 0 0 0 1.92-1.44l2.7-9.28a1 1 0 0 0-.95-1.28Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconFolderOpen"
      },
      {
        "titleKey": "Demo.Item.WindowedStackPanel",
        "page": "WindowedStackPanelPage",
        "api": "WindowedStackPanel",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "wsp",
        "titleZh": "WindowedStackPanel",
        "titleEn": "WindowedStackPanel",
        "description": "WindowedStackPanel",
        "sectionKey": "Demo.Catalog.SectionLayout",
        "icon": "▥",
        "prototypeIcon": "<path d=\"M4 6.5a2 2 0 1 0 0-4 2 2 0 0 0 0 4ZM9.5 3a1.5 1.5 0 1 0 0 3h11a1.5 1.5 0 0 0 0-3h-11ZM8 11.5c0-.83.67-1.5 1.5-1.5h11a1.5 1.5 0 0 1 0 3h-11A1.5 1.5 0 0 1 8 11.5Zm0 7c0-.83.67-1.5 1.5-1.5h11a1.5 1.5 0 0 1 0 3h-11A1.5 1.5 0 0 1 8 18.5Zm-2-7a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-2 9a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconList"
      },
      {
        "titleKey": "Demo.Item.DiagonalLine",
        "page": "ControlApiPage(\"LeftTop2RightBottomLine\")",
        "api": "LeftTop2RightBottomLine",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "diagonal-line",
        "titleZh": "LeftTop2RightBottomLine",
        "titleEn": "LeftTop2RightBottomLine",
        "description": "LeftTop2RightBottomLine",
        "sectionKey": "Demo.Catalog.SectionLayout",
        "icon": "╲",
        "prototypeIcon": "<path d=\"M14.5 4.5l 5 5 2.09-2.09a2 2 0 0 0 0-2.82L19.4 2.4a2 2 0 0 0-2.82 0L14.5 4.5Z\n        M2.25 21.13l 1.68-5.88a1 1 0 0 1 .25-.43L13 6l5 5-8.82 8.82a1 1 0 0 1-.43.25l-5.88 1.68a.5.5 0 0 1-.62-.62Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconEdit"
      },
      {
        "titleKey": "Demo.Item.ContentExpansionAnimator",
        "page": "ContentExpansionPage",
        "api": "ContentExpansionAnimator",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "expansion",
        "titleZh": "ContentExpansionAnimator",
        "titleEn": "ContentExpansionAnimator",
        "description": "ContentExpansionAnimator",
        "sectionKey": "Demo.Catalog.SectionLayout",
        "icon": "◆",
        "prototypeIcon": "<path d=\"M10 2a1.5 1.5 0 0 1 0 3H7.12l3.94 3.94a1.5 1.5 0 0 1-2.12 2.12L5 7.12V10a1.5 1.5 0 0 1-3 0V3.5C2 2.67 2.67 2 3.5 2H10Z\n        M19 14a1.5 1.5 0 0 1 3 0v6.5c0 .83-.67 1.5-1.5 1.5H14a1.5 1.5 0 0 1 0-3h2.88l-3.94-3.94a1.5 1.5 0 0 1 2.12-2.12L19 16.88V14Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconExpand"
      }
    ],
    "icon": "❐",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Markdown",
    "titleZh": "Markdown",
    "titleEn": "Markdown",
    "id": "markdown",
    "items": [
      {
        "titleKey": "Demo.Item.MarkdownViewer",
        "page": "MarkdownViewerPage",
        "api": "MarkdownViewer",
        "package": "CodeWF.Avalonia.Markdown",
        "prototypeId": "md-viewer",
        "titleZh": "MarkdownViewer",
        "titleEn": "MarkdownViewer",
        "description": "MarkdownViewer",
        "sectionKey": "Demo.Catalog.SectionReading",
        "icon": "▤",
        "prototypeIcon": "<path d=\"M19 1a2 2 0 0 1 2 2v19a1 1 0 0 1-1 1H6a3 3 0 0 1-3-3V5a4 4 0 0 1 4-4h12ZM6 19a1 1 0 0 0 0 2h13v-2H6Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBook"
      },
      {
        "titleKey": "Demo.Item.MarkdownLite",
        "page": "MarkdownLitePage",
        "api": "Lite.MarkdownViewer",
        "package": "CodeWF.Avalonia.Markdown.Lite",
        "prototypeId": "md-lite",
        "titleZh": "Lite.MarkdownViewer",
        "titleEn": "Lite.MarkdownViewer",
        "description": "Lite.MarkdownViewer",
        "sectionKey": "Demo.Catalog.SectionReading",
        "icon": "▭",
        "prototypeIcon": "<path d=\"M19 1a2 2 0 0 1 2 2v19a1 1 0 0 1-1 1H6a3 3 0 0 1-3-3V5a4 4 0 0 1 4-4h12ZM6 19a1 1 0 0 0 0 2h13v-2H6Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconBook"
      },
      {
        "titleKey": "Demo.Item.MarkdownEditor",
        "page": "MarkdownEditorPage",
        "api": "MarkdownEditorView",
        "package": "CodeWF.Avalonia.Markdown",
        "prototypeId": "md-editor",
        "titleZh": "MarkdownEditorView",
        "titleEn": "MarkdownEditorView",
        "description": "MarkdownEditorView",
        "sectionKey": "Demo.Catalog.SectionEditing",
        "icon": "✎",
        "prototypeIcon": "<path d=\"M14.5 4.5l 5 5 2.09-2.09a2 2 0 0 0 0-2.82L19.4 2.4a2 2 0 0 0-2.82 0L14.5 4.5Z\n        M2.25 21.13l 1.68-5.88a1 1 0 0 1 .25-.43L13 6l5 5-8.82 8.82a1 1 0 0 1-.43.25l-5.88 1.68a.5.5 0 0 1-.62-.62Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconEdit"
      },
      {
        "titleKey": "Demo.Item.MarkdownLiveEditor",
        "page": "MarkdownLiveEditorPage",
        "api": "MarkdownLiveEditorView",
        "package": "CodeWF.Avalonia.Markdown",
        "prototypeId": "md-live",
        "titleZh": "MarkdownLiveEditorView",
        "titleEn": "MarkdownLiveEditorView",
        "description": "MarkdownLiveEditorView",
        "sectionKey": "Demo.Catalog.SectionEditing",
        "icon": "⚡",
        "prototypeIcon": "<path d=\"m16.43 4.1 3.54 3.54 1.48-1.48c.55-.55.55-1.45 0-2L19.9 2.62a1.42 1.42 0 0 0-2 0L16.43 4.1Z\n        m-8.76 11.88 1.19-4.17a.7.7 0 0 1.18-.3l6.24-6.25 3.54 3.54-6.24 6.24a.7.7 0 0 1-.3.18l-4.17 1.2a.35.35 0 0 1-.44-.44Z\n        M7.97 9.78c-.28.29-.5.63-.64 1l-.05.17-1.19 4.17A2.35 2.35 0 0 0 9 18.02l4.17-1.18c.44-.13.84-.37 1.17-.7l4.8-4.8a.5.5 0 0 1 .86.36V21a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h9.54a.5.5 0 0 1 .36.85L7.97 9.78Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconEdit2"
      },
      {
        "titleKey": "Demo.Item.MarkdownExport",
        "page": "MarkdownExportPage",
        "api": "MarkdownDocumentExporter",
        "package": "CodeWF.Avalonia.Markdown",
        "prototypeId": "md-export",
        "titleZh": "MarkdownDocumentExporter",
        "titleEn": "MarkdownDocumentExporter",
        "description": "MarkdownDocumentExporter",
        "sectionKey": "Demo.Catalog.SectionExport",
        "icon": "⇪",
        "prototypeIcon": "<path d=\"M17.84 8.06A6.5 6.5 0 1 0 5.18 11 4.5 4.5 0 0 0 5.5 20H17a6 6 0 0 0 .84-11.94Zm-5.52.2a.5.5 0 0 0-.64 0l-5.15 4.3a.25.25 0 0 0 .16.44H10v4.5c0 .28.22.5.5.5h3a.5.5 0 0 0 .5-.5V13h3.31c.23 0 .34-.3.16-.44l-5.15-4.3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconUpload"
      }
    ],
    "icon": "▤",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Utilities",
    "titleZh": "开发工具",
    "titleEn": "Developer Tools",
    "id": "utilities",
    "items": [
      {
        "titleKey": "Demo.Item.If",
        "page": "MarkupExtensionPage(\"If\")",
        "api": "If",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "if",
        "titleZh": "If",
        "titleEn": "If",
        "description": "If",
        "icon": "⟨⟩",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.Switch",
        "page": "MarkupExtensionPage(\"Switch\")",
        "api": "Switch",
        "package": "CodeWF.Avalonia.Controls",
        "prototypeId": "switch",
        "titleZh": "Switch",
        "titleEn": "Switch",
        "description": "Switch",
        "icon": "⇄",
        "prototypeIcon": "<path d=\"M14.49 4.19a1.5 1.5 0 0 0-2.98-.38l-2 16a1.5 1.5 0 1 0 2.98.38l2-16ZM8.06 5.94a1.5 1.5 0 0 1 0 2.12L4.12 12l3.94 3.94a1.5 1.5 0 1 1-2.12 2.12l-5-5a1.5 1.5 0 0 1 0-2.12l5-5a1.5 1.5 0 0 1 2.12 0Zm7.88 0a1.5 1.5 0 0 1 2.12 0l5 5a1.5 1.5 0 0 1 0 2.12l-5 5a1.5 1.5 0 0 1-2.12-2.12L19.88 12l-3.94-3.94a1.5 1.5 0 0 1 0-2.12Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconCode"
      },
      {
        "titleKey": "Demo.Item.Lang",
        "page": "LangPage",
        "api": "I18nManager",
        "package": "CodeWF.Avalonia.Lang",
        "prototypeId": "lang",
        "titleZh": "I18nManager",
        "titleEn": "I18nManager",
        "description": "I18nManager",
        "icon": "⊕",
        "prototypeIcon": "<path d=\"M3 4.5C3 3.67 3.67 3 4.5 3h15c.83 0 1.5.67 1.5 1.5v3a1.5 1.5 0 0 1-3 0V6h-4.5v13h2a1.5 1.5 0 0 1 0 3h-7a1.5 1.5 0 0 1 0-3h2V6H6v1.5a1.5 1.5 0 1 1-3 0v-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconText"
      }
    ],
    "icon": "◆",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  },
  {
    "titleKey": "Demo.Group.Examples",
    "titleZh": "综合示例",
    "titleEn": "Examples",
    "id": "examples",
    "items": [
      {
        "titleKey": "Demo.Item.MarkdownFlow",
        "page": "MarkdownFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-write",
        "titleZh": "Markdown 工作流",
        "titleEn": "Markdown workflow",
        "description": "",
        "icon": "▶",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      },
      {
        "titleKey": "Demo.Item.DataFlow",
        "page": "DataFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-data",
        "titleZh": "数据工作流",
        "titleEn": "Data workflow",
        "description": "",
        "icon": "▤",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      },
      {
        "titleKey": "Demo.Item.DocsFlow",
        "page": "DocsFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-docs",
        "titleZh": "多文档工作流",
        "titleEn": "Document workflow",
        "description": "",
        "icon": "❐",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      },
      {
        "titleKey": "Demo.Item.AppGuide",
        "page": "AppGuidePage",
        "api": "",
        "package": "",
        "prototypeId": "appguide",
        "titleZh": "应用级引导示例",
        "titleEn": "Application guide",
        "description": "",
        "icon": "◎",
        "prototypeIcon": "<path d=\"M2 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Z\n        M9.5 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M10.5 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M2 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-3Z\n        M18 9.5a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\n        M9.5 10.5a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1v-3Z\n        M3 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H3Z\n        M17 3a1 1 0 0 1 1-1h3a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-3a1 1 0 0 1-1-1V3Z\n        M18 17a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3Z\" fill=\"currentColor\" stroke=\"none\"/>",
        "iconKey": "SemiIconApps"
      }
    ],
    "icon": "▶",
    "prototypeIcon": "<path d=\"M4.5 10.8L12 4l7.5 6.8\"/><path d=\"M6.5 10v9.5h11V10\"/>"
  }
];
window.CodeWFCatalogText = {
  "zh-CN": {
    "Title": "控件目录",
    "Intro": "按控件名称查找演示；Ribbon 和导航树使用同一目录。",
    "Examples": "综合示例与交互实验",
    "Source": "查看源码",
    "Input": "输入参数",
    "Run": "运行示例",
    "Output": "运行结果",
    "Ready": "修改输入后运行示例。",
    "Failed": "调用失败：{0}",
    "About": "关于项目",
    "AboutDesc": "开源库的 API 演示。Ribbon 负责分类，Dock 承载演示、属性与日志。",
    "Project": "项目与安装",
    "License": "MIT 开源许可 · .NET 10 · Avalonia 12",
    "Notification": "由 LogNotifications 发出的日志通知。",
    "Notifications": "发送日志通知",
    "Preview": "浏览器交互示意；完整调用结果由 .NET Demo 产生。",
    "WindowsOnly": "此帮助类目前仅支持 Windows。",
    "ExpansionToggle": "展开 / 折叠",
    "ExpansionAnimate": "启用动画",
    "ExpansionHorizontal": "水平展开",
    "ExpansionVertical": "垂直展开",
    "ExpansionHint": "快速连续点击可在动画途中反向。关闭动画后立即到达终态；时长与展开尺寸可实时调整。",
    "ExpansionContent": "同一控件驱动尺寸与透明度，适用于侧栏、折叠面板与导航。",
    "TreeHints": "点击列头：升序 → 降序 → 原顺序。聚焦表格后按 Ctrl+A 全选；将鼠标悬停在截断文本上查看完整内容。增强适用于 FlatTreeDataGridSource。",
    "TreeSelection": "已选 {0} / {1} 行",
    "TreeLongText": "这是一段用于测试截断提示的较长文本，拖动列宽后可比较提示出现与消失的状态。",
    "LiveHint": "点击任意块直接编辑，离开该块恢复渲染；支持标题、列表、表格与格式命令。",
    "LiveFocus": "聚焦首块",
    "LiveInsert": "插入段落",
    "LiveTable": "插入表格",
    "LiveParagraph": "直接编辑这段 Markdown。",
    "StatusDriven": "在右侧属性面板切换状态与图标宽度",
    "StatusSampleDebug": "断点调试进行中",
    "StatusSampleInfo": "服务已就绪",
    "StatusSampleWarn": "磁盘空间不足",
    "StatusSampleError": "下游服务超时",
    "StatusSampleFatal": "数据库连接失败",
    "SectionStatuses": "状态显示",
    "SectionGuidance": "引导",
    "SectionNotifications": "通知",
    "SectionGrids": "表格",
    "SectionProperties": "属性编辑",
    "SectionLogs": "日志",
    "SectionWindows": "窗口",
    "SectionNavigation": "停靠与页签",
    "SectionLayout": "布局与动效",
    "SectionReading": "阅读",
    "SectionEditing": "编辑",
    "SectionExport": "导出"
  },
  "en-US": {
    "Title": "Control catalog",
    "Intro": "Find demos by API name. Ribbon and explorer share the same catalog.",
    "Examples": "Examples and experiments",
    "Source": "View source",
    "Input": "Input",
    "Run": "Run example",
    "Output": "Output",
    "Ready": "Edit the input and run the example.",
    "Failed": "Call failed: {0}",
    "About": "About this project",
    "AboutDesc": "An open source API gallery. Ribbon organizes APIs; Dock hosts demos, properties and logs.",
    "Project": "Project and installation",
    "License": "MIT license · .NET 10 · Avalonia 12",
    "Notification": "A log notification presented by LogNotifications.",
    "Notifications": "Send log notification",
    "Preview": "Browser interaction preview; the .NET demo runs the complete API call.",
    "WindowsOnly": "This helper currently supports Windows only.",
    "ExpansionToggle": "Expand / collapse",
    "ExpansionAnimate": "Enable animation",
    "ExpansionHorizontal": "Horizontal expansion",
    "ExpansionVertical": "Vertical expansion",
    "ExpansionHint": "Click rapidly to reverse mid-animation. Disable animation for an immediate transition. Adjust duration and size in place.",
    "ExpansionContent": "One control animates layout size and opacity for sidebars, panels and navigation.",
    "TreeHints": "Click a header: ascending → descending → original order. Focus the grid and press Ctrl+A; hover clipped text for its full value. Enhancements target FlatTreeDataGridSource.",
    "TreeSelection": "Selected {0} / {1} rows",
    "TreeLongText": "A deliberately long description for testing clipped-text tooltips; resize this column to compare clipped and fully visible text.",
    "LiveHint": "Click a block to edit it directly; leave the block to render it again. Try headings, lists, tables and formatting commands.",
    "LiveFocus": "Focus first block",
    "LiveInsert": "Insert paragraph",
    "LiveTable": "Insert table",
    "LiveParagraph": "Edit this Markdown block directly.",
    "StatusDriven": "Change the status and icon width in the property panel.",
    "StatusSampleDebug": "Debugging",
    "StatusSampleInfo": "Service ready",
    "StatusSampleWarn": "Disk space low",
    "StatusSampleError": "Service timeout",
    "StatusSampleFatal": "Database offline",
    "SectionStatuses": "Status",
    "SectionGuidance": "Guidance",
    "SectionNotifications": "Notifications",
    "SectionGrids": "Grids",
    "SectionProperties": "Properties",
    "SectionLogs": "Logs",
    "SectionWindows": "Windows",
    "SectionNavigation": "Dock & Tabs",
    "SectionLayout": "Layout & Motion",
    "SectionReading": "Reading",
    "SectionEditing": "Editing",
    "SectionExport": "Export"
  }
};
window.CodeWFCatalogDescriptions = {
  "zh-CN": {
    "SearchListBox": {
      "Desc": "输入关键词筛选列表，调整选择方式与下拉展示。"
    },
    "Transfer": {
      "Desc": "在两侧列表间移动条目，演示搜索、选择与批量穿梭。"
    },
    "ColumnDisplayEditor": {
      "Desc": "配置列的显示、顺序和宽度。"
    },
    "StatusBadge": {
      "Desc": "使用 Debug / Info / Warn / Error / Fatal 样式类切换徽标等级，编辑左右文本。",
      "Xaml": "<codewf:StatusBadge Classes=\"Info\" LeftText=\"Info\" RightText=\"Ready\" />",
      "CSharp": "badge.Classes.Set(\"Info\", true);\nbadge.LeftText = \"Info\";\nbadge.RightText = \"Ready\";"
    },
    "StatusCard": {
      "Desc": "切换状态等级和上下文本，查看状态卡片的颜色与布局。",
      "Xaml": "<codewf:StatusCard Kind=\"{Binding Kind}\" IconWidth=\"{Binding IconWidth}\" TopText=\"INFO\" BottomText=\"Ready\" />",
      "CSharp": "card.Kind = StatusLabelKind.Info;\ncard.IconWidth = 32;"
    },
    "Guide": {
      "Desc": "启动控件引导，体验步骤切换、遮罩与目标定位。"
    },
    "Notifications": {
      "Desc": "写入警告日志，通过 LogNotifications 触发应用内通知。"
    },
    "DataGrid": {
      "Desc": "对比原生表格与增强表格的三态排序、自然排序和智能提示。"
    },
    "TreeDataGrid": {
      "Desc": "使用 EnableDefaults 启用三态排序、全选与智能提示。"
    },
    "PropertyGrid": {
      "Desc": "编辑不同类型的属性，体验分类、搜索、提交和重置。"
    },
    "Log": {
      "Desc": "按级别和关键词过滤实时日志，查看着色与详情。"
    },
    "CodeWFWindow": {
      "Desc": "体验自定义窗口标题栏、内容槽和窗口交互。"
    },
    "TitleBar": {
      "Desc": "修改标题文本，查看左右内容槽和主题适配。"
    },
    "Dock": {
      "Desc": "拖动、停靠和浮动文档或工具面板，保存与恢复布局。"
    },
    "TrapezoidTab": {
      "Desc": "将梯形主题应用于 TabControl，演示选中与位置样式。"
    },
    "WindowedStackPanel": {
      "Desc": "观察可视窗口内的条目布局，调整方向与间距。"
    },
    "DiagonalLine": {
      "Desc": "调整 BorderThickness，实时查看对角线的绘制效果。"
    },
    "MarkdownViewer": {
      "Desc": "渲染 Markdown、代码、公式和图表，切换排版主题。"
    },
    "MarkdownLite": {
      "Desc": "使用轻量渲染器展示常规 Markdown 元素与排版主题。"
    },
    "MarkdownEditor": {
      "Desc": "编辑 Markdown 源码，体验语法高亮与编辑工具。"
    },
    "MarkdownLiveEditor": {
      "Desc": "体验 Markdown 实时编辑与预览。"
    },
    "If": {
      "Desc": "改变条件，观察 If 标记扩展的值选择。",
      "Xaml": "<TextBlock.Text>\n  <codewf:If Condition=\"{Binding IsBusy}\" True=\"Busy\" False=\"Ready\" />\n</TextBlock.Text>"
    },
    "Switch": {
      "Desc": "改变状态，观察 Switch 标记扩展的分支匹配。",
      "Xaml": "<TextBlock.Text>\n  <codewf:Switch Value=\"{Binding State}\" Default=\"Unknown\">\n    <codewf:SwitchCase When=\"Idle\" Then=\"Ready\" />\n    <codewf:SwitchCase When=\"Busy\" Then=\"Working\" />\n  </codewf:Switch>\n</TextBlock.Text>"
    },
    "Lang": {
      "Desc": "切换语言，验证资源绑定和文案实时更新。"
    },
    "MarkdownExport": {
      "Desc": "将 Markdown 导出为 PNG、PDF、Word 或 HTML。"
    },
    "ContentExpansionAnimator": {
      "Desc": "展开、折叠、横纵布局与快速反向；尺寸和透明度由同一动画进度驱动。"
    }
  },
  "en-US": {
    "SearchListBox": {
      "Desc": "Filter a list by keyword and explore selection and dropdown options."
    },
    "Transfer": {
      "Desc": "Move items between two lists with search, selection and batch transfer."
    },
    "ColumnDisplayEditor": {
      "Desc": "Configure column visibility, order and width."
    },
    "StatusBadge": {
      "Desc": "Select Debug / Info / Warn / Error / Fatal style classes and edit left/right badge text.",
      "Xaml": "<codewf:StatusBadge Classes=\"Info\" LeftText=\"Info\" RightText=\"Ready\" />",
      "CSharp": "badge.Classes.Set(\"Info\", true);\nbadge.LeftText = \"Info\";\nbadge.RightText = \"Ready\";"
    },
    "StatusCard": {
      "Desc": "Change status levels and top/bottom text to preview status cards.",
      "Xaml": "<codewf:StatusCard Kind=\"{Binding Kind}\" IconWidth=\"{Binding IconWidth}\" TopText=\"INFO\" BottomText=\"Ready\" />",
      "CSharp": "card.Kind = StatusLabelKind.Info;\ncard.IconWidth = 32;"
    },
    "Guide": {
      "Desc": "Start a control guide with step navigation, overlays and target placement."
    },
    "Notifications": {
      "Desc": "Write a warning log to trigger an in-app notification through LogNotifications."
    },
    "DataGrid": {
      "Desc": "Compare standard and enhanced grids with three-state sorting, natural sorting and smart tooltips."
    },
    "TreeDataGrid": {
      "Desc": "Enable three-state sorting, select-all and smart tooltips with EnableDefaults."
    },
    "PropertyGrid": {
      "Desc": "Edit typed properties with categories, search, commits and reset."
    },
    "Log": {
      "Desc": "Filter live logs by level and keyword, with color coding and details."
    },
    "CodeWFWindow": {
      "Desc": "Explore a custom window title bar, content slots and window interactions."
    },
    "TitleBar": {
      "Desc": "Edit the title and inspect left/right content slots and theme styling."
    },
    "Dock": {
      "Desc": "Drag, dock and float documents and tool panels, then save and restore layouts."
    },
    "TrapezoidTab": {
      "Desc": "Apply the trapezoid theme to TabControl and explore selection and placement."
    },
    "WindowedStackPanel": {
      "Desc": "Explore item layout inside the visible window, with orientation and spacing options."
    },
    "DiagonalLine": {
      "Desc": "Change BorderThickness to preview diagonal line rendering."
    },
    "MarkdownViewer": {
      "Desc": "Render Markdown, code, formulas and diagrams with typography themes."
    },
    "MarkdownLite": {
      "Desc": "Explore standard Markdown elements and typography in the lightweight renderer."
    },
    "MarkdownEditor": {
      "Desc": "Edit Markdown source with syntax highlighting and editing tools."
    },
    "MarkdownLiveEditor": {
      "Desc": "Explore live Markdown editing and preview."
    },
    "If": {
      "Desc": "Change a condition to inspect value selection by the If markup extension.",
      "Xaml": "<TextBlock.Text>\n  <codewf:If Condition=\"{Binding IsBusy}\" True=\"Busy\" False=\"Ready\" />\n</TextBlock.Text>"
    },
    "Switch": {
      "Desc": "Change state to inspect branch matching by the Switch markup extension.",
      "Xaml": "<TextBlock.Text>\n  <codewf:Switch Value=\"{Binding State}\" Default=\"Unknown\">\n    <codewf:SwitchCase When=\"Idle\" Then=\"Ready\" />\n    <codewf:SwitchCase When=\"Busy\" Then=\"Working\" />\n  </codewf:Switch>\n</TextBlock.Text>"
    },
    "Lang": {
      "Desc": "Switch language to verify resource bindings and live text updates."
    },
    "MarkdownExport": {
      "Desc": "Export Markdown to PNG, PDF, Word or HTML."
    },
    "ContentExpansionAnimator": {
      "Desc": "Expand, collapse and reverse horizontal or vertical layout animations with a single progress value."
    }
  }
};
window.CodeWFPrototypeSettings = {
  "zh-CN": {
    "shell": {
      "Palette": "色板",
      "Dark": "深色",
      "Light": "浅色",
      "Properties": "属性",
      "Log": "日志",
      "Navigation": "导航",
      "WriteLogs": "写入三类日志",
      "Ready": "就绪",
      "Reset": "重置",
      "DemoShell": "演示台",
      "QuickSave": "保存布局",
      "QuickSettings": "设置",
      "QuickGuide": "打开引导（应用级）",
      "PinRibbon": "展开/折叠命令区",
      "Group": "外壳",
      "Command": "命令面板",
      "CommandPlaceholder": "搜索页面或命令（↑↓ 选择，Enter 执行，Esc 关闭）",
      "CommandHint": "Ctrl+K 开关命令面板 · Ctrl+D 深浅 · Ctrl+L 语言",
      "CommandEmpty": "没有匹配的页面或命令",
      "Density": "密度",
      "DensityLabel": "切换密度：舒适 / 紧凑",
      "DensityCompact": "紧凑",
      "DensityComfort": "舒适",
      "LayoutSaved": "布局已保存",
      "LayoutSaveFailed": "保存布局失败",
      "LayoutLoaded": "已恢复上次布局",
      "ResetRenames": "恢复原名",
      "ResetRenamesHint": "清除全部就地重命名",
      "Rename": "重命名",
      "RenameHint": "双击或点铅笔就地重命名，回车提交 / Esc 取消",
      "Notification": "通知",
      "NotificationClear": "清空通知",
      "NotificationDismiss": "关闭该通知",
      "NotifyDemo": "发送示例通知",
      "NotifyDemoHint": "演示通知中心：卡片 6 秒后自动消失，最多保留 4 条。",
      "NavFilter": "过滤页面…",
      "NavEmpty": "没有匹配的页面",
      "Account": "账户",
      "GlobalSettings": {
        "PaletteLabel": "色板",
        "PaletteDescription": "八套配色：.NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏与夜空。",
        "ModeLabel": "界面模式",
        "LightMode": "浅色",
        "DarkMode": "深色",
        "ModeDescription": "浅色、深色或跟随系统，适用于每套配色。",
        "LanguageLabel": "语言",
        "LanguageDescription": "界面语言，切换即时生效。",
        "AppearanceGroup": "外观",
        "AppearanceDescription": "演示台外观设置。",
        "LanguageGroup": "语言",
        "LanguageGroupDescription": "演示台语言设置。",
        "SystemMode": "跟随系统"
      },
      "LanguageChinese": "简体中文",
      "DocumentOpened": "打开文档：",
      "LayoutReset": "重置工作区布局",
      "LanguageChanged": "界面语言切换为 ",
      "DocumentClosed": "关闭文档：",
      "PanelShown": "显示面板：",
      "PanelHidden": "隐藏面板：",
      "SampleInformationLog": "演示：一条信息日志",
      "SampleWarningLog": "演示：一条警告日志",
      "SampleErrorLog": "演示：一条错误日志"
    },
    "palettes": {
      "Default": "默认",
      "Aquatic": "水生",
      "Desert": "沙漠",
      "Dusk": "黄昏",
      "NightSky": "夜空",
      "HighContrast": "高对比",
      "DotNet": ".NET 紫",
      "Aurora": "AI 极光",
      "Ember": "AI 暖陶"
    },
    "language": "语言",
    "group": "外观"
  },
  "en-US": {
    "shell": {
      "Palette": "Palette",
      "Dark": "Dark",
      "Light": "Light",
      "Properties": "Props",
      "Log": "Log",
      "Navigation": "Navigation",
      "WriteLogs": "Write sample logs",
      "Ready": "Ready",
      "Reset": "Reset",
      "DemoShell": "Demo Shell",
      "QuickSave": "Save layout",
      "QuickSettings": "Settings",
      "QuickGuide": "Open guidance (app-wide)",
      "PinRibbon": "Expand / collapse commands",
      "Group": "Shell",
      "Command": "Command palette",
      "CommandPlaceholder": "Search pages or commands (↑↓ select, Enter run, Esc close)",
      "CommandHint": "Ctrl+K palette · Ctrl+D theme · Ctrl+L language",
      "CommandEmpty": "No matching page or command",
      "Density": "Density",
      "DensityLabel": "Toggle density: cozy / compact",
      "DensityCompact": "Compact",
      "DensityComfort": "Cozy",
      "LayoutSaved": "Layout saved",
      "LayoutSaveFailed": "Saving layout failed",
      "LayoutLoaded": "Restored the previous layout",
      "ResetRenames": "Restore names",
      "ResetRenamesHint": "Clear every in-place rename",
      "Rename": "Rename",
      "RenameHint": "Double-click or press the pencil to rename in place; Enter commits, Esc cancels",
      "Notification": "Notifications",
      "NotificationClear": "Clear notifications",
      "NotificationDismiss": "Dismiss this notification",
      "NotifyDemo": "Send a sample notification",
      "NotifyDemoHint": "Notification centre demo: cards auto-dismiss after 6 seconds, at most 4 kept.",
      "NavFilter": "Filter pages…",
      "NavEmpty": "No matching page",
      "Account": "Account",
      "GlobalSettings": {
        "PaletteLabel": "Palette",
        "PaletteDescription": "Eight palettes: .NET Purple, AI Aurora, AI Ember, Default, Aquatic, Desert, Dusk, and Night Sky.",
        "ModeLabel": "Mode",
        "LightMode": "Light",
        "DarkMode": "Dark",
        "ModeDescription": "Light, dark, or system mode applies to every palette.",
        "LanguageLabel": "Language",
        "LanguageDescription": "Application language; changes take effect immediately.",
        "AppearanceGroup": "Appearance",
        "AppearanceDescription": "Workspace appearance settings.",
        "LanguageGroup": "Language",
        "LanguageGroupDescription": "Workspace language settings.",
        "SystemMode": "System"
      },
      "LanguageChinese": "简体中文",
      "DocumentOpened": "Opened document: ",
      "LayoutReset": "Reset workspace layout",
      "LanguageChanged": "Language changed to ",
      "DocumentClosed": "Closed document: ",
      "PanelShown": "Showing panel: ",
      "PanelHidden": "Hiding panel: ",
      "SampleInformationLog": "Demo: information log",
      "SampleWarningLog": "Demo: warning log",
      "SampleErrorLog": "Demo: error log"
    },
    "palettes": {
      "Default": "Default",
      "Aquatic": "Aquatic",
      "Desert": "Desert",
      "Dusk": "Dusk",
      "NightSky": "NightSky",
      "HighContrast": "High Contrast",
      "DotNet": ".NET Purple",
      "Aurora": "AI Aurora",
      "Ember": "AI Ember"
    },
    "language": "Language",
    "group": "Appearance"
  }
};
window.CodeWFMarkdownSamples = {
  "zh-CN": {
    "viewer": "# MarkdownViewer 演示\r\n\r\n完整渲染管线：**Markdig 解析** → 块级模型 → `WindowedStackPanel` 虚拟化呈现。\r\n\r\n## 任务回写\r\n勾选下面的任务，Markdown 源文本会同步更新（TaskWriteBackRequested）：\r\n\r\n- [x] 解析 Markdown 文本\r\n- [ ] 调整排版主题（右侧属性面板）\r\n- [ ] 展开输入参数，修改源文本后点击「应用」，观察增量渲染耗时\r\n\r\n## 表格与代码\r\n\r\n| 能力 | 说明 |\r\n| --- | --- |\r\n| 增量渲染 | 只重排变更块 |\r\n| 虚拟化 | 超过阈值后按窗口实现块 |\r\n\r\n```csharp\r\nvar viewer = new MarkdownViewer { Markdown = \"# 你好\" };\r\n```\r\n\r\n> 提示：右侧 PropertyGrid 即本控件的兄弟实例——属性面板自身也是一次自我演示。",
    "lite": "# Markdown Lite 渲染范围\r\n\r\n本页内容刻意只使用 **Lite 支持的元素**：标题、**加粗**、*斜体*、`行内代码`、[链接](https://codewf.com)、列表、表格与代码块。\r\n\r\n## 列表\r\n- 基础块级元素\r\n- 表格与引用\r\n- 任务列表勾选回写同样可用\r\n\r\n> 引用块：Lite 与完整包共用同一渲染管线。\r\n\r\n## 降级演示（完整包能力在 Lite 中的表现）\r\n\r\n| 元素 | Lite 行为 |\r\n| --- | --- |\r\n| 图片 | 显示替代文本 |\r\n| 数学公式 | 显示 LaTeX 原文 |\r\n| 代码高亮 | 单色渲染 |\r\n\r\n```csharp\r\n// 代码块单色渲染（完整包为 TextMate 高亮）\r\nConsole.WriteLine(\"Hello, Markdown Lite!\");\r\n```\r\n\r\n- [x] 使用 Lite 渲染\r\n- [ ] 切换排版主题（右侧属性面板）",
    "editor": "# MarkdownEditorView\r\n\r\nAvaloniaEdit 驱动的 Markdown 源码编辑器。\r\n\r\n## 内置能力\r\n- TextMate 语法着色\r\n- 标题 / 列表 / 表格 / 任务列表 / 代码块 / 数学块等约 40 项动作\r\n- 查找替换、自动配对、行号、字号\r\n\r\n```csharp\r\nvar editor = new MarkdownEditorView();\r\n```\r\n\r\n> 用上方按钮或快捷键体验编辑动作；修改会实时反映在状态栏。",
    "live": "# 所见即所得\r\n\r\n直接在块上编辑：标题、段落、列表、表格都是可编辑块。\r\n\r\n## 列表\r\n- 块级建模（MarkdownBlockModel）\r\n- 编辑即渲染\r\n- 无左右分栏\r\n\r\n> 点击任意块开始编辑；顶部按钮演示插入与聚焦。\r\n\r\n| 能力 | 说明 |\r\n| --- | --- |\r\n| FocusBlock | 聚焦指定块 |\r\n| ApplyExternalEdit | 外部回写 |",
    "apply": "应用",
    "append": "追加段落（增量）",
    "reset": "重置示例",
    "themeLabel": "排版主题",
    "sizeLabel": "排版尺寸",
    "virtualizationLabel": "虚拟化",
    "thresholdLabel": "虚拟化阈值"
  },
  "en-US": {
    "viewer": "# MarkdownViewer\n\n**Markdig** → block models → windowed rendering.\n\n## Features\n- [x] Incremental rendering\n- [ ] Task write-back\n\n| Component | Purpose |\n| --- | --- |\n| Viewer | Render Markdown |\n| Editor | Edit source |\n\n```csharp\nviewer.Markdown = source;\n```\n\n> Try changing the typography theme.",
    "lite": "# Markdown Lite\n\nHeadings, **bold**, *italic*, `inline code`, lists, quotes, tables, links, and code blocks.\n\n- Smaller dependency set\n- 18 typography themes\n\n| Feature | Supported |\n| --- | --- |\n| Common Markdown | Yes |\n| Export | Use full package |\n\n```csharp\nviewer.Markdown = source;\n```\n\n> A focused rendering package.",
    "editor": "# MarkdownEditorView\n\nMarkdown source editing with AvaloniaEdit.\n\n## Built-in actions\n- Bold and italic\n- Headings and tables\n- Task lists and code\n\n```csharp\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\n```",
    "live": "# Live editing\n\nEdit headings, paragraphs, lists, and tables directly.\n\n## List\n- Block models\n- Incremental rendering\n\n```csharp\nvar editor = new MarkdownLiveEditorView();\n```\n\n| Feature | State |\n| --- | --- |\n| Editing | Ready |",
    "apply": "Apply",
    "append": "Append paragraph (incremental)",
    "reset": "Reset sample",
    "themeLabel": "Typography theme",
    "sizeLabel": "Typography size",
    "virtualizationLabel": "Virtualization",
    "thresholdLabel": "Virtualization threshold"
  }
};
window.CodeWFMarkdownTypography = {
  "Common": {
    "CodeWFMarkdownParagraphFontSize": 16.0,
    "CodeWFMarkdownParagraphLineHeight": 28.0,
    "CodeWFMarkdownHeading1FontSize": 30.0,
    "CodeWFMarkdownHeading2FontSize": 26.0,
    "CodeWFMarkdownHeading3FontSize": 22.0,
    "CodeWFMarkdownHeading4FontSize": 20.0,
    "CodeWFMarkdownHeading5FontSize": 18.0,
    "CodeWFMarkdownHeading6FontSize": 16.0,
    "CodeWFMarkdownBlockSpacing": 8.0,
    "CodeWFMarkdownDocumentBottomPadding": 72.0,
    "CodeWFMarkdownCodeBlockFontSize": 13.0,
    "CodeWFMarkdownCodeBlockLineHeight": 20.0,
    "CodeWFMarkdownCodeLanguageFontSize": 12.0,
    "CodeWFMarkdownCodeBlockSpacing": 8.0,
    "CodeWFMarkdownCodeBlockHeaderSpacing": 8.0,
    "CodeWFMarkdownCopyButtonMinWidth": 54.0,
    "CodeWFMarkdownCopyButtonHeight": 28.0,
    "CodeWFMarkdownListSpacing": 6.0,
    "CodeWFMarkdownListItemContentSpacing": 4.0,
    "CodeWFMarkdownUnorderedListMarkerWidth": 24.0,
    "CodeWFMarkdownOrderedListMarkerMinWidth": 28.0,
    "CodeWFMarkdownOrderedListMarkerCharacterWidth": 9.0,
    "CodeWFMarkdownOrderedListMarkerExtraWidth": 6.0,
    "CodeWFMarkdownQuoteContentSpacing": 4.0,
    "CodeWFMarkdownThematicBreakHeight": 1.0,
    "CodeWFMarkdownTableCellMinHeight": 38.0,
    "CodeWFMarkdownTableCellContentSpacing": 4.0,
    "CodeWFMarkdownImageMaxWidth": 900.0,
    "CodeWFMarkdownImageMaxHeight": 520.0,
    "CodeWFMarkdownImageFallbackOpacity": 0.72,
    "CodeWFMarkdownImagePreviewWidth": 1120.0,
    "CodeWFMarkdownImagePreviewHeight": 760.0,
    "CodeWFMarkdownImagePreviewMinWidth": 720.0,
    "CodeWFMarkdownImagePreviewMinHeight": 480.0,
    "CodeWFMarkdownImagePreviewToolbarSpacing": 8.0,
    "CodeWFMarkdownImagePreviewZoomTextMinWidth": 58.0
  },
  "Basic": {
    "CodeWFMarkdownParagraphFontSize": 16.0,
    "CodeWFMarkdownParagraphLineHeight": 26.0,
    "CodeWFMarkdownHeading1FontSize": 32.0,
    "CodeWFMarkdownHeading2FontSize": 24.0,
    "CodeWFMarkdownHeading3FontSize": 20.0,
    "CodeWFMarkdownHeading4FontSize": 18.0,
    "CodeWFMarkdownHeading5FontSize": 16.0,
    "CodeWFMarkdownHeading6FontSize": 16.0,
    "CodeWFMarkdownTableCellMinHeight": 34.0,
    "CodeWFMarkdownTableCellContentSpacing": 0.0
  },
  "OrangeHeart": {},
  "InkBlack": {
    "CodeWFMarkdownHeading1FontSize": 29.0
  },
  "ColorfulPurple": {
    "CodeWFMarkdownHeading1FontSize": 32.0,
    "CodeWFMarkdownParagraphLineHeight": 29.0
  },
  "TenderGreen": {},
  "Verdant": {},
  "RedScarlet": {
    "CodeWFMarkdownHeading1FontSize": 31.0
  },
  "BlueGlow": {},
  "TechnologyBlue": {
    "CodeWFMarkdownParagraphLineHeight": 29.0
  },
  "LanQing": {},
  "Yamabuki": {
    "CodeWFMarkdownHeading1FontSize": 31.0
  },
  "FrontendPeak": {
    "CodeWFMarkdownParagraphLineHeight": 30.0,
    "CodeWFMarkdownHeading1FontSize": 33.0
  },
  "GeekBlack": {},
  "Simple": {
    "CodeWFMarkdownParagraphLineHeight": 27.0
  },
  "RosePurple": {},
  "CuteGreen": {
    "CodeWFMarkdownParagraphFontSize": 16.5,
    "CodeWFMarkdownParagraphLineHeight": 30.0
  },
  "FullStackBlue": {
    "CodeWFMarkdownHeading1FontSize": 32.0
  },
  "WeChatFormat": {
    "CodeWFMarkdownParagraphFontSize": 16.0,
    "CodeWFMarkdownParagraphLineHeight": 26.0,
    "CodeWFMarkdownHeading2FontSize": 22.4,
    "CodeWFMarkdownHeading3FontSize": 19.2,
    "CodeWFMarkdownQuoteContentSpacing": 0.0
  },
  "CompactTypographyResources": {
    "CodeWFMarkdownParagraphFontSize": 14.0,
    "CodeWFMarkdownParagraphLineHeight": 22.0,
    "CodeWFMarkdownHeading1FontSize": 26.0,
    "CodeWFMarkdownHeading2FontSize": 22.0,
    "CodeWFMarkdownHeading3FontSize": 19.0,
    "CodeWFMarkdownHeading4FontSize": 17.0,
    "CodeWFMarkdownHeading5FontSize": 15.0,
    "CodeWFMarkdownHeading6FontSize": 14.0,
    "CodeWFMarkdownBlockSpacing": 4.0,
    "CodeWFMarkdownDocumentBottomPadding": 40.0,
    "CodeWFMarkdownCodeBlockFontSize": 12.0,
    "CodeWFMarkdownCodeBlockLineHeight": 18.0,
    "CodeWFMarkdownCodeLanguageFontSize": 11.0,
    "CodeWFMarkdownCodeBlockSpacing": 4.0,
    "CodeWFMarkdownCodeBlockHeaderSpacing": 6.0,
    "CodeWFMarkdownCopyButtonMinWidth": 48.0,
    "CodeWFMarkdownCopyButtonHeight": 24.0,
    "CodeWFMarkdownListSpacing": 3.0,
    "CodeWFMarkdownListItemContentSpacing": 2.0,
    "CodeWFMarkdownUnorderedListMarkerWidth": 20.0,
    "CodeWFMarkdownOrderedListMarkerMinWidth": 24.0,
    "CodeWFMarkdownOrderedListMarkerCharacterWidth": 8.0,
    "CodeWFMarkdownOrderedListMarkerExtraWidth": 4.0,
    "CodeWFMarkdownQuoteContentSpacing": 3.0,
    "CodeWFMarkdownTableCellMinHeight": 28.0,
    "CodeWFMarkdownTableCellContentSpacing": 2.0,
    "CodeWFMarkdownImagePreviewToolbarSpacing": 6.0,
    "CodeWFMarkdownImagePreviewZoomTextMinWidth": 52.0
  }
};
