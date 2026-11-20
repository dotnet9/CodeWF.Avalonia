# CodeWF.Avalonia.TreeDataGrid

社区版 [TreeDataGrid.Avalonia](https://www.nuget.org/packages/TreeDataGrid.Avalonia)（MIT，Avalonia 官方 TreeDataGrid 11.1.1 开源版本的社区维护分支）的增强扩展：三态排序（升/降/取消）、Ctrl+A 全选与智能 ToolTip，内置 Semi 风格主题的本地 vendor 副本（不依赖商业版）；另含通用属性面板 PropertyGrid。Enhancements for the community TreeDataGrid.Avalonia control with a vendored Semi theme, plus a generic PropertyGrid control.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.TreeDataGrid?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.TreeDataGrid)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.TreeDataGrid
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

依赖 TreeDataGrid.Avalonia、Semi.Avalonia、Avalonia.Controls.ColorPicker（随包自动安装）；`CodeWF.Avalonia.Lang` 经传递引用带入。

## 主题接入

需先引入 Semi.Avalonia 基础主题：

```xml
<semi:SemiTheme Locale="zh-CN" />
<codewf:TreeDataGridSemiTheme />
```

控件命名空间保持上游的 `Avalonia.Controls`，XAML 中直接使用：

```xml
<avalonia:TreeDataGrid Source="{Binding Source}" />
```

## 默认增强

```csharp
using CodeWF.Avalonia.TreeDataGrid;

treeDataGrid.EnableDefaults(flatSource);
```

| 能力 | 方法 | 说明 |
|---|---|---|
| 三态排序 | `AddSorting<T>(source)` | 表头点击 升序 → 降序 → 取消；取消时还原原始行序 |
| 全选 | `AddSelectAll<T>(source)` | Ctrl+A 全选，Ctrl+Shift+A 取消全选 |
| 智能 ToolTip | `EnableSmartTooltips()` | 截断单元格自动 ToolTip，可指定列 |

## PropertyGrid

通用属性面板控件（对标 Visual Studio 属性窗口）：分类/字母双视图、无限层级、搜索、描述面板与多类型编辑器，XAML 经 `https://codewf.com` 命名空间使用。

## 文档

- 完整说明文档：<https://github.com/dotnet9/CodeWF.Avalonia/blob/main/docs/CodeWF.Avalonia.TreeDataGrid%20说明文档.md>
- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
