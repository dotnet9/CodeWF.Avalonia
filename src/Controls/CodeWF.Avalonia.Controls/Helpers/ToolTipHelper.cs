// 统一文本提示封装：字符串提示用 TextBlock 承载（折行 + 最大宽度），避免长文案
// 单行铺满或被屏幕边缘裁切；前景走 ToolTipForeground 主题令牌，随明暗主题反色跟随。
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;

namespace CodeWF.Avalonia.Controls.Helpers;

/// <summary>
///     普通文本 ToolTip 的统一封装。
///     用 TextBlock 承载字符串提示，统一换行、最大宽度和主题前景色；
///     附加属性可在 XAML 直接绑定视图模型（helpers:ToolTipHelper.Text="{Binding ...}"），
///     空文本不显示提示。
/// </summary>
public sealed class ToolTipHelper
{
    /// <summary>默认最大宽度：与 Semi 的 ToolTipMaxWidth(320) 对齐，长文案在此折行。</summary>
    private const double DefaultMaxWidth = 320;

    /// <summary>要显示为 ToolTip 的文本。</summary>
    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<ToolTipHelper, Control, string?>("Text");

    /// <summary>覆盖默认最大宽度（每控件可调，如需更窄的紧凑提示）。</summary>
    public static readonly AttachedProperty<double> MaxWidthProperty =
        AvaloniaProperty.RegisterAttached<ToolTipHelper, Control, double>("MaxWidth", DefaultMaxWidth);

    static ToolTipHelper()
    {
        TextProperty.Changed.AddClassHandler<Control>(OnTextChanged);
        MaxWidthProperty.Changed.AddClassHandler<Control>(OnMaxWidthChanged);
    }

    private ToolTipHelper()
    {
    }

    /// <summary>获取 ToolTip 文本。</summary>
    public static string? GetText(Control control) => control.GetValue(TextProperty);

    /// <summary>设置 ToolTip 文本；支持 XAML 绑定。</summary>
    public static void SetText(Control control, string? value) => control.SetValue(TextProperty, value);

    /// <summary>获取本控件的提示最大宽度。</summary>
    public static double GetMaxWidth(Control control) => control.GetValue(MaxWidthProperty);

    /// <summary>设置本控件的提示最大宽度。</summary>
    public static void SetMaxWidth(Control control, double value) => control.SetValue(MaxWidthProperty, value);

    private static void OnTextChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        ToolTip.SetTip(control, CreateTextTip(args.GetNewValue<string?>(), GetMaxWidth(control)));
    }

    private static void OnMaxWidthChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (GetText(control) is { } text)
        {
            ToolTip.SetTip(control, CreateTextTip(text, args.GetNewValue<double>()));
        }
    }

    /// <summary>
    ///     创建统一样式的 ToolTip 内容：折行、最大宽度、前景跟随主题令牌；空文本返回 null（不显示提示）。
    /// </summary>
    public static object? CreateTextTip(string? text, double maxWidth = DefaultMaxWidth)
    {
        return string.IsNullOrWhiteSpace(text)
            ? null
            : new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = maxWidth,
                [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("ToolTipForeground")
            };
    }
}
