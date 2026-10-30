using AvaloniaEdit;

using CodeWF.Avalonia.Markdown.Shared.Rendering;

namespace CodeWF.Avalonia.Markdown.Editor.Services;

public interface IMarkdownEditorMutationService
{
    void WrapSelection(TextEditor editor, string prefix, string suffix, string placeholder);

    /// <summary>给选区包上行内 <c>&lt;span style&gt;</c>（property 见 <see cref="MarkdownSpanOperations.Properties"/>）；同款再执行一次解包。</summary>
    void WrapSpan(TextEditor editor, string property, string value);

    void InsertText(TextEditor editor, string insertion);

    void InsertLink(TextEditor editor, string textPlaceholder, string urlPlaceholder);

    void InsertImage(TextEditor editor, string altPlaceholder, string targetPlaceholder);

    void InsertTable(TextEditor editor, string fallbackInsertion);

    void InsertSmartNewLine(TextEditor editor);

    void IndentSelection(TextEditor editor);

    void OutdentSelection(TextEditor editor);

    void ClearFormatting(TextEditor editor);

    void PrefixCurrentLine(TextEditor editor, string prefix);
}
