using L = global::CodeWF.Avalonia.Lang.I18nManager;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace CodeWF.Avalonia.Demo.ViewModels.Pages;

public sealed record SearchItem(string Name, string Category)
{
    public override string ToString() => $"{Name} · {Category}";
}

/// <summary>SearchListBox 演示页 VM：数据源 + 交互事件日志。</summary>
public class SearchListBoxPageViewModel : ReactiveObject
{
    public ObservableCollection<SearchItem> Items { get; } = CreateItems(64);

    public ObservableCollection<string> EventLog { get; } = new();

    public void Log(string message) => EventLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");

    private static ObservableCollection<SearchItem> CreateItems(int count)
    {
        var categories = new[] { L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.Audio"), L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.Videos"), L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.Documents"), L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.Images"), L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.Archives") };
        var items = new ObservableCollection<SearchItem>();
        for (var i = 1; i <= count; i++)
        {
            items.Add(new SearchItem(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.SearchListBox.CreateItems.AssetFormat"), i), categories[i % categories.Length]));
        }

        return items;
    }
}
