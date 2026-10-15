# CodeWF.Avalonia 原生 Demo

当前版本 `12.1.4.23`。Demo 使用 Ribbon + Dock，以单个控件或公开 API 为导航单位：8 个分类、32 个页面。目录由 `design/catalog.json` 统一维护，它也是 Demo 的构建输入，不能在构建前删除。

## 窗口与演示

- 每个演示是一个 Dock Document；内部使用 `Grid + GridSplitter` 放置内容和属性面板。分屏、浮动、重新停靠时使用 Dock 官方内容回收模板，保留同一份视图、参数和属性状态。
- 文档组位于独立且不可折叠的中央比例容器内。文档分屏后再浮动，不会把导航区一起归一化；回归同时检查默认份额和手动调整后的份额及实际宽度。
- 首屏聚焦真实功能、输入和结果，源码按需展开；介绍页提供全局设置，其他页面仅显示自身参数。
- Ribbon 组只显示标题；主命令图标 32×32，快捷图标 18×18、点击热区 30×30。密集分组双行排列，窄窗口可以横向滚动。
- 属性面板使用 Semi 图标，搜索独占下一行。名称列默认 118px、行高 31px；横竖分隔线支持拖动调整列宽和行高。文本、数值、下拉编辑器铺满值单元格，默认无边框；下拉文本左对齐。

## 外观与国际化

标题栏只保留一排八色点，不再重复放置配色下拉。配色包括 .NET 紫、AI 极光、AI 赤陶、默认蓝、水生、沙漠、黄昏和夜空。

模式直接复用 Ursa `ThemeToggleButton`，按系统 → 浅色 → 深色循环。按钮放在独立 `ThemeVariantScope` 中，通过 `StudioThemeModeConverter` 绑定原有模式；应用仍由 `StudioThemeController` 管理自定义配色和系统主题变化，按钮不会把配色重置成默认主题。

语言图标点击后展开语言列表，文本左对齐。列表来自宿主的语言集合，增加语言无需重写标题栏控件；当前 Demo 提供简体中文和英文。

全局只使用 `CodeWF.Toolkit.Lang.I18nManager.Instance`。库的 JSON 内嵌程序集，宿主的 JSON 输出到 `I18n/`，优先级为宿主输出 > 外层库 > 内层库；强类型键由 T4 生成。宿主正常加载资源后即可同键覆盖库文案，无需 resolver 或事件桥接。

## 验证与发布

```powershell
pwsh scripts/sync_localization_keys.ps1 -Check
pwsh scripts/publish_avalonia_demo.ps1 -RuntimeIdentifier win-x64
# 本机并行开发可增加 -BuildArtifactsPath E:/github/temp/CodeWF.Avalonia-build
```

`scripts/localization-verification` 加载真实 App、XAML 和 Skia，在锁屏期间离屏运行；验证八色点、Ursa 内部按钮的实际点击、语言列表、1120/1440 宽度，以及 Dock 分屏 → 浮动 → 重新停靠。页面覆盖 1024 个页面/语言/配色组合。渲染数量只表示覆盖范围，不代表全页视觉还原率。

2026-10-16 的 `12.1.4.23` 已执行仓库脚本发布 Windows x64 NativeAOT，正式 EXE 的两语资源和 32 条内嵌目录自检通过。独立 NativeAOT UI 验证器完成八色三态、动态语言菜单、Dock 分屏／浮动／重新停靠和 1024 组目录渲染；新增实际 Semi 复制／粘贴菜单、Ursa 回退、直接全局切语与宿主同键覆盖检查。属性面板验证覆盖业务侧值更新、校验拒绝回退、不重复提交、旧数据解绑和视图重新附加。272 项控件测试、5 份 T4 映射及单文件原型的 128 组路由／语言／宽度审计通过，Windows EXE 安装器及 SHA-256 已生成，未执行安装。

正式 Demo 另提供 `--verify-runtime <独立目录>`：直接运行发布的 NativeAOT EXE，检查两种输出语言及全部 32 条内嵌目录，不启动窗口、不读取用户配置。Windows 发布流水线在制作安装器前执行该检查；它与加载真实控件的 UI 验证器分别验证正式产物与界面行为。

Windows 本机执行仓库发布脚本，再运行 NativeAOT 验证器并制作 EXE 安装包。发布工作流分别在对应平台生成 win-x64、linux-x64、linux-arm64、osx-x64、osx-arm64 的 EXE / DEB / DMG 和 SHA-256；不提供便携压缩包。

交付原型为 `design/` 下自包含单 HTML，内嵌全部运行资源，可复制到空目录离线交互。审计实际点击全部路由、色点、三态按钮和语言菜单；窗口框架支持最小化、最大化、关闭与恢复。

## 验收边界

浏览器原型不运行 .NET、原生窗口、网络后端或系统安装。字体度量、Dock 页签与窗口按钮受平台和第三方模板影响，页面非空和局部几何通过均不能视为达到 95% 全页还原。

本轮后台验证不会操作锁屏桌面。人工鼠标拖出、多 DPI、跨屏、输入法及安装器交互仍需在桌面可用时补充；Linux/macOS 的构建与安装结构由对应 CI 验证，本机没有运行它们的 UI。Demo 将传递 XAML Loader 固定到 Avalonia 12.1.4，避免混用 11.x 解析器。旧 Prism 8 的未使用 DialogWindow 路径仍有缺失绑定方法的 AOT 警告，Prism/DryIoc 等也有裁剪分析警告；保留原反射元数据配置，验证结果只覆盖实际调用路径。
