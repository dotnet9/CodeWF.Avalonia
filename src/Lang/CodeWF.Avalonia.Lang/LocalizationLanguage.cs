namespace CodeWF.Avalonia.Lang;

/// <summary>Compatibility metadata type for existing Avalonia language selectors.</summary>
public class LocalizationLanguage : global::CodeWF.Toolkit.Lang.LocalizationLanguage
{
    internal static LocalizationLanguage FromCore(global::CodeWF.Toolkit.Lang.LocalizationLanguage language)
    {
        if (language is LocalizationLanguage compatible) return compatible;
        var result = new LocalizationLanguage
        {
            Language = language.Language, Description = language.Description, CultureName = language.CultureName
        };
        foreach (var (key, value) in language.Languages) result.Languages[key] = value;
        return result;
    }
}
