# CodeWF.Avalonia 原生 Demo

Demo 使用 Ribbon + Dock，以单个控件或公开 API 为导航单位。8 个分类、32 个页面由 `design/catalog.json` 管理；组合工作流放在“综合示例”，首页直接展示目录。

## 文档与参数

每个演示独占一个 Dock Document，内部使用 `Grid + GridSplitter` 组合效果区和属性面板。浮动、重新停靠、切换主窗口活动文档时，参数绑定、属性区宽度及显隐仍属于原文档。

- 首屏展示真实控件、输入和结果，源码按需展开。介绍页提供全局设置；其他页面仅显示自身参数，无参数时保持空面板。
- 属性面板使用 Semi 分类和排序图标；搜索独占下一行。文本、数值和下拉编辑器铺满单元格、默认无边框，下拉内容左对齐；复选和单选保持常规外观。
- 表格具有横竖分隔线，Demo 默认行高 31px、名称列宽 118px；可拖动名称和值之间的分隔条调整列宽，可拖动行底边调整行高。
- Ribbon 组仅显示标题；命令图标使用 Semi 矢量资源，统一 32×32。快捷图标为 16×16，按钮热区 30×30。密集组双行排列，长 API 名不换行，窄窗口横向滚动。

## 主题与国际化

标题栏色圈和下拉共用配色状态，提供 .NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏、夜空八套配色。浅色、深色和跟随系统独立选择，标题栏与属性面板同步；切换语言保留模式选中项。

`StudioTheme` 与 `StudioThemeController` 位于 `CodeWF.Avalonia.Controls`。在 Semi 与 CodeWF 控件主题后加载 StudioTheme，控制器使用显式枚举选择独立浅深资源，并订阅平台主题变化；退出时释放订阅。新增配色遵循 Semi 的资源字典结构，不按 RGB 字符串判断主题。

NuGet 库的语言 JSON 内嵌程序集，Demo 自身的 JSON 输出到 `I18n/`。应用通过 Lang 正常加载自身资源即可按同键覆盖库文案，无需 resolver 或逐库注册。资源键由 T4 生成，普通构建直接消费已提交的生成文件。

```powershell
pwsh scripts/sync_localization_keys.ps1
pwsh scripts/sync_localization_keys.ps1 -Check
```

业务状态与属性提交放在 ViewModel；AXAML 后置仅保留视图接线及窗口、剪贴板等平台适配。切换语言保留用户输入、执行结果和命令实例。

## 目录与浏览器门禁

`design/catalog.json` 是 Demo 的构建输入。页面工厂、导航标识及图标由目录生成，使用显式调用与源生成 JSON，避免 AOT 中反射发现页面。原生 `iconKey` 引用 Semi 图标，浏览器使用对应 MIT 图形路径。

```powershell
pwsh scripts/sync_design_catalog.ps1
pwsh scripts/sync_design_catalog.ps1 -Check
npm ci --prefix design/tests
# 首次安装后，在 design/tests 中执行 npx playwright install chromium
npm test --prefix design/tests
```

浏览器门禁覆盖全部路由 × 中英文 × 1440/1120 两种宽度，共 128 次检查，另验八套配色的浅深组合与系统模式变化，验证实际 Ribbon/导航点击、内容、源码、下拉对齐、属性搜索/排序、列宽/行高拖拽和脚本异常。浏览器原型不执行 .NET API；真实行为由原生 Demo 验证。

## Windows NativeAOT

本轮版本 `12.1.4.18`，环境为 Windows x64 / .NET SDK 10.0.303。`global.json` 固定稳定版 .NET 10。

```powershell
dotnet test CodeWF.Avalonia.slnx -c Release
pwsh scripts/publish_avalonia_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Avalonia.Demo/CodeWF.Avalonia.Demo.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture E:/github/temp/CodeWF.Avalonia.NativeQA' -Wait -PassThru
```

`--smoke` 验证目录页面、主题渲染、外壳命令、真实 Document 浮动与参数隔离。`--capture` 使用固定布局，输出页面、双语 Ribbon、深浅配色及紧凑窗口截图，并校验图标尺寸。README 的 PNG 来自原生 EXE；GIF 分别用于真实展开动画或主题截图巡览。

Windows x64 NativeAOT 冒烟与截图均退出 0，32 个页面 × 16 配色组合共 512 次渲染零失败；真实文档浮动、参数隔离、语言切换后的模式选择及 Markdown 动态国际化均通过。

本轮库测试 334 项通过（Content 267、Controls 67）。Markdown 额外在同一个已挂载控件中切换中、英、日、繁体中文及未配置语言，检查菜单、选择菜单、代码按钮、英文回退与宿主资源覆盖；也覆盖离开界面后切换语言、重新挂载时的刷新。

## 验收边界

字体度量、Dock 页签与窗口行为由 Semi/Dock 和操作系统决定，原型不承诺逐像素一致。第三方 Prism 默认对话框、旧运行时 XAML 加载器及部分 Markdown/反射路径仍有 AOT/裁剪分析警告；本轮只验证 Demo 实际调用路径。

本机未运行 Linux/macOS UI，未完成跨屏、跨网段、断网重连和长时间负载验收；页面渲染成功不等于所有公开 API 及异常路径均已验证。

快捷区使用 Semi 线性图标、2px 间距和独立分隔留白；鼠标悬停、按下及键盘焦点各有反馈。Ribbon 分类复用 Semi LineTabStrip，单层 13px 水平内边距。标题栏通知入口复用 Semi 图标；导航叶子与 Ribbon 使用同一目录图形。应用默认字体覆盖 Dock 浮动窗和弹出层，代码区域仍使用等宽字体。

本次视觉校准已执行仓库发布脚本，Windows x64 NativeAOT 冒烟与截图退出 0。截图检查包括 16×16 图标、30×30 热区、32×32 主命令，以及中英文、1440×860 与 1120×640 窗口；README 中的 PNG/GIF 来自本次 AOT 程序输出。CodeWFTitleBar 的浮动文档用例同时检查属性修改后的标题实际可见，控件页不再重复展示属性输入区。原型文件保持固定版本。

本轮桌面工具捕获到锁屏，因此未重新执行人工鼠标、键盘操作；程序内部的原生布局、浮动窗口、渲染与 API 冒烟仍已验证。桌面解锁后需补充快捷入口悬停、按下及键盘焦点检查，多 DPI 和跨屏交互仍未验收。

## 2026-10-15 原型细节复核

- 固定原型保持原样。Ribbon 左侧三个快捷图标，右侧放置命令、密度、保存；选中反馈覆盖整块命令按钮，组标题无图标。
- 文档左右两列独立排版，标题、效果和源码属于左列，属性面板从文档顶部开始；打开源码不会移动右侧属性面板。
- 日志区复用 LogView 右键菜单，不重复标题、路径与写示例日志工具行；最小高度 84px，避免窄窗口只剩半行内容。
- 中文字形显式回退到微软雅黑、苹方或 Noto CJK，修正字号相同但中文显示成宋体的偏差。

本轮后台门禁：原型全部路由 × 中英 × 两种宽度通过。原生验证分别记录构建、NativeAOT 冒烟、窗口参数传递和离屏渲染结果；锁屏期间不执行人工桌面操作，多 DPI 与跨屏检查仍待补充。
