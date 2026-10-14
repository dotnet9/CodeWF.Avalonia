# 更新日志

## 12.1.4.14 (2026-10-14)

- 校准 Demo 快捷区的视觉重量：18 像素图标与 30 像素热区分别控制，增加分类分隔留白和悬停、按下、键盘焦点反馈。
- Ribbon 分类复用 Semi 线形页签，消除模板与内容重复内边距；导航使用目录矢量图标，标题栏密度和通知改用 Semi 图形。
- 应用默认字体统一覆盖 Dock 浮动窗与弹出层，目录入口按原型使用 12 像素常规字重，原型和视觉规格同步。
- 属性面板排序按钮使用面板主题令牌，选中状态采用浅底与描边；Toolkit Demo 通过 NuGet 消费相同改进。
- 验证：Windows x64 NativeAOT 脚本发布、原生冒烟和截图通过；图标与热区运行尺寸、中英文及窄窗口检查通过，README 的 PNG/GIF 同步更新；实机复核快捷保存的鼠标反馈与键盘焦点，修正浅色导航展开箭头的对比度。

## 12.1.4.13 (2026-10-14)

- 新增 StudioPalette、StudioThemeMode 与 StudioThemeController：八套配色各自提供浅深资源，跟随系统通过平台事件更新；宿主退出可释放订阅。
- Demo 标题栏和属性面板共用浅色、深色、跟随系统状态，保留配色色圈；语言切换只更新选项标题，布局存档兼容旧明暗字段。
- Markdown 表格按内容分配自然列宽，长列在窄视口内换行，兼顾无空格网址、多短列及恢复窗口宽度后的行高。
- 原型同步八色三态、系统变化与布局保存；发布脚本限制工程级并发，降低多仓联调的 MSBuild 资源争用。
- macOS ARM64 发布配置改用经典链接器并关闭符号压缩，规避 Xcode 16 的 Fixup::addend 内部断言；恢复构建结果以云端工作流为准。
- 验证：310 项库测试、128 次原型路由/语言/宽度检查及主题检查通过；Windows x64 NativeAOT 原生冒烟与截图退出 0，32 页 × 16 配色渲染零失败，验证文档浮动、动态语言及主题模式选择。

## 12.1.4.12 (2026-10-14)

- Markdown 翻译 JSON 内嵌程序集，宿主仅维护自身输出资源；支持同键覆盖、运行时切换及重新加载，修复控件重新挂载后菜单语言未同步的问题。
- Demo 的强类型国际化键改用可重复执行的 T4，生成注释使用简体中文；新增已挂载控件的菜单、选择菜单和代码按钮语言切换测试。
- 每个 Dock Document 内使用 Grid 与 GridSplitter 组合演示和属性面板，配置原生浮动窗口，保留独立属性与分隔宽度。
- Ribbon 菜单图标统一 32 像素，首页及各演示页精简重复说明；属性面板采用 Semi 图标、下一行搜索、无边框表格编辑器及横竖分隔线，支持列宽与行高拖拽。
- 两套原型的下拉框文本统一左对齐，并添加属性搜索、排序及尺寸拖拽的自动化验收。
- 验证：304 项单元测试、T4/目录同步和 128 次浏览器路由检查通过；Windows x64 NativeAOT 原生冒烟与截图均退出 0，覆盖文档浮动、参数隔离、动态语言和 Ribbon 矢量图标尺寸。

## 12.1.4.11 (2026-10-14)

- 国际化 BCL 引擎移至 `CodeWF.Toolkit.Lang 1.0.0.13`；Avalonia 包保留 XAML 标记扩展、转换器和旧接口兼容层，不再维护第二份解析器或语言状态。
- 保留旧 `I18nManager`、`ILangPlugin`、`LocalizationLanguage` 和三个插件的使用方式，包括可修改的语言字典；Toolkit 发起的语言或资源变化可刷新现有 Avalonia 绑定。
- 日志依赖升级至 `CodeWF.Toolkit.Logging 1.0.0.13`，与库级国际化共用同一语言状态。
- Demo 语义资源与目录工厂统一生成，原型增加浏览器路由、语言及导航门禁；继续使用开源 Dock Simple 基础主题。
- 验证：293 项测试通过，使用公开 NuGet 依赖重新还原和测试通过；旧版 12.1.4.10 编译的独立消费程序替换为新兼容层后运行成功；Windows x64 NativeAOT 发布和原生冒烟通过。

## 12.1.4.5 (2026-10-13)

- 🐛[修复]-主题令牌在渲染期解析失败：Light / Dark 变体字典用 `StaticResource` 把 `SemiColor*`、`WindowDefaultBackground` 别名到自己变体里，而样式级静态查找看不到宿主应用级（Semi 主题包）提供的同名键，导致应用引导 / 文档流程等页面在测量排布时抛 `KeyNotFoundException`；现两个变体各增一份字面量令牌集（`Themes/Light/Tokens.axaml`、`Themes/Dark/Tokens.axaml`）并置于该变体 `_index` 的首位。
- 🐛[修复]-高对比色板（Aquatic / Desert / Dusk / NightSky）此前完全没有这组令牌，控件静默退化成无色；现按各自既有字面色就地补齐。
- 🐛[修复]-控件主题对 Dock 包的隐藏依赖：`ColumnDisplayEditor`、`Guide`、`SearchListBox`、`Transfer` 引用的 `CodeWFDock*` 令牌改走本库 `SemiColor*`，只装 Controls 不再缺键。
- 🐛[修复]-`SemiIconHandle` / `SemiIconSearchStroked` 在 Default 变体下解析失败或类型不符（`PathIcon.Data` 需要 `SvgPath`），改为内联几何；`SemiShadowElevated` 不可达时内联阴影字面量。
- 🧪[验证]-演示台冒烟升级为「逐页面 × 逐色板」渲染：24 页 × 7 变体（Light / Dark + 4 套高对比）= 168 次测量排布，失败 0，退出码即门禁；CI 的 AOT 冒烟不再 `continue-on-error`。

## 12.1.4.4 (2026-10-13)

- 🚀[发布]-全平台安装包：推送 v* 标签即由 GitHub Actions 产出 6 个 RID 的 NativeAOT 产物，Windows 出 Inno Setup 安装向导（x64）与便携 zip（x86 仅 zip），Linux 出 deb，macOS 出 dmg，均附 sha256；本地同款脚本在 scripts/ 下（publish_avalonia_demo.ps1、package_linux_deb.ps1、package_macos.sh、package_demo_artifacts.ps1、CodeWF.Avalonia.Demo.iss）。
- 🔨[优化]-便携 zip 不再打包调试符号，并改为逐文件写入归档，输出稳定可复现。

## 12.1.4.3 (2026-10-12)

- 🐛[修复]-高对比色板（Aquatic/Desert/Dusk/NightSky）深色底配深色字看不清：Semi 的这四个变体只提供语义令牌（Window/WindowText/GrayText/Hotlight/Highlight），数字令牌会回落到 Light；本库此前四者共用一份 Light 字典，现改为每变体一份并对齐 Semi 高对比取值（Controls 与 Dock 两处同步），同时把控件主题里跨 Style 指向 Semi 令牌的引用自足化。
- 🐛[修复]-构建流水线在全新检出下必然失败：根目录 `NuGet.config` 写死了本地包源 `artifacts/packages`（该目录被忽略、CI 上不存在），NuGet 对不存在的本地源报 NU1301 直接中断；现只注册 nuget.org，本机要用本地打包产物时在命令行追加包源。
- 🐛[修复]-`CodeWF.Avalonia.TreeDataGrid` 对同仓 `CodeWF.Avalonia.Lang` 用的是 `PackageReference`，CI 会去 nuget.org 找本仓尚未发布的版本号（NU1102）；改为 `ProjectReference`，打包后的依赖声明不变。
- 🎨[界面]-Dock 主题去掉 Fluent 依赖：DockSemiTheme 改以 Dock.Avalonia.Themes.Simple 为基座，配色令牌按 6 套变体（Default/Light/Dark + Semi 四套色板）字面量化，彻底绕开启动早期跨文件 StaticResource 解析失败的问题；不再需要 Avalonia.Themes.Fluent / Dock.Avalonia.Themes.Fluent。
- 🐛[修复]-Semi.Avalonia 12.x 不再提供 `{x:Type Button}` / `{x:Type Window}` 基类主题（旧版由 FluentTheme 提供），Dock 的页签关闭/新建按钮、工具条按钮、浮动窗口等 BasedOn 会解析失败；主题内置最小可用的基类主题。
- 🐛[修复]-演示应用改回 `DockSemiTheme`，App.axaml 移除自有 FluentTheme，Dock 区域配色跟随所选变体。
- 🐛[修复]-文档块虚拟化下大块滚出视口即被解体、滚回来又重建（图片重新解码/光栅化、Mermaid 重新布局渲染、代码重新高亮），滚动卡顿：`WindowedStackPanel` 新增保留策略——本帧刚离开窗口的重块按高度预算保留控件，轻量段落照常解体；新增离屏块滚动锚点补偿，变高块量测引起的高度变化不再让视口跳动。
- 🐛[修复]-`MarkdownViewer` 的保留预算只作用于重块（代码块、表格、图片、公式、Mermaid），并接上「已量测 + 离视口最近」的锚点固定，整篇重建时先清空旧锚点。
- 🐛[修复]-`MarkdownImage` 复用已解码位图与光栅化结果（LRU 上限 64MB），SVG/GIF 预览不再每次重建；单帧 GIF 按静态图渲染，避免空转动画时钟。
- 🐛[修复]-Mermaid 图表 SVG 按「源码 + 主题配色」缓存（上限 48 张，失败结果同样缓存），块重建时直接复用。
- 🐛[修复]-大纲/滚动定位取块边界时先物化目标块并推进布局，读位置采样跳过未量测块。
- 🧪[验证]-新增虚拟化保留策略单元测试（8 例）与 `MarkdownViewer` 真实控件回归（重块离屏保留、轻量块释放、仅物化视口附近），库全量测试通过。
- 🐛[修复]-演示应用 `MainWindow` 缺少 `using Avalonia.Controls;`，`TryFindResource` 扩展方法解析失败，导致整仓构建（含发布流水线）在演示项目上中断。

## 12.1.4.1 (2026-10-10)

- 🎨[界面]-更换全新设计的官方 logo（居中字母 A · 界面构成），SVG/PNG/ICO 三格式随包更新。

## 12.1.4.0 (2026-10-10)

- 🚀[发布]-首个正式版：9 个包面向 Avalonia 12.1.4，覆盖 Controls、DataGrid、TreeDataGrid、Dock、Markdown、Log、Lang 产品线。
- 🐛[修复]-Dock 主题在 Avalonia 12.1.4 下 BasedOn 静态资源解析失败：DockSemiTheme 内联 FluentTheme 提供同文件作用域基主题。
- 🐛[修复]-TreeDataGrid 取消排序在社区分支上抛 MissingFieldException：字段清空改为尽力而为，兼容开源血统两条版本线。
- ✨[新功能]-DataGrid 新增 ApplyPerformancePreset 性能预设，承接原 ProDataGrid 调用点。
- 🧪[验证]-新增 TreeDataGrid 排序回归测试（Headless 反射直调取消排序）；演示应用全主题 10 秒运行冒烟通过。

## 12.1.4.0-preview.1 (2026-10-10)

- 🚀[发布]-首个预览版：9 个包覆盖 Controls、DataGrid、TreeDataGrid、Dock、Markdown、Log、Lang 七条产品线，目标框架 net10.0，版本前三位对齐 Avalonia 12.1.4。
- ✨[新功能]-TreeDataGrid 增强切换至社区版 TreeDataGrid.Avalonia，Semi 主题以本地副本内置，不再依赖商业版组件。
- 🔨[优化]-全部控件的 XAML 命名空间统一挂 https://codewf.com，主题入口类名保持稳定，应用侧 App.axaml 引入行无需变化。
- 🔨[优化]-主题资源并入对应控件包，安装控件即得主题；表格增强、Dock、Markdown 的主题拆分包取消。
- 🧪[验证]-解决方案全量构建与打包演练通过；相对主题引用全量静态校验通过。
