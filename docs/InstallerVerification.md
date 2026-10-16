# 统一安装器实施与验证

2026-10-16，本轮修订将 CodeWF 品牌、安装动画和单文件原型同步到 11 个桌面 App 及两个库 Demo。所有仓库直接在 main 维护各自安装源码，打包不读取相邻仓库。

## 当前版本与发布入口

| 产品 | 本轮版本 | 五平台下载 |
| --- | --- | --- |
| AvaGithubDesktop | 0.118.0.20 | [Release](https://github.com/dotnet9/AvaGithubDesktop/releases/tag/v0.118.0.20) |
| ClearC | 0.2.14 | [Release](https://github.com/dotnet9/ClearC/releases/tag/v0.2.14) |
| CodeWF.Avalonia.Demo | 12.1.4.27 | [Release](https://github.com/dotnet9/CodeWF.Avalonia/releases/tag/v12.1.4.27) |
| CodeWF.Toolbox | 12.0.19 | [Release](https://github.com/dotnet9/CodeWF.Toolbox/releases/tag/v12.0.19) |
| CodeWF.Toolkit.Demo | 1.0.0.26 | [Release](https://github.com/dotnet9/CodeWF.Toolkit/releases/tag/v1.0.0.26) |
| HomeLedger | 0.1.14 | [Release](https://github.com/dotnet9/HomeLedger/releases/tag/v0.1.14) |
| IEditor | 0.3.14 | [Release](https://github.com/dotnet9/IEditor/releases/tag/v0.3.14) |
| KidWall | 1.0.13 | [Release](https://github.com/dotnet9/KidWall/releases/tag/v1.0.13) |
| NexusDash | 0.1.16 | [Release](https://github.com/dotnet9/NexusDash/releases/tag/v0.1.16) |
| QuickApp | 0.9.8 | [Release](https://github.com/dotnet9/QuickApp/releases/tag/v0.9.8) |
| Vex | 1.5.9 | [Release](https://github.com/dotnet9/Vex/releases/tag/v1.5.9) |
| Zhijian | 12.1.0.18 | [Release](https://github.com/dotnet9/Zhijian/releases/tag/v12.1.0.18) |
| Zitie | 0.2.7 | [Release](https://github.com/dotnet9/Zitie/releases/tag/v0.2.7) |

本轮标签发布正在进行。上表为本轮下载入口，不表示全部工作流已经成功；以各仓正式发布结果和资产清单为准。上一轮 65 个安装包的成功记录属于各自历史版本，不能替代本轮验收。

## 本轮变更

- Windows 保留 .NET 紫 `#512BD4`、异形窗口和原生安装流程；核心字标改为 CodeWF。品牌使用可聚焦按钮，仅用户点击时打开 `https://codewf.com`，安装不会自动打开网页。
- 品牌和进度在可复用离屏位图完成整帧，再一次提交；正常重绘与打印捕获均使用完整帧，禁止品牌背景擦除，百分比未变化时不重复更新文本。
- 单文件安装原型保留动画节点，避免切换状态时重建核心或重启轨道；减少动态效果选项继续有效。各仓原型均在自己的 `design/` 维护，可独立演进。
- macOS DMG 的三套背景同步 CodeWF 字标及官网文字。Linux 继续使用本机包管理器，不宣称复现 Windows 异形向导。

Windows 生产来源清单 SHA-256：`5b541cb4099ec74a4ea764e7c425bafc1bb69f6bfb672d758ed34233d3b2bcab`。本轮标签最初使用的 macOS 来源清单为 `6c919cdfd26967f990e0a232c9353cfc3e95ca1923402157b570d6a481c3adc7`；后续打包修复同步各仓 main，新清单为 `f2b2561eba734472ae4c4a3c8cf54b49c9d3df66451ead318de9353c8495a92c`，历史标签不移动。

macOS 打包修复针对真实 CI 失败：卷已卸载但镜像设备仍连接时，不能继续按失效的挂载路径分离。现在从 `hdiutil attach -plist` 识别整盘设备，按设备有限重试；失败后只有系统状态明确证明本镜像及设备已分离才继续压缩。状态不明时保留临时目录。该修复不改应用载荷；已成功的安装包保留，失败产品以固定提交恢复发布，并记录标签与实际构建提交的差异。

## 验证范围

- 同源 Windows 安装夹具中英文各 38 项通过，包含十二帧品牌稳定性、真实进度、三态主题、键盘、许可门禁、目录与完成退出；夹具不创建产品注册、快捷方式、文件关联或产品进程。
- 安装原型每份 202 项检查及 13 份最终版本的 156 个变体通过；品牌点击和二十帧动效采样通过。macOS 背景已实际重新生成并检查，13 份来源快照一致。
- macOS 卸载修复通过 25 个受控场景，覆盖部分卸载、设备已分离、查询及解析失败、永久占用与退出清理；这是命令替身回归，系统工具及 Foundation 桥接的执行结果以实际 macOS 发布为准。
- HomeLedger 0.1.14、IEditor 0.3.14 已完成本轮正式 Windows NativeAOT 与隔离运行自检。Vex 1.5.9 继续重点进行实际窗口布局、拖拽、菜单、即时编辑、四语言、代码背景及最终 NativeAOT 检查，结果单独记录在 Vex 仓。
- 按本轮维护决定，其余未完成的本机重复验证停止；后续使用标签触发各产品正式五平台 AOT 与打包。不将云端编译成功表述为实际系统安装通过。

本机编译器为 Inno Setup 6.3.3 x86；安装器引擎与应用 Windows x64 NativeAOT 载荷架构分别维护。生产安装器在本机只编译，不执行真实安装、升级或卸载。

## 实际原生界面

以下截图来自同源安全夹具，标题为测试产品，不能作为实际产品安装记录。

![原生异形欢迎页](images/installer-native-welcome.png)

![原生安装进度](images/installer-native-progress.png)

## 交付和平台边界

正式交付 Windows x64 EXE、Linux x64/arm64 DEB、macOS x64/arm64 DMG，每包附 SHA-256，不生成便携 ZIP。发布完成后逐仓检查五个安装包和五份校验文件；本机及云端的二进制摘要分别记录，不混用。

尚未执行真实产品安装、升级、卸载，或真实 Linux/macOS 安装、多 DPI、跨屏、输入法、读屏、签名和公证验收。原型中的系统授权、可选 PKG 和个人数据删除开关不作为已实现功能声明。默认文件关联、用户偏好和业务数据未改动。

运行方式见 [共享安装器说明](../scripts/installer/README.md)、[维护逻辑回归](../scripts/installer/tests/README.Maintenance.md) 和 [卸载隔离验证](../scripts/installer/tests/Uninstall.Fixture.md)。
