using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media;
using CodeWF.Avalonia.Controls.Themes;
using Xunit;

namespace CodeWF.Avalonia.Controls.Tests;

/// <summary>
/// 同一套令牌有两种落地：内置九套读静态 <c>Tokens/*.axaml</c>（由 Vex 的 sync_native_design.cjs 生成），
/// 用户配色运行时走 <see cref="StudioTokens.Fill"/> 整本替换变体字典。
/// 两侧一旦不同步，症状是「内置配色正常、自定义配色某处失联」——画刷 DynamicResource 取不到值直接透明，
/// 极难归因。这里把键集与取色公式都钉住：分段轨道/指示块与浮起面三键就是先漏在 Fill 一侧的。
/// </summary>
public sealed class StudioTokenParityTests
{
    private static readonly string TokensDirectory = LocateTokens();

    private static readonly Dictionary<string, string> VariantNames = new()
    {
        ["graphite"] = "Graphite", ["indigo"] = "Indigo", ["teal"] = "Teal", ["pine"] = "Pine",
        ["sky"] = "Sky", ["amber"] = "Amber", ["crimson"] = "Crimson", ["violet"] = "Violet",
        ["terracotta"] = "Terracotta",
    };

    public static TheoryData<string, bool> PaletteModes { get; } = CreatePaletteModes();

    /// <summary>静态令牌定义了、运行时却没落的键 = 用户配色上那片透明的画刷。</summary>
    [Theory]
    [MemberData(nameof(PaletteModes))]
    public void RuntimeFillDefinesEveryStaticColorToken(string paletteKey, bool dark)
    {
        var label = paletteKey + "/" + (dark ? "dark" : "light");
        var (dictionary, _) = Fill(paletteKey, dark);
        foreach (var key in ReadStaticTokens(paletteKey, dark).Keys)
        {
            Assert.True(dictionary.TryGetResource(key, null, out var value), label + " 运行时字典缺 " + key);
            Assert.IsType<Color>(value!);
        }
    }

    /// <summary>反过来：运行时填了、静态令牌里没有的键（内置九套取不到，只有用户配色生效）同样算漂移。</summary>
    [Theory]
    [MemberData(nameof(PaletteModes))]
    public void StaticTokensDefineEveryRuntimeColorToken(string paletteKey, bool dark)
    {
        var label = paletteKey + "/" + (dark ? "dark" : "light");
        var (dictionary, accessed) = Fill(paletteKey, dark);
        var staticKeys = ReadStaticTokens(paletteKey, dark).Keys.ToHashSet();
        foreach (var key in accessed)
        {
            if (!dictionary.TryGetResource(key, null, out var value) || value is not Color) continue;
            Assert.True(staticKeys.Contains(key), label + " 运行时填了静态令牌没有的颜色键 " + key);
        }
    }

    /// <summary>逐键比色：静态令牌烧的是原型 deriveVars 的产物，运行时字典必须一字不差。</summary>
    [Theory]
    [MemberData(nameof(PaletteModes))]
    public void RuntimeColorsMatchThePrototypeTokens(string paletteKey, bool dark)
    {
        var staticValues = ReadStaticTokens(paletteKey, dark);
        var derived = DerivedColors(paletteKey, dark);
        var label = paletteKey + "/" + (dark ? "dark" : "light");
        foreach (var (key, expected) in staticValues)
        {
            Assert.True(derived.TryGetValue(key, out var actual), label + " 派生表没有 " + key);
            // 逐通道比：Color.Equals 还带色彩状态（Aelc/Srgb），两边构造路径不同会误报同色不等。
            Assert.True(expected.R == actual.R && expected.G == actual.G && expected.B == actual.B,
                label + "/" + key + " 静态 #" + Hex(expected) + " ≠ 运行时 #" + Hex(actual));
        }
    }

    /// <summary>规则 §5.1：paper/surface 是大面积画布，去色温后色度归零；带任何一档色相都不是原型的样子。</summary>
    [Theory]
    [MemberData(nameof(PaletteModes))]
    public void CanvasStaysAchromaticAfterNeutralization(string paletteKey, bool dark)
    {
        var derived = DerivedColors(paletteKey, dark);
        var label = paletteKey + "/" + (dark ? "dark" : "light");
        foreach (var key in new[] { "StudioPaperColor", "StudioSurfaceColor" })
        {
            var color = derived[key];
            Assert.True(Math.Abs(color.R - color.G) <= 1 && Math.Abs(color.G - color.B) <= 1,
                label + "/" + key + " = #" + Hex(color) + " 带色温");
        }
    }

    private static string Hex(Color color) => color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

    private static TheoryData<string, bool> CreatePaletteModes()
    {
        var data = new TheoryData<string, bool>();
        foreach (var palette in StudioPalettes.All)
        foreach (var dark in new[] { false, true })
            data.Add(palette.Key, dark);
        return data;
    }

    private static Dictionary<string, Color> DerivedColors(string paletteKey, bool dark)
    {
        var palette = StudioPalettes.All.Single(item => item.Key == paletteKey);
        var derived = StudioPaletteEngine.Derive(paletteKey, palette, dark);
        foreach (var pair in StudioPaletteEngine.DeriveExtras(palette, dark)) derived[pair.Key] = pair.Value;
        return derived;
    }

    private static (ResourceDictionary Dictionary, HashSet<string> Accessed) Fill(string paletteKey, bool dark)
    {
        // 键从派生表读出去的那一瞬间被记下来：Fill 写了哪些令牌，只有它自己知道。
        var accessed = new HashSet<string>();
        var dictionary = new ResourceDictionary();
        StudioTokens.Fill(dictionary, new RecordingDictionary(DerivedColors(paletteKey, dark), accessed));
        return (dictionary, accessed);
    }

    /// <summary>静态令牌里 <c>&lt;Color x:Key&gt;</c> 的直接条目（画刷与别名不参与比对）。</summary>
    private static Dictionary<string, Color> ReadStaticTokens(string paletteKey, bool dark)
    {
        var file = Path.Combine(TokensDirectory, VariantNames[paletteKey] + (dark ? "Dark" : "Light") + ".axaml");
        var values = new Dictionary<string, Color>();
        foreach (Match match in Regex.Matches(File.ReadAllText(file), "<Color x:Key=\"([^\"]+)\">#([0-9A-Fa-f]{6})</Color>"))
        {
            var hex = match.Groups[2].Value;
            values[match.Groups[1].Value] = Color.FromRgb(
                Convert.ToByte(hex.Substring(0, 2), 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
        }

        Assert.True(values.Count > 20, file + " 只解析出 " + values.Count + " 个颜色令牌");
        return values;
    }

    private static string LocateTokens()
    {
        const string relative = "src/Controls/CodeWF.Avalonia.Controls/Themes/Studio/Tokens";
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(candidate)) return candidate;
        }

        throw new DirectoryNotFoundException("找不到静态令牌目录（" + relative + "）：本测试依赖仓库源码树。");
    }

    private sealed class RecordingDictionary(Dictionary<string, Color> inner, HashSet<string> accessed) : IReadOnlyDictionary<string, Color>
    {
        public Color this[string key]
        {
            get
            {
                accessed.Add(key);
                return inner[key];
            }
        }

        public IEnumerable<string> Keys => inner.Keys;

        public IEnumerable<Color> Values => inner.Values;

        public int Count => inner.Count;

        public bool ContainsKey(string key) => inner.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, Color>> GetEnumerator() => inner.GetEnumerator();

        public bool TryGetValue(string key, out Color value)
        {
            accessed.Add(key);
            return inner.TryGetValue(key, out value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => inner.GetEnumerator();
    }
}
