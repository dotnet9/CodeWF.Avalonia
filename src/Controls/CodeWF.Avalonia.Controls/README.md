# CodeWF.Avalonia.Controls

基础自定义控件库：控件 API、状态模型、绘制逻辑、标记扩展与转换器，内置 Semi 主题资源（引导、传输列表、状态徽标、标题栏等）。Core custom control library for Avalonia with built-in Semi theme resources.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Controls?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Controls)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.Controls
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

依赖 Avalonia 与 Semi.Avalonia（随包自动安装）；`CodeWF.Avalonia.Lang` 经传递引用带入，无需单独安装。目标框架 net11.0，XAML 命名空间统一 `https://codewf.com`。

## 主题接入

需先引入 Semi.Avalonia 基础主题，再加载本包样式入口：

```xml
<Application.Styles>
  <semi:SemiTheme Locale="zh-CN" />
  <codewf:ACSemiTheme />
</Application.Styles>
```

窗口类控件另有聚合主题入口 `CodeWFTheme`。主题资源按 Light / Dark / HighContrast 变体组织，跟随应用主题切换。

## 控件一览

| 控件 | 说明 |
|---|---|
| Guide / GuideOverlay / GuideStep | 新手引导：多步骤高亮遮罩、指示器与箭头，支持缺省目标行为 |
| Transfer / SearchListBox | 穿梭框与可搜索列表，左右栏自定义渲染 |
| StatusBadge / StatusCard | 状态徽标与状态卡片，内置多主题配色（`StatusLabelKind`） |
| CodeWFWindow / CodeWFTitleBar | 统一标题栏窗口：DWM 圆角、图标与按钮定制（Windows 增强，其余平台自动降级） |
| WindowedStackPanel | 窗口化虚拟化堆叠面板，长列表降低可视化树规模 |
| TrapezoidShapedTabItem / TabStrip | 梯形页签 |
| EqualHeightWrapPanel | 固定列数、同行等高布局 |
| WatermarkOverlay | 输入透明的文字水印（字体/颜色/强度/角度/间距可配） |
| ColumnDisplayEditor | 表格列显示配置编辑器 |
| EntranceMotion（附加属性） | 入场动效：淡入/位移/缩放，支持取消与离树清理 |

另含标记扩展 `{codewf:If}` / `{codewf:Switch}` 与配套转换器（`EnumToBooleanConverter` 等），可独立使用。

## 用法示例

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:codewf="https://codewf.com">
  <codewf:StatusBadge Classes="Info" LeftText="状态" RightText="运行中" />
</Window>
```

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.Controls%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
