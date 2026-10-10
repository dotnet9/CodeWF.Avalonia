# CodeWF.Avalonia.TreeDataGrid

社区版 [TreeDataGrid.Avalonia](https://www.nuget.org/packages/TreeDataGrid.Avalonia)（MIT，Avalonia 官方 TreeDataGrid 11.1.1 开源版本的社区维护分支）的增强扩展包：三态排序、Ctrl+A 全选与智能 ToolTip，并内置 Semi 风格主题的本地副本。

> 不依赖任何商业许可组件；主题样式 vendor 自 Semi.Avalonia（MIT），随本包分发。

## 安装

```bash
dotnet add package CodeWF.Avalonia.TreeDataGrid
```

## 主题接入

```xml
<codewf:TreeDataGridSemiTheme />
```

控件本身的命名空间保持上游的 `Avalonia.Controls`（社区分支为替换安装式升级），XAML 中直接使用内置 TreeDataGrid 控件：

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
| 三态排序 | `AddSorting<T>(source)` | 表头点击 升序 → 降序 → 取消；取消排序会还原底层行的原始顺序 |
| 全选 | `AddSelectAll<T>(source)` | Ctrl+A 全选当前 FlatTreeDataGridSource 行，Ctrl+Shift+A 取消全选 |
| 智能 ToolTip | `EnableSmartTooltips()` | 截断单元格自动 ToolTip，支持指定列索引 |

## 注意事项

- 依赖 TreeDataGrid.Avalonia 12.0.x（net8/net10 资产，net11 项目可正常消费）。
- 取消排序依赖上游内部字段的反射回写并带有预定义保留声明（DynamicDependency），升级社区分支版本后请回归验证排序取消行为。
