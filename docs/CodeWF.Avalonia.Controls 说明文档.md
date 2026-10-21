# CodeWF.Avalonia.Controls

基础自定义控件库，控件 API、状态模型、绘制逻辑与 Semi 主题资源在同一包内，安装即用。

## 安装

```bash
dotnet add package CodeWF.Avalonia.Controls
```

## 主题接入

在 App.axaml 的 Styles 中引入（需先引入 Semi.Avalonia 主题）：

```xml
<codewf:ACSemiTheme />
```

窗口类控件另有一个聚合主题入口 `CodeWFTheme`。主题资源按 Light / Dark / HighContrast 变体组织，跟随应用主题切换。

## 控件一览

| 控件 | 命名空间 | 说明 |
|---|---|---|
| Guide / GuideOverlay / GuideStep | CodeWF.Avalonia.Controls.Controls | 新手引导：多步骤高亮遮罩、指示器与箭头，支持缺省目标行为（GuideMissingTargetBehavior） |
| Transfer / SearchListBox | CodeWF.Avalonia.Controls.Controls | 穿梭框与可搜索列表，支持左右栏自定义渲染 |
| StatusBadge / StatusCard | CodeWF.Avalonia.Controls.Controls | 状态徽标与状态卡片，内置多主题配色（StatusLabelKind） |
| CodeWFWindow / CodeWFTitleBar | CodeWF.Avalonia.Controls.Controls | 统一标题栏窗口：DWM 圆角、图标与按钮定制，Windows 平台增强 |
| WindowedStackPanel | CodeWF.Avalonia.Controls.Controls | 窗口化虚拟化堆叠面板，长列表场景降低可视化树规模 |
| TrapezoidShapedTabItem / TabStrip | CodeWF.Avalonia.Controls.Controls | 梯形页签样式，边框绘制由 TrapezoidShapedTabItemBorder 承担 |
| ColumnDisplayEditor | CodeWF.Avalonia.Controls.Controls | 表格列显示配置编辑器，配合 ColumnDisplayItem 模型 |
| ContentExpansionAnimator | CodeWF.Avalonia.Controls.Controls | 内容展开/收起动效 |
| EqualHeightWrapPanel | CodeWF.Avalonia.Controls.Controls | 固定列数、同行等高布局，支持隐藏项和行列间距 |
| WatermarkOverlay | CodeWF.Avalonia.Controls.Controls | 输入透明的文字水印，配置字体、颜色、强度、角度、间距和偏移 |
| EntranceMotion | CodeWF.Avalonia.Controls.Animations | 有效可见时入场，支持取消、离树清理和可配置的透明度/位移/缩放 |

XAML 中统一通过 `https://codewf.com` 命名空间使用：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:codewf="https://codewf.com">
  <codewf:StatusBadge Classes="Info" LeftText="状态" RightText="运行中" />
</Window>
```

`CodeWFWindow.TitleBarHeight` 控制标题栏及窗口按钮的高度，`CaptionButtonWidth` 独立控制最小化、最大化和关闭按钮宽度（默认 32px）。两者支持运行时调整，例如 52px 标题栏配 34px 按钮，或 44×44px 的窗口按钮；宽度必须是有限正数。Demo 的 CodeWFWindow 页面通过文档属性面板配置下一次打开的示例窗口。

## 标记扩展

`EntranceMotion` 通过附加属性使用：`IsEnabled` 管理行为生命周期，`Animate` 控制是否播放，`Duration`、`OffsetY`、`InitialScale` 和 `Easing` 配置动画。默认只有淡入，隐藏、离树、禁用播放或时长归零均取消当前动画并恢复基础值；不修改布局尺寸。业务应用自行绑定全局动效开关和主题参数。

`WatermarkOverlay.Strength` 为 0–100 的透明度百分比，默认不设置文字；`Spacing` 是水平/垂直重复间距，`Offset` 是首个水印的位置。不读写宿主正文，也不接收指针输入。

| 扩展 | 用法示例 |
|---|---|
| IfExtension | `{codewf:If Condition={Binding IsBusy}, True='加载中', False='就绪'}`，条件/分支支持绑定与静态值混合 |
| IfBinding | If 的多绑定形态，条件与分支均为 Binding 时使用 |
| SwitchExtension / SwitchCase | `{codewf:Switch}` + `SwitchCase` 集合，多分支值切换 |

## 转换器

`EnumToBooleanConverter`、`IsCollectionEmptyConverter`、If/Switch 配套转换器（IfConditionConverter、IfConverter、SwitchConverter）均在 `CodeWF.Avalonia.Controls.Converters` 命名空间，可独立于标记扩展使用。

## 注意事项

- 目标框架 net10.0；控件默认启用编译期绑定（AvaloniaUseCompiledBindingsByDefault）。
- 主题资源引用本程序集内绝对 URI（avares://CodeWF.Avalonia.Controls/Themes/...），不要手动覆盖资源字典路径。
- DWM 相关能力（WindowsDwmWindowCornerHelper）仅 Windows 生效，其余平台自动降级。
