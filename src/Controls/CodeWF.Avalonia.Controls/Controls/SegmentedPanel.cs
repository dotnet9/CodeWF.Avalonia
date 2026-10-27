using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace CodeWF.Avalonia.Controls.Controls;

/// <summary>
/// 分段选择器的一条轨道：横排各段、留选项间距，并把唯一那颗滑动指示块对齐到选中段。
/// 底色只由指示块表达——按钮本身永远透明，选中只换字色与字重；轨道与指示块的画法在
/// Themes/Controls/SegmentedPanel.axaml，宿主只需把按钮放进来并给其中一只 IsActive。
/// </summary>
public class SegmentedPanel : Panel
{
    /// <summary>指示块的样式类，主题按它上色；宿主想换形状可直接命中同一个 Border。轨道留给宿主
    /// （Border.segmented-track），面板只管排段和对齐指示块。</summary>
    public const string ThumbClasses = "segmented-thumb";

    /// <summary>宿主给每个分段按钮标的当前选中项；同一面板最多一个为真，全为假时指示块淡出。</summary>
    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<SegmentedPanel, Control, bool>("IsActive");

    /// <summary>各段之间的间距；轨道内缩由主题的 Padding 负责，这里只管段与段。</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SegmentedPanel, double>(nameof(Spacing), 2);

    /// <summary>各段等宽并长满轨道（对应 CSS 的 button{flex:1}）；默认按内容自然宽排。</summary>
    public static readonly StyledProperty<bool> FillItemsProperty =
        AvaloniaProperty.Register<SegmentedPanel, bool>(nameof(FillItems));

    /// <summary>指示块与选中段的圆角；胶囊档给一个大值，方形档给 0。</summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SegmentedPanel, CornerRadius>(nameof(CornerRadius), new CornerRadius(999));

    private readonly Border _thumb;

    /// <summary>本面板按内容算出的自然宽度；可用宽度不足时按它等比让位，指示块才能一直贴着所选段。</summary>
    private double _contentWidth;

    /// <summary>等宽档（FillItems）下每段的统一宽度；安排时按它铺开，否则 DesiredSize 只有内容的自然宽。</summary>
    private double _uniformItemWidth;

    static SegmentedPanel()
    {
        SpacingProperty.Changed.AddClassHandler<SegmentedPanel>((panel, _) => panel.InvalidateMeasure());
        FillItemsProperty.Changed.AddClassHandler<SegmentedPanel>((panel, _) => panel.InvalidateMeasure());
        CornerRadiusProperty.Changed.AddClassHandler<SegmentedPanel>((panel, _) => panel.ApplyCornerRadius());
        IsActiveProperty.Changed.AddClassHandler<Control>((control, args) =>
        {
            // 选中态只有 IsActive 一个入口：主题按 .active 类换字色字重，这里代宿主同步那个类，
            // 免得宿主两头维护同一个状态；切换段不改变按钮尺寸，所以直接按现成 Bounds 移动指示块。
            if (args.NewValue is not bool active) return;
            control.Classes.Set("active", active);
            if (control.Parent is SegmentedPanel panel) panel.MoveThumb();
        });
    }

    public SegmentedPanel()
    {
        _thumb = new Border
        {
            Classes = { ThumbClasses },
            IsHitTestVisible = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Stretch,
            Opacity = 0,
        };
        // 指示块排第一，画在按钮之下，正好对应轨道的第一个子元素。
        Children.Add(_thumb);
        ApplyCornerRadius();
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public bool FillItems
    {
        get => GetValue(FillItemsProperty);
        set => SetValue(FillItemsProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static bool GetIsActive(Control control) => control.GetValue(IsActiveProperty);

    public static void SetIsActive(Control control, bool value) => control.SetValue(IsActiveProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        var spacing = Math.Max(0, Spacing);
        // 轨道高度取最高的一段，每段都撑满这一行高（CSS flex 行的 align-items:stretch）。
        // 无约束高度在 Avalonia 里是 +∞（NaN 会被 Measure 直接拒掉）。
        var height = double.IsFinite(availableSize.Height) ? availableSize.Height : double.PositiveInfinity;
        var items = new List<Layoutable>();
        var width = 0d;
        var tallest = 0d;
        var widest = 0d;

        foreach (var child in Children)
        {
            if (child == _thumb || child is not Layoutable layoutable || !child.IsVisible) continue;
            layoutable.Measure(new Size(double.PositiveInfinity, height));
            items.Add(layoutable);
            width += layoutable.DesiredSize.Width + spacing;
            tallest = Math.Max(tallest, layoutable.DesiredSize.Height);
            widest = Math.Max(widest, layoutable.DesiredSize.Width);
        }

        if (items.Count > 0) width -= spacing;
        width = Math.Max(0, width);
        _contentWidth = width;

        // 等宽档：整行按同一段宽重测一次，否则 DesiredSize 只有内容的自然宽。
        if (FillItems && items.Count > 0)
        {
            var room = double.IsPositiveInfinity(availableSize.Width) ? width : Math.Max(width, availableSize.Width);
            var uniform = Math.Max(widest, (room - spacing * (items.Count - 1)) / items.Count);
            foreach (var item in items) item.Measure(new Size(uniform, height));
            _uniformItemWidth = uniform;
            width = uniform * items.Count + spacing * (items.Count - 1);
            _contentWidth = width;
            tallest = items.Max(item => item.DesiredSize.Height);
        }
        else
        {
            _uniformItemWidth = 0;
        }

        // 指示块按整行尺寸测量：它自己的 Margin/Width 不参与面板大小，免得选中末段时轨道被撑宽。
        _thumb.Measure(new Size(width, tallest));
        return new Size(width, tallest);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var spacing = Math.Max(0, Spacing);
        var visible = Children.Count(child => child != _thumb && child is Layoutable && child.IsVisible);
        // 轨道宽度由内容决定；被压缩时按等比让位，保证指示块始终贴着所选段。
        var scale = visible > 0 && finalSize.Width < _contentWidth && _contentWidth > 0
            ? Math.Max(0, (finalSize.Width - spacing * (visible - 1)) / _contentWidth)
            : 1d;
        // 等宽档还要吃掉父级多给的空间（flex 的 grow 是按剩余空间平分的）。
        var grow = FillItems && visible > 0 ? Math.Max(0, finalSize.Width - _contentWidth) / visible : 0d;
        var x = 0d;
        var rowHeight = finalSize.Height;

        foreach (var child in Children)
        {
            if (child == _thumb || child is not Layoutable layoutable) continue;
            if (!child.IsVisible)
            {
                layoutable.Arrange(default);
                continue;
            }
            var desired = FillItems && _uniformItemWidth > 0 ? _uniformItemWidth : layoutable.DesiredSize.Width;
            var width = desired * scale + grow;
            child.Arrange(new Rect(x, 0, width, rowHeight));
            x += width + spacing;
        }

        _thumb.Arrange(new Rect(0, 0, finalSize.Width, rowHeight));
        MoveThumb();
        return finalSize;
    }

    private void ApplyCornerRadius() => _thumb.CornerRadius = CornerRadius;

    private void MoveThumb()
    {
        if (!IsArrangeValid) return;
        foreach (var child in Children)
        {
            if (child == _thumb || child is not Control control || !child.IsVisible) continue;
            if (!control.GetValue(IsActiveProperty)) continue;
            var bounds = control.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) continue;
            _thumb.Width = bounds.Width;
            _thumb.Margin = new Thickness(bounds.X, 0, 0, 0);
            _thumb.Opacity = 1;
            return;
        }
        _thumb.Opacity = 0;
    }
}
