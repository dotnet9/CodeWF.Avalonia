using global::Avalonia.Controls;

namespace CodeWF.Avalonia.Markdown.Themes;

/// <summary>
/// 排版查找的统一入口：内置目录（生成的静态表）+ 应用运行时注册的用户排版。
/// 渲染器与导出统一走这里，生成文件不感知用户数据。
/// </summary>
public static class ArticleTypographyResolver
{
    private static readonly object Gate = new();
    private static readonly Dictionary<string, ArticleTypography> Custom = new(StringComparer.Ordinal);

    public static void Register(ArticleTypography typography)
    {
        if (string.IsNullOrWhiteSpace(typography.Key)) return;
        lock (Gate)
        {
            Custom[typography.Key] = typography;
        }
        MarkdownTypographyThemeRegistry.Register(typography.Key, typography.CreateResources);
    }

    public static bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        lock (Gate)
        {
            return Custom.Remove(key);
        }
    }

    public static ArticleTypography? Find(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        lock (Gate)
        {
            if (Custom.TryGetValue(key, out var custom)) return custom;
        }
        return ArticleTypographyCatalog.Find(key);
    }

    public static IReadOnlyList<ArticleTypography> All
    {
        get
        {
            lock (Gate)
            {
                return Custom.Values.Concat(ArticleTypographyCatalog.All).ToList();
            }
        }
    }

    /// <summary>以内置「简约」为模板派生用户排版：改名称/强调与装饰色/字体/行距，其余沿用。</summary>
    public static ArticleTypography CreateCustom(string key, string name, string accentLight, string accentDark,
        string decorationLight, string decorationDark, string font, double leading)
    {
        var template = ArticleTypographyCatalog.Find("Simple")
            ?? ArticleTypographyCatalog.All.First();
        return template with
        {
            Key = key,
            Name = name,
            EnglishName = "Custom",
            Purpose = "自定义排版",
            Recommended = false,
            LightText = accentLight,
            DarkText = accentDark,
            LightDecoration = decorationLight,
            DarkDecoration = decorationDark,
            Font = font,
            Leading = Math.Clamp(leading, 1.5, 2.4),
        };
    }
}
