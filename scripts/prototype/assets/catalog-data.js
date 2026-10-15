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
window.CodeWFPrototypeResources = {
  "zh-CN": {
    "Content": {
      "AppGuide": {
        "View": {
          "TourTheRibbonNavigationDocumentsPropertiesAndLogs": "应用引导：对整个演示台做逐区域引导——Ribbon 页签、导航树、文档区、属性面板、日志面板。目标控件位于主窗口（跨控件树解析），因此步骤在代码中构建。",
          "StartWorkspaceTour": "开始演示台引导",
          "TourRegions": "引导区域",
          "RibbonCommandsNavigationDocumentsWithDemoXAMLC": "① Ribbon 命令区（双击页签可折叠）\n② 左侧导航树（单击打开演示文档）\n③ 中央文档区（演示与可折叠源码）\n④ 右侧 PropertyGrid（驱动当前演示）\n⑤ 底部日志面板（实时事件流）"
        },
        "Properties": {
          "TargetLookup": "目标解析",
          "NamesOfTheFiveWorkspaceRegionsResolvedControls": "演示台五个区域控件名；跨控件树目标在代码中解析后传入 GuideStep.Target。"
        },
        "Source": {
          "CSharp": "// 目标在另一个控件树（主窗口）时：解析到根后按名称查找，再构建步骤\r\nvar root = TopLevel.GetTopLevel(this) as Visual;\r\nvar ribbon = FindDescendant(root, \"RibbonRoot\");\r\nvar navTree = FindDescendant(root, \"NavTree\");\r\n\r\nAppGuide.Steps.Add(new GuideStep\r\n{\r\n    Title = \"① Ribbon\",\r\n    Description = \"7 个页签分组演示命令\",\r\n    Target = ribbon,\r\n    Placement = GuidePlacementMode.Bottom,\r\n});\r\nAppGuide.Show();",
          "Xaml": "<!-- 引导控件置于页面即可（遮罩为窗口级 Popup）；目标在主窗口上，步骤由代码构建 -->\r\n<codewf:Guide x:Name=\"AppGuide\" IsArrowVisible=\"True\" />"
        },
        "OnStartGuide": {
          "TargetLookupFailedUsingTheDefaultBehavior": "（目标解析失败，将按缺省行为处理）",
          "RibbonCommands": "① Ribbon 命令区",
          "EightRibbonGroupsContainTheDemoCommandsDouble": "7 个页签分组全部演示命令；双击页签可折叠 / 展开命令区。",
          "Navigation": "② 导航树",
          "MirrorsRibbonGroupsClickAnItemToOpen": "与 Ribbon 分组镜像；单击叶子即打开对应演示文档。",
          "Documents": "③ 文档区",
          "EachComponentHasADemoDocumentWithDemo": "每个控件一个独立演示文档；演示始终可见，源码按需展开。",
          "ThePropertyPanelConfiguresTheActiveDemoThe": "右侧属性面板驱动当前演示；无文档时展示全局外观 / 语言设置。",
          "Logs": "⑤ 日志面板",
          "LiveLoggerEventsWithFilteringCopyClearAnd": "Logger 实时事件流；支持级别过滤、复制、清空与打开日志目录。",
          "AppGuideStartingWorkspaceTour": "AppGuide：开始演示台引导"
        },
        "OnStepOpened": {
          "AppGuideStepFormat": "AppGuide：第 {0}/{1} 步 · {2}"
        }
      },
      "CodeWFWindow": {
        "View": {
          "BorderlessCodeWFWindowWithDWMCornersACustomTitle": "CodeWFWindow：无边框窗口（DWM 圆角 / 自定义标题栏 / 标题栏按钮开关 / 内容钻入标题栏）。下方为可嵌入的 CodeWFTitleBar 配置样例，按钮打开不同配置的真实窗口。",
          "OpenDefaultConfiguration": "打开：默认配置",
          "OpenMinimalButtonsAndCustomTitleBar": "打开：精简按钮 + 自定义标题栏内容",
          "OpenExtendContentIntoTitleBar": "打开：内容钻入标题栏",
          "Example1DefaultTitleBar": "样例 1：默认标题栏",
          "BorderlessCustomTitleBar": "无边界 · 自定义标题栏",
          "TitleBarHeight36WithMinimizeRestoreCloseButtons": "TitleBarHeight=36，默认最小化 / 还原 / 关闭按钮。",
          "Example2CustomLeftRightContent": "样例 2：自定义左 / 右内容",
          "DocumentDesignNotes": "文档：设计说明",
          "Search": "搜索…",
          "LeftContentRightContentAcceptArbitraryContent": "LeftContent / RightContent 槽位可放任意内容。",
          "Example3CenteredContent": "样例 3：居中内容",
          "Left": "（左）",
          "CenteredTitleCenterContent": "居中标题 · CenterContent",
          "ArrangeCenterContentAndTheTitleToAvoidOverlap": "CenterContent 与标题并存时自行规划布局。"
        },
        "Properties": {
          "CaptionButtonWidth": "窗口按钮宽度",
          "CaptionButtonWidthDescription": "控制下一示例窗口的最小化、最大化和关闭按钮宽度。",
          "TitleBarHeight": "标题栏高度",
          "TitleBarHeightForTheNextExampleWindow": "新打开示例窗口的标题栏高度。",
          "WindowCornerRadius": "窗口圆角",
          "WindowCornerRadiusWindowsDWM": "窗口四角圆角半径（Windows DWM）。",
          "ExtendContentIntoTitleBar": "内容钻入标题栏",
          "ExtendTheWindowContentUnderneathTheTitleBar": "窗口内容延伸至标题栏下方，形成整体化视觉。"
        },
        "Source": {
          "CSharp": "var window = new CodeWFWindow\n{\n    Title = \"CodeWFWindow 示例\",\n    Width = 640,\n    Height = 420,\n    ContentExtendsIntoTitleBar = true,   // 内容钻入标题栏\n    IsFullScreenButtonVisible = true,\n    IsMinimizeButtonVisible = true,\n    WindowCornerRadius = new CornerRadius(8),\n};\nwindow.Content = new TextBlock\n{\n    Text = \"无边框窗口内容\",\n    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,\n    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,\n};\nwindow.Show(owner);   // DWM 圆角仅 Windows 生效，其它平台自动降级",
          "Xaml": "<!-- 替换 Window：无边框 + DWM 圆角 + 自定义标题栏，需引入 ACSemiTheme -->\n<codewf:CodeWFWindow xmlns:codewf=\"https://codewf.com\"\n                     Title=\"CodeWFWindow 示例\"\n                     TitleBarHeight=\"36\"\n                     WindowCornerRadius=\"8\"\n                     ContentExtendsIntoTitleBar=\"True\"\n                     IsFullScreenButtonVisible=\"True\">\n  <codewf:CodeWFWindow.LeftContent>\n    <TextBlock Text=\"◆\" Foreground=\"DodgerBlue\" VerticalAlignment=\"Center\" />\n  </codewf:CodeWFWindow.LeftContent>\n  <Panel>\n    <!-- 窗口内容；钻入模式下可延伸到标题栏下方 -->\n  </Panel>\n</codewf:CodeWFWindow>"
        },
        "OnOpenCustom": {
          "MinimalButtonsAndCustomTitleBar": "精简按钮 + 自定义标题栏"
        },
        "OnOpenDefault": {
          "DefaultConfiguration": "默认配置"
        },
        "OnOpenExtends": {
          "ExtendContentIntoTitleBar": "内容钻入标题栏"
        },
        "ShowSample": {
          "CodeWFWindowExampleFormat": "CodeWFWindow 示例 · {0}",
          "Search": "搜索…",
          "ThisContentExtendsIntoTheTitleBarContentExtendsIntoTitleBar": "这段内容钻入了标题栏（ContentExtendsIntoTitleBar=True）",
          "ConfigurationCloseThisWindowToTryAnotherConfigurationFormat": "配置：{0}\n关闭此窗口后可再试其它配置。"
        }
      },
      "ColumnDisplayEditor": {
        "View": {
          "ResetDefaults": "重置默认配置",
          "DragARowHandleToReorderColumnsAnd": "拖拽行首手柄排序；勾选控制列可见性。编辑器直接在源集合上 Move，业务侧无需二次同步。",
          "ChangeAudit": "变更审计"
        },
        "Properties": {
          "DataContract": "数据契约",
          "TheEditorRequiresIListItemsSourceDraggingReordersIt": "编辑器要求 ItemsSource 为 IList；拖拽排序通过 IList.Move 完成。"
        },
        "Source": {
          "CSharp": "var columns = new ObservableCollection<ColumnDisplayItem>\r\n{\r\n    new(\"name\", \"名称\"),\r\n    new(\"status\", \"状态\"),\r\n    new(\"remark\", \"备注\", visible: false),\r\n};\r\n\r\nvar editor = new ColumnDisplayEditor { ItemsSource = columns };\r\n\r\n// 业务侧监听即可获得排序 / 可见性变更（编辑器直接操作源集合）\r\ncolumns.CollectionChanged += (_, e) =>\r\n{\r\n    if (e.Action == NotifyCollectionChangedAction.Move)\r\n    {\r\n        Save(columns);   // 顺序已实时变更\r\n    }\r\n};\r\ncolumns[0].PropertyChanged += (_, e) =>\r\n{\r\n    if (e.PropertyName == nameof(ColumnDisplayItem.Visible))\r\n    {\r\n        Save(columns);\r\n    }\r\n};\r\n\r\n// 持久化恢复：按保存的 Key 顺序与 Visible 重建集合",
          "Xaml": "<!-- 拖拽排序直接 Move 源集合；Visible 绑定到 ColumnDisplayItem -->\r\n<codewf:ColumnDisplayEditor ItemsSource=\"{Binding Columns}\"\r\n                            VisibleHeaderText=\"显示\"\r\n                            DisplayTextHeaderText=\"列名\" />"
        },
        "CreateDefaultColumns": {
          "Name": "名称",
          "Type": "类型",
          "Status": "状态",
          "Owner": "负责人",
          "UpdatedAt": "更新时间",
          "Notes": "备注"
        },
        "OnColumnPropertyChanged": {
          "Visible": "显示",
          "Hidden": "隐藏",
          "VisibilityOfFormat": "[{0:HH:mm:ss}] 「{1}」可见性 → {2}"
        },
        "OnColumnsCollectionChanged": {
          "MovedToPositionFormat": "[{0:HH:mm:ss}] 「{1}」移动到位置 {2}"
        },
        "Reset": {
          "RestoredDefaultColumnsFormat": "[{0:HH:mm:ss}] 重置为默认列配置"
        }
      },
      "DataFlow": {
        "CSharpCode": {
          "DataGridEnableDefaultsItemsSourceColumnsMoveOldIndexNewIndexDataGrid": "// 1) 导入：一键打开三态排序 / 自然排序 / 智能 ToolTip\ndataGrid.EnableDefaults(itemsSource);\n\n// 2) 列配置：拖拽排序直接在源集合上移动\nColumns.Move(oldIndex, newIndex);\n\n// 3) 排序 / 过滤：三态循环，取消时恢复原始顺序\ndataGrid.AddNaturalSorting();\n\n// 4) 导出：DataTableExtensions 负责 Csv / Excel 落盘\ndataTable.ToExcel(\"order-2026.xlsx\");"
        },
        "XamlCode": {
          "DataGridCodewfDataGridEnhancementUseDefaultsTrueItemsSourceCodewfColumnDisplayEditor": "<!-- 表数据工作流：DataGrid 增强 + 列配置 + 导出 -->\n<DataGrid codewf:DataGridEnhancement.UseDefaults=\"True\"\n          ItemsSource=\"{Binding Rows}\" />\n\n<codewf:ColumnDisplayEditor Items=\"{Binding Columns}\" />"
        }
      },
      "DataGrid": {
        "View": {
          "TopEnhancedDataGridWithThreeStateAndNatural": "上：DataGridEnhancement.UseDefaults=True（三态排序 升→降→取消 / 自然排序 item2<item10 / 智能 ToolTip 仅对截断单元格生效）。下：原生 DataGrid 对照。两份相同数据，自行对比表头点击与悬停行为。",
          "EnhancedUseDefaults": "增强版（UseDefaults）",
          "Name": "名称",
          "Age": "年龄",
          "City": "城市",
          "NotesHoverToTrySmartToolTip": "备注（悬停体验智能 ToolTip）",
          "NativeComparison": "原生对照",
          "Notes": "备注"
        },
        "Properties": {
          "EnableDefaults": "一键增强",
          "UseDefaultsEnablesThreeStateAndNaturalSortingSmart": "上方网格的 UseDefaults 开关（三态排序 / 自然排序 / 智能 ToolTip / 全选）。",
          "RowHeight": "行高",
          "RowHeightOfTheEnhancedGrid": "增强版网格行高。"
        },
        "Source": {
          "CSharp": "// 等价的扩展方法形态，可按需开关单项能力\r\ndataGrid.EnableDefaults(dataGrid.ItemsSource as IEnumerable);\r\n// 等价于：\r\ndataGrid.AddSorting();            // 升 → 降 → 取消，取消时恢复原始顺序\r\ndataGrid.AddNaturalSorting();     // item2 排在 item10 之前\r\ndataGrid.EnableSmartTooltips();   // 仅对被截断的单元格显示 ToolTip，可传列索引\r\n\r\n// 附加属性 / 扩展方法二选一\r\nDataGridEnhancement.SetUseDefaults(dataGrid, true);\r\n\r\n// 性能预设（大数据量）\r\ndataGrid.ApplyPerformancePreset();",
          "Xaml": "<!-- 附加属性一键启用：三态排序 + 自然排序 + 智能 ToolTip + Ctrl+A 全选 -->\r\n<DataGrid codewf:DataGridEnhancement.UseDefaults=\"True\"\r\n          ItemsSource=\"{Binding Rows}\"\r\n          AutoGenerateColumns=\"False\" CanUserSortColumns=\"True\"\r\n          IsReadOnly=\"True\" HeadersVisibility=\"Column\">\r\n  <DataGrid.Columns>\r\n    <DataGridTextColumn Header=\"名称\" Binding=\"{Binding Name}\" Width=\"140\" />\r\n    <DataGridTextColumn Header=\"备注\" Binding=\"{Binding Comment}\" Width=\"*\" />\r\n  </DataGrid.Columns>\r\n</DataGrid>"
        },
        "CreateRows": {
          "Hangzhou": "杭州",
          "Beijing": "北京",
          "Seattle": "西雅图",
          "Chengdu": "成都",
          "Berlin": "柏林",
          "Shanghai": "上海",
          "Shenzhen": "深圳",
          "Guangzhou": "广州",
          "Nanjing": "南京",
          "Tokyo": "东京",
          "NoteThisTextIsDeliberatelyLongAndClipsFormat": "{0} 的备注信息第 {1} 行——这段文字足够长，会在列宽不足时被截断，",
          "SmartToolTipRevealsClippedContentOnHoverFully": "启用智能 ToolTip 后悬停自动显示完整内容，未截断的单元格不弹提示。"
        }
      },
      "DemoBootstrapper": {
        "Initialize": {
          "CodeWFAvaloniaStartedLogDirectory": "CodeWF.Avalonia 演示启动：日志目录 "
        }
      },
      "Dock": {
        "View": {
          "DockAvaloniaProvidesTheLayoutEngineDockSemiThemeSupplies": "Dock 工作区：Dock.Avalonia 布局引擎 + CodeWF DockSemiTheme（Semi 皮肤与中英文本地化）。可拖拽停靠、浮动、最大化；内容存放在 Document/Tool 的 Context，经 DataTemplate 解析为 MarkdownViewer。"
        },
        "Properties": {
          "Composition": "构成",
          "CodeWFAvaloniaDockSuppliesTheSemiAppearanceTrapezoid": "CodeWF.Avalonia.Dock 提供 Semi 皮肤、梯形文档页签边框与固定工具区行为；布局引擎来自 Dock.Avalonia。"
        },
        "Source": {
          "CSharp": "// App.Styles 中引入 Semi 皮肤（含 Dock 内置文案的 zh-cn / en-us 本地化）：\r\n//   <codewf:DockSemiTheme Locale=\"zh-CN\" />\r\n\r\nvar factory = new Dock.Model.ReactiveUI.Factory();\r\n\r\nvar document = new Document { Id = \"Doc.1\", Title = \"设计说明\", Content = markdown };\r\nvar docDock = new DocumentDock\r\n{\r\n    VisibleDockables = AvaloniaList<IDockable> [document],\r\n    ActiveDockable = document,\r\n    DocumentTemplate = markdownTemplate,   // 文档内容模板\r\n    CanCreateDocument = false,\r\n};\r\nvar mainDock = new ProportionalDock\r\n{\r\n    Orientation = Orientation.Horizontal,\r\n    VisibleDockables = AvaloniaList<IDockable>\r\n    [\r\n        toolDock, new ProportionalDockSplitter(), docDock\r\n    ],\r\n};\r\nvar layout = new RootDock { VisibleDockables = AvaloniaList<IDockable> [mainDock] };\r\nfactory.InitLayout(layout);",
          "Xaml": "<!-- 布局（RootDock 树）由 VM 构建，DockControl 只负责呈现与交互 -->\r\n<dock:DockControl xmlns:dock=\"using:Dock.Avalonia.Controls\"\r\n                  Layout=\"{Binding Layout}\" />"
        },
        "apiDocument": {
          "APIDraft": "接口草稿"
        },
        "Content": {
          "DockWorkspace": "Dock 工作区"
        },
        "designDocument": {
          "DesignNotes": "设计说明"
        },
        "DocApiMarkdown": {
          "APIDraftCsharpVarFactoryNewDockModel": "# 接口草稿\r\n\r\n```csharp\r\nvar factory = new Dock.Model.ReactiveUI.Factory();\r\nvar layout = new RootDock { VisibleDockables = [mainDock] };\r\nfactory.InitLayout(layout);\r\n```"
        },
        "DocDesignMarkdown": {
          "DesignNotesDockLayoutRootDockProportionalDockToolDockDocumentDock": "# 设计说明\r\n\r\nDock.Avalonia 的布局模型：**RootDock → ProportionalDock → ToolDock / DocumentDock**。\r\n\r\n| 概念 | 说明 |\r\n| --- | --- |\r\n| RootDock | 布局根，含隐藏停靠物集合 |\r\n| ProportionalDock | 比例分割容器 |\r\n| ToolDock | 工具停靠区（左 / 右 / 上 / 下） |\r\n| DocumentDock | 文档页签区 |\r\n\r\n本页内容存放在 Document.Context，由页面级 DataTemplate 解析为 MarkdownViewer。"
        },
        "overviewTool": {
          "WorkspaceOverview": "工作区概览"
        },
        "ToolOverviewMarkdown": {
          "WorkspaceOverviewLeftToolPanelRightDraggableDocuments": "### 工作区概览\r\n\r\n- 左侧：工具面板（本面板）\r\n- 右侧：文档区（可拖拽 / 停靠 / 浮动）\r\n- 页签：双击可最大化工作区\r\n\r\n试试拖动文档页签重新停靠。"
        }
      },
      "DocsFlow": {
        "CSharpCode": {
          "VarDocNewDocumentFactoryHideDockableToolFactory": "// 12.x 文档内容放 Context，由 DataTemplate 解析成视图\nvar doc = new Document\n{\n    Title = \"设计说明\",\n    Context = new DocContext(markdown),\n};\n\n// 面板显隐 / 恢复走同一套工厂方法，布局重置整体重建\nfactory.HideDockable(tool);\nfactory.RestoreDockable(tool.Id);"
        },
        "XamlCode": {
          "DockDockControlLayoutDockControlDataTemplatesDataTemplateDataTypeVm": "<!-- 多文档工作区：文档内容走 Context + DataTemplate -->\n<dock:DockControl Layout=\"{Binding Layout}\">\n  <DockControl.DataTemplates>\n    <DataTemplate DataType=\"vm:DocContext\">\n      <md:MarkdownViewer Markdown=\"{Binding Markdown}\" />\n    </DataTemplate>\n  </DockControl.DataTemplates>\n</dock:DockControl>"
        }
      },
      "Guide": {
        "View": {
          "StartGuide13Placements": "开始引导（13 方位演示）",
          "HighlightTheTargetIn13PlacementsWithEnter": "选择方位查看聚光效果。Enter 下一步，Esc 退出；拖动或缩放窗口时定位同步更新。",
          "Step1Center": "步骤 1 · Center",
          "IsPointAtCenterTrueOrNoTargetHighlightsTheScreen": "IsPointAtCenter=True 或缺省目标时聚光屏幕中心。",
          "Step2TopLeft": "步骤 2 · TopLeft",
          "TheBubbleAppearsAtTheUpperLeftOf": "气泡出现在目标的左上方。",
          "Step3TopRight": "步骤 3 · TopRight",
          "TheBubbleAppearsAtTheUpperRightOf": "气泡出现在目标的右上方。",
          "Step4Right": "步骤 4 · Right",
          "TheBubbleAppearsToTheRightOfThe": "气泡出现在目标的正右侧。",
          "Step5Bottom": "步骤 5 · Bottom",
          "TheBubbleAppearsBelowTheTarget": "气泡出现在目标的正下方。",
          "Step6LeftBottom": "步骤 6 · LeftBottom",
          "TheBubbleAppearsAtTheLowerLeftWith": "气泡出现在目标的左下方，本步骤使用 Primary 样式。",
          "Step7CustomMaskColor": "步骤 7 · 自定义主题色遮罩",
          "ThisStepDemonstratesATranslucentRedMaskColor": "本步骤使用半透明红色遮罩演示 MaskColor 自定义。",
          "Step8Complete": "步骤 8 · 完成",
          "FinalStepPressEnterOrClickFinishTo": "最后一步：按 Enter 或点击“完成”结束引导。"
        },
        "Properties": {
          "PlacementDetails": "方位说明",
          "EachStepSetsPlacement13PositionsCoverThe": "13 方位由每一步的 Placement 指定，覆盖全部气泡落位组合。"
        },
        "Source": {
          "CSharp": "var guide = new Guide\r\n{\r\n    IsArrowVisible = true,\r\n    MissingTargetBehavior = GuideMissingTargetBehavior.Center, // 缺省目标：居中/跳过/关闭\r\n};\r\nguide.Steps.Add(new GuideStep\r\n{\r\n    Title = \"第一步\",\r\n    Description = \"聚光目标控件\",\r\n    Target = someControl,\r\n    Placement = GuidePlacementMode.RightTop,\r\n    OpeningCommand = prepareCommand,     // 步骤打开前命令\r\n});\r\nguide.StepOpened += (_, e) => Logger.Info($\"进入第 {e.Index} 步\");\r\nguide.Completed += (_, _) => Logger.Info(\"引导完成\");\r\nguide.Show();      // 键盘导航：Enter 下一 / Esc 关闭\r\nguide.GoNext();\r\nguide.Close();",
          "Xaml": "<!-- 引导控件：Steps 为内容属性，目标用元素名绑定 -->\r\n<codewf:Guide IsArrowVisible=\"True\" Completed=\"OnGuideCompleted\">\r\n  <codewf:GuideStep Title=\"步骤 1\"\r\n                    Description=\"气泡出现在目标的右上方\"\r\n                    Target=\"{Binding #TargetControl}\"\r\n                    Placement=\"RightTop\" />\r\n  <codewf:GuideStep Title=\"步骤 2\"\r\n                    Description=\"Primary 样式 + 自定义遮罩色\"\r\n                    Target=\"{Binding #TargetControl2}\"\r\n                    Placement=\"Bottom\"\r\n                    StyleType=\"Primary\"\r\n                    MaskColor=\"#66B3261E\" />\r\n</codewf:Guide>"
        },
        "OnGuideCompleted": {
          "GuideTourCompleted": "Guide：引导完成"
        },
        "OnStartGuide": {
          "GuideStartingTour": "Guide：开始引导演示"
        },
        "OnStepOpened": {
          "GuideStepFormat": "Guide：第 {0}/{1} 步 · {2}"
        }
      },
      "Lang": {
        "View": {
          "I18nManagerUsesAPluggableLocalizationBackendTheDemo": "I18nManager：插件化国际化核心。演示台使用 Json 后端（输出目录 I18n\\\\ 递归扫描，含 Markdown 包分发的语言文件），整窗文案经 {codewf:I18n} 绑定，Culture 切换即时刷新——现在点击下面的按钮试试。",
          "SwitchToEnglish": "切换到 English",
          "SwitchToSimplifiedChinese": "切换到 简体中文",
          "UseSystemCulture": "按系统文化切换"
        },
        "ApplyCulture": {
          "LangChangingLanguage": "Lang：切换语言 → "
        },
        "Source": {
          "CSharp": "// 三种内置后端按需注册（同一时刻一个插件）\r\nI18nManager.Instance.Register(\r\n    new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, \"I18n\") },\r\n    new CultureInfo(\"zh-CN\"));            // 递归扫描目录内 *.json\r\n// I18nManager.Instance.Register(new XmlLangPlugin { ResourceFolder = folder }, culture);\r\n// I18nManager.Instance.Register(new ResxLangPlugin(typeof(Properties.Resources)), culture);\r\n\r\n// 追加程序集内嵌语言资源（如控件包分发的 JSON）\r\nI18nManager.Instance.AddResource(typeof(MainWindow).Assembly);\r\n\r\n// 运行时切换：全部 {codewf:I18n} 绑定自动刷新\r\nI18nManager.Instance.Culture = new CultureInfo(\"en-US\");\r\n\r\n// 直接取值与诊断\r\nvar text = I18nManager.Instance.GetResource(\"Demo.Lang.SampleGreeting\");\r\nvar languages = I18nManager.Instance.GetLanguages();\r\nvar diagnostics = ((JsonLangPlugin)plugin).LoadDiagnostics;",
          "Xaml": "<!-- 全部界面文案经 I18n 标记扩展绑定，文化切换即时刷新 -->\r\n<TextBlock Text=\"{codewf:I18n Demo.Lang.SampleGreeting}\" />\r\n<TextBlock Text=\"{codewf:I18n Demo.Lang.SampleFormat}\" />\r\n\r\n<!-- 带参数模板：{codewf:I18nBinding Hello, Name={Binding UserName}} -->"
        },
        "UpdateState": {
          "None": "无",
          "CurrentCultureFormat": "当前文化：{0} · ",
          "LoadedLanguagesFormat": "已加载语言：{0} · ",
          "ResourceDirectoryFormat": "资源目录：{0}"
        }
      },
      "Log": {
        "View": {
          "LogViewSubscribesToLoggerSLiveEventsWith": "LogView + Logger：控件订阅 Logger 的 UI 事件流（LogEventFeed）实时展示；支持级别过滤、最大条数、时间戳格式与右键菜单（复制 / 清空 / 打开日志目录）。演示台底部的日志面板也是本控件的实例。",
          "WriteBatchEventStreamLoadTest": "批量写入（压测事件流）"
        },
        "Properties": {
          "MinimumLevel": "最低显示级别",
          "Trace": "跟踪",
          "Debug": "调试",
          "Information": "消息",
          "Warning": "警告",
          "Error": "错误",
          "HideLogsBelowThisLevelMinimumLevel": "低于该级别的日志不显示（MinimumLevel）。",
          "MaximumEntries": "最大显示条数",
          "MaximumRetainedEntriesMaxDisplayCountOlderEntriesAreDropped": "控件保留的最大条目数（MaxDisplayCount），超出后滚动丢弃。"
        },
        "Source": {
          "CSharp": "// 事件流也可显式指定（多窗口场景共享同一数据源）\r\nlogView.Source = Logger.Events;\r\n\r\n// 写入各级别日志（微软 ILogger 语义：Trace → Critical）\r\nLogger.Info(\"服务启动完成\");\r\nLogger.Warn(\"磁盘空间不足\");\r\nLogger.Error(\"下游服务超时\");\r\n\r\n// 级别过滤（控件侧，只影响显示）\r\nlogView.MinimumLevel = LogLevel.Warning;\r\nlogView.MaxDisplayCount = 2000;\r\n\r\n// 右键菜单：复制 / 清空 / 打开日志目录（跨平台内联实现）",
          "Xaml": "<!-- 初始化 Logger 后，LogView 自动订阅其 UI 事件流 -->\r\nLogger.TryInitialize(new LoggerOptions\r\n{\r\n    File = new FileLogOptions { DirectoryPath = logDirectory },\r\n});\r\n\r\n<codewf:CodeWFLogTheme />   <!-- App.Styles：Light / Dark / HighContrast 三套日志配色 -->\r\n\r\n<codewf:LogView MinimumLevel=\"Information\"\r\n                MaxDisplayCount=\"2000\"\r\n                LogLineHeightMultiplier=\"1.5\" />"
        },
        "OnPropertyCommitted": {
          "Trace": "跟踪",
          "Debug": "调试",
          "Warning": "警告",
          "Error": "错误"
        },
        "OnWriteBurst": {
          "LoadTestLogEventThroughputAndDisplayThrottlingFormat": "压测日志 {0:000}：验证事件流吞吐与展示节流"
        },
        "OnWriteCritical": {
          "DemoCriticalLogDatabaseConnectionFailed": "演示：严重日志——数据库连接失败"
        },
        "OnWriteDebug": {
          "DemoDebugLogHiddenAtTheDefaultLevel": "演示：调试日志（默认级别下不显示）"
        },
        "OnWriteError": {
          "DemoErrorLogDownstreamTimeoutOf3000ms": "演示：错误日志——下游服务超时 3000ms"
        },
        "OnWriteInfo": {
          "DemoInformationLog": "演示：信息日志 "
        },
        "OnWriteTrace": {
          "DemoTraceLogHiddenAtTheDefaultLevel": "演示：跟踪日志（默认级别下不显示）"
        },
        "OnWriteWarning": {
          "DemoWarningLogLessThan10FreeDisk": "演示：警告日志——磁盘空间不足 10%"
        }
      },
      "Shell": {
        "ApplyAppearance": {
          "Dark": "深色",
          "Light": "浅色",
          "AppearanceFormat": "外观切换：{0} / {1}"
        },
        "OnGlobalPropertyCommitted": {
          "Dark": "深色"
        },
        "RunSmokeAsync": {
          "SmokeStarted": "冒烟开始",
          "SmokePassed": "冒烟通过",
          "SmokeFailed": "冒烟失败"
        },
        "SyncGlobalItems": {
          "Dark": "深色",
          "Light": "浅色"
        },
        "CommitRename": {
          "RestoredOriginalNameFormat": "恢复原名：{0}",
          "RenamedFormat": "重命名：{0} → {1}"
        },
        "KindLabel": {
          "Documents": "文档",
          "Commands": "命令"
        },
        "LoadLayoutFrom": {
          "LayoutRestored": "已恢复布局：",
          "FailedToRestoreLayout": "恢复布局失败："
        },
        "SaveLayoutTo": {
          "FailedToSaveLayout": "保存布局失败："
        }
      },
      "MarkdownEditor": {
        "View": {
          "SourceEditorPoweredByAvaloniaEditWithTextMateHighlighting": "MarkdownEditorView：AvaloniaEdit 承载的源码编辑器，内置语法着色（TextMate）与约 40 项编辑动作（标题 / 列表 / 表格 / 代码块 / 数学块等）。下方为常用动作按钮，完整动作集见 MarkdownEditorAction 枚举。",
          "Bold": "加粗",
          "Italic": "斜体",
          "InlineCode": "行内代码",
          "Heading2": "标题 2",
          "Table": "表格",
          "TaskList": "任务列表",
          "CodeBlock": "代码块",
          "Quote": "引用"
        },
        "Properties": {
          "EditorFontSize": "编辑器字号",
          "EditorTextFontSize": "编辑器文本字号。",
          "ShowLineNumbers": "显示行号",
          "ShowLineNumbersToTheLeftOfThe": "是否在编辑器左侧显示行号。",
          "AutoPairing": "自动配对",
          "AutomaticallyCompletePairedMarkersSuchAsAndBackticks": "输入 **、` 等成对标记时自动补全后半个标记。"
        },
        "Source": {
          "CSharp": "var editor = new MarkdownEditorView(new MarkdownEditorOptions\r\n{\r\n    LinkUrlPlaceholder = \"https://example.com\",\r\n    CodeFenceLanguage = \"csharp\",\r\n    EnableAutoPair = true,\r\n    ShowLineNumbers = true,\r\n    FontSize = 15,\r\n});\r\nEditorHost.Content = editor;\r\neditor.SetText(\"# 标题\\n\\n正文\");\r\neditor.MarkdownChanged += (_, text) => Save(text);\r\n\r\n// 40 项动作统一入口：MarkdownEditorAction 枚举\r\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\r\nawait editor.ExecuteAsync(MarkdownEditorAction.Table);\r\neditor.InsertTable(\"| 列一 | 列二 |\\\\n| --- | --- |\\\\n| A | B |\");\r\neditor.WrapSelection(\"**\", \"**\");\r\n\r\n// 光标 / 选区 / 行数\r\nvar (line, column, lineCount) = editor.CaretPosition;\r\nvar text = editor.Text;",
          "Xaml": "<!-- MarkdownEditorView 为纯代码构建控件（无 XAML 模板依赖），在代码中创建并放入容器 -->\r\n<Border x:Name=\"EditorHost\" />"
        },
        "OnTable": {
          "Column1Column2AB": "| 列一 | 列二 |\n| --- | --- |\n| A | B |"
        },
        "Samples": {
          "Markdown": "# MarkdownEditorView\r\n\r\nAvaloniaEdit 驱动的 Markdown 源码编辑器。\r\n\r\n## 内置能力\r\n- TextMate 语法着色\r\n- 标题 / 列表 / 表格 / 任务列表 / 代码块 / 数学块等约 40 项动作\r\n- 查找替换、自动配对、行号、字号\r\n\r\n```csharp\r\nvar editor = new MarkdownEditorView();\r\n```\r\n\r\n> 用上方按钮或快捷键体验编辑动作；修改会实时反映在状态栏。"
        },
        "UpdateStatus": {
          "LineColumnLinesCharactersFormat": "行 {0} / 列 {1} · 共 {2} 行 · {3} 字符"
        }
      },
      "MarkdownExport": {
        "View": {
          "ExportMarkdownAsPNGPDFWordOrHTML": "MarkdownDocumentExporter：把 Markdown 导出为 PNG / PDF / Word / HTML；MarkdownHtmlClipboard 提供公众号（微信 / 知乎 / 掘金）富文本复制。导出排版主题可在右侧属性面板选择。",
          "ExportPNG": "导出 PNG",
          "ExportPDF": "导出 PDF",
          "ExportWord": "导出 Word",
          "ExportHTML": "导出 HTML",
          "CopyPublishingHTML": "复制公众号 HTML"
        },
        "Properties": {
          "ExportTheme": "导出主题",
          "TypographyThemeOfTheExportedDocument": "导出文档使用的排版主题。",
          "TypographySize": "排版尺寸",
          "TypographySizeOfTheExportedDocument": "导出排版尺寸。",
          "CopyDestination": "复制目标",
          "WeChat": "微信",
          "Zhihu": "知乎",
          "Juejin": "掘金",
          "DestinationPlatformForRichText": "公众号富文本复制的目标平台。"
        },
        "copyKind": {
          "WeChat": "微信"
        },
        "Source": {
          "CSharp": "// 四种格式一条龙：MarkdownDocumentExporter（themeName / typographySize 可选）\r\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, @\"D:\\\\out\\\\doc.png\");\r\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Pdf, @\"D:\\\\out\\\\doc.pdf\");\r\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Word, @\"D:\\\\out\\\\doc.docx\");\r\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Html, @\"D:\\\\out\\\\doc.html\",\r\n    themeName: MarkdownTypographyThemes.WeChatFormat);\r\n\r\n// 公众号富文本复制（微信 / 知乎 / 掘金），写入系统剪贴板后可直接粘贴\r\nvar ok = await MarkdownHtmlClipboard.TrySetHtmlAsync(\r\n    markdown, \"公众号\", MarkdownTypographyThemes.WeChatFormat);\r\n\r\n// 存储选择器拿到本地路径后再导出\r\nvar file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions\r\n{\r\n    Title = \"导出 PNG\",\r\n    DefaultExtension = \".png\",\r\n});\r\nif (file is not null)\r\n{\r\n    MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, file.Path.LocalPath);\r\n}",
          "Xaml": "<!-- 导出按钮 + 存储选择器，导出实现见 C# 段 -->\r\n<Button Content=\"导出 PNG\" Click=\"OnExportPng\" />\r\n<Button Content=\"复制公众号 HTML\" Click=\"OnCopyWechat\" />"
        },
        "ExportAsync": {
          "ExportingFormat": "正在导出 {0} → {1}",
          "ExportCompleteFormat": "导出完成：{0}",
          "ExportFailedFormat": "导出失败：{0}"
        },
        "OnCopyWechat": {
          "Zhihu": "知乎",
          "Juejin": "掘金",
          "CopiedRichTextPasteItIntoTheEditorFormat": "已复制 {0} 富文本，可直接粘贴到编辑器。",
          "CopyFailed": "复制失败："
        },
        "OnExportHtml": {
          "ExportHTML": "导出 HTML"
        },
        "OnExportPdf": {
          "ExportPDF": "导出 PDF"
        },
        "OnExportPng": {
          "ExportPNG": "导出 PNG"
        },
        "OnExportWord": {
          "ExportWord": "导出 Word"
        },
        "Samples": {
          "Markdown": "# 导出示例文档\r\n\r\n这份文档用于演示 **PNG / PDF / Word / HTML** 四种导出与公众号富文本复制。\r\n\r\n## 特性清单\r\n- 按排版主题渲染导出样式\r\n- PDF 支持页面尺寸与边距选项\r\n- 公众号 HTML 兼容微信编辑器粘贴\r\n\r\n## 表格\r\n\r\n| 格式 | 说明 |\r\n| --- | --- |\r\n| PNG | 位图，适合预览分享 |\r\n| PDF | 打印与归档 |\r\n| Word | 二次编辑 |\r\n| HTML | 发布到公众号 / 站点 |\r\n\r\n```csharp\r\nMarkdownDocumentExporter.ExportMarkdown(md, ExportKind.Pdf, path);\r\n```\r\n\r\n> 提示：导出主题可在右侧属性面板切换后重试。"
        }
      },
      "MarkdownFlow": {
        "CSharpCode": {
          "AddStepDemoSceneStepOpenNewMarkdownViewerPageAddStep": "// 场景页把「打开 → 编辑 → 预览 → 导出 → 回写」编成一条工作流，\n// 每一步直接复用组件库里的同一个页面实例（不重复实现）：\nAddStep(\"Demo.Scene.Step.Open\", new MarkdownViewerPage());\nAddStep(\"Demo.Scene.Step.Edit\", new MarkdownEditorPage());\nAddStep(\"Demo.Scene.Step.Preview\", new MarkdownLiveEditorPage());\nAddStep(\"Demo.Scene.Step.Export\", new MarkdownExportPage());\nAddStep(\"Demo.Scene.Step.WriteBack\", new MarkdownLitePage());\nAttachSceneShell();\n\n// 任务勾选回写源文由 MarkdownViewer 发起：\nviewer.TaskWriteBackRequested += (_, e) => e.Result.Markdown = e.Rendered;"
        },
        "XamlCode": {
          "CodewfMarkdownViewerMarkdownTypographyThemeCodewfMarkdownEditorViewMarkdownShowLineNumbers": "<!-- 场景页外壳由 ScenePageBase 构建：工作流步骤条 + 内嵌组件页 -->\n<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\n                       TypographyTheme=\"{Binding Theme}\" />\n<codewf:MarkdownEditorView Markdown=\"{Binding Source}\" ShowLineNumbers=\"True\" />\n<codewf:MarkdownLiveEditorView Markdown=\"{Binding Doc}\" />"
        }
      },
      "MarkdownLite": {
        "View": {
          "MarkdownLiteUsesMarkdigAndASmallerRendering": "CodeWF.Avalonia.Markdown.Lite：Markdig 解析 + 自研渲染管线，只渲染常规元素——标题 / 段落 / 列表 / 引用 / 表格 / 链接 / 代码块（单色）/ 图片替代文本 / 公式原文，内置同款 18 套排版主题，体积与依赖更小。"
        },
        "Properties": {
          "TypographyTheme": "排版主题",
          "LiteIncludesTheSame18TypographyThemesFour": "Lite 内置同款 18 套排版主题，此处列出常用四个。",
          "TypographySize": "排版尺寸"
        },
        "Source": {
          "CSharp": "// Lite 包：只带渲染管线与 18 套排版主题（无编辑器 / 导出 / 数学 / Mermaid / TextMate 高亮）\r\n// 完整包：在其上叠加图片（GIF/SVG）、公式、图表、代码高亮、PNG/PDF/Word/公众号导出\r\n//\r\n// 下方示例演示降级行为：图片退化为替代文本、公式保留原文、代码块单色。\r\nvar viewer = new MarkdownViewer\r\n{\r\n    Markdown = \"# Lite 示例\",\r\n    TypographyTheme = MarkdownTypographyThemes.Simple,\r\n    EnableVirtualization = true,\r\n};",
          "Xaml": "<!-- Lite 与完整包的渲染控件同名同命名空间，换包不换代码 -->\r\n<PackageReference Include=\"CodeWF.Avalonia.Markdown.Lite\" />\r\n\r\n<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\r\n                       TypographyTheme=\"Basic\"\r\n                       TypographySize=\"Normal\" />"
        },
        "Samples": {
          "Markdown": "# Markdown Lite 渲染范围\r\n\r\n本页内容刻意只使用 **Lite 支持的元素**：标题、**加粗**、*斜体*、`行内代码`、[链接](https://codewf.com)、列表、表格与代码块。\r\n\r\n## 列表\r\n- 基础块级元素\r\n- 表格与引用\r\n- 任务列表勾选回写同样可用\r\n\r\n> 引用块：Lite 与完整包共用同一渲染管线。\r\n\r\n## 降级演示（完整包能力在 Lite 中的表现）\r\n\r\n| 元素 | Lite 行为 |\r\n| --- | --- |\r\n| 图片 | 显示替代文本 |\r\n| 数学公式 | 显示 LaTeX 原文 |\r\n| 代码高亮 | 单色渲染 |\r\n\r\n```csharp\r\n// 代码块单色渲染（完整包为 TextMate 高亮）\r\nConsole.WriteLine(\"Hello, Markdown Lite!\");\r\n```\r\n\r\n- [x] 使用 Lite 渲染\r\n- [ ] 切换排版主题（右侧属性面板）"
        }
      },
      "MarkdownLiveEditor": {
        "View": {
          "MarkdownLiveEditorViewEditsBlockModelsDirectlyHeadingsParagraphsLists": "MarkdownLiveEditorView：单栏所见即所得编辑。文档按块（标题 / 段落 / 列表 / 表格 / 代码块）建模，编辑时直接在块上进行，所见即所得；写作体验与渲染结果一致。",
          "InsertText": "插入文本",
          "FocusFirstBlock": "聚焦首块",
          "Bold": "加粗",
          "Table": "表格"
        },
        "Properties": {
          "TypographyTheme": "排版主题",
          "TypographyThemeUsedByTheLiveEditor": "所见即所得视图应用的排版主题。",
          "EditorFontSize": "编辑器字号",
          "FontSizeOfTheBlockEditor": "块编辑器文本字号。"
        },
        "Source": {
          "CSharp": "var editor = new MarkdownLiveEditorView(new MarkdownEditorOptions\r\n{\r\n    EnableAutoPair = true,\r\n    FontSize = 15,\r\n});\r\neditor.TypographyTheme = MarkdownTypographyThemes.Verdrant;  // 所见即所得的排版主题\r\neditor.SetText(\"# 实时编辑\\\\n\\\\n直接在块上编辑\");\r\nEditorHost.Content = editor;\r\n\r\neditor.MarkdownChanged += (_, text) => Save(text);\r\neditor.FocusBlock(0);                       // 聚焦指定块\r\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\r\neditor.InsertText(\"追加文本\");\r\n\r\n// 块模型指标\r\nvar blockCount = editor.BlockCount;\r\nvar activeBlock = editor.ActiveBlockIndex;\r\nvar markdown = editor.Text;                 // = MarkdownBlockParser.Write(models)",
          "Xaml": "<!-- MarkdownLiveEditorView 为纯代码构建控件，在代码中创建并放入容器 -->\r\n<Border x:Name=\"EditorHost\" />"
        },
        "OnInsert": {
          "ContentAddedWithInsertText": "\n\n新插入的内容（InsertText）。"
        },
        "Samples": {
          "Markdown": "# 所见即所得\r\n\r\n直接在块上编辑：标题、段落、列表、表格都是可编辑块。\r\n\r\n## 列表\r\n- 块级建模（MarkdownBlockModel）\r\n- 编辑即渲染\r\n- 无左右分栏\r\n\r\n> 点击任意块开始编辑；顶部按钮演示插入与聚焦。\r\n\r\n| 能力 | 说明 |\r\n| --- | --- |\r\n| FocusBlock | 聚焦指定块 |\r\n| ApplyExternalEdit | 外部回写 |"
        },
        "UpdateStatus": {
          "BlocksActiveBlockCharactersFormat": "块数 {0} · 活动块索引 {1} · {2} 字符"
        }
      },
      "MarkdownViewer": {
        "View": {
          "EditMarkdownSource": "编辑 Markdown 源文本",
          "Apply": "应用",
          "AppendParagraphIncremental": "追加段落（增量）",
          "ResetSample": "重置示例"
        },
        "Properties": {
          "TypographyTheme": "排版主题",
          "18TypographyThemesChangesRenderImmediately": "18 套排版主题，提交后立即重渲染。",
          "TypographySize": "排版尺寸",
          "NormalSmallSmallUsesCompactTypography": "Normal / Small，Small 为紧凑排版。",
          "Virtualization": "虚拟化",
          "RealizeVisibleBlocksAboveTheThresholdToReduce": "超过阈值后按窗口实现块级内容，长文档显存占用更低。",
          "VirtualizationThreshold": "虚拟化阈值",
          "EnableVirtualizationAboveThisBlockCount": "块数超过该值才启用虚拟化。"
        },
        "Source": {
          "CSharp": "// 主题切换：单控件覆盖\r\nviewer.TypographyTheme = MarkdownTypographyThemes.GeekBlack;\r\nviewer.TypographySize = MarkdownTypographySizes.Small;\r\n\r\n// 应用级全局覆盖\r\nMarkdownThemes.OverrideTypographyResources(Application.Current, MarkdownTypographyThemes.Verdant);\r\n\r\n// 渲染指标与任务回写\r\nviewer.TaskWriteBackRequested += (_, e) =>\r\n    editor.ApplyExternalEdit(e.Markdown);   // 勾选任务后同步回编辑器\r\nvar parseMs = viewer.LastParseDuration.TotalMilliseconds;\r\nvar mode = viewer.LastRenderMode;           // Incremental / Full\r\nvar realized = viewer.RealizedBlockCount;",
          "Xaml": "<!-- 18 套排版主题、增量渲染、块级虚拟化、任务勾选回写 -->\r\n<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\r\n                       TypographyTheme=\"{Binding Theme}\"\r\n                       TypographySize=\"{Binding Size}\"\r\n                       EnableVirtualization=\"True\"\r\n                       VirtualizationThreshold=\"40\" />"
        },
        "UpdateMetrics": {
          "ModeParsingMsRealizedBlocksFormat": "渲染模式：{0} · 解析耗时：{1:F1} ms · 已实现块：{2}"
        },
        "AppendParagraph": {
          "NewParagraphIncrementalRenderingReparsesAndLaysOut": "\n\n新增段落：增量渲染只解析与重排变更块（"
        },
        "Samples": {
          "Markdown": "# MarkdownViewer 演示\r\n\r\n完整渲染管线：**Markdig 解析** → 块级模型 → `WindowedStackPanel` 虚拟化呈现。\r\n\r\n## 任务回写\r\n勾选下面的任务，Markdown 源文本会同步更新（TaskWriteBackRequested）：\r\n\r\n- [x] 解析 Markdown 文本\r\n- [ ] 调整排版主题（右侧属性面板）\r\n- [ ] 展开输入参数，修改源文本后点击「应用」，观察增量渲染耗时\r\n\r\n## 表格与代码\r\n\r\n| 能力 | 说明 |\r\n| --- | --- |\r\n| 增量渲染 | 只重排变更块 |\r\n| 虚拟化 | 超过阈值后按窗口实现块 |\r\n\r\n```csharp\r\nvar viewer = new MarkdownViewer { Markdown = \"# 你好\" };\r\n```\r\n\r\n> 提示：右侧 PropertyGrid 即本控件的兄弟实例——属性面板自身也是一次自我演示。"
        }
      },
      "MarkupExtension": {
        "View": {
          "IfSwitchMarkupExtensionsEvaluateConditionalAndMulti": "If / Switch 标记扩展：在 XAML 里直接做条件取值与多分支取值，条件与分支支持绑定与常量混合，省去为纯展示逻辑写转换器。切换下方开关与状态，观察文本即时变化。",
          "TaskRunningPleaseWait": "任务执行中，请稍候…",
          "SystemIdle": "系统空闲",
          "UnknownState": "未知状态",
          "Running": "运行中",
          "ServiceRunning": "● 服务运行中",
          "Paused": "已暂停",
          "ServicePaused": "‖ 服务已暂停",
          "Maintenance": "维护中",
          "MaintenanceWindowOpen": "🛠 维护窗口开启"
        },
        "Properties": {
          "UseCases": "适用场景",
          "ConditionalDisplayTextAndIcons": "纯展示型条件文案 / 图标切换",
          "IfSwitchChooseDisplayValuesKeepMultiProperty": "If / Switch 只解决“取值”；需要联动多属性或命令时仍建议在 VM 中处理。"
        },
        "CSharpCode": {
          "SwitchBindingIfConverterIfConditionConverterSwitchConverterEnumToBooleanConverterIsCollectionEmptyConverter": "// If / Switch 均有绑定形态（多绑定），供条件与分支都来自数据时使用：\r\n//   {codewf:IfBinding ...} / SwitchBinding\r\n//\r\n// 配套转换器可独立使用（CodeWF.Avalonia.Controls.Converters）：\r\n//   IfConverter / IfConditionConverter / SwitchConverter\r\n//   EnumToBooleanConverter / IsCollectionEmptyConverter"
        },
        "Source": {
          "Xaml": "<!-- If：条件取值，条件/分支支持绑定与常量混合 -->\r\n<TextBlock.Text>\r\n  <codewf:If Condition=\"{Binding IsBusy}\"\r\n             True=\"任务执行中，请稍候…\"\r\n             False=\"系统空闲\" />\r\n</TextBlock.Text>\r\n\r\n<!-- Switch：多分支取值，未命中走 Default -->\r\n<TextBlock.Text>\r\n  <codewf:Switch Value=\"{Binding State}\" Default=\"未知状态\">\r\n    <codewf:SwitchCase When=\"运行中\" Then=\"● 服务运行中\" />\r\n    <codewf:SwitchCase When=\"已暂停\" Then=\"‖ 服务已暂停\" />\r\n  </codewf:Switch>\r\n</TextBlock.Text>"
        },
        "state": {
          "Running": "运行中"
        },
        "StateOptions": {
          "Running": "运行中",
          "Paused": "已暂停",
          "Maintenance": "维护中",
          "Unknown": "未知"
        }
      },
      "PropertyGrid": {
        "View": {
          "PropertyGridDisplaysACompleteValvePropertyTreeTry": "PropertyGrid 自我演示：右侧属性面板即本控件；此处再放一个实例承载完整的阀门属性树。试试：搜索过滤、分类/字母视图、修改值出现加粗与子树重置、右键行重置单项、拖拽名称列宽。",
          "ExpandAll": "全部展开",
          "CollapseAll": "全部折叠",
          "ResetAllChanges": "重置全部修改",
          "ValueChanges": "值变更日志",
          "BrowseRequests": "浏览请求"
        },
        "Properties": {
          "ViewMode": "视图模式",
          "Categorized": "分类",
          "Alphabetical": "字母",
          "CategorizedViewGroupsByCategoryAlphabeticalViewFlattens": "分类视图按 Category 分组；字母视图平铺并按名称排序。",
          "ShowToolbar": "显示工具栏",
          "ToolbarForViewSwitchingExpandCollapseAndReset": "顶部工具栏（视图切换 / 展开 / 折叠 / 重置）。",
          "ShowDescriptions": "显示描述面板",
          "ShowTheSelectedPropertySDescriptionBelowThe": "底部描述面板，显示选中项的说明。",
          "NameColumnWidth": "名称列宽",
          "InitialNameColumnWidthDragTheSeparatorTo": "名称列初始宽度，运行中也可直接拖拽分隔条。"
        },
        "Source": {
          "CSharp": "// 数据模型是纯 POCO：PropertyItem 树（Category → 子项/子分组）\r\nvar name = new PropertyItem { Id = \"n\", Name = \"名称\", Kind = PropertyItemKind.Text, Value = \"阀门-01\" };\r\nvar visible = new PropertyItem { Id = \"v\", Name = \"可见\", Kind = PropertyItemKind.Boolean, Value = true };\r\nvar appearance = new PropertyItem\r\n{\r\n    Id = \"g1\", Name = \"外观\", Kind = PropertyItemKind.Category,\r\n    Children = [name, visible],\r\n};\r\n\r\n// 摘要链与动态可见性\r\nposition.SummaryProvider = () => $\"{posX.Value}, {posY.Value}\";\r\nbackoffFactor.VisibleWhen = () => Equals(backoff.Value, true);\r\n\r\n// 控件能力\r\ngrid.ExpandAll();\r\ngrid.CollapseAll();\r\ngrid.ResetAll();\r\ngrid.Select(item);\r\ngrid.CommitValue(item, newValue);      // 程序化提交（触发加粗与摘要刷新）\r\ngrid.ValueCommitted += (_, e) => Log(e.Item.Name, e.OldValue, e.NewValue);\r\n\r\n// 11 种编辑器：Text/MultilineText/Number/Boolean/Choice/Radio/Percent/Color/Path/Button/DateTime/ReadOnly",
          "Xaml": "<!-- 分类/字母视图、搜索过滤、描述面板、修改加粗、列宽拖拽 -->\r\n<codewf:PropertyGrid Items=\"{Binding Items}\"\r\n                     ViewMode=\"Categorized\"\r\n                     ShowToolBar=\"True\"\r\n                     ShowDescriptionPane=\"True\"\r\n                     SearchText=\"{Binding SearchText}\"\r\n                     ValueCommittedCommand=\"{Binding ValueCommittedCommand}\"\r\n                     BrowseRequestedCommand=\"{Binding BrowseRequestedCommand}\" />"
        },
        "OnPropertyCommitted": {
          "Alphabetical": "字母"
        }
      },
      "PropertyGridSample": {
        "Sample": {
          "Name": "名称",
          "Valve01": "阀门-01",
          "DisplayNameUsedOnTheCanvasAndIn": "节点的显示名称，同时显示在画布与搜索结果中。",
          "Visible": "可见",
          "HideTheNodeOnTheCanvasWhileKeeping": "关闭后节点在画布中隐藏，但仍参与计算与导出。",
          "FillColor": "填充颜色",
          "NodeFillColorInRRGGBBFormat": "节点主体的填充颜色，支持 #RRGGBB 格式。",
          "Opacity": "不透明度",
          "NodeOpacityDragToPreviewAndReleaseTo": "节点整体不透明度，拖动滑块实时预览，松开后提交。",
          "FontFamily": "字体族",
          "MicrosoftYaHei": "微软雅黑",
          "SimSun": "宋体",
          "FontFamilyUsedToRenderText": "渲染文本使用的字体族。",
          "FontSize": "字号",
          "TextFontSize": "文本的字号大小。",
          "Weight": "粗细",
          "Light": "细体",
          "Regular": "常规",
          "Bold": "粗体",
          "FontWeight": "字体的粗细样式。",
          "Italic": "斜体",
          "RenderTextInItalic": "是否以斜体渲染文本。",
          "Font": "字体",
          "GlobalTextFontSettings": "文本的全局字体设置。",
          "ItalicSummary": " 斜体",
          "Appearance": "外观",
          "NodeAppearanceProperties": "节点的外观属性。",
          "HorizontalNodeCoordinateOnTheCanvas": "节点在画布中的横坐标。",
          "VerticalNodeCoordinateOnTheCanvas": "节点在画布中的纵坐标。",
          "Position": "位置",
          "NodePositionOnTheCanvas": "节点在画布中的位置。",
          "Width": "宽",
          "NodeWidth": "节点宽度。",
          "High": "高",
          "NodeHeight": "节点高度。",
          "KeepAspectRatio": "保持比例",
          "LockTheAspectRatioWhileResizing": "调整时锁定宽高比。",
          "Size": "尺寸",
          "NodeDimensionsAndConstraints": "节点尺寸与约束。",
          "Rotation": "旋转",
          "RotationAroundTheAnchorPositiveValuesAreCounterclockwise": "绕旋转中心的旋转角度，逆时针为正。",
          "CenterX": "中心 X",
          "HorizontalRotationAnchorAsAPercentageOfWidth": "旋转中心的横坐标（占宽度百分比）。",
          "CenterY": "中心 Y",
          "VerticalRotationAnchorAsAPercentageOfHeight": "旋转中心的纵坐标（占高度百分比）。",
          "RotationAnchor": "旋转中心",
          "RotationAnchorInPercentageCoordinates": "旋转锚点，取值为中心的百分比坐标。",
          "Layout": "布局",
          "IncludeInComputation": "参与计算",
          "PreviewOnly": "仅预览",
          "Exclude": "不参与",
          "IncludeTheNodeInSimulationAndResultExport": "该节点是否参与仿真计算与结果导出。",
          "Trigger": "触发方式",
          "Manual": "手动",
          "Automatic": "自动",
          "Scheduled": "定时",
          "TriggerThatStartsTheNode": "节点开始执行的触发方式。",
          "Priority": "优先级",
          "Medium": "中",
          "Low": "低",
          "ExecutionOrderWithinASchedulingCycle": "同一调度周期内的执行先后顺序。",
          "MaximumRetries": "最大重试次数",
          "Retries": "次",
          "MaximumAutomaticRetriesAfterFailure": "失败后最大的自动重试次数。",
          "RetryInterval": "重试间隔",
          "DelayBetweenRetries": "两次重试之间的等待时间。",
          "ExponentialBackoff": "指数退避",
          "MultiplyRetryDelaysByThisFactorToAvoid": "重试间隔按系数逐次放大，避免雪崩。",
          "BackoffFactor": "退避系数",
          "MultiplierAppliedToEachDelayWithExponentialBackoff": "每次重试间隔的放大倍数（仅指数退避时可用）。",
          "RetryPolicy": "重试策略",
          "AutomaticRetryPolicyAfterExecutionFailure": "执行失败后的自动重试策略。",
          "RetriesMsFormat": "{0} 次 / {1}ms",
          "Note": "便签",
          "PeriodicallyCheckThatInletPressureMeetsOperatingRequirements": "定期检查入口压力是否满足工况要求。",
          "AnnotationSavedWithTheModelExcludedFromComputation": "仅用于标注说明，不参与计算，随模型一并保存。",
          "Behavior": "行为",
          "FullPath": "完整路径",
          "AbsoluteModelFilePathReadOnly": "模型文件的绝对路径（只读）。",
          "Encoding": "编码",
          "DocumentCharacterEncoding": "文档的字符编码。",
          "OutputDirectory": "输出目录",
          "SimulationResultDirectory": "仿真结果的输出目录。",
          "StyleSheet": "样式表",
          "BrowseStyleSheet": "浏览样式表文件…",
          "StyleSheetUsedInTheNodeReportClick": "节点在报告中使用的样式表文件，点击选择文件。",
          "ID": "编号",
          "UniquePortIdentifier": "端口的唯一编号。",
          "Type": "类型",
          "Inlet": "进水",
          "Outlet": "出水",
          "Drain": "排污",
          "DirectionOfFlowThroughThePort": "端口的介质方向。",
          "Diameter": "直径",
          "NominalPortDiameter": "端口公称直径。",
          "ConfigurationForFormat": "{0} 的配置。",
          "Ports": "端口列表",
          "NodePortsExpandToConfigureEachPort": "节点的端口集合，展开查看各端口配置。",
          "Inlet1": "进口 #1",
          "Outlet1": "出口 #1",
          "DrainPort": "排污口",
          "ItemsFormat": "{0} 项",
          "File": "文件"
        },
        "Content": {
          "BrowseFormat": "[{0:HH:mm:ss}] 浏览：{1}"
        },
        "Describe": {
          "Empty": "（空）"
        }
      },
      "ScenePageBase": {
        "GoToStep": {
          "WorkflowStepFormat": "工作流步骤 {0}/{1}：{2}"
        }
      },
      "SearchListBox": {
        "View": {
          "InteractionLog": "交互日志",
          "Hint": "提示",
          "SearchHasA400msDebounceUseCtrlShift": "顶部搜索框 400ms 防抖；按 Ctrl/Shift 多选；双击条目触发事件（右侧记录）。"
        },
        "Properties": {
          "Total": "总数",
          "NumberOfSourceItemsTotalCount": "数据源总条目数（TotalCount）。",
          "Filtered": "过滤结果",
          "MatchingItemsSearchCountWithABuiltIn400ms": "当前搜索过滤后的条目数（SearchCount），控件内置 400ms 防抖。",
          "SelectedItems": "选中项",
          "NumberOfSelectedItemsSupportsCtrlShiftSelection": "当前多选选中数量（Ctrl / Shift 多选）。"
        },
        "Content": {
          "DoubleClickFormat": "双击：{0}",
          "SelectionChangedItemsFormat": "选区变化：{0} 项"
        },
        "Source": {
          "CSharp": "var list = new SearchListBox { ItemsSource = items };\r\n\r\n// 计数条指标\r\nvar total = list.TotalCount;        // 数据源总数\r\nvar visible = list.SearchCount;     // 当前过滤结果数\r\n\r\n// 多选（Ctrl / Shift）与事件\r\nvar selected = list.SelectedItems;  // List<object?>\r\nlist.ListSelectionChanged += (item, args) => Refresh(args.Source.SelectedItems);\r\nlist.ListItemDoubleTapped += (item, args) => Open(item);\r\n\r\n// 批量增删（直接作用到源集合）\r\nlist.AddRange(newItems);\r\nlist.RemoveRange(selected);",
          "Xaml": "<!-- 内置搜索框（400ms 防抖）、多选、计数条；条目文本按 item.ToString() 匹配 -->\r\n<codewf:SearchListBox ItemsSource=\"{Binding Items}\" />"
        },
        "UpdateMetrics": {
          "TotalFilteredSelectedFormat": "总数 {0} · 过滤结果 {1} · 选中 {2}"
        },
        "CreateItems": {
          "Audio": "音频",
          "Videos": "视频",
          "Documents": "文档",
          "Images": "图片",
          "Archives": "压缩包",
          "AssetFormat": "素材 {0:000}"
        }
      },
      "StatusComponents": {
        "View": {
          "StatusBadgeAndStatusCardProvideFiveSemanticStatesAnd": "StatusBadge / StatusCard：内置五档语义状态（Debug / Info / Warn / Error / Fatal）与浅深换肤；徽标为左签 + 右值，卡片为图标 + 两行文案。下方最后一组卡片由右侧属性面板驱动。",
          "Debug": "调试",
          "Information": "信息",
          "Warning": "警告",
          "Error": "错误",
          "Critical": "严重",
          "Debugging": "断点调试进行中",
          "ServiceReady": "服务已就绪",
          "DiskSpaceLow": "磁盘空间不足",
          "ServiceTimeout": "下游服务超时",
          "DatabaseOffline": "数据库连接失败",
          "DrivenByPropertyPanel": "属性面板驱动",
          "ChangeTheStatusAndIconWidthInThe": "在右侧属性面板切换状态与图标宽度"
        },
        "Properties": {
          "CardStatus": "卡片状态",
          "Debug": "调试",
          "Information": "信息",
          "Warning": "警告",
          "Error": "错误",
          "Critical": "严重",
          "FiveStatusLabelKindValuesChangeTheCardStateAnd": "StatusLabelKind 五档语义状态，切换时卡片伪类与配色同步变化。",
          "IconWidth": "图标宽度",
          "IconWidthOfTheConfigurableCard": "驱动卡片的图标宽度。"
        },
        "Source": {
          "CSharp": "// StatusLabelKind：Debug / Info / Warn / Error / Fatal（内置浅深主题配色）\r\nvar card = new StatusCard\r\n{\r\n    Kind = StatusLabelKind.Warn,\r\n    TopText = \"WARN\",\r\n    BottomText = \"磁盘空间不足 10%\",\r\n    IconWidth = 32,\r\n};\r\n\r\n// 运行时切状态：伪类与配色随 Kind 自动切换\r\ncard.Kind = StatusLabelKind.Error;\r\n\r\nvar badge = new StatusBadge { LeftText = \"Build\", RightText = \"通过\" };",
          "Xaml": "<!-- 徽标：左签 + 右值；卡片：语义状态驱动伪类换肤（:debug/:info/:warn/:error/:fatal） -->\r\n<codewf:StatusBadge LeftText=\"Info\" RightText=\"信息\" />\r\n\r\n<codewf:StatusCard Kind=\"Warn\"\r\n                   TopText=\"WARN\"\r\n                   BottomText=\"磁盘空间不足 10%\"\r\n                   IconWidth=\"32\" />"
        },
        "OnPropertyCommitted": {
          "Information": "信息",
          "Warning": "警告",
          "Error": "错误",
          "Critical": "严重"
        }
      },
      "Transfer": {
        "View": {
          "MoveSelectedItemsBetweenTwoSearchableListsUsing": "双列穿梭框：选中条目后用中间按钮移动，或直接双击任一侧条目移动到对侧；两侧均为独立搜索框（SearchListBox 组合而成）。",
          "FeatureBacklogAvailable": "功能清单（可选）",
          "CurrentDeliverySelected": "本期交付（已选）",
          "LiveCounts": "实时计数",
          "Available": "可选：",
          "Selected": "已选：",
          "OperationLog": "操作日志",
          "TheControlMovesItemsBetweenSourceCollectionsAnd": "控件直接在两个源集合上移动条目，计数随集合变更自动刷新。"
        },
        "Properties": {
          "AvailableCount": "可选数量",
          "NumberOfAvailableItemsOnTheLeft": "左列（待选）条目数。",
          "SelectedCount": "已选数量",
          "NumberOfSelectedItemsOnTheRight": "右列（已选）条目数。"
        },
        "Source": {
          "CSharp": "var transfer = new Transfer\r\n{\r\n    LeftHeader = \"可选\",\r\n    RightHeader = \"已选\",\r\n    LeftItemsSource = available,   // ObservableCollection<FeatureItem>\r\n    RightItemsSource = selected,\r\n};\r\n\r\n// 移动方式一：中间按钮（有选中项时可用）\r\ntransfer.MoveLeftToRight();\r\ntransfer.MoveRightToLeft();\r\n\r\n// 移动方式二：双击任一侧条目，自动移动到对侧（控件内部处理）\r\n\r\n// 集合即状态：两侧集合的 CollectionChanged 即业务侧的真实变更\r\nselected.CollectionChanged += (_, e) => Save(selected);",
          "Xaml": "<!-- 双列穿梭：LeftItemsSource / RightItemsSource 为普通 IList，控件直接在两侧集合间移动 -->\r\n<codewf:Transfer LeftHeader=\"功能清单（可选）\"\r\n                 RightHeader=\"本期交付（已选）\"\r\n                 LeftItemsSource=\"{Binding Available}\"\r\n                 RightItemsSource=\"{Binding Selected}\" />"
        },
        "OnCollectionChanged": {
          "MoveIn": "移入",
          "MoveOut": "移出",
          "AvailableSelectedFormat": "{0}：{1} · 可选 {2} / 已选 {3}"
        },
        "Available": {
          "IncrementalRendering": "增量渲染",
          "Completed": "已完成",
          "BlockVirtualization": "块级虚拟化",
          "18TypographyThemes": "18 套排版主题",
          "PDFExport": "PDF 导出",
          "WordExport": "Word 导出",
          "RichTextPublishing": "公众号复制",
          "CollaborativeAnnotations": "协作批注",
          "Planned": "规划中",
          "VersionComparison": "版本对比",
          "AISummaries": "AI 摘要",
          "HandwrittenFormulaRecognition": "公式手写识别"
        }
      },
      "TrapezoidTab": {
        "View": {
          "TrapezoidControlThemeAdaptsTheFirstMiddleAndFinal": "TrapezoidShapedTabControl：梯形页签 ControlTheme（TrapezoidShapedTabItemBorder 负责路径绘制），首 / 中 / 末页签自动适配左右斜边；配套 TabStrip 版本用于 Ribbon。本演示台的文档页签即该主题。",
          "TabControlTrapezoidTheme": "TabControl（梯形主题）",
          "Home": "首页",
          "FirstTabFlushLeftEdgeAndSlantedRight": "首个页签：左侧贴齐、右侧斜边。",
          "Design": "设计",
          "MiddleTabTwoSlantedEdgesDiagonalFilletRatioSetsThe": "中间页签：两侧斜边，DiagonalFilletRatio 控制斜角比例。",
          "Development": "开发",
          "MiddleTabHighlightedOutlineWhenSelected": "中间页签：选中态高亮描边。",
          "Release": "发布",
          "MiddleTabHoverAndPressedFeedback": "中间页签：悬停 / 按压反馈。",
          "About": "关于",
          "LastTabFlushRightEdgeAndSlantedLeft": "末个页签：右侧贴齐、左侧斜边。",
          "TabStripStandalone": "TabStrip（独立页签条）",
          "Overview": "概览",
          "Details": "明细",
          "Statistics": "统计",
          "TabStripManagesSelectionTheHostSuppliesTheAssociated": "↑ TabStrip 仅承载选择状态，内容区自行联动（Ribbon 页签即此用法）。"
        },
        "Properties": {
          "ThemeKeys": "主题键",
          "ACSemiThemeInjectsTheResourceKeysForUseWith": "资源键均随 ACSemiTheme 注入，直接 StaticResource 引用。"
        },
        "Source": {
          "CSharp": "// 路径绘制：TrapezoidShapedTabItemBorder（const DiagonalFilletRatio = 0.8）\r\n// Dock 包另有 TrapezoidShapedDocumentTabStripItemBorder，与文档页签视觉一致。\r\n//\r\n// 首 / 中 / 末自适应：主题内通过 TrapezoidShapedTabItemPaddingConverter\r\n// 与相邻页签关系自动绘制单侧 / 双侧斜边，无需逐个配置。\r\ntabControl.Theme = (ControlTheme)Application.Current.Resources[\"TrapezoidShapedTabControl\"]!;",
          "Xaml": "<!-- 梯形页签是 ControlTheme，套在普通 TabControl / TabStrip 上即可 -->\r\n<TabControl Theme=\"{StaticResource TrapezoidShapedTabControl}\">\r\n  <TabItem Header=\"首页\" />\r\n  <TabItem Header=\"设计\" />\r\n  <TabItem Header=\"发布\" />\r\n</TabControl>\r\n\r\n<TabStrip Theme=\"{StaticResource TrapezoidShapedTabStrip}\">\r\n  <TabStripItem Content=\"概览\" />\r\n  <TabStripItem Content=\"明细\" />\r\n</TabStrip>"
        }
      },
      "Welcome": {
        "View": {
          "EditPreviewExport": "编辑 · 预览 · 导出",
          "ThreeStateNaturalSorting": "三态排序 · 自然排序",
          "DockFloatDocuments": "停靠 · 浮动 · 文档",
          "SearchTransferStatus": "搜索 · 穿梭 · 状态",
          "TypedPropertiesLiveCommits": "类型化属性 · 即时提交",
          "LogStreamsRuntimeLocalization": "日志流 · 运行时多语言"
        },
        "markdown": {
          "GiveInterfacesExpressionBuildDesktopExperiencesWithCodeWF": "# 让界面更有表现力\n\n用 **CodeWF.Avalonia** 构建桌面体验。\n\n- 实时 Markdown 预览\n- 三态排序与属性编辑\n- `Dock` 多文档工作区"
        }
      },
      "WindowedStackPanel": {
        "View": {
          "Generate5000Blocks": "生成 5000 块",
          "Clear": "清空",
          "WindowedStackPanelRealizesBlocksNearTheViewportWithOverscan": "窗口化虚拟化堆叠面板：只实现视口附近的块（Overscan 个视口缓冲），可视化树规模与长列表解耦。滚动时观察下方实时指标。"
        },
        "Properties": {
          "Virtualization": "虚拟化",
          "KeepAllBlocksInTheVisualTreeWhen": "关闭后全部块常驻可视化树（5000 块时可感受差异）。",
          "VirtualizationThreshold": "虚拟化阈值",
          "EnableWindowedRealizationAboveThisBlockCount": "块数超过阈值才启用窗口化实现。",
          "Overscan": "视口缓冲",
          "ExtraViewportBuffersAboveAndBelowTheVisible": "视口上下各多实现的视口数量，影响滚动平滑度。",
          "BlockSpacing": "块间距",
          "SpacingBetweenAdjacentBlocks": "相邻块之间的间距。"
        },
        "Source": {
          "CSharp": "var panel = new WindowedStackPanel\n{\n    EnableVirtualization = true,\n    VirtualizationThreshold = 40,\n    OverscanViewports = 2,\n    Spacing = 8,\n};\nscrollViewer.Content = panel;\n\nfor (var i = 0; i < 5000; i++)\n{\n    panel.AddItem(new TextBlock { Text = $\"块 {i}\" });\n}\n\n// 实时指标\npanel.ItemCount;           // 5000\npanel.RealizedItemCount;   // 视口 + 缓冲内已实现的块数（几十）\npanel.IsVirtualizing;      // 是否处于虚拟化状态\n\n// 定点操作\npanel.RealizeItem(100);          // 强制实现\npanel.TryGetItemTop(100, out var top);\npanel.ScrollHost?.Offset = new Vector(0, top);   // 滚动到指定块\npanel.ClearItems();",
          "Xaml": "<!-- 与 ScrollViewer 配合使用；子项可通过 XAML 子元素或代码 AddItem -->\n<ScrollViewer>\n  <codewf:WindowedStackPanel x:Name=\"BlocksPanel\"\n                             EnableVirtualization=\"True\"\n                             VirtualizationThreshold=\"40\"\n                             OverscanViewports=\"2\"\n                             Spacing=\"8\" />\n</ScrollViewer>"
        },
        "OnGenerate": {
          "BlockFormat": "块 {0} / {1}",
          "WindowedStackPanelGeneratedBlocksFormat": "WindowedStackPanel：已生成 {0} 块"
        },
        "UpdateMetrics": {
          "Yes": "是",
          "No": "否",
          "BlocksRealizedVirtualizingFormat": "总块数：{0} · 已实现：{1} · 虚拟化中：{2}"
        }
      }
    },
    "Scene": {
      "Step.Open": "打开文档",
      "Step.Edit": "编辑",
      "Step.Preview": "实时预览",
      "Step.Export": "导出复制",
      "Step.WriteBack": "任务回写",
      "Step.Import": "导入数据",
      "Step.Columns": "列配置",
      "Step.SortFilter": "排序过滤",
      "Step.ExportData": "导出",
      "Step.Dock": "停靠布局",
      "Step.Tabs": "多文档页签",
      "Step.Panels": "面板联动",
      "Step.Status": "状态反馈",
      "Summary.Markdown": "5 步写作流程 · 每一步复用下方组件页",
      "Summary.Data": "4 步表数据流程 · 与属性面板联动",
      "Summary.Docs": "4 步多文档流程 · 面板与页签协同"
    },
    "Segment": {
      "Demo": "演示",
      "Xaml": "XAML",
      "CSharp": "C#"
    }
  },
  "en-US": {
    "Content": {
      "AppGuide": {
        "View": {
          "TourTheRibbonNavigationDocumentsPropertiesAndLogs": "Tour the Ribbon, navigation, documents, properties, and logs. Targets in the main window are resolved across the control tree.",
          "StartWorkspaceTour": "Start workspace tour",
          "TourRegions": "Tour regions",
          "RibbonCommandsNavigationDocumentsWithDemoXAMLC": "① Ribbon commands\n② Navigation\n③ Documents with Demo / XAML / C# views\n④ PropertyGrid\n⑤ Live logs"
        },
        "Properties": {
          "TargetLookup": "Target lookup",
          "NamesOfTheFiveWorkspaceRegionsResolvedControls": "Names of the five workspace regions; resolved controls are assigned to GuideStep.Target."
        },
        "Source": {
          "CSharp": "var root = TopLevel.GetTopLevel(this) as Visual;\nvar ribbon = FindDescendant(root, \"RibbonRoot\");\nvar navTree = FindDescendant(root, \"NavTree\");\n\nAppGuide.Steps.Add(new GuideStep\n{\n    Title = \"① Ribbon\",\n    Description = \"Example\",\n    Target = ribbon,\n    Placement = GuidePlacementMode.Bottom,\n});\nAppGuide.Show();",
          "Xaml": "<codewf:Guide x:Name=\"AppGuide\" IsArrowVisible=\"True\" />"
        },
        "OnStartGuide": {
          "TargetLookupFailedUsingTheDefaultBehavior": "(Target lookup failed; using the default behavior.)",
          "RibbonCommands": "① Ribbon commands",
          "EightRibbonGroupsContainTheDemoCommandsDouble": "Eight Ribbon groups contain the demo commands. Double-click a tab to collapse or expand them.",
          "Navigation": "② Navigation",
          "MirrorsRibbonGroupsClickAnItemToOpen": "Mirrors Ribbon groups; click an item to open its demo.",
          "Documents": "③ Documents",
          "EachComponentHasADemoDocumentWithDemo": "Each component has a demo document with Demo / XAML / C# views.",
          "ThePropertyPanelConfiguresTheActiveDemoThe": "The property panel configures the active demo; the overview shows global appearance and language settings.",
          "Logs": "⑤ Logs",
          "LiveLoggerEventsWithFilteringCopyClearAnd": "Live Logger events with filtering, copy, clear, and access to the log directory.",
          "AppGuideStartingWorkspaceTour": "AppGuide: starting workspace tour"
        },
        "OnStepOpened": {
          "AppGuideStepFormat": "AppGuide: step {0}/{1} · {2}"
        }
      },
      "CodeWFWindow": {
        "View": {
          "BorderlessCodeWFWindowWithDWMCornersACustomTitle": "Borderless CodeWFWindow with DWM corners, a custom title bar, configurable buttons, and extended content. Open a real window using the examples below.",
          "OpenDefaultConfiguration": "Open: default configuration",
          "OpenMinimalButtonsAndCustomTitleBar": "Open: minimal buttons and custom title bar",
          "OpenExtendContentIntoTitleBar": "Open: extend content into title bar",
          "Example1DefaultTitleBar": "Example 1: default title bar",
          "BorderlessCustomTitleBar": "Borderless · custom title bar",
          "TitleBarHeight36WithMinimizeRestoreCloseButtons": "TitleBarHeight=36, with minimize / restore / close buttons.",
          "Example2CustomLeftRightContent": "Example 2: custom left / right content",
          "DocumentDesignNotes": "Document: design notes",
          "Search": "Search…",
          "LeftContentRightContentAcceptArbitraryContent": "LeftContent / RightContent accept arbitrary content.",
          "Example3CenteredContent": "Example 3: centered content",
          "Left": "(left)",
          "CenteredTitleCenterContent": "Centered title · CenterContent",
          "ArrangeCenterContentAndTheTitleToAvoidOverlap": "Arrange CenterContent and the title to avoid overlap."
        },
        "Properties": {
          "CaptionButtonWidth": "Caption button width",
          "CaptionButtonWidthDescription": "Width of the minimize, maximize, and close buttons in the next example window.",
          "TitleBarHeight": "Title bar height",
          "TitleBarHeightForTheNextExampleWindow": "Title bar height for the next example window.",
          "WindowCornerRadius": "Window corner radius",
          "WindowCornerRadiusWindowsDWM": "Window corner radius (Windows DWM).",
          "ExtendContentIntoTitleBar": "Extend content into title bar",
          "ExtendTheWindowContentUnderneathTheTitleBar": "Extend the window content underneath the title bar."
        },
        "Source": {
          "CSharp": "var window = new CodeWFWindow\n{\n    Title = \"CodeWFWindow example\",\n    Width = 640,\n    Height = 420,\n    ContentExtendsIntoTitleBar = true,\n    IsFullScreenButtonVisible = true,\n    IsMinimizeButtonVisible = true,\n    WindowCornerRadius = new CornerRadius(8),\n};\nwindow.Content = new TextBlock\n{\n    Text = \"Example\",\n    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,\n    VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,\n};\nwindow.Show(owner);",
          "Xaml": "<codewf:CodeWFWindow xmlns:codewf=\"https://codewf.com\"\n                     Title=\"CodeWFWindow example\"\n                     TitleBarHeight=\"36\"\n                     WindowCornerRadius=\"8\"\n                     ContentExtendsIntoTitleBar=\"True\"\n                     IsFullScreenButtonVisible=\"True\">\n  <codewf:CodeWFWindow.LeftContent>\n    <TextBlock Text=\"◆\" Foreground=\"DodgerBlue\" VerticalAlignment=\"Center\" />\n  </codewf:CodeWFWindow.LeftContent>\n  <Panel>\n    \n  </Panel>\n</codewf:CodeWFWindow>"
        },
        "OnOpenCustom": {
          "MinimalButtonsAndCustomTitleBar": "Minimal buttons and custom title bar"
        },
        "OnOpenDefault": {
          "DefaultConfiguration": "Default configuration"
        },
        "OnOpenExtends": {
          "ExtendContentIntoTitleBar": "Extend content into title bar"
        },
        "ShowSample": {
          "CodeWFWindowExampleFormat": "CodeWFWindow example · {0}",
          "Search": "Search…",
          "ThisContentExtendsIntoTheTitleBarContentExtendsIntoTitleBar": "This content extends into the title bar (ContentExtendsIntoTitleBar=True).",
          "ConfigurationCloseThisWindowToTryAnotherConfigurationFormat": "Configuration: {0}\nClose this window to try another configuration."
        }
      },
      "ColumnDisplayEditor": {
        "View": {
          "ResetDefaults": "Reset defaults",
          "DragARowHandleToReorderColumnsAnd": "Drag a row handle to reorder columns and toggle their visibility. Changes operate on the source collection.",
          "ChangeAudit": "Change audit"
        },
        "Properties": {
          "DataContract": "Data contract",
          "TheEditorRequiresIListItemsSourceDraggingReordersIt": "The editor requires IList ItemsSource; dragging reorders it with IList.Move."
        },
        "Source": {
          "CSharp": "var columns = new ObservableCollection<ColumnDisplayItem>\n{\n    new(\"name\", \"Name\"),\n    new(\"status\", \"Status\"),\n    new(\"remark\", \"Notes\", visible: false),\n};\n\nvar editor = new ColumnDisplayEditor { ItemsSource = columns };\n\ncolumns.CollectionChanged += (_, e) =>\n{\n    if (e.Action == NotifyCollectionChangedAction.Move)\n    {\n        Save(columns);\n    }\n};\ncolumns[0].PropertyChanged += (_, e) =>\n{\n    if (e.PropertyName == nameof(ColumnDisplayItem.Visible))\n    {\n        Save(columns);\n    }\n};",
          "Xaml": "<codewf:ColumnDisplayEditor ItemsSource=\"{Binding Columns}\"\n                            VisibleHeaderText=\"Visible\"\n                            DisplayTextHeaderText=\"Column name\" />"
        },
        "CreateDefaultColumns": {
          "Name": "Name",
          "Type": "Type",
          "Status": "Status",
          "Owner": "Owner",
          "UpdatedAt": "Updated at",
          "Notes": "Notes"
        },
        "OnColumnPropertyChanged": {
          "Visible": "Visible",
          "Hidden": "Hidden",
          "VisibilityOfFormat": "[{0:HH:mm:ss}] Visibility of “{1}” → {2}"
        },
        "OnColumnsCollectionChanged": {
          "MovedToPositionFormat": "[{0:HH:mm:ss}] Moved “{1}” to position {2}"
        },
        "Reset": {
          "RestoredDefaultColumnsFormat": "[{0:HH:mm:ss}] Restored default columns"
        }
      },
      "DataFlow": {
        "CSharpCode": {
          "DataGridEnableDefaultsItemsSourceColumnsMoveOldIndexNewIndexDataGrid": "dataGrid.EnableDefaults(itemsSource);\n\nColumns.Move(oldIndex, newIndex);\n\ndataGrid.AddNaturalSorting();\n\ndataTable.ToExcel(\"order-2026.xlsx\");"
        },
        "XamlCode": {
          "DataGridCodewfDataGridEnhancementUseDefaultsTrueItemsSourceCodewfColumnDisplayEditor": "<DataGrid codewf:DataGridEnhancement.UseDefaults=\"True\"\n          ItemsSource=\"{Binding Rows}\" />\n\n<codewf:ColumnDisplayEditor Items=\"{Binding Columns}\" />"
        }
      },
      "DataGrid": {
        "View": {
          "TopEnhancedDataGridWithThreeStateAndNatural": "Top: enhanced DataGrid with three-state and natural sorting, plus ToolTips for clipped cells. Bottom: native DataGrid using the same rows. Compare header clicks and hover behavior.",
          "EnhancedUseDefaults": "Enhanced (UseDefaults)",
          "Name": "Name",
          "Age": "Age",
          "City": "City",
          "NotesHoverToTrySmartToolTip": "Notes (hover to try smart ToolTip)",
          "NativeComparison": "Native comparison",
          "Notes": "Notes"
        },
        "Properties": {
          "EnableDefaults": "Enable defaults",
          "UseDefaultsEnablesThreeStateAndNaturalSortingSmart": "UseDefaults enables three-state and natural sorting, smart ToolTips, and select all.",
          "RowHeight": "Row height",
          "RowHeightOfTheEnhancedGrid": "Row height of the enhanced grid."
        },
        "Source": {
          "CSharp": "dataGrid.EnableDefaults(dataGrid.ItemsSource as IEnumerable);\ndataGrid.AddSorting();\ndataGrid.AddNaturalSorting();\ndataGrid.EnableSmartTooltips();\n\nDataGridEnhancement.SetUseDefaults(dataGrid, true);\n\ndataGrid.ApplyPerformancePreset();",
          "Xaml": "<DataGrid codewf:DataGridEnhancement.UseDefaults=\"True\"\n          ItemsSource=\"{Binding Rows}\"\n          AutoGenerateColumns=\"False\" CanUserSortColumns=\"True\"\n          IsReadOnly=\"True\" HeadersVisibility=\"Column\">\n  <DataGrid.Columns>\n    <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" Width=\"140\" />\n    <DataGridTextColumn Header=\"Notes\" Binding=\"{Binding Comment}\" Width=\"*\" />\n  </DataGrid.Columns>\n</DataGrid>"
        },
        "CreateRows": {
          "Hangzhou": "Hangzhou",
          "Beijing": "Beijing",
          "Seattle": "Seattle",
          "Chengdu": "Chengdu",
          "Berlin": "Berlin",
          "Shanghai": "Shanghai",
          "Shenzhen": "Shenzhen",
          "Guangzhou": "Guangzhou",
          "Nanjing": "Nanjing",
          "Tokyo": "Tokyo",
          "NoteThisTextIsDeliberatelyLongAndClipsFormat": "{0}, note {1}: this text is deliberately long and clips in a narrow column. ",
          "SmartToolTipRevealsClippedContentOnHoverFully": "Smart ToolTip reveals clipped content on hover; fully visible cells need no popup."
        }
      },
      "DemoBootstrapper": {
        "Initialize": {
          "CodeWFAvaloniaStartedLogDirectory": "CodeWF.Avalonia started. Log directory: "
        }
      },
      "Dock": {
        "View": {
          "DockAvaloniaProvidesTheLayoutEngineDockSemiThemeSupplies": "Dock.Avalonia provides the layout engine; DockSemiTheme supplies appearance and localization. Drag, dock, float, and maximize documents and panels."
        },
        "Properties": {
          "Composition": "Composition",
          "CodeWFAvaloniaDockSuppliesTheSemiAppearanceTrapezoid": "CodeWF.Avalonia.Dock supplies the Semi appearance, trapezoid document tabs, and tool panel behavior on Dock.Avalonia."
        },
        "Source": {
          "CSharp": "//   <codewf:DockSemiTheme Locale=\"en-US\" />\n\nvar factory = new Dock.Model.ReactiveUI.Factory();\n\nvar document = new Document { Id = \"Doc.1\", Title = \"Design notes\", Content = markdown };\nvar docDock = new DocumentDock\n{\n    VisibleDockables = AvaloniaList<IDockable> [document],\n    ActiveDockable = document,\n    DocumentTemplate = markdownTemplate,\n    CanCreateDocument = false,\n};\nvar mainDock = new ProportionalDock\n{\n    Orientation = Orientation.Horizontal,\n    VisibleDockables = AvaloniaList<IDockable>\n    [\n        toolDock, new ProportionalDockSplitter(), docDock\n    ],\n};\nvar layout = new RootDock { VisibleDockables = AvaloniaList<IDockable> [mainDock] };\nfactory.InitLayout(layout);",
          "Xaml": "<dock:DockControl xmlns:dock=\"using:Dock.Avalonia.Controls\"\n                  Layout=\"{Binding Layout}\" />"
        },
        "apiDocument": {
          "APIDraft": "API draft"
        },
        "Content": {
          "DockWorkspace": "Dock workspace"
        },
        "designDocument": {
          "DesignNotes": "Design notes"
        },
        "DocApiMarkdown": {
          "APIDraftCsharpVarFactoryNewDockModel": "# API draft\n\n```csharp\nvar factory = new Dock.Model.ReactiveUI.Factory();\nfactory.InitLayout(root);\n```\n\nDrag tabs to explore the workspace."
        },
        "DocDesignMarkdown": {
          "DesignNotesDockLayoutRootDockProportionalDockToolDockDocumentDock": "# Design notes\n\nDock layout: **RootDock → ProportionalDock → ToolDock / DocumentDock**.\n\nContent is resolved from Context through DataTemplates.\n\n- Drag to dock\n- Float a panel\n- Double-click to maximize\n\n> The layout engine is Dock.Avalonia."
        },
        "overviewTool": {
          "WorkspaceOverview": "Workspace overview"
        },
        "ToolOverviewMarkdown": {
          "WorkspaceOverviewLeftToolPanelRightDraggableDocuments": "### Workspace overview\n\n- Left: tool panel\n- Right: draggable documents\n- Tabs: double-click to maximize\n\nDrag a document tab to dock it."
        }
      },
      "DocsFlow": {
        "CSharpCode": {
          "VarDocNewDocumentFactoryHideDockableToolFactory": "var doc = new Document\n{\n    Title = \"Design notes\",\n    Context = new DocContext(markdown),\n};\n\nfactory.HideDockable(tool);\nfactory.RestoreDockable(tool.Id);"
        },
        "XamlCode": {
          "DockDockControlLayoutDockControlDataTemplatesDataTemplateDataTypeVm": "<dock:DockControl Layout=\"{Binding Layout}\">\n  <DockControl.DataTemplates>\n    <DataTemplate DataType=\"vm:DocContext\">\n      <md:MarkdownViewer Markdown=\"{Binding Markdown}\" />\n    </DataTemplate>\n  </DockControl.DataTemplates>\n</dock:DockControl>"
        }
      },
      "Guide": {
        "View": {
          "StartGuide13Placements": "Start guide (13 placements)",
          "HighlightTheTargetIn13PlacementsWithEnter": "Choose a placement. Enter advances, Esc closes; highlights follow window moves and resizing.",
          "Step1Center": "Step 1 · Center",
          "IsPointAtCenterTrueOrNoTargetHighlightsTheScreen": "IsPointAtCenter=True or no target highlights the screen center.",
          "Step2TopLeft": "Step 2 · TopLeft",
          "TheBubbleAppearsAtTheUpperLeftOf": "The bubble appears at the upper left of the target.",
          "Step3TopRight": "Step 3 · TopRight",
          "TheBubbleAppearsAtTheUpperRightOf": "The bubble appears at the upper right of the target.",
          "Step4Right": "Step 4 · Right",
          "TheBubbleAppearsToTheRightOfThe": "The bubble appears to the right of the target.",
          "Step5Bottom": "Step 5 · Bottom",
          "TheBubbleAppearsBelowTheTarget": "The bubble appears below the target.",
          "Step6LeftBottom": "Step 6 · LeftBottom",
          "TheBubbleAppearsAtTheLowerLeftWith": "The bubble appears at the lower left with the Primary style.",
          "Step7CustomMaskColor": "Step 7 · Custom mask color",
          "ThisStepDemonstratesATranslucentRedMaskColor": "This step demonstrates a translucent red MaskColor.",
          "Step8Complete": "Step 8 · Complete",
          "FinalStepPressEnterOrClickFinishTo": "Final step: press Enter or click Finish to end the guide."
        },
        "Properties": {
          "PlacementDetails": "Placement details",
          "EachStepSetsPlacement13PositionsCoverThe": "Each step sets Placement; 13 positions cover the available bubble placements."
        },
        "Source": {
          "CSharp": "var guide = new Guide\n{\n    IsArrowVisible = true,\n    MissingTargetBehavior = GuideMissingTargetBehavior.Center,\n};\nguide.Steps.Add(new GuideStep\n{\n    Title = \"Example\",\n    Description = \"Example\",\n    Target = someControl,\n    Placement = GuidePlacementMode.RightTop,\n    OpeningCommand = prepareCommand,\n});\nguide.StepOpened += (_, e) => Logger.Info($\"Example\");\nguide.Completed += (_, _) => Logger.Info(\"Example\");\nguide.Show();\nguide.GoNext();\nguide.Close();",
          "Xaml": "<codewf:Guide IsArrowVisible=\"True\" Completed=\"OnGuideCompleted\">\n  <codewf:GuideStep Title=\"Step 1\"\n                    Description=\"Example\"\n                    Target=\"{Binding #TargetControl}\"\n                    Placement=\"RightTop\" />\n  <codewf:GuideStep Title=\"Step 2\"\n                    Description=\"Example\"\n                    Target=\"{Binding #TargetControl2}\"\n                    Placement=\"Bottom\"\n                    StyleType=\"Primary\"\n                    MaskColor=\"#66B3261E\" />\n</codewf:Guide>"
        },
        "OnGuideCompleted": {
          "GuideTourCompleted": "Guide: tour completed"
        },
        "OnStartGuide": {
          "GuideStartingTour": "Guide: starting tour"
        },
        "OnStepOpened": {
          "GuideStepFormat": "Guide: step {0}/{1} · {2}"
        }
      },
      "Lang": {
        "View": {
          "I18nManagerUsesAPluggableLocalizationBackendTheDemo": "I18nManager uses a pluggable localization backend. The Demo loads JSON resources and uses live I18n bindings. Try changing Culture with the buttons below.",
          "SwitchToEnglish": "Switch to English",
          "SwitchToSimplifiedChinese": "Switch to Simplified Chinese",
          "UseSystemCulture": "Use system culture"
        },
        "ApplyCulture": {
          "LangChangingLanguage": "Lang: changing language → "
        },
        "Source": {
          "CSharp": "I18nManager.Instance.Register(\n    new JsonLangPlugin { ResourceFolder = Path.Combine(AppContext.BaseDirectory, \"I18n\") },\n    new CultureInfo(\"en-US\"));\n// I18nManager.Instance.Register(new XmlLangPlugin { ResourceFolder = folder }, culture);\n// I18nManager.Instance.Register(new ResxLangPlugin(typeof(Properties.Resources)), culture);\n\nI18nManager.Instance.AddResource(typeof(MainWindow).Assembly);\n\nI18nManager.Instance.Culture = new CultureInfo(\"en-US\");\n\nvar text = I18nManager.Instance.GetResource(\"Demo.Lang.SampleGreeting\");\nvar languages = I18nManager.Instance.GetLanguages();\nvar diagnostics = ((JsonLangPlugin)plugin).LoadDiagnostics;",
          "Xaml": "<TextBlock Text=\"{codewf:I18n Demo.Lang.SampleGreeting}\" />\n<TextBlock Text=\"{codewf:I18n Demo.Lang.SampleFormat}\" />"
        },
        "UpdateState": {
          "None": "None",
          "CurrentCultureFormat": "Current culture: {0} · ",
          "LoadedLanguagesFormat": "Loaded languages: {0} · ",
          "ResourceDirectoryFormat": "Resource directory: {0}"
        }
      },
      "Log": {
        "View": {
          "LogViewSubscribesToLoggerSLiveEventsWith": "LogView subscribes to Logger's live events with filtering, retention, timestamps, and copy / clear / open-folder actions. The workspace log panel uses the same control.",
          "WriteBatchEventStreamLoadTest": "Write batch (event stream load test)"
        },
        "Properties": {
          "MinimumLevel": "Minimum level",
          "Trace": "Trace",
          "Debug": "Debug",
          "Information": "Information",
          "Warning": "Warning",
          "Error": "Error",
          "HideLogsBelowThisLevelMinimumLevel": "Hide logs below this level (MinimumLevel).",
          "MaximumEntries": "Maximum entries",
          "MaximumRetainedEntriesMaxDisplayCountOlderEntriesAreDropped": "Maximum retained entries (MaxDisplayCount); older entries are dropped."
        },
        "Source": {
          "CSharp": "logView.Source = Logger.Events;\n\nLogger.Info(\"Service started\");\nLogger.Warn(\"Disk space low\");\nLogger.Error(\"Downstream service timeout\");\n\nlogView.MinimumLevel = LogLevel.Warning;\nlogView.MaxDisplayCount = 2000;",
          "Xaml": "Logger.TryInitialize(new LoggerOptions\n{\n    File = new FileLogOptions { DirectoryPath = logDirectory },\n});\n\n<codewf:CodeWFLogTheme />   \n\n<codewf:LogView MinimumLevel=\"Information\"\n                MaxDisplayCount=\"2000\"\n                LogLineHeightMultiplier=\"1.5\" />"
        },
        "OnPropertyCommitted": {
          "Trace": "Trace",
          "Debug": "Debug",
          "Warning": "Warning",
          "Error": "Error"
        },
        "OnWriteBurst": {
          "LoadTestLogEventThroughputAndDisplayThrottlingFormat": "Load-test log {0:000}: event throughput and display throttling"
        },
        "OnWriteCritical": {
          "DemoCriticalLogDatabaseConnectionFailed": "Demo: critical log — database connection failed"
        },
        "OnWriteDebug": {
          "DemoDebugLogHiddenAtTheDefaultLevel": "Demo: debug log (hidden at the default level)"
        },
        "OnWriteError": {
          "DemoErrorLogDownstreamTimeoutOf3000ms": "Demo: error log — downstream timeout of 3000ms"
        },
        "OnWriteInfo": {
          "DemoInformationLog": "Demo: information log "
        },
        "OnWriteTrace": {
          "DemoTraceLogHiddenAtTheDefaultLevel": "Demo: trace log (hidden at the default level)"
        },
        "OnWriteWarning": {
          "DemoWarningLogLessThan10FreeDisk": "Demo: warning log — less than 10% free disk space"
        }
      },
      "Shell": {
        "ApplyAppearance": {
          "Dark": "Dark",
          "Light": "Light",
          "AppearanceFormat": "Appearance: {0} / {1}"
        },
        "OnGlobalPropertyCommitted": {
          "Dark": "Dark"
        },
        "RunSmokeAsync": {
          "SmokeStarted": "Smoke started",
          "SmokePassed": "Smoke passed",
          "SmokeFailed": "Smoke failed"
        },
        "SyncGlobalItems": {
          "Dark": "Dark",
          "Light": "Light"
        },
        "CommitRename": {
          "RestoredOriginalNameFormat": "Restored original name: {0}",
          "RenamedFormat": "Renamed: {0} → {1}"
        },
        "KindLabel": {
          "Documents": "Documents",
          "Commands": "Commands"
        },
        "LoadLayoutFrom": {
          "LayoutRestored": "Layout restored: ",
          "FailedToRestoreLayout": "Failed to restore layout: "
        },
        "SaveLayoutTo": {
          "FailedToSaveLayout": "Failed to save layout: "
        }
      },
      "MarkdownEditor": {
        "View": {
          "SourceEditorPoweredByAvaloniaEditWithTextMateHighlighting": "Source editor powered by AvaloniaEdit, with TextMate highlighting and Markdown editing commands. Try the common actions below; see MarkdownEditorAction for the complete set.",
          "Bold": "Bold",
          "Italic": "Italic",
          "InlineCode": "Inline code",
          "Heading2": "Heading 2",
          "Table": "Table",
          "TaskList": "Task list",
          "CodeBlock": "Code block",
          "Quote": "Quote"
        },
        "Properties": {
          "EditorFontSize": "Editor font size",
          "EditorTextFontSize": "Editor text font size.",
          "ShowLineNumbers": "Show line numbers",
          "ShowLineNumbersToTheLeftOfThe": "Show line numbers to the left of the editor.",
          "AutoPairing": "Auto pairing",
          "AutomaticallyCompletePairedMarkersSuchAsAndBackticks": "Automatically complete paired markers such as ** and backticks."
        },
        "Source": {
          "CSharp": "var editor = new MarkdownEditorView(new MarkdownEditorOptions\n{\n    LinkUrlPlaceholder = \"https://example.com\",\n    CodeFenceLanguage = \"csharp\",\n    EnableAutoPair = true,\n    ShowLineNumbers = true,\n    FontSize = 15,\n});\nEditorHost.Content = editor;\neditor.SetText(\"Example\");\neditor.MarkdownChanged += (_, text) => Save(text);\n\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\nawait editor.ExecuteAsync(MarkdownEditorAction.Table);\neditor.InsertTable(\"Example\");\neditor.WrapSelection(\"**\", \"**\");\n\nvar (line, column, lineCount) = editor.CaretPosition;\nvar text = editor.Text;",
          "Xaml": "<Border x:Name=\"EditorHost\" />"
        },
        "OnTable": {
          "Column1Column2AB": "| Column 1 | Column 2 |\n| --- | --- |\n| A | B |"
        },
        "Samples": {
          "Markdown": "# MarkdownEditorView\n\nMarkdown source editing with AvaloniaEdit.\n\n## Built-in actions\n- Bold and italic\n- Headings and tables\n- Task lists and code\n\n```csharp\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\n```"
        },
        "UpdateStatus": {
          "LineColumnLinesCharactersFormat": "Line {0} / column {1} · {2} lines · {3} characters"
        }
      },
      "MarkdownExport": {
        "View": {
          "ExportMarkdownAsPNGPDFWordOrHTML": "Export Markdown as PNG, PDF, Word, or HTML, or copy rich text for WeChat, Zhihu, and Juejin. Set export typography in the property panel.",
          "ExportPNG": "Export PNG",
          "ExportPDF": "Export PDF",
          "ExportWord": "Export Word",
          "ExportHTML": "Export HTML",
          "CopyPublishingHTML": "Copy publishing HTML"
        },
        "Properties": {
          "ExportTheme": "Export theme",
          "TypographyThemeOfTheExportedDocument": "Typography theme of the exported document.",
          "TypographySize": "Typography size",
          "TypographySizeOfTheExportedDocument": "Typography size of the exported document.",
          "CopyDestination": "Copy destination",
          "WeChat": "WeChat",
          "Zhihu": "Zhihu",
          "Juejin": "Juejin",
          "DestinationPlatformForRichText": "Destination platform for rich text."
        },
        "copyKind": {
          "WeChat": "WeChat"
        },
        "Source": {
          "CSharp": "MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, @\"D:\\\\out\\\\doc.png\");\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Pdf, @\"D:\\\\out\\\\doc.pdf\");\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Word, @\"D:\\\\out\\\\doc.docx\");\nMarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Html, @\"D:\\\\out\\\\doc.html\",\n    themeName: MarkdownTypographyThemes.WeChatFormat);\n\nvar ok = await MarkdownHtmlClipboard.TrySetHtmlAsync(\n    markdown, \"Example\", MarkdownTypographyThemes.WeChatFormat);\n\nvar file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions\n{\n    Title = \"Export PNG\",\n    DefaultExtension = \".png\",\n});\nif (file is not null)\n{\n    MarkdownDocumentExporter.ExportMarkdown(markdown, ExportKind.Png, file.Path.LocalPath);\n}",
          "Xaml": "<Button Content=\"Export PNG\" Command=\"{Binding ExportCommand}\" />"
        },
        "ExportAsync": {
          "ExportingFormat": "Exporting {0} → {1}",
          "ExportCompleteFormat": "Export complete: {0}",
          "ExportFailedFormat": "Export failed: {0}"
        },
        "OnCopyWechat": {
          "Zhihu": "Zhihu",
          "Juejin": "Juejin",
          "CopiedRichTextPasteItIntoTheEditorFormat": "Copied {0} rich text. Paste it into the editor.",
          "CopyFailed": "Copy failed: "
        },
        "OnExportHtml": {
          "ExportHTML": "Export HTML"
        },
        "OnExportPdf": {
          "ExportPDF": "Export PDF"
        },
        "OnExportPng": {
          "ExportPNG": "Export PNG"
        },
        "OnExportWord": {
          "ExportWord": "Export Word"
        },
        "Samples": {
          "Markdown": "# Export example\n\nExport this document as **PNG / PDF / Word / HTML** or copy rich text.\n\n## Lists\n- Typography themes\n- File export\n\n| Format | Extension |\n| --- | --- |\n| Image | .png |\n| Document | .pdf |\n\n```csharp\nConsole.WriteLine(\"Hello, CodeWF!\");\n```\n\n> Choose a theme in the property panel."
        }
      },
      "MarkdownFlow": {
        "CSharpCode": {
          "AddStepDemoSceneStepOpenNewMarkdownViewerPageAddStep": "AddStep(\"Demo.Scene.Step.Open\", new MarkdownViewerPage());\nAddStep(\"Demo.Scene.Step.Edit\", new MarkdownEditorPage());\nAddStep(\"Demo.Scene.Step.Preview\", new MarkdownLiveEditorPage());\nAddStep(\"Demo.Scene.Step.Export\", new MarkdownExportPage());\nAddStep(\"Demo.Scene.Step.WriteBack\", new MarkdownLitePage());\nAttachSceneShell();\n\nviewer.TaskWriteBackRequested += (_, e) => e.Result.Markdown = e.Rendered;"
        },
        "XamlCode": {
          "CodewfMarkdownViewerMarkdownTypographyThemeCodewfMarkdownEditorViewMarkdownShowLineNumbers": "<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\n                       TypographyTheme=\"{Binding Theme}\" />\n<codewf:MarkdownEditorView Markdown=\"{Binding Source}\" ShowLineNumbers=\"True\" />\n<codewf:MarkdownLiveEditorView Markdown=\"{Binding Doc}\" />"
        }
      },
      "MarkdownLite": {
        "View": {
          "MarkdownLiteUsesMarkdigAndASmallerRendering": "Markdown.Lite uses Markdig and a smaller rendering pipeline for common Markdown elements. It includes the same 18 typography themes with fewer dependencies."
        },
        "Properties": {
          "TypographyTheme": "Typography theme",
          "LiteIncludesTheSame18TypographyThemesFour": "Lite includes the same 18 typography themes; four common themes are listed here.",
          "TypographySize": "Typography size"
        },
        "Source": {
          "CSharp": "//\nvar viewer = new MarkdownViewer\n{\n    Markdown = \"# Lite Example\",\n    TypographyTheme = MarkdownTypographyThemes.Simple,\n    EnableVirtualization = true,\n};",
          "Xaml": "<PackageReference Include=\"CodeWF.Avalonia.Markdown.Lite\" />\n\n<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\n                       TypographyTheme=\"Basic\"\n                       TypographySize=\"Normal\" />"
        },
        "Samples": {
          "Markdown": "# Markdown Lite\n\nHeadings, **bold**, *italic*, `inline code`, lists, quotes, tables, links, and code blocks.\n\n- Smaller dependency set\n- 18 typography themes\n\n| Feature | Supported |\n| --- | --- |\n| Common Markdown | Yes |\n| Export | Use full package |\n\n```csharp\nviewer.Markdown = source;\n```\n\n> A focused rendering package."
        }
      },
      "MarkdownLiveEditor": {
        "View": {
          "MarkdownLiveEditorViewEditsBlockModelsDirectlyHeadingsParagraphsLists": "MarkdownLiveEditorView edits block models directly: headings, paragraphs, lists, tables, and code. Its editing appearance matches the rendered output.",
          "InsertText": "Insert text",
          "FocusFirstBlock": "Focus first block",
          "Bold": "Bold",
          "Table": "Table"
        },
        "Properties": {
          "TypographyTheme": "Typography theme",
          "TypographyThemeUsedByTheLiveEditor": "Typography theme used by the live editor.",
          "EditorFontSize": "Editor font size",
          "FontSizeOfTheBlockEditor": "Font size of the block editor."
        },
        "Source": {
          "CSharp": "var editor = new MarkdownLiveEditorView(new MarkdownEditorOptions\n{\n    EnableAutoPair = true,\n    FontSize = 15,\n});\neditor.TypographyTheme = MarkdownTypographyThemes.Verdrant;\neditor.SetText(\"# Live editing\\\\n\\\\nEdit blocks directly\");\nEditorHost.Content = editor;\n\neditor.MarkdownChanged += (_, text) => Save(text);\neditor.FocusBlock(0);\nawait editor.ExecuteAsync(MarkdownEditorAction.Bold);\neditor.InsertText(\"Appended text\");\n\nvar blockCount = editor.BlockCount;\nvar activeBlock = editor.ActiveBlockIndex;\nvar markdown = editor.Text;                 // = MarkdownBlockParser.Write(models)",
          "Xaml": "<Border x:Name=\"EditorHost\" />"
        },
        "OnInsert": {
          "ContentAddedWithInsertText": "\n\nContent added with InsertText."
        },
        "Samples": {
          "Markdown": "# Live editing\n\nEdit headings, paragraphs, lists, and tables directly.\n\n## List\n- Block models\n- Incremental rendering\n\n```csharp\nvar editor = new MarkdownLiveEditorView();\n```\n\n| Feature | State |\n| --- | --- |\n| Editing | Ready |"
        },
        "UpdateStatus": {
          "BlocksActiveBlockCharactersFormat": "{0} blocks · active block {1} · {2} characters"
        }
      },
      "MarkdownViewer": {
        "View": {
          "EditMarkdownSource": "Edit Markdown source",
          "Apply": "Apply",
          "AppendParagraphIncremental": "Append paragraph (incremental)",
          "ResetSample": "Reset sample"
        },
        "Properties": {
          "TypographyTheme": "Typography theme",
          "18TypographyThemesChangesRenderImmediately": "18 typography themes; changes render immediately.",
          "TypographySize": "Typography size",
          "NormalSmallSmallUsesCompactTypography": "Normal / Small; Small uses compact typography.",
          "Virtualization": "Virtualization",
          "RealizeVisibleBlocksAboveTheThresholdToReduce": "Realize visible blocks above the threshold to reduce memory use for long documents.",
          "VirtualizationThreshold": "Virtualization threshold",
          "EnableVirtualizationAboveThisBlockCount": "Enable virtualization above this block count."
        },
        "Source": {
          "CSharp": "viewer.TypographyTheme = MarkdownTypographyThemes.GeekBlack;\nviewer.TypographySize = MarkdownTypographySizes.Small;\n\nMarkdownThemes.OverrideTypographyResources(Application.Current, MarkdownTypographyThemes.Verdant);\n\nviewer.TaskWriteBackRequested += (_, e) =>\n    editor.ApplyExternalEdit(e.Markdown);\nvar parseMs = viewer.LastParseDuration.TotalMilliseconds;\nvar mode = viewer.LastRenderMode;           // Incremental / Full\nvar realized = viewer.RealizedBlockCount;",
          "Xaml": "<codewf:MarkdownViewer Markdown=\"{Binding Content}\"\n                       TypographyTheme=\"{Binding Theme}\"\n                       TypographySize=\"{Binding Size}\"\n                       EnableVirtualization=\"True\"\n                       VirtualizationThreshold=\"40\" />"
        },
        "UpdateMetrics": {
          "ModeParsingMsRealizedBlocksFormat": "Mode: {0} · parsing: {1:F1} ms · realized blocks: {2}"
        },
        "AppendParagraph": {
          "NewParagraphIncrementalRenderingReparsesAndLaysOut": "\n\nNew paragraph: incremental rendering reparses and lays out changed blocks only ("
        },
        "Samples": {
          "Markdown": "# MarkdownViewer\n\n**Markdig** → block models → windowed rendering.\n\n## Features\n- [x] Incremental rendering\n- [ ] Task write-back\n\n| Component | Purpose |\n| --- | --- |\n| Viewer | Render Markdown |\n| Editor | Edit source |\n\n```csharp\nviewer.Markdown = source;\n```\n\n> Try changing the typography theme."
        }
      },
      "MarkupExtension": {
        "View": {
          "IfSwitchMarkupExtensionsEvaluateConditionalAndMulti": "If / Switch markup extensions evaluate conditional and multi-branch values in XAML. Toggle the state below to see the display update.",
          "TaskRunningPleaseWait": "Task running, please wait…",
          "SystemIdle": "System idle",
          "UnknownState": "Unknown state",
          "Running": "Running",
          "ServiceRunning": "● Service running",
          "Paused": "Paused",
          "ServicePaused": "‖ Service paused",
          "Maintenance": "Maintenance",
          "MaintenanceWindowOpen": "🛠 Maintenance window open"
        },
        "Properties": {
          "UseCases": "Use cases",
          "ConditionalDisplayTextAndIcons": "Conditional display text and icons",
          "IfSwitchChooseDisplayValuesKeepMultiProperty": "If / Switch choose display values; keep multi-property updates and commands in the ViewModel."
        },
        "CSharpCode": {
          "SwitchBindingIfConverterIfConditionConverterSwitchConverterEnumToBooleanConverterIsCollectionEmptyConverter": "//   {codewf:IfBinding ...} / SwitchBinding\n//\n//   IfConverter / IfConditionConverter / SwitchConverter\n//   EnumToBooleanConverter / IsCollectionEmptyConverter"
        },
        "Source": {
          "Xaml": "<TextBlock.Text>\n  <codewf:If Condition=\"{Binding IsBusy}\"\n             True=\"Task running, please wait…\"\n             False=\"System idle\" />\n</TextBlock.Text>\n\n\n<TextBlock.Text>\n  <codewf:Switch Value=\"{Binding State}\" Default=\"Unknown state\">\n    <codewf:SwitchCase When=\"Running\" Then=\"● Service running\" />\n    <codewf:SwitchCase When=\"Paused\" Then=\"‖ Service paused\" />\n  </codewf:Switch>\n</TextBlock.Text>"
        },
        "state": {
          "Running": "Running"
        },
        "StateOptions": {
          "Running": "Running",
          "Paused": "Paused",
          "Maintenance": "Maintenance",
          "Unknown": "Unknown"
        }
      },
      "PropertyGrid": {
        "View": {
          "PropertyGridDisplaysACompleteValvePropertyTreeTry": "PropertyGrid displays a complete valve property tree. Try filtering, view switching, edits, resets, and resizing the name column.",
          "ExpandAll": "Expand all",
          "CollapseAll": "Collapse all",
          "ResetAllChanges": "Reset all changes",
          "ValueChanges": "Value changes",
          "BrowseRequests": "Browse requests"
        },
        "Properties": {
          "ViewMode": "View mode",
          "Categorized": "Categorized",
          "Alphabetical": "Alphabetical",
          "CategorizedViewGroupsByCategoryAlphabeticalViewFlattens": "Categorized view groups by Category; alphabetical view flattens and sorts by name.",
          "ShowToolbar": "Show toolbar",
          "ToolbarForViewSwitchingExpandCollapseAndReset": "Toolbar for view switching, expand, collapse, and reset.",
          "ShowDescriptions": "Show descriptions",
          "ShowTheSelectedPropertySDescriptionBelowThe": "Show the selected property's description below the grid.",
          "NameColumnWidth": "Name column width",
          "InitialNameColumnWidthDragTheSeparatorTo": "Initial name column width; drag the separator to resize it."
        },
        "Source": {
          "CSharp": "var name = new PropertyItem { Id = \"n\", Name = \"Name\", Kind = PropertyItemKind.Text, Value = \"Valve-01\" };\nvar visible = new PropertyItem { Id = \"v\", Name = \"Visible\", Kind = PropertyItemKind.Boolean, Value = true };\nvar appearance = new PropertyItem\n{\n    Id = \"g1\", Name = \"Appearance\", Kind = PropertyItemKind.Category,\n    Children = [name, visible],\n};\n\nposition.SummaryProvider = () => $\"{posX.Value}, {posY.Value}\";\nbackoffFactor.VisibleWhen = () => Equals(backoff.Value, true);\n\ngrid.ExpandAll();\ngrid.CollapseAll();\ngrid.ResetAll();\ngrid.Select(item);\ngrid.CommitValue(item, newValue);\ngrid.ValueCommitted += (_, e) => Log(e.Item.Name, e.OldValue, e.NewValue);",
          "Xaml": "<codewf:PropertyGrid Items=\"{Binding Items}\"\n                     ViewMode=\"Categorized\"\n                     ShowToolBar=\"True\"\n                     ShowDescriptionPane=\"True\"\n                     SearchText=\"{Binding SearchText}\"\n                     ValueCommittedCommand=\"{Binding ValueCommittedCommand}\"\n                     BrowseRequestedCommand=\"{Binding BrowseRequestedCommand}\" />"
        },
        "OnPropertyCommitted": {
          "Alphabetical": "Alphabetical"
        }
      },
      "PropertyGridSample": {
        "Sample": {
          "Name": "Name",
          "Valve01": "Valve-01",
          "DisplayNameUsedOnTheCanvasAndIn": "Display name used on the canvas and in search results.",
          "Visible": "Visible",
          "HideTheNodeOnTheCanvasWhileKeeping": "Hide the node on the canvas while keeping computation and export enabled.",
          "FillColor": "Fill color",
          "NodeFillColorInRRGGBBFormat": "Node fill color in #RRGGBB format.",
          "Opacity": "Opacity",
          "NodeOpacityDragToPreviewAndReleaseTo": "Node opacity; drag to preview and release to commit.",
          "FontFamily": "Font family",
          "MicrosoftYaHei": "Microsoft YaHei",
          "SimSun": "SimSun",
          "FontFamilyUsedToRenderText": "Font family used to render text.",
          "FontSize": "Font size",
          "TextFontSize": "Text font size.",
          "Weight": "Weight",
          "Light": "Light",
          "Regular": "Regular",
          "Bold": "Bold",
          "FontWeight": "Font weight.",
          "Italic": "Italic",
          "RenderTextInItalic": "Render text in italic.",
          "Font": "Font",
          "GlobalTextFontSettings": "Global text font settings.",
          "ItalicSummary": " Italic",
          "Appearance": "Appearance",
          "NodeAppearanceProperties": "Node appearance properties.",
          "HorizontalNodeCoordinateOnTheCanvas": "Horizontal node coordinate on the canvas.",
          "VerticalNodeCoordinateOnTheCanvas": "Vertical node coordinate on the canvas.",
          "Position": "Position",
          "NodePositionOnTheCanvas": "Node position on the canvas.",
          "Width": "Width",
          "NodeWidth": "Node width.",
          "High": "High",
          "NodeHeight": "Node height.",
          "KeepAspectRatio": "Keep aspect ratio",
          "LockTheAspectRatioWhileResizing": "Lock the aspect ratio while resizing.",
          "Size": "Size",
          "NodeDimensionsAndConstraints": "Node dimensions and constraints.",
          "Rotation": "Rotation",
          "RotationAroundTheAnchorPositiveValuesAreCounterclockwise": "Rotation around the anchor; positive values are counterclockwise.",
          "CenterX": "Center X",
          "HorizontalRotationAnchorAsAPercentageOfWidth": "Horizontal rotation anchor as a percentage of width.",
          "CenterY": "Center Y",
          "VerticalRotationAnchorAsAPercentageOfHeight": "Vertical rotation anchor as a percentage of height.",
          "RotationAnchor": "Rotation anchor",
          "RotationAnchorInPercentageCoordinates": "Rotation anchor in percentage coordinates.",
          "Layout": "Layout",
          "IncludeInComputation": "Include in computation",
          "PreviewOnly": "Preview only",
          "Exclude": "Exclude",
          "IncludeTheNodeInSimulationAndResultExport": "Include the node in simulation and result export.",
          "Trigger": "Trigger",
          "Manual": "Manual",
          "Automatic": "Automatic",
          "Scheduled": "Scheduled",
          "TriggerThatStartsTheNode": "Trigger that starts the node.",
          "Priority": "Priority",
          "Medium": "Medium",
          "Low": "Low",
          "ExecutionOrderWithinASchedulingCycle": "Execution order within a scheduling cycle.",
          "MaximumRetries": "Maximum retries",
          "Retries": "retries",
          "MaximumAutomaticRetriesAfterFailure": "Maximum automatic retries after failure.",
          "RetryInterval": "Retry interval",
          "DelayBetweenRetries": "Delay between retries.",
          "ExponentialBackoff": "Exponential backoff",
          "MultiplyRetryDelaysByThisFactorToAvoid": "Multiply retry delays by this factor to avoid excessive load.",
          "BackoffFactor": "Backoff factor",
          "MultiplierAppliedToEachDelayWithExponentialBackoff": "Multiplier applied to each delay with exponential backoff.",
          "RetryPolicy": "Retry policy",
          "AutomaticRetryPolicyAfterExecutionFailure": "Automatic retry policy after execution failure.",
          "RetriesMsFormat": "{0} retries / {1}ms",
          "Note": "Note",
          "PeriodicallyCheckThatInletPressureMeetsOperatingRequirements": "Periodically check that inlet pressure meets operating requirements.",
          "AnnotationSavedWithTheModelExcludedFromComputation": "Annotation saved with the model; excluded from computation.",
          "Behavior": "Behavior",
          "FullPath": "Full path",
          "AbsoluteModelFilePathReadOnly": "Absolute model file path (read only).",
          "Encoding": "Encoding",
          "DocumentCharacterEncoding": "Document character encoding.",
          "OutputDirectory": "Output directory",
          "SimulationResultDirectory": "Simulation result directory.",
          "StyleSheet": "Style sheet",
          "BrowseStyleSheet": "Browse style sheet…",
          "StyleSheetUsedInTheNodeReportClick": "Style sheet used in the node report; click to select a file.",
          "ID": "ID",
          "UniquePortIdentifier": "Unique port identifier.",
          "Type": "Type",
          "Inlet": "Inlet",
          "Outlet": "Outlet",
          "Drain": "Drain",
          "DirectionOfFlowThroughThePort": "Direction of flow through the port.",
          "Diameter": "Diameter",
          "NominalPortDiameter": "Nominal port diameter.",
          "ConfigurationForFormat": "Configuration for {0}.",
          "Ports": "Ports",
          "NodePortsExpandToConfigureEachPort": "Node ports; expand to configure each port.",
          "Inlet1": "Inlet #1",
          "Outlet1": "Outlet #1",
          "DrainPort": "Drain port",
          "ItemsFormat": "{0} items",
          "File": "File"
        },
        "Content": {
          "BrowseFormat": "[{0:HH:mm:ss}] Browse: {1}"
        },
        "Describe": {
          "Empty": "(empty)"
        }
      },
      "ScenePageBase": {
        "GoToStep": {
          "WorkflowStepFormat": "Workflow step {0}/{1}: {2}"
        }
      },
      "SearchListBox": {
        "View": {
          "InteractionLog": "Interaction log",
          "Hint": "Hint",
          "SearchHasA400msDebounceUseCtrlShift": "Search has a 400ms debounce. Use Ctrl / Shift to select multiple items; double-click events appear on the right."
        },
        "Properties": {
          "Total": "Total",
          "NumberOfSourceItemsTotalCount": "Number of source items (TotalCount).",
          "Filtered": "Filtered",
          "MatchingItemsSearchCountWithABuiltIn400ms": "Matching items (SearchCount), with a built-in 400ms search debounce.",
          "SelectedItems": "Selected items",
          "NumberOfSelectedItemsSupportsCtrlShiftSelection": "Number of selected items; supports Ctrl / Shift selection."
        },
        "Content": {
          "DoubleClickFormat": "Double-click: {0}",
          "SelectionChangedItemsFormat": "Selection changed: {0} items"
        },
        "Source": {
          "CSharp": "var list = new SearchListBox { ItemsSource = items };\n\nvar total = list.TotalCount;\nvar visible = list.SearchCount;\n\nvar selected = list.SelectedItems;  // List<object?>\nlist.ListSelectionChanged += (item, args) => Refresh(args.Source.SelectedItems);\nlist.ListItemDoubleTapped += (item, args) => Open(item);\n\nlist.AddRange(newItems);\nlist.RemoveRange(selected);",
          "Xaml": "<codewf:SearchListBox ItemsSource=\"{Binding Items}\" />"
        },
        "UpdateMetrics": {
          "TotalFilteredSelectedFormat": "Total {0} · filtered {1} · selected {2}"
        },
        "CreateItems": {
          "Audio": "Audio",
          "Videos": "Videos",
          "Documents": "Documents",
          "Images": "Images",
          "Archives": "Archives",
          "AssetFormat": "Asset {0:000}"
        }
      },
      "StatusComponents": {
        "View": {
          "StatusBadgeAndStatusCardProvideFiveSemanticStatesAnd": "StatusBadge and StatusCard provide five semantic states and light/dark appearances. Configure the final card through the property panel.",
          "Debug": "Debug",
          "Information": "Information",
          "Warning": "Warning",
          "Error": "Error",
          "Critical": "Critical",
          "Debugging": "Debugging",
          "ServiceReady": "Service ready",
          "DiskSpaceLow": "Disk space low",
          "ServiceTimeout": "Service timeout",
          "DatabaseOffline": "Database offline",
          "DrivenByPropertyPanel": "Driven by property panel",
          "ChangeTheStatusAndIconWidthInThe": "Change the status and icon width in the property panel."
        },
        "Properties": {
          "CardStatus": "Card status",
          "Debug": "Debug",
          "Information": "Information",
          "Warning": "Warning",
          "Error": "Error",
          "Critical": "Critical",
          "FiveStatusLabelKindValuesChangeTheCardStateAnd": "Five StatusLabelKind values change the card state and appearance.",
          "IconWidth": "Icon width",
          "IconWidthOfTheConfigurableCard": "Icon width of the configurable card."
        },
        "Source": {
          "CSharp": "var card = new StatusCard\n{\n    Kind = StatusLabelKind.Warn,\n    TopText = \"WARN\",\n    BottomText = \"Less than 10% free disk space\",\n    IconWidth = 32,\n};\n\ncard.Kind = StatusLabelKind.Error;\n\nvar badge = new StatusBadge { LeftText = \"Build\", RightText = \"Example\" };",
          "Xaml": "<codewf:StatusBadge LeftText=\"Info\" RightText=\"Information\" />\n\n<codewf:StatusCard Kind=\"Warn\"\n                   TopText=\"WARN\"\n                   BottomText=\"Less than 10% free disk space\"\n                   IconWidth=\"32\" />"
        },
        "OnPropertyCommitted": {
          "Information": "Information",
          "Warning": "Warning",
          "Error": "Error",
          "Critical": "Critical"
        }
      },
      "Transfer": {
        "View": {
          "MoveSelectedItemsBetweenTwoSearchableListsUsing": "Move selected items between two searchable lists using the middle buttons or a double-click. Each side uses SearchListBox.",
          "FeatureBacklogAvailable": "Feature backlog (available)",
          "CurrentDeliverySelected": "Current delivery (selected)",
          "LiveCounts": "Live counts",
          "Available": "Available: ",
          "Selected": "Selected: ",
          "OperationLog": "Operation log",
          "TheControlMovesItemsBetweenSourceCollectionsAnd": "The control moves items between source collections and updates counts automatically."
        },
        "Properties": {
          "AvailableCount": "Available count",
          "NumberOfAvailableItemsOnTheLeft": "Number of available items on the left.",
          "SelectedCount": "Selected count",
          "NumberOfSelectedItemsOnTheRight": "Number of selected items on the right."
        },
        "Source": {
          "CSharp": "var transfer = new Transfer\n{\n    LeftHeader = \"Available\",\n    RightHeader = \"Selected\",\n    LeftItemsSource = available,   // ObservableCollection<FeatureItem>\n    RightItemsSource = selected,\n};\n\ntransfer.MoveLeftToRight();\ntransfer.MoveRightToLeft();\n\n\nselected.CollectionChanged += (_, e) => Save(selected);",
          "Xaml": "<codewf:Transfer LeftHeader=\"Feature backlog (available)\"\n                 RightHeader=\"Current delivery (selected)\"\n                 LeftItemsSource=\"{Binding Available}\"\n                 RightItemsSource=\"{Binding Selected}\" />"
        },
        "OnCollectionChanged": {
          "MoveIn": "Move in",
          "MoveOut": "Move out",
          "AvailableSelectedFormat": "{0}: {1} · available {2} / selected {3}"
        },
        "Available": {
          "IncrementalRendering": "Incremental rendering",
          "Completed": "Completed",
          "BlockVirtualization": "Block virtualization",
          "18TypographyThemes": "18 typography themes",
          "PDFExport": "PDF export",
          "WordExport": "Word export",
          "RichTextPublishing": "Rich-text publishing",
          "CollaborativeAnnotations": "Collaborative annotations",
          "Planned": "Planned",
          "VersionComparison": "Version comparison",
          "AISummaries": "AI summaries",
          "HandwrittenFormulaRecognition": "Handwritten formula recognition"
        }
      },
      "TrapezoidTab": {
        "View": {
          "TrapezoidControlThemeAdaptsTheFirstMiddleAndFinal": "Trapezoid ControlTheme adapts the first, middle, and final tab edges. A matching TabStrip theme is used in the Ribbon.",
          "TabControlTrapezoidTheme": "TabControl (trapezoid theme)",
          "Home": "Home",
          "FirstTabFlushLeftEdgeAndSlantedRight": "First tab: flush left edge and slanted right edge.",
          "Design": "Design",
          "MiddleTabTwoSlantedEdgesDiagonalFilletRatioSetsThe": "Middle tab: two slanted edges; DiagonalFilletRatio sets the angle.",
          "Development": "Development",
          "MiddleTabHighlightedOutlineWhenSelected": "Middle tab: highlighted outline when selected.",
          "Release": "Release",
          "MiddleTabHoverAndPressedFeedback": "Middle tab: hover and pressed feedback.",
          "About": "About",
          "LastTabFlushRightEdgeAndSlantedLeft": "Last tab: flush right edge and slanted left edge.",
          "TabStripStandalone": "TabStrip (standalone)",
          "Overview": "Overview",
          "Details": "Details",
          "Statistics": "Statistics",
          "TabStripManagesSelectionTheHostSuppliesTheAssociated": "↑ TabStrip manages selection; the host supplies the associated content."
        },
        "Properties": {
          "ThemeKeys": "Theme keys",
          "ACSemiThemeInjectsTheResourceKeysForUseWith": "ACSemiTheme injects the resource keys for use with StaticResource."
        },
        "Source": {
          "CSharp": "//\ntabControl.Theme = (ControlTheme)Application.Current.Resources[\"TrapezoidShapedTabControl\"]!;",
          "Xaml": "<TabControl Theme=\"{StaticResource TrapezoidShapedTabControl}\">\n  <TabItem Header=\"Home\" />\n  <TabItem Header=\"Design\" />\n  <TabItem Header=\"Release\" />\n</TabControl>\n\n<TabStrip Theme=\"{StaticResource TrapezoidShapedTabStrip}\">\n  <TabStripItem Content=\"Overview\" />\n  <TabStripItem Content=\"Details\" />\n</TabStrip>"
        }
      },
      "Welcome": {
        "View": {
          "EditPreviewExport": "Edit · preview · export",
          "ThreeStateNaturalSorting": "Three-state · natural sorting",
          "DockFloatDocuments": "Dock · float · documents",
          "SearchTransferStatus": "Search · transfer · status",
          "TypedPropertiesLiveCommits": "Typed properties · live commits",
          "LogStreamsRuntimeLocalization": "Log streams · runtime localization"
        },
        "markdown": {
          "GiveInterfacesExpressionBuildDesktopExperiencesWithCodeWF": "# Give interfaces expression\n\nBuild desktop experiences with **CodeWF.Avalonia**.\n\n- Live Markdown preview\n- Three-state sorting and property editing\n- `Dock` multi-document workspace"
        }
      },
      "WindowedStackPanel": {
        "View": {
          "Generate5000Blocks": "Generate 5000 blocks",
          "Clear": "Clear",
          "WindowedStackPanelRealizesBlocksNearTheViewportWithOverscan": "WindowedStackPanel realizes blocks near the viewport, with Overscan buffers. Observe the metrics while scrolling."
        },
        "Properties": {
          "Virtualization": "Virtualization",
          "KeepAllBlocksInTheVisualTreeWhen": "Keep all blocks in the visual tree when disabled; compare with 5000 blocks.",
          "VirtualizationThreshold": "Virtualization threshold",
          "EnableWindowedRealizationAboveThisBlockCount": "Enable windowed realization above this block count.",
          "Overscan": "Overscan",
          "ExtraViewportBuffersAboveAndBelowTheVisible": "Extra viewport buffers above and below the visible area.",
          "BlockSpacing": "Block spacing",
          "SpacingBetweenAdjacentBlocks": "Spacing between adjacent blocks."
        },
        "Source": {
          "CSharp": "var panel = new WindowedStackPanel\n{\n    EnableVirtualization = true,\n    VirtualizationThreshold = 40,\n    OverscanViewports = 2,\n    Spacing = 8,\n};\nscrollViewer.Content = panel;\n\nfor (var i = 0; i < 5000; i++)\n{\n    panel.AddItem(new TextBlock { Text = $\"Example\" });\n}\n\npanel.ItemCount;           // 5000\npanel.RealizedItemCount;\npanel.IsVirtualizing;\n\npanel.RealizeItem(100);\npanel.TryGetItemTop(100, out var top);\npanel.ScrollHost?.Offset = new Vector(0, top);\npanel.ClearItems();",
          "Xaml": "<ScrollViewer>\n  <codewf:WindowedStackPanel x:Name=\"BlocksPanel\"\n                             EnableVirtualization=\"True\"\n                             VirtualizationThreshold=\"40\"\n                             OverscanViewports=\"2\"\n                             Spacing=\"8\" />\n</ScrollViewer>"
        },
        "OnGenerate": {
          "BlockFormat": "Block {0} / {1}",
          "WindowedStackPanelGeneratedBlocksFormat": "WindowedStackPanel: generated {0} blocks"
        },
        "UpdateMetrics": {
          "Yes": "Yes",
          "No": "No",
          "BlocksRealizedVirtualizingFormat": "Blocks: {0} · realized: {1} · virtualizing: {2}"
        }
      }
    },
    "Scene": {
      "Step.Open": "Open doc",
      "Step.Edit": "Edit",
      "Step.Preview": "Live preview",
      "Step.Export": "Export / copy",
      "Step.WriteBack": "Task write-back",
      "Step.Import": "Import",
      "Step.Columns": "Columns",
      "Step.SortFilter": "Sort / filter",
      "Step.ExportData": "Export",
      "Step.Dock": "Dock layout",
      "Step.Tabs": "Doc tabs",
      "Step.Panels": "Panel sync",
      "Step.Status": "Status",
      "Summary.Markdown": "5-step writing flow · each step reuses the component pages below",
      "Summary.Data": "4-step data flow · wired to the property panel",
      "Summary.Docs": "4-step multi-document flow · panels and tabs working together"
    },
    "Segment": {
      "Demo": "Demo",
      "Xaml": "XAML",
      "CSharp": "C#"
    }
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
