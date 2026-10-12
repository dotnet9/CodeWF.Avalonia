using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.Demo.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using ReactiveUI.Primitives;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>演示目录项（导航树叶子节点 / Ribbon 命令按钮）。</summary>
public class DemoItemViewModel : ReactiveObject
{
    public DemoItemViewModel(string titleKey, Func<DemoPageBase> pageFactory)
    {
        TitleKey = titleKey;
        PageFactory = pageFactory;

        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    /// <summary>标题的 Lang 键，同时作为文档的唯一标识。</summary>
    public string TitleKey { get; }

    public Func<DemoPageBase> PageFactory { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>目录图标（与设计原型一致）。</summary>
    public string Icon => DemoIcons.ResolveItem(TitleKey);

    public override string ToString() => Title;
}

/// <summary>演示分组（左树组节点 = Ribbon 页签名）。</summary>
public class DemoGroupViewModel : ReactiveObject
{
    private bool _isExpanded = true;

    public DemoGroupViewModel(string titleKey, IEnumerable<DemoItemViewModel> items)
    {
        TitleKey = titleKey;
        Items = new ObservableCollection<DemoItemViewModel>(items);
        ToggleExpandCommand = ReactiveCommand.Create(() => { IsExpanded = !IsExpanded; });

        I18nManager.Instance.CultureChanged += (_, _) => this.RaisePropertyChanged(nameof(Title));
    }

    public string TitleKey { get; }

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>分组图标（导航树组节点）。</summary>
    public string Icon => DemoIcons.ResolveGroup(TitleKey);

    public ObservableCollection<DemoItemViewModel> Items { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public ReactiveCommand<RxVoid, RxVoid> ToggleExpandCommand { get; }

    public override string ToString() => Title;
}
