# CodeWF.Avalonia 原生 Demo

Demo 使用 Ribbon + Dock，以单个控件或公开 API 为导航单位。8 个分类、32 个页面由 `design/catalog.json` 管理；组合工作流放在“综合示例”，首页直接展示目录。

## 文档与参数

每个演示独占一个 Dock Document，内部使用 `Grid + GridSplitter` 组合效果区和属性面板。浮动、重新停靠、切换主窗口活动文档时，参数绑定、属性区宽度及显隐仍属于原文档。

- 首屏展示真实控件、输入和结果，源码按需展开。介绍页提供全局设置；其他页面仅显示自身参数，无参数时保持空面板。
- 属性面板使用 Semi 分类和排序图标；搜索独占下一行。文本、数值和下拉编辑器铺满单元格、默认无边框，下拉内容左对齐；复选和单选保持常规外观。
- 表格具有横竖分隔线，可拖动名称和值之间的分隔条调整列宽，可拖动行底边调整行高。
- Ribbon 组仅显示标题；命令图标使用 Semi 矢量资源，统一 32×32。快捷图标为 24×24，按钮热区 34×34。密集组双行排列，长 API 名不换行，窄窗口横向滚动。

## 主题与国际化

标题栏色圈和下拉共用配色状态，提供 .NET 紫、AI 极光、AI 暖陶及既有 Semi 配色。新增配色遵循 Semi 的资源字典结构，通过显式 `ThemeVariant` 加载；不按 RGB 字符串判断主题。

`StudioTheme` 位于 `CodeWF.Avalonia.Controls`，在 Semi 与 CodeWF 控件主题后加载。`DotNetLight/Dark`、`AuroraLight/Dark`、`EmberLight/Dark` 分别继承浅色与深色变体；`StudioBackground`、`StudioSurface`、`StudioText`、`StudioAccent` 等资源用于应用外壳。

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

浏览器门禁覆盖全部路由 × 中英文 × 1440/1120 两种宽度，共 128 次检查，验证实际 Ribbon/导航点击、内容、源码、下拉对齐、属性搜索/排序、列宽/行高拖拽和脚本异常。浏览器原型不执行 .NET API；真实行为由原生 Demo 验证。

## Windows NativeAOT

本轮版本 `12.1.4.12`，环境为 Windows x64 / .NET SDK 10.0.303。`global.json` 固定稳定版 .NET 10。

```powershell
dotnet test CodeWF.Avalonia.slnx -c Release
pwsh scripts/publish_avalonia_demo.ps1 -RuntimeIdentifier win-x64
$demo = './artifacts/publish/win-x64/CodeWF.Avalonia.Demo/CodeWF.Avalonia.Demo.exe'
Start-Process $demo -ArgumentList '--smoke' -Wait -PassThru
Start-Process $demo -ArgumentList '--capture ./artifacts/studio-qa' -Wait -PassThru
```

`--smoke` 验证目录页面、主题渲染、外壳命令、真实 Document 浮动与参数隔离。`--capture` 使用固定布局，输出页面、双语 Ribbon、深浅配色及紧凑窗口截图，并校验图标尺寸。README 的 PNG 来自原生 EXE；GIF 分别用于真实展开动画或主题截图巡览。

本轮库测试 304 项通过（Content 241、Controls 63）。Markdown 额外在同一个已挂载控件中切换中、英、日、繁体中文及未配置语言，检查菜单、选择菜单、代码按钮、英文回退与宿主资源覆盖；也覆盖离开界面后切换语言、重新挂载时的刷新。

## 验收边界

字体度量、Dock 页签与窗口行为由 Semi/Dock 和操作系统决定，原型不承诺逐像素一致。第三方 Prism 默认对话框、旧运行时 XAML 加载器及部分 Markdown/反射路径仍有 AOT/裁剪分析警告；本轮只验证 Demo 实际调用路径。

本机未运行 Linux/macOS UI，未完成跨屏、跨网段、断网重连和长时间负载验收；页面渲染成功不等于所有公开 API 及异常路径均已验证。
