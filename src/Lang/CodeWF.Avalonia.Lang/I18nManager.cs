using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CoreManager = CodeWF.Toolkit.Lang.I18nManager;

namespace CodeWF.Avalonia.Lang;

/// <summary>基于共享国际化引擎的 Avalonia 兼容入口，底层引擎不依赖界面框架。</summary>
public class I18nManager : INotifyPropertyChanged
{
    /// <summary>获取进程内共享 Toolkit 国际化管理器的兼容入口。</summary>
    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
        CoreManager.Instance.PropertyChanged += (_, args) => PropertyChanged?.Invoke(this, args);
        CoreManager.Instance.CultureChanged += (_, args) => CultureChanged?.Invoke(this, args);
        CoreManager.Instance.ResourcesChanged += (_, args) => ResourcesChanged?.Invoke(this, args);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>任一入口切换共享语言后触发。</summary>
    public event EventHandler<EventArgs>? CultureChanged;
    /// <summary>任一入口添加共享资源后触发。</summary>
    public event EventHandler<EventArgs>? ResourcesChanged;
    /// <summary>获取或设置应用共享的当前语言。</summary>
    public CultureInfo? Culture { get => CoreManager.Instance.Culture; set => CoreManager.Instance.Culture = value; }
    /// <summary>获取国际化绑定使用的共享资源版本。</summary>
    public int ResourceVersion => CoreManager.Instance.ResourceVersion;

    /// <summary>向共享管理器注册兼容旧接口的插件或内置插件。</summary>
    public void Register(ILangPlugin plugin, CultureInfo? defaultCulture = null) => CoreManager.Instance.Register(Adapt(plugin), defaultCulture);
    /// <summary>注册语言插件，加载失败时保留原插件。</summary>
    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error) => CoreManager.Instance.Register(Adapt(plugin), defaultCulture, out error);
    /// <summary>添加资源并同步刷新 Toolkit 与 Avalonia 的资源使用方。</summary>
    public void AddResource(params Assembly[] assemblies) => CoreManager.Instance.AddResource(assemblies);
    /// <summary>以兼容旧接口的元数据类型返回可用语言快照。</summary>
    public List<LocalizationLanguage>? GetLanguages() => CoreManager.Instance.GetLanguages()?.Select(LocalizationLanguage.FromCore).ToList();
    /// <summary>通过共享管理器查询翻译文本。</summary>
    public string GetResource(string key, string? cultureName = null) => CoreManager.Instance.GetResource(key, cultureName);

    private static global::CodeWF.Toolkit.Lang.ILangPlugin Adapt(ILangPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin as global::CodeWF.Toolkit.Lang.ILangPlugin ?? new PluginAdapter(plugin);
    }

    private sealed class PluginAdapter(ILangPlugin plugin) : global::CodeWF.Toolkit.Lang.ILangPlugin
    {
        public CultureInfo Culture { get => plugin.Culture; set => plugin.Culture = value; }
        public void Load(CultureInfo cultureInfo) => plugin.Load(cultureInfo);
        public void AddResource(params Assembly[] assemblies) => plugin.AddResource(assemblies);
        public List<global::CodeWF.Toolkit.Lang.LocalizationLanguage>? GetLanguages() => plugin.GetLanguages()?.Cast<global::CodeWF.Toolkit.Lang.LocalizationLanguage>().ToList();
        public string GetResource(string key, string? cultureName = null) => plugin.GetResource(key, cultureName);
    }
}
