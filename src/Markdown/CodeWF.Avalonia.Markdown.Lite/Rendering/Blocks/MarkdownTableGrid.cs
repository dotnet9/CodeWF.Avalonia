using Avalonia;
using global::Avalonia.Controls;

namespace CodeWF.Avalonia.Markdown.Rendering;

/// <summary>
/// 根据单元格的自然宽度布局表格。空间不足时优先保留短列，长列压缩后自动换行。
/// </summary>
internal sealed class MarkdownTableGrid : Grid
{
    protected override Type StyleKeyOverride => typeof(Grid);

    protected override Size MeasureOverride(Size availableSize)
    {
        var widths = new double[ColumnDefinitions.Count];
        foreach (var child in Children)
        {
            child.Measure(Size.Infinity);
            var column = GetColumn(child);
            var span = Math.Min(GetColumnSpan(child), widths.Length - column);
            if (span <= 0) continue;
            var width = child.DesiredSize.Width / span;
            for (var index = column; index < column + span; index++)
                widths[index] = Math.Max(widths[index], width);
        }

        var naturalWidth = widths.Sum();
        if (double.IsFinite(availableSize.Width) && naturalWidth > availableSize.Width && widths.Length > 0)
        {
            // 极窄视口允许缩小保底宽度，避免列数多时溢出整个文档。
            var minimum = Math.Min(56, availableSize.Width / widths.Length);
            var minimumTotal = widths.Sum(width => Math.Min(width, minimum));
            var ratio = Math.Clamp((availableSize.Width - minimumTotal) / (naturalWidth - minimumTotal), 0, 1);
            for (var index = 0; index < widths.Length; index++)
            {
                var floor = Math.Min(widths[index], minimum);
                widths[index] = floor + (widths[index] - floor) * ratio;
            }
        }

        for (var index = 0; index < widths.Length; index++)
            ColumnDefinitions[index].Width = new GridLength(widths[index]);

        return base.MeasureOverride(availableSize);
    }
}
