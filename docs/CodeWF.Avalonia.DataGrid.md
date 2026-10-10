# CodeWF.Avalonia.DataGrid

官方 Avalonia.Controls.DataGrid 的增强扩展包：一次性启用三态排序、Ctrl+A 全选与智能 ToolTip，内置 Semi 主题入口。

## 安装

```bash
dotnet add package CodeWF.Avalonia.DataGrid
```

## 主题接入

```xml
<codewf:DataGridSemiTheme />
```

该主题内部已包含 Semi.Avalonia.DataGrid 的官方 DataGrid 样式与本级增强样式，无需再单独引入 Semi 的 DataGrid 主题。

## 一键默认增强

```csharp
using CodeWF.Avalonia.DataGrid;

dataGrid.EnableDefaults(dataGrid.ItemsSource as IEnumerable);
```

`EnableDefaults` 等价于依次调用：

| 能力 | 方法 | 说明 |
|---|---|---|
| 三态排序 | `AddSorting()` | 表头点击在 升序 → 降序 → 取消排序 间循环；内部托管列头点击事件，无需在 VM 侧处理 |
| 自然排序 | `AddNaturalSorting()` | 字符串按数字感知方式比较（DataGridNaturalSortComparer），"item2" 排在 "item10" 之前 |
| 取消排序状态 | DataGridDefaultState | 记录用户排序状态，取消排序时恢复原始顺序 |
| 智能 ToolTip | `EnableSmartTooltips()` | 单元格文本被截断时自动显示 ToolTip，可传入列索引只处理指定列 |

附加属性方式（XAML）：

```xml
<DataGrid codewf:DataGridEnhancement.UseDefaults="True" ItemsSource="{Binding Rows}" />
```

## 注意事项

- 依赖 Avalonia.Controls.DataGrid（12.1.2 线）与 Semi.Avalonia / Semi.Avalonia.DataGrid。
- `DataGrid` 类型引用在扩展方法签名中使用全限定名，应用侧代码正常 `using Avalonia.Controls;` 即可。
