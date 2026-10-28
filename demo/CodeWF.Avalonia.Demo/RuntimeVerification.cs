using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Avalonia.Demo.Infrastructure;
using CodeWF.Avalonia.Demo.ViewModels;
using ResourceKeys = CodeWF.Avalonia.Demo.I18n.ResourceKeys;

namespace CodeWF.Avalonia.Demo;

/// <summary>无需桌面即可验证正式原生程序的输出资源与内嵌目录，不读取用户配置。</summary>
internal static class RuntimeVerification
{
    public static void Run(string directory)
    {
        if (RuntimeFeature.IsDynamicCodeSupported)
            throw new InvalidOperationException("Runtime verification requires a NativeAOT executable.");
        var root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        var resources = DemoBootstrapper.I18nFolder;
        var manager = I18nManager.Instance;
        manager.Register(new JsonLangPlugin { ResourceFolder = resources }, CultureInfo.GetCultureInfo("en-US"));
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("Demo.Catalog.json")
            ?? throw new InvalidOperationException("Published API catalog is missing.");
        var groups = JsonSerializer.Deserialize(stream, DemoCatalogJsonContext.Default.CatalogGroupArray)
            ?? throw new InvalidOperationException("Published API catalog is empty.");
        var entries = groups.SelectMany(group => group.Items).ToArray();
        // 常数由 sync_design_catalog.ps1 随目录一起生成：目录加了一页而这里仍是字面量，
        // 报错的是自检而不是产品，所以两边只留一个来源。
        if (entries.Length != DemoCatalog.RouteCount
            || entries.Select(entry => entry.PrototypeId).Distinct().Count() != DemoCatalog.RouteCount)
        {
            throw new InvalidOperationException(
                $"Published API catalog routes are incomplete or duplicated: {entries.Length} entries vs {DemoCatalog.RouteCount} generated routes.");
        }
        var labels = new HashSet<string>();
        foreach (var culture in new[] { "en-US", "zh-CN" })
        {
            if (!File.Exists(Path.Combine(resources, "CodeWF.Avalonia.Demo." + culture + ".json")))
                throw new InvalidOperationException("Published language file is missing: " + culture);
            manager.Culture = CultureInfo.GetCultureInfo(culture);
            foreach (var key in entries.Select(entry => entry.TitleKey).Append(ResourceKeys.Shell.Ready))
            {
                var text = manager.GetResource(key);
                if (string.IsNullOrWhiteSpace(text) || text == key)
                    throw new InvalidOperationException("Published language resource is missing: " + key + "/" + culture);
            }
            labels.Add(manager.GetResource(ResourceKeys.Shell.Ready));
        }
        if (labels.Count != 2) throw new InvalidOperationException("Published languages do not switch independently.");
        var result = $"NativeAOT runtime verification passed; version={typeof(Program).Assembly.GetName().Version}; languages=2; routes={entries.Length}; userConfiguration=untouched";
        File.WriteAllText(Path.Combine(root, "runtime-verification.txt"), result);
        Console.WriteLine(result);
    }
}
