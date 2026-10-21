using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace CodeWF.Avalonia.Controls.Controls;

/// <summary>不接收指针输入的重复文字水印，独立于正文和选区。</summary>
public class WatermarkOverlay : Control
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<WatermarkOverlay, string?>(nameof(Text));
    public static readonly StyledProperty<double> StrengthProperty = AvaloniaProperty.Register<WatermarkOverlay, double>(nameof(Strength), 12,
        validate: value => double.IsFinite(value) && value >= 0 && value <= 100);
    public static readonly StyledProperty<IBrush?> InkProperty = AvaloniaProperty.Register<WatermarkOverlay, IBrush?>(nameof(Ink), Brushes.Gray);
    public static readonly StyledProperty<FontFamily> FontFamilyProperty = AvaloniaProperty.Register<WatermarkOverlay, FontFamily>(nameof(FontFamily), FontFamily.Default);
    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty.Register<WatermarkOverlay, double>(nameof(FontSize), 19,
        validate: value => double.IsFinite(value) && value > 0);
    public static readonly StyledProperty<double> AngleProperty = AvaloniaProperty.Register<WatermarkOverlay, double>(nameof(Angle), -25, validate: double.IsFinite);
    public static readonly StyledProperty<Size> SpacingProperty = AvaloniaProperty.Register<WatermarkOverlay, Size>(nameof(Spacing), new Size(280, 180),
        validate: value => double.IsFinite(value.Width) && double.IsFinite(value.Height) && value.Width >= 1 && value.Height >= 1);
    public static readonly StyledProperty<Point> OffsetProperty = AvaloniaProperty.Register<WatermarkOverlay, Point>(nameof(Offset), new Point(40, 100),
        validate: value => double.IsFinite(value.X) && double.IsFinite(value.Y));

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>水印文字透明度，0 到 100。</summary>
    public double Strength { get => GetValue(StrengthProperty); set => SetValue(StrengthProperty, value); }
    public IBrush? Ink { get => GetValue(InkProperty); set => SetValue(InkProperty, value); }
    public FontFamily FontFamily { get => GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public double FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public double Angle { get => GetValue(AngleProperty); set => SetValue(AngleProperty, value); }
    public Size Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public Point Offset { get => GetValue(OffsetProperty); set => SetValue(OffsetProperty, value); }

    static WatermarkOverlay() => AffectsRender<WatermarkOverlay>(TextProperty, StrengthProperty, InkProperty,
        FontFamilyProperty, FontSizeProperty, AngleProperty, SpacingProperty, OffsetProperty);
    public WatermarkOverlay() { IsHitTestVisible = false; ClipToBounds = true; }

    public override void Render(DrawingContext context)
    {
        if (string.IsNullOrWhiteSpace(Text) || Strength <= 0 || Ink is null) return;
        var text = new FormattedText(Text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(FontFamily), FontSize, Ink);
        using var clip = context.PushClip(new Rect(Bounds.Size));
        using var opacity = context.PushOpacity(Strength / 100);
        // 对偏移取模，避免极端负偏移产生大量离屏绘制。
        var startX = Offset.X % Spacing.Width;
        var startY = Offset.Y % Spacing.Height;
        for (var y = startY; y < Bounds.Height; y += Spacing.Height)
        for (var x = startX; x < Bounds.Width; x += Spacing.Width)
        {
            using var transform = context.PushTransform(Matrix.CreateRotation(Angle * Math.PI / 180) * Matrix.CreateTranslation(x, y));
            context.DrawText(text, default);
        }
    }
}
