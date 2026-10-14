namespace CodeWF.Avalonia.Lang;

/// <summary>兼容现有 Avalonia 语言选择器的语言元数据类型。</summary>
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
