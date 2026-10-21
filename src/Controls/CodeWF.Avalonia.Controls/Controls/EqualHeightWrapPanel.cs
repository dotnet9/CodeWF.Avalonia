using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace CodeWF.Avalonia.Controls.Controls;

/// <summary>按固定列数排列可见子项，每行使用该行最大的期望高度。</summary>
public class EqualHeightWrapPanel : Panel
{
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<EqualHeightWrapPanel, int>(nameof(Columns), 1, validate: value => value > 0);
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<EqualHeightWrapPanel, double>(nameof(ColumnSpacing),
            validate: value => double.IsFinite(value) && value >= 0);
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<EqualHeightWrapPanel, double>(nameof(RowSpacing),
            validate: value => double.IsFinite(value) && value >= 0);
    private double[] _heights = [];

    public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public double ColumnSpacing { get => GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }
    public double RowSpacing { get => GetValue(RowSpacingProperty); set => SetValue(RowSpacingProperty, value); }

    static EqualHeightWrapPanel() => AffectsMeasure<EqualHeightWrapPanel>(ColumnsProperty, ColumnSpacingProperty, RowSpacingProperty);

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = Columns;
        var visible = Children.Where(child => child.IsVisible).ToArray();
        if (visible.Length == 0) { _heights = []; return default; }
        var bounded = double.IsFinite(availableSize.Width);
        var cardWidth = bounded ? Math.Max(0, (availableSize.Width - ColumnSpacing * (columns - 1)) / columns) : double.PositiveInfinity;
        var heights = new double[(visible.Length + columns - 1) / columns];
        var naturalWidth = 0d;
        for (var i = 0; i < visible.Length; i++)
        {
            visible[i].Measure(new Size(cardWidth, double.PositiveInfinity));
            naturalWidth = Math.Max(naturalWidth, visible[i].DesiredSize.Width);
            heights[i / columns] = Math.Max(heights[i / columns], visible[i].DesiredSize.Height);
        }
        _heights = heights;
        var width = bounded ? availableSize.Width : naturalWidth * columns + ColumnSpacing * (columns - 1);
        return new Size(width, heights.Sum() + RowSpacing * (heights.Length - 1));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = Columns;
        var cardWidth = Math.Max(0, (finalSize.Width - ColumnSpacing * (columns - 1)) / columns);
        var index = 0;
        var y = 0d;
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            var row = index / columns;
            var height = row < _heights.Length ? _heights[row] : child.DesiredSize.Height;
            child.Arrange(new Rect((index % columns) * (cardWidth + ColumnSpacing), y, cardWidth, height));
            if (++index % columns == 0) y += height + RowSpacing;
        }
        return finalSize;
    }
}
