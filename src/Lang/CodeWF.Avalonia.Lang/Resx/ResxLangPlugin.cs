using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

namespace CodeWF.Avalonia.Lang.Resx;

/// <summary>Compatibility RESX plugin backed by the Toolkit parser.</summary>
public class ResxLangPlugin : global::CodeWF.Toolkit.Lang.Resx.ResxLangPlugin<LocalizationLanguage>, ILangPlugin
{
    /// <summary>Creates a loader with convention-based discovery.</summary>
    public ResxLangPlugin() { }
    /// <summary>Creates a loader for an explicit resource manager.</summary>
    public ResxLangPlugin(ResourceManager resourceManager) : base(resourceManager) { }
    /// <summary>Creates a loader for a resource designer type.</summary>
    public ResxLangPlugin([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type resourceType) : base(resourceType) { }
}
