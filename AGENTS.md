# AGENTS.md — 仓库协作约定

> 面向 AI 助手与新成员的改动守则。设计决策背景见 `docs/决策记录.md`，
> 流式规格见 `docs/流式规则.md`，性能数据见 `docs/基准报告.md`。

## 构建与测试

```bash
dotnet build CodeWF.Avalonia.slnx -c Release
dotnet test tests/CodeWF.Avalonia.Content.Tests/CodeWF.Avalonia.Content.Tests.csproj --filter "FullyQualifiedName~Markdown"
dotnet run --project bench/CodeWF.Avalonia.Markdown.Benchmarks -c Release -- all
```

- 包版本由 `Directory.Build.props` 的 `Version` 统一管理，采用 `Avalonia版本.自研修订号`（当前 `12.1.5.x`）；依赖版本在 `build/Versions.props` 中集中维护，`Directory.Packages.props` 引用版本变量，框架默认值在 `build/ProjectDefaults.props`。发布标签必须与包版本一致。
- 发版升版本号后必须重跑 `scripts/build_standalone_prototype.ps1`：原型标题栏
  版本徽章取自版本号，漏更会导致 CI 的 `sync_design_catalog -Check` 失败。
- 提交：中文、一个逻辑单元一个提交，随做随推。

## 改动同步清单（改这里就必须改那里）

### 流式（Streaming/）

1. **改冻结规则**：先改 `docs/流式规则.md`（规格先行），再改
   `MarkdownStreamSegmenter`，跑 `StreamingEquivalenceTests` 矩阵必须全绿。
2. **改 remend 处理器**：`MarkdownTailRemender` 加处理器 →
   `docs/流式规则.md` §4.1 优先级表同步 + `Fixtures/remend/` 加 JSON fixture；
   fixture 数量基线在 `RemendFixtureTests.FixtureCount_MeetsBaseline`。
3. **改快照结构**：`MarkdownStreamingSnapshot` 加字段 →
   `MarkdownViewer.OnStreamingSnapshotReady` 与流式测试同步。
4. 性能相关改动 → `bench` 跑对照，数字进 `docs/基准报告.md`。

### 渲染（Rendering/）

1. **新增块渲染器**：`Rendering/Blocks/` 新建 → `MarkdownViewer.CreateDefaultPipeline`
   注册 → `MarkdownStyleKeys` 加样式类 → 主题字典（`Themes/`）补对应排版资源。
2. **`MarkdownBlockKind` 加成员**：分类器 `MarkdownBlockClassifier`、
   `MarkdownDiffService` 的指纹/比较分支、渲染器管道三处同步，漏一处
   会出现"解析到了但永远走降级渲染"。
3. **`IMarkdownRenderContext` 加成员**：唯一实现者是 `MarkdownViewer`；
   能力包（Mermaid/Math/Images/Highlighting）是该接口的消费者，签名变更
   要全量编译能力包确认。

### 块级状态（折叠等）

- 状态字典键 = 块源码起始偏移（`SourceSpan.Start`），存 `MarkdownViewer`
  不存控件；换文档清理点在 `RenderDocumentFull`（源码变更才清，
  同文档重渲染保留）。

### 图片（Images）

- 本地加载统一过 `MarkdownImageSourceLoader.LoadLocalFile/Async`——
  白名单等安全约束加在这两处，不要绕过。
- 远程读取统一过 `ReadRemoteBytesWithDiskCache/Async`。

## 测试约定

- headless 平台测试（控件构造/布局/点击）挂 `[Collection("AvaloniaPlatform")]`
  + `platform.Run(() => ...)`；纯逻辑（切分器/remend/diff/loader）不挂，
  直接跑。
- 点击模拟：`window.MouseMove/MouseDown/MouseUp` + `Dispatcher.UIThread.RunJobs()`。
- 涉及会真的打开浏览器的路径（链接点击），测试必须置 `args.Cancel = true`。

## 已知边界（不要试图"修复"）

- 无障碍：Avalonia 无公开 TextPattern，文本级光标浏览不可用（见 D19）。
- 行内单个 `_` 不参与 remend 补全（snake_case 误报成本高于收益，见 §4.1）。
- 解析失败保留旧快照不清空（流式规格 B2），排障先看 `Failed` 事件。

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
