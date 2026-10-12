// Mermaid 渲染手法移植自 MarkView.Avalonia.Mermaid（MIT, Copyright (c) Nicolas Musset）：
// Mermaider 输出的 SVG 使用 CSS 自定义属性（var(--_xxx)），而 SkiaSharp 不实现 CSS
// 级联会静默忽略 var()，因此渲染后需按 Mermaider 的 color-mix 公式把变量内联为具体色值。

using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Svg.Skia;
using global::Avalonia.VisualTree;

using CodeWF.Avalonia.Markdown.Controls;
using CodeWF.Avalonia.Markdown.Rendering;

using Markdig.Syntax;

using Mermaider;

using MermaidRenderOptions = Mermaider.Models.RenderOptions;

namespace CodeWF.Avalonia.Markdown.Mermaid;

/// <summary>
/// ```mermaid 围栏代码块渲染器：Mermaider（纯 .NET，无 JavaScript）解析渲染为 SVG，
/// 后台线程出图并内联 CSS 变量，跟随明暗主题重渲，失败回落显示源码。
/// </summary>
public sealed class MermaidBlockRenderer : IMarkdownBlockRenderer
{
    private static readonly Regex ThemeChangeProperty = new("^ActualThemeVariant$", RegexOptions.Compiled);

    /// <summary>缓存图表上限：按「源码 + 主题配色」缓存，块被虚拟化回收后重建时不再重新解析渲染。</summary>
    private const int MaxCachedDiagrams = 48;

    private static readonly Dictionary<string, SvgSource?> DiagramCache = new(StringComparer.Ordinal);
    private static readonly LinkedList<string> DiagramCacheOrder = new();
    private static readonly object DiagramCacheGate = new();

    public Control? TryRender(Block block, string? sourceMarkdown, IMarkdownRenderContext context)
    {
        if (block is not FencedCodeBlock fenced
            || !string.Equals(fenced.Info?.Trim(), "mermaid", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return CreateMermaidBlock(ExtractSource(fenced));
    }

    private static Control CreateMermaidBlock(string source)
    {
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var border = new Border { Child = image };
        border.Classes.Add("MdMermaid");

        CancellationTokenSource? cts = null;

        // 主题切换时 SVG 里的颜色是烘焙进文本的，必须重渲。
        void OnThemeChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (!ThemeChangeProperty.IsMatch(e.Property.Name))
            {
                return;
            }

            _ = ApplyThemeAsync();
        }

        Application.Current?.PropertyChanged += OnThemeChanged;

        // ScrollViewer 会给子项传递无穷可用宽度：钳制 MaxWidth 到视口宽度并随尺寸变化更新。
        image.AttachedToVisualTree += (_, _) =>
        {
            var scrollViewer = image.FindAncestorOfType<ScrollViewer>();
            if (scrollViewer is null)
            {
                return;
            }

            scrollViewer.SizeChanged += OnSizeChanged;
            image.DetachedFromLogicalTree += (_, _) =>
            {
                scrollViewer.SizeChanged -= OnSizeChanged;
                Application.Current?.PropertyChanged -= OnThemeChanged;
                cts?.Cancel();
                cts?.Dispose();
            };
            Update();

            void Update()
            {
                var width = scrollViewer.Viewport.Width;
                if (width > 0)
                {
                    image.MaxWidth = Math.Min(width, 800);
                }
            }

            void OnSizeChanged(object? sender, SizeChangedEventArgs e) => Update();
        };

        _ = ApplyThemeAsync();

        // Mermaid 解析渲染与 SkiaSharp SVG 加载都是 CPU 密集操作，放后台线程；
        // CTS 用于主题快速切换时取消在途渲染。
        async Task ApplyThemeAsync()
        {
            cts?.Cancel();
            cts?.Dispose();
            var localCts = cts = new CancellationTokenSource();
            var token = localCts.Token;

            var options = GetRenderOptions();
            var cacheKey = CreateCacheKey(source, options);

            try
            {
                var svgSource = await Task.Run(() => ResolveSvgSource(cacheKey, source, options), token);

                if (!token.IsCancellationRequested)
                {
                    if (svgSource is null)
                    {
                        // 上次渲染已失败且结果被缓存：本次直接回落，不重复尝试。
                        ShowFallback("Mermaid render failed.");
                        return;
                    }

                    image.Source = new SvgImage { Source = svgSource };
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (!token.IsCancellationRequested)
                {
                    ShowFallback(exception.Message);
                }
            }

            void ShowFallback(string message)
            {
                var panel = new StackPanel { Spacing = 4 };
                panel.Children.Add(new TextBlock { Text = $"Mermaid render error: {message}" });
                panel.Children.Add(new TextBlock { Text = source, TextWrapping = TextWrapping.Wrap });
                border.Child = panel;
                border.Classes.Clear();
                border.Classes.Add("MdMermaidFallback");
            }
        }

        return border;
    }

    /// <summary>
    /// 取图表 SVG：按「源码 + 主题配色」缓存，块被虚拟化回收后重新物化时
    /// 直接复用已解析的 SVG，不再跑一遍布局与 CSS 变量内联。
    /// 渲染失败以 null 缓存，避免每次重建都重试同一份坏图。
    /// </summary>
    private static SvgSource? ResolveSvgSource(string cacheKey, string source, MermaidRenderOptions options)
    {
        lock (DiagramCacheGate)
        {
            if (DiagramCache.TryGetValue(cacheKey, out var cached))
            {
                DiagramCacheOrder.Remove(cacheKey);
                DiagramCacheOrder.AddLast(cacheKey);
                return cached;
            }
        }

        SvgSource? svgSource = null;
        try
        {
            var svg = MermaidRenderer.RenderSvg(source, options);
            svg = InlineCssVariables(svg, options);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
            svgSource = SvgSource.LoadFromStream(stream);
        }
        catch
        {
            svgSource = null;
        }

        lock (DiagramCacheGate)
        {
            DiagramCache[cacheKey] = svgSource;
            DiagramCacheOrder.AddLast(cacheKey);
            while (DiagramCacheOrder.Count > MaxCachedDiagrams && DiagramCacheOrder.First is { } oldest)
            {
                DiagramCacheOrder.RemoveFirst();
                DiagramCache.Remove(oldest.Value);
            }
        }

        return svgSource;
    }

    /// <summary>缓存 Key 含主题配色：同一份图表在明暗主题下是两张不同的图。</summary>
    private static string CreateCacheKey(string source, MermaidRenderOptions options) =>
        $"{options.Bg}|{options.Fg}|{options.Accent}|{source}";

    /// <summary>
    /// 按当前主题变体构造渲染选项；颜色来自 CodeWFMarkdownMermaid* 画刷资源
    /// （排版主题可覆盖），字面量仅作资源缺失时的兜底。
    /// </summary>
    private static MermaidRenderOptions GetRenderOptions()
    {
        var app = Application.Current;
        var theme = app?.ActualThemeVariant ?? ThemeVariant.Light;
        var isDark = IsDarkTheme(theme);

        return new MermaidRenderOptions
        {
            Bg = ResolveHex(app, theme, "CodeWFMarkdownMermaidBackground", isDark ? "#18181B" : "#FFFFFF"),
            Fg = ResolveHex(app, theme, "CodeWFMarkdownMermaidForeground", isDark ? "#FAFAFA" : "#27272A"),
            Accent = ResolveHex(app, theme, "CodeWFMarkdownMermaidAccent", isDark ? "#60A5FA" : "#3B82F6"),
            Transparent = false
        };
    }

    internal static bool IsDarkTheme(ThemeVariant variant)
    {
        for (ThemeVariant? current = variant; current is not null; current = current.InheritVariant)
            if (current == ThemeVariant.Dark) return true;
        return false;
    }

    private static string ResolveHex(Application? app, ThemeVariant theme, string resourceKey, string fallback)
    {
        if (app != null
            && app.TryGetResource(resourceKey, theme, out var resource)
            && resource is ISolidColorBrush brush)
        {
            var color = brush.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        return fallback;
    }

    private readonly record struct Rgb(byte R, byte G, byte B);

    /// <summary>
    /// 把 Mermaider 的 CSS 自定义属性引用（var(--_xxx)）替换为计算后的 hex 色值，
    /// 公式镜像自 Mermaider 样式块中的 color-mix 定义。
    /// </summary>
    internal static string InlineCssVariables(string svg, MermaidRenderOptions options)
    {
        var bg = Parse(options.Bg ?? "#FFFFFF");
        var fg = Parse(options.Fg ?? "#27272A");
        var acc = Parse(options.Accent ?? "#3b82f6");

        var vars = new (string Token, Rgb Color)[]
        {
            ("var(--_text)", fg),
            ("var(--_text-sec)", Mix(fg, 55, bg)),
            ("var(--_text-muted)", Mix(fg, 35, bg)),
            ("var(--_text-faint)", Mix(fg, 20, bg)),
            ("var(--_line)", Mix(fg, 32, bg)),
            ("var(--_arrow)", acc),
            ("var(--_node-fill)", Mix(fg, 4, bg)),
            ("var(--_node-stroke)", Mix(fg, 22, bg)),
            ("var(--_group-fill)", bg),
            ("var(--_group-hdr)", Mix(fg, 4, bg)),
            ("var(--_group-stroke)", Mix(fg, 10, bg)),
            ("var(--_inner-stroke)", Mix(fg, 10, bg)),
            ("var(--_key-badge)", Mix(fg, 8, bg)),
            ("var(--_accent-fill)", Mix(acc, 8, bg)),
            ("var(--_accent-stroke)", Mix(acc, 20, bg)),
            ("var(--_accent-text)", Mix(acc, 65, bg))
        };

        var builder = new StringBuilder(svg);
        foreach (var (token, color) in vars)
        {
            builder.Replace(token, Hex(color));
        }

        // 0.14.x 除样式块外还以属性形式（fill=\"var(--bg)\"、var(--fg)）引用根变量
        builder.Replace("var(--bg)", Hex(bg));
        builder.Replace("var(--fg)", Hex(fg));
        builder.Replace("var(--fs-xs)", "12");
        builder.Replace("var(--fs-s)", "14");
        builder.Replace("var(--fs-m)", "16");
        builder.Replace("var(--fs-l)", "18");

        // Mermaider 0.14.x 在节点 fill 属性里也直接输出 color-mix，Skia 会将
        // 不支持的颜色当作黑色。变量替换后将这些具体混色表达式一并计算。
        return Regex.Replace(builder.ToString(),
            @"color-mix\(\s*in\s+srgb,\s*(#[0-9a-fA-F]{6})\s+(\d+(?:\.\d+)?)%,\s*(#[0-9a-fA-F]{6})\s*\)",
            match => Hex(Mix(Parse(match.Groups[1].Value),
                double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), Parse(match.Groups[3].Value))));

        static Rgb Parse(string hex)
        {
            hex = hex.TrimStart('#');
            return new Rgb(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16));
        }

        // color-mix(in srgb, a N%, b) —— sRGB 空间线性插值
        static Rgb Mix(Rgb a, double aPercent, Rgb b)
        {
            double bPercent = 100 - aPercent;
            return new Rgb(
                (byte)(a.R * aPercent / 100 + b.R * bPercent / 100),
                (byte)(a.G * aPercent / 100 + b.G * bPercent / 100),
                (byte)(a.B * aPercent / 100 + b.B * bPercent / 100));
        }

        static string Hex(Rgb color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static string ExtractSource(FencedCodeBlock block)
    {
        if (block.Lines.Lines is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var line in block.Lines.Lines)
        {
            builder.AppendLine(line.ToString().TrimEnd());
        }

        return builder.ToString();
    }
}
