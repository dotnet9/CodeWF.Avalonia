using System.Collections.Generic;

namespace CodeWF.Avalonia.Controls.Themes;

/// <summary>
/// 配色注册表（规则 §13）：字符串 Key，内置九套 + 运行时注册用户配色（8 色 × 双明暗 + 名称/描述）。
/// 内置只读；用户项可增删改，无兼容负担（自用）。
/// </summary>
public sealed class StudioPaletteRegistry
{
    public sealed record UserPalette(
        string Key, string Name, string Note,
        string LightPaper, string LightSurface, string LightInk, string LightAccent,
        string DarkPaper, string DarkSurface, string DarkInk, string DarkAccent);

    private readonly Dictionary<string, StudioPaletteDefinition> _builtin = new();
    private readonly Dictionary<string, UserPalette> _user = new();

    public StudioPaletteRegistry()
    {
        foreach (var palette in StudioPalettes.All) _builtin[palette.Key] = palette;
    }

    public StudioPaletteDefinition? Find(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        return _builtin.TryGetValue(key, out var builtin) ? builtin
            : _user.TryGetValue(key, out var user) ? ToDefinition(user)
            : null;
    }

    public IReadOnlyList<(string Key, string Name, string Note, bool IsUser)> List()
    {
        var list = new List<(string, string, string, bool)>();
        foreach (var palette in StudioPalettes.All)
            list.Add((palette.Key, palette.Name, palette.Note, false));
        foreach (var user in _user.Values)
            list.Add((user.Key, user.Name, user.Note, true));
        return list;
    }

    /// <summary>注册/更新用户配色（upsert）。返回配色 Key。</summary>
    public string Register(UserPalette palette)
    {
        _user[palette.Key] = palette;
        return palette.Key;
    }

    public bool Remove(string key) => key.StartsWith("user:") && _user.Remove(key);

    private static StudioPaletteDefinition ToDefinition(UserPalette user) => new(
        user.Key, user.Name, "Custom", user.Note, false,
        new StudioBaseColors(user.LightPaper, user.LightSurface, user.LightInk, user.LightAccent),
        new StudioBaseColors(user.DarkPaper, user.DarkSurface, user.DarkInk, user.DarkAccent));
}
