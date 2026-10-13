# 更新日志

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
