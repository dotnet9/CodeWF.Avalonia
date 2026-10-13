using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeWF.Avalonia.Demo.Views;
using CodeWF.Avalonia.Demo.Views.Pages;
using CodeWF.Avalonia.Demo.Views.Scenes;
using CodeWF.Avalonia.Demo.ViewModels.Pages;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>The prototype and native navigation share an embedded API catalog.</summary>
public static class DemoCatalog
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
                () => CreatePage(entry.TitleKey), entry.Api, entry.Package, entry.PrototypeId));
            return new DemoGroupViewModel(group.TitleKey, items);
        }));
    }

    private static DemoPageBase CreatePage(string key) => key switch
    {
        "Demo.Item.Welcome" => new WelcomePage(),
        "Demo.Item.About" => new AboutPage(),
        "Demo.Item.SearchListBox" => new SearchListBoxPage(),
        "Demo.Item.Transfer" => new TransferPage(),
        "Demo.Item.ColumnDisplayEditor" => new ColumnDisplayEditorPage(),
        "Demo.Item.StatusBadge" => new StatusComponentsPage("StatusBadge"),
        "Demo.Item.StatusCard" => new StatusComponentsPage("StatusCard"),
        "Demo.Item.Guide" => new GuidePage(),
        "Demo.Item.Notifications" => new ControlApiPage("LogNotifications"),
        "Demo.Item.DataGrid" => new DataGridPage(),
        "Demo.Item.TreeDataGrid" => new ControlApiPage("TreeDataGridExtension"),
        "Demo.Item.PropertyGrid" => new PropertyGridPage(),
        "Demo.Item.Log" => new LogPage(),
        "Demo.Item.CodeWFWindow" => new CodeWFWindowPage(),
        "Demo.Item.TitleBar" => new ControlApiPage("CodeWFTitleBar"),
        "Demo.Item.Dock" => new DockPage(),
        "Demo.Item.TrapezoidTab" => new TrapezoidTabPage(),
        "Demo.Item.WindowedStackPanel" => new WindowedStackPanelPage(),
        "Demo.Item.DiagonalLine" => new ControlApiPage("LeftTop2RightBottomLine"),
        "Demo.Item.MarkdownViewer" => new MarkdownViewerPage(),
        "Demo.Item.MarkdownLite" => new MarkdownLitePage(),
        "Demo.Item.MarkdownEditor" => new MarkdownEditorPage(),
        "Demo.Item.MarkdownLiveEditor" => new MarkdownLiveEditorPage(),
        "Demo.Item.If" => new MarkupExtensionPage("If"),
        "Demo.Item.Switch" => new MarkupExtensionPage("Switch"),
        "Demo.Item.Lang" => new LangPage(),
        "Demo.Item.MarkdownExport" => new MarkdownExportPage(),
        "Demo.Item.MarkdownFlow" => new MarkdownFlowPage(),
        "Demo.Item.DataFlow" => new DataFlowPage(),
        "Demo.Item.DocsFlow" => new DocsFlowPage(),
        "Demo.Item.AppGuide" => new AppGuidePage(),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown catalog API.")
    };
}

internal sealed record CatalogGroup(string TitleKey, CatalogItem[] Items);
internal sealed record CatalogItem(string TitleKey, string Api, string Package, string PrototypeId);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CatalogGroup[]))]
internal partial class DemoCatalogJsonContext : JsonSerializerContext;
