using Avalonia;
using global::Avalonia.Controls;

namespace CodeWF.Avalonia.Markdown.Rendering;

/// <summary>阅读与即时编辑表格共用的布局选项，宿主可以通过样式启用铺满正文。</summary>
public sealed class MarkdownTableLayout : AvaloniaObject
{
    public static readonly AttachedProperty<bool> FillAvailableWidthProperty =
        AvaloniaProperty.RegisterAttached<MarkdownTableLayout, Grid, bool>("FillAvailableWidth");

    static MarkdownTableLayout() => FillAvailableWidthProperty.Changed.AddClassHandler<Grid>((grid, _) => grid.InvalidateMeasure());
    public static bool GetFillAvailableWidth(Grid grid) => grid.GetValue(FillAvailableWidthProperty);
    public static void SetFillAvailableWidth(Grid grid, bool value) => grid.SetValue(FillAvailableWidthProperty, value);
}
