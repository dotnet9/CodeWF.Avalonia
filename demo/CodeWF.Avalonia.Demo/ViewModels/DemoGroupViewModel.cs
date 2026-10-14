using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Demo.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using ReactiveUI.Primitives;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>演示目录项（导航树叶子节点 / Ribbon 命令按钮）。</summary>
public class DemoItemViewModel : ReactiveObject
{
    private string? _customTitle;
    private bool _isRenaming;
    private bool _isSelected;
    private bool _isFilteredOut;

    public DemoItemViewModel(string titleKey, Func<DemoPageBase> pageFactory, string api = "", string package = "", string prototypeId = "", string sectionKey = "")
    {
        TitleKey = titleKey;
        PageFactory = pageFactory;
        Api = api;
        Package = package;
        PrototypeId = prototypeId;
        SectionKey = sectionKey;

        I18nManager.Instance.CultureChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(Title));
            this.RaisePropertyChanged(nameof(DisplayTitle));
        };
    }

    /// <summary>标题的 Lang 键，同时作为文档的唯一标识。</summary>
    public string TitleKey { get; }
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string Api { get; }
    public string Package { get; }
    public string PrototypeId { get; }
    public string SectionKey { get; }

    public Func<DemoPageBase> PageFactory { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>导航树就地重命名后的标题；为空表示沿用资源标题。</summary>
    public string? CustomTitle
    {
        get => _customTitle;
        set
        {
            if (Equals(_customTitle, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _customTitle, value);
            this.RaisePropertyChanged(nameof(DisplayTitle));
            this.RaisePropertyChanged(nameof(HasCustomTitle));
        }
    }

    public bool HasCustomTitle => !string.IsNullOrEmpty(CustomTitle);

    /// <summary>界面显示名：自定义优先，否则资源标题。</summary>
    public string DisplayTitle => string.IsNullOrEmpty(CustomTitle) ? Title : CustomTitle!;

    /// <summary>导航树是否正把该节点渲染成输入框。</summary>
    public bool IsRenaming
    {
        get => _isRenaming;
        set => this.RaiseAndSetIfChanged(ref _isRenaming, value);
    }

    /// <summary>被导航过滤框排除（仅隐藏，不改变集合）。</summary>
    public bool IsFilteredOut
    {
        get => _isFilteredOut;
        set => this.RaiseAndSetIfChanged(ref _isFilteredOut, value);
    }

    /// <summary>目录图标（与设计原型一致）。</summary>
    public string Icon => DemoIcons.ResolveItem(TitleKey);

    public override string ToString() => DisplayTitle;
}

/// <summary>演示分组（左树组节点 = Ribbon 页签名）。</summary>
public class DemoGroupViewModel : ReactiveObject
{
    private bool _isExpanded;
    private bool _isFilteredOut;

    public DemoGroupViewModel(string titleKey, IEnumerable<DemoItemViewModel> items)
    {
        TitleKey = titleKey;
        Items = new ObservableCollection<DemoItemViewModel>(items);
        RibbonSections = Items.GroupBy(item => string.IsNullOrEmpty(item.SectionKey) ? TitleKey : item.SectionKey)
            .Select(section => new DemoRibbonSection(section.Key, section.ToArray(), Items.Count > 7 || section.Count() > 3)).ToArray();
        ToggleExpandCommand = ReactiveCommand.Create(() => { IsExpanded = !IsExpanded; });

        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    public string TitleKey { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>分组图标（导航树组节点）。</summary>
    public string Icon => DemoIcons.ResolveGroup(TitleKey);

    public ObservableCollection<DemoItemViewModel> Items { get; }

    public IReadOnlyList<DemoRibbonSection> RibbonSections { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>被导航过滤框排除（仅隐藏，不改变集合）。</summary>
    public bool IsFilteredOut
    {
        get => _isFilteredOut;
        set => this.RaiseAndSetIfChanged(ref _isFilteredOut, value);
    }

    public ReactiveCommand<RxVoid, RxVoid> ToggleExpandCommand { get; }

    /// <summary>该分组下被就地重命名过的叶子数量。</summary>
    public int RenamedCount => Items.Count(i => i.HasCustomTitle);

    public override string ToString() => Title;
}

/// <summary>A presentation group inside a Ribbon tab; commands still map one-to-one to APIs.</summary>
public sealed class DemoRibbonSection : ReactiveObject
{
    public DemoRibbonSection(string titleKey, IReadOnlyList<DemoItemViewModel> items, bool useSmallCommands)
    {
        TitleKey = titleKey;
        Items = items;
        UseSmallCommands = useSmallCommands;
        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    public string TitleKey { get; }
    public IReadOnlyList<DemoItemViewModel> Items { get; }
    public bool UseSmallCommands { get; }
    public string Title => I18nManager.Instance.GetResource(TitleKey);
    public double CommandHeight => UseSmallCommands ? 31 : 62;
}
