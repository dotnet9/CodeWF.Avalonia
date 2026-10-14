using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

namespace CodeWF.Avalonia.Lang.Resx;

/// <summary>使用 Toolkit 解析器的兼容 RESX 语言插件。</summary>
public class ResxLangPlugin : global::CodeWF.Toolkit.Lang.Resx.ResxLangPlugin<LocalizationLanguage>, ILangPlugin
{
    /// <summary>创建按约定查找资源的加载器。</summary>
    public ResxLangPlugin() { }
    /// <summary>为指定的资源管理器创建加载器。</summary>
    public ResxLangPlugin(ResourceManager resourceManager) : base(resourceManager) { }
    /// <summary>为资源设计器类型创建加载器。</summary>
    public ResxLangPlugin([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type resourceType) : base(resourceType) { }
}
