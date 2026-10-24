using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using CodeWF.Toolkit.Lang;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>界面配色选项（规则 §13 注册表驱动：内置九套 + 用户配色，名称与描述随注册表）。
/// 卡片预览 = 编写层 8 色（浅/深 × 纸面/面板/墨色/强调）的 4×2 圆点（原型 swatch-dots）。</summary>
public sealed class AppearanceOption : global::ReactiveUI.ReactiveObject
{
    public AppearanceOption(string key, string title, string note, IBrush swatch, bool isRecommended = false, bool isUser = false)
    {
        Key = key; Title = title; Note = note; Swatch = swatch;
        IsRecommended = isRecommended; IsUser = isUser;
    }

    public string Key { get; }
    public string Title { get; }
    public string Note { get; }
    public IBrush Swatch { get; }

    /// <summary>推荐配色（原型 palette-badge，置顶并带角标）。</summary>
    public bool IsRecommended { get; }

    /// <summary>用户自定义配色（卡片带 ✎/×，可编辑与删除）。</summary>
    public bool IsUser { get; }

    /// <summary>当前应用的配色（Vex palette-card-on：选中环 + 右缘 ✓）。</summary>
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => global::ReactiveUI.IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref _isSelected, value);
    }

    /// <summary>编写层 8 色圆点（浅 4 + 深 4，4×2 排布）。</summary>
    public System.Collections.Generic.IReadOnlyList<IBrush> SwatchDots { get; init; } = System.Array.Empty<IBrush>();

    public ThemeVariant ResolveVariant(bool dark) => StudioTheme.GetVariant(Key, dark);
    public override string ToString() => Title;

    /// <summary>从控制器注册表构建全部配色选项（含用户自定义；推荐置顶；圆点取编写层基色）。</summary>
    public static System.Collections.Generic.IReadOnlyList<AppearanceOption> CreateAll(StudioThemeController controller)
    {
        static IBrush Brush(string hex) => new SolidColorBrush(global::Avalonia.Media.Color.Parse(hex));
        static System.Collections.Generic.IReadOnlyList<IBrush> Dots(StudioBaseColors light, StudioBaseColors dark) =>
            new[] { Brush(light.Paper), Brush(light.Surface), Brush(light.Ink), Brush(light.Accent),
                    Brush(dark.Paper), Brush(dark.Surface), Brush(dark.Ink), Brush(dark.Accent) };

        var entries = controller.Registry.List();
        var built = new System.Collections.Generic.List<AppearanceOption>();
        foreach (var entry in entries)
        {
            var definition = controller.Registry.Find(entry.Key)!;
            built.Add(new AppearanceOption(entry.Key, entry.Name, entry.Note, controller.GetSwatch(entry.Key), definition.Recommended)
            {
                SwatchDots = Dots(definition.Light, definition.Dark),
            });
        }
        return built;
    }
}

/// <summary>新增/编辑用户配色的草稿（8 个基色 + 名称；视图以对话框呈现，确认后经注册表落库）。</summary>
public sealed class UserPaletteDraft
{
    private static readonly System.Collections.Generic.IReadOnlyList<string> Defaults =
    [
        "#FBFEFD", "#EDF7F5", "#102722", "#0F766E",
        "#0E1614", "#16211E", "#DFF2EE", "#5EEAD4",
    ];

    public UserPaletteDraft(string? key = null) : this(
        key: key,
        name: null,
        colors: Defaults,
        labels:
        [
            "Demo.Shell.ColorLightPaper", "Demo.Shell.ColorLightSurface", "Demo.Shell.ColorLightInk", "Demo.Shell.ColorLightAccent",
            "Demo.Shell.ColorDarkPaper", "Demo.Shell.ColorDarkSurface", "Demo.Shell.ColorDarkInk", "Demo.Shell.ColorDarkAccent",
        ])
    {
    }

    public UserPaletteDraft(string? key, string? name, System.Collections.Generic.IReadOnlyList<string> colors, System.Collections.Generic.IReadOnlyList<string> labels)
    {
        Key = key;
        Name = name ?? string.Empty;
        Colors = [.. colors];
        Labels = labels;
    }

    /// <summary>null = 新增；user:* = 编辑既有自定义配色。</summary>
    public string? Key { get; }

    public string Name { get; set; } = string.Empty;

    /// <summary>顺序：浅(纸面/面板/墨色/强调) + 深(纸面/面板/墨色/强调)。</summary>
    public System.Collections.Generic.IList<string> Colors { get; }

    public System.Collections.Generic.IReadOnlyList<string> Labels { get; }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Name)
        && Colors.All(c => System.Text.RegularExpressions.Regex.IsMatch(c, "^#[0-9a-fA-F]{6}$"));
}

/// <summary>保留对象身份的主题模式选项，切换语言只刷新显示标题。</summary>
public sealed class ThemeModeOption(string titleKey) : global::ReactiveUI.ReactiveObject
{
    public string Title => I18nManager.Instance.GetResource(titleKey);
    public void RefreshTitle() => global::ReactiveUI.IReactiveObjectExtensions.RaisePropertyChanged(this, nameof(Title));
    public override string ToString() => Title;
}

/// <summary>可选语言。</summary>
public sealed class CultureOption
{
    public CultureOption(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
        ShortName = name switch
        {
            "zh-CN" => "中",
            "zh-Hant" => "繁",
            "ja-JP" => "日",
            _ => "EN",
        };
    }

    public string Name { get; }

    /// <summary>语言名以其自身语言展示（不随界面语言切换）。</summary>
    public string DisplayName { get; }

    /// <summary>语言菜单 24px 徽标文字（Vex studio-language chip）。</summary>
    public string ShortName { get; }

    public override string ToString() => DisplayName;
}
