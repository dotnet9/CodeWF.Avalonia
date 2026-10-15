# CodeWF.Avalonia Demo 设计

直接打开 `CodeWF.Avalonia.Demo原型.html`。原型与原生 Demo 共用 `catalog.json` 目录；Ribbon 与导航树读取同一份分类。

## 信息架构

按功能分类，每个主导航叶子对应一个具体控件或公开 API；介绍、关于和综合示例是明确的辅助入口。保留 Ribbon + Dock 外壳，将组合工作流集中到最后一组，首页先展示 API 目录，再提供默认折叠的实验区。

| Ribbon 分类 | 菜单对应 API |
|---|---|
| 首页 / Home | `介绍`、`关于` |
| 输入与选择 / Inputs & Selection | `SearchListBox`、`Transfer` |
| 反馈与引导 / Feedback & Guidance | `StatusBadge`、`StatusCard`、`Guide`、`LogNotifications` |
| 数据展示 / Data Display | `DataGridEnhancement`、`ColumnDisplayEditor`、`TreeDataGridExtension`、`PropertyGrid`、`LogView` |
| 布局与导航 / Layout & Navigation | `CodeWFWindow`、`CodeWFTitleBar`、`DockControl`、`TrapezoidShapedTabControl`、`WindowedStackPanel`、`LeftTop2RightBottomLine`、`ContentExpansionAnimator` |
| Markdown / Markdown | `MarkdownViewer`、`Lite.MarkdownViewer`、`MarkdownEditorView`、`MarkdownLiveEditorView`、`MarkdownDocumentExporter` |
| 开发工具 / Developer Tools | `If`、`Switch`、`I18nManager` |
| 综合示例 / Examples | `Markdown 工作流`、`数据工作流`、`多文档工作流`、`应用级引导示例` |

共 8 个分类、32 个页面。文档区默认完整展示功能演示；源码面板由“查看源码”打开，只提供有内容的 XAML / C# 标签。页面标题展示 API 名、NuGet 包与功能说明。

每个演示文档包含效果区、可拖动分隔条和自己的属性区；拖成独立窗口后两区保持在同一文档内。属性搜索放在排序按钮下一行，下拉文本左对齐，值编辑器无边框铺满单元格，表格显示横竖分隔线并支持拖拽列宽和行高。原型路由审计同时验证这些交互。

属性适配位于 `scripts/prototype/assets/catalog-properties.js`，以原生页面 `BuildProperties()` 的参数种类、范围和顺序为准，文案直接读取 Demo 中英文 JSON。无页面参数时显示全局外观设置；只读计数随搜索、选择和穿梭操作更新。工作流属性直接委托当前步骤的控件页面。`design/tests/audit-properties.cjs` 检查全部路由的参数契约，并实际编辑状态、标题、线宽、表格、窗口、日志、虚拟列表和 Markdown 参数检查效果，不以“页面非空”代替功能验证。浏览器虚拟列表使用窗口化 DOM，Markdown 使用浏览器可见性优化；原生 API、AOT 和桌面行为仍由原生验收负责。

## 配色与交互

默认 .NET 品牌紫 `#512BD4`，常规配色共八套：.NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏、夜空。浅色、深色、跟随系统独立选择；平铺色圈和文档属性共用状态，系统模式响应系统主题变化，布局保存模式选择。AI 配色是视觉设计名称。

保留主题、语言、密度、命令搜索、布局存档、Dock 面板显隐与引导。页面进入和按钮悬停有动画，并遵循浏览器“减少动态效果”设置。主导航菜单与首页目录点击后同步当前 Ribbon 分类。

## 维护

1. 在 `catalog.json` 维护分类、API、包名、页面构造表达式、路由与图标。`icon` 是导航字形，`iconKey` 引用 Semi 原生矢量资源，`prototypeIcon` 是对应 MIT 图标的浏览器 SVG 内容，两者均由目录管理。
2. 在 Demo 的 `I18n/` JSON 中维护中英文标题和说明。资源键使用稳定的业务语义，例如 `Demo.Shell.GlobalSettings.PaletteLabel`；不要使用方法名加位置编号，修改文案时保留原键。
3. 实现原生页面，并为原型注册交互演示。执行 `pwsh scripts/sync_design_catalog.ps1`，同步浏览器数据与 `DemoCatalog.g.cs`。页面标识 `DemoPages` 和显式工厂由脚本生成，无需手工维护 switch 或图标字典。
4. 执行 `pwsh scripts/sync_design_catalog.ps1 -Check`、目录一致性测试和原生 Demo 冒烟。生成文件需要提交，正常 `dotnet build` 不依赖 PowerShell 或 Node.js。
5. 执行 `npm ci --prefix design/tests`，在 `design/tests` 运行 `npx playwright install chromium` 和 `npm test`。浏览器门禁遍历全部路由 × 中英文 × 1440/1120 两种宽度，检查实际 Ribbon/导航点击、页面内容、源码面板和脚本错误。失败截图保存在 `design/tests/artifacts/`；CI 使用同一脚本。

`design/catalog.json` 是 Demo 的嵌入式构建输入，属于仓库源码，不能当草稿删除。`scripts/prototype/assets/catalog-data.js` 与 `DemoCatalog.g.cs` 是生成文件；`catalog-gallery.js/css` 负责浏览器布局。每个仓库独立携带全部资源。资源文件采用 UTF-8，允许有或无 BOM；测试检查无效字节和替换字符，但不能识别所有已经被错误转码的合法文字。

## 实现与验证边界

浏览器原型展示交互与布局，不运行 .NET、原生窗口、Socket 或文件导出。页面明确提示仅为浏览器预览；实际 API 结果在原生 Demo 中产生。原生控件使用 Semi 与 Dock 的模板，字体、窗口按钮和平台交互存在差异。

浏览器逐页检查 32 个路由、中英文和导航切换，未发现脚本异常。NativeAOT 运行范围、命令与限制见 [原生 Demo 文档](../docs/NativeDemo.md)。

## 重点页面用例

| API | 原型与原生共同表达的用例 |
|---|---|
| MarkdownLiveEditorView | 单栏直接编辑块、失焦渲染、定位首块、插入段落和表格；不使用双栏源码 / 预览模型 |
| ContentExpansionAnimator | 水平 / 垂直展开、快速反向、尺寸与时长调节、关闭动画 |
| TreeDataGridExtension | 平面数据源、三态自然排序、Ctrl+A 多选、长文本截断提示与选择计数 |
| StatusCard | 180×80 状态样例、相同双语内容、动态 Kind；保留公开控件本身的模板 |
| MarkdownViewer | 首次打开先显示渲染结果；源码输入折叠，保留增量追加、任务回写与排版配置 |
| If / Switch | 使用有效的 MarkupExtension 表达式，源码与原生演示一致 |

`scripts/prototype/assets/control-examples.js` 维护这些具体交互。Ribbon 分类、当前命令、导航叶子的选中状态都跟随活动文档；原生 Dock 页签切换也更新对应菜单。介绍页提供全局设置，其余页面只显示自身参数，无参数时保持空面板。

## 设计判断与验收标准

分类以可独立使用的控件 / API 为单位，包名作为页面上的查找线索；模板部件、数据模型和引导指示器等配套类型在所属控件页表达。介绍、关于及综合示例不计入控件覆盖率。

默认文档优先展示真实控件、可操作参数及结果。源码按需打开，右侧仅放当前页面参数，介绍页提供全局设置；多个方法和状态放在同页，避免为了增加页面数量拆散一个控件。Markdown 块编辑、平面表格增强和展开动画采用各自实际支持的交互模型。

还原检查包括首屏可见、选中状态、裁切、深浅主题、中英文、紧凑窗口，以及输入后结果是否变化。按钮和控件继续使用 Semi 模板；浏览器原型与原生字体、Markdown 渲染和窗口行为存在平台差异，不以所有像素相同作为验收承诺。


## 排版与 Ribbon 校准

保留 8 个一级分类，在命令区显示明确的功能子组；每个菜单仍对应单个 API。超过 7 项的分类使用双行命令，保留完整 API 名，窗口较窄时横向滚动。分类标题 12px、命令 11px、子组标题 10px；标题栏 52px、分类行 42px、命令区约 104px。页头标题 19px、说明 12px，导航叶子 11.5px。

原型对照时裁掉 HTML 的装饰性桌面背景，使用 1440×860 和 1120×640 两种窗口尺寸、中英文 64 个 Ribbon 状态。原生继续使用 Semi / Dock 模板；窗口按钮、PropertyGrid 编辑器和平台字体的几何不做 HTML 仿制。不能把路由检查或截图数量当作全状态逐像素相同的证明。

Markdown 导出归到 Markdown。阅读器、Lite 和编辑器的原型样例从原生双语资源生成；默认 Basic 排版正文 16px、一级标题 32px、二级标题 24px。任务勾选会写回原型输入，编辑器和块编辑器展示同一份文档。

属性栏只显示该页参数，介绍页提供全局外观与语言设置；不额外堆放空参数和状态统计。全局标签、密度选项和配色名称也从原生语言资源生成。

Ribbon 命令图标统一为 32×32 Semi 矢量图标，快捷图标 18×18、按钮热区 30×30；组仅显示标题。Semi 图形资源遵循其 MIT 许可。

快捷区使用 Semi 线性图标、2px 间距和独立分隔留白；鼠标悬停、按下及键盘焦点各有反馈。Ribbon 分类复用 Semi LineTabStrip，单层 13px 水平内边距。标题栏密度与通知入口复用 Semi 图标；导航叶子与 Ribbon 使用同一目录图形。应用默认字体覆盖 Dock 浮动窗和弹出层，代码区域仍使用等宽字体。

## 单文件交付

`design/` 中的主 HTML 已内嵌样式、脚本、图标和图片。只复制这一个 HTML 到空目录即可离线查看并操作，无需旁边的资源文件或本地服务。

维护源码放在 `scripts/prototype/`；执行 `pwsh scripts/build_standalone_prototype.ps1` 重新生成，`-Check` 校验生成文件。不要直接修改交付 HTML。交互审计会复制 HTML 到空目录，阻止其他文件和网络资源请求，并检查脚本错误与图片完整性。

标题栏交互采用单排八色点、Ursa 风格三态模式按钮、语言图标弹出列表；不再重复显示模式或配色下拉。窗口外框与三个窗口按钮属于桌面模拟，可最小化、最大化、关闭并恢复。
