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

    /// <summary>
    /// 与原型 <c>palette-rules.js</c> 同式：先四舍五入（.5 一律向上）再钳位。
    /// 早先直接 (byte) 截断，半档差被吞成 1/255，运行时字典与静态令牌（原型生成）就差一档。
    /// </summary>
    private static byte Channel(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0d, 255d);

    /// <summary><see cref="Color"/> 的构造参数是 A-R-G-B（不是 R-G-B-A）：写成 (r, g, b, 255) 会让派生色蓝通道钉满、透明度跟着红通道走。</summary>
    private static Color Mix(Color a, Color b, double weight) => new(
        255,
        Channel(a.R * (1 - weight) + b.R * weight),
        Channel(a.G * (1 - weight) + b.G * weight),
        Channel(a.B * (1 - weight) + b.B * weight));
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
        var selectedAlpha = dark ? 0.18 : 0.12;
        var accentSoftAlpha = dark ? 0.16 : 0.10;
        var selectionAlpha = dark ? 0.30 : 0.24;
        var statusSoftAlpha = dark ? 0.18 : 0.12;
        var scrimAlpha = dark ? 0.60 : 0.40;

        var bg = Mix(paper, surface, 0.30);
        var black = Colors.Black;
        var white = Colors.White;

        var dangerFamily = StatusFamily(paper, surface, ink, Parse(status.Danger), dark, statusSoftAlpha);
        var successFamily = StatusFamily(paper, surface, ink, Parse(status.Success), dark, statusSoftAlpha);
        var warningFamily = StatusFamily(paper, surface, ink, Parse(status.Warning), dark, statusSoftAlpha);
        var infoFamily = StatusFamily(paper, surface, ink, Parse(status.Info), dark, statusSoftAlpha);
        var (danger, success, warning, info) = (dangerFamily.Color, successFamily.Color, warningFamily.Color, infoFamily.Color);

        // onAccent：白字与黑字取对比度更高者
        var onAccent = Contrast(white, accent) > Contrast(black, accent) ? white : black;

        var result = new Dictionary<string, Color>
        {
            // 中性轴
            ["StudioBackgroundColor"] = bg,
            ["StudioSurfaceColor"] = surface,
            ["StudioPaperColor"] = paper,
            // 分段控件与浮起面（规则 §5.1 之后仍按原型 .vex-segmented / --elevated 取色，底色一律用去色温后的画布）：
            // 亮色轨道 = 墨 4% 叠纸面、指示块 = 白 55% 叠纸面；深色轨道 = 墨 4.5% 叠面板、指示块回到面板色；
            // elevated = 纸面 88% + 面板 12%。这三键此前只在静态令牌里有，用户配色走运行时 Fill 会漏掉。
            ["StudioSegmentedTrackColor"] = dark ? Over(surface, ink, 0.045) : Over(paper, ink, 0.04),
            ["StudioSegmentedThumbColor"] = dark ? surface : Over(paper, Colors.White, 0.55),
            ["StudioElevatedColor"] = Mix(paper, surface, 0.12),
            // 原生令牌词汇表里「浮起面」与「悬停面」取同一个值：静态令牌由 Vex 的 sync_native_design.cjs
            // 把 StudioRaisedColor 映射到原型 --hover（不是 --raised），运行时必须同式，否则自定义配色的
            // 列表/卡片浮起面比内置配色暗一档。
            ["StudioRaisedColor"] = Over(paper, ink, hoverAlpha),
            ["StudioBorderColor"] = Over(paper, ink, borderAlpha),
            ["StudioControlBorderColor"] = Over(paper, ink, controlBorderAlpha),
            // 原型 --hover 落在纸面：resolveConstants 传给回底规则的是公式里的前景键（ink），不是派生键名，
            // 「hover 落 surface」那条特例在这条路径上从不生效。照面板算会暗一档，与静态令牌不同式。
            ["StudioHoverColor"] = Over(paper, ink, hoverAlpha),
            ["StudioPressedColor"] = Over(paper, accent, selectedAlpha),
            // 选中面用强调软底（原型 --accent-soft，静态令牌同源）：accent 作字落在软底上不足 4.5 时
            // 按 1% 减淡（规则 §3 收敛），写死 10% 会让暖色板的自定义配色选中底比内置深一档。
            ["StudioSelectedColor"] = SoftFor(paper, accent, accentSoftAlpha),
            ["StudioFocusRingColor"] = accent,
            ["StudioDisabledColor"] = Over(paper, ink, 0.40),
            // 文字
            ["StudioTextColor"] = ink,
            // muted：ink@64% 起步，对 bg/surface/raised 任一工作面不足 4.5 时按 1% 步进加深（规则 §3 约束）。
            ["StudioMutedColor"] = MutedFor(paper, surface, ink, dark),
            ["StatusCardTopTextForeground"] = ink,
            // 徽章左侧胶囊有两张脸：默认/Debug 落在强调色上，四种状态落在状态实色上。
            // 一个前景键喂不满两套底色——鼠尾草的 accent 要黑字（白字只有 3.56:1），
            // 而浅色的 success/warning/danger 要白字（黑字 4.17:1）。所以按胶囊各自的底色分派。
            ["StatusBadgeLeftTextForeground"] = onAccent,
            ["StatusBadgeStatusTextForeground"] = OnColors(success, warning, danger),
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
            // Fatal 卡与徽章的实色胶囊共用 danger：原型 --danger-pressed-fg 与 --status-error 同值
            //（状态色族解出的那一枚），深一档的只是底色 SemiColorDangerLightActive。
            ["SemiColorDangerActive"] = danger,
            ["SemiColorSuccessLight"] = successFamily.Soft,
            ["SemiColorWarningLight"] = warningFamily.Soft,
            ["SemiColorDangerLight"] = dangerFamily.Soft,
            ["SemiColorDangerLightActive"] = dangerFamily.Pressed,
            ["SemiColorInfo"] = info,
            ["SemiColorInfoLight"] = infoFamily.Soft,
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

    /// <summary>
    /// 一组底色共用的文字色：白与黑取「最差那枚对比更高者」。
    /// 状态徽章的四枚胶囊共享一个前景键，按最浅的那枚定夺才不会让某一颗先失去可读性。
    /// </summary>
    private static Color OnColors(params Color[] backgrounds)
    {
        var worst = (Color ink) =>
        {
            var min = double.MaxValue;
            foreach (var background in backgrounds) min = System.Math.Min(min, Contrast(ink, background));
            return min;
        };
        return worst(Colors.White) >= worst(Colors.Black) ? Colors.White : Colors.Black;
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

    /// <summary>
    /// 软底收敛（规则 §3）：底色越贴近纸面、叠在其上的文字越可读，故两种明暗都向 paper 退（减透明度），
    /// 下限 3%——再退就看不出这是一层底色。
    /// 曾按明暗分成 ±0.01 两个方向，且深色态的守卫写成 <c>a &gt; 0.40</c>（起步即假），深色一次都没步进；
    /// 而底色方向其实只由「退向纸面」决定，与明暗无关（原型 palette-rules.js 的 convergeSoft 同式）。
    /// </summary>
    private static Color SoftFor(Color paper, Color foreground, double alpha)
    {
        var a = alpha;
        var bg = Over(paper, foreground, a);
        while (Contrast(foreground, bg) < 4.5 && a > 0.03)
        {
            a -= 0.01;
            bg = Over(paper, foreground, a);
        }
        return bg;
    }

    /// <summary>
    /// 状态色族（规则 §3「保证文字与软底仍达标」）：状态实色决定两档软底，软底又反过来约束状态实色，
    /// 故取不动点解——每轮按当前色派生 hover/pressed 两档底，再要求色对 [hover 底、pressed 底、纸面] 同时 ≥4.5:1。
    /// 纸面偏暗的配色（鼠尾草浅底 #f5f5f5）状态字对纸面 4.64:1 达标，深一档按压底只剩 4.44:1，
    /// 而 3% 的底色下限要保住「这确实是一层底色」，能动的一侧只剩文字。
    /// 原型 palette-rules.js 的 statusFamily 同式：九套内置令牌由它生成，这里管用户配色的运行时路径。
    /// </summary>
    private static (Color Color, Color Soft, Color Pressed) StatusFamily(
        Color paper, Color surface, Color ink, Color raw, bool dark, double softAlpha)
    {
        var color = StatusColorFor(paper, surface, ink, raw, dark);
        for (var pass = 0; pass < 4; pass++)
        {
            var soft = SoftFor(paper, color, softAlpha);
            var pressed = SoftFor(paper, color, softAlpha + 0.04);
            var settled = ReadableOn(color, soft, pressed, paper);
            if (settled == color) return (color, soft, pressed);
            color = settled;
        }
        return (color, SoftFor(paper, color, softAlpha), SoftFor(paper, color, softAlpha + 0.04));
    }

    /// <summary>
    /// 文字向可读端点按 1% 插值，直到对全部底色同时 ≥4.5:1（原型 palette-rules.js 的 readable 同式）。
    /// 端点只看第一枚底色：浅底压向 #111820、深底提向白——方向选错会越插越糊。
    /// </summary>
    private static Color ReadableOn(Color color, params Color[] backgrounds)
    {
        var readable = true;
        foreach (var background in backgrounds) readable &= Contrast(color, background) >= 4.5;
        if (readable) return color;
        var endpoint = Luminance(backgrounds[0]) > 0.4 ? Color.Parse("#111820") : Colors.White;
        for (var step = 1; step <= 100; step++)
        {
            var candidate = Mix(color, endpoint, step / 100.0);
            var settled = true;
            foreach (var background in backgrounds) settled &= Contrast(candidate, background) >= 4.5;
            if (settled) return candidate;
        }
        return endpoint;
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
        // 与 Derive 同源：规则 §3 的 hover/渐变端点都朝「画布」收敛，而 §5.1 之后画布是去色温的，
        // 拿原始 paper 收敛会让用户配色的这两键与九套静态令牌不同式。
        var paper = Neutralize(Parse(dark ? palette.Dark.Paper : palette.Light.Paper));
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
