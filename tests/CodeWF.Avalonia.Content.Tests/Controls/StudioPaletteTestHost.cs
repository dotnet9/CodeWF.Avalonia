using Avalonia;
using CodeWF.Avalonia.Controls.Themes;

namespace CodeWF.Avalonia.Content.Tests.Controls;

/// <summary>测试宿主：内置九套经静态 axaml include 提供（StudioThemeVariants.g.cs），无需运行时填充。</summary>
public static class StudioPaletteTestHost
{
    public static IEnumerable<string> Keys() => StudioPalettes.All.Select(palette => palette.Key);
}
