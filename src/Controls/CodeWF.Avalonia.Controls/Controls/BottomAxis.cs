using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace CodeWF.Avalonia.Controls.Controls;

/// <summary>
/// 把悬浮在容器底部的若干件对齐到同一条水平基线（CSS 里用 bottom + translateY(50%) 表达，
/// Avalonia 没有 calc）。这里把「中心距容器底」换算成外边距：margin-bottom = 轴线 - 自身高 / 2，
/// 自身高度一变（切密度、换语言、字号变化）就重算，所以多档缩放都不会失配。
/// 轴线数值由宿主按自己的版面给出，本类只负责维持对齐这个不变量。
/// </summary>
public static class BottomAxis
{
    /// <summary>件的中心要落到的、距容器底边的距离；给 double.NaN 即解除跟踪。</summary>
    public static readonly AttachedProperty<double> CenterFromBottomProperty =
        AvaloniaProperty.RegisterAttached<Control, double>("CenterFromBottom", typeof(BottomAxis), double.NaN);

    private static readonly AttachedProperty<object?> SubscriptionProperty =
        AvaloniaProperty.RegisterAttached<Control, object?>("Subscription", typeof(BottomAxis));

    static BottomAxis()
    {
        CenterFromBottomProperty.Changed.AddClassHandler<Control>((control, args) =>
        {
            // 重复赋值先退订再订，避免换轴线（例如从内层基线切到外层基线）后残留旧监听。
            if (control.GetValue(SubscriptionProperty) is EventHandler<AvaloniaPropertyChangedEventArgs> previous)
                control.PropertyChanged -= previous;

            if (args.NewValue is not double axis || double.IsNaN(axis))
            {
                control.SetValue(SubscriptionProperty, null);
                return;
            }

            void OnChanged(object? _, AvaloniaPropertyChangedEventArgs e)
            {
                // 只有自身高度或外边距变了才需要重算底距；其余属性变化直接跳过。
                if (e.Property != Visual.BoundsProperty && e.Property != Layoutable.MarginProperty) return;
                Apply(control, axis);
            }
            control.PropertyChanged += OnChanged;
            control.SetValue(SubscriptionProperty, (EventHandler<AvaloniaPropertyChangedEventArgs>)OnChanged);
            Apply(control, axis);
        });
    }

    public static double GetCenterFromBottom(Control control) => control.GetValue(CenterFromBottomProperty);

    public static void SetCenterFromBottom(Control control, double value) => control.SetValue(CenterFromBottomProperty, value);

    private static void Apply(Control control, double axis)
    {
        var current = control.Bounds;
        if (current.Height <= 0) return;
        var margin = control.Margin;
        var bottom = axis - current.Height / 2;
        if (Math.Abs(margin.Bottom - bottom) < 0.01) return;
        control.Margin = new Thickness(margin.Left, margin.Top, margin.Right, bottom);
    }
}
