using System.Windows.Input;
using CodeWF.Avalonia.Markdown.Editor.Services;

namespace CodeWF.Avalonia.Markdown.Editor.Controls;

internal sealed class MarkdownSelectionToolbarViewModel
{
    internal MarkdownSelectionToolbarViewModel(Action<MarkdownEditorAction> format, Action<string, string> apply,
        Func<string, string?> query, Action clear, Action close)
    {
        FormatCommand = new MarkdownFormattingCommand(value => { format((MarkdownEditorAction)value!); close(); });
        ClearCommand = new MarkdownFormattingCommand(_ => { clear(); close(); });
        FontSize = new("font-size", Apply, Clear, query);
        Foreground = new("color", Apply, Clear, query);
        Background = new("background-color", Apply, Clear, query);
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
}
