# CodeWF.Avalonia.DataGrid

官方 Avalonia DataGrid 的增强扩展：三态排序（升/降/取消）、自然排序比较器与智能 ToolTip，内置 Semi 主题入口。Enhancements for the Avalonia DataGrid control with a built-in Semi theme entry.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.DataGrid?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.DataGrid)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.DataGrid
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

依赖 Avalonia.Controls.DataGrid 与 Semi.Avalonia，随包自动安装。

## 主题接入

```xml
<codewf:DataGridSemiTheme />
```

主题内部已包含 Semi.Avalonia.DataGrid 的官方 DataGrid 样式与本包增强样式，无需再单独引入 Semi 的 DataGrid 主题。

## 快速使用

一键启用默认增强：

```csharp
using CodeWF.Avalonia.DataGrid;

dataGrid.EnableDefaults(dataGrid.ItemsSource as IEnumerable);
```

或用 XAML 附加属性：

```xml
<DataGrid codewf:DataGridEnhancement.UseDefaults="True" ItemsSource="{Binding Rows}" />
```

`EnableDefaults` 依次启用：

| 能力 | 方法 | 说明 |
|---|---|---|
| 三态排序 | `AddSorting()` | 表头点击在 升序 → 降序 → 取消 间循环，VM 侧无需处理 |
| 自然排序 | `AddNaturalSorting()` | "item2" 排在 "item10" 之前 |
| 取消排序状态 | `DataGridDefaultState` | 取消排序时恢复原始顺序 |
| 智能 ToolTip | `EnableSmartTooltips()` | 单元格文本被截断时自动显示 ToolTip，可指定列 |

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.DataGrid%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
