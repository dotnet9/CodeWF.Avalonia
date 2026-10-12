using CodeWF.Avalonia.Demo.Views;
using CodeWF.Avalonia.Lang;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

namespace CodeWF.Avalonia.Demo.ViewModels;

/// <summary>
/// 打开的演示文档：头部带 ShowMeCode 三段切换（演示 / XAML / C#），
/// 右侧 PropertyGrid 绑定 <see cref="Page"/> 的属性定义。
/// </summary>
public class DemoDocumentViewModel : ReactiveObject
{
    private DemoSegment _segment = DemoSegment.Demo;

    public DemoDocumentViewModel(DemoItemViewModel item, DemoPageBase page)
    {
        Item = item;
        Page = page;
        ValueCommittedCommand = ReactiveCommand.Create<PropertyValueChange>(change => Page.OnPropertyCommitted(change));
        BrowseRequestedCommand = ReactiveCommand.Create<PropertyItem>(_ => { });

        I18nManager.Instance.CultureChanged += OnCultureChanged;
    }

    public DemoItemViewModel Item { get; }

    public DemoPageBase Page { get; }

    /// <summary>承载本文档的 Dock Document 包装（用于 Dock 工作区页签），由 Shell 回填。</summary>
    public global::Dock.Model.Core.IDockable? DockDocument { get; set; }

    public string TitleKey => Item.TitleKey;

    public string Title => I18nManager.Instance.GetResource(TitleKey);

    public string Description => I18nManager.Instance.GetResource(Page.DescriptionKey);

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
        set => Segment = DemoSegment.Xaml;
    }

    public bool IsCSharpSegment
    {
        get => Segment == DemoSegment.CSharp;
        set => Segment = DemoSegment.CSharp;
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

    public ICommand ValueCommittedCommand { get; }

    public ICommand BrowseRequestedCommand { get; }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(Title));
        this.RaisePropertyChanged(nameof(Description));
    }
}
