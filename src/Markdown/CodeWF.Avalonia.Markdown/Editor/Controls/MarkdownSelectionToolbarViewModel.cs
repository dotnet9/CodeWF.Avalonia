using System.ComponentModel;
using System.Windows.Input;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Toolkit.Lang;

namespace CodeWF.Avalonia.Markdown.Editor.Controls;

/// <summary>
/// 选区浮动工具条视图模型。tooltip/无障碍文案是求值一次的绑定，语言切换需要
/// INPC 通知才会重取；订阅 I18nManager.CultureChanged 全量刷新（编辑器与宿主
/// 应用同生命周期单实例复用，强订阅不泄漏）。
/// </summary>
internal sealed class MarkdownSelectionToolbarViewModel : INotifyPropertyChanged
{
    internal MarkdownSelectionToolbarViewModel(Action<MarkdownEditorAction> format, Action<string, string> apply,
        Func<string, string?> query, Action clear, Action close)
    {
        FormatCommand = new MarkdownFormattingCommand(value => { format((MarkdownEditorAction)value!); close(); });
        ClearCommand = new MarkdownFormattingCommand(_ => { clear(); close(); });
        FontSize = new("font-size", Apply, Clear, query);
        Foreground = new("color", Apply, Clear, query);
        Background = new("background-color", Apply, Clear, query);
        I18nManager.Instance.CultureChanged += (_, _) => Changed();
        void Apply(string property, string value) { apply(property, value); close(); }
        void Clear() { clear(); close(); }
    }
    public ICommand FormatCommand { get; }
    public ICommand ClearCommand { get; }
    public MarkdownSpanStylePickerViewModel FontSize { get; }
    public MarkdownSpanStylePickerViewModel Foreground { get; }
    public MarkdownSpanStylePickerViewModel Background { get; }
    public string Bold => MarkdownLocalization.GetResource(MarkdownL.SelectionBold);
    public string Italic => MarkdownLocalization.GetResource(MarkdownL.SelectionItalic);
    public string Strike => MarkdownLocalization.GetResource(MarkdownL.SelectionStrike);
    public string Code => MarkdownLocalization.GetResource(MarkdownL.SelectionCode);
    public string Link => MarkdownLocalization.GetResource(MarkdownL.SelectionLink);
    public string Heading1 => MarkdownLocalization.GetResource(MarkdownL.SelectionHeading1);
    public string Heading2 => MarkdownLocalization.GetResource(MarkdownL.SelectionHeading2);
    public string List => MarkdownLocalization.GetResource(MarkdownL.SelectionList);
    public string ClearLabel => MarkdownLocalization.GetResource(MarkdownL.SelectionClear);

    private void Changed() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    public event PropertyChangedEventHandler? PropertyChanged;
}
