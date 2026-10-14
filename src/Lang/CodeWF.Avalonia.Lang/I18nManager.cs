using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CoreManager = CodeWF.Toolkit.Lang.I18nManager;

namespace CodeWF.Avalonia.Lang;

/// <summary>Avalonia compatibility facade over the shared UI-independent localization manager.</summary>
public class I18nManager : INotifyPropertyChanged
{
    /// <summary>Gets the facade over the process-wide Toolkit manager.</summary>
    public static I18nManager Instance { get; } = new();

    private I18nManager()
    {
        CoreManager.Instance.PropertyChanged += (_, args) => PropertyChanged?.Invoke(this, args);
        CoreManager.Instance.CultureChanged += (_, args) => CultureChanged?.Invoke(this, args);
        CoreManager.Instance.ResourcesChanged += (_, args) => ResourcesChanged?.Invoke(this, args);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raised when either facade changes the shared culture.</summary>
    public event EventHandler<EventArgs>? CultureChanged;
    /// <summary>Raised after resources are added through either facade.</summary>
    public event EventHandler<EventArgs>? ResourcesChanged;
    /// <summary>Gets or sets the shared application culture.</summary>
    public CultureInfo? Culture { get => CoreManager.Instance.Culture; set => CoreManager.Instance.Culture = value; }
    /// <summary>Gets the shared resource version used by localization bindings.</summary>
    public int ResourceVersion => CoreManager.Instance.ResourceVersion;

    /// <summary>Registers a legacy or built-in plugin with the shared manager.</summary>
    public void Register(ILangPlugin plugin, CultureInfo? defaultCulture = null) => CoreManager.Instance.Register(Adapt(plugin), defaultCulture);
    /// <summary>Registers a plugin, preserving the previous one if loading fails.</summary>
    public bool Register(ILangPlugin plugin, CultureInfo defaultCulture, out string? error) => CoreManager.Instance.Register(Adapt(plugin), defaultCulture, out error);
    /// <summary>Adds resources and refreshes both Toolkit and Avalonia consumers.</summary>
    public void AddResource(params Assembly[] assemblies) => CoreManager.Instance.AddResource(assemblies);
    /// <summary>Gets a snapshot of available languages using the legacy metadata type.</summary>
    public List<LocalizationLanguage>? GetLanguages() => CoreManager.Instance.GetLanguages()?.Select(LocalizationLanguage.FromCore).ToList();
    /// <summary>Resolves a translation through the shared manager.</summary>
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
