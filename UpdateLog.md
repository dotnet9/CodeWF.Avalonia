# 更新日志

## 未发布

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
