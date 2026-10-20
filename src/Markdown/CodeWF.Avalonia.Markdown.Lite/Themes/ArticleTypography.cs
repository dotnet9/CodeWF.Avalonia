using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Styling;

namespace CodeWF.Avalonia.Markdown.Themes;

public sealed record ArticleHeadingStyle(double Size, int Weight, string Alignment, string Border, string Padding,
    double Tint, string Foreground, double Radius, string Font = "system");

/// <summary>正文、卡片和导出共用的文章排版定义；标题装饰与文字强调分别取色。</summary>
public sealed record ArticleTypography(string Key, string Name, string EnglishName, string Purpose, bool Recommended,
    double BodySize, double Leading, string LightText, string DarkText, string LightDecoration, string DarkDecoration,
    string Font, ArticleHeadingStyle[] Headings, string ExportCss, string CompactExportCss)
{
    public static FontFamily ResolveFont(string font) => new(font switch
    {
        "serif" => "Noto Serif SC, Songti SC, SimSun, serif",
        "mono" => "Cascadia Mono, Consolas, Menlo, monospace",
        _ => "Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI, PingFang SC, Noto Sans SC"
    });
    public ResourceDictionary CreateResources()
    {
        var resources = new ResourceDictionary
        {
            [MarkdownStyleKeys.ParagraphFontSizeResource] = BodySize,
            [MarkdownStyleKeys.ParagraphLineHeightResource] = BodySize * Leading,
            ["CodeWFMarkdownBlockSpacing"] = 0d,
            ["CodeWFMarkdownParagraphMargin"] = new Thickness(0, 0, 0, BodySize),
            ["CodeWFMarkdownHeadingMargin"] = new Thickness(0, BodySize * 1.4, 0, BodySize * .45),
            ["CodeWFMarkdownQuotePadding"] = new Thickness(16, 10),
            ["CodeWFMarkdownQuoteMargin"] = new Thickness(0, 12, 0, 20),
            ["CodeWFMarkdownQuoteBorderThickness"] = new Thickness(3, 0, 0, 0),
            ["CodeWFMarkdownQuoteCornerRadius"] = new CornerRadius(4),
        };
        for (var i = 0; i < Headings.Length; i++)
        {
            var h = Headings[i]; var prefix = "CodeWFMarkdownHeading" + (i + 1);
            resources[prefix + "FontSize"] = h.Size;
            resources[prefix + "FontWeight"] = (FontWeight)h.Weight;
            resources[prefix + "TextAlignment"] = Enum.Parse<TextAlignment>(h.Alignment);
            resources[prefix + "BorderThickness"] = Thickness.Parse(h.Border);
            resources[prefix + "Padding"] = Thickness.Parse(h.Padding);
            resources[prefix + "CornerRadius"] = new CornerRadius(h.Radius);
        }
        resources.ThemeDictionaries[ThemeVariant.Light] = Colors(false);
        resources.ThemeDictionaries[ThemeVariant.Dark] = Colors(true);
        return resources;
    }

    private ResourceDictionary Colors(bool dark)
    {
        var foreground = Brush.Parse(dark ? "#E4E5E7" : "#30343B");
        var accent = Brush.Parse(dark ? DarkText : LightText);
        var decoration = Color.Parse(dark ? DarkDecoration : LightDecoration);
        var paper = Color.Parse(dark ? "#202124" : "#FFFFFF");
        var resources = new ResourceDictionary
        {
            [MarkdownStyleKeys.TextBrushResource] = foreground,
            [MarkdownStyleKeys.AccentBrushResource] = accent,
            [MarkdownStyleKeys.MutedTextBrushResource] = Brush.Parse(dark ? "#AFB4BB" : "#606770"),
            [MarkdownStyleKeys.BorderBrushResource] = Brush.Parse(dark ? "#484C53" : "#DDE1E5"),
            [MarkdownStyleKeys.QuoteBackgroundBrushResource] = new SolidColorBrush(Mix(paper, decoration, .06)),
            [MarkdownStyleKeys.CodeBackgroundBrushResource] = Brush.Parse(dark ? "#292B2F" : "#F6F7F8"),
            [MarkdownStyleKeys.InlineCodeBackgroundBrushResource] = Brush.Parse(dark ? "#292B2F" : "#F6F7F8"),
            [MarkdownStyleKeys.TableHeaderBackgroundBrushResource] = Brush.Parse(dark ? "#292B2F" : "#F6F7F8"),
        };
        for (var i = 0; i < Headings.Length; i++)
        {
            var h = Headings[i]; var prefix = "CodeWFMarkdownHeading" + (i + 1);
            resources[prefix + "Foreground"] = h.Tint == 1 ? Brush.Parse(OnSolid(decoration))
                : h.Foreground.Contains("--fg)", StringComparison.Ordinal) ? foreground : accent;
            resources[prefix + "Background"] = h.Tint == 0 ? Brushes.Transparent : new SolidColorBrush(Mix(paper, decoration, h.Tint));
            resources[prefix + "BorderBrush"] = new SolidColorBrush(decoration);
        }
        return resources;
    }

    private static Color Mix(Color a, Color b, double amount) => Color.FromRgb(
        (byte)Math.Round(a.R * (1 - amount) + b.R * amount), (byte)Math.Round(a.G * (1 - amount) + b.G * amount),
        (byte)Math.Round(a.B * (1 - amount) + b.B * amount));

    private static string OnSolid(Color color)
    {
        static double Linear(byte v) => v / 255d <= .04045 ? v / 255d / 12.92 : Math.Pow((v / 255d + .055) / 1.055, 2.4);
        var luminance = .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        return (1.05 / (luminance + .05)) >= ((luminance + .05) / .065) ? "#FFFFFF" : "#17211C";
    }
}
