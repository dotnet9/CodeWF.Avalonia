# AGENTS.md — 仓库协作约定

> 面向 AI 助手与新成员的改动守则。设计决策背景见 `docs/决策记录.md`。
> Markdown / MindView 已迁至 `apps/Vex`（Markdown/MindView 包）与 `libs/CodeWF.Toolkit`
> （无 UI 基础库），相关流式规格与基准报告随包迁移，本仓不再维护。

## 构建与测试

```bash
dotnet build CodeWF.Avalonia.slnx -c Release
dotnet test CodeWF.Avalonia.slnx -c Release
pwsh scripts/sync_localization_keys.ps1 -Check
pwsh scripts/sync_design_catalog.ps1 -Check
```

- 包版本由 `Directory.Build.props` 的 `Version` 统一管理，采用 `Avalonia版本.自研修订号`（当前 `12.1.6.x`）；依赖版本在 `build/Versions.props` 中集中维护，`Directory.Packages.props` 引用版本变量，框架默认值在 `build/ProjectDefaults.props`。发布标签必须与包版本一致。
- 发版升版本号后必须重跑 `scripts/build_standalone_prototype.ps1`：原型标题栏
  版本徽章取自版本号，漏更会导致 CI 的 `sync_design_catalog -Check` 失败。
- Demo 目录与国际化映射是生成物：改 `design/catalog.json` 后跑
  `scripts/sync_design_catalog.ps1`，改 `I18n/*.json` 后跑
  `scripts/sync_localization_keys.ps1`，两者 `-Check` 形态都在 CI 门禁里。
- 提交：中文、一个逻辑单元一个提交，随做随推。

## 包级 README 约定

- 每个可打包工程目录自带 `README.md`（定位、安装、主题接入、快速使用、文档链接），
  `Directory.Build.props` 按 `$(MSBuildProjectDirectory)` 打包；不要改回共享根 README。
- nuget.org 上并存现行 `12.1.6.x` 与已废弃历史 `12.2.x` 两条版本线，包级 README
  必须带精确钉版警告。

## 改动同步清单（改这里就必须改那里）

### 主题与样式

1. **新增控件样式**：样式入口类（`*Theme`）放对应包 `Themes/`，XAML 资源以程序集
   内绝对 URI（`avares://CodeWF.Avalonia.*/Themes/...`）引用，不要手动改写路径。
2. **Studio 八套配色**：配色令牌改动跑 `scripts/sync_design_catalog.ps1`
   重新生成原型令牌，对比度回归见 `docs/配色与排版令牌规则.md`。
3. **主题入口增删**：同步根 `README.md` 的主题入口表与快速开始组合表。

### 国际化（Lang）

1. 库 JSON 用 `EmbeddedResource` 嵌入程序集，`JsonLocalizer` 读取；
   业务覆盖优先级 = 业务输出资源 > 外层库内嵌 > 内层依赖库内嵌。
2. 强类型键一律 T4 或 `CodeWF.Avalonia.Lang.Generator` 生成，不手写映射；
   Demo 映射由 `scripts/sync_localization_keys.ps1` 维护。

### 演示台（Demo）

1. **新增/删除演示页**：改 `design/catalog.json` → `sync_design_catalog.ps1`
   再生成 `DemoCatalog.g.cs` 与原型数据；`DemoResources` 键增删后跑
   `sync_localization_keys.ps1`。
2. **离屏验证**：`scripts/localization-verification` 是 AOT 门禁的一部分，
   验证内容调整要同步 `ci.yml` 的两处调用。

## 测试约定

- headless 平台测试（控件构造/布局/点击）挂 `[Collection("AvaloniaPlatform")]`
  + `platform.Run(() => ...)`；纯逻辑测试不挂，直接跑。
- 点击模拟：`window.MouseMove/MouseDown/MouseUp` + `Dispatcher.UIThread.RunJobs()`。
- 涉及会真的打开浏览器的路径（链接点击），测试必须置 `args.Cancel = true`。

## 已知边界（不要试图"修复"）

- 无障碍：Avalonia 无公开 TextPattern，文本级光标浏览不可用（见 D19）。
- AOT/裁剪：新增包必须保持 NativeAOT 干净，`il_trim_warnings_test.ps1`
  会卡"本仓代码产生的 IL 告警"。

## 发布与版本线

- 每个功能/修复提交随附版本递增（`Directory.Build.props` 的 `<Version>`），
  历史提交都是"……；升版 X.Y.Z.W"形态；nuget 发布只在推 `v*` tag 时触发
  （publish-nuget.yml），平时推 main 不会发包。
- **nuget.org 上存在两条版本线**：现行 `12.1.6.x` 与历史 `12.2.x`（1102 时代所发，
  已废弃但仍在架上）。版本排序 `12.2.x > 12.1.6.x`——任何 `>= 12.1.x` 的浮动解析
  都会命中旧线。消费方必须**逐包精确钉版**；新包上架前用 flatcontainer
  `index.json` + 探针工程还原验证（验证队列可拖几十分钟，且 http-cache 会缓存
  旧索引响应，先 `dotnet nuget locals http-cache --clear` 再探）。
- 本地候选包打**同版本号**会被官方源+全局缓存抢（见 2026-11-11 影子包事故）：
  临时验证用 `packageSourceMapping` 把 `codewf.avalonia.*` 强制映射到候选源，
  并把该前缀下消费到的全部兄弟包补齐；最终判定看**编译输出 bin 里 DLL 的
  标记物**（新方法名 grep），nupkg 对 ≠ 提取对 ≠ 编译对。
