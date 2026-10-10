using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Styling;
using CodeWF.Avalonia.Dock.Themes.Locales;
using Irihi.Avalonia.Shared.Helpers;

namespace CodeWF.Avalonia.Dock.Themes;

public class DockSemiTheme : Styles
{
    private static readonly CultureInfo DefaultLocale = new("en-US");

    private static readonly Dictionary<CultureInfo, ResourceDictionary> LocaleResources = new()
    {
        [new CultureInfo("en-US")] = new en_us(),
        [new CultureInfo("zh-CN")] = new zh_cn()
    };

    private static readonly ResourceDictionary DefaultResources = new en_us();

    public CultureInfo? Locale
    {
        get;
        set
        {
            if (TryGetLocaleResources(value, out var resources) && value is not null)
            {
                field = value;
            }
            else
            {
                field = DefaultLocale;
            }

            Resources.BulkSetResources(resources);
        }
    }

    private static bool TryGetLocaleResources(
        CultureInfo? locale,
        out ResourceDictionary resources)
    {
        if (locale is not null && LocaleResources.TryGetValue(locale, out var localeResources))
        {
            resources = localeResources;
            return true;
        }

        resources = DefaultResources;
        return false;
    }
}
