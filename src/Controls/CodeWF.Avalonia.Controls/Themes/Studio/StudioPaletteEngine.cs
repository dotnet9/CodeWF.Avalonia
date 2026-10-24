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

    /// <summary>§5.1 画布中性化：paper/surface 去色温、保留相对明度（海拔差不变），色相只留在墨、强调与交互态。</summary>
    private static Color Neutralize(Color c)
    {
        var luma = (byte)System.Math.Round(0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B);
        return new Color(255, luma, luma, luma);
    }

    /// <summary>派生指定明暗态的全部令牌基色（键集与令牌模板的 Color/画刷字面量条目对应）。</summary>
    public static Dictionary<string, Color> Derive(string key, StudioPaletteDefinition palette, bool dark)
    {
        var mode = dark ? "dark" : "light";
        var status = StatusDefaults[mode];
        // §5.1 画布中性化：先剥掉 paper/surface 的色温再派生，大面积画布不随色板染色。
        var paper = Neutralize(Parse(dark ? palette.Dark.Paper : palette.Light.Paper));
        var surface = Neutralize(Parse(dark ? palette.Dark.Surface : palette.Light.Surface));
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

        Color StatusSoft(string hex) => StatusSoftFor(paper, Parse(hex), statusSoftAlpha, dark);
        Color StatusPressedSoft(string hex) => StatusSoftFor(paper, Parse(hex), statusSoftAlpha + 0.04, dark);

        var danger = StatusColorFor(paper, surface, ink, Parse(status.Danger), dark);
        var success = StatusColorFor(paper, surface, ink, Parse(status.Success), dark);
        var warning = StatusColorFor(paper, surface, ink, Parse(status.Warning), dark);
        var info = StatusColorFor(paper, surface, ink, Parse(status.Info), dark);

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
            // muted：ink@64% 起步，对 bg/surface/raised 任一工作面不足 4.5 时按 1% 步进加深（规则 §3 约束）。
            ["StudioMutedColor"] = MutedFor(paper, surface, ink, dark),
            ["StatusCardTopTextForeground"] = ink,
            ["StatusBadgeLeftTextForeground"] = dark ? Colors.Black : Colors.White,
            // 品牌交互
            ["StudioAccentColor"] = accent,
            ["StudioSecondaryColor"] = accent,
            ["StudioPrimaryColor"] = accent,
            ["StudioGradientStartColor"] = accent,
            ["StudioTextGradientEndColor"] = GradientEndFor(accent, paper, dark),
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
            // 组件专用键（附录 A componentOverrides）：通知 muted 落在固定深色卡上，提高不透明度保证 4.5:1
            ["notificationMuted"] = Over(paper, ink, 0.85),
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
    /// <summary>两色对比度（WCAG），供宿主复核用户自定义色板（规则 §5 校验清单）。</summary>
    public static double ContrastRatio(Color a, Color b) => Contrast(a, b);

    private static double Contrast(Color a, Color b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (System.Math.Max(la, lb) + 0.05) / (System.Math.Min(la, lb) + 0.05);
    }

    /// <summary>状态色可读性（规则 §3）：对 paper 与 surface 任一不足 4.5:1 时，浅色态向 ink、深色态向白按 1% 步进收敛。</summary>
    private static Color StatusColorFor(Color paper, Color surface, Color ink, Color status, bool dark)
    {
        if (Contrast(status, paper) >= 4.5 && Contrast(status, surface) >= 4.5) return status;
        var target = dark ? Colors.White : ink;
        for (var step = 1; step <= 100; step++)
        {
            var candidate = Mix(status, target, step / 100.0);
            if (Contrast(candidate, paper) >= 4.5 && Contrast(candidate, surface) >= 4.5) return candidate;
        }
        return target;
    }

    /// <summary>状态软底（规则 §3 组件可读性约束）：浅色态向 paper 回退、深色态向状态色加深，保证状态文字 ≥4.5:1。</summary>
    private static Color StatusSoftFor(Color paper, Color status, double alpha, bool dark)
    {
        var a = alpha;
        var bg = Over(paper, status, a);
        while (Contrast(status, bg) < 4.5 && a > (dark ? 0.40 : 0.03))
        {
            a += dark ? 0.01 : -0.01;
            bg = Over(paper, status, a);
        }
        return bg;
    }

    /// <summary>primaryHover：规则值 mix(accent→paper/white,8%)，与 OnPrimary 对比不足 4.5 时向 accent 收敛混合比例。</summary>
    private static Color HoverFor(Color accent, Color paper, bool dark)
    {
        var onPrimary = Contrast(Colors.White, accent) > Contrast(Colors.Black, accent) ? Colors.White : Colors.Black;
        var target = dark ? Colors.White : paper;
        var weight = 0.08;
        var hover = Mix(accent, target, weight);
        while (Contrast(onPrimary, hover) < 4.5 && weight > 0.005)
        {
            weight -= 0.005;
            hover = Mix(accent, target, weight);
        }
        return hover;
    }

    /// <summary>muted：ink@64% 起步，对 bg/surface/raised/hover 任一工作面不足 4.5:1 时按 1% 步进加深。</summary>
    private static Color MutedFor(Color paper, Color surface, Color ink, bool dark)
    {
        var background = Mix(paper, surface, 0.30);
        var raised = Over(paper, ink, dark ? 0.06 : 0.04);
        var hover = Over(surface, ink, dark ? 0.07 : 0.05);
        var alpha = 0.64;
        var muted = Over(paper, ink, alpha);
        while ((Contrast(muted, background) < 4.5 || Contrast(muted, surface) < 4.5 || Contrast(muted, raised) < 4.5 || Contrast(muted, hover) < 4.5) && alpha < 0.90)
        {
            alpha += 0.01;
            muted = Over(paper, ink, alpha);
        }
        return muted;
    }

    /// <summary>行动/标题渐变端点：浅色 mix(accent→白,25%)、深色 mix(accent→paper,20%)；与 OnPrimary 对比不足 4.5 时向 accent 收敛。</summary>
    private static Color GradientEndFor(Color accent, Color paper, bool dark)
    {
        var onPrimary = Contrast(Colors.White, accent) > Contrast(Colors.Black, accent) ? Colors.White : Colors.Black;
        var target = dark ? paper : Colors.White;
        var weight = dark ? 0.20 : 0.25;
        var end = Mix(accent, target, weight);
        while (Contrast(onPrimary, end) < 4.5 && weight > 0.005)
        {
            weight -= 0.005;
            end = Mix(accent, target, weight);
        }
        return end;
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
            // primaryHover：浅 mix(accent→paper,8%) / 深 mix(accent→white,8%)；不足 4.5 时向 accent 收敛（规则 §3）
            ["StudioPrimaryHover"] = HoverFor(accent, paper, dark),
            // primaryPressed：浅 mix(accent→ink,10%) / 深 mix(accent→black,12%)
            ["StudioPrimaryPressed"] = Mix(accent, dark ? Colors.Black : ink, dark ? 0.12 : 0.10),
            // 行动渐变端点（规则 §3 行动渐变端点约束）：规则值混合，与 OnPrimary 对比不足 4.5 时向 accent 收敛
            ["StudioGradientEndColor"] = GradientEndFor(accent, paper, dark),
            ["PgSelectedText"] = Contrast(Colors.White, accent) > Contrast(Colors.Black, accent) ? Colors.White : Colors.Black,
        };
    }
}
