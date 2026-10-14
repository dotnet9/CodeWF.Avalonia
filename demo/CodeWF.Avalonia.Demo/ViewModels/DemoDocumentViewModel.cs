using Avalonia;
using Avalonia.Controls;
using CodeWF.Avalonia.Demo.Views;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 打开的演示文档：演示始终可见，源码按需展开（XAML / C#），
/// 右侧 PropertyGrid 绑定 <see cref="Page"/> 的属性定义。
/// </summary>
public class DemoDocumentViewModel : ReactiveObject, IDisposable
{
    private DemoSegment _segment = DemoSegment.CSharp;

    public DemoDocumentViewModel(DemoItemViewModel item, DemoPageBase page, MainViewModel shell)
    {
        _shell = shell;
        Item = item;
        Page = page;
        Page.PropertiesChanged += OnPagePropertiesChanged;
        _segment = string.IsNullOrWhiteSpace(page.CSharpCode) ? DemoSegment.Xaml : DemoSegment.CSharp;
        ValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(change => Page.OnPropertyCommitted(change));
        BrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        I18nManager.Instance.CultureChanged += OnCultureChanged;
    }

    private bool _showSource;
    public bool ShowSource { get => _showSource; set => this.RaiseAndSetIfChanged(ref _showSource, value); }
    public string Api => Item.Api;
    public string Package => Item.Package;
    public bool HasXaml => !string.IsNullOrWhiteSpace(Page.XamlCode);
    public bool HasCSharp => !string.IsNullOrWhiteSpace(Page.CSharpCode);
    public bool HasSource => HasXaml || HasCSharp;
    public bool HasApi => !string.IsNullOrEmpty(Api);

    public DemoItemViewModel Item { get; }

    public DemoPageBase Page { get; }

    public bool HasDocumentHeader => Page is not Views.Pages.WelcomePage;

    /// <summary>承载本文档的 Dock Document 包装（用于 Dock 工作区页签），由 Shell 回填。</summary>
    public global::Dock.Model.Core.IDockable? DockDocument { get; set; }

    public string TitleKey => Item.TitleKey;

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    /// <summary>文档页签显示名：跟随导航树的自定义名。</summary>
    public string DisplayTitle => Item.DisplayTitle;

    public string Description => I18nManager.Instance.GetResource(HasApi ? "Demo.Api." + TitleKey.Split('.').Last() + ".Desc" : Page.DescriptionKey);

    /// <summary>就地重命名后刷新页签标题。</summary>
    public void RefreshTitle()
    {
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(DisplayTitle));
        if (DockDocument is not null) DockDocument.Title = DisplayTitle;
    }

    /// <summary>ShowMeCode 当前段。</summary>
    public DemoSegment Segment
    {
        get => _segment;
        set
        {
            if (Equals(_segment, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _segment, value);
            this.RaisePropertyChanged(nameof(IsDemoSegment));
            this.RaisePropertyChanged(nameof(IsXamlSegment));
            this.RaisePropertyChanged(nameof(IsCSharpSegment));
        }
    }

    public bool IsDemoSegment
    {
        get => Segment == DemoSegment.Demo;
        set => Segment = DemoSegment.Demo;
    }

    public bool IsXamlSegment
    {
        get => Segment == DemoSegment.Xaml;
        set { if (value) Segment = DemoSegment.Xaml; }
    }

    public bool IsCSharpSegment
    {
        get => Segment == DemoSegment.CSharp;
        set { if (value) Segment = DemoSegment.CSharp; }
    }

    /// <summary>XAML 源码（以 Markdown 代码块承载，复用 MarkdownViewer 的高亮渲染）。</summary>
    public string XamlMarkdown => string.IsNullOrEmpty(Page.XamlCode)
        ? string.Empty
        : $"```xml\n{Page.XamlCode.TrimEnd()}\n```";

    /// <summary>C# 源码；无内容时回退 XAML 片段，避免空文档。</summary>
    public string CSharpMarkdown
    {
        get
        {
            var code = string.IsNullOrEmpty(Page.CSharpCode) ? Page.XamlCode : Page.CSharpCode;
            return string.IsNullOrEmpty(code) ? string.Empty : $"```csharp\n{code.TrimEnd()}\n```";
        }
    }

    private readonly MainViewModel _shell;
    private bool _propertiesVisible = true;
    private GridLength _propertyWidth = new(260);

    /// <summary>参数与演示内容共用一个文档，浮动窗口无需查找主窗口的活动文档。</summary>
    public IReadOnlyList<PropertyItem> PropertyItems => Page is Views.Pages.WelcomePage ? _shell.GlobalProperties : Page.Properties;
    public ICommand PropertyValueCommittedCommand => Page is Views.Pages.WelcomePage ? _shell.GlobalValueCommittedCommand : ValueCommittedCommand;
    public ICommand PropertyBrowseRequestedCommand => Page is Views.Pages.WelcomePage ? _shell.GlobalBrowseRequestedCommand : BrowseRequestedCommand;
    public ColumnDefinitions ContentColumns { get; } = new()
    {
        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 160 },
        new ColumnDefinition { Width = new GridLength(5) },
        new ColumnDefinition { Width = new GridLength(260), MinWidth = 180 }
    };

    public bool PropertiesVisible
    {
        get => _propertiesVisible;
        set
        {
            if (_propertiesVisible == value) return;
            if (!value) _propertyWidth = ContentColumns[2].Width;
            this.RaiseAndSetIfChanged(ref _propertiesVisible, value);
            ContentColumns[2].MinWidth = value ? 180 : 0;
            ContentColumns[2].Width = value ? _propertyWidth : new GridLength(0);
            ContentColumns[1].Width = new GridLength(value ? 5 : 0);
        }
    }

    public void RefreshProperties()
    {
        Page.RefreshProperties();
        this.RaisePropertyChanged(nameof(PropertyItems));
    }

    private void OnPagePropertiesChanged(object? sender, EventArgs args) => this.RaisePropertyChanged(nameof(PropertyItems));

    public void Dispose()
    {
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        Page.PropertiesChanged -= OnPagePropertiesChanged;
    }

    public ICommand ValueCommittedCommand { get; }

    public ICommand BrowseRequestedCommand { get; }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        RefreshTitle();
        RefreshProperties();
        this.RaisePropertyChanged(nameof(XamlMarkdown));
        this.RaisePropertyChanged(nameof(CSharpMarkdown));
        this.RaisePropertyChanged(nameof(DisplayTitle));
        this.RaisePropertyChanged(nameof(Description));
    }
}
