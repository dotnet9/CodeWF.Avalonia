using System.Globalization;

// Localization access for the PropertyGrid chrome: resolves keys through the host
// application's Lang plugin when registered, falling back to the embedded resources.
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Lang.Json;

namespace CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

public static class PropertyGridLocalization
{
    public const string DefaultCultureName = "en-US";

    private static readonly Lazy<JsonLangPlugin> EmbeddedResources = new(CreateEmbeddedResourcePlugin);
    private static bool _resourceRegistered;

    /// <summary>
    /// Adds the embedded PropertyGrid resources to the active Lang plugin. Safe to call
    /// repeatedly; no-ops while no plugin is registered.
    /// </summary>
    public static void RegisterResources()
    {
        if (_resourceRegistered)
        {
            return;
        }

        I18nManager.Instance.AddResource(typeof(PropertyGridLocalization).Assembly);
        _resourceRegistered = true;
    }

    /// <summary>
    /// Gets a localized string. Falls back to the embedded English resource when no
    /// Lang plugin provides the key.
    /// </summary>
    public static string Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var localized = I18nManager.Instance.GetResource(key);
        return string.Equals(localized, key, StringComparison.Ordinal)
            ? EmbeddedResources.Value.GetResource(key, DefaultCultureName)
            : localized;
    }

    private static JsonLangPlugin CreateEmbeddedResourcePlugin()
    {
        var plugin = new JsonLangPlugin
        {
            ResourceFolder = Path.Combine(AppContext.BaseDirectory, ".codewf-treedatagrid-embedded-i18n")
        };
        plugin.Load(new CultureInfo(DefaultCultureName));
        plugin.AddResource(typeof(PropertyGridLocalization).Assembly);
        return plugin;
    }
}
