using System.Collections.Generic;
using Avalonia.Media;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 配色派生引擎（规则《配色与排版令牌规则》§3）：
/// 从编写层 4 色（纸面/面板/墨色/强调）+ 状态色全局默认，派生全部令牌基色，
/// 透明度一律烘焙为不透明色。公式与原型侧 palette-rules.js 保持一致。
/// </summary>
public static class StudioPaletteEngine
{
    public sealed record StatusColors(string Danger, string Success, string Warning, string Info);

    public static IReadOnlyDictionary<string, StatusColors> StatusDefaults { get; } = new Dictionary<string, StatusColors>
    {
        ["light"] = new StatusColors("#DC2626", "#15803D", "#B45309", "#4338CA"),
        ["dark"] = new StatusColors("#F87171", "#4ADE80", "#FBBF24", "#A5B4FC"),
    };

    private static Color Parse(string hex) => Color.Parse(hex);
    private static Color Mix(Color a, Color b, double weight) => new(
        (byte)(a.R * (1 - weight) + b.R * weight),
        (byte)(a.G * (1 - weight) + b.G * weight),
        (byte)(a.B * (1 - weight) + b.B * weight),
        255);
    private static Color Over(Color baseColor, Color foreground, double alpha) => Mix(baseColor, foreground, alpha);

    /// <summary>派生指定明暗态的全部令牌基色（键集与令牌模板的 Color/画刷字面量条目对应）。</summary>
    public static Dictionary<string, Color> Derive(string key, StudioPaletteDefinition palette, bool dark)
    {
        var mode = dark ? "dark" : "light";
        var status = StatusDefaults[mode];
        var paper = Parse(dark ? palette.Dark.Paper : palette.Light.Paper);
        var surface = Parse(dark ? palette.Dark.Surface : palette.Light.Surface);
        var ink = Parse(dark ? palette.Dark.Ink : palette.Light.Ink);
        var accent = Parse(dark ? palette.Dark.Accent : palette.Light.Accent);

        // §3 派生表（明暗各一套常数；@ 透明度烘焙到指定底色）
        var borderAlpha = dark ? 0.16 : 0.12;
        var controlBorderAlpha = dark ? 0.26 : 0.20;
        var hoverAlpha = dark ? 0.07 : 0.05;
        var raisedAlpha = dark ? 0.06 : 0.04;
        var selectedAlpha = dark ? 0.18 : 0.12;
        var accentSoftAlpha = dark ? 0.16 : 0.10;
        var selectionAlpha = dark ? 0.30 : 0.24;
        var statusSoftAlpha = dark ? 0.18 : 0.12;
        var scrimAlpha = dark ? 0.60 : 0.40;

        var bg = Mix(paper, surface, 0.30);
        var black = Colors.Black;
        var white = Colors.White;

        Color StatusSoft(string hex) => Over(paper, Parse(hex), statusSoftAlpha);
        Color StatusPressedSoft(string hex) => Over(paper, Parse(hex), statusSoftAlpha + 0.04);

        var danger = Parse(status.Danger);
        var success = Parse(status.Success);
        var warning = Parse(status.Warning);
        var info = Parse(status.Info);

        // onAccent：白字与黑字取对比度更高者
        var onAccent = Contrast(white, accent) > Contrast(black, accent) ? white : black;

        var result = new Dictionary<string, Color>
        {
            // 中性轴
            ["StudioBackgroundColor"] = bg,
            ["StudioSurfaceColor"] = surface,
            ["StudioPaperColor"] = paper,
            ["StudioRaisedColor"] = Over(paper, ink, raisedAlpha),
            ["StudioBorderColor"] = Over(paper, ink, borderAlpha),
            ["StudioControlBorderColor"] = Over(paper, ink, controlBorderAlpha),
            ["StudioHoverColor"] = Over(surface, ink, hoverAlpha),
            ["StudioPressedColor"] = Over(paper, accent, selectedAlpha),
            ["StudioSelectedColor"] = Over(paper, accent, selectedAlpha),
            ["StudioFocusRingColor"] = accent,
            ["StudioDisabledColor"] = Over(paper, ink, 0.40),
            // 文字
            ["StudioTextColor"] = ink,
            ["StudioMutedColor"] = Over(paper, ink, 0.64),
            // 品牌交互
            ["StudioAccentColor"] = accent,
            ["StudioSecondaryColor"] = accent,
            ["StudioPrimaryColor"] = accent,
            ["StudioGradientStartColor"] = accent,
            ["StudioTextGradientEndColor"] = Mix(accent, dark ? paper : white, dark ? 0.20 : 0.25),
            // 状态
            ["SemiColorSuccess"] = success,
            ["SemiColorWarning"] = warning,
            ["SemiColorDanger"] = danger,
            ["SemiColorDangerActive"] = StatusPressedSoft(status.Danger),
            ["SemiColorSuccessLight"] = StatusSoft(status.Success),
            ["SemiColorWarningLight"] = StatusSoft(status.Warning),
            ["SemiColorDangerLight"] = StatusSoft(status.Danger),
            ["SemiColorDangerLightActive"] = StatusPressedSoft(status.Danger),
            ["SemiColorInfo"] = info,
            ["SemiColorInfoLight"] = StatusSoft(status.Info),
        };
        return result;

    }

    private static double Luminance(Color c)
    {
        static double Channel(byte v)
        {
            var d = v / 255.0;
            return d <= 0.04045 ? d / 12.92 : System.Math.Pow((d + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }
    private static double Contrast(Color a, Color b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (System.Math.Max(la, lb) + 0.05) / (System.Math.Min(la, lb) + 0.05);
    }

    /// <summary>附加派生键（部分键在令牌模板中以画刷字面量出现，引擎按同表派生）。</summary>
    public static Dictionary<string, Color> DeriveExtras(StudioPaletteDefinition palette, bool dark)
    {
        var ink = Parse(dark ? palette.Dark.Ink : palette.Light.Ink);
        var paper = Parse(dark ? palette.Dark.Paper : palette.Light.Paper);
        var accent = Parse(dark ? palette.Dark.Accent : palette.Light.Accent);
        return new Dictionary<string, Color>
        {
            // onAccent：白字与黑字取对比度更高者
            ["StudioOnPrimary"] = Contrast(Colors.White, accent) > Contrast(Colors.Black, accent) ? Colors.White : Colors.Black,
            // primaryHover：浅 mix(accent→paper,8%) / 深 mix(accent→white,8%)
            ["StudioPrimaryHover"] = Mix(accent, dark ? Colors.White : paper, 0.08),
            // primaryPressed：浅 mix(accent→ink,10%) / 深 mix(accent→black,12%)
            ["StudioPrimaryPressed"] = Mix(accent, dark ? Colors.Black : ink, dark ? 0.12 : 0.10),
        };
    }
}
