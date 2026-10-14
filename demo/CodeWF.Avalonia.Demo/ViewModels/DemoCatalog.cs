using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>原型与原生导航共用内嵌的 API 目录。</summary>
public static partial class DemoCatalog
{
    public static ObservableCollection<DemoGroupViewModel> CreateGroups()
    {
        using var stream = typeof(DemoCatalog).Assembly.GetManifestResourceStream("Demo.Catalog.json")
            ?? throw new InvalidOperationException("Demo catalog resource missing.");
        var groups = JsonSerializer.Deserialize(stream, DemoCatalogJsonContext.Default.CatalogGroupArray)
            ?? throw new InvalidOperationException("Demo catalog is empty.");
        return new(groups.Select(group =>
        {
            var items = group.Items.Select(entry => new DemoItemViewModel(entry.TitleKey,
                () => CreatePage(entry.TitleKey), entry.Api, entry.Package, entry.PrototypeId, entry.SectionKey, entry.Icon, entry.IconKey));
            return new DemoGroupViewModel(group.TitleKey, group.Icon, items);
        }));
    }
}

internal sealed record CatalogGroup(string TitleKey, string Icon, CatalogItem[] Items);
internal sealed record CatalogItem(string TitleKey, string Api, string Package, string PrototypeId, string Icon, string SectionKey = "", string IconKey = "SemiIconGridView");

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CatalogGroup[]))]
internal partial class DemoCatalogJsonContext : JsonSerializerContext;
