using DemoResources = global::CodeWF.Avalonia.Demo.I18n.ResourceKeys;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownEditorPage : DemoPageBase
{
    private const string IdFontSize = "mde.fontsize";
    private const string IdLineNumbers = "mde.linenums";
    private const string IdAutoPair = "mde.autopair";

    public static string XamlSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Source.Xaml);

    public static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Source.CSharp);

    private readonly MarkdownEditorView _editor;

    public MarkdownEditorPage()
    {
        InitializeComponent();

        _editor = new MarkdownEditorView(new MarkdownEditorOptions
        {
            LinkUrlPlaceholder = "https://example.com",
            CodeFenceLanguage = "csharp",
            EnableAutoPair = true,
            ShowLineNumbers = true,
            FontSize = 15,
        })
        {
            EditorBackgroundKey = "StudioBackground", EditorForegroundKey = "StudioText",
            CurrentLineBackgroundKey = "StudioSurface", CurrentLineBorderKey = "StudioBorder",
            AccentKey = "StudioAccent", CodeKey = "StudioAccent", QuoteKey = "StudioMuted",
            LinkKey = "StudioSecondary", SeparatorKey = "StudioBorder"
        };
        _editor.SetText(SampleMarkdown);
        _editor.MarkdownChanged += (_, _) => UpdateStatus();

        EditorHost.Child = _editor;
        UpdateStatus();
    }



    public override string TitleKey => DemoResources.Page.MarkdownEditor.Title;

    public override string DescriptionKey => DemoResources.Page.MarkdownEditor.Desc;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateStatus();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdFontSize when change.NewValue is decimal size:
                _editor.EditorFontSize = (double)size;
                break;
            case IdLineNumbers when change.NewValue is bool lineNumbers:
                _editor.ShowLineNumbers = lineNumbers;
                break;
            case IdAutoPair when change.NewValue is bool autoPair:
                _editor.EnableAutoPair = autoPair;
                break;
        }

        UpdateStatus();
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var fontSize = new PropertyItem
        {
            Id = IdFontSize,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.EditorFontSize),
            Kind = PropertyItemKind.Number,
            Value = 15m,
            DefaultValue = 15m,
            Minimum = 10,
            Maximum = 28,
            Unit = "px",
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.EditorTextFontSize),
        };
        var lineNumbers = new PropertyItem
        {
            Id = IdLineNumbers,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.ShowLineNumbers),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.ShowLineNumbersToTheLeftOfThe),
        };
        var autoPair = new PropertyItem
        {
            Id = IdAutoPair,
            Name = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.AutoPairing),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Properties.AutomaticallyCompletePairedMarkersSuchAsAndBackticks),
        };

        return [fontSize, lineNumbers, autoPair];
    }

    private void OnBold(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Bold);

    private void OnItalic(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Italic);

    private void OnInlineCode(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.InlineCode);

    private void OnHeading(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Heading2);

    private void OnTable(object? sender, RoutedEventArgs e) =>
        _editor.InsertTable(L.Instance.GetResource(DemoResources.Content.MarkdownEditor.OnTable.Column1Column2AB));

    private void OnTaskList(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.TaskList);

    private void OnCodeFence(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.CodeFence);

    private void OnQuote(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Quote);

    private void UpdateStatus()
    {
        var caret = _editor.CaretPosition;
        StatusText.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.MarkdownEditor.UpdateStatus.LineColumnLinesCharactersFormat), caret.Line, caret.Column, caret.LineCount, (_editor.Text?.Length ?? 0));
    }

    private static string SampleMarkdown => L.Instance.GetResource(DemoResources.Content.MarkdownEditor.Samples.Markdown);
}
