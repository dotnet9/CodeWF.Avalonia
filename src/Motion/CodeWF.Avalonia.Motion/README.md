# CodeWF.Avalonia.Motion

Avalonia 动效引擎：弹簧物理（闭式解、可中断保速）、声明式弹簧过渡、动效分级（Full/Reduced/Off）与常驻循环。移植自 [TKCode.Arc](https://github.com/AIDotNet/TKCode.Arc)（MIT）。Spring-physics motion engine for Avalonia: interruptible velocity-preserving values, declarative spring transitions, motion levels and live loops.

[![NuGet](https://img.shields.io/nuget/v/CodeWF.Avalonia.Motion?label=nuget)](https://www.nuget.org/packages/CodeWF.Avalonia.Motion)
[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dotnet9/CodeWF.Avalonia/blob/main/LICENSE)

## 安装

```bash
dotnet add package CodeWF.Avalonia.Motion
```

> nuget.org 上并存现行 `12.1.6.x` 与已废弃的历史 `12.2.x` 两条版本线（后者版本号更大），浮动解析会命中旧线；请逐包精确钉版到 `12.1.6.*`。

仅依赖 Avalonia，无主题接入要求；全程裁剪/NativeAOT 安全（AOT 分析器告警已升为编译错误门禁）。

## 动效分级

`ArcMotion.Level` 是继承附加属性，在窗口或任意容器上设置即可作用于子树；控件/主题声明 `ArcMotion.Tracks="True"` 后按当前级别获得 `:motion-full` / `:motion-reduced` 伪类（首帧不动画），级别为 Off 时两者都不出现、一切静止：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:motion="using:CodeWF.Avalonia.Motion"
        motion:ArcMotion.Level="Reduced">
```

宿主也可把 `MotionMode` 资源写为 `"full"` / `"reduced"` / `"off"` 参与分级。

## 弹簧值与时间线

```csharp
using CodeWF.Avalonia.Motion;

// 组件化弹簧值：double/Point/Vector/Size/Rect/Thickness/CornerRadius/Color 均有内建向量
var position = new MotionValue<double>(control, 0, MotionVectors.Double);
await position.AnimateTo(100, MotionSpec.Spring(new SpringSpec(stiffness: 170, damping: 20)));
// 弹簧可中断并保留速度；Reduced/Off 级别下直接落位

// 可暂停时间线：0 → 1 线性运行，适合倒计时/进度条等函数型动效
var timeline = new MotionTimeline(control);
timeline.Changed += value => progressBar.Value = value;
timeline.Start(TimeSpan.FromSeconds(3));
```

常驻循环用 `LiveLoop`（骨架呼吸、微光扫过、live 点）：只在所有者已挂载、实际可见、进入视口且动效级别达标时消耗帧；暂停不丢相位，运行时零分配。

## 文档

- 仓库与全部包一览：<https://github.com/dotnet9/CodeWF.Avalonia>
