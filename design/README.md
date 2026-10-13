# CodeWF.Avalonia Demo 设计

直接打开 `CodeWF.Avalonia.Demo原型.html`。原型与原生 Demo 共用 `catalog.json` 目录；Ribbon 与导航树读取同一份分类。

## 信息架构

按功能分类，每个主导航叶子对应一个具体控件或公开 API；介绍、关于和综合示例是明确的辅助入口。保留 Ribbon + Dock 外壳，将组合工作流集中到最后一组，首页先展示 API 目录，再提供默认折叠的实验区。

| Ribbon 分类 | 菜单对应 API |
|---|---|
| 首页 / Home | `介绍`、`关于` |
| 按钮与输入 / Buttons & Inputs | `SearchListBox`、`Transfer`、`ColumnDisplayEditor` |
| 对话与反馈 / Dialogs & Feedback | `StatusBadge`、`StatusCard`、`Guide`、`LogNotifications` |
| 数据展示 / Data Display | `DataGridEnhancement`、`TreeDataGridExtension`、`PropertyGrid`、`LogView` |
| 布局与导航 / Layout & Navigation | `CodeWFWindow`、`CodeWFTitleBar`、`DockControl`、`TrapezoidShapedTabControl`、`WindowedStackPanel`、`LeftTop2RightBottomLine` |
| Markdown / Markdown | `MarkdownViewer`、`Lite.MarkdownViewer`、`MarkdownEditorView`、`MarkdownLiveEditorView` |
| 开发工具 / Developer Tools | `If`、`Switch`、`I18nManager`、`MarkdownDocumentExporter` |
| 综合示例 / Examples | `Markdown 工作流`、`数据工作流`、`多文档工作流`、`应用级引导示例` |

共 8 个分类、31 个页面。文档区默认完整展示功能演示；源码面板由“查看源码”打开，只提供有内容的 XAML / C# 标签。页面标题展示 API 名、NuGet 包与功能说明。

## 配色与交互

默认 .NET 品牌紫 `#512BD4`，另有 AI Aurora（紫 / 青）与 AI Ember（暖陶）视觉主题，均支持深浅切换。AI 配色是视觉设计名称。标题栏色圈与下拉框共享选中状态，继续提供 Semi 原有配色。

保留主题、语言、密度、命令搜索、布局存档、Dock 面板显隐与引导。页面进入和按钮悬停有动画，并遵循浏览器“减少动态效果”设置。主导航菜单与首页目录点击后同步当前 Ribbon 分类。

## 维护

1. 在 `catalog.json` 维护分类、API、NuGet 包、页面工厂说明和原型路由。
2. 在 Demo 的 `I18n/` 配置中维护双语标题与 `Demo.Api.*.Desc` 功能说明。
3. 为原生 `DemoCatalog` 添加显式页面工厂，为原型注册对应演示。
4. 执行 `pwsh scripts/sync_design_catalog.ps1` 更新浏览器目录与文案。
5. 执行目录一致性测试与 Demo 冒烟检查。

`assets/catalog-data.js` 是生成文件；`catalog-gallery.js` 和 `catalog-gallery.css` 负责目录布局与浏览器预览。各仓库独立携带所有原型资源。

## 实现与验证边界

浏览器原型展示交互与布局，不运行 .NET、原生窗口、Socket 或文件导出。页面明确提示仅为浏览器预览；实际 API 结果在原生 Demo 中产生。原生控件使用 Semi 与 Dock 的模板，字体、窗口按钮和平台交互存在差异。

浏览器逐页检查 31 个路由、中英文和导航切换，未发现脚本异常。NativeAOT 运行范围、命令与限制见 [原生 Demo 文档](../docs/NativeDemo.md)。
