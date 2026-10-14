using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CodeWF.Avalonia.Markdown.Editor.Controls;
using CodeWF.Avalonia.Markdown.Editor.Services;
using CodeWF.Avalonia.Markdown.Themes;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;

namespace CodeWF.Avalonia.Demo.Views.Pages;

public partial class MarkdownLiveEditorPage : DemoPageBase
{
    private const string IdTheme = "mle.theme";
    private const string IdFontSize = "mle.fontsize";

    public static string XamlSnippet => L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Source.Xaml");

    public static string CSharpSnippet => L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Source.CSharp");

    private readonly MarkdownLiveEditorView _editor;

    public MarkdownLiveEditorPage()
    {
        InitializeComponent();

        _editor = new MarkdownLiveEditorView(new MarkdownEditorOptions
        {
            EnableAutoPair = true,
            FontSize = 15,
        })
        {
            EditorBackgroundKey = "StudioBackground", EditorForegroundKey = "StudioText"
        };
        _editor.TypographyTheme = MarkdownTypographyThemes.Verdant;
        _editor.SetText(SampleMarkdown);
        _editor.MarkdownChanged += (_, _) => UpdateStatus();

        EditorHost.Child = _editor;
        UpdateStatus();
    }



    public override string TitleKey => "Demo.Page.MarkdownLiveEditor.Title";

    public override string DescriptionKey => "Demo.Page.MarkdownLiveEditor.Desc";

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    public override void OnDocumentOpened() => UpdateStatus();

    public override void OnPropertyCommitted(PropertyValueChange change)
    {
        switch (change.Item.Id)
        {
            case IdTheme when change.NewValue is string theme:
                _editor.TypographyTheme = theme;
                break;
            case IdFontSize when change.NewValue is decimal size:
                _editor.EditorFontSize = (double)size;
                break;
        }

        UpdateStatus();
    }

    protected override IReadOnlyList<PropertyItem> BuildProperties()
    {
        var theme = new PropertyItem
        {
            Id = IdTheme,
            Name = L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Properties.TypographyTheme"),
            Kind = PropertyItemKind.Choice,
            Options =
            [
                MarkdownTypographyThemes.Basic,
                MarkdownTypographyThemes.Verdant,
                MarkdownTypographyThemes.TenderGreen,
                MarkdownTypographyThemes.GeekBlack,
                MarkdownTypographyThemes.WeChatFormat,
            ],
            Value = MarkdownTypographyThemes.Verdant,
            DefaultValue = MarkdownTypographyThemes.Basic,
            Description = L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Properties.TypographyThemeUsedByTheLiveEditor"),
        };
        var fontSize = new PropertyItem
        {
            Id = IdFontSize,
            Name = L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Properties.EditorFontSize"),
            Kind = PropertyItemKind.Number,
            Value = 15m,
            DefaultValue = 15m,
            Minimum = 10,
            Maximum = 28,
            Unit = "px",
            Description = L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Properties.FontSizeOfTheBlockEditor"),
        };

        return [theme, fontSize];
    }

    private void OnInsert(object? sender, RoutedEventArgs e) =>
        _editor.InsertText(L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.OnInsert.ContentAddedWithInsertText"));

    private void OnFocusFirst(object? sender, RoutedEventArgs e) =>
        _editor.FocusBlock(0);

    private void OnBold(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Bold);

    private void OnTable(object? sender, RoutedEventArgs e) =>
        _ = _editor.ExecuteAsync(MarkdownEditorAction.Table);

    private void UpdateStatus()
    {
        StatusText.Text =
            string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.UpdateStatus.BlocksActiveBlockCharactersFormat"), _editor.BlockCount, _editor.ActiveBlockIndex, (_editor.Text?.Length ?? 0));
    }

    private static string SampleMarkdown => L.Instance.GetResource("Demo.Content.MarkdownLiveEditor.Samples.Markdown");
}
